using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 씬 컨트롤러. CombatFlow 를 보유하고, 런타임 참가자/몬스터 스포너/입력 공급자/타겟 셀렉터를 조립한다.
/// </summary>
public sealed class CombatController
    : SceneControllerBase,
        IPlayerInputProvider,
        IDeadEnemyRemover
{
    private const string PlayerVisualPrefabPath = "Player/Player";
    private const string CombatBackgroundRootName = "CombatBackgroundRoot";
    private const string CombatBackgroundResourceRoot = "CombatBackgrounds/BG_Combat_Stage";

    [SerializeField]
    private CombatMonsterSpawner spawnerRef;

    [SerializeField]
    private CombatHud hudRef;

    [SerializeField]
    private bool autoPlayerActions;

    [SerializeField]
    private Transform runtimeRootRef;

    [SerializeField]
    private Transform backgroundRootRef;

    /// <summary>전투 FSM.</summary>
    public CombatFlow Flow;

    /// <summary>몬스터 스포너.</summary>
    public CombatMonsterSpawner Spawner;

    /// <summary>타겟 셀렉터(레이캐스트).</summary>
    public TargetSelector Targeter;

    /// <summary>HUD 페이지(전투 전용 UI).</summary>
    public CombatHud Hud;

    /// <summary>플레이어 본체.</summary>
    public Player Player;

    /// <summary>참여 동료(최대 3).</summary>
    public List<TeamBase> Companions;

    /// <summary>참여 몬스터.</summary>
    public List<Monster> Monsters;

    private static readonly Dictionary<string, Sprite> generatedSprites =
        new Dictionary<string, Sprite>();

    /// <summary>플레이어 행동 선택 상태.</summary>
    private PlayerActionSelector playerActionSelector;

    /// <summary>이미 EndCombat을 호출했는지 방어 플래그.</summary>
    private bool combatEnded;

    private Transform runtimeRoot;
    private Transform backgroundRoot;
    private EntityBase hoveredTarget;

    private sealed class DeadEnemyRemovalMarker : MonoBehaviour { }

    /// <inheritdoc/>
    public override void OnEnter()
    {
        GameSystemManager gsm = GameSystemManager.Instance;
        CombatContext ctx = gsm.CombatContext;
        if (ctx == null || gsm.CurrentRun == null)
        {
            return;
        }

        if (!ResolveCoreReferences())
        {
            return;
        }

        PrepareRuntimeRoot();
        PrepareCombatBackground(ctx.Node);
        DisableSceneAuthoredCombatUnits();

        int allyCount = 1 + CountActiveCompanions(gsm.CurrentRun.Roster);
        Player = CreateRuntimePlayer(gsm.CurrentRun.Player, allyCount);
        Companions = CreateRuntimeCompanions(gsm.CurrentRun.Roster, allyCount);

        Spawner.SpawnFromNode(ctx.Node, runtimeRoot, BuildEnemyPositions(ctx.Node));
        Monsters = Spawner.SpawnedT ?? new List<Monster>();
        for (int i = 0; i < Monsters.Count; i++)
        {
            ConfigureCombatUnit(
                Monsters[i],
                "monster",
                new Color(0.68f, 0.86f, 0.32f, 1f),
                false,
                8
            );
        }

        Flow = new CombatFlow
        {
            Input = this,
            TargetSelector = Targeter,
            MonsterAi = new MonsterActionSelector(),
            CompanionAi = new CompanionActionSelector(),
            DeadEnemyRemover = this,
        };

        var allies = new List<EntityBase> { Player };
        foreach (TeamBase companion in Companions)
        {
            allies.Add(companion);
        }

        var enemies = new List<EntityBase>();
        foreach (Monster monster in Monsters)
        {
            enemies.Add(monster);
        }

        combatEnded = false;
        playerActionSelector = new PlayerActionSelector();
        hoveredTarget = null;

        Hud.Bind(this, Player);
        Hud.OnOpen();
        Targeter.OnTargetConfirmed += OnTargetConfirmed;
        Targeter.OnHoverChanged += OnTargetHoverChanged;
        Flow.StartCombat(allies, enemies);
    }

    private bool ResolveCoreReferences()
    {
        if (Spawner == null)
            Spawner = spawnerRef;
        if (Spawner == null)
            Spawner = GetComponent<CombatMonsterSpawner>();
        if (Spawner == null)
        {
            GameLog.LogError(
                "[CombatController] CombatMonsterSpawner 가 씬에 직접 배치/할당되어 있지 않습니다."
            );
            return false;
        }

        if (Hud == null)
            Hud = hudRef;
        if (Hud == null)
        {
            GameLog.LogError(
                "[CombatController] CombatHud 가 씬에 직접 배치/할당되어 있지 않습니다."
            );
            return false;
        }
        if (!Hud.HasRequiredReferences())
        {
            return false;
        }

        if (Targeter == null)
            Targeter = new TargetSelector();
        return true;
    }

    /// <inheritdoc/>
    public override void OnExit()
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && Player != null)
        {
            Player.CopyToState(gsm.CurrentRun?.Player);
        }

        Hud?.OnClose();
        Spawner?.Cleanup();
        if (Targeter != null)
        {
            Targeter.OnTargetConfirmed -= OnTargetConfirmed;
            Targeter.OnHoverChanged -= OnTargetHoverChanged;
            Targeter.EndHover();
        }

        ClearRuntimeRoot();
        Flow = null;
        Player = null;
        Companions = null;
        Monsters = null;
        playerActionSelector = null;
        hoveredTarget = null;
    }

    /// <inheritdoc/>
    public override void OnSceneUpdate()
    {
        Targeter?.Tick();
        Flow?.Tick();
        if (Flow != null)
        {
            if (Flow.State == CombatState.Ended && !combatEnded)
            {
                combatEnded = true;
                CombatResult result = Flow.CheckOutcome() ?? CombatResult.Defeat;
                GameSystemManager.Instance.EndCombat(result, this);
            }
        }
    }

    /// <inheritdoc/>
    public bool HasAction =>
        playerActionSelector != null && playerActionSelector.HasConfirmedAction;

    /// <inheritdoc/>
    public void RequestPlayerAction(EntityBase actor)
    {
        playerActionSelector?.Clear();
        Hud?.ClearTargetPrompt();
        if (!IsAutoMode)
        {
            Hud?.ShowPlayerActionPanel(actor);
        }

        if (IsAutoMode)
        {
            PlayerPickAttack();
        }
    }

    /// <inheritdoc/>
    public CombatAction PopAction()
    {
        CombatAction action = playerActionSelector?.PopAction();
        Hud?.HidePlayerActionPanel();
        Hud?.ClearTargetPrompt();
        return action;
    }

    private bool IsAutoMode => autoPlayerActions;

    /// <summary>공격 버튼.</summary>
    public void PlayerPickAttack()
    {
        playerActionSelector?.PickAttack(Player, Flow, Targeter, Hud, IsAutoMode);
    }

    /// <summary>스킬 버튼.</summary>
    public void PlayerPickSkill(int slotIndex)
    {
        playerActionSelector?.PickSkill(Player, Flow, Targeter, Hud, slotIndex, IsAutoMode);
    }

    /// <summary>방어 버튼.</summary>
    public void PlayerPickDefend()
    {
        playerActionSelector?.PickDefend(Player, Targeter, Hud);
    }

    /// <summary>소모 아이템 슬롯 클릭.</summary>
    public void PlayerUseItem(int slotIndex)
    {
        if (
            Player?.Consumables != null
            && Player.Consumables.TryUse(slotIndex, Player, Player.Inventory)
        )
        {
            Hud?.RefreshConsumableSlots();
        }
    }

    /// <summary>타겟 호버 후 클릭 시 호출.</summary>
    public void OnTargetConfirmed(EntityBase target)
    {
        playerActionSelector?.ConfirmTarget(target, Targeter, Hud);
    }

    public void ScheduleDeadEnemyRemoval(EntityBase enemy, float delaySec)
    {
        if (enemy == null || enemy.GetComponent<DeadEnemyRemovalMarker>() != null)
        {
            return;
        }

        enemy.WorldUI?.SetTargetHighlight(false);
        enemy.WorldUI?.HideActionIcon();
        Collider collider = enemy.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        enemy.gameObject.AddComponent<DeadEnemyRemovalMarker>();
        StartCoroutine(DestroyDeadEnemyAfterDelay(enemy, delaySec));
    }

    private System.Collections.IEnumerator DestroyDeadEnemyAfterDelay(
        EntityBase enemy,
        float delaySec
    )
    {
        yield return new WaitForSeconds(Mathf.Max(0f, delaySec));
        if (enemy != null)
        {
            Destroy(enemy.gameObject);
        }
    }

    private void OnTargetHoverChanged(EntityBase target)
    {
        if (hoveredTarget != null)
        {
            hoveredTarget.WorldUI?.SetTargetHighlight(false);
        }

        hoveredTarget = target;
        if (hoveredTarget != null)
        {
            hoveredTarget.WorldUI?.SetTargetHighlight(true);
        }
    }

    private void PrepareRuntimeRoot()
    {
        runtimeRoot = runtimeRootRef;
        if (runtimeRoot == null)
        {
            GameObject root = new GameObject("RuntimeCombatUnits");
            runtimeRoot = root.transform;
            runtimeRoot.SetParent(transform, false);
        }

        ClearRuntimeRootChildren();
    }

    private void ClearRuntimeRoot()
    {
        ClearRuntimeRootChildren();
        if (runtimeRootRef == null && runtimeRoot != null)
        {
            Destroy(runtimeRoot.gameObject);
        }

        runtimeRoot = null;
    }

    private void PrepareCombatBackground(FloorNode node)
    {
        backgroundRoot = ResolveBackgroundRoot();
        if (backgroundRoot == null)
        {
            return;
        }

        int stageIndex = node != null ? Mathf.Max(1, node.StageIndex) : 1;
        string resourcePath = CombatBackgroundResourceRoot + stageIndex.ToString("00");
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            GameLog.LogError(
                "[CombatController] Combat background sprite missing: Resources/"
                    + resourcePath
            );
            return;
        }

        SpriteRenderer renderer = backgroundRoot.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            GameLog.LogError(
                "[CombatController] CombatBackgroundRoot SpriteRenderer is missing."
            );
            return;
        }

        renderer.sprite = sprite;
    }

    private Transform ResolveBackgroundRoot()
    {
        if (backgroundRootRef != null)
        {
            return backgroundRootRef;
        }

        Transform found = transform.Find(CombatBackgroundRootName);
        if (found != null)
        {
            return found;
        }

        GameLog.LogError(
            "[CombatController] CombatBackgroundRoot is missing under CombatController."
        );
        return null;
    }

    private void ClearRuntimeRootChildren()
    {
        if (runtimeRoot == null)
        {
            return;
        }

        for (int i = runtimeRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(runtimeRoot.GetChild(i).gameObject);
        }
    }

    private void DisableSceneAuthoredCombatUnits()
    {
        EntityBase[] sceneEntities = FindObjectsByType<EntityBase>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        for (int i = 0; i < sceneEntities.Length; i++)
        {
            EntityBase entity = sceneEntities[i];
            if (entity == null || entity.transform.IsChildOf(runtimeRoot))
            {
                continue;
            }

            entity.gameObject.SetActive(false);
        }
    }

    private Player CreateRuntimePlayer(PlayerState state, int allyCount)
    {
        GameObject go = new GameObject("Player.Runtime");
        go.transform.SetParent(runtimeRoot, false);
        go.transform.position = AllyPosition(0, allyCount);

        Player runtimePlayer = go.AddComponent<Player>();
        runtimePlayer.BindState(state);
        AttachPlayerVisual(go.transform);
        ConfigureCombatUnit(runtimePlayer, "player", new Color(0.35f, 0.66f, 1f, 1f), true, 10);
        return runtimePlayer;
    }

    private static void AttachPlayerVisual(Transform host)
    {
        if (host == null)
        {
            return;
        }

        GameObject visualPrefab = Resources.Load<GameObject>(PlayerVisualPrefabPath);
        if (visualPrefab == null)
        {
            GameLog.LogError("[CombatController] Player prefab missing: Resources/" + PlayerVisualPrefabPath);
            return;
        }

        GameObject visual = Instantiate(visualPrefab, host);
        visual.name = "Player_Visual";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = visualPrefab.transform.localScale;
        DisableVisualArtifacts(visual);
    }

    private List<TeamBase> CreateRuntimeCompanions(CompanionRosterState roster, int allyCount)
    {
        var result = new List<TeamBase>();
        if (roster?.Active == null)
        {
            return result;
        }

        DataManager data = GameSystemManager.TryGetInstance(out GameSystemManager gsm)
            ? gsm.Data
            : null;
        int count = Mathf.Min(3, roster.Active.Count);
        for (int i = 0; i < count; i++)
        {
            CompanionInstance state = roster.Active[i];
            if (state == null)
            {
                continue;
            }

            GameObject go = CompanionPrefabResolver.CreateRuntimeObject(
                state,
                data,
                runtimeRoot,
                AllyPosition(result.Count + 1, allyCount)
            );

            CombatCompanion companion = go.GetComponent<CombatCompanion>();
            if (companion == null)
            {
                companion = go.AddComponent<CombatCompanion>();
            }

            companion.BindState(state);
            ConfigureCombatUnit(
                companion,
                "companion",
                new Color(0.62f, 0.86f, 1f, 1f),
                true,
                9
            );
            result.Add(companion);
        }

        return result;
    }

    private static int CountActiveCompanions(CompanionRosterState roster)
    {
        if (roster?.Active == null)
        {
            return 0;
        }

        int count = 0;
        int limit = Mathf.Min(3, roster.Active.Count);
        for (int i = 0; i < limit; i++)
        {
            if (roster.Active[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    private static void DisableVisualArtifacts(GameObject visualRoot)
    {
        if (visualRoot == null)
        {
            return;
        }

        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.gameObject.name.Contains("Circle"))
            {
                renderer.enabled = false;
            }
        }
    }

    private void ConfigureCombatUnit(
        EntityBase entity,
        string spriteKey,
        Color fallbackColor,
        bool includeMana,
        int sortingOrder
    )
    {
        if (entity == null)
        {
            return;
        }

        SpriteRenderer[] renderers = entity.GetComponentsInChildren<SpriteRenderer>(true);
        bool hasAuthoredVisual = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer childRenderer = renderers[i];
            if (childRenderer == null)
            {
                continue;
            }

            if (childRenderer.sprite != null)
            {
                hasAuthoredVisual = true;
                childRenderer.sortingOrder = sortingOrder;
            }
        }

        if (!hasAuthoredVisual)
        {
            SpriteRenderer renderer = entity.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = entity.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = GetGeneratedSprite(spriteKey, fallbackColor);
            renderer.color = Color.white;
            renderer.sortingOrder = sortingOrder;
        }

        BoxCollider collider = entity.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = entity.gameObject.AddComponent<BoxCollider>();
        }

        collider.enabled = true;
        collider.center = new Vector3(0f, 0.45f, 0f);
        collider.size = new Vector3(1.25f, 1.55f, 0.25f);

        EntityWorldUI.EnsureFor(entity, includeMana);
    }

    private static Sprite GetGeneratedSprite(string key, Color fillColor)
    {
        if (generatedSprites.TryGetValue(key, out Sprite sprite) && sprite != null)
        {
            return sprite;
        }

        int size = ResolveGeneratedSpriteSize();
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.DontSave;
        texture.filterMode = FilterMode.Point;

        Color outline = new Color(0.08f, 0.08f, 0.09f, 1f);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Vector2 center = new Vector2(size * 0.5f, size * 0.48f);
        float radiusX = size * 0.32f;
        float radiusY = size * 0.40f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - center.x) / radiusX;
                float ny = (y - center.y) / radiusY;
                float d = nx * nx + ny * ny;
                if (d <= 0.78f)
                {
                    texture.SetPixel(x, y, fillColor);
                }
                else if (d <= 1f)
                {
                    texture.SetPixel(x, y, outline);
                }
                else
                {
                    texture.SetPixel(x, y, clear);
                }
            }
        }

        texture.Apply();
        sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.18f),
            ResolveGeneratedSpritePixelsPerUnit()
        );
        sprite.name = "GeneratedCombatUnit_" + key;
        generatedSprites[key] = sprite;
        return sprite;
    }

    private static int ResolveGeneratedSpriteSize()
    {
        if (
            GameSystemManager.TryGetInstance(out GameSystemManager gsm)
            && gsm.Data?.Balance != null
            && gsm.Data.Balance.CombatGeneratedSpriteSize > 0
        )
        {
            return gsm.Data.Balance.CombatGeneratedSpriteSize;
        }

        return 64;
    }

    private static float ResolveGeneratedSpritePixelsPerUnit()
    {
        if (
            GameSystemManager.TryGetInstance(out GameSystemManager gsm)
            && gsm.Data?.Balance != null
            && gsm.Data.Balance.CombatGeneratedSpritePixelsPerUnit > 0f
        )
        {
            return gsm.Data.Balance.CombatGeneratedSpritePixelsPerUnit;
        }

        return 48f;
    }

    private static Vector3 AllyPosition(int index, int allyCount)
    {
        int count = Mathf.Clamp(allyCount, 1, 4);
        int safeIndex = Mathf.Clamp(index, 0, count - 1);
        float spacing = 1.1f;
        float startX = -3.35f - (count - 1) * spacing * 0.5f;
        return new Vector3(startX + safeIndex * spacing, 0f, 0f);
    }

    private static IReadOnlyList<Vector3> BuildEnemyPositions(FloorNode node)
    {
        int count = node != null ? Mathf.Max(1, node.MonsterCount) : 1;
        if (node != null && node.IsBoss)
        {
            count = 1;
        }

        count = Mathf.Min(3, count);
        var positions = new List<Vector3>(count);
        float spacing = 1.1f;
        float startX = 3.35f - (count - 1) * spacing * 0.5f;
        for (int i = 0; i < count; i++)
        {
            positions.Add(new Vector3(startX + i * spacing, 0.7f, 0f));
        }

        return positions;
    }

    /// <summary>
    /// 노드 전투에서 발생한 모든 보상을 수집 및 집계한다.
    /// Flow.State == Ended(Victory) 시 OnSceneUpdate 에서 호출.
    /// </summary>
    public NodeRewardSummary CollectNodeRewards()
    {
        var contributions = new List<NodeRewardContribution>();
        if (Spawner?.SpawnedT != null)
        {
            foreach (Monster monster in Spawner.SpawnedT)
            {
                if (monster != null)
                {
                    contributions.Add(monster.GetRewardContribution());
                }
            }
        }

        return NodeRewardSummary.Aggregate(contributions);
    }
}

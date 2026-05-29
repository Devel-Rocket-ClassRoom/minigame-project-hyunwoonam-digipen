using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 씬 컨트롤러. CombatFlow 를 보유하고, 런타임 참가자/몬스터 스포너/입력 공급자/타겟 셀렉터를 조립한다.
    /// </summary>
    public sealed class CombatController : SceneControllerBase, IPlayerInputProvider, IDeadEnemyRemover
    {
        [SerializeField] private CombatMonsterSpawner spawnerRef;
        [SerializeField] private CombatHud hudRef;
        [SerializeField] private bool autoPlayerActions;
        [SerializeField] private Transform runtimeRootRef;

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
        public List<MonsterBase> Monsters;

        private static readonly Dictionary<string, Sprite> generatedSprites = new Dictionary<string, Sprite>();

        /// <summary>플레이어 임시 행동(입력 진행 중).</summary>
        private CombatAction pendingAction;

        /// <summary>이미 EndCombat을 호출했는지 방어 플래그.</summary>
        private bool combatEnded;

        private Transform runtimeRoot;
        private EntityBase hoveredTarget;

        private sealed class DeadEnemyRemovalMarker : MonoBehaviour
        {
        }

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
            DisableSceneAuthoredCombatUnits();

            Player = CreateRuntimePlayer(gsm.CurrentRun.Player);
            Companions = CreateRuntimeCompanions(gsm.CurrentRun.Roster);

            Spawner.SpawnFromNode(ctx.Node, runtimeRoot, BuildEnemyPositions(ctx.Node));
            Monsters = Spawner.SpawnedT ?? new List<MonsterBase>();
            for (int i = 0; i < Monsters.Count; i++)
            {
                ConfigureCombatUnit(Monsters[i], "monster", new Color(0.68f, 0.86f, 0.32f, 1f), false, 8);
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
            foreach (MonsterBase monster in Monsters)
            {
                enemies.Add(monster);
            }

            combatEnded = false;
            pendingAction = null;
            hoveredTarget = null;

            Hud.Bind(this, Player);
            Hud.OnOpen();
            Targeter.OnTargetConfirmed += OnTargetConfirmed;
            Targeter.OnHoverChanged += OnTargetHoverChanged;
            Flow.StartCombat(allies, enemies);
        }

        private bool ResolveCoreReferences()
        {
            if (Spawner == null) Spawner = spawnerRef;
            if (Spawner == null) Spawner = GetComponent<CombatMonsterSpawner>();
            if (Spawner == null)
            {
                Debug.LogError("[CombatController] CombatMonsterSpawner 가 씬에 직접 배치/할당되어 있지 않습니다.");
                return false;
            }

            if (Hud == null) Hud = hudRef;
            if (Hud == null)
            {
                Debug.LogError("[CombatController] CombatHud 가 씬에 직접 배치/할당되어 있지 않습니다.");
                return false;
            }
            if (!Hud.HasRequiredReferences())
            {
                return false;
            }

            if (Targeter == null) Targeter = new TargetSelector();
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
            pendingAction = null;
            hoveredTarget = null;
        }

        /// <inheritdoc/>
        public override void OnSceneUpdate()
        {
            Targeter?.Tick();
            Flow?.Tick();
            if (Flow != null && Flow.State == CombatState.Ended && !combatEnded)
            {
                combatEnded = true;
                CombatResult result = Flow.CheckOutcome() ?? CombatResult.Defeat;
                GameSystemManager.Instance.EndCombat(result, this);
            }
        }

        /// <inheritdoc/>
        public bool HasAction => pendingAction != null && IsActionConfirmed();

        /// <inheritdoc/>
        public void RequestPlayerAction(EntityBase actor)
        {
            pendingAction = null;
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
            CombatAction action = pendingAction;
            pendingAction = null;
            Hud?.HidePlayerActionPanel();
            Hud?.ClearTargetPrompt();
            return action;
        }

        private bool IsAutoMode => autoPlayerActions;

        /// <summary>공격 버튼.</summary>
        public void PlayerPickAttack()
        {
            pendingAction = new CombatAction
            {
                Actor = Player,
                Type = CombatActionType.Attack,
                Targets = new List<EntityBase>(),
                ConsumesTurn = true,
            };

            if (IsAutoMode)
            {
                EntityBase auto = CombatTargeting.FirstAlive(Flow?.EnemiesT);
                if (auto != null) pendingAction.Targets.Add(auto);
                return;
            }

            Targeter.BeginHover(SkillTargetType.EnemySingle);
            Hud?.ShowTargetPrompt(SkillTargetType.EnemySingle);
        }

        /// <summary>스킬 버튼.</summary>
        public void PlayerPickSkill(int slotIndex)
        {
            Skill skill = Player != null ? Player.GetActiveSkill(slotIndex) : null;
            if (skill == null || !skill.CanUse(Player))
            {
                return;
            }

            pendingAction = new CombatAction
            {
                Actor = Player,
                Type = CombatActionType.Skill,
                Skill = skill,
                Targets = new List<EntityBase>(),
                ConsumesTurn = true,
            };

            SkillTargetType tt = skill.Data.TargetType;
            if (tt == SkillTargetType.Self)
            {
                pendingAction.Targets.Add(Player);
                Hud?.ClearTargetPrompt();
                return;
            }
            if (tt == SkillTargetType.EnemyAll)
            {
                CombatTargeting.FillByTargetType(
                    pendingAction.Targets,
                    SkillTargetType.EnemyAll,
                    Player,
                    Flow?.AlliesT,
                    Flow?.EnemiesT,
                    CombatTargeting.SingleSelect.Random);
                Hud?.ClearTargetPrompt();
                return;
            }
            if (tt == SkillTargetType.AllyAll)
            {
                CombatTargeting.FillByTargetType(
                    pendingAction.Targets,
                    SkillTargetType.AllyAll,
                    Player,
                    Flow?.AlliesT,
                    Flow?.EnemiesT,
                    CombatTargeting.SingleSelect.Random);
                Hud?.ClearTargetPrompt();
                return;
            }

            if (IsAutoMode)
            {
                IList<EntityBase> source = tt == SkillTargetType.AllySingle ? Flow?.AlliesT : Flow?.EnemiesT;
                EntityBase pick = CombatTargeting.FirstAlive(source);
                if (pick != null) pendingAction.Targets.Add(pick);
                return;
            }

            Targeter.BeginHover(tt);
            Hud?.ShowTargetPrompt(tt);
        }

        /// <summary>방어 버튼.</summary>
        public void PlayerPickDefend()
        {
            pendingAction = new CombatAction
            {
                Actor = Player,
                Type = CombatActionType.Defend,
                Targets = new List<EntityBase> { Player },
                ConsumesTurn = true,
            };
            Targeter?.EndHover();
            Hud?.ClearTargetPrompt();
        }

        /// <summary>소모 아이템 슬롯 클릭.</summary>
        public void PlayerUseItem(int slotIndex)
        {
            if (Player?.Consumables != null && Player.Consumables.TryUse(slotIndex, Player, Player.Inventory))
            {
                Hud?.RefreshConsumableSlots();
            }
        }

        /// <summary>타겟 호버 후 클릭 시 호출.</summary>
        public void OnTargetConfirmed(EntityBase target)
        {
            if (pendingAction == null || target == null || target.IsDead || !target.gameObject.activeInHierarchy)
            {
                return;
            }

            if (pendingAction.Targets == null)
            {
                pendingAction.Targets = new List<EntityBase>();
            }

            pendingAction.Targets.Clear();
            pendingAction.Targets.Add(target);
            Targeter?.EndHover();
            Hud?.ClearTargetPrompt();
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

        private System.Collections.IEnumerator DestroyDeadEnemyAfterDelay(EntityBase enemy, float delaySec)
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
            EntityBase[] sceneEntities = FindObjectsByType<EntityBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

        private Player CreateRuntimePlayer(PlayerState state)
        {
            GameObject go = new GameObject("Player.Runtime");
            go.transform.SetParent(runtimeRoot, false);
            go.transform.position = AllyPosition(0);

            Player runtimePlayer = go.AddComponent<Player>();
            runtimePlayer.BindState(state);
            ConfigureCombatUnit(runtimePlayer, "player", new Color(0.35f, 0.66f, 1f, 1f), true, 10);
            return runtimePlayer;
        }

        private List<TeamBase> CreateRuntimeCompanions(CompanionRosterState roster)
        {
            var result = new List<TeamBase>();
            if (roster?.Active == null)
            {
                return result;
            }

            int count = Mathf.Min(3, roster.Active.Count);
            for (int i = 0; i < count; i++)
            {
                CompanionInstance state = roster.Active[i];
                if (state == null)
                {
                    continue;
                }

                GameObject go = new GameObject("Companion.Runtime." + state.CompanionDataId);
                go.transform.SetParent(runtimeRoot, false);
                go.transform.position = AllyPosition(result.Count + 1);

                CombatCompanion companion = go.AddComponent<CombatCompanion>();
                companion.BindState(state);
                ConfigureCombatUnit(companion, "companion", new Color(0.62f, 0.86f, 1f, 1f), true, 9);
                result.Add(companion);
            }

            return result;
        }

        private void ConfigureCombatUnit(EntityBase entity, string spriteKey, Color fallbackColor, bool includeMana, int sortingOrder)
        {
            if (entity == null)
            {
                return;
            }

            SpriteRenderer renderer = entity.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = entity.gameObject.AddComponent<SpriteRenderer>();
            }

            if (renderer.sprite == null)
            {
                renderer.sprite = GetGeneratedSprite(spriteKey, fallbackColor);
                renderer.color = Color.white;
            }

            renderer.sortingOrder = sortingOrder;

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

            const int size = 64;
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
            sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.18f), 48f);
            sprite.name = "GeneratedCombatUnit_" + key;
            generatedSprites[key] = sprite;
            return sprite;
        }

        private static Vector3 AllyPosition(int index)
        {
            float[] yOffsets = { 0f, -0.95f, 0.95f, -1.9f };
            int safeIndex = Mathf.Clamp(index, 0, yOffsets.Length - 1);
            return new Vector3(-3.35f, 1.85f + yOffsets[safeIndex], 0f);
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
                positions.Add(new Vector3(startX + i * spacing, 1.85f, 0f));
            }

            return positions;
        }

        /// <summary>행동이 완전히 확정됐는지(타겟 포함).</summary>
        private bool IsActionConfirmed()
        {
            if (pendingAction == null)
            {
                return false;
            }

            if (pendingAction.Type == CombatActionType.Defend)
            {
                return true;
            }

            if (pendingAction.Type == CombatActionType.Attack)
            {
                return pendingAction.Targets != null && pendingAction.Targets.Count > 0;
            }

            if (pendingAction.Type == CombatActionType.Skill && pendingAction.Skill?.Data != null)
            {
                SkillTargetType targetType = pendingAction.Skill.Data.TargetType;
                if (targetType == SkillTargetType.Self
                    || targetType == SkillTargetType.EnemyAll
                    || targetType == SkillTargetType.AllyAll)
                {
                    return true;
                }

                return pendingAction.Targets != null && pendingAction.Targets.Count > 0;
            }

            return false;
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
                foreach (MonsterBase monster in Spawner.SpawnedT)
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
}

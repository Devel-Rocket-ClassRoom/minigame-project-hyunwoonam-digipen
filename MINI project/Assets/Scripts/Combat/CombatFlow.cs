using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable CS0414

/// <summary>
/// 전투의 턴 상태입니다.
/// </summary>
public enum CombatTurnState
{
    Idle,
    RoundStart,
    PlayerAction,
    TargetSelect,
    MonsterAction,
    Victory,
    Defeat,
}

public enum CombatActorType
{
    Player,
    Monster,
}

public sealed class CombatActorTurn
{
    public CombatActorType ActorType { get; }
    public EntityBase Actor { get; }
    public int MonsterIndex { get; }
    public int BaseInitiative { get; }
    public int InitiativeBonus { get; }
    public int Initiative { get; }
    public int MonsterGrade { get; }

    public CombatActorTurn(
        CombatActorType actorType,
        EntityBase actor,
        int monsterIndex,
        int baseInitiative,
        int initiativeBonus,
        int monsterGrade
    )
    {
        ActorType = actorType;
        Actor = actor;
        MonsterIndex = monsterIndex;
        BaseInitiative = baseInitiative;
        InitiativeBonus = initiativeBonus;
        Initiative = baseInitiative + initiativeBonus;
        MonsterGrade = monsterGrade;
    }
}

/// <summary>
/// Combat 씬의 턴 흐름을 단일 FSM으로 관리합니다.
/// </summary>
/// <remarks>
/// Week 1 범위:
/// - 다키스트 던전식 라운드 기반 개별 유닛 턴 큐
/// - 임시 initiative = ATK + 작은 랜덤 보정
/// - 동률이면 Player 우선, Monster끼리는 MonsterGrade 우선
/// - 플레이어 행동 4종: Attack / Skill1 / Skill2 / Defend
/// - Attack / Skill은 Raycast 대상 선택 후 실행
/// - 아이템: 턴 미소비, Week 1엔 로그만 출력
/// - 몬스터 행동: Attack 70 / Skill 20 / Defend 10 가중치
/// - 데미지 = max(1, ATK - DEF), 방어 시 절반
/// - 모든 주요 지점에 Debug.Log 출력
/// Player / Monster는 별도 FSM을 갖지 않고 행동 데이터만 제공합니다.
/// </remarks>
public class CombatFlow : MonoBehaviour
{
    public const float MinimumActorActionSeconds = 1f;

    [Header("참조 (비워두면 씬에서 자동 탐색)")]
    [SerializeField]
    private Player player;

    [SerializeField]
    private List<MonsterBase> monsters = new List<MonsterBase>();

    [Header("전투 규칙")]
    [Tooltip("방어 중 받는 데미지 배율")]
    [SerializeField]
    private float defendDamageMultiplier = 0.5f;

    [Tooltip(
        "다키스트 던전식 라운드 주도권 보정값 범위입니다. 현재 initiative = ATK + Random.Range(0, initiativeBonusMax + 1)."
    )]
    [SerializeField]
    private int initiativeBonusMax = 2;

    [Header("행동 연출 시간")]
    [Tooltip(
        "각 액터의 행동이 끝나기 전 최소로 유지할 시간입니다. 이펙트/모션이 붙으면 이 시간 이후 완료 신호까지 기다리는 구조로 확장합니다."
    )]
    [SerializeField]
    private float actorActionHoldSeconds = MinimumActorActionSeconds;

    [Header("UI / 피격 이펙트")]
    [SerializeField]
    private GameObject hitEffectPrefab;

    [SerializeField]
    private float hitEffectLifetimeSeconds = CombatHitEffectPresenter.DefaultEffectLifetimeSeconds;

    private CombatTurnState turnState = CombatTurnState.Idle;
    private CombatActionType pendingPlayerAction;
    private ActiveSkill pendingPlayerSkill;
    private bool hasPendingPlayerAction;
    private bool isResolvingActorAction;
    private Coroutine actorActionRoutine;
    private PlayerStatsPanelUI playerStatsPanelUI;
    private readonly List<CombatActorTurn> turnQueue = new List<CombatActorTurn>();
    private int currentTurnIndex;
    private CombatActorTurn currentActorTurn;

    /// <summary>
    /// 현재 턴 상태입니다.
    /// </summary>
    public CombatTurnState TurnState => turnState;

    public static float GetActionHoldSeconds(float configuredSeconds)
    {
        return Mathf.Max(MinimumActorActionSeconds, configuredSeconds);
    }

    public static bool CanAcceptPlayerInputForState(CombatTurnState state, bool isResolvingAction)
    {
        return state == CombatTurnState.PlayerAction && !isResolvingAction;
    }

    private void ChangeState(CombatTurnState nextState, bool forceEnter = false)
    {
        // TODO:
        // - 목표: CombatFlow의 모든 상태 전환을 단일 진입점으로 통제한다.
        // - 의도: turnState가 바뀌는 순간 switch로 해당 상태의 진입 동작이 반드시 실행되게 한다.
        // - 구현해야 할 것: 중복 전환 방어, 상태 변경 로그, 상태별 Enter 메서드 호출을 수행한다.
        if (ShouldSkipStateEntry(turnState, nextState, forceEnter))
        {
            return;
        }

        CombatTurnState previousState = turnState;
        turnState = nextState;
        Debug.Log($"[CombatFlow] State: {previousState} -> {turnState}");

        switch (turnState)
        {
            case CombatTurnState.RoundStart:
                EnterRoundStart();
                break;
            case CombatTurnState.PlayerAction:
                EnterPlayerAction();
                break;
            case CombatTurnState.TargetSelect:
                EnterTargetSelect();
                break;
            case CombatTurnState.MonsterAction:
                EnterMonsterAction();
                break;
            case CombatTurnState.Victory:
                EnterVictory();
                break;
            case CombatTurnState.Defeat:
                EnterDefeat();
                break;
            default:
                Debug.LogWarning($"[CombatFlow] Unknown turn state: {turnState}");
                break;
        }
    }

    internal static bool ShouldSkipStateEntry(
        CombatTurnState currentState,
        CombatTurnState nextState,
        bool forceEnter
    )
    {
        return currentState == nextState && !forceEnter;
    }

    private void Start()
    {
        // 기존 구현:
        // ResolveSceneReferences();
        // if (!HasRequiredReferences())
        // {
        //     return;
        // }
        //
        // InitializeRuntimeState();
        //
        // bool playerFirst = DecideFirstAttacker();
        // Debug.Log(
        //     $"[CombatFlow] 선공 결정: {(playerFirst ? "Player" : "Monster")} "
        //         + $"(Player.ATK={player.ATK}, MonsterMaxATK={GetMaxMonsterATK()})"
        // );
        //
        // if (playerFirst)
        // {
        //     EnterPlayerTurn();
        // }
        // else
        // {
        //     EnterMonsterTurn();
        // }

        // TODO:
        // - 목표: Combat 씬 시작 시 참조, 런타임 상태, 선공자를 준비하고 첫 턴으로 진입한다.
        // - 의도: CombatFlow가 전투 전체의 단일 FSM 시작점이 되게 한다.
        // - 구현해야 할 것: 씬 참조 자동 탐색, 필수 참조 검증, HP/MP 준비, RoundStart 진입을 수행한다.
        ResolveSceneReferences();
        if (!HasRequiredReferences())
        {
            return;
        }

        InitializeRuntimeState();
        ChangeState(CombatTurnState.RoundStart);
    }

    private void Update()
    {
        // TODO:
        // - 목표: TargetSelect 상태에서 화면 클릭을 Raycast로 몬스터 선택 입력으로 변환한다.
        // - 의도: Attack/Skill 버튼은 행동 종류만 정하고, 실제 대상은 플레이어가 몬스터 오브젝트를 클릭해 결정하게 한다.
        // - 구현해야 할 것: TargetSelect 상태와 마우스 클릭을 확인하고, UI 클릭은 무시하며, Camera raycast 결과를 대상 선택으로 처리한다.
        if (turnState != CombatTurnState.TargetSelect)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelTargetSelection();
            return;
        }

        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        TrySelectTargetFromScreenPosition(Input.mousePosition);
    }

    private void EnterRoundStart()
    {
        // TODO:
        // - 목표: 다키스트 던전식 라운드 시작 시 모든 생존 유닛의 행동 순서 큐를 만든다.
        // - 의도: 진영 단위 PlayerTurn/MonsterTurn이 아니라 Player와 Monster가 initiative 순서대로 한 번씩 행동하게 한다.
        // - 구현해야 할 것: 승패 선검사, ATK+랜덤 보정 기반 큐 생성/정렬, 첫 액터 진행을 수행한다.
        if (TryEndCombatIfFinished())
        {
            return;
        }

        BuildTurnQueue();
        LogTurnQueue();
        currentTurnIndex = 0;
        AdvanceToNextActor();
    }

    private void ResolveSceneReferences()
    {
        // 기존 구현:
        // if (player == null)
        // {
        //     player = FindAnyObjectByType<Player>();
        // }
        //
        // if (monsters == null || monsters.Count == 0)
        // {
        //     monsters = new List<MonsterBase>(
        //         FindObjectsByType<MonsterBase>(FindObjectsSortMode.None)
        //     );
        // }

        // TODO:
        // - 목표: Inspector에 연결되지 않은 Player와 MonsterBase 참조를 씬에서 자동 탐색한다.
        // - 의도: Combat 씬의 수동 연결 누락을 줄이고 데모 실행 가능성을 높인다.
        // - 구현해야 할 것: player가 null이면 Player를 찾고, monsters가 비어 있으면 MonsterBase 목록을 수집한다.
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        if (monsters == null)
        {
            monsters = new List<MonsterBase>();
        }
        else
        {
            monsters.RemoveAll(monster => monster == null);
        }

        RefreshSceneMonsterReferences();
    }

    private bool HasRequiredReferences()
    {
        // TODO:
        // - 목표: 전투 시작에 필요한 Player와 Monster 참조가 모두 존재하는지 검증한다.
        // - 의도: 참조 누락 상태에서 전투 로직이 NullReferenceException으로 진행되지 않게 한다.
        // - 구현해야 할 것: player와 monsters를 검사하고 누락 시 에러 로그와 enabled=false 처리 후 false를 반환한다.
        if (player == null)
        {
            Debug.LogError("[CombatFlow] Player reference is missing.");
            enabled = false;
            return false;
        }

        if (monsters == null || monsters.Count == 0)
        {
            Debug.LogError("[CombatFlow] Monster reference is missing.");
            enabled = false;
            return false;
        }

        return true;
    }

    private void InitializeRuntimeState()
    {
        // TODO:
        // - 목표: Player와 모든 Monster의 전투 런타임 상태를 새 전투 상태로 초기화한다.
        // - 의도: Player의 HP/MP는 현재 도전 상태로 유지하고, Monster는 새 전투마다 초기화한다.
        // - 구현해야 할 것: 저장된 Player 런타임 상태가 있으면 복원하고, Player는 전투 상태만 준비하며, 각 Monster는 새 전투 상태로 리셋한다.
        if (GameSystemManager.TryGetInstance(out GameSystemManager gameSystemManager))
        {
            gameSystemManager.TryApplyPlayerCombatState(player);
        }

        player.ResetForNewCombat();
        EntityWorldUI.EnsureFor(player, false);
        playerStatsPanelUI = PlayerStatsPanelUI.EnsureForScene(player);
        RefreshPlayerStatsPanel();

        foreach (var monster in monsters)
        {
            monster.ResetForNewCombat();
            EntityWorldUI.EnsureFor(monster, true);
        }
    }

    private void BuildTurnQueue()
    {
        // TODO:
        // - 목표: 이번 라운드에 한 번씩 행동할 Player와 살아있는 Monster 목록을 만든다.
        // - 의도: 다키스트 던전처럼 라운드마다 모든 유닛이 initiative 순서에 따라 개별 턴을 갖게 한다.
        // - 구현해야 할 것: Player와 살아있는 Monster를 CombatActorTurn으로 만들고 initiative 내림차순으로 정렬한다.
        turnQueue.Clear();
        int bonusMax = Mathf.Max(0, initiativeBonusMax);

        if (!player.IsDead)
        {
            int bonus = Random.Range(0, bonusMax + 1);
            turnQueue.Add(
                new CombatActorTurn(
                    CombatActorType.Player,
                    player,
                    -1,
                    player.ATK,
                    bonus,
                    int.MaxValue
                )
            );
        }

        for (int i = 0; i < monsters.Count; i++)
        {
            MonsterBase monster = monsters[i];
            if (monster == null || monster.IsDead)
            {
                continue;
            }

            int bonus = Random.Range(0, bonusMax + 1);
            turnQueue.Add(
                new CombatActorTurn(
                    CombatActorType.Monster,
                    monster,
                    i,
                    monster.ATK,
                    bonus,
                    monster.MonsterGrade
                )
            );
        }

        turnQueue.Sort(CompareActorTurns);
    }

    private int CompareActorTurns(CombatActorTurn left, CombatActorTurn right)
    {
        // TODO:
        // - 목표: ATK+보정 initiative, Player 우선, Monster 등급 순으로 행동 순서를 정렬한다.
        // - 의도: 다키스트 던전식 라운드 순서에 사용자 지정 동률 규칙을 적용한다.
        // - 구현해야 할 것: initiative 내림차순, Player-vs-Monster 동률 시 Player 우선, Monster 동률 시 grade 내림차순을 적용한다.
        int initiativeCompare = right.Initiative.CompareTo(left.Initiative);
        if (initiativeCompare != 0)
        {
            return initiativeCompare;
        }

        if (left.ActorType != right.ActorType)
        {
            return left.ActorType == CombatActorType.Player ? -1 : 1;
        }

        if (left.ActorType == CombatActorType.Monster)
        {
            int gradeCompare = right.MonsterGrade.CompareTo(left.MonsterGrade);
            if (gradeCompare != 0)
            {
                return gradeCompare;
            }

            return left.MonsterIndex.CompareTo(right.MonsterIndex);
        }

        return 0;
    }

    private void LogTurnQueue()
    {
        // TODO:
        // - 목표: 라운드 시작마다 행동 순서와 initiative 계산 근거를 로그로 남긴다.
        // - 의도: 선공/순서 규칙이 디버깅 가능하도록 한다.
        // - 구현해야 할 것: Player/Monster index, ATK, 보정, 최종 initiative를 순서대로 출력한다.
        List<string> entries = new List<string>();
        for (int i = 0; i < turnQueue.Count; i++)
        {
            CombatActorTurn actorTurn = turnQueue[i];
            string actorName =
                actorTurn.ActorType == CombatActorType.Player
                    ? "Player"
                    : $"Monster[{actorTurn.MonsterIndex}] grade={actorTurn.MonsterGrade}";
            entries.Add(
                $"{i + 1}:{actorName} initiative={actorTurn.Initiative}"
                    + $"(ATK={actorTurn.BaseInitiative}+{actorTurn.InitiativeBonus})"
            );
        }

        Debug.Log($"[CombatFlow] Round order: {string.Join(" | ", entries)}");
    }

    private void AdvanceToNextActor()
    {
        // TODO:
        // - 목표: 현재 라운드 큐에서 다음 생존 유닛의 행동 상태로 전환한다.
        // - 의도: 각 유닛이 라운드마다 한 번씩 행동하고, 큐가 끝나면 다음 라운드를 시작한다.
        // - 구현해야 할 것: 승패 확인, 사망 유닛 skip, PlayerAction/MonsterAction 전환, 큐 종료 시 RoundStart 전환을 수행한다.
        if (TryEndCombatIfFinished())
        {
            return;
        }

        while (currentTurnIndex < turnQueue.Count)
        {
            currentActorTurn = turnQueue[currentTurnIndex];
            currentTurnIndex++;

            if (currentActorTurn.Actor == null || currentActorTurn.Actor.IsDead)
            {
                continue;
            }

            CombatTurnState nextState =
                currentActorTurn.ActorType == CombatActorType.Player
                    ? CombatTurnState.PlayerAction
                    : CombatTurnState.MonsterAction;
            ChangeState(nextState, true);
            return;
        }

        currentActorTurn = null;
        ChangeState(CombatTurnState.RoundStart);
    }

    private void EnterPlayerAction()
    {
        // 기존 구현:
        // turnState = CombatTurnState.PlayerTurn;
        // player.SetDefending(false);
        // Debug.Log("[CombatFlow] === Player Turn ===");

        // TODO:
        // - 목표: 라운드 큐에서 Player 차례가 왔을 때 입력 대기 상태로 진입한다.
        // - 의도: 다키스트 던전식 개별 유닛 턴에서 Player만 직접 입력을 기다리게 한다.
        // - 구현해야 할 것: 방어 상태 초기화, 현재 액터 검증, 턴 진입 로그 출력을 수행한다.
        player.SetDefending(false);
        Debug.Log("[CombatFlow] === Player Action ===");
    }

    private void EnterTargetSelect()
    {
        // TODO:
        // - 목표: 플레이어가 공격/스킬 대상을 클릭할 수 있는 상태로 진입한다.
        // - 의도: 여러 몬스터 중 어떤 몬스터를 공격할지 명시적으로 선택하게 한다.
        // - 구현해야 할 것: 대기 중인 행동이 없으면 PlayerAction으로 되돌리고, 있으면 선택 안내 로그를 출력한다.
        if (!hasPendingPlayerAction)
        {
            Debug.LogWarning("[CombatFlow] TargetSelect entered without a pending player action.");
            ChangeState(CombatTurnState.PlayerAction);
            return;
        }

        Debug.Log($"[CombatFlow] Target Select: {pendingPlayerAction}. Click a monster target.");
    }

    private void EnterMonsterAction()
    {
        // 기존 구현:
        // turnState = CombatTurnState.MonsterTurn;
        // Debug.Log("[CombatFlow] === Monster Turn ===");
        // ResolveAllMonsterActions();
        //
        // if (IsPlayerDefeated())
        // {
        //     EndCombatWithResult(DemoCombatResult.Defeat);
        //     return;
        // }
        //
        // EnterPlayerTurn();

        // TODO:
        // - 목표: 라운드 큐에서 지정된 몬스터 한 마리의 행동을 처리한다.
        // - 의도: 몬스터 그룹 전체가 몰아서 행동하지 않고, 다키스트 던전처럼 큐 순서대로 개별 행동하게 한다.
        // - 구현해야 할 것: 현재 몬스터 검증, 단일 몬스터 행동 실행, 다음 액터로 진행한다.
        if (
            currentActorTurn == null
            || currentActorTurn.ActorType != CombatActorType.Monster
            || currentActorTurn.MonsterIndex < 0
            || currentActorTurn.MonsterIndex >= monsters.Count
        )
        {
            Debug.LogWarning("[CombatFlow] MonsterAction entered without a valid monster actor.");
            AdvanceToNextActor();
            return;
        }

        Debug.Log($"[CombatFlow] === Monster[{currentActorTurn.MonsterIndex}] Action ===");
        CombatActionType resolvedAction = ResolveSingleMonsterAction(currentActorTurn.MonsterIndex);
        StartActorActionHold(resolvedAction, AdvanceToNextActor);
    }

    /// <summary>
    /// Attack 버튼: 기본 공격.
    /// </summary>
    public void OnAttackButton()
    {
        // 기존 구현:
        // if (turnState != CombatTurnState.PlayerTurn)
        // {
        //     return;
        // }
        //
        // int target = GetFirstAliveMonster();
        // if (target < 0)
        // {
        //     return;
        // }
        //
        // int dmg = CalculateDamage(player.ATK, monsters[target].DEF, monsters[target].IsDefending);
        // Debug.Log($"[CombatFlow] Player Attack → Monster[{target}], damage={dmg}");
        // ApplyDamageToMonster(target, dmg);
        //
        // AfterPlayerMainAction();

        // TODO:
        // - 목표: 플레이어 기본 공격 버튼 입력을 대상 선택 대기 상태로 전환한다.
        // - 의도: 여러 몬스터 중 어떤 몬스터를 공격할지 Raycast 클릭으로 결정하게 한다.
        // - 구현해야 할 것: PlayerAction 검증 후 Attack 행동을 pending으로 저장하고 TargetSelect에 진입한다.
        if (!CanAcceptPlayerInput())
        {
            return;
        }

        BeginTargetSelection(CombatActionType.Attack, null);
    }

    /// <summary>
    /// Skill 1 버튼: 안전지대에서 설정한 슬롯 1 스킬.
    /// </summary>
    public void OnSkill1Button()
    {
        // 기존 구현:
        // if (turnState != CombatTurnState.PlayerTurn)
        // {
        //     return;
        // }
        // UsePlayerSkill(player.Skill1);

        // TODO:
        // - 목표: 플레이어 Skill1 버튼 입력을 처리한다.
        // - 의도: 슬롯 1 액티브 스킬 사용을 공통 스킬 처리 함수로 위임한다.
        // - 구현해야 할 것: PlayerAction 검증 후 player.Skill1을 UsePlayerSkill에 전달한다.
        if (!CanAcceptPlayerInput())
        {
            return;
        }

        UsePlayerSkill(player.Skill1);
    }

    /// <summary>
    /// Skill 2 버튼: 안전지대에서 설정한 슬롯 2 스킬.
    /// </summary>
    public void OnSkill2Button()
    {
        // 기존 구현:
        // if (turnState != CombatTurnState.PlayerTurn)
        // {
        //     return;
        // }
        // UsePlayerSkill(player.Skill2);

        // TODO:
        // - 목표: 플레이어 Skill2 버튼 입력을 처리한다.
        // - 의도: 슬롯 2 액티브 스킬 사용을 공통 스킬 처리 함수로 위임한다.
        // - 구현해야 할 것: PlayerAction 검증 후 player.Skill2를 UsePlayerSkill에 전달한다.
        if (!CanAcceptPlayerInput())
        {
            return;
        }

        UsePlayerSkill(player.Skill2);
    }

    private void UsePlayerSkill(ActiveSkill skill)
    {
        // 기존 구현:
        // if (skill == null)
        // {
        //     Debug.Log("[CombatFlow] 빈 스킬 슬롯입니다.");
        //     return;
        // }
        //
        // if (player.CurrentMP < skill.mpCost)
        // {
        //     Debug.Log(
        //         $"[CombatFlow] MP 부족: [{skill.skillName}] (필요 {skill.mpCost} / 보유 {player.CurrentMP})"
        //     );
        //     return;
        // }
        //
        // int target = GetFirstAliveMonster();
        // if (target < 0)
        // {
        //     return;
        // }
        //
        // player.TrySpendMP(skill.mpCost);
        // int skillATK = Mathf.RoundToInt(player.ATK * skill.atkMultiplier);
        // int dmg = CalculateDamage(skillATK, monsters[target].DEF, monsters[target].IsDefending);
        // Debug.Log(
        //     $"[CombatFlow] Player Skill [{skill.skillName}] → Monster[{target}], "
        //         + $"damage={dmg}, MP-{skill.mpCost} ({player.CurrentMP}/{player.MaxMP})"
        // );
        // ApplyDamageToMonster(target, dmg);
        //
        // AfterPlayerMainAction();

        // TODO:
        // - 목표: 플레이어 액티브 스킬 사용을 처리한다.
        // - 의도: Skill1/Skill2 버튼이 같은 MP 검증, 대상 선택, 데미지 계산 흐름을 공유하게 한다.
        // - 구현해야 할 것: 빈 슬롯/MP 부족/대상 없음 검증, MP 소비, 배율 기반 데미지 계산, 피해 적용, 메인 행동 후처리를 수행한다.
        if (skill == null)
        {
            Debug.Log("[CombatFlow] Skill slot empty. Button should be disabled by UI.");
            return;
        }

        if (player.CurrentMP < skill.mpCost)
        {
            Debug.Log(
                $"[CombatFlow] MP 부족: [{skill.skillName}] (필요 {skill.mpCost} / 보유 {player.CurrentMP})"
            );
            return;
        }

        BeginTargetSelection(CombatActionType.Skill, skill);
    }

    /// <summary>
    /// Defend 버튼: 이번 턴 동안 받는 데미지 감소.
    /// </summary>
    public void OnDefendButton()
    {
        // 기존 구현:
        // if (turnState != CombatTurnState.PlayerTurn)
        // {
        //     return;
        // }
        //
        // player.SetDefending(true);
        // Debug.Log("[CombatFlow] Player Defend");
        //
        // AfterPlayerMainAction();

        // TODO:
        // - 목표: 플레이어 방어 버튼 입력을 처리한다.
        // - 의도: 방어를 메인 행동으로 취급하고 다음 몬스터 턴 피해를 감소시킨다.
        // - 구현해야 할 것: PlayerAction 검증, player.SetDefending(true), 로그 출력, 메인 행동 후처리를 수행한다.
        if (!CanAcceptPlayerInput())
        {
            return;
        }

        player.SetDefending(true);
        ShowActionIndicator(player, CombatActionType.Defend);
        Debug.Log("[CombatFlow] Player Defend");

        StartActorActionHold(CombatActionType.Defend, AfterPlayerMainAction);
    }

    /// <summary>
    /// Item 버튼: 턴을 소비하지 않으며 메인 행동 확정 전까지 여러 번 사용 가능.
    /// Week 1에서는 로그만 출력합니다.
    /// </summary>
    public void OnItemButton()
    {
        // 기존 구현:
        // if (turnState != CombatTurnState.PlayerTurn)
        // {
        //     return;
        // }
        //
        // Debug.Log("[CombatFlow] Player Item Use (Week 1 stub, 턴 미소비)");

        // TODO:
        // - 목표: 플레이어 아이템 버튼 입력을 턴 미소비 보조 행동으로 처리한다.
        // - 의도: 메인 행동 전까지 아이템을 여러 번 사용할 수 있는 전투 규칙을 유지한다.
        // - 구현해야 할 것: PlayerAction 검증, 아이템 보유/효과 처리, 턴 상태 유지, Week 1에서는 최소 로그 출력을 수행한다.
        if (!CanAcceptPlayerInput())
        {
            return;
        }

        Debug.Log("[CombatFlow] Player Item Use (Week 1 stub, 턴 미소비)");
    }

    private void AfterPlayerMainAction()
    {
        // 기존 구현:
        // if (AreAllMonstersDead())
        // {
        //     EndCombatWithResult(DemoCombatResult.Victory);
        //     return;
        // }
        //
        // EnterMonsterTurn();

        // TODO:
        // - 목표: 플레이어 메인 행동 후 승리 여부를 판단하고 라운드 큐의 다음 유닛으로 넘긴다.
        // - 의도: Attack/Skill/Defend가 동일한 턴 소비 후처리를 공유하게 한다.
        // - 구현해야 할 것: 승패 확인 후 AdvanceToNextActor를 호출한다.
        AdvanceToNextActor();
    }

    private void BeginTargetSelection(CombatActionType actionType, ActiveSkill skill)
    {
        // TODO:
        // - 목표: 플레이어의 대상 지정이 필요한 행동을 저장하고 TargetSelect 상태로 진입한다.
        // - 의도: 버튼 클릭 시 즉시 첫 몬스터를 공격하지 않고, 다음 몬스터 클릭으로 대상이 확정되게 한다.
        // - 구현해야 할 것: 행동/스킬을 pending 필드에 저장하고 TargetSelect로 상태 전환한다.
        pendingPlayerAction = actionType;
        pendingPlayerSkill = skill;
        hasPendingPlayerAction = true;
        ChangeState(CombatTurnState.TargetSelect);
    }

    private void CancelTargetSelection()
    {
        // TODO:
        // - 목표: 대상 선택 대기 상태를 취소하고 PlayerAction으로 돌아간다.
        // - 의도: 잘못 Attack/Skill을 눌렀을 때 우클릭으로 행동 선택을 되돌릴 수 있게 한다.
        // - 구현해야 할 것: pending 행동을 비우고 PlayerAction으로 전환한다.
        ClearPendingPlayerAction();
        Debug.Log("[CombatFlow] Target selection canceled.");
        ChangeState(CombatTurnState.PlayerAction);
    }

    private void TrySelectTargetFromScreenPosition(Vector3 screenPosition)
    {
        // TODO:
        // - 목표: 화면 좌표를 카메라 Ray로 변환하고 2D Collider에 맞은 몬스터를 선택한다.
        // - 의도: 블로그 참고 방식(ScreenPointToRay -> Raycast)을 2D 전투 씬에 맞춰 사용한다.
        // - 구현해야 할 것: Camera.main, Physics2D.GetRayIntersection, MonsterBase 조회, 생존 여부 검증, 대상 확정 처리를 수행한다.
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[CombatFlow] Cannot select target because Camera.main is missing.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
        if (hit.collider == null)
        {
            Debug.Log("[CombatFlow] Target selection missed.");
            return;
        }

        MonsterBase selectedMonster = hit.collider.GetComponentInParent<MonsterBase>();
        if (selectedMonster == null)
        {
            Debug.Log("[CombatFlow] Raycast hit is not a monster target.");
            return;
        }

        int targetIndex = monsters.IndexOf(selectedMonster);
        if (targetIndex < 0)
        {
            Debug.LogWarning(
                "[CombatFlow] Selected monster is not registered in the combat monster list."
            );
            return;
        }

        if (selectedMonster.IsDead)
        {
            Debug.Log("[CombatFlow] Selected monster is already dead.");
            return;
        }

        ResolvePendingPlayerAction(targetIndex);
    }

    private void ResolvePendingPlayerAction(int targetIndex)
    {
        // TODO:
        // - 목표: 선택된 대상에게 pending 플레이어 행동을 실행한다.
        // - 의도: 대상 선택과 행동 실행을 분리해 Attack/Skill이 같은 TargetSelect 흐름을 공유하게 한다.
        // - 구현해야 할 것: Attack/Skill 분기, 데미지/MP 처리, pending 정리, 메인 행동 후처리를 수행한다.
        if (!hasPendingPlayerAction)
        {
            Debug.LogWarning("[CombatFlow] Target selected without a pending player action.");
            ChangeState(CombatTurnState.PlayerAction);
            return;
        }

        CombatActionType resolvedActionType = pendingPlayerAction;
        bool actionResolved;
        switch (pendingPlayerAction)
        {
            case CombatActionType.Attack:
                actionResolved = ResolvePlayerAttack(targetIndex);
                break;
            case CombatActionType.Skill:
                actionResolved = ResolvePlayerSkill(targetIndex, pendingPlayerSkill);
                break;
            default:
                Debug.LogWarning(
                    $"[CombatFlow] Unsupported pending player action: {pendingPlayerAction}"
                );
                ClearPendingPlayerAction();
                ChangeState(CombatTurnState.PlayerAction);
                return;
        }

        ClearPendingPlayerAction();
        if (actionResolved)
        {
            StartActorActionHold(resolvedActionType, AfterPlayerMainAction);
            return;
        }

        ChangeState(CombatTurnState.PlayerAction);
    }

    private bool ResolvePlayerAttack(int targetIndex)
    {
        // TODO:
        // - 목표: 선택된 몬스터에게 플레이어 기본 공격을 적용한다.
        // - 의도: 대상 선택 이후의 실제 공격 처리를 별도 함수로 격리한다.
        // - 구현해야 할 것: 데미지 계산, 로그 출력, 몬스터 피해 적용을 수행한다.
        int dmg = CalculateDamage(
            player.ATK,
            monsters[targetIndex].DEF,
            monsters[targetIndex].IsDefending
        );
        ShowActionIndicator(player, CombatActionType.Attack);
        Debug.Log($"[CombatFlow] Player Attack -> Monster[{targetIndex}], damage={dmg}");
        ApplyDamageToMonster(targetIndex, dmg);
        return true;
    }

    private bool ResolvePlayerSkill(int targetIndex, ActiveSkill skill)
    {
        // TODO:
        // - 목표: 선택된 몬스터에게 플레이어 액티브 스킬을 적용한다.
        // - 의도: 스킬 MP 소비는 대상 선택이 확정된 뒤에만 발생하게 한다.
        // - 구현해야 할 것: 스킬/MP 재검증, MP 소비, 배율 데미지 계산, 몬스터 피해 적용을 수행한다.
        if (skill == null)
        {
            Debug.LogWarning("[CombatFlow] Pending skill is empty.");
            return false;
        }

        if (!player.TrySpendMP(skill.mpCost))
        {
            Debug.Log(
                $"[CombatFlow] MP 부족: [{skill.skillName}] (필요 {skill.mpCost} / 보유 {player.CurrentMP})"
            );
            return false;
        }

        int skillATK = Mathf.RoundToInt(player.ATK * skill.atkMultiplier);
        int dmg = CalculateDamage(
            skillATK,
            monsters[targetIndex].DEF,
            monsters[targetIndex].IsDefending
        );
        Debug.Log(
            $"[CombatFlow] Player Skill [{skill.skillName}] -> Monster[{targetIndex}], "
                + $"damage={dmg}, MP-{skill.mpCost} ({player.CurrentMP}/{player.MaxMP})"
        );
        ShowActionIndicator(player, CombatActionType.Skill);
        ApplyDamageToMonster(targetIndex, dmg);
        return true;
    }

    private void ClearPendingPlayerAction()
    {
        // TODO:
        // - 목표: TargetSelect에서 사용한 임시 행동 데이터를 정리한다.
        // - 의도: 다음 플레이어 행동이 이전 대상 선택 데이터에 영향받지 않게 한다.
        // - 구현해야 할 것: pending action/skill/flag를 기본값으로 되돌린다.
        pendingPlayerAction = CombatActionType.Attack;
        pendingPlayerSkill = null;
        hasPendingPlayerAction = false;
    }

    private CombatActionType ResolveSingleMonsterAction(int index)
    {
        // 기존 구현:
        // MonsterBase m = monsters[index];
        // CombatActionType action = m.DecideAction();
        //
        // switch (action)
        // {
        //     case CombatActionType.Attack:
        //     {
        //         int dmg = CalculateDamage(m.ATK, player.DEF, player.IsDefending);
        //         Debug.Log($"[CombatFlow] Monster[{index}] Attack → Player, damage={dmg}");
        //         ApplyDamageToPlayer(dmg);
        //         break;
        //     }
        //     case CombatActionType.Skill:
        //     {
        //         ActiveSkill skill = m.GetActiveSkill(0);
        //         if (skill == null)
        //         {
        //             int fallbackDamage = CalculateDamage(m.ATK, player.DEF, player.IsDefending);
        //             Debug.Log(
        //                 $"[CombatFlow] Monster[{index}] Skill slot empty. Attack fallback → Player, damage={fallbackDamage}"
        //             );
        //             ApplyDamageToPlayer(fallbackDamage);
        //             break;
        //         }
        //
        //         int skillATK = Mathf.RoundToInt(m.ATK * skill.atkMultiplier);
        //         int dmg = CalculateDamage(skillATK, player.DEF, player.IsDefending);
        //         Debug.Log(
        //             $"[CombatFlow] Monster[{index}] Skill [{skill.skillName}] → Player, damage={dmg}"
        //         );
        //         ApplyDamageToPlayer(dmg);
        //         break;
        //     }
        //     case CombatActionType.Defend:
        //     {
        //         m.SetDefending(true);
        //         Debug.Log($"[CombatFlow] Monster[{index}] Defend");
        //         break;
        //     }
        // }

        // TODO:
        // - 목표: 특정 몬스터 한 마리의 결정된 행동을 실행한다.
        // - 의도: MonsterBase.DecideAction 결과를 CombatFlow의 피해/방어 처리와 연결한다.
        // - 구현해야 할 것: Attack/Skill/Defend 분기, 빈 스킬 슬롯 fallback, 데미지 계산/적용, 방어 상태 설정, 로그 출력을 수행한다.
        MonsterBase monster = monsters[index];
        CombatActionType action = monster.DecideAction();

        switch (action)
        {
            case CombatActionType.Attack:
            {
                int dmg = CalculateDamage(monster.ATK, player.DEF, player.IsDefending);
                ShowActionIndicator(monster, CombatActionType.Attack);
                Debug.Log($"[CombatFlow] Monster[{index}] Attack -> Player, damage={dmg}");
                ApplyDamageToPlayer(dmg);
                break;
            }
            case CombatActionType.Skill:
            {
                ActiveSkill skill = monster.GetActiveSkill(0);
                if (skill == null)
                {
                    Debug.LogWarning(
                        $"[CombatFlow] Monster[{index}] selected Skill, but its skill slot is empty."
                    );
                    return action;
                }

                int skillATK = Mathf.RoundToInt(monster.ATK * skill.atkMultiplier);
                int dmg = CalculateDamage(skillATK, player.DEF, player.IsDefending);
                ShowActionIndicator(monster, CombatActionType.Skill);
                Debug.Log(
                    $"[CombatFlow] Monster[{index}] Skill [{skill.skillName}] -> Player, damage={dmg}"
                );
                ApplyDamageToPlayer(dmg);
                break;
            }
            case CombatActionType.Defend:
            {
                monster.SetDefending(true);
                ShowActionIndicator(monster, CombatActionType.Defend);
                Debug.Log($"[CombatFlow] Monster[{index}] Defend");
                break;
            }
            default:
            {
                int fallbackDamage = CalculateDamage(monster.ATK, player.DEF, player.IsDefending);
                Debug.LogWarning(
                    $"[CombatFlow] Monster[{index}] unsupported action {action}. Attack fallback -> Player, damage={fallbackDamage}"
                );
                ApplyDamageToPlayer(fallbackDamage);
                break;
            }
        }

        return action;
    }

    private int CalculateDamage(int atk, int def, bool defending)
    {
        // 기존 구현:
        // int raw = Mathf.Max(1, atk - def);
        // if (defending)
        // {
        //     raw = Mathf.Max(1, Mathf.RoundToInt(raw * defendDamageMultiplier));
        // }
        // return raw;

        // TODO:
        // - 목표: ATK, DEF, 방어 상태를 바탕으로 최종 데미지를 계산한다.
        // - 의도: Week 1 데모의 단순 데미지 규칙을 한 함수에 격리한다.
        // - 구현해야 할 것: 최소 1 피해 보장, 방어 시 defendDamageMultiplier 적용, 정수 반올림 보정을 수행한다.
        int raw = Mathf.Max(1, atk - def);
        if (defending)
        {
            raw = Mathf.Max(1, Mathf.RoundToInt(raw * defendDamageMultiplier));
        }

        return raw;
    }

    private void ApplyDamageToPlayer(int dmg)
    {
        // 기존 구현:
        // int appliedDamage = player.TakeDamage(dmg);
        // Debug.Log($"[CombatFlow] Player HP: {player.CurrentHP}/{player.MaxHP} (-{appliedDamage})");
        // if (player.IsDead)
        // {
        //     Debug.Log("[CombatFlow] Player 사망");
        // }

        // TODO:
        // - 목표: 계산된 피해를 Player에게 적용하고 HP 변화와 사망을 기록한다.
        // - 의도: CombatFlow가 전투 로그와 사망 판정을 일관되게 관리한다.
        // - 구현해야 할 것: player.TakeDamage 호출, HP 로그 출력, player.IsDead일 때 사망 로그 출력을 수행한다.
        int appliedDamage = player.TakeDamage(dmg);
        RefreshPlayerStatsPanel();
        ShowHitEffect(player, appliedDamage);
        Debug.Log($"[CombatFlow] Player HP: {player.CurrentHP}/{player.MaxHP} (-{appliedDamage})");
        if (player.IsDead)
        {
            Debug.Log("[CombatFlow] Player 사망");
        }
    }

    private void ApplyDamageToMonster(int index, int dmg)
    {
        // 기존 구현:
        // int appliedDamage = monsters[index].TakeDamage(dmg);
        // Debug.Log(
        //     $"[CombatFlow] Monster[{index}] HP: {monsters[index].CurrentHP}/{monsters[index].MaxHP} (-{appliedDamage})"
        // );
        // if (monsters[index].IsDead)
        // {
        //     Debug.Log($"[CombatFlow] Monster[{index}] 사망");
        // }

        // TODO:
        // - 목표: 계산된 피해를 지정 몬스터에게 적용하고 HP 변화와 사망을 기록한다.
        // - 의도: 플레이어 행동 처리에서 몬스터 피해 적용을 공통화한다.
        // - 구현해야 할 것: monsters[index].TakeDamage 호출, HP 로그 출력, 사망 시 사망 로그 출력을 수행한다.
        int appliedDamage = monsters[index].TakeDamage(dmg);
        EntityWorldUI.EnsureFor(monsters[index], true)?.UpdateHealthDisplay();
        ShowHitEffect(monsters[index], appliedDamage);
        Debug.Log(
            $"[CombatFlow] Monster[{index}] HP: {monsters[index].CurrentHP}/{monsters[index].MaxHP} (-{appliedDamage})"
        );
        if (monsters[index].IsDead)
        {
            Debug.Log($"[CombatFlow] Monster[{index}] 사망");
            monsters[index].Die();
        }
    }

    private void StartActorActionHold(CombatActionType actionType, System.Action onComplete)
    {
        if (actorActionRoutine != null)
        {
            StopCoroutine(actorActionRoutine);
            actorActionRoutine = null;
        }

        actorActionRoutine = StartCoroutine(HoldActorActionThenComplete(actionType, onComplete));
    }

    private IEnumerator HoldActorActionThenComplete(
        CombatActionType actionType,
        System.Action onComplete
    )
    {
        isResolvingActorAction = true;

        float holdSeconds = GetActionHoldSeconds(actorActionHoldSeconds);
        Debug.Log($"[CombatFlow] Hold {actionType} action for at least {holdSeconds:0.00}s.");
        yield return new WaitForSeconds(holdSeconds);

        actorActionRoutine = null;
        isResolvingActorAction = false;
        onComplete?.Invoke();
    }

    private bool AreAllMonstersDead()
    {
        // 기존 구현:
        // for (int i = 0; i < monsters.Count; i++)
        // {
        //     if (!monsters[i].IsDead)
        //     {
        //         return false;
        //     }
        // }
        // return true;

        // TODO:
        // - 목표: 전투 승리 조건인 몬스터 전멸 여부를 판단한다.
        // - 의도: 플레이어 메인 행동 후 Victory 전환 여부를 결정한다.
        // - 구현해야 할 것: 살아있는 몬스터가 하나라도 있으면 false, 모두 죽었으면 true를 반환한다.
        RefreshSceneMonsterReferences();

        for (int i = 0; i < monsters.Count; i++)
        {
            if (!monsters[i].IsDead)
            {
                return false;
            }
        }

        return true;
    }

    private void RefreshSceneMonsterReferences()
    {
        if (monsters == null)
        {
            monsters = new List<MonsterBase>();
        }
        else
        {
            monsters.RemoveAll(monster => monster == null);
        }

        MonsterBase[] sceneMonsters = FindObjectsByType<MonsterBase>(FindObjectsSortMode.None);
        for (int i = 0; i < sceneMonsters.Length; i++)
        {
            MonsterBase sceneMonster = sceneMonsters[i];
            if (!monsters.Contains(sceneMonster))
            {
                monsters.Add(sceneMonster);
            }

            EntityWorldUI.EnsureFor(sceneMonster, true);
        }
    }

    private void ShowActionIndicator(EntityBase entity, CombatActionType actionType)
    {
        EntityWorldUI worldUI = EntityWorldUI.EnsureFor(entity, entity is MonsterBase);
        if (worldUI == null)
        {
            return;
        }

        worldUI.ShowAction(actionType, GetActionHoldSeconds(actorActionHoldSeconds));
    }

    private void RefreshPlayerStatsPanel()
    {
        if (player == null)
        {
            return;
        }

        if (playerStatsPanelUI == null)
        {
            playerStatsPanelUI = PlayerStatsPanelUI.EnsureForScene(player);
        }

        playerStatsPanelUI?.Refresh();
    }

    private void ShowHitEffect(EntityBase target, int appliedDamage)
    {
        if (target == null || appliedDamage <= 0)
        {
            return;
        }

        GameObject effectPrefab =
            hitEffectPrefab != null ? hitEffectPrefab : CombatHitEffectPresenter.LoadSplashPrefab();
        if (effectPrefab == null)
        {
            Debug.LogWarning(
                $"[CombatFlow] Hit effect prefab is missing. Expected {CombatHitEffectPresenter.SplashPrefabAssetPath}."
            );
            return;
        }

        CombatHitEffectPresenter.SpawnHitEffect(
            target,
            effectPrefab,
            Mathf.Max(0f, hitEffectLifetimeSeconds)
        );
    }

    private bool TryEndCombatIfFinished()
    {
        // TODO:
        // - 목표: 라운드 큐 진행 전후에 전투 종료 조건을 공통으로 확인한다.
        // - 의도: 어떤 유닛 행동 뒤에도 Victory/Defeat가 즉시 반영되게 한다.
        // - 구현해야 할 것: Monster 전멸은 Victory, Player 사망은 Defeat로 전환하고 종료 여부를 반환한다.
        if (AreAllMonstersDead())
        {
            EndCombatWithResult(DemoCombatResult.Victory);
            return true;
        }

        if (IsPlayerDefeated())
        {
            EndCombatWithResult(DemoCombatResult.Defeat);
            return true;
        }

        return false;
    }

    private bool IsPlayerDefeated()
    {
        // 기존 구현:
        // return player.IsDead;

        // TODO:
        // - 목표: 플레이어 패배 조건을 판단한다.
        // - 의도: 몬스터 행동 후 Defeat 전환 여부를 결정한다.
        // - 구현해야 할 것: player.IsDead 값을 반환한다.
        return player.IsDead;
    }

    private void EndCombatWithResult(DemoCombatResult result)
    {
        // 기존 구현:
        // turnState =
        //     result == DemoCombatResult.Victory ? CombatTurnState.Victory : CombatTurnState.Defeat;
        // Debug.Log($"[CombatFlow] 전투 종료: {result}");
        // GameSystemManager.Instance.EndCombat(result);

        // TODO:
        // - 목표: 전투 FSM을 종료 상태로 바꾸고 결과를 GameSystemManager에 보고한다.
        // - 의도: CombatFlow가 직접 씬을 로드하지 않고 전투 결과만 상위 시스템에 전달한다.
        // - 구현해야 할 것: Victory/Defeat turnState 설정, 종료 로그 출력, GameSystemManager.Instance.EndCombat(result) 호출을 수행한다.
        switch (result)
        {
            case DemoCombatResult.Victory:
                ChangeState(CombatTurnState.Victory);
                break;
            case DemoCombatResult.Defeat:
                ChangeState(CombatTurnState.Defeat);
                break;
            default:
                Debug.LogWarning($"[CombatFlow] Unsupported combat result: {result}");
                break;
        }
    }

    private bool CanAcceptPlayerInput()
    {
        // TODO:
        // - 목표: 플레이어 입력이 현재 전투 상태에서 유효한지 판단한다.
        // - 의도: UI 버튼은 항상 호출될 수 있지만 실제 행동은 PlayerAction에서만 처리되게 한다.
        // - 구현해야 할 것: turnState가 PlayerAction인지 확인하고 아니면 로그 없이 입력을 무시한다.
        return CanAcceptPlayerInputForState(turnState, isResolvingActorAction);
    }

    private void EnterVictory()
    {
        // TODO:
        // - 목표: Victory 상태 진입 시 전투 종료 로그와 상위 시스템 보고를 수행한다.
        // - 의도: 종료 상태도 ChangeState의 switch를 통해 진입 동작을 실행하게 한다.
        // - 구현해야 할 것: Victory 로그 출력 후 GameSystemManager에 결과를 보고한다.
        Debug.Log("[CombatFlow] 전투 종료: Victory");
        ReportCombatResult(DemoCombatResult.Victory);
    }

    private void EnterDefeat()
    {
        // TODO:
        // - 목표: Defeat 상태 진입 시 전투 종료 로그와 상위 시스템 보고를 수행한다.
        // - 의도: 종료 상태도 ChangeState의 switch를 통해 진입 동작을 실행하게 한다.
        // - 구현해야 할 것: Defeat 로그 출력 후 GameSystemManager에 결과를 보고한다.
        Debug.Log("[CombatFlow] 전투 종료: Defeat");
        ReportCombatResult(DemoCombatResult.Defeat);
    }

    private void ReportCombatResult(DemoCombatResult result)
    {
        // TODO:
        // - 목표: CombatFlow가 직접 씬을 로드하지 않고 전투 결과만 GameSystemManager에 전달한다.
        // - 의도: Combat 씬 단독 실행이나 임시 테스트에서 GameSystemManager가 없을 때 NullReferenceException을 막는다.
        // - 구현해야 할 것: GameSystemManager가 있으면 EndCombat을 호출하고, 없으면 경고 로그만 남긴다.
        if (!GameSystemManager.TryGetInstance(out GameSystemManager gameSystemManager))
        {
            Debug.LogWarning(
                $"[CombatFlow] GameSystemManager is missing. Result was not reported: {result}"
            );
            return;
        }

        gameSystemManager.RecordPlayerCombatState(player);
        gameSystemManager.EndCombat(result);
    }
}

#pragma warning restore CS0414

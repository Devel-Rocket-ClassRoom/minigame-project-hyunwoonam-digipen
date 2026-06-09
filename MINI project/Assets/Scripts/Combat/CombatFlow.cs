using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 단일 FSM. Player/Companion/Monster 별도 FSM 금지.
/// 라운드 큐를 순회하며 행동을 결정/실행/대기한다.
/// </summary>
public sealed class CombatFlow
{
    /// <summary>현재 라운드 큐.</summary>
    public ActionQueue Queue { get; private set; }

    /// <summary>플레이어 측 엔티티(Player + 동료 ≤3).</summary>
    public List<EntityBase> AlliesT;

    /// <summary>적 엔티티(Monster 1~3).</summary>
    public List<EntityBase> EnemiesT;

    /// <summary>현재 FSM 상태.</summary>
    public CombatState State { get; private set; }

    /// <summary>현재 라운드 번호.</summary>
    public int RoundNumber { get; private set; }

    /// <summary>플레이어 입력을 받기 위한 인터페이스.</summary>
    public IPlayerInputProvider Input;

    /// <summary>타겟 선택 유틸.</summary>
    public TargetSelector TargetSelector;

    /// <summary>몬스터 행동 선택기.</summary>
    public MonsterActionSelector MonsterAi;

    /// <summary>동료 행동 선택기.</summary>
    public CompanionActionSelector CompanionAi;

    /// <summary>사망 몬스터를 연출 후 제거하는 씬 측 어댑터.</summary>
    public IDeadEnemyRemover DeadEnemyRemover;

    // 연출 타임라인(2026-06-05): Resolving 은 Anticipation→Impact→Recovery 로 진행한다.
    private CombatAction currentAction;
    private ActionPhaseTiming currentTiming;
    private CombatActionPhase actionPhase;
    private float phaseTimer;
    private float actingDelayTimer;
    private CombatAction delayedAiAction;
    private bool firstActionDelayAvailable;
    private float roundTransitionTimer;

    // 라운드 경계 비트(2026-06-05): 동일 행위자의 라운드 끝-시작 연속 행동을 시각적으로 분리.
    private const float DefaultFirstNonPlayerActionDelaySec = 0.8f;
    private const float DefaultRoundTransitionSec = 0.5f;

    // Wave0refactor 2026-05-27 (F.5): BeginRound / EndRound 가 매번 new List 를 만들지 않도록
    // 재사용 가능한 참가자 버퍼를 보유한다. 라운드 시작/종료 시 Clear 후 채운다.
    private readonly List<EntityBase> participantsBuffer = new List<EntityBase>();

    /// <summary>
    /// 전투 시작. CombatControllert가 호출.
    /// </summary>
    public void StartCombat(List<EntityBase> allies, List<EntityBase> enemies)
    {
        // 동작 요약:
        // - AlliesT = allies, EnemiesT = enemies.
        // - 각 엔티티 PrepareForCombat.
        // - Queue = new ActionQueue().
        // - RoundNumber = 1.
        // - BeginRound().
        AlliesT = allies ?? new List<EntityBase>();
        EnemiesT = enemies ?? new List<EntityBase>();
        foreach (EntityBase entity in AlliesT)
        {
            entity?.PrepareForCombat();
        }
        foreach (EntityBase entity in EnemiesT)
        {
            entity?.PrepareForCombat();
        }

        Queue = new ActionQueue();
        RoundNumber = 1;
        firstActionDelayAvailable = true;
        delayedAiAction = null;
        actingDelayTimer = 0f;
        // Guid5 §8 2026-05-29 — Starting 은 StartCombat 직후 1프레임 머무는 명시 진입 상태.
        State = CombatState.Starting;
    }

    /// <summary>
    /// 매 프레임 호출. 코루틴/await 대신 단순 펌프 방식.
    /// </summary>
    public void Tick()
    {
        // 동작 요약:
        // - State 분기:
        //   * Starting  → StartCombat 초기화 완료 후 State = RoundStart.
        //   * RoundStart → BeginRound() 호출 → State = NextActor.
        //   * NextActor  → Queue.PeekNext()로 다음 행위자 가져와 ResolveActor(actor) 호출.
        //                  큐 비어있으면 State = RoundEndCheck.
        //   * AwaitInput → Input.HasAction이면 Input.PopAction() → ResolveAction(action).
        //   * Resolving  → TickResolving(): Anticipation 경과 시 ApplyImpact()로 결과 적용,
        //                  Recovery 경과 시 Queue.ConsumeCurrent() 후 State = RoundEndCheck.
        //                  ※ 소모 아이템은 CombatController.PlayerUseItem()에서 즉시 처리하고
        //                    CombatFlow 큐에 CombatAction으로 넣지 않는다.
        //   * RoundTransition → 짧은 정지 후 State = RoundStart.
        //   * RoundEndCheck → Queue.IsRoundFinished()이면 EndRound()(→ RoundTransition).
        //                     아니면 State = NextActor.
        //   * Ended → 결과는 CombatController.OnSceneUpdate가 CheckOutcome()으로 감지.
        switch (State)
        {
            case CombatState.Starting:
                State = CombatState.RoundStart;
                break;
            case CombatState.RoundStart:
                BeginRound();
                break;
            case CombatState.NextActor:
                TurnEntry next = Queue?.PeekNext();
                if (next == null)
                {
                    State = CombatState.RoundEndCheck;
                    break;
                }
                ResolveActor(next.Actor);
                break;
            case CombatState.AwaitInput:
                if (Input != null && Input.HasAction)
                {
                    ResolveAction(Input.PopAction());
                }
                break;
            case CombatState.ActingDelay:
                actingDelayTimer -= Time.deltaTime;
                if (actingDelayTimer <= 0f)
                {
                    CombatAction action = delayedAiAction;
                    delayedAiAction = null;
                    ResolveAction(action);
                }
                break;
            // 연출 타임라인(2026-06-05): Anticipation 경과 후 Impact 적용, Recovery 경과 후 소비.
            case CombatState.Resolving:
                TickResolving();
                break;
            // 라운드 경계 비트(2026-06-05): 짧은 정지 후 다음 라운드 빌드.
            case CombatState.RoundTransition:
                roundTransitionTimer -= Time.deltaTime;
                if (roundTransitionTimer <= 0f)
                {
                    State = CombatState.RoundStart;
                }
                break;
            // F.2: CheckOutcome 호출은 ApplyImpact 단일 위치에서만(연출 타임라인 도입으로 ResolveAction→ApplyImpact 이동).
            // HANDOFFtemp §17 명세("CheckOutcome 단일 위치, EndRound/RoundEndCheck 중복 검사 제거")에 합치.
            case CombatState.RoundEndCheck:
                if (Queue == null || Queue.IsRoundFinished())
                {
                    EndRound();
                }
                else
                {
                    State = CombatState.NextActor;
                }
                break;
        }
    }

    // Wave0refactor 2026-05-27 (F.5): participantsBuffer 재사용.
    /// <summary>
    /// 새 라운드 준비.
    /// </summary>
    private void BeginRound()
    {
        // 동작 요약:
        // - participantsBuffer 를 AlliesT ∪ EnemiesT 로 채운다(재할당 없음).
        // - Queue.BuildRound(participantsBuffer) 호출.
        // - State = NextActor.
        RebuildParticipantsBuffer();
        if (Queue == null)
        {
            Queue = new ActionQueue();
        }
        Queue.BuildRound(participantsBuffer);
        State = CombatState.NextActor;
    }

    // Wave0refactor 2026-05-27 (F.5): 참가자 리스트 재구성을 한 곳으로.
    private void RebuildParticipantsBuffer()
    {
        participantsBuffer.Clear();
        if (AlliesT != null) participantsBuffer.AddRange(AlliesT);
        if (EnemiesT != null) participantsBuffer.AddRange(EnemiesT);
    }

    // Wave0refactor 2026-05-27 (F.5): participantsBuffer 재사용.
    /// <summary>
    /// 라운드 종료 처리.
    /// 승패 검사는 ResolveAction 에서만 수행하므로 여기서는 검사하지 않는다(F.2 단일 위치 규칙).
    /// </summary>
    private void EndRound()
    {
        // 동작 요약:
        // - participantsBuffer 재사용으로 OnRoundEnd 일괄 호출.
        // - 살아있는 모든 엔티티의 ActiveSkills 쿨다운 1 감소.
        // - RoundNumber += 1.
        // - State = RoundStart (BeginRound 는 다음 Tick 에서 호출).
        RebuildParticipantsBuffer();
        for (int i = 0; i < participantsBuffer.Count; i++)
        {
            EntityBase entity = participantsBuffer[i];
            if (entity == null) continue;
            entity.OnRoundEnd();
            if (entity.ActiveSkills == null) continue;
            for (int s = 0; s < entity.ActiveSkills.Length; s++)
            {
                entity.ActiveSkills[s]?.TickCooldown();
            }
        }

        RoundNumber += 1;
        // 라운드 경계 비트(2026-06-05): 같은 행위자가 라운드 끝-시작에 연속 행동해도
        // 시각적으로 분리되도록 짧은 전환 정지를 둔 뒤 다음 라운드를 빌드한다.
        roundTransitionTimer = DefaultRoundTransitionSec;
        State = CombatState.RoundTransition;
    }

    /// <summary>
    /// 현재 행위자의 행동을 받아온다(Player → 입력 대기, AI → 즉시 결정).
    /// </summary>
    private void ResolveActor(EntityBase actor)
    {
        // 동작 요약:
        // - actor가 Playert면 Input.RequestPlayerAction(actor) 호출 → State = AwaitInput.
        // - actor가 TeamBaset이면 CompanionAi.Pick() → ResolveAction.
        // - actor가 Monstert이면 MonsterAi.Pick() → ResolveAction.
        // Wave0refactor 2026-05-27 (F.5): ConsumeAndRoundCheck 헬퍼 사용.
        if (actor == null || actor.IsDead)
        {
            ConsumeAndRoundCheck();
            return;
        }

        if (actor is Player player)
        {
            firstActionDelayAvailable = false;
            Input?.RequestPlayerAction(player);
            State = CombatState.AwaitInput;
            return;
        }

        if (actor is TeamBase companion)
        {
            if (CompanionAi == null)
            {
                CompanionAi = new CompanionActionSelector();
            }
            ResolveActionWithOpeningDelay(CompanionAi.Pick(companion, AlliesT, EnemiesT));
            return;
        }

        if (actor is Monster monster)
        {
            if (MonsterAi == null)
            {
                MonsterAi = new MonsterActionSelector();
            }
            ResolveActionWithOpeningDelay(MonsterAi.Pick(monster, EnemiesT, AlliesT));
        }
    }

    private void ResolveActionWithOpeningDelay(CombatAction action)
    {
        if (firstActionDelayAvailable)
        {
            firstActionDelayAvailable = false;
            delayedAiAction = action;
            actingDelayTimer = ResolveFirstNonPlayerActionDelay();
            // P1(2026-06-05): 시작 직후 죽은 시간을 텔레그래프로 전환. 행위자 의도를 미리 노출.
            TelegraphAction(action);
            State = CombatState.ActingDelay;
            return;
        }

        ResolveAction(action);
    }

    private static float ResolveFirstNonPlayerActionDelay()
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Data?.Balance != null && gsm.Data.Balance.FirstNonPlayerActionDelaySec > 0f)
        {
            return gsm.Data.Balance.FirstNonPlayerActionDelaySec;
        }

        return DefaultFirstNonPlayerActionDelaySec;
    }

    /// <summary>
    /// 결정된 CombatAction 의 연출을 시작한다(Anticipation 진입).
    /// 결과(데미지/이펙트/방어)는 즉시 적용하지 않고 Impact 시점(ApplyImpact)으로 미룬다.
    /// </summary>
    private void ResolveAction(CombatAction action)
    {
        // 동작 요약:
        // - currentTiming = ActionTiming.ComputePhases(action).
        // - actionPhase = Anticipation, phaseTimer = 0.
        // - TelegraphAction(action): 행위자/타겟 준비 연출 노출.
        // - State = Resolving. 이후 Tick()의 Resolving 분기가 Impact/Recovery 를 진행.
        if (action == null || action.Actor == null)
        {
            ConsumeAndRoundCheck();
            return;
        }

        currentTiming = ActionTiming.ComputePhases(action);
        action.DurationSec = currentTiming.TotalSec;
        currentAction = action;
        actionPhase = CombatActionPhase.Anticipation;
        phaseTimer = 0f;

        TelegraphAction(action);
        State = CombatState.Resolving;
    }

    /// <summary>
    /// Resolving 진행. Anticipation 경과 시 Impact 적용 후 Recovery 로 전환,
    /// Recovery 경과 시 현재 행위자를 소비하고 라운드 검사로 넘어간다.
    /// </summary>
    private void TickResolving()
    {
        if (currentAction == null)
        {
            ConsumeAndRoundCheck();
            return;
        }

        phaseTimer += Time.deltaTime;

        if (actionPhase == CombatActionPhase.Anticipation)
        {
            if (phaseTimer >= currentTiming.AnticipationSec)
            {
                if (!ApplyImpact())
                {
                    return; // 전투 종료 확정 → State = Ended.
                }

                actionPhase = CombatActionPhase.Recovery;
                phaseTimer = 0f;
            }

            return;
        }

        if (phaseTimer >= currentTiming.RecoverySec)
        {
            ConsumeAndRoundCheck();
        }
    }

    /// <summary>
    /// Impact 시점 결과 적용. 데미지/이펙트/방어 상태를 이 시점에 적용한다.
    /// 승패 검사는 이 단일 위치에서만 수행한다(F.2). 전투 종료 시 false 반환.
    /// </summary>
    private bool ApplyImpact()
    {
        CombatAction action = currentAction;
        if (action == null || action.Actor == null)
        {
            ConsumeAndRoundCheck();
            return false;
        }

        switch (action.Type)
        {
            case CombatActionType.Attack:
                ApplyAttack(action);
                action.Actor.PlayAttackAnimation(action.Actor is Monster atkMon ? atkMon.AttackAnimIndex : 0);
                CombatEffectPresenter.Play(action);
                CombatSfxPresenter.Play(action);
                action.Actor.WorldUI?.ShowAttackIcon();
                break;
            case CombatActionType.Skill:
                ApplySkill(action);
                action.Actor.PlayAttackAnimation(0);
                CombatEffectPresenter.Play(action);
                CombatSfxPresenter.Play(action);
                action.Actor.WorldUI?.ShowAttackIcon();
                break;
            case CombatActionType.Defend:
                action.Actor.SetDefending(true);
                break;
        }

        // 텔레그래프로 켠 타겟 하이라이트를 Impact 시점에 해제.
        HighlightTargets(action, false);
        Queue?.RemoveDeadEntries();
        ScheduleDeadEnemyRemoval();
        // CheckOutcome 은 ApplyImpact 단일 위치(F.2).
        if (CheckOutcome().HasValue)
        {
            State = CombatState.Ended;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Anticipation 시작 시 행위자 의도를 노출한다(준비 연출).
    /// Attack/Skill: 행위자 공격 아이콘 + 타겟 하이라이트. Defend: 방어 아이콘.
    /// </summary>
    private void TelegraphAction(CombatAction action)
    {
        if (action?.Actor == null)
        {
            return;
        }

        switch (action.Type)
        {
            case CombatActionType.Attack:
            case CombatActionType.Skill:
                action.Actor.WorldUI?.ShowAttackIcon();
                HighlightTargets(action, true);
                break;
            case CombatActionType.Defend:
                action.Actor.WorldUI?.ShowDefendIcon();
                break;
        }
    }

    private static void HighlightTargets(CombatAction action, bool on)
    {
        if (action?.Targets == null)
        {
            return;
        }

        for (int i = 0; i < action.Targets.Count; i++)
        {
            EntityBase target = action.Targets[i];
            if (target == null || target.IsDead)
            {
                continue;
            }

            target.WorldUI?.SetTargetHighlight(on);
        }
    }

    private void ScheduleDeadEnemyRemoval()
    {
        if (EnemiesT == null)
        {
            return;
        }

        foreach (EntityBase enemy in EnemiesT)
        {
            if (enemy != null && enemy.IsDead)
            {
                enemy.WorldUI?.SetTargetHighlight(false);
                enemy.WorldUI?.HideActionIcon();
                DeadEnemyRemover?.ScheduleDeadEnemyRemoval(enemy, 0.5f);
            }
        }
    }

    // Wave0refactor 2026-05-27 (F.5): 반복 패턴 "Queue?.ConsumeCurrent(); State = RoundEndCheck;" 추출.
    private void ConsumeAndRoundCheck()
    {
        Queue?.ConsumeCurrent();
        State = CombatState.RoundEndCheck;
    }

    /// <summary>
    /// 승패 검사. 한쪽 전멸 시 결과 전달.
    /// </summary>
    public CombatResult? CheckOutcome()
    {
        // 동작 요약:
        // - 적 전멸 → Victory.
        // - 아군의 Player 사망 → Defeat (동료만 사망은 패배 아님).
        // - 결정되지 않으면 null.
        if (EnemiesT != null && EnemiesT.TrueForAll(e => e == null || e.IsDead))
        {
            return CombatResult.Victory;
        }

        if (AlliesT != null && AlliesT.Exists(a => a is Player && a.IsDead))
        {
            return CombatResult.Defeat;
        }

        return null;
    }

    private static void ApplyAttack(CombatAction action)
    {
        if (action.Targets == null) return;
        foreach (EntityBase target in action.Targets)
        {
            if (target == null || target.IsDead) continue;
            int damage = DamageCalculator.ComputeAttack(action.Actor, target);
            target.ApplyDamage(damage);
        }
    }

    // Wave0refactor 2026-05-27 (F.1): 한 스킬은 한 효과(Damage > Heal > Shield) 우선순위로 단일 적용.
    // 과거 코드는 if 셋이 독립이라 데이터가 셋 다 가지면 중첩 적용되는 잠재 버그가 있었다.
    private static void ApplySkill(CombatAction action)
    {
        SkillData data = action.ResolvedSkillData;
        if (data == null) return;

        if (data.DamageScale > 0f)
        {
            SkillEffect.ApplyDamage(action.Actor, action.Targets, data);
        }
        else if (data.HealScale > 0f)
        {
            SkillEffect.ApplyHeal(action.Actor, action.Targets, data);
        }
        else if (data.ShieldScale > 0f)
        {
            SkillEffect.ApplyShield(action.Actor, action.Targets, data);
        }

        action.Skill.ConsumeForUse(action.Actor, data);
    }
}

/// <summary>전투 FSM 상태.</summary>
public enum CombatState
{
    /// <summary>전투 시작 직전.</summary>
    Starting,

    /// <summary>라운드 큐 빌드 직전.</summary>
    RoundStart,

    /// <summary>라운드 종료 후 다음 라운드 빌드 전 짧은 전환 정지.</summary>
    RoundTransition,

    /// <summary>다음 행위자 가져오기.</summary>
    NextActor,

    /// <summary>플레이어 입력 대기.</summary>
    AwaitInput,

    /// <summary>전투 시작 직후 AI 선행 행동 연출 대기.</summary>
    ActingDelay,

    /// <summary>행동 실행/연출 대기.</summary>
    Resolving,

    /// <summary>라운드 종료 검사.</summary>
    RoundEndCheck,

    /// <summary>전투 종료.</summary>
    Ended,
}

/// <summary>액션 연출 페이즈.</summary>
public enum CombatActionPhase
{
    /// <summary>준비(결과 적용 전).</summary>
    Anticipation,

    /// <summary>복귀(결과 적용 후 여운).</summary>
    Recovery,
}

/// <summary>플레이어 입력 제공자.</summary>
public interface IPlayerInputProvider
{
    /// <summary>아직 확정되지 않은 행동을 가지고 있는가.</summary>
    bool HasAction { get; }

    /// <summary>플레이어 행동 결정을 시작.</summary>
    void RequestPlayerAction(EntityBase actor);

    /// <summary>확정된 행동을 꺼내고 큐 진행.</summary>
    CombatAction PopAction();
}

public interface IDeadEnemyRemover
{
    void ScheduleDeadEnemyRemoval(EntityBase enemy, float delaySec);
}

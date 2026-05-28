using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 단일 FSM. Player/Companion/Monster 별도 FSM 금지.
    /// 라운드 큐를 순회하며 행동을 결정/실행/대기한다.
    /// </summary>
    public sealed class CombatFlowt
    {
        /// <summary>현재 라운드 큐.</summary>
        public ActionQueuet Queue { get; private set; }

        /// <summary>플레이어 측 엔티티(Player + 동료 ≤3).</summary>
        public List<EntityBaset> AlliesT;

        /// <summary>적 엔티티(Monster 1~3).</summary>
        public List<EntityBaset> EnemiesT;

        /// <summary>현재 FSM 상태.</summary>
        public CombatStatet State { get; private set; }

        /// <summary>현재 라운드 번호.</summary>
        public int RoundNumber { get; private set; }

        /// <summary>플레이어 입력을 받기 위한 인터페이스.</summary>
        public IPlayerInputProvidert Input;

        /// <summary>타겟 선택 유틸.</summary>
        public TargetSelectort TargetSelector;

        /// <summary>몬스터 행동 선택기.</summary>
        public MonsterActionSelectort MonsterAi;

        /// <summary>동료 행동 선택기.</summary>
        public CompanionActionSelectort CompanionAi;

        // Resolving 상태에서의 경과 시간 추적
        private float elapsedTime;
        private CombatActiont currentAction;

        /// <summary>
        /// 전투 시작. CombatControllert가 호출.
        /// </summary>
        public void StartCombat(List<EntityBaset> allies, List<EntityBaset> enemies)
        {
            // 동작 요약:
            // - AlliesT = allies, EnemiesT = enemies.
            // - 각 엔티티 PrepareForCombat.
            // - Queue = new ActionQueuet().
            // - RoundNumber = 1.
            // - BeginRound().
            //TODO: AlliesT = allies;
            //TODO: EnemiesT = enemies;
            //TODO: foreach (EntityBaset entity in AlliesT) entity.PrepareForCombat();
            //TODO: foreach (EntityBaset entity in EnemiesT) entity.PrepareForCombat();
            //TODO: Queue = new ActionQueuet();
            //TODO: RoundNumber = 1;
            //TODO: BeginRound();
            AlliesT = allies ?? new List<EntityBaset>(); //Wave0write
            EnemiesT = enemies ?? new List<EntityBaset>(); //Wave0write
            foreach (EntityBaset entity in AlliesT) //Wave0write
            { //Wave0write
                entity?.PrepareForCombat(); //Wave0write
            } //Wave0write
            foreach (EntityBaset entity in EnemiesT) //Wave0write
            { //Wave0write
                entity?.PrepareForCombat(); //Wave0write
            } //Wave0write

            Queue = new ActionQueuet(); //Wave0write
            RoundNumber = 1; //Wave0write
            State = CombatStatet.Starting; //Wave0write
            BeginRound(); //Wave0write
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
            //   * Resolving  → elapsedTime += Time.deltaTime.
            //                  elapsedTime >= action.DurationSec이면:
            //                    - Queue.ConsumeCurrent().
            //                    - State = RoundEndCheck.
            //                  ※ 소모 아이템은 CombatControllert.PlayerUseItem()에서 즉시 처리하고
            //                    CombatFlowt 큐에 CombatAction으로 넣지 않는다.
            //   * RoundEndCheck → Queue.IsRoundFinished()이면 EndRound().
            //                     아니면 State = NextActor.
            //   * Ended → 결과는 CombatControllert.OnSceneUpdate가 CheckOutcome()으로 감지.
            //TODO: switch (State)
            //TODO: {
            //TODO:     case CombatStatet.Starting:
            //TODO:         State = CombatStatet.RoundStart;
            //TODO:         break;
            //TODO:     case CombatStatet.RoundStart:
            //TODO:         BeginRound();
            //TODO:         break;
            //TODO:     case CombatStatet.NextActor:
            //TODO:         TurnEntryt next = Queue.PeekNext();
            //TODO:         if (next == null) { State = CombatStatet.RoundEndCheck; break; }
            //TODO:         ResolveActor(next.Actor);
            //TODO:         break;
            //TODO:     case CombatStatet.AwaitInput:
            //TODO:         if (Input != null && Input.HasAction)
            //TODO:         {
            //TODO:             CombatActiont action = Input.PopAction();
            //TODO:             ResolveAction(action);
            //TODO:         }
            //TODO:         break;
            //TODO:     case CombatStatet.Resolving:
            //TODO:         elapsedTime += Time.deltaTime;
            //TODO:         if (currentAction != null && elapsedTime >= currentAction.DurationSec)
            //TODO:         {
            //TODO:             Queue.ConsumeCurrent();
            //TODO:             State = CombatStatet.RoundEndCheck;
            //TODO:         }
            //TODO:         break;
            //TODO:     case CombatStatet.RoundEndCheck:
            //TODO:         if (Queue.IsRoundFinished()) EndRound();
            //TODO:         else State = CombatStatet.NextActor;
            //TODO:         break;
            //TODO:     case CombatStatet.Ended:
            //TODO:         break;
            //TODO: }
            switch (State) //Wave0write
            { //Wave0write
                case CombatStatet.Starting: //Wave0write
                case CombatStatet.RoundStart: //Wave0write
                    BeginRound(); //Wave0write
                    break; //Wave0write
                case CombatStatet.NextActor: //Wave0write
                    TurnEntryt next = Queue?.PeekNext(); //Wave0write
                    if (next == null) //Wave0write
                    { //Wave0write
                        State = CombatStatet.RoundEndCheck; //Wave0write
                        break; //Wave0write
                    } //Wave0write
                    ResolveActor(next.Actor); //Wave0write
                    break; //Wave0write
                case CombatStatet.AwaitInput: //Wave0write
                    if (Input != null && Input.HasAction) //Wave0write
                    { //Wave0write
                        ResolveAction(Input.PopAction()); //Wave0write
                    } //Wave0write
                    break; //Wave0write
                case CombatStatet.Resolving: //Wave0write
                    elapsedTime += Time.deltaTime; //Wave0write
                    if (currentAction == null || elapsedTime >= currentAction.DurationSec) //Wave0write
                    { //Wave0write
                        Queue?.ConsumeCurrent(); //Wave0write
                        State = CombatStatet.RoundEndCheck; //Wave0write
                    } //Wave0write
                    break; //Wave0write
                case CombatStatet.RoundEndCheck: //Wave0write
                    CombatResultt? outcome = CheckOutcome(); //Wave0write
                    if (outcome.HasValue) //Wave0write
                    { //Wave0write
                        State = CombatStatet.Ended; //Wave0write
                    } //Wave0write
                    else if (Queue == null || Queue.IsRoundFinished()) //Wave0write
                    { //Wave0write
                        EndRound(); //Wave0write
                    } //Wave0write
                    else //Wave0write
                    { //Wave0write
                        State = CombatStatet.NextActor; //Wave0write
                    } //Wave0write
                    break; //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 새 라운드 준비.
        /// </summary>
        private void BeginRound()
        {
            // 동작 요약:
            // - Queue.BuildRound(AlliesT ∪ EnemiesT).
            // - 각 엔티티의 OnRoundStart 훅(쿨다운 감소 등).
            // - State = NextActor.
            //TODO: List<EntityBaset> all = new List<EntityBaset>(AlliesT);
            //TODO: all.AddRange(EnemiesT);
            //TODO: Queue.BuildRound(all);
            //TODO: foreach (EntityBaset entity in all)
            //TODO: {
            //TODO:     if (!entity.IsDead) entity.OnRoundStart();
            //TODO: }
            //TODO: State = CombatStatet.NextActor;
            var all = new List<EntityBaset>(); //Wave0write
            if (AlliesT != null) all.AddRange(AlliesT); //Wave0write
            if (EnemiesT != null) all.AddRange(EnemiesT); //Wave0write
            if (Queue == null) //Wave0write
            { //Wave0write
                Queue = new ActionQueuet(); //Wave0write
            } //Wave0write
            Queue.BuildRound(all); //Wave0write
            State = CombatStatet.NextActor; //Wave0write
        }

        /// <summary>
        /// 라운드 종료 처리.
        /// 승패 검사는 ResolveAction에서 이미 수행하므로 여기서는 중복 검사 없음.
        /// </summary>
        private void EndRound()
        {
            // 동작 요약:
            // - 각 엔티티.OnRoundEnd() (방어 상태 해제, WorldUI 아이콘 숨김).
            // - 살아있는 모든 엔티티의 ActiveSkills 쿨다운 1씩 감소.
            // - RoundNumber += 1.
            // - State = RoundStart (BeginRound는 다음 Tick에서 호출).
            //TODO: List<EntityBaset> all = new List<EntityBaset>(AlliesT);
            //TODO: all.AddRange(EnemiesT);
            //TODO: foreach (EntityBaset entity in all)
            //TODO: {
            //TODO:     entity.OnRoundEnd();
            //TODO:     if (!entity.IsDead && entity is CharacterBaset character)
            //TODO:     {
            //TODO:         foreach (Skillt skill in character.ActiveSkills)
            //TODO:         {
            //TODO:             if (skill != null) skill.TickCooldown();
            //TODO:         }
            //TODO:     }
            //TODO: }
            //TODO: RoundNumber += 1;
            //TODO: State = CombatStatet.RoundStart;
            var all = new List<EntityBaset>(); //Wave0write
            if (AlliesT != null) all.AddRange(AlliesT); //Wave0write
            if (EnemiesT != null) all.AddRange(EnemiesT); //Wave0write
            foreach (EntityBaset entity in all) //Wave0write
            { //Wave0write
                if (entity == null) continue; //Wave0write
                entity.OnRoundEnd(); //Wave0write
                if (entity.ActiveSkills == null) continue; //Wave0write
                foreach (Skillt skill in entity.ActiveSkills) //Wave0write
                { //Wave0write
                    skill?.TickCooldown(); //Wave0write
                } //Wave0write
            } //Wave0write

            RoundNumber += 1; //Wave0write
            State = CombatStatet.RoundStart; //Wave0write
        }

        /// <summary>
        /// 현재 행위자의 행동을 받아온다(Player → 입력 대기, AI → 즉시 결정).
        /// </summary>
        private void ResolveActor(EntityBaset actor)
        {
            // 동작 요약:
            // - actor가 Playert면 Input.RequestPlayerAction(actor) 호출 → State = AwaitInput.
            // - actor가 TeamBaset이면 CompanionAi.Pick() → ResolveAction.
            // - actor가 MonsterBaset이면 MonsterAi.Pick() → ResolveAction.
            //TODO: if (actor is Playert player)
            //TODO: {
            //TODO:     Input.RequestPlayerAction(player);
            //TODO:     State = CombatStatet.AwaitInput;
            //TODO: }
            //TODO: else if (actor is TeamBaset companion)
            //TODO: {
            //TODO:     CombatActiont companionAction = CompanionAi.Pick(companion, AlliesT, EnemiesT);
            //TODO:     ResolveAction(companionAction);
            //TODO: }
            //TODO: else if (actor is MonsterBaset monster)
            //TODO: {
            //TODO:     CombatActiont monsterAction = MonsterAi.Pick(monster, EnemiesT, AlliesT);
            //TODO:     ResolveAction(monsterAction);
            //TODO: }
            if (actor == null || actor.IsDead) //Wave0write
            { //Wave0write
                Queue?.ConsumeCurrent(); //Wave0write
                State = CombatStatet.RoundEndCheck; //Wave0write
                return; //Wave0write
            } //Wave0write

            if (actor is Playert player) //Wave0write
            { //Wave0write
                Input?.RequestPlayerAction(player); //Wave0write
                State = CombatStatet.AwaitInput; //Wave0write
                return; //Wave0write
            } //Wave0write

            if (actor is TeamBaset companion) //Wave0write
            { //Wave0write
                if (CompanionAi == null) //Wave0write
                { //Wave0write
                    CompanionAi = new CompanionActionSelectort(); //Wave0write
                } //Wave0write
                ResolveAction(CompanionAi.Pick(companion, AlliesT, EnemiesT)); //Wave0write
                return; //Wave0write
            } //Wave0write

            if (actor is MonsterBaset monster) //Wave0write
            { //Wave0write
                if (MonsterAi == null) //Wave0write
                { //Wave0write
                    MonsterAi = new MonsterActionSelectort(); //Wave0write
                } //Wave0write
                ResolveAction(MonsterAi.Pick(monster, EnemiesT, AlliesT)); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 결정된 CombatActiont를 실행하고 State = Resolving으로 전환한다.
        /// 실제 대기(DurationSec 카운트다운)는 Tick()의 Resolving 분기에서 처리.
        /// </summary>
        private void ResolveAction(CombatActiont action)
        {
            // 동작 요약:
            // - action.DurationSec = ActionTimingt.Compute(action) (0.1s 최소 + 애니/이펙트 합산).
            // - 타입별 즉시 효과 적용:
            //   * Attack → DamageCalculatort.ComputeAttack(actor, target) + actor.WorldUI 빨강 아이콘.
            //   * Skill  → SkillEffectt.Apply*(actor, targets, skill.Data) + actor.WorldUI 빨강 아이콘.
            //              skill.ConsumeForUse(actor) 호출(MP 차감, 쿨다운 시작).
            //   * Defend → actor.SetDefending(true) + 파랑 아이콘.
            //   * Item   → 이 경로에서 처리하지 않음.
            //              소모 아이템은 CombatControllert.PlayerUseItem()가 즉시 처리하며
            //              pending main action과 라운드 큐를 건드리지 않는다.
            // - Queue.RemoveDeadEntries() 호출(사망자 즉시 제거).
            // - CheckOutcome() 호출: 승패 확정 시 State = Ended, 아니면 State = Resolving.
            // ※ 승패 검사는 여기서만 수행. EndRound에서는 중복 검사하지 않는다.
            //TODO: action.DurationSec = ActionTimingt.Compute(action);
            //TODO: currentAction = action;
            //TODO: elapsedTime = 0f;
            //TODO: switch (action.Type)
            //TODO: {
            //TODO:     case CombatActionTypet.Attack:
            //TODO:         foreach (EntityBaset target in action.Targets)
            //TODO:         {
            //TODO:             int dmg = DamageCalculatort.ComputeAttack(action.Actor, target);
            //TODO:             target.ApplyDamage(dmg);
            //TODO:         }
            //TODO:         action.Actor.WorldUI.ShowActionIcon(ActionIconTypet.Attack);
            //TODO:         break;
            //TODO:     case CombatActionTypet.Skill:
            //TODO:         SkillDatat skillData = action.Skill.Data;
            //TODO:         if (skillData.DamageScale > 0f)
            //TODO:             SkillEffectt.ApplyDamage(action.Actor, action.Targets, skillData);
            //TODO:         else if (skillData.HealScale > 0f)
            //TODO:             SkillEffectt.ApplyHeal(action.Actor, action.Targets, skillData);
            //TODO:         else if (skillData.ShieldScale > 0f)
            //TODO:             SkillEffectt.ApplyShield(action.Actor, action.Targets, skillData);
            //TODO:         action.Skill.ConsumeForUse(action.Actor);
            //TODO:         action.Actor.WorldUI.ShowActionIcon(ActionIconTypet.Skill);
            //TODO:         break;
            //TODO:     case CombatActionTypet.Defend:
            //TODO:         action.Actor.SetDefending(true);
            //TODO:         action.Actor.WorldUI.ShowActionIcon(ActionIconTypet.Defend);
            //TODO:         break;
            //TODO:     case CombatActionTypet.Item:
            //TODO:         return; // 아이템은 CombatControllert 직접 처리
            //TODO: }
            //TODO: Queue.RemoveDeadEntries();
            //TODO: CombatResultt? outcome = CheckOutcome();
            //TODO: if (outcome.HasValue)
            //TODO:     State = CombatStatet.Ended;
            //TODO: else
            //TODO:     State = CombatStatet.Resolving;
            if (action == null || action.Actor == null) //Wave0write
            { //Wave0write
                Queue?.ConsumeCurrent(); //Wave0write
                State = CombatStatet.RoundEndCheck; //Wave0write
                return; //Wave0write
            } //Wave0write

            action.DurationSec = ActionTimingt.Compute(action); //Wave0write
            currentAction = action; //Wave0write
            elapsedTime = 0f; //Wave0write
            switch (action.Type) //Wave0write
            { //Wave0write
                case CombatActionTypet.Attack: //Wave0write
                    ApplyAttack(action); //Wave0write
                    action.Actor.WorldUI?.ShowAttackIcon(); //Wave0write
                    break; //Wave0write
                case CombatActionTypet.Skill: //Wave0write
                    ApplySkill(action); //Wave0write
                    action.Actor.WorldUI?.ShowAttackIcon(); //Wave0write
                    break; //Wave0write
                case CombatActionTypet.Defend: //Wave0write
                    action.Actor.SetDefending(true); //Wave0write
                    break; //Wave0write
                case CombatActionTypet.Item: //Wave0write
                    Queue?.ConsumeCurrent(); //Wave0write
                    State = CombatStatet.RoundEndCheck; //Wave0write
                    return; //Wave0write
            } //Wave0write

            Queue?.RemoveDeadEntries(); //Wave0write
            if (CheckOutcome().HasValue) //Wave0write
            { //Wave0write
                State = CombatStatet.Ended; //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                State = CombatStatet.Resolving; //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 승패 검사. 한쪽 전멸 시 결과 전달.
        /// </summary>
        public CombatResultt? CheckOutcome()
        {
            // 동작 요약:
            // - 적 전멸 → Victory.
            // - 아군의 Player 사망 → Defeat (동료만 사망은 패배 아님).
            // - 결정되지 않으면 null.
            //TODO: bool allEnemiesDead = EnemiesT.TrueForAll(e => e.IsDead);
            //TODO: if (allEnemiesDead) return CombatResultt.Victory;
            //TODO: bool playerDead = AlliesT.Exists(a => a is Playert && a.IsDead);
            //TODO: if (playerDead) return CombatResultt.Defeat;
            //TODO: return null;
            if (EnemiesT != null && EnemiesT.TrueForAll(e => e == null || e.IsDead)) //Wave0write
            { //Wave0write
                return CombatResultt.Victory; //Wave0write
            } //Wave0write

            if (AlliesT != null && AlliesT.Exists(a => a is Playert && a.IsDead)) //Wave0write
            { //Wave0write
                return CombatResultt.Defeat; //Wave0write
            } //Wave0write

            return null; //Wave0write
        }

        private static void ApplyAttack(CombatActiont action) //Wave0write
        { //Wave0write
            if (action.Targets == null) return; //Wave0write
            foreach (EntityBaset target in action.Targets) //Wave0write
            { //Wave0write
                if (target == null || target.IsDead) continue; //Wave0write
                int damage = DamageCalculatort.ComputeAttack(action.Actor, target); //Wave0write
                target.ApplyDamage(damage); //Wave0write
            } //Wave0write
        } //Wave0write

        private static void ApplySkill(CombatActiont action) //Wave0write
        { //Wave0write
            SkillDatat data = action.Skill?.Data; //Wave0write
            if (data == null) return; //Wave0write
            if (data.DamageScale > 0f) SkillEffectt.ApplyDamage(action.Actor, action.Targets, data); //Wave0write
            if (data.HealScale > 0f) SkillEffectt.ApplyHeal(action.Actor, action.Targets, data); //Wave0write
            if (data.ShieldScale > 0f) SkillEffectt.ApplyShield(action.Actor, action.Targets, data); //Wave0write
            action.Skill.ConsumeForUse(action.Actor); //Wave0write
        } //Wave0write
    }

    /// <summary>전투 FSM 상태.</summary>
    public enum CombatStatet
    {
        /// <summary>전투 시작 직전.</summary>
        Starting,

        /// <summary>라운드 큐 빌드 직전.</summary>
        RoundStart,

        /// <summary>다음 행위자 가져오기.</summary>
        NextActor,

        /// <summary>플레이어 입력 대기.</summary>
        AwaitInput,

        /// <summary>행동 실행/연출 대기.</summary>
        Resolving,

        /// <summary>라운드 종료 검사.</summary>
        RoundEndCheck,

        /// <summary>전투 종료.</summary>
        Ended,
    }

    /// <summary>플레이어 입력 제공자.</summary>
    public interface IPlayerInputProvidert
    {
        /// <summary>아직 확정되지 않은 행동을 가지고 있는가.</summary>
        bool HasAction { get; }

        /// <summary>플레이어 행동 결정을 시작.</summary>
        void RequestPlayerAction(EntityBaset actor);

        /// <summary>확정된 행동을 꺼내고 큐 진행.</summary>
        CombatActiont PopAction();
    }
}

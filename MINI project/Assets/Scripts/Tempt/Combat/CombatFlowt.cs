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
        }

        /// <summary>
        /// 매 프레임 호출. 코루틴/await 대신 단순 펌프 방식.
        /// </summary>
        public void Tick()
        {
            // 동작 요약:
            // - State 분기:
            //   * RoundStart → BeginRound() 처리 후 NextActor로.
            //   * NextActor → 큐에서 다음 행위자 가져와 ResolveActor로.
            //   * AwaitInput → Input.HasAction이면 ResolveAction으로.
            //   * Resolving → DurationSec 카운트다운 후 RoundEndCheck.
            //   * RoundEndCheck → 큐 비었으면 EndRound, 아니면 NextActor.
            //   * Ended → 외부에 결과 통보.
            // - 사망/승패 매 시점 검사.
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
        }

        /// <summary>
        /// 라운드 종료 처리.
        /// </summary>
        private void EndRound()
        {
            // 동작 요약:
            // - 각 엔티티.OnRoundEnd().
            // - 스킬 쿨다운 1씩 감소.
            // - 방어 상태 해제.
            // - 승패 검사.
            // - 종료 아니면 RoundNumber += 1, BeginRound 재진입.
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
        }

        /// <summary>
        /// 결정된 CombatActiont를 실행한다.
        /// </summary>
        private void ResolveAction(CombatActiont action)
        {
            // 동작 요약:
            // - 타입별 분기:
            //   * Attack → DamageCalculatort + actor.WorldUI 빨강 아이콘.
            //   * Skill → SkillEffectt.Apply* + actor.WorldUI 빨강 아이콘.
            //   * Defend → actor.SetDefending(true) + 파랑 아이콘.
            //   * Item → ConsumableSlotst.TryUse, action.ConsumesTurn=false → 큐 진행 안 함 + Input 다시 받기.
            // - DurationSec(ActionTimingt 합산) 동안 대기 후 ConsumeCurrent.
            // - 사망 엔티티 제거(Queue.RemoveDeadEntries).
            // - 승패 검사.
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
            return null;
        }
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

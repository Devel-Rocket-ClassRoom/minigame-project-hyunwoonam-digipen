using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 씬 컨트롤러. CombatFlowt를 보유하고, 몬스터 스포너/입력 공급자/타겟 셀렉터를 조립한다.
    /// 단축키 중 소비 4칸 변경은 이 컨트롤러에서 차단(view-only 모드).
    /// </summary>
    public sealed class CombatControllert : SceneControllerBaset, IPlayerInputProvidert
    {
        /// <summary>전투 FSM.</summary>
        public CombatFlowt Flow;

        /// <summary>몬스터 스포너.</summary>
        public CombatMonsterSpawnert Spawner;

        /// <summary>타겟 셀렉터(레이캐스트).</summary>
        public TargetSelectort Targeter;

        /// <summary>HUD 페이지(전투 전용 UI).</summary>
        public CombatHudt Hud;

        /// <summary>플레이어 본체.</summary>
        public Playert Player;

        /// <summary>참여 동료(최대 3).</summary>
        public List<TeamBaset> Companions;

        /// <summary>참여 몬스터.</summary>
        public List<MonsterBaset> Monsters;

        /// <summary>플레이어 임시 행동(입력 진행 중).</summary>
        private CombatActiont pendingAction;

        /// <inheritdoc/>
        public override void OnEnter()
        {
            // 동작 요약:
            // - GameSystemManagert.Instance.CombatContext 읽기.
            // - Spawner.SpawnFromNode(node, erosionMultiplier).
            // - Flow = new CombatFlowt(); Flow.Input = this; Flow.TargetSelector = Targeter; Flow.MonsterAi=...; Flow.CompanionAi=...
            // - Flow.StartCombat(allies, enemies).
            // - UIManagert.SetConsumablesEditable(false) → 4칸 변경 차단.
            // - Hud.Show().
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            // 동작 요약:
            // - UIManagert.SetConsumablesEditable(true) 복원.
            // - Hud.Hide().
            // - Spawner.Cleanup().
        }

        /// <inheritdoc/>
        public override void OnSceneUpdate()
        {
            // 동작 요약:
            // - Flow.Tick() 호출.
            // - 종료(Ended)면 결과 GameSystemManagert.EndCombat 호출 + EXP 합산 지급.
        }

        /// <inheritdoc/>
        public bool HasAction => pendingAction != null && IsActionConfirmed();

        /// <inheritdoc/>
        public void RequestPlayerAction(EntityBaset actor)
        {
            // 동작 요약:
            // - pendingAction = null.
            // - Hud.ShowPlayerActionPanel(actor).
            // - 사용자 입력 흐름:
            //   * Attack 버튼 → PlayerPickAttack().
            //   * Skill1/2 버튼 → PlayerPickSkill(slotIndex).
            //   * Defend 버튼 → PlayerPickDefend().
            //   * Item 슬롯 → PlayerUseItem(slotIndex) — 확정 즉시 실행 + 메인 행동 미확정 시 재입력.
            //   * 메인 행동을 골랐어도 타겟 미확정이면 다른 메인 행동으로 변경 가능.
        }

        /// <inheritdoc/>
        public CombatActiont PopAction()
        {
            // 동작 요약:
            // - 임시 저장된 pendingAction 반환 후 null로 비움.
            // - Hud.HidePlayerActionPanel().
            return null;
        }

        /// <summary>공격 버튼.</summary>
        public void PlayerPickAttack()
        {
            // 동작 요약:
            // - pendingAction = {Attack, Target 미정}.
            // - TargetSelector.BeginHover(EnemySingle).
        }

        /// <summary>스킬 버튼.</summary>
        public void PlayerPickSkill(int slotIndex)
        {
            // 동작 요약:
            // - skill = Player.GetActiveSkill(slotIndex).
            // - skill.CanUse 검사(MP/쿨다운).
            // - TargetType에 따라:
            //   * EnemySingle/AllySingle → 호버 시작.
            //   * Self → 즉시 타겟 확정.
            //   * AllAll → 즉시 타겟 확정(전원).
            // - pendingAction에 저장.
        }

        /// <summary>방어 버튼.</summary>
        public void PlayerPickDefend()
        {
            // 동작 요약: pendingAction = {Defend, Targets=[Player]}. 타겟팅 불필요.
        }

        /// <summary>소모 아이템 슬롯 클릭.</summary>
        public void PlayerUseItem(int slotIndex)
        {
            // 동작 요약:
            // - 행동 비용 0(설계변경).
            // - ConsumableSlotst.TryUse 직접 호출.
            // - 사용 후 메인 행동은 그대로 유지 또는 미정 상태 유지.
        }

        /// <summary>타겟 호버 후 클릭 시 호출.</summary>
        public void OnTargetConfirmed(EntityBaset target)
        {
            // 동작 요약:
            // - pendingAction.Targets = [target].
            // - 행동이 확정됐다고 표시(Flow가 PopAction으로 회수).
        }

        /// <summary>행동이 완전히 확정됐는지(타겟 포함).</summary>
        private bool IsActionConfirmed()
        {
            // 동작 요약:
            // - Type별로 타겟 필요 여부 검사.
            // - Defend/AllAll/Self는 항상 확정.
            // - EnemySingle/AllySingle은 Targets.Count==1이면 확정.
            return false;
        }
    }
}

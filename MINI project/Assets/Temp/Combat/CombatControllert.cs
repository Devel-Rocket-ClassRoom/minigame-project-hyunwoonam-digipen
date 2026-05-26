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

        /// <summary>이미 EndCombat을 호출했는지 방어 플래그.</summary>
        private bool combatEnded;

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
            //TODO: var ctx = GameSystemManagert.Instance.CombatContext;
            //TODO: Spawner.SpawnFromNode(ctx.Node, ctx.ErosionMultiplier);
            //TODO: Monsters = Spawner.SpawnedT;
            //TODO: Player = GameSystemManagert.Instance.CurrentRun.Player.Entity as Playert;
            //TODO: Companions = GameSystemManagert.Instance.CurrentRun.Roster.Active.ConvertAll(c => c.Entity as TeamBaset);
            //TODO: Flow = new CombatFlowt();
            //TODO: Flow.Input = this;
            //TODO: Flow.TargetSelector = Targeter;
            //TODO: Flow.MonsterAi = new MonsterActionSelectort();
            //TODO: Flow.CompanionAi = new CompanionActionSelectort();
            //TODO: var allies = new List<EntityBaset>(); allies.Add(Player); allies.AddRange(Companions);
            //TODO: var enemies = new List<EntityBaset>(Monsters);
            //TODO: Flow.StartCombat(allies, enemies);
            //TODO: combatEnded = false;
            //TODO: UIManagert.Instance.SetConsumablesEditable(false);
            //TODO: Hud.OnOpen();
            //TODO: Targeter.OnTargetConfirmed += OnTargetConfirmed;
            GameSystemManagert gsm = GameSystemManagert.Instance; //Wave0write
            CombatContextt ctx = gsm.CombatContext; //Wave0write
            if (ctx == null || gsm.CurrentRun == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            Spawner = Spawner != null ? Spawner : GetComponent<CombatMonsterSpawnert>(); //Wave0write
            if (Spawner == null) //Wave0write
            { //Wave0write
                Spawner = gameObject.AddComponent<CombatMonsterSpawnert>(); //Wave0write
            } //Wave0write

            Targeter = Targeter ?? new TargetSelectort(); //Wave0write
            Player = Player != null ? Player : FindAnyObjectByType<Playert>(); //Wave0write
            if (Player == null) //Wave0write
            { //Wave0write
                Player = gameObject.AddComponent<Playert>(); //Wave0write
            } //Wave0write

            Player.BindState(gsm.CurrentRun.Player); //Wave0write
            Spawner.SpawnFromNode(ctx.Node, ctx.ErosionMultiplier); //Wave0write
            Monsters = Spawner.SpawnedT; //Wave0write
            Companions = new List<TeamBaset>(); //Wave0write

            Flow = new CombatFlowt //Wave0write
            { //Wave0write
                Input = this, //Wave0write
                TargetSelector = Targeter, //Wave0write
                MonsterAi = new MonsterActionSelectort(), //Wave0write
                CompanionAi = new CompanionActionSelectort(), //Wave0write
            }; //Wave0write

            var allies = new List<EntityBaset> { Player }; //Wave0write
            foreach (TeamBaset companion in Companions) //Wave0write
            { //Wave0write
                allies.Add(companion); //Wave0write
            } //Wave0write

            var enemies = new List<EntityBaset>(); //Wave0write
            foreach (MonsterBaset monster in Monsters) //Wave0write
            { //Wave0write
                enemies.Add(monster); //Wave0write
            } //Wave0write

            Flow.StartCombat(allies, enemies); //Wave0write
            combatEnded = false; //Wave0write
            pendingAction = null; //Wave0write
            Hud?.OnOpen(); //Wave0write
            Targeter.OnTargetConfirmed += OnTargetConfirmed; //Wave0write
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            // 동작 요약:
            // - UIManagert.SetConsumablesEditable(true) 복원.
            // - Hud.Hide().
            // - Spawner.Cleanup().
            //TODO: UIManagert.Instance.SetConsumablesEditable(true);
            //TODO: Hud.OnClose();
            //TODO: Spawner.Cleanup();
            //TODO: Targeter.OnTargetConfirmed -= OnTargetConfirmed;
            if (GameSystemManagert.TryGetInstance(out GameSystemManagert gsm) && Player != null) //Wave0write
            { //Wave0write
                Player.CopyToState(gsm.CurrentRun?.Player); //Wave0write
            } //Wave0write

            Hud?.OnClose(); //Wave0write
            Spawner?.Cleanup(); //Wave0write
            if (Targeter != null) //Wave0write
            { //Wave0write
                Targeter.OnTargetConfirmed -= OnTargetConfirmed; //Wave0write
            } //Wave0write
        }

        /// <inheritdoc/>
        public override void OnSceneUpdate()
        {
            // 동작 요약:
            // - Flow.Tick() 호출.
            // - Flow.State == Ended이면:
            //     * CombatResultt result = Flow.CheckOutcome() ?? CombatResultt.Defeat.
            //     * GameSystemManagert.Instance.EndCombat(result, this) 호출.
            //       (이 컨트롤러 자신을 넘겨 CollectNodeRewards() 접근 허용)
            //TODO: Flow.Tick();
            //TODO: if (Flow.State == CombatStatet.Ended && !combatEnded)
            //TODO: {
            //TODO:     combatEnded = true;
            //TODO:     CombatResultt result = Flow.CheckOutcome() ?? CombatResultt.Defeat;
            //TODO:     GameSystemManagert.Instance.EndCombat(result, this);
            //TODO: }
            Targeter?.Tick(); //Wave0write
            Flow?.Tick(); //Wave0write
            if (Flow != null && Flow.State == CombatStatet.Ended && !combatEnded) //Wave0write
            { //Wave0write
                combatEnded = true; //Wave0write
                CombatResultt result = Flow.CheckOutcome() ?? CombatResultt.Defeat; //Wave0write
                GameSystemManagert.Instance.EndCombat(result, this); //Wave0write
            } //Wave0write
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
            //   * Item 슬롯 → PlayerUseItem(slotIndex) — CombatFlowt에 넣지 않고 즉시 실행.
            //     아이템은 턴을 소비하지 않으며 pendingAction을 유지한다.
            //   * 메인 행동을 골랐어도 타겟 미확정이면 다른 메인 행동으로 변경 가능.
            //TODO: pendingAction = null;
            //TODO: Hud.ShowPlayerActionPanel(actor);
            // 버튼 바인딩은 Hud.OnOpen()에서 등록된 상태이므로 여기서는 패널만 표시
            pendingAction = null; //Wave0write
            Hud?.ShowPlayerActionPanel(actor); //Wave0write
            if (Hud == null) //Wave0write
            { //Wave0write
                PlayerPickAttack(); //Wave0write
            } //Wave0write
        }

        /// <inheritdoc/>
        public CombatActiont PopAction()
        {
            // 동작 요약:
            // - 임시 저장된 pendingAction 반환 후 null로 비움.
            // - Hud.HidePlayerActionPanel().
            //TODO: var action = pendingAction;
            //TODO: pendingAction = null;
            //TODO: Hud.HidePlayerActionPanel();
            //TODO: return action;
            CombatActiont action = pendingAction; //Wave0write
            pendingAction = null; //Wave0write
            Hud?.HidePlayerActionPanel(); //Wave0write
            return action; //Wave0write
        }

        /// <summary>공격 버튼.</summary>
        public void PlayerPickAttack()
        {
            // 동작 요약:
            // - pendingAction = {Attack, Target 미정}.
            // - TargetSelector.BeginHover(EnemySingle).
            //TODO: pendingAction = new CombatActiont { Actor = Player, Type = CombatActionTypet.Attack, ConsumesTurn = true };
            //TODO: pendingAction.Targets = new List<EntityBaset>();
            //TODO: Targeter.BeginHover(SkillTargetTypet.EnemySingle);
            pendingAction = new CombatActiont { Actor = Player, Type = CombatActionTypet.Attack, Targets = new List<EntityBaset>(), ConsumesTurn = true }; //Wave0write
            if (Targeter != null && Hud != null) //Wave0write
            { //Wave0write
                Targeter.BeginHover(SkillTargetTypet.EnemySingle); //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                AddIfNotNull(pendingAction.Targets, FirstAlive(Flow?.EnemiesT)); //Wave0write
            } //Wave0write
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
            //TODO: var skill = Player.ActiveSkills[slotIndex];
            //TODO: if (skill == null || !skill.CanUse(Player)) return;
            //TODO: pendingAction = new CombatActiont { Actor = Player, Type = CombatActionTypet.Skill, Skill = skill, ConsumesTurn = true };
            //TODO: pendingAction.Targets = new List<EntityBaset>();
            //TODO: switch (skill.Data.TargetType)
            //TODO: {
            //TODO:     case SkillTargetTypet.EnemySingle: Targeter.BeginHover(SkillTargetTypet.EnemySingle); break;
            //TODO:     case SkillTargetTypet.AllySingle:  Targeter.BeginHover(SkillTargetTypet.AllySingle); break;
            //TODO:     case SkillTargetTypet.Self:        pendingAction.Targets.Add(Player); break;
            //TODO:     case SkillTargetTypet.EnemyAll:    pendingAction.Targets.AddRange(Flow.EnemiesT); break;
            //TODO:     case SkillTargetTypet.AllyAll:     pendingAction.Targets.AddRange(Flow.AlliesT); break;
            //TODO: }
            Skillt skill = Player != null ? Player.GetActiveSkill(slotIndex) : null; //Wave0write
            if (skill == null || !skill.CanUse(Player)) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            pendingAction = new CombatActiont { Actor = Player, Type = CombatActionTypet.Skill, Skill = skill, Targets = new List<EntityBaset>(), ConsumesTurn = true }; //Wave0write
            switch (skill.Data.TargetType) //Wave0write
            { //Wave0write
                case SkillTargetTypet.EnemySingle: //Wave0write
                    if (Targeter != null && Hud != null) Targeter.BeginHover(SkillTargetTypet.EnemySingle); //Wave0write
                    else AddIfNotNull(pendingAction.Targets, FirstAlive(Flow?.EnemiesT)); //Wave0write
                    break; //Wave0write
                case SkillTargetTypet.AllySingle: //Wave0write
                    if (Targeter != null && Hud != null) Targeter.BeginHover(SkillTargetTypet.AllySingle); //Wave0write
                    else AddIfNotNull(pendingAction.Targets, FirstAlive(Flow?.AlliesT)); //Wave0write
                    break; //Wave0write
                case SkillTargetTypet.Self: //Wave0write
                    pendingAction.Targets.Add(Player); //Wave0write
                    break; //Wave0write
                case SkillTargetTypet.EnemyAll: //Wave0write
                    pendingAction.Targets.AddRange(AliveList(Flow?.EnemiesT)); //Wave0write
                    break; //Wave0write
                case SkillTargetTypet.AllyAll: //Wave0write
                    pendingAction.Targets.AddRange(AliveList(Flow?.AlliesT)); //Wave0write
                    break; //Wave0write
            } //Wave0write
        }

        /// <summary>방어 버튼.</summary>
        public void PlayerPickDefend()
        {
            // 동작 요약: pendingAction = {Defend, Targets=[Player]}. 타겟팅 불필요.
            //TODO: pendingAction = new CombatActiont { Actor = Player, Type = CombatActionTypet.Defend, ConsumesTurn = true };
            //TODO: pendingAction.Targets = new List<EntityBaset> { Player };
            pendingAction = new CombatActiont { Actor = Player, Type = CombatActionTypet.Defend, Targets = new List<EntityBaset> { Player }, ConsumesTurn = true }; //Wave0write
        }

        /// <summary>소모 아이템 슬롯 클릭.</summary>
        public void PlayerUseItem(int slotIndex)
        {
            // 동작 요약:
            // - 행동 비용 0(설계변경).
            // - ConsumableSlotst.TryUse 직접 호출.
            // - CombatActionTypet.Item을 만들지 않는다.
            // - 사용 후 메인 행동 pendingAction은 그대로 유지 또는 미정 상태 유지.
            //TODO: Player.Consumables.TryUse(slotIndex, Player, Player.Inventory);
            // 메인 행동 pendingAction은 변경하지 않음(아이템은 턴 미소모)
            Player?.Consumables?.TryUse(slotIndex, Player, Player.Inventory); //Wave0write
        }

        /// <summary>타겟 호버 후 클릭 시 호출.</summary>
        public void OnTargetConfirmed(EntityBaset target)
        {
            // 동작 요약:
            // - pendingAction.Targets = [target].
            // - 행동이 확정됐다고 표시(Flow가 PopAction으로 회수).
            //TODO: if (pendingAction == null) return;
            //TODO: if (pendingAction.Targets == null) pendingAction.Targets = new List<EntityBaset>();
            //TODO: pendingAction.Targets.Clear();
            //TODO: pendingAction.Targets.Add(target);
            //TODO: Targeter.EndHover();
            if (pendingAction == null || target == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (pendingAction.Targets == null) //Wave0write
            { //Wave0write
                pendingAction.Targets = new List<EntityBaset>(); //Wave0write
            } //Wave0write

            pendingAction.Targets.Clear(); //Wave0write
            pendingAction.Targets.Add(target); //Wave0write
            Targeter?.EndHover(); //Wave0write
        }

        /// <summary>행동이 완전히 확정됐는지(타겟 포함).</summary>
        private bool IsActionConfirmed()
        {
            // 동작 요약:
            // - Type별로 타겟 필요 여부 검사.
            // - Defend/AllAll/Self는 항상 확정.
            // - EnemySingle/AllySingle은 Targets.Count==1이면 확정.
            //TODO: if (pendingAction == null) return false;
            //TODO: switch (pendingAction.Type)
            //TODO: {
            //TODO:     case CombatActionTypet.Defend: return true;
            //TODO:     case CombatActionTypet.Attack: return pendingAction.Targets != null && pendingAction.Targets.Count == 1;
            //TODO:     case CombatActionTypet.Skill:
            //TODO:         if (pendingAction.Skill == null) return false;
            //TODO:         var tt = pendingAction.Skill.Data.TargetType;
            //TODO:         if (tt == SkillTargetTypet.Self || tt == SkillTargetTypet.EnemyAll || tt == SkillTargetTypet.AllyAll) return true;
            //TODO:         return pendingAction.Targets != null && pendingAction.Targets.Count == 1;
            //TODO:     default: return false;
            //TODO: }
            if (pendingAction == null) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (pendingAction.Type == CombatActionTypet.Defend) //Wave0write
            { //Wave0write
                return true; //Wave0write
            } //Wave0write

            if (pendingAction.Type == CombatActionTypet.Attack) //Wave0write
            { //Wave0write
                return pendingAction.Targets != null && pendingAction.Targets.Count > 0; //Wave0write
            } //Wave0write

            if (pendingAction.Type == CombatActionTypet.Skill && pendingAction.Skill?.Data != null) //Wave0write
            { //Wave0write
                SkillTargetTypet targetType = pendingAction.Skill.Data.TargetType; //Wave0write
                if (targetType == SkillTargetTypet.Self || targetType == SkillTargetTypet.EnemyAll || targetType == SkillTargetTypet.AllyAll) //Wave0write
                { //Wave0write
                    return true; //Wave0write
                } //Wave0write

                return pendingAction.Targets != null && pendingAction.Targets.Count > 0; //Wave0write
            } //Wave0write

            return false; //Wave0write
        }

        /// <summary>
        /// 노드 전투에서 발생한 모든 보상을 수집 및 집계한다.
        /// Flow.State == Ended(Victory) 시 OnSceneUpdate에서 호출.
        /// </summary>
        public NodeRewardSummaryt CollectNodeRewards()
        {
            // 동작 요약:
            // - contributions = new List<NodeRewardContributiont>().
            // - Spawner.SpawnedT 순회 → 각 monster.GetRewardContribution() 호출 후 리스트에 추가.
            //   (사망 여부 무관하게 SpawnedT에 있는 모든 몬스터 포함 — 이미 사망해야 Victory이므로 안전)
            // - NodeRewardSummaryt.Aggregate(contributions) 결과 반환.
            //TODO: var contributions = new List<NodeRewardContributiont>();
            //TODO: foreach (var monster in Spawner.SpawnedT)
            //TODO:     contributions.Add(monster.GetRewardContribution());
            //TODO: return NodeRewardSummaryt.Aggregate(contributions);
            var contributions = new List<NodeRewardContributiont>(); //Wave0write
            if (Spawner?.SpawnedT != null) //Wave0write
            { //Wave0write
                foreach (MonsterBaset monster in Spawner.SpawnedT) //Wave0write
                { //Wave0write
                    if (monster != null) //Wave0write
                    { //Wave0write
                        contributions.Add(monster.GetRewardContribution()); //Wave0write
                    } //Wave0write
                } //Wave0write
            } //Wave0write

            return NodeRewardSummaryt.Aggregate(contributions); //Wave0write
        }

        private static EntityBaset FirstAlive(List<EntityBaset> source) //Wave0write
        { //Wave0write
            if (source == null) //Wave0write
            { //Wave0write
                return null; //Wave0write
            } //Wave0write

            foreach (EntityBaset entity in source) //Wave0write
            { //Wave0write
                if (entity != null && !entity.IsDead) //Wave0write
                { //Wave0write
                    return entity; //Wave0write
                } //Wave0write
            } //Wave0write

            return null; //Wave0write
        } //Wave0write

        private static List<EntityBaset> AliveList(List<EntityBaset> source) //Wave0write
        { //Wave0write
            var result = new List<EntityBaset>(); //Wave0write
            if (source == null) //Wave0write
            { //Wave0write
                return result; //Wave0write
            } //Wave0write

            foreach (EntityBaset entity in source) //Wave0write
            { //Wave0write
                if (entity != null && !entity.IsDead) //Wave0write
                { //Wave0write
                    result.Add(entity); //Wave0write
                } //Wave0write
            } //Wave0write

            return result; //Wave0write
        } //Wave0write

        private static void AddIfNotNull(List<EntityBaset> list, EntityBaset entity) //Wave0write
        { //Wave0write
            if (list != null && entity != null) //Wave0write
            { //Wave0write
                list.Add(entity); //Wave0write
            } //Wave0write
        } //Wave0write
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 씬 컨트롤러. CombatFlow 를 보유하고, 몬스터 스포너/입력 공급자/타겟 셀렉터를 조립한다.
    /// 단축키 중 소비 4칸 변경은 이 컨트롤러에서 차단(view-only 모드).
    /// </summary>
    public sealed class CombatController : SceneControllerBase, IPlayerInputProvider
    {
        // Wave0refactor 2026-05-27 (F.6): 씬 직접 배치/할당 참조 필드.
        [SerializeField] private Player playerRef;
        [SerializeField] private CombatMonsterSpawner spawnerRef;
        [SerializeField] private CombatHud hudRef;
        [SerializeField] private bool autoPlayerActions = true;

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

        /// <summary>플레이어 임시 행동(입력 진행 중).</summary>
        private CombatAction pendingAction;

        /// <summary>이미 EndCombat을 호출했는지 방어 플래그.</summary>
        private bool combatEnded;

        // Wave0refactor 2026-05-27 (F.6 + F.7):
        // - 1) Inspector 참조(*Ref) 가 있으면 우선 사용.
        // - 2) 같은 GameObject 에 직접 배치된 컴포넌트만 보조로 사용.
            // - 3) 필수 참조가 없으면 LogError 후 진입 중단.
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

            Player.BindState(gsm.CurrentRun.Player);
            Spawner.SpawnFromNode(ctx.Node, ctx.ErosionMultiplier);
            Monsters = Spawner.SpawnedT;
            Companions = new List<TeamBase>();

            Flow = new CombatFlow
            {
                Input = this,
                TargetSelector = Targeter,
                MonsterAi = new MonsterActionSelector(),
                CompanionAi = new CompanionActionSelector(),
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

            Flow.StartCombat(allies, enemies);
            combatEnded = false;
            pendingAction = null;
            Hud?.OnOpen();
            Targeter.OnTargetConfirmed += OnTargetConfirmed;
        }

        // Wave0refactor 2026-05-27 (direct scene wiring): 핵심 참조 해결 단일 함수.
        // Inspector 직렬화 또는 같은 GameObject 직접 배치만 허용한다.
        private bool ResolveCoreReferences()
        {
            // Spawner
            if (Spawner == null) Spawner = spawnerRef;
            if (Spawner == null) Spawner = GetComponent<CombatMonsterSpawner>();
            if (Spawner == null)
            {
                Debug.LogError("[CombatController] CombatMonsterSpawner 가 씬에 직접 배치/할당되어 있지 않습니다.");
                return false;
            }

            // Targeter
            if (Targeter == null) Targeter = new TargetSelector();

            // Hud
            if (Hud == null) Hud = hudRef;
            if (!autoPlayerActions && Hud == null)
            {
                Debug.LogError("[CombatController] 수동 전투 모드에는 CombatHud 직접 할당이 필요합니다.");
                return false;
            }

            // Player
            if (Player == null) Player = playerRef;
            if (Player == null) Player = GetComponent<Player>();
            if (Player == null)
            {
                Debug.LogError("[CombatController] Player 가 씬에 직접 배치/할당되어 있지 않습니다.");
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            // 동작 요약:
            // - 살아있는 PlayerState 에 Player MonoBehaviour 의 참조 정합성을 맞춘다(Level/Exp 는 PlayerState 권위).
            // - HUD / Spawner 정리.
            // - 타겟 셀렉터 콜백 해제.
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && Player != null)
            {
                Player.CopyToState(gsm.CurrentRun?.Player);
            }

            Hud?.OnClose();
            Spawner?.Cleanup();
            if (Targeter != null)
            {
                Targeter.OnTargetConfirmed -= OnTargetConfirmed;
            }
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
            // 동작 요약:
            // - pendingAction 초기화.
            // - Hud 가 있으면 행동 패널 표시.
            // - Hud 가 없는 자동 모드에서는 즉시 PlayerPickAttack 으로 기본 공격 확정.
            pendingAction = null;
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
            return action;
        }

        // Wave0refactor 2026-05-27 (F.7): 자동 모드 판정 단일화.
        private bool IsAutoMode => autoPlayerActions;

        /// <summary>공격 버튼.</summary>
        public void PlayerPickAttack()
        {
            // 동작 요약:
            // - pendingAction 을 Attack 으로 만들고 Targets 비움.
            // - 자동 모드면 CombatTargeting.FirstAlive 로 즉시 타겟 확정.
            // - 수동 모드면 호버 시작.
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
        }

        /// <summary>스킬 버튼.</summary>
        public void PlayerPickSkill(int slotIndex)
        {
            // 동작 요약:
            // - 슬롯의 Skill 조회. CanUse 실패 시 무시.
            // - pendingAction 을 Skill 로 만들고 자동/수동 모드 분기.
            // - 자동 모드:
            //   * EnemySingle / AllySingle → CombatTargeting.FirstAlive 로 즉시 확정.
            //   * Self → 자기 자신 1명.
            //   * EnemyAll / AllyAll → 살아있는 전체.
            // - 수동 모드:
            //   * 단일 타겟이면 호버 시작.
            //   * 그 외는 즉시 확정(스킬 데이터 기반).
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

            // 자동 확정 케이스: Self / EnemyAll / AllyAll 은 호버 없이 즉시 확정.
            if (tt == SkillTargetType.Self)
            {
                pendingAction.Targets.Add(Player);
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
                return;
            }

            // 단일 타겟. 자동 모드면 첫 살아있는 후보로 확정, 수동 모드면 호버 시작.
            if (IsAutoMode)
            {
                IList<EntityBase> source = tt == SkillTargetType.AllySingle ? Flow?.AlliesT : Flow?.EnemiesT;
                EntityBase pick = CombatTargeting.FirstAlive(source);
                if (pick != null) pendingAction.Targets.Add(pick);
                return;
            }

            Targeter.BeginHover(tt);
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
        }

        /// <summary>소모 아이템 슬롯 클릭.</summary>
        public void PlayerUseItem(int slotIndex)
        {
            // 동작 요약:
            // - 아이템은 큐에 넣지 않고 ConsumableSlots.TryUse 가 즉시 처리.
            // - pendingAction 은 유지(아이템은 턴 미소모).
            Player?.Consumables?.TryUse(slotIndex, Player, Player.Inventory);
        }

        /// <summary>타겟 호버 후 클릭 시 호출.</summary>
        public void OnTargetConfirmed(EntityBase target)
        {
            if (pendingAction == null || target == null)
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

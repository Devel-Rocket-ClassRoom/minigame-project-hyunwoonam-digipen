using System.Collections.Generic;

/// <summary>
/// 플레이어 행동 선택 상태를 보관하고 CombatAction 으로 확정한다.
/// </summary>
public sealed class PlayerActionSelector
{
    private CombatAction pendingAction;

    public bool HasConfirmedAction => IsActionConfirmed();

    public void Clear()
    {
        pendingAction = null;
    }

    public CombatAction PopAction()
    {
        CombatAction action = pendingAction;
        pendingAction = null;
        return action;
    }

    /// <summary>공격 행동 선택.</summary>
    public void PickAttack(
        Player player,
        CombatFlow flow,
        TargetSelector targeter,
        CombatHud hud,
        bool isAutoMode
    )
    {
        pendingAction = new CombatAction
        {
            Actor = player,
            Type = CombatActionType.Attack,
            Targets = new List<EntityBase>(),
            ConsumesTurn = true,
        };

        if (isAutoMode)
        {
            EntityBase auto = CombatTargeting.FirstAlive(flow?.EnemiesT);
            if (auto != null)
                pendingAction.Targets.Add(auto);
            return;
        }

        targeter?.BeginHover(SkillTargetType.EnemySingle);
        hud?.ShowTargetPrompt(SkillTargetType.EnemySingle);
    }

    /// <summary>스킬 행동 선택.</summary>
    public void PickSkill(
        Player player,
        CombatFlow flow,
        TargetSelector targeter,
        CombatHud hud,
        int slotIndex,
        bool isAutoMode
    )
    {
        Skill skill = player != null ? player.GetActiveSkill(slotIndex) : null;
        SkillData effectiveData = SkillRuntimeResolver.Resolve(skill, player);
        if (skill == null || !skill.CanUse(player, effectiveData))
        {
            return;
        }

        pendingAction = new CombatAction
        {
            Actor = player,
            Type = CombatActionType.Skill,
            Skill = skill,
            EffectiveSkillData = effectiveData,
            Targets = new List<EntityBase>(),
            ConsumesTurn = true,
        };

        SkillTargetType targetType = effectiveData.TargetType;
        if (targetType == SkillTargetType.Self)
        {
            pendingAction.Targets.Add(player);
            hud?.ClearTargetPrompt();
            return;
        }

        if (targetType == SkillTargetType.EnemyAll || targetType == SkillTargetType.AllyAll)
        {
            CombatTargeting.FillByTargetType(
                pendingAction.Targets,
                targetType,
                player,
                flow?.AlliesT,
                flow?.EnemiesT,
                CombatTargeting.SingleSelect.Random
            );
            hud?.ClearTargetPrompt();
            return;
        }

        if (isAutoMode)
        {
            IList<EntityBase> source =
                targetType == SkillTargetType.AllySingle ? flow?.AlliesT : flow?.EnemiesT;
            EntityBase pick = CombatTargeting.FirstAlive(source);
            if (pick != null)
                pendingAction.Targets.Add(pick);
            return;
        }

        targeter?.BeginHover(targetType);
        hud?.ShowTargetPrompt(targetType);
    }

    /// <summary>방어 행동 선택.</summary>
    public void PickDefend(Player player, TargetSelector targeter, CombatHud hud)
    {
        pendingAction = new CombatAction
        {
            Actor = player,
            Type = CombatActionType.Defend,
            Targets = new List<EntityBase> { player },
            ConsumesTurn = true,
        };
        targeter?.EndHover();
        hud?.ClearTargetPrompt();
    }

    /// <summary>타겟 호버 후 클릭 확정.</summary>
    public void ConfirmTarget(EntityBase target, TargetSelector targeter, CombatHud hud)
    {
        if (
            pendingAction == null
            || target == null
            || target.IsDead
            || !target.gameObject.activeInHierarchy
        )
        {
            return;
        }

        if (pendingAction.Targets == null)
        {
            pendingAction.Targets = new List<EntityBase>();
        }

        pendingAction.Targets.Clear();
        pendingAction.Targets.Add(target);
        targeter?.EndHover();
        hud?.ClearTargetPrompt();
    }

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
            SkillTargetType targetType = pendingAction.ResolvedSkillData.TargetType;
            if (
                targetType == SkillTargetType.Self
                || targetType == SkillTargetType.EnemyAll
                || targetType == SkillTargetType.AllyAll
            )
            {
                return true;
            }

            return pendingAction.Targets != null && pendingAction.Targets.Count > 0;
        }

        return false;
    }
}

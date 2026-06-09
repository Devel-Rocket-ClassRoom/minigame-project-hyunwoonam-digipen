using System.Collections.Generic;

/// <summary>
/// 스킬 효과 실행기. SkillDatat의 배수(Damage/Heal/Shield)와 타겟 리스트를 받아
/// 데미지/회복/보호막을 적용한다. CombatFlowt가 호출.
/// </summary>
public static class SkillEffect
{
    /// <summary>
    /// 데미지 스킬 적용.
    /// </summary>
    // Wave0refactor 2026-05-27 (F.3.2): 추가 공격 확률(ExtraAttackChance) 인용 제거.
    // 룬 데미지 보정 / 추가 공격은 Wave4(스킬+룬) 에서 BalanceData 곡선과 함께 다시 도입.
    public static void ApplyDamage(EntityBase caster, IList<EntityBase> targets, SkillData data)
    {
        // 동작 요약:
        // - 각 타겟마다 DamageCalculator.ComputeSkill(caster, target, data) → target.ApplyDamage().
        // - 룬 / 패시브 보정은 StatBlock 의 PassiveBonus 에 이미 누적되어 ATK 로 반영되므로
        //   여기서는 별도 곱 보정을 하지 않는다(중복 방지).
        if (caster == null || targets == null || data == null)
        {
            return;
        }

        foreach (EntityBase target in targets)
        {
            if (target == null || target.IsDead)
            {
                continue;
            }

            target.ApplyDamage(DamageCalculator.ComputeSkill(caster, target, data));
        }
    }

    /// <summary>
    /// 회복 스킬 적용.
    /// </summary>
    public static void ApplyHeal(EntityBase caster, IList<EntityBase> targets, SkillData data)
    {
        // 동작 요약:
        // - 각 타겟마다 (caster.ATK 또는 SPELL_POWER * HealScale) 회복량 계산.
        // - target.ApplyHeal().
        if (caster?.Stats == null || targets == null || data == null)
        {
            return;
        }

        int healAmount = UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(caster.Stats.ATK * data.HealScale));
        foreach (EntityBase target in targets)
        {
            if (target != null && !target.IsDead)
            {
                target.ApplyHeal(healAmount);
            }
        }
    }

    /// <summary>
    /// 보호막 스킬 적용.
    /// </summary>
    public static void ApplyShield(EntityBase caster, IList<EntityBase> targets, SkillData data)
    {
        // 동작 요약:
        // - shield = caster.Stats.DEF * data.ShieldScale.
        // - target.ApplyShield(shield, durationRounds=1 또는 데이터값).
        if (caster?.Stats == null || targets == null || data == null)
        {
            return;
        }

        int shieldAmount = UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(caster.Stats.DEF * data.ShieldScale));
        foreach (EntityBase target in targets)
        {
            if (target != null && !target.IsDead)
            {
                target.ApplyShield(shieldAmount, System.Math.Max(1, data.ShieldDurationRounds));
            }
        }
    }
}


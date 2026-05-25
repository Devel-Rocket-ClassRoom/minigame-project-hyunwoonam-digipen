using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 스킬 효과 실행기. SkillDatat의 배수(Damage/Heal/Shield)와 타겟 리스트를 받아
    /// 데미지/회복/보호막을 적용한다. CombatFlowt가 호출.
    /// </summary>
    public static class SkillEffectt
    {
        /// <summary>
        /// 데미지 스킬 적용.
        /// </summary>
        public static void ApplyDamage(EntityBaset caster, IList<EntityBaset> targets, SkillDatat data)
        {
            // 동작 요약:
            // - 각 타겟마다 DamageCalculatort.Compute(caster, target, data) 호출 후 target.ApplyDamage().
            // - 룬 보정(RuneEffectt.BoostSkillId == data.Id) 적용.
            // - 추가 공격 확률(SPD) 처리.
        }

        /// <summary>
        /// 회복 스킬 적용.
        /// </summary>
        public static void ApplyHeal(EntityBaset caster, IList<EntityBaset> targets, SkillDatat data)
        {
            // 동작 요약:
            // - 각 타겟마다 (caster.ATK 또는 SPELL_POWER * HealScale) 회복량 계산.
            // - target.ApplyHeal().
        }

        /// <summary>
        /// 보호막 스킬 적용.
        /// </summary>
        public static void ApplyShield(EntityBaset caster, IList<EntityBaset> targets, SkillDatat data)
        {
            // 동작 요약:
            // - shield = caster.Stats.DEF * data.ShieldScale.
            // - target.ApplyShield(shield, durationRounds=1 또는 데이터값).
        }
    }
}

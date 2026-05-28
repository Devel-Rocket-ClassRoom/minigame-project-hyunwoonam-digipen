using System.Collections.Generic;

namespace Tempt
{
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
            if (caster == null || targets == null || data == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            foreach (EntityBase target in targets) //Wave0write
            { //Wave0write
                if (target == null || target.IsDead) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                target.ApplyDamage(DamageCalculator.ComputeSkill(caster, target, data)); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 회복 스킬 적용.
        /// </summary>
        public static void ApplyHeal(EntityBase caster, IList<EntityBase> targets, SkillData data)
        {
            // 동작 요약:
            // - 각 타겟마다 (caster.ATK 또는 SPELL_POWER * HealScale) 회복량 계산.
            // - target.ApplyHeal().
            //TODO: int healAmount = UnityEngine.Mathf.RoundToInt(caster.Stats.ATK * data.HealScale);
            //TODO: foreach (EntityBase target in targets)
            //TODO:     target.ApplyHeal(healAmount);
            if (caster?.Stats == null || targets == null || data == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            int healAmount = UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(caster.Stats.ATK * data.HealScale)); //Wave0write
            foreach (EntityBase target in targets) //Wave0write
            { //Wave0write
                if (target != null && !target.IsDead) //Wave0write
                { //Wave0write
                    target.ApplyHeal(healAmount); //Wave0write
                } //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 보호막 스킬 적용.
        /// </summary>
        public static void ApplyShield(EntityBase caster, IList<EntityBase> targets, SkillData data)
        {
            // 동작 요약:
            // - shield = caster.Stats.DEF * data.ShieldScale.
            // - target.ApplyShield(shield, durationRounds=1 또는 데이터값).
            //TODO: int shieldAmount = UnityEngine.Mathf.RoundToInt(caster.Stats.DEF * data.ShieldScale);
            //TODO: int duration = data.ShieldDuration > 0 ? data.ShieldDuration : 1;
            //TODO: foreach (EntityBase target in targets)
            //TODO:     target.ApplyShield(shieldAmount, duration);
            if (caster?.Stats == null || targets == null || data == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            int shieldAmount = UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(caster.Stats.DEF * data.ShieldScale)); //Wave0write
            foreach (EntityBase target in targets) //Wave0write
            { //Wave0write
                if (target != null && !target.IsDead) //Wave0write
                { //Wave0write
                    target.ApplyShield(shieldAmount, 1); //Wave0write
                } //Wave0write
            } //Wave0write
        }
    }
}


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
            // - 룬 데미지 보정: caster.PassiveSkills 또는 caster의 해금된 룬 노드 중
            //   EffectType == DamageBoost 인 노드의 EffectValue(%) 를 최종 데미지에 곱 적용.
            //   (CharacterBaset.SyncPassivesFromRunes 이후 스탯에 이미 반영된 경우 중복 적용 금지)
            // - 추가 공격 확률(SPD) 처리.
            //TODO: foreach (EntityBaset target in targets)
            //TODO: {
            //TODO:     int dmg = DamageCalculatort.ComputeSkill(caster, target, data);
            //TODO:     target.ApplyDamage(dmg);
            //TODO: }
            //TODO: // 추가 공격 확률(SPD 곡선) — 동일 행동을 한 번 더 실행
            //TODO: float extraChance = DamageCalculatort.ExtraAttackChance(caster);
            //TODO: if (UnityEngine.Random.value < extraChance)
            //TODO:     foreach (EntityBaset target in targets)
            //TODO:         target.ApplyDamage(DamageCalculatort.ComputeSkill(caster, target, data));
            if (caster == null || targets == null || data == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            foreach (EntityBaset target in targets) //Wave0write
            { //Wave0write
                if (target == null || target.IsDead) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                target.ApplyDamage(DamageCalculatort.ComputeSkill(caster, target, data)); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 회복 스킬 적용.
        /// </summary>
        public static void ApplyHeal(EntityBaset caster, IList<EntityBaset> targets, SkillDatat data)
        {
            // 동작 요약:
            // - 각 타겟마다 (caster.ATK 또는 SPELL_POWER * HealScale) 회복량 계산.
            // - target.ApplyHeal().
            //TODO: int healAmount = UnityEngine.Mathf.RoundToInt(caster.Stats.ATK * data.HealScale);
            //TODO: foreach (EntityBaset target in targets)
            //TODO:     target.ApplyHeal(healAmount);
            if (caster?.Stats == null || targets == null || data == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            int healAmount = UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(caster.Stats.ATK * data.HealScale)); //Wave0write
            foreach (EntityBaset target in targets) //Wave0write
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
        public static void ApplyShield(EntityBaset caster, IList<EntityBaset> targets, SkillDatat data)
        {
            // 동작 요약:
            // - shield = caster.Stats.DEF * data.ShieldScale.
            // - target.ApplyShield(shield, durationRounds=1 또는 데이터값).
            //TODO: int shieldAmount = UnityEngine.Mathf.RoundToInt(caster.Stats.DEF * data.ShieldScale);
            //TODO: int duration = data.ShieldDuration > 0 ? data.ShieldDuration : 1;
            //TODO: foreach (EntityBaset target in targets)
            //TODO:     target.ApplyShield(shieldAmount, duration);
            if (caster?.Stats == null || targets == null || data == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            int shieldAmount = UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(caster.Stats.DEF * data.ShieldScale)); //Wave0write
            foreach (EntityBaset target in targets) //Wave0write
            { //Wave0write
                if (target != null && !target.IsDead) //Wave0write
                { //Wave0write
                    target.ApplyShield(shieldAmount, 1); //Wave0write
                } //Wave0write
            } //Wave0write
        }
    }
}

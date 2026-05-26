namespace Tempt
{
    /// <summary>
    /// 데미지/회복/보호막 계산. 데이터 + 룬 보정 반영.
    /// 방어 상태 시 추가 경감.
    /// </summary>
    public static class DamageCalculatort
    {
        /// <summary>
        /// 기본 공격 데미지 계산.
        /// </summary>
        public static int ComputeAttack(EntityBaset attacker, EntityBaset defender)
        {
            // 동작 요약:
            // - raw = attacker.Stats.ATK.
            // - mitigated = max(1, raw - defender.Stats.DEF).
            // - defender.IsDefending이면 mitigated -= defender.Stats.DEF (또는 비율 차감).
            // - 결과 반환.
            //TODO: int raw = attacker.Stats.ATK;
            //TODO: int mitigated = Mathf.Max(1, raw - defender.Stats.DEF);
            //TODO: if (defender.IsDefending) mitigated = Mathf.Max(1, mitigated - defender.Stats.DEF / 2);
            //TODO: return mitigated;
            if (attacker?.Stats == null || defender?.Stats == null) //Wave0write
            { //Wave0write
                return 0; //Wave0write
            } //Wave0write

            int raw = attacker.Stats.ATK; //Wave0write
            return UnityEngine.Mathf.Max(1, raw - defender.Stats.DEF); //Wave0write
        }

        /// <summary>
        /// 스킬 데미지 계산.
        /// </summary>
        public static int ComputeSkill(EntityBaset attacker, EntityBaset defender, SkillDatat skill)
        {
            // 동작 요약:
            // - base = attacker.Stats.ATK * skill.DamageScale.
            // - 룬 보정 BoostSkillId == skill.Id면 BoostSkillDamagePercent 적용.
            // - DEF 감쇠.
            // - 방어 상태 추가 경감.
            //TODO: float baseDmg = attacker.Stats.ATK * skill.DamageScale;
            //TODO: float boost = attacker.GetDamageBoostForSkill(skill.Id); // 룬 RuneEffectTypet.DamageBoost 확인
            //TODO: baseDmg *= (1f + boost);
            //TODO: int mitigated = Mathf.Max(1, Mathf.RoundToInt(baseDmg) - defender.Stats.DEF);
            //TODO: if (defender.IsDefending) mitigated = Mathf.Max(1, mitigated - defender.Stats.DEF / 2);
            //TODO: return mitigated;
            if (attacker?.Stats == null || defender?.Stats == null || skill == null) //Wave0write
            { //Wave0write
                return 0; //Wave0write
            } //Wave0write

            float baseDamage = attacker.Stats.ATK * UnityEngine.Mathf.Max(0.1f, skill.DamageScale); //Wave0write
            return UnityEngine.Mathf.Max(1, UnityEngine.Mathf.RoundToInt(baseDamage) - defender.Stats.DEF); //Wave0write
        }

        /// <summary>
        /// SPD 기반 추가 공격 확률(0~1).
        /// </summary>
        public static float ExtraAttackChance(EntityBaset attacker)
        {
            // 동작 요약: attacker.Stats.SPD를 BalanceDatat 곡선에 통과시켜 확률 반환.
            //TODO: float spd = attacker.Stats.SPD;
            //TODO: // BalanceDatat.ExtraAttackCurve: AnimationCurve, x=SPD, y=확률(0~1)
            //TODO: return BalanceDatat.ExtraAttackCurve.Evaluate(spd);
            if (attacker?.Stats == null) //Wave0write
            { //Wave0write
                return 0f; //Wave0write
            } //Wave0write

            return UnityEngine.Mathf.Clamp01((attacker.Stats.SPD - 10) * 0.01f); //Wave0write
        }
    }
}

namespace Tempt
{
    /// <summary>
    /// 데미지/회복/보호막 계산. 데이터 + 룬 보정 반영.
    /// 방어 상태 시 추가 경감.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// 기본 공격 데미지 계산.
        /// </summary>
        public static int ComputeAttack(EntityBase attacker, EntityBase defender)
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
        public static int ComputeSkill(EntityBase attacker, EntityBase defender, SkillData skill)
        {
            // 동작 요약:
            // - base = attacker.Stats.ATK * skill.DamageScale.
            // - 룬 보정 BoostSkillId == skill.Id면 BoostSkillDamagePercent 적용.
            // - DEF 감쇠.
            // - 방어 상태 추가 경감.
            //TODO: float baseDmg = attacker.Stats.ATK * skill.DamageScale;
            //TODO: float boost = attacker.GetDamageBoostForSkill(skill.Id); // 룬 RuneEffectType.DamageBoost 확인
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

        // Wave0refactor 2026-05-27 (F.3.2): ExtraAttackChance 제거.
        // 사유: 어디서도 호출되지 않는 데드 코드 + (SPD - 10) * 0.01f 매직 넘버.
        // 다시 도입할 때는 BalanceData 의 AnimationCurve / 계수와 함께 한 번에 구현(Wave4 스킬+룬).
        // 호출자가 다시 생긴다면 SkillEffect.ApplyDamage 가 후보지.
    }
}


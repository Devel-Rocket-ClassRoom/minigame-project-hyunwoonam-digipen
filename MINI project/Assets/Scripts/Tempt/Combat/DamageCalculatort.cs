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
            return 0;
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
            return 0;
        }

        /// <summary>
        /// SPD 기반 추가 공격 확률(0~1).
        /// </summary>
        public static float ExtraAttackChance(EntityBaset attacker)
        {
            // 동작 요약: attacker.Stats.SPD를 BalanceDatat 곡선에 통과시켜 확률 반환.
            return 0f;
        }
    }
}

namespace Tempt
{
    /// <summary>
    /// 행동의 실행 시간을 합산하는 유틸. 기본 0.1초 + 애니메이션 + 이펙트 + 데이터 명시 시간.
    /// </summary>
    public static class ActionTiming
    {
        /// <summary>최소 기본 시간(초).</summary>
        private const float DefaultMinBaseSec = 0.1f;
        private const float DefaultAttackActionSec = 0.3f;
        private const float DefaultSkillFallbackSec = 0.5f;
        private const float DefaultDefendActionSec = 0.1f;

        /// <summary>
        /// CombatActiont에 사용할 총 실행 시간 계산.
        /// </summary>
        public static float Compute(CombatAction action)
        {
            // 동작 요약:
            // - total = MinBaseSec.
            // - action.Type별:
            //   * Attack → +기본 공격 애니메이션 길이(0.3 등).
            //   * Skill → + Skill.Data.ActionDuration(설정값) 또는 애니/이펙트 길이.
            //   * Defend → +0.1(즉시).
            // - 합산 후 반환. 데이터에 명시값 있으면 우선.
            if (action == null)
            {
                return GetBalanceValue(balance => balance.MinActionTimeSec, DefaultMinBaseSec);
            }

            float total = GetBalanceValue(balance => balance.MinActionTimeSec, DefaultMinBaseSec);
            switch (action.Type)
            {
                case CombatActionType.Attack:
                    total += GetBalanceValue(balance => balance.AttackActionTimeSec, DefaultAttackActionSec);
                    break;
                case CombatActionType.Skill:
                    total += action.Skill?.Data != null && action.Skill.Data.ActionDuration > 0f
                        ? action.Skill.Data.ActionDuration
                        : GetBalanceValue(balance => balance.SkillActionFallbackSec, DefaultSkillFallbackSec);
                    break;
                case CombatActionType.Defend:
                    total += GetBalanceValue(balance => balance.DefendActionTimeSec, DefaultDefendActionSec);
                    break;
            }

            return total;
        }

        private static float GetBalanceValue(System.Func<BalanceData, float> getter, float fallback)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Data?.Balance != null)
            {
                float value = getter(gsm.Data.Balance);
                return value > 0f ? value : fallback;
            }

            return fallback;
        }
    }
}


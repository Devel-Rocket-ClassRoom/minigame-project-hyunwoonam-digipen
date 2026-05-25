namespace Tempt
{
    /// <summary>
    /// 행동의 실행 시간을 합산하는 유틸. 기본 0.1초 + 애니메이션 + 이펙트 + 데이터 명시 시간.
    /// </summary>
    public static class ActionTimingt
    {
        /// <summary>최소 기본 시간(초).</summary>
        public const float MinBaseSec = 0.1f;

        /// <summary>
        /// CombatActiont에 사용할 총 실행 시간 계산.
        /// </summary>
        public static float Compute(CombatActiont action)
        {
            // 동작 요약:
            // - total = MinBaseSec.
            // - action.Type별:
            //   * Attack → +기본 공격 애니메이션 길이(0.3 등).
            //   * Skill → + Skill.Data.ActionDuration(설정값) 또는 애니/이펙트 길이.
            //   * Defend → +0.1(즉시).
            //   * Item → +0.2 정도(애니 짧음).
            // - 합산 후 반환. 데이터에 명시값 있으면 우선.
            return MinBaseSec;
        }
    }
}

namespace Tempt
{
    /// <summary>
    /// 광산. 일자가 지날 때마다 마석을 누적. 진입 시 적립분을 합산해 GameRunStatet.ManaStone에 더한다.
    /// </summary>
    public sealed class Minet
    {
        /// <summary>광산 단계(1~3).</summary>
        public int MineLevel;

        /// <summary>일일 마석 획득량.</summary>
        public int DailyGain;

        /// <summary>마지막 정산 일자.</summary>
        public int LastSettledDay;

        /// <summary>
        /// 광산 초기화.
        /// </summary>
        public void Initialize(int mineLevel, int dailyGain)
        {
            // 동작 요약: 필드 저장. LastSettledDay = 현재 CurrentDay로 초기화(첫 진입 시).
            //TODO: MineLevel = mineLevel;
            //TODO: DailyGain = dailyGain;
            //TODO: if (LastSettledDay == 0) // 최초 진입
            //TODO:     LastSettledDay = GameSystemManagert.Instance.CurrentRun.CurrentDay;
        }

        /// <summary>
        /// 안전지대 진입 시 누적 정산.
        /// </summary>
        public int SettleAccrued(int currentDay)
        {
            // 동작 요약:
            // - daysPassed = currentDay - LastSettledDay.
            // - gained = daysPassed * DailyGain.
            // - LastSettledDay = currentDay.
            // - gained 반환(호출자가 ManaStone에 합산).
            //TODO: int daysPassed = currentDay - LastSettledDay;
            //TODO: int gained = UnityEngine.Mathf.Max(0, daysPassed * DailyGain);
            //TODO: LastSettledDay = currentDay;
            //TODO: return gained;
            return 0;
        }
    }
}

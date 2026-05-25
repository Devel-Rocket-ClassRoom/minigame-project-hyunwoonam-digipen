namespace Tempt
{
    /// <summary>
    /// 안전지대 3: 광산 1. 일자가 지날 때마다 마석 일정량 자동 적립.
    /// </summary>
    public sealed class Safe3Controllert : SafeZoneControllerBaset
    {
        /// <summary>광산 1.</summary>
        public Minet Mine;

        /// <inheritdoc/>
        protected override void SetupZoneFeatures()
        {
            // 동작 요약:
            // - Mine.Initialize(mineLevel=1, dailyGain=BalanceDatat.MineDailyGain[0]).
            // - 진입 시 누적된 마석을 GameRunStatet.ManaStone에 합산.
        }
    }
}

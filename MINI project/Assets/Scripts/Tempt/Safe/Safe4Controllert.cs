namespace Tempt
{
    /// <summary>
    /// 안전지대 4: 광산 2. 광산 1보다 일일 획득량 ↑.
    /// </summary>
    public sealed class Safe4Controllert : SafeZoneControllerBaset
    {
        /// <summary>광산 2.</summary>
        public Minet Mine;

        /// <inheritdoc/>
        protected override void SetupZoneFeatures()
        {
            // 동작 요약: Mine.Initialize(level=2, dailyGain=BalanceDatat.MineDailyGain[1]).
        }
    }
}

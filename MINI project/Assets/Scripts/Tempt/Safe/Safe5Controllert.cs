namespace Tempt
{
    /// <summary>
    /// 안전지대 5: 광산 3. 광산 2보다 일일 획득량 ↑.
    /// </summary>
    public sealed class Safe5Controllert : SafeZoneControllerBaset
    {
        /// <summary>광산 3.</summary>
        public Minet Mine;

        /// <inheritdoc/>
        protected override void SetupZoneFeatures()
        {
            // 동작 요약: Mine.Initialize(level=3, dailyGain=BalanceDatat.MineDailyGain[2]).
        }
    }
}

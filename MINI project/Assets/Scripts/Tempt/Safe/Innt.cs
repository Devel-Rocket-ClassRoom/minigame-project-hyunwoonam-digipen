namespace Tempt
{
    /// <summary>
    /// 주점. 숙소(돈을 내면 활성), 보관함 구매, 동료 모집(층수에 따라 해금).
    /// </summary>
    public sealed class Innt
    {
        /// <summary>
        /// 주점 UI 열기.
        /// </summary>
        public void Open()
        {
            // 동작 요약:
            // - UIManagert.OpenPage(InnPage).
            // - 보관함 잠금 여부에 따라 구매 버튼 표시.
            // - 동료 후보 = CompanionDatat 중 RequiredFloor ≤ HighestFloor.
            // - 모집 버튼은 RecruitPrice 만큼 골드 차감으로 동작.
        }

        /// <summary>
        /// 숙소 사용(휴식). 골드 차감 후 HP/MP 완전 회복.
        /// </summary>
        public bool TryRest(int price)
        {
            // 동작 요약:
            // - 골드 >= price 검사 후 차감.
            // - Player + 동료 HP/MP 완전 회복.
            return false;
        }

        /// <summary>
        /// 보관함 구매.
        /// </summary>
        public bool TryBuyLocker(int price)
        {
            // 동작 요약:
            // - 보관함 이미 구매 시 false.
            // - 골드 차감.
            // - Player.Locker.Unlock().
            return false;
        }

        /// <summary>
        /// 동료 모집.
        /// </summary>
        public bool TryRecruit(int companionId)
        {
            // 동작 요약:
            // - CompanionDatat 조회.
            // - RecruitPrice 차감.
            // - Roster에 추가(Bench로). 길드에서 파티 구성 가능.
            return false;
        }
    }
}

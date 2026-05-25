namespace Tempt
{
    /// <summary>
    /// 상점. 아이템 구매/판매. 단계별 가격 인플레이션 적용.
    /// </summary>
    public sealed class Shopt
    {
        /// <summary>이번 진입 가격 보정(단계 침식률 기반).</summary>
        public float CurrentInflation = 1f;

        /// <summary>
        /// 상점 UI 열기.
        /// </summary>
        public void Open()
        {
            // 동작 요약:
            // - 인플레이션 계산(현재 안전지대 단계, 현재 침식률).
            // - 판매 목록 UI 그리기.
        }

        /// <summary>
        /// 구매.
        /// </summary>
        public bool TryBuy(int itemId, int count)
        {
            // 동작 요약:
            // - price = data.BaseBuyPrice * CurrentInflation * count.
            // - 골드 차감 → Inventory.Add.
            return false;
        }

        /// <summary>
        /// 판매.
        /// </summary>
        public bool TrySell(int itemId, int count)
        {
            // 동작 요약:
            // - Inventory.Remove(itemId, count) → 골드 += BaseSellPrice * count.
            return false;
        }
    }
}

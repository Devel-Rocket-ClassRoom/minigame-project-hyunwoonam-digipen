namespace Tempt
{
    /// <summary>
    /// 대장간. 장비 강화. 가격은 단계 인플레이션 영향.
    /// </summary>
    public sealed class Forget
    {
        /// <summary>
        /// 대장간 UI 열기.
        /// </summary>
        public void Open()
        {
            // 동작 요약:
            // - 인벤토리 + 장비 슬롯 합쳐 강화 가능 목록 표시.
            // - 현재 단계 강화 등급 상한(SafeZoneDeft 또는 BalanceDatat) 표시.
        }

        /// <summary>
        /// 강화 시도.
        /// </summary>
        public bool TryEnhance(Itemt target)
        {
            // 동작 요약:
            // - 가격 = base * (level + 1) * inflation.
            // - 골드 차감.
            // - target.Enhancement += 1.
            // - 상한(SafeIndex별) 초과 거부.
            // - 성공 시 EventBust.RaiseEquipmentChanged.
            return false;
        }
    }
}

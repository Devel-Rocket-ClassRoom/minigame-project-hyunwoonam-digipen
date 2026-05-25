namespace Tempt
{
    /// <summary>
    /// 전투 중 사용 가능한 소모 4칸. 인벤토리에서 미리 지정(전투 외에서만 변경 가능).
    /// </summary>
    public sealed class ConsumableSlotst
    {
        /// <summary>슬롯 4칸의 아이템 ID(0=비어있음).</summary>
        public int[] SlotItemIds = new int[4];

        /// <summary>
        /// 슬롯 설정. 전투 중에는 호출 금지(UIManagert가 차단).
        /// </summary>
        public bool TrySetSlot(int slotIndex, int itemId, InventoryStatet inv)
        {
            // 동작 요약:
            // - 슬롯 범위 검사 [0,3].
            // - itemId가 0이면 비우기.
            // - itemId가 인벤토리에 존재하고 소모 카테고리인지 검사.
            // - 같은 itemId가 다른 슬롯에 이미 있으면 정책 결정(설계: 중복 허용).
            return false;
        }

        /// <summary>
        /// 전투 중 슬롯 사용. 행동 비용 없음(설계변경.txt).
        /// </summary>
        public bool TryUse(int slotIndex, EntityBaset user, InventoryStatet inv)
        {
            // 동작 요약:
            // - 슬롯 아이템 조회.
            // - inv.CountOf 검사 → Remove 1.
            // - 아이템 효과 적용(ConsumeEffectKey 분기 또는 데이터 표).
            // - 후퇴 아이템(IsRetreat)이면 Playert.UseRetreatItem() 위임.
            return false;
        }

        /// <summary>
        /// 인벤토리 동기화 후 잔량 0이 된 슬롯은 자동으로 비운다(설계: 전투 외 변경 시).
        /// </summary>
        public void PruneEmptySlots(InventoryStatet inv)
        {
            // 동작 요약: 각 슬롯의 아이템 보유량이 0이면 슬롯을 0으로 비운다.
        }
    }
}

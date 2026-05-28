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
            //TODO: if (slotIndex < 0 || slotIndex > 3) return false;
            //TODO: if (itemId == 0) { SlotItemIds[slotIndex] = 0; return true; }
            //TODO: // 인벤토리 보유 확인 + 소모 카테고리 확인
            //TODO: ItemDatat data = GameSystemManagert.Instance.Data.Items[itemId];
            //TODO: if (data.Category != ItemCategoryt.Consumable) return false;
            //TODO: if (inv.CountOf(itemId) <= 0) return false;
            //TODO: SlotItemIds[slotIndex] = itemId;
            //TODO: return true;
            if (slotIndex < 0 || slotIndex >= SlotItemIds.Length) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (itemId == 0) //Wave0write
            { //Wave0write
                SlotItemIds[slotIndex] = 0; //Wave0write
                return true; //Wave0write
            } //Wave0write

            if (inv == null || inv.CountOf(itemId) <= 0) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (!GameSystemManagert.TryGetInstance(out GameSystemManagert gsm) || !gsm.Data.Items.TryGetValue(itemId, out ItemDatat data)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (data.Category != ItemCategoryt.Consumable) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            SlotItemIds[slotIndex] = itemId; //Wave0write
            return true; //Wave0write
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
            //TODO: if (slotIndex < 0 || slotIndex > 3) return false;
            //TODO: int itemId = SlotItemIds[slotIndex];
            //TODO: if (itemId == 0) return false;
            //TODO: if (inv.CountOf(itemId) <= 0) return false;
            //TODO: inv.Remove(itemId, 1);
            //TODO: ItemDatat data = GameSystemManagert.Instance.Data.Items[itemId];
            //TODO: if (data.IsRetreat)
            //TODO:     (user as Playert)?.UseRetreatItem();
            //TODO: else
            //TODO:     ItemEffectAppliert.Apply(data, user); // 회복/버프 효과 분기 처리
            //TODO: return true;
            if (slotIndex < 0 || slotIndex >= SlotItemIds.Length || user == null || inv == null) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            int itemId = SlotItemIds[slotIndex]; //Wave0write
            if (itemId == 0 || inv.CountOf(itemId) <= 0) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (!GameSystemManagert.TryGetInstance(out GameSystemManagert gsm) || !gsm.Data.Items.TryGetValue(itemId, out ItemDatat data)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (!inv.Remove(itemId, 1)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            ApplyConsumableEffect(data, user); //Wave0write
            if (inv.CountOf(itemId) <= 0) //Wave0write
            { //Wave0write
                SlotItemIds[slotIndex] = 0; //Wave0write
            } //Wave0write

            return true; //Wave0write
        }

        /// <summary>
        /// 인벤토리 동기화 후 잔량 0이 된 슬롯은 자동으로 비운다(설계: 전투 외 변경 시).
        /// </summary>
        public void PruneEmptySlots(InventoryStatet inv)
        {
            // 동작 요약: 각 슬롯의 아이템 보유량이 0이면 슬롯을 0으로 비운다.
            //TODO: for (int i = 0; i < 4; i++)
            //TODO:     if (SlotItemIds[i] != 0 && inv.CountOf(SlotItemIds[i]) <= 0)
            //TODO:         SlotItemIds[i] = 0;
            if (inv == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            for (int i = 0; i < SlotItemIds.Length; i++) //Wave0write
            { //Wave0write
                if (SlotItemIds[i] != 0 && inv.CountOf(SlotItemIds[i]) <= 0) //Wave0write
                { //Wave0write
                    SlotItemIds[i] = 0; //Wave0write
                } //Wave0write
            } //Wave0write
        }

        private static void ApplyConsumableEffect(ItemDatat data, EntityBaset user) //Wave0write
        { //Wave0write
            if (data == null || user?.Stats == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (data.IsRetreat || data.SubCategory == "Escape" || data.ConsumeEffectKey == "Escape") //Wave0write
            { //Wave0write
                if (GameSystemManagert.TryGetInstance(out GameSystemManagert gsm)) //Wave0write
                { //Wave0write
                    gsm.EndCombat(CombatResultt.Retreat, null); //Wave0write
                } //Wave0write
                return; //Wave0write
            } //Wave0write

            int amount = UnityEngine.Mathf.RoundToInt(data.ParamValue); //Wave0write
            if (data.SubCategory == "MP_Potion" || data.ConsumeEffectKey == "HealMP") //Wave0write
            { //Wave0write
                user.Stats.CurrentMP = UnityEngine.Mathf.Min(user.Stats.MaxMP, user.Stats.CurrentMP + amount); //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                user.ApplyHeal(amount); //Wave0write
            } //Wave0write
        } //Wave0write
    }
}

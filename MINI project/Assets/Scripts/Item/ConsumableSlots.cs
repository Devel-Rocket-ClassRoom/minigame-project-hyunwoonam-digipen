namespace Tempt
{
    /// <summary>
    /// 전투 중 사용 가능한 소모 4칸. 인벤토리에서 미리 지정(전투 외에서만 변경 가능).
    /// </summary>
    public sealed class ConsumableSlots
    {
        /// <summary>슬롯 4칸의 아이템 ID(0=비어있음).</summary>
        public int[] SlotItemIds = new int[4];

        /// <summary>
        /// 슬롯 설정. 전투 중에는 호출 금지(UI와 도메인 양쪽에서 차단).
        /// </summary>
        public bool TrySetSlot(int slotIndex, int itemId, InventoryState inv)
        {
            if (slotIndex < 0 || slotIndex >= SlotItemIds.Length) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm)) //Wave0write
            { //Wave0write
                UnityEngine.Debug.LogError("[ConsumableSlots.TrySetSlot] GameSystemManager 참조가 없습니다."); //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (gsm.CombatContext != null || (gsm.Scenes != null && gsm.Scenes.CurrentSceneId == SceneId.Combat)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (itemId == 0) //Wave0write
            { //Wave0write
                SlotItemIds[slotIndex] = 0; //Wave0write
                gsm.Events?.RaiseInventoryChanged(); //Wave0write
                return true; //Wave0write
            } //Wave0write

            if (inv == null || inv.CountOf(itemId) <= 0) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (gsm.Data?.Items == null || !gsm.Data.Items.TryGetValue(itemId, out ItemData data)) //Wave0write
            { //Wave0write
                UnityEngine.Debug.LogError("[ConsumableSlots.TrySetSlot] 아이템 ID 없음: " + itemId); //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (data.Category != ItemCategory.Consumable) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            for (int i = 0; i < SlotItemIds.Length; i++) //Wave0write
            { //Wave0write
                if (i != slotIndex && SlotItemIds[i] == itemId) //Wave0write
                { //Wave0write
                    SlotItemIds[i] = 0; //Wave0write
                } //Wave0write
            } //Wave0write

            SlotItemIds[slotIndex] = itemId; //Wave0write
            gsm.Events?.RaiseInventoryChanged(); //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 전투 중 슬롯 사용. 행동 비용 없음(설계변경.txt).
        /// </summary>
        public bool TryUse(int slotIndex, EntityBase user, InventoryState inv)
        {
            if (slotIndex < 0 || slotIndex >= SlotItemIds.Length || user == null || inv == null) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            int itemId = SlotItemIds[slotIndex]; //Wave0write
            if (itemId == 0 || inv.CountOf(itemId) <= 0) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || !gsm.Data.Items.TryGetValue(itemId, out ItemData data)) //Wave0write
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
        public void PruneEmptySlots(InventoryState inv)
        {
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

        private static void ApplyConsumableEffect(ItemData data, EntityBase user) //Wave0write
        { //Wave0write
            if (data == null || user?.Stats == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (data.IsRetreat || data.SubCategory == "Escape" || data.ConsumeEffectKey == "Escape") //Wave0write
            { //Wave0write
                if (GameSystemManager.TryGetInstance(out GameSystemManager gsm)) //Wave0write
                { //Wave0write
                    gsm.EndCombat(CombatResult.Retreat, null); //Wave0write
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


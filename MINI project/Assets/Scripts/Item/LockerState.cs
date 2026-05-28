namespace Tempt
{
    /// <summary>
    /// 보관함. 주점에서 구매 후 활성. 인벤토리와 별도 저장.
    /// </summary>
    public sealed class LockerState : StackableContainer
    {
        /// <summary>활성화 여부(주점에서 구매 시 true).</summary>
        public bool Unlocked;

        /// <summary>주점에서 보관함 구매 시 활성화.</summary>
        public void Unlock()
        {
            // 동작 요약: Unlocked = true. 비활성 → 활성 1회만 유효.
            //TODO: if (Unlocked) return;
            //TODO: Unlocked = true;
            if (!Unlocked) //Wave0write
            { //Wave0write
                Unlocked = true; //Wave0write
            } //Wave0write
        }

        /// <summary>소모/재료 아이템 추가(InventoryState.MoveToLocker가 호출).</summary>
        public bool Add(int itemId, int count)
        {
            // 동작 요약: StackableItems[itemId] += count.
            //TODO: if (count <= 0) return;
            //TODO: if (StackableItems.ContainsKey(itemId)) StackableItems[itemId] += count;
            //TODO: else StackableItems[itemId] = count;
            if (count <= 0) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            bool added = AddStackCore(itemId, count); //Wave0write
            if (added) //Wave0write
            { //Wave0write
                RaiseInventoryChanged(); //Wave0write
            } //Wave0write

            return added; //Wave0write
        }

        /// <summary>장비 아이템 추가.</summary>
        public bool AddEquip(Item item)
        {
            // 동작 요약: EquipItems.Add(item).
            //TODO: EquipItems.Add(item);
            bool added = AddEquipCore(item); //Wave0write
            if (added) //Wave0write
            { //Wave0write
                RaiseInventoryChanged(); //Wave0write
            } //Wave0write

            return added; //Wave0write
        }

        /// <summary>소모/재료 아이템 꺼냄(수량 지정).</summary>
        public bool Remove(int itemId, int count)
        {
            // 동작 요약: 보유량 검사 후 StackableItems 차감. 0이하면 키 제거.
            //TODO: if (!StackableItems.TryGetValue(itemId, out int current)) return false;
            //TODO: if (current < count) return false;
            //TODO: int remaining = current - count;
            //TODO: if (remaining <= 0) StackableItems.Remove(itemId);
            //TODO: else StackableItems[itemId] = remaining;
            //TODO: return true;
            if (count <= 0 || !StackableItems.TryGetValue(itemId, out int current) || current < count) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            bool removed = RemoveStackCore(itemId, count); //Wave0write
            if (removed) //Wave0write
            { //Wave0write
                RaiseInventoryChanged(); //Wave0write
            } //Wave0write

            return removed; //Wave0write
        }

        /// <summary>장비 아이템 꺼냄.</summary>
        public bool RemoveEquip(Item item)
        {
            // 동작 요약: EquipItems.Remove(item).
            //TODO: return EquipItems.Remove(item);
            bool removed = RemoveEquipCore(item); //Wave0write
            if (removed) //Wave0write
            { //Wave0write
                RaiseInventoryChanged(); //Wave0write
            } //Wave0write

            return removed; //Wave0write
        }

        /// <summary>보유량 조회(소모/재료).</summary>
        public int CountOf(int itemId) //Wave0write
        { //Wave0write
            return CountStackCore(itemId); //Wave0write
        } //Wave0write

        /// <summary>
        /// 보관함 → 인벤토리로 소모/재료 이동(수량 지정). InventoryState.MoveFromLocker와 동일 흐름.
        /// 직접 호출하지 말고 InventoryState.MoveFromLocker()를 통해 사용.
        /// </summary>
        public bool MoveToInventory(InventoryState inventory, int itemId, int count)
        {
            // 동작 요약:
            // - Unlocked 검사.
            // - this.Remove(itemId, count) → inventory.Add(itemId, count).
            //TODO: if (!Unlocked) return false;
            //TODO: if (!Remove(itemId, count)) return false;
            //TODO: inventory.Add(itemId, count);
            //TODO: return true;
            if (!Unlocked || inventory == null || !Remove(itemId, count)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (!inventory.Add(itemId, count)) //Wave0write
            { //Wave0write
                Add(itemId, count); //Wave0write
                return false; //Wave0write
            } //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 보관함 → 인벤토리로 장비 이동.
        /// 직접 호출하지 말고 InventoryState.MoveEquipFromLocker()를 통해 사용.
        /// </summary>
        public bool MoveEquipToInventory(InventoryState inventory, Item item)
        {
            // 동작 요약:
            // - this.RemoveEquip(item) → inventory.AddEquip(item).
            //TODO: if (!Unlocked) return false;
            //TODO: if (!RemoveEquip(item)) return false;
            //TODO: inventory.AddEquip(item);
            //TODO: return true;
            if (!Unlocked || inventory == null || !RemoveEquip(item)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (!inventory.AddEquip(item)) //Wave0write
            { //Wave0write
                AddEquip(item); //Wave0write
                return false; //Wave0write
            } //Wave0write
            return true; //Wave0write
        }

        private static void RaiseInventoryChanged() //Wave0write
        { //Wave0write
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm)) //Wave0write
            { //Wave0write
                gsm.Events?.RaiseInventoryChanged(); //Wave0write
            } //Wave0write
        } //Wave0write
    }
}


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
            if (!Unlocked)
            {
                Unlocked = true;
            }
        }

        /// <summary>소모/재료 아이템 추가(InventoryState.MoveToLocker가 호출).</summary>
        public bool Add(int itemId, int count)
        {
            // 동작 요약: StackableItems[itemId] += count.
            if (count <= 0)
            {
                return false;
            }

            bool added = AddStackCore(itemId, count);
            if (added)
            {
                RaiseInventoryChanged();
            }

            return added;
        }

        /// <summary>장비 아이템 추가.</summary>
        public bool AddEquip(Item item)
        {
            // 동작 요약: EquipItems.Add(item).
            bool added = AddEquipCore(item);
            if (added)
            {
                RaiseInventoryChanged();
            }

            return added;
        }

        /// <summary>소모/재료 아이템 꺼냄(수량 지정).</summary>
        public bool Remove(int itemId, int count)
        {
            // 동작 요약: 보유량 검사 후 StackableItems 차감. 0이하면 키 제거.
            if (count <= 0 || !StackableItems.TryGetValue(itemId, out int current) || current < count)
            {
                return false;
            }

            bool removed = RemoveStackCore(itemId, count);
            if (removed)
            {
                RaiseInventoryChanged();
            }

            return removed;
        }

        /// <summary>장비 아이템 꺼냄.</summary>
        public bool RemoveEquip(Item item)
        {
            // 동작 요약: EquipItems.Remove(item).
            bool removed = RemoveEquipCore(item);
            if (removed)
            {
                RaiseInventoryChanged();
            }

            return removed;
        }

        /// <summary>보유량 조회(소모/재료).</summary>
        public int CountOf(int itemId)
        {
            return CountStackCore(itemId);
        }

        /// <summary>
        /// 보관함 → 인벤토리로 소모/재료 이동(수량 지정). InventoryState.MoveFromLocker와 동일 흐름.
        /// 직접 호출하지 말고 InventoryState.MoveFromLocker()를 통해 사용.
        /// </summary>
        public bool MoveToInventory(InventoryState inventory, int itemId, int count)
        {
            // 동작 요약:
            // - Unlocked 검사.
            // - this.Remove(itemId, count) → inventory.Add(itemId, count).
            if (!Unlocked || inventory == null || !Remove(itemId, count))
            {
                return false;
            }

            if (!inventory.Add(itemId, count))
            {
                Add(itemId, count);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 보관함 → 인벤토리로 장비 이동.
        /// 직접 호출하지 말고 InventoryState.MoveEquipFromLocker()를 통해 사용.
        /// </summary>
        public bool MoveEquipToInventory(InventoryState inventory, Item item)
        {
            // 동작 요약:
            // - this.RemoveEquip(item) → inventory.AddEquip(item).
            if (!Unlocked || inventory == null || !RemoveEquip(item))
            {
                return false;
            }

            if (!inventory.AddEquip(item))
            {
                AddEquip(item);
                return false;
            }
            return true;
        }

        private static void RaiseInventoryChanged()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseInventoryChanged();
            }
        }
    }
}


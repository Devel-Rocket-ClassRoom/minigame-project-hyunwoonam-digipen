namespace Tempt
{
    /// <summary>
    /// 플레이어 인벤토리. 보유 아이템(스택) + 골드/마석은 별도(GameRunState).
    /// 용량 제한: StackableItems는 슬롯 종류 수 기준, EquipItems는 개수 기준.
    /// </summary>
    public sealed class InventoryState : StackableContainer
    {
        /// <summary>소모/재료 아이템 슬롯 최대 종류 수.</summary>
        public const int MaxStackableSlots = 20;

        /// <summary>장비 아이템 최대 보관 개수.</summary>
        public const int MaxEquipSlots = 10;

        /// <summary>현재 사용 중인 소모/재료 슬롯 수(키 종류).</summary>
        public int UsedStackableSlots => StackableItems.Count;

        /// <summary>
        /// 소모/재료 슬롯이 가득 찼는가.
        /// 이미 보유 중인 itemId는 슬롯 추가 없이 수량만 늘므로 false.
        /// </summary>
        public bool IsStackableFull(int itemId)
        {
            // 동작 요약:
            // - StackableItems.ContainsKey(itemId) → 이미 있으면 false(수량만 추가 가능).
            // - StackableItems.Count >= MaxStackableSlots이면 true.
            if (!TryGetItemData(itemId, out ItemData itemData))
            {
                return true;
            }

            return !CanAddStack(itemData, 1);
        }

        /// <summary>장비 슬롯이 가득 찼는가.</summary>
        public bool IsEquipFull() => EquipItems.Count >= MaxEquipSlots;

        public bool CanAcceptStack(int itemId, int count)
        {
            return TryGetItemData(itemId, out ItemData itemData) && CanAddStack(itemData, count);
        }

        /// <summary>
        /// itemData 기준으로 인벤토리가 가득 찼는가.
        /// Stackable=true → IsStackableFull, false → IsEquipFull.
        /// </summary>
        public bool IsFull(ItemData itemData)
        {
            // 동작 요약:
            // - itemData.IsStackable → IsStackableFull(itemData.Id).
            // - 아니면 → IsEquipFull().
            if (itemData == null)
            {
                return true;
            }

            return itemData.Stackable ? !CanAddStack(itemData, 1) : IsEquipFull();
        }

        /// <summary>
        /// 소모/재료 아이템 추가 시도. 슬롯 초과 시 false 반환.
        /// </summary>
        public bool TryAdd(int itemId, int count)
        {
            // 동작 요약:
            // - IsStackableFull(itemId)이면 false.
            // - Add(itemId, count) 호출 후 true.
            if (count <= 0 || !TryGetItemData(itemId, out ItemData itemData) || !itemData.Stackable || !CanAddStack(itemData, count))
            {
                return false;
            }

            Add(itemId, count);
            return true;
        }

        /// <summary>
        /// 정적 데이터가 이미 확인된 스택 가능 아이템 추가 시도.
        /// Shop / 보상 처리처럼 DataManager 를 보유한 호출자가 사용한다.
        /// </summary>
        public bool TryAdd(ItemData itemData, int count)
        {
            if (count <= 0 || itemData == null || !itemData.Stackable || !CanAddStack(itemData, count))
            {
                return false;
            }

            Add(itemData, count);
            return true;
        }

        /// <summary>
        /// 장비 아이템 추가 시도. 슬롯 초과 시 false 반환.
        /// CombatRewardPaget이 오버플로우 감지에 사용.
        /// </summary>
        public bool TryAddEquip(Item item)
        {
            // 동작 요약:
            // - IsEquipFull()이면 false.
            // - AddEquip(item) 호출 후 true.
            if (item == null || IsEquipFull())
            {
                return false;
            }

            AddEquip(item);
            return true;
        }

        /// <summary>
        /// 소모/재료 아이템 추가.
        /// </summary>
        public bool Add(int itemId, int count)
        {
            // 동작 요약:
            // - count > 0 검사.
            // - StackableItems[itemId] += count, 새 키면 count로 시작.
            // - 데이터의 MaxStack 검사.
            // - EventBus.RaiseInventoryChanged().
            if (count <= 0)
            {
                return false;
            }

            if (!TryGetItemData(itemId, out ItemData itemData) || !itemData.Stackable || !CanAddStack(itemData, count))
            {
                return false;
            }

            AddStackCore(itemId, count);
            RaiseInventoryChanged();
            return true;
        }

        /// <summary>정적 데이터가 이미 확인된 소모/재료 아이템 추가.</summary>
        public bool Add(ItemData itemData, int count)
        {
            if (count <= 0 || itemData == null || !itemData.Stackable || !CanAddStack(itemData, count))
            {
                return false;
            }

            AddStackCore(itemData.Id, count);
            RaiseInventoryChanged();
            return true;
        }

        /// <summary>
        /// 장비 아이템 추가(드랍/해제/구매 등).
        /// </summary>
        public bool AddEquip(Item item)
        {
            // 동작 요약:
            // - EquipItems.Add(item).
            // - EventBus.RaiseInventoryChanged().
            if (item == null)
            {
                return false;
            }

            AddEquipCore(item);
            RaiseInventoryChanged();
            return true;
        }

        /// <summary>
        /// 소모/재료 아이템 차감. 부족 시 false.
        /// </summary>
        public bool Remove(int itemId, int count)
        {
            // 동작 요약: 보유량 검사 후 StackableItems 차감. 0이하면 키 제거.
            if (count <= 0 || !StackableItems.TryGetValue(itemId, out int current) || current < count)
            {
                return false;
            }

            RemoveStackCore(itemId, count);
            RaiseInventoryChanged();
            return true;
        }

        /// <summary>
        /// 장비 아이템 제거(장착/판매/폐기 시).
        /// </summary>
        public bool RemoveEquip(Item item)
        {
            // 동작 요약: EquipItems.Remove(item). EventBus.RaiseInventoryChanged().
            bool removed = RemoveEquipCore(item);
            if (removed)
            {
                RaiseInventoryChanged();
            }

            return removed;
        }

        /// <summary>
        /// 소모/재료 아이템 폐기(수량 지정). UI에서 수량 입력 후 호출.
        /// </summary>
        public bool Discard(int itemId, int count)
        {
            // 동작 요약:
            // - count > 0 && count <= StackableItems[itemId] 검사.
            // - Remove(itemId, count) 호출.
            // - EventBus.RaiseInventoryChanged().
            return Remove(itemId, count);
        }

        /// <summary>
        /// 장비 아이템 폐기.
        /// </summary>
        public bool DiscardEquip(Item item)
        {
            // 동작 요약: RemoveEquip(item).
            return RemoveEquip(item);
        }

        /// <summary>
        /// 보유량 조회(소모/재료).
        /// </summary>
        public int CountOf(int itemId)
        {
            // 동작 요약: StackableItems.TryGetValue 반환.
            return CountStackCore(itemId);
        }

        /// <summary>
        /// 인벤토리 → 보관함으로 아이템 이동(수량 지정). UI에서 수량 입력 후 호출.
        /// </summary>
        public bool MoveToLocker(LockerState locker, int itemId, int count)
        {
            // 동작 요약:
            // - locker.Unlocked 검사, 잠금 시 false 반환.
            // - this.Remove(itemId, count) → locker.Add(itemId, count).
            if (locker == null || !locker.Unlocked || !Remove(itemId, count))
            {
                return false;
            }

            if (!locker.Add(itemId, count))
            {
                Add(itemId, count);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 인벤토리 → 보관함으로 장비 이동.
        /// </summary>
        public bool MoveEquipToLocker(LockerState locker, Item item)
        {
            // 동작 요약:
            // - locker.Unlocked 검사.
            // - this.RemoveEquip(item) → locker.AddEquip(item).
            if (locker == null || !locker.Unlocked || !RemoveEquip(item))
            {
                return false;
            }

            if (!locker.AddEquip(item))
            {
                AddEquip(item);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 보관함 → 인벤토리로 아이템 이동(수량 지정). UI에서 수량 입력 후 호출.
        /// </summary>
        public bool MoveFromLocker(LockerState locker, int itemId, int count)
        {
            // 동작 요약:
            // - locker.Unlocked 검사.
            // - locker.Remove(itemId, count) → this.Add(itemId, count).
            if (locker == null || !locker.Unlocked || count <= 0 || !TryGetItemData(itemId, out ItemData itemData) || !CanAddStack(itemData, count) || !locker.Remove(itemId, count))
            {
                return false;
            }

            if (!Add(itemId, count))
            {
                locker.Add(itemId, count);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 보관함 → 인벤토리로 장비 이동.
        /// </summary>
        public bool MoveEquipFromLocker(LockerState locker, Item item)
        {
            // 동작 요약:
            // - locker.RemoveEquip(item) → this.AddEquip(item).
            if (locker == null || !locker.Unlocked || IsEquipFull() || !locker.RemoveEquip(item))
            {
                return false;
            }

            if (!AddEquip(item))
            {
                locker.AddEquip(item);
                return false;
            }
            return true;
        }

        private bool CanAddStack(ItemData itemData, int count)
        {
            if (itemData == null || !itemData.Stackable || count <= 0)
            {
                return false;
            }

            int current = CountOf(itemData.Id);
            if (current <= 0 && StackableItems.Count >= MaxStackableSlots)
            {
                return false;
            }

            int maxStack = System.Math.Max(1, itemData.MaxStack);
            return current + count <= maxStack;
        }

        private static bool TryGetItemData(int itemId, out ItemData itemData)
        {
            itemData = null;
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.Data?.Items == null)
            {
                GameLog.LogError("[InventoryState] DataManager.Items 참조가 없습니다.");
                return false;
            }

            if (!gsm.Data.Items.TryGetValue(itemId, out itemData) || itemData == null)
            {
                GameLog.LogError("[InventoryState] 아이템 ID 없음: " + itemId);
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


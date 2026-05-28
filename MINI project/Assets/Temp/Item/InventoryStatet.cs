using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 플레이어 인벤토리. 보유 아이템(스택) + 골드/마석은 별도(GameRunStatet).
    /// 용량 제한: StackableItems는 슬롯 종류 수 기준, EquipItems는 개수 기준.
    /// </summary>
    public sealed class InventoryStatet
    {
        /// <summary>소모/재료 아이템 슬롯 최대 종류 수.</summary>
        public const int MaxStackableSlots = 20;

        /// <summary>장비 아이템 최대 보관 개수.</summary>
        public const int MaxEquipSlots = 10;

        /// <summary>아이템 ID → 보유 개수(소모품/재료).</summary>
        public Dictionary<int, int> StackableItems = new Dictionary<int, int>();

        /// <summary>
        /// 인벤토리에 보관 중인 장비 인스턴스 목록(강화 단계 포함).
        /// 장비는 Stackable=false이므로 개별 Itemt 인스턴스로 관리.
        /// </summary>
        public List<Itemt> EquipItems = new List<Itemt>();

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
            //TODO: if (StackableItems.ContainsKey(itemId)) return false;
            //TODO: return StackableItems.Count >= MaxStackableSlots;
            if (StackableItems.ContainsKey(itemId)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            return StackableItems.Count >= MaxStackableSlots; //Wave0write
        }

        /// <summary>장비 슬롯이 가득 찼는가.</summary>
        public bool IsEquipFull() => EquipItems.Count >= MaxEquipSlots;

        /// <summary>
        /// itemData 기준으로 인벤토리가 가득 찼는가.
        /// Stackable=true → IsStackableFull, false → IsEquipFull.
        /// </summary>
        public bool IsFull(ItemDatat itemData)
        {
            // 동작 요약:
            // - itemData.IsStackable → IsStackableFull(itemData.Id).
            // - 아니면 → IsEquipFull().
            //TODO: if (itemData.IsStackable) return IsStackableFull(itemData.Id);
            //TODO: return IsEquipFull();
            if (itemData == null) //Wave0write
            { //Wave0write
                return true; //Wave0write
            } //Wave0write

            return itemData.Stackable ? IsStackableFull(itemData.Id) : IsEquipFull(); //Wave0write
        }

        /// <summary>
        /// 소모/재료 아이템 추가 시도. 슬롯 초과 시 false 반환.
        /// </summary>
        public bool TryAdd(int itemId, int count)
        {
            // 동작 요약:
            // - IsStackableFull(itemId)이면 false.
            // - Add(itemId, count) 호출 후 true.
            //TODO: if (IsStackableFull(itemId)) return false;
            //TODO: Add(itemId, count);
            //TODO: return true;
            if (count <= 0 || IsStackableFull(itemId)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            Add(itemId, count); //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 장비 아이템 추가 시도. 슬롯 초과 시 false 반환.
        /// CombatRewardPaget이 오버플로우 감지에 사용.
        /// </summary>
        public bool TryAddEquip(Itemt item)
        {
            // 동작 요약:
            // - IsEquipFull()이면 false.
            // - AddEquip(item) 호출 후 true.
            //TODO: if (IsEquipFull()) return false;
            //TODO: AddEquip(item);
            //TODO: return true;
            if (item == null || IsEquipFull()) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            AddEquip(item); //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 소모/재료 아이템 추가.
        /// </summary>
        public void Add(int itemId, int count)
        {
            // 동작 요약:
            // - count > 0 검사.
            // - StackableItems[itemId] += count, 새 키면 count로 시작.
            // - 데이터의 MaxStack 검사.
            // - EventBust.RaiseInventoryChanged().
            //TODO: if (count <= 0) return;
            //TODO: if (StackableItems.ContainsKey(itemId)) StackableItems[itemId] += count;
            //TODO: else StackableItems[itemId] = count;
            //TODO: EventBust.RaiseInventoryChanged();
            if (count <= 0) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (StackableItems.ContainsKey(itemId)) //Wave0write
            { //Wave0write
                StackableItems[itemId] += count; //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                StackableItems[itemId] = count; //Wave0write
            } //Wave0write

            RaiseInventoryChanged(); //Wave0write
        }

        /// <summary>
        /// 장비 아이템 추가(드랍/해제/구매 등).
        /// </summary>
        public void AddEquip(Itemt item)
        {
            // 동작 요약:
            // - EquipItems.Add(item).
            // - EventBust.RaiseInventoryChanged().
            //TODO: EquipItems.Add(item);
            //TODO: EventBust.RaiseInventoryChanged();
            if (item == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            EquipItems.Add(item); //Wave0write
            RaiseInventoryChanged(); //Wave0write
        }

        /// <summary>
        /// 소모/재료 아이템 차감. 부족 시 false.
        /// </summary>
        public bool Remove(int itemId, int count)
        {
            // 동작 요약: 보유량 검사 후 StackableItems 차감. 0이하면 키 제거.
            //TODO: if (!StackableItems.TryGetValue(itemId, out int current)) return false;
            //TODO: if (current < count) return false;
            //TODO: int remaining = current - count;
            //TODO: if (remaining <= 0) StackableItems.Remove(itemId);
            //TODO: else StackableItems[itemId] = remaining;
            //TODO: EventBust.RaiseInventoryChanged();
            //TODO: return true;
            if (count <= 0 || !StackableItems.TryGetValue(itemId, out int current) || current < count) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            int remaining = current - count; //Wave0write
            if (remaining <= 0) //Wave0write
            { //Wave0write
                StackableItems.Remove(itemId); //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                StackableItems[itemId] = remaining; //Wave0write
            } //Wave0write

            RaiseInventoryChanged(); //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 장비 아이템 제거(장착/판매/폐기 시).
        /// </summary>
        public bool RemoveEquip(Itemt item)
        {
            // 동작 요약: EquipItems.Remove(item). EventBust.RaiseInventoryChanged().
            //TODO: bool removed = EquipItems.Remove(item);
            //TODO: if (removed) EventBust.RaiseInventoryChanged();
            //TODO: return removed;
            bool removed = EquipItems.Remove(item); //Wave0write
            if (removed) //Wave0write
            { //Wave0write
                RaiseInventoryChanged(); //Wave0write
            } //Wave0write

            return removed; //Wave0write
        }

        /// <summary>
        /// 소모/재료 아이템 폐기(수량 지정). UI에서 수량 입력 후 호출.
        /// </summary>
        public bool Discard(int itemId, int count)
        {
            // 동작 요약:
            // - count > 0 && count <= StackableItems[itemId] 검사.
            // - Remove(itemId, count) 호출.
            // - EventBust.RaiseInventoryChanged().
            //TODO: if (count <= 0) return false;
            //TODO: if (!StackableItems.TryGetValue(itemId, out int current)) return false;
            //TODO: if (count > current) return false;
            //TODO: bool success = Remove(itemId, count);
            //TODO: if (success) EventBust.RaiseInventoryChanged();
            //TODO: return success;
            return Remove(itemId, count); //Wave0write
        }

        /// <summary>
        /// 장비 아이템 폐기.
        /// </summary>
        public bool DiscardEquip(Itemt item)
        {
            // 동작 요약: RemoveEquip(item).
            //TODO: return RemoveEquip(item);
            return RemoveEquip(item); //Wave0write
        }

        /// <summary>
        /// 보유량 조회(소모/재료).
        /// </summary>
        public int CountOf(int itemId)
        {
            // 동작 요약: StackableItems.TryGetValue 반환.
            //TODO: StackableItems.TryGetValue(itemId, out int count);
            //TODO: return count;
            return StackableItems.TryGetValue(itemId, out int count) ? count : 0; //Wave0write
        }

        /// <summary>
        /// 인벤토리 → 보관함으로 아이템 이동(수량 지정). UI에서 수량 입력 후 호출.
        /// </summary>
        public bool MoveToLocker(LockerStatet locker, int itemId, int count)
        {
            // 동작 요약:
            // - locker.Unlocked 검사, 잠금 시 false 반환.
            // - this.Remove(itemId, count) → locker.Add(itemId, count).
            //TODO: if (!locker.Unlocked) return false;
            //TODO: if (!Remove(itemId, count)) return false;
            //TODO: locker.Add(itemId, count);
            //TODO: return true;
            if (locker == null || !locker.Unlocked || !Remove(itemId, count)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            locker.Add(itemId, count); //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 인벤토리 → 보관함으로 장비 이동.
        /// </summary>
        public bool MoveEquipToLocker(LockerStatet locker, Itemt item)
        {
            // 동작 요약:
            // - locker.Unlocked 검사.
            // - this.RemoveEquip(item) → locker.AddEquip(item).
            //TODO: if (!locker.Unlocked) return false;
            //TODO: if (!RemoveEquip(item)) return false;
            //TODO: locker.AddEquip(item);
            //TODO: return true;
            if (locker == null || !locker.Unlocked || !RemoveEquip(item)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            locker.AddEquip(item); //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 보관함 → 인벤토리로 아이템 이동(수량 지정). UI에서 수량 입력 후 호출.
        /// </summary>
        public bool MoveFromLocker(LockerStatet locker, int itemId, int count)
        {
            // 동작 요약:
            // - locker.Unlocked 검사.
            // - locker.Remove(itemId, count) → this.Add(itemId, count).
            //TODO: if (!locker.Unlocked) return false;
            //TODO: if (!locker.Remove(itemId, count)) return false;
            //TODO: Add(itemId, count);
            //TODO: return true;
            if (locker == null || !locker.Unlocked || IsStackableFull(itemId) || !locker.Remove(itemId, count)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            Add(itemId, count); //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 보관함 → 인벤토리로 장비 이동.
        /// </summary>
        public bool MoveEquipFromLocker(LockerStatet locker, Itemt item)
        {
            // 동작 요약:
            // - locker.RemoveEquip(item) → this.AddEquip(item).
            //TODO: if (!locker.Unlocked) return false;
            //TODO: if (!locker.RemoveEquip(item)) return false;
            //TODO: AddEquip(item);
            //TODO: return true;
            if (locker == null || !locker.Unlocked || IsEquipFull() || !locker.RemoveEquip(item)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            AddEquip(item); //Wave0write
            return true; //Wave0write
        }

        private static void RaiseInventoryChanged() //Wave0write
        { //Wave0write
            if (GameSystemManagert.TryGetInstance(out GameSystemManagert gsm)) //Wave0write
            { //Wave0write
                gsm.Events?.RaiseInventoryChanged(); //Wave0write
            } //Wave0write
        } //Wave0write
    }
}

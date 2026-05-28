using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 보관함. 주점에서 구매 후 활성. 인벤토리와 별도 저장.
    /// </summary>
    public sealed class LockerStatet
    {
        /// <summary>활성화 여부(주점에서 구매 시 true).</summary>
        public bool Unlocked;

        /// <summary>보관 중인 소모/재료 아이템(itemId → 수량).</summary>
        public Dictionary<int, int> StackableItems = new Dictionary<int, int>();

        /// <summary>보관 중인 장비 아이템 목록(강화 단계 포함).</summary>
        public List<Itemt> EquipItems = new List<Itemt>();

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

        /// <summary>소모/재료 아이템 추가(InventoryStatet.MoveToLocker가 호출).</summary>
        public void Add(int itemId, int count)
        {
            // 동작 요약: StackableItems[itemId] += count.
            //TODO: if (count <= 0) return;
            //TODO: if (StackableItems.ContainsKey(itemId)) StackableItems[itemId] += count;
            //TODO: else StackableItems[itemId] = count;
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
        }

        /// <summary>장비 아이템 추가.</summary>
        public void AddEquip(Itemt item)
        {
            // 동작 요약: EquipItems.Add(item).
            //TODO: EquipItems.Add(item);
            if (item != null) //Wave0write
            { //Wave0write
                EquipItems.Add(item); //Wave0write
            } //Wave0write
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

            int remaining = current - count; //Wave0write
            if (remaining <= 0) //Wave0write
            { //Wave0write
                StackableItems.Remove(itemId); //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                StackableItems[itemId] = remaining; //Wave0write
            } //Wave0write

            return true; //Wave0write
        }

        /// <summary>장비 아이템 꺼냄.</summary>
        public bool RemoveEquip(Itemt item)
        {
            // 동작 요약: EquipItems.Remove(item).
            //TODO: return EquipItems.Remove(item);
            return EquipItems.Remove(item); //Wave0write
        }

        /// <summary>
        /// 보관함 → 인벤토리로 소모/재료 이동(수량 지정). InventoryStatet.MoveFromLocker와 동일 흐름.
        /// 직접 호출하지 말고 InventoryStatet.MoveFromLocker()를 통해 사용.
        /// </summary>
        public bool MoveToInventory(InventoryStatet inventory, int itemId, int count)
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

            inventory.Add(itemId, count); //Wave0write
            return true; //Wave0write
        }

        /// <summary>
        /// 보관함 → 인벤토리로 장비 이동.
        /// 직접 호출하지 말고 InventoryStatet.MoveEquipFromLocker()를 통해 사용.
        /// </summary>
        public bool MoveEquipToInventory(InventoryStatet inventory, Itemt item)
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

            inventory.AddEquip(item); //Wave0write
            return true; //Wave0write
        }
    }
}

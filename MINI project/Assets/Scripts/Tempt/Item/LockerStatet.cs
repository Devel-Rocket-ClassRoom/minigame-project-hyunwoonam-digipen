using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 보관함. 주점에서 구매 후 활성. 인벤토리와 별도 저장.
    /// </summary>
    public sealed class LockerStatet
    {
        /// <summary>활성화 여부.</summary>
        public bool Unlocked;

        /// <summary>보관 아이템.</summary>
        public Dictionary<int, int> Items = new Dictionary<int, int>();

        /// <summary>주점에서 보관함 구매 시 활성화.</summary>
        public void Unlock()
        {
            // 동작 요약: Unlocked = true. 비활성 → 활성 1회만 유효.
        }

        /// <summary>보관함에 추가(InventoryStatet.MoveToLocker가 호출).</summary>
        public void Add(int itemId, int count)
        {
            // 동작 요약: 보유량 추가.
        }

        /// <summary>보관함에서 꺼냄.</summary>
        public bool Remove(int itemId, int count)
        {
            // 동작 요약: 보유량 차감.
            return false;
        }
    }
}

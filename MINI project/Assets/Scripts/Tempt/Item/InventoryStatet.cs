using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 플레이어 인벤토리. 보유 아이템(스택) + 골드/마석은 별도(GameRunStatet).
    /// </summary>
    public sealed class InventoryStatet
    {
        /// <summary>아이템 ID → 보유 개수.</summary>
        public Dictionary<int, int> Items = new Dictionary<int, int>();

        /// <summary>
        /// 아이템 추가.
        /// </summary>
        public void Add(int itemId, int count)
        {
            // 동작 요약:
            // - count > 0 검사.
            // - Items[itemId] += count, 새 키면 count로 시작.
            // - 데이터의 MaxStack 검사.
            // - EventBust.RaiseInventoryChanged().
        }

        /// <summary>
        /// 아이템 차감. 부족 시 false.
        /// </summary>
        public bool Remove(int itemId, int count)
        {
            // 동작 요약: 보유량 검사 후 차감.
            return false;
        }

        /// <summary>
        /// 보유량 조회.
        /// </summary>
        public int CountOf(int itemId)
        {
            // 동작 요약: Items.TryGetValue 반환.
            return 0;
        }

        /// <summary>
        /// 보관함으로 이동.
        /// </summary>
        public bool MoveToLocker(LockerStatet locker, int itemId, int count)
        {
            // 동작 요약:
            // - locker.Unlocked 검사.
            // - this.Remove + locker.Add.
            return false;
        }
    }
}

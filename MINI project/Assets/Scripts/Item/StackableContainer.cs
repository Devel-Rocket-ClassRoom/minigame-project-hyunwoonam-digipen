using System.Collections.Generic;

namespace Tempt
{
    // Guid2 §6 2026-05-27: 인벤토리 / 보관함의 공통 저장소.
    // 본 클래스는 이벤트 발행을 하지 않는다. 이벤트 발행은 InventoryState/LockerState 가 책임.
    /// <summary>
    /// 스택 가능 아이템(itemId → count) + 장비 인스턴스 목록의 공통 저장소.
    /// InventoryState / LockerState 의 공통 부모.
    /// </summary>
    public abstract class StackableContainer
    {
        /// <summary>아이템 ID → 보유 수량(소비/재료).</summary>
        public Dictionary<int, int> StackableItems { get; protected set; }

        /// <summary>장비 인스턴스 목록(강화 단계 포함).</summary>
        public List<Item> EquipItems { get; protected set; }

        /// <summary>
        /// 초기 빈 컨테이너를 생성한다. 파생 클래스의 ctor 에서 호출.
        /// </summary>
        protected StackableContainer()
        {
            // 동작 요약: 두 컬렉션을 빈 인스턴스로 초기화.
            // Guid2 §6.3 참고.
            StackableItems = new Dictionary<int, int>();
            EquipItems = new List<Item>();
        }

        /// <summary>파생 클래스가 호출할 스택 추가 공통 처리. count &lt;= 0 이면 false.</summary>
        protected bool AddStackCore(int itemId, int count)
        {
            if (itemId <= 0 || count <= 0)
            {
                return false;
            }

            if (StackableItems.TryGetValue(itemId, out int current))
            {
                StackableItems[itemId] = current + count;
            }
            else
            {
                StackableItems[itemId] = count;
            }

            return true;
        }

        /// <summary>파생 클래스가 호출할 스택 차감 공통 처리. 부족하면 false.</summary>
        protected bool RemoveStackCore(int itemId, int count)
        {
            if (itemId <= 0 || count <= 0 || !StackableItems.TryGetValue(itemId, out int current) || current < count)
            {
                return false;
            }

            int remaining = current - count;
            if (remaining <= 0)
            {
                StackableItems.Remove(itemId);
            }
            else
            {
                StackableItems[itemId] = remaining;
            }

            return true;
        }

        /// <summary>파생 클래스가 호출할 장비 추가 공통 처리. item null 이면 false.</summary>
        protected bool AddEquipCore(Item item)
        {
            if (item == null)
            {
                return false;
            }

            EquipItems.Add(item);
            return true;
        }

        /// <summary>파생 클래스가 호출할 장비 제거 공통 처리. 존재하지 않으면 false.</summary>
        protected bool RemoveEquipCore(Item item)
        {
            return item != null && EquipItems.Remove(item);
        }

        /// <summary>해당 아이템 보유량.</summary>
        protected int CountStackCore(int itemId)
        {
            return StackableItems.TryGetValue(itemId, out int count) ? count : 0;
        }
    }
}

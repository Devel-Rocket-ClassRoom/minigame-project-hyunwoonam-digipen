using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 드랍 테이블 1행. DropTable.csv 1행에 대응.
    /// 같은 DropTableId를 가진 여러 행이 하나의 드랍 그룹을 구성한다.
    /// </summary>
    public sealed class DropEntry : DataTable
    {
        /// <summary>드랍 그룹 식별자(MonsterData.DropTableId와 매핑).</summary>
        public int DropTableId;

        /// <summary>드랍될 아이템 ID.</summary>
        public int ItemId;

        /// <summary>한 번 드랍 시 최소 개수.</summary>
        public int MinCount;

        /// <summary>한 번 드랍 시 최대 개수.</summary>
        public int MaxCount;

        /// <summary>드랍 확률 (0.0 ~ 1.0).</summary>
        public float DropRate;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            // 동작 요약:
            // - cells[0] = DropTableId (int).
            // - cells[1] = ItemId (int).
            // - cells[2] = MinCount (int).
            // - cells[3] = MaxCount (int).
            // - cells[4] = DropRate (float).
            //TODO: DropTableId = int.Parse(cells[0]);
            //TODO: ItemId      = int.Parse(cells[1]);
            //TODO: MinCount    = int.Parse(cells[2]);
            //TODO: MaxCount    = int.Parse(cells[3]);
            //TODO: DropRate    = float.Parse(cells[4]);
            //TODO: Id = DropTableId * 1000 + ItemId; // 복합키(Id 필드 활용)
        }
    }

    /// <summary>
    /// 드랍 해결 결과 단위. DataManager.ResolveDrops() 반환 원소.
    /// </summary>
    public sealed class DroppedItemStack
    {
        /// <summary>드랍된 아이템 ID.</summary>
        public int ItemId;

        /// <summary>드랍 수량(MinCount~MaxCount 범위 무작위).</summary>
        public int Count;
    }
}


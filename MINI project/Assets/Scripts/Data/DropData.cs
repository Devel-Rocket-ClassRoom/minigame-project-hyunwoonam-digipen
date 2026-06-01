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
            Id = cells.Length > 0 && CsvParser.TryParseInt(cells[0], out int id) ? id : 0;
            DropTableId = cells.Length > 1 && CsvParser.TryParseInt(cells[1], out int tableId) ? tableId : 0;
            ItemId = cells.Length > 2 && CsvParser.TryParseInt(cells[2], out int itemId) ? itemId : 0;
            MinCount = cells.Length > 3 && CsvParser.TryParseInt(cells[3], out int minCount) ? minCount : 1;
            MaxCount = cells.Length > 4 && CsvParser.TryParseInt(cells[4], out int maxCount) ? maxCount : MinCount;
            DropRate = cells.Length > 5 && CsvParser.TryParseFloat(cells[5], out float rate) ? rate : 0f;
        }

        public static DropEntry FromRow(IDictionary<string, string> row)
        {
            if (!CsvParser.HasColumns(row, nameof(DropEntry), "Id", "DropTableId", "ItemId", "MinCount", "MaxCount", "DropRate"))
            {
                return null;
            }

            return new DropEntry
            {
                Id = CsvParser.GetInt(row, "Id"),
                DropTableId = CsvParser.GetInt(row, "DropTableId"),
                ItemId = CsvParser.GetInt(row, "ItemId"),
                MinCount = CsvParser.GetInt(row, "MinCount", 1),
                MaxCount = CsvParser.GetInt(row, "MaxCount", 1),
                DropRate = CsvParser.GetFloat(row, "DropRate"),
            };
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


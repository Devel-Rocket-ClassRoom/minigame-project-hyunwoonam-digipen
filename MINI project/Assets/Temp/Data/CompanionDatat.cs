using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 동료 정적 데이터. 모집 조건, 직업, 기본 스탯, 트리 풀.
    /// </summary>
    public sealed class CompanionDatat : DataTablet
    {
        /// <summary>모집 해금에 필요한 최저 도달 층.</summary>
        public int RequiredFloor;

        /// <summary>주점 모집 가격(골드).</summary>
        public int RecruitPrice;

        /// <summary>직업.</summary>
        public RuneClasst ClassId;

        /// <summary>레벨 1 기본 스탯.</summary>
        public EquipmentStatModt BaseStats;

        /// <summary>레벨업 시 증가량.</summary>
        public EquipmentStatModt StatGrowth;

        /// <summary>고정 트리 생성에 사용할 노드 풀(시작 룬 제외).</summary>
        public List<int> RuneNodePool;

        /// <summary>고정 트리 길이(레벨업 가능 한도).</summary>
        public int RuneTreeLength;

        /// <summary>프리팹 키.</summary>
        public string PrefabKey;

        /// <summary>행동 우선순위 규칙 ID(직업별 규칙 참조).</summary>
        public string ActionRuleKey;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            // 동작 요약: 필드 파싱.
            //TODO: Id             = int.Parse(cells[0]);
            //TODO: NameKey        = cells[1];
            //TODO: ClassId        = (RuneClasst)System.Enum.Parse(typeof(RuneClasst), cells[2]);
            //TODO: ActionRuleKey  = cells[3];
            //TODO: RequiredFloor  = int.Parse(cells[4]);
            //TODO: RecruitPrice   = int.Parse(cells[5]);
            //TODO: BaseStats      = new EquipmentStatModt { HP = int.Parse(cells[6]), ATK = int.Parse(cells[7]), DEF = int.Parse(cells[8]), SPD = int.Parse(cells[9]) };
            //TODO: StatGrowth     = new EquipmentStatModt { HP = int.Parse(cells[10]), ATK = int.Parse(cells[11]), DEF = int.Parse(cells[12]), SPD = int.Parse(cells[13]) };
            //TODO: RuneTreeLength = int.Parse(cells[14]);
            //TODO: PrefabKey      = cells[15];
            //TODO: // RuneNodePool: ';' 구분 int 목록
            //TODO: RuneNodePool = new List<int>();
            //TODO: if (cells.Length > 16 && !string.IsNullOrEmpty(cells[16]))
            //TODO:     foreach (var s in cells[16].Split(';')) RuneNodePool.Add(int.Parse(s.Trim()));
        }
    }
}

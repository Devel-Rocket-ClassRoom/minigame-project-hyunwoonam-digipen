using System.Collections.Generic;

/// <summary>
/// 동료 정적 데이터. 모집 조건, 직업, 기본 스탯, 트리 풀.
/// </summary>
public sealed class CompanionData : DataTable
{
    /// <summary>모집 해금에 필요한 최저 도달 층.</summary>
    public int RequiredFloor;

    /// <summary>주점 모집 가격(골드).</summary>
    public int RecruitPrice;

    /// <summary>직업.</summary>
    public RuneClass ClassId;

    /// <summary>레벨 1 기본 스탯.</summary>
    public EquipmentStatMod BaseStats;

    /// <summary>레벨업 시 증가량.</summary>
    public EquipmentStatMod StatGrowth;

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
        Id = cells.Length > 0 && CsvParser.TryParseInt(cells[0], out int id) ? id : 0;
        NameKey = cells.Length > 1 ? cells[1] : string.Empty;
        RequiredFloor = cells.Length > 2 && CsvParser.TryParseInt(cells[2], out int floor) ? floor : 0;
        RecruitPrice = cells.Length > 3 && CsvParser.TryParseInt(cells[3], out int price) ? price : 0;
    }

    public static CompanionData FromRow(IDictionary<string, string> row)
    {
        if (!CsvParser.HasColumns(row, nameof(CompanionData), "Id", "NameKey"))
        {
            return null;
        }

        return new CompanionData
        {
            Id = CsvParser.GetInt(row, "Id"),
            NameKey = CsvParser.GetString(row, "NameKey"),
            DescKey = CsvParser.GetString(row, "DescKey"),
            RequiredFloor = CsvParser.GetInt(row, "RequiredFloor"),
            RecruitPrice = CsvParser.GetInt(row, "RecruitPrice"),
            ClassId = CsvParser.GetEnum(row, "ClassId", RuneClass.None),
            BaseStats = new EquipmentStatMod
            {
                HP = CsvParser.GetInt(row, "BaseStats_HP"),
                MP = CsvParser.GetInt(row, "BaseStats_MP"),
                ATK = CsvParser.GetInt(row, "BaseStats_ATK"),
                DEF = CsvParser.GetInt(row, "BaseStats_DEF"),
                SPD = CsvParser.GetInt(row, "BaseStats_SPD"),
            },
            StatGrowth = new EquipmentStatMod
            {
                HP = CsvParser.GetInt(row, "StatGrowth_HP"),
                MP = CsvParser.GetInt(row, "StatGrowth_MP"),
                ATK = CsvParser.GetInt(row, "StatGrowth_ATK"),
                DEF = CsvParser.GetInt(row, "StatGrowth_DEF"),
                SPD = CsvParser.GetInt(row, "StatGrowth_SPD"),
            },
            RuneNodePool = CsvParser.GetIntList(row, "RuneNodePool"),
            RuneTreeLength = CsvParser.GetInt(row, "RuneTreeLength"),
            PrefabKey = CsvParser.GetString(row, "PrefabKey"),
            ActionRuleKey = CsvParser.GetString(row, "ActionRuleKey"),
        };
    }
}


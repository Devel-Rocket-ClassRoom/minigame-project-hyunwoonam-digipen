using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 정적 데이터(CSV/JSON) 일괄 로드 및 조회. 런타임에 데이터를 수정하지 않는다.
/// </summary>
public sealed partial class DataManager
{
    /// <summary>몬스터 테이블.</summary>
    public IReadOnlyDictionary<int, MonsterData> Monsters => monsters;

    /// <summary>스킬 테이블.</summary>
    public IReadOnlyDictionary<int, SkillData> Skills => skills;

    /// <summary>룬 직업별 액티브 스킬 보정 테이블.</summary>
    public IReadOnlyDictionary<int, SkillRuneModifierData> SkillRuneModifiers => skillRuneModifiers;

    /// <summary>아이템 테이블.</summary>
    public IReadOnlyDictionary<int, ItemData> Items => items;

    /// <summary>룬 테이블.</summary>
    public IReadOnlyDictionary<int, RuneData> Runes => runes;

    /// <summary>월드/노드/단계 설정.</summary>
    public WorldData World => world;

    /// <summary>동료 모집/기본 데이터.</summary>
    public IReadOnlyDictionary<int, CompanionData> Companions => companions;

    /// <summary>밸런스 곡선(EXP, 침식, 가격 인플레이션).</summary>
    public BalanceData Balance => balance;

    /// <summary>언어 리소스.</summary>
    public LanguageData Language => language;

    /// <summary>
    /// 드랍 테이블. 키 = DropTableId, 값 = 해당 그룹 항목 목록.
    /// MonsterData.DropTableId == 0 이면 이 테이블에 항목 없음.
    /// </summary>
    public IReadOnlyDictionary<int, List<DropEntry>> DropTables => dropTables;

    private Dictionary<int, MonsterData> monsters;
    private Dictionary<int, SkillData> skills;
    private Dictionary<int, SkillRuneModifierData> skillRuneModifiers;
    private Dictionary<int, ItemData> items;
    private Dictionary<int, RuneData> runes;
    private WorldData world;
    private Dictionary<int, CompanionData> companions;
    private BalanceData balance;
    private LanguageData language;
    private Dictionary<int, List<DropEntry>> dropTables;

    /// <summary>
    /// 모든 데이터 파일을 일괄 로드.
    /// </summary>
    public void LoadAll()
    {
        monsters = ToDictionary(
            CsvParser.Parse("Tables/MonsterStatusTable", MonsterData.FromRow),
            BuildFallbackMonsters(),
            "MonsterStatusTable"
        );
        skills = ToDictionary(
            CsvParser.Parse("Tables/SkillTable", SkillData.FromRow),
            BuildFallbackSkills(),
            "SkillTable"
        );
        skillRuneModifiers = ToDictionary(
            CsvParser.Parse("Tables/SkillRuneModifierTable", SkillRuneModifierData.FromRow),
            BuildFallbackSkillRuneModifiers(),
            "SkillRuneModifierTable",
            true
        );
        items = ToDictionary(
            CsvParser.Parse("Tables/ItemTable", ItemData.FromRow),
            BuildFallbackItems(),
            "ItemTable"
        );
        runes = ToDictionary(
            CsvParser.Parse("Tables/RuneTable", RuneData.FromRow),
            BuildFallbackRunes(),
            "RuneTable"
        );
        companions = ToDictionary(
            CsvParser.Parse("Tables/Companions", CompanionData.FromRow),
            new Dictionary<int, CompanionData>(),
            "Companions",
            true
        );
        dropTables = ToDropTableDictionary(
            CsvParser.Parse("Tables/DropTable", DropEntry.FromRow),
            BuildFallbackDropTables(),
            "DropTable"
        );
        world = LoadJson("Tables/World", BuildFallbackWorld(), "World");
        balance = LoadJson("Tables/Balance", BuildFallbackBalance(), "Balance");
        ApplyPlayerLevelTable(
            balance,
            CsvParser.Parse<int>("Tables/PlayerLevelTable", PlayerLevelFromRow)
        );
        language = LoadLanguage();
        CsvParser.Parse("Tables/ActionWeightTable", ActionWeightFromRow);
        DataValidator.Validate(this);
    }

    /// <summary>
    /// 특정 난이도 등급의 몬스터 풀에서 1~3마리 무작위 선택.
    /// </summary>
    /// <param name="difficulty">노드 난이도.</param>
    /// <param name="count">선택할 마리 수(1~3).</param>
    /// <returns>선택된 몬스터 ID 리스트.</returns>
    public IList<int> PickMonsterGroup(int difficulty, int count)
    {
        // 동작 요약:
        // - monsters에서 Difficulty 일치 + IsBoss == false 항목 필터링.
        // - WeightedRandomt로 count개 선택(중복 허용 여부는 World.MonsterPoolDuplicateAllowed).
        var result = new List<int>();

        if (monsters == null || monsters.Count == 0 || count <= 0)
        {
            return result;
        }

        var pool = new List<MonsterData>();

        foreach (MonsterData monster in monsters.Values)
        {
            if (!monster.IsBoss && monster.Difficulty == difficulty)
            {
                pool.Add(monster);
            }
        }

        if (pool.Count == 0)
        {
            foreach (MonsterData monster in monsters.Values)
            {
                if (!monster.IsBoss)
                {
                    pool.Add(monster);
                }
            }
        }

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[index].Id);
        }

        return result;
    }

    public bool TryGetSkillRuneModifier(
        int skillId,
        RuneClass runeClass,
        out SkillRuneModifierData modifier
    )
    {
        modifier = null;
        if (skillId <= 0 || runeClass == RuneClass.None || skillRuneModifiers == null)
        {
            return false;
        }

        foreach (SkillRuneModifierData row in skillRuneModifiers.Values)
        {
            if (row != null && row.SkillId == skillId && row.RuneClass == runeClass)
            {
                modifier = row;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 드랍 테이블을 확률로 해결해 실제 드랍 결과를 반환한다.
    /// </summary>
    /// <param name="dropTableId">MonsterData.DropTableId.</param>
    /// <returns>드랍된 아이템 스택 목록(빈 목록이면 드랍 없음).</returns>
    public List<DroppedItemStack> ResolveDrops(int dropTableId)
    {
        // 동작 요약:
        // - dropTableId == 0 이면 빈 목록 반환.
        // - dropTables[dropTableId] 항목 순회.
        // - 각 항목: Random.value <= DropRate 이면 드랍 결정.
        //   드랍 수량 = Random.Range(MinCount, MaxCount + 1).
        // - DroppedItemStack 목록으로 수집하여 반환.
        var result = new List<DroppedItemStack>();

        if (dropTableId == 0 || dropTables == null)
        {
            return result;
        }

        if (!dropTables.TryGetValue(dropTableId, out List<DropEntry> entries))
        {
            return result;
        }

        foreach (DropEntry entry in entries)
        {
            if (UnityEngine.Random.value > entry.DropRate)
            {
                continue;
            }

            int min = System.Math.Max(1, entry.MinCount);
            int max = System.Math.Max(min, entry.MaxCount);

            result.Add(
                new DroppedItemStack
                {
                    ItemId = entry.ItemId,
                    Count = UnityEngine.Random.Range(min, max + 1),
                }
            );
        }

        return result;
    }

    /// <summary>
    /// 단계의 가격 인플레이션 배수 계산.
    /// </summary>
    /// <param name="stageIndex">단계(1~6).</param>
    /// <param name="erosionRate">해당 단계 침식률(0~100).</param>
    public float ComputeInflation(int stageIndex, float erosionRate)
    {
        // 동작 요약:
        // - balance.InflationCoef 사용해 price = base * (1 + erosionRate * coef).
        float coef = balance != null ? balance.InflationCoef : 0f;

        return 1f + UnityEngine.Mathf.Clamp01(erosionRate / 100f) * coef;
    }

    /// <summary>
    /// 현재 층 기준 인플레이션 배수. stageIndex 해석 + 침식률 조회 + ComputeInflation 을 한 곳에 모은다.
    /// Shop/Guild/Tavern 의 동일 3줄 중복을 제거한다.
    /// </summary>
    public float ComputeInflationForFloor(GameRunState run, int currentFloor)
    {
        int stageIndex = StageIndexResolver.FromFloor(currentFloor, World);
        float erosionRate = run?.Erosion != null ? run.Erosion.GetRate(stageIndex) : 0f;
        return ComputeInflation(stageIndex, erosionRate);
    }

    private static Dictionary<int, T> ToDictionary<T>(
        IList<T> rows,
        Dictionary<int, T> fallback,
        string tableName,
        bool allowEmpty = false
    )
        where T : DataTable
    {
        var result = new Dictionary<int, T>();

        if (rows != null)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                T row = rows[i];

                if (row == null)
                {
                    continue;
                }

                if (result.ContainsKey(row.Id))
                {
                    GameLog.LogError(
                        "[DataManager] Duplicate Id in " + tableName + ": " + row.Id
                    );
                    continue;
                }

                result[row.Id] = row;
            }
        }

        if (result.Count == 0 && !allowEmpty)
        {
            GameLog.LogWarning(
                "[DataManager] " + tableName + " load failed or empty. Using fallback data."
            );

            return fallback;
        }

        return result;
    }

    private static Dictionary<int, List<DropEntry>> ToDropTableDictionary(
        IList<DropEntry> rows,
        Dictionary<int, List<DropEntry>> fallback,
        string tableName
    )
    {
        var result = new Dictionary<int, List<DropEntry>>();

        if (rows != null)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                DropEntry row = rows[i];

                if (row == null)
                {
                    continue;
                }

                if (!result.TryGetValue(row.DropTableId, out List<DropEntry> entries))
                {
                    entries = new List<DropEntry>();
                    result[row.DropTableId] = entries;
                }

                entries.Add(row);
            }
        }

        if (result.Count == 0)
        {
            GameLog.LogWarning(
                "[DataManager] " + tableName + " load failed or empty. Using fallback data."
            );
            return fallback;
        }

        return result;
    }

    private static T LoadJson<T>(string resourcePath, T fallback, string tableName)
        where T : class
    {
        TextAsset asset = Resources.Load<TextAsset>(resourcePath);
        if (asset == null)
        {
            GameLog.LogWarning(
                "[DataManager] " + tableName + ".json missing. Using fallback data."
            );
            return fallback;
        }

        T parsed = JsonUtility.FromJson<T>(asset.text);

        if (parsed == null)
        {
            GameLog.LogWarning(
                "[DataManager] " + tableName + ".json parse failed. Using fallback data."
            );

            return fallback;
        }

        return parsed;
    }

    private static int PlayerLevelFromRow(IDictionary<string, string> row)
    {
        if (!CsvParser.HasColumns(row, "PlayerLevelTable", "Level", "ExpToNext"))
        {
            return 0;
        }

        return CsvParser.GetInt(row, "ExpToNext");
    }

    private static void ApplyPlayerLevelTable(BalanceData target, IList<int> levels)
    {
        if (target == null || levels == null || levels.Count == 0)
        {
            GameLog.LogWarning(
                "[DataManager] PlayerLevelTable load failed or empty. Keeping Balance.ExpToNextLevel."
            );
            return;
        }

        target.ExpToNextLevel = new List<int>(levels);
    }

    private static LanguageData LoadLanguage()
    {
        IList<IDictionary<string, string>> rows = CsvParser.Parse<IDictionary<string, string>>(
            "Tables/LocalizationTable",
            row => new Dictionary<string, string>(row)
        );
        return LanguageData.FromRows(rows);
    }

    private static ActionWeightTable ActionWeightFromRow(IDictionary<string, string> row)
    {
        if (
            !CsvParser.HasColumns(
                row,
                nameof(ActionWeightTable),
                "Key",
                "Attack",
                "Skill",
                "Defend"
            )
        )
        {
            return null;
        }

        return new ActionWeightTable
        {
            Attack = CsvParser.GetInt(row, "Attack", 80),
            Skill = CsvParser.GetInt(row, "Skill", 10),
            Defend = CsvParser.GetInt(row, "Defend", 10),
        };
    }


    private static void AddMonster(
        Dictionary<int, MonsterData> table,
        int id,
        string nameKey,
        bool isBoss,
        int difficulty,
        int hp,
        int mp,
        int atk,
        int def,
        int spd,
        int exp,
        int gold,
        int dropTableId
    )
    {
        table[id] = new MonsterData
        {
            Id = id,
            NameKey = nameKey,
            IsBoss = isBoss,
            Difficulty = difficulty,
            MaxHP = hp,
            MaxMP = mp,
            ATK = atk,
            DEF = def,
            SPD = spd,
            RewardExp = exp,
            RewardGold = gold,
            DropTableId = dropTableId,
            SkillIds = new List<int> { 900 },
            ActionWeights = new ActionWeightTable
            {
                Attack = 80,
                Skill = 10,
                Defend = 10,
            },
            PrefabKey = string.Empty,
            ErosionShaderKey = string.Empty,
        };
    }

    // Guid3 §9.F.2 2026-05-27: AcquireType.Shop 인 스킬 4개(SkillId 4~7) 추가.
    // 길드 구매 시나리오 검증을 위한 단일/범위 공격 풀.

    // Guid3 §9.F.1 2026-05-27: 직업별 시작 스킬 ID 매핑. Player.ApplyStartingClass 가 호출.
    // 추후 SkillTable.csv / RuneTable.csv 에 시작 스킬 정의가 들어오면 그쪽 권위로 교체.
    /// <summary>직업별 시작 스킬 ID 2개. 0 은 빈 슬롯.</summary>
    public int[] GetStartingSkillIds(RuneClass cls)
    {
        switch (cls)
        {
            case RuneClass.Dealer:
                return new int[] { 1, 0 };
            case RuneClass.Tanker:
                return new int[] { 1, 0 };
            case RuneClass.MagicDealer:
                return new int[] { 2, 0 };
            case RuneClass.Supporter:
                return new int[] { 3, 0 };
            default:
                return new int[] { 0, 0 };
        }
    }





}

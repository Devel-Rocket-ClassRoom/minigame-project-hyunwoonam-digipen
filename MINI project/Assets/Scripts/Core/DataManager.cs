using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 정적 데이터(CSV/JSON) 일괄 로드 및 조회. 런타임에 데이터를 수정하지 않는다.
    /// </summary>
    public sealed class DataManager
    {
        /// <summary>몬스터 테이블.</summary>
        public IReadOnlyDictionary<int, MonsterData> Monsters => monsters;

        /// <summary>스킬 테이블.</summary>
        public IReadOnlyDictionary<int, SkillData> Skills => skills;

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
                        Debug.LogError(
                            "[DataManager] Duplicate Id in " + tableName + ": " + row.Id
                        );
                        continue;
                    }

                    result[row.Id] = row;
                }
            }

            if (result.Count == 0 && !allowEmpty)
            {
                Debug.LogWarning(
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
                Debug.LogWarning(
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
                Debug.LogWarning(
                    "[DataManager] " + tableName + ".json missing. Using fallback data."
                );
                return fallback;
            }

            T parsed = JsonUtility.FromJson<T>(asset.text);

            if (parsed == null)
            {
                Debug.LogWarning(
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
                Debug.LogWarning(
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

        private static Dictionary<int, MonsterData> BuildFallbackMonsters()
        {
            var table = new Dictionary<int, MonsterData>();
            AddMonster(table, 1001, "monster.slime", false, 1, 1, 4, 7, 1, 8, 5, 5, 1);
            AddMonster(table, 1002, "monster.imp", false, 2, 1, 6, 9, 2, 10, 7, 7, 1);
            AddMonster(table, 1003, "monster.guard", false, 3, 1, 8, 12, 4, 9, 10, 10, 2);
            AddMonster(table, 1004, "monster.knight", false, 4, 1, 10, 15, 5, 11, 14, 14, 2);
            AddMonster(table, 1005, "monster.wraith", false, 5, 1, 12, 18, 6, 13, 18, 18, 2);
            AddMonster(table, 1006, "monster.elite", false, 6, 1, 14, 22, 8, 14, 24, 24, 3);
            AddMonster(table, 1901, "monster.boss.stage1", true, 3, 1, 12, 14, 4, 9, 30, 30, 3);
            AddMonster(table, 1902, "monster.boss.stage2", true, 5, 1, 16, 20, 6, 11, 45, 45, 3);
            AddMonster(table, 1903, "monster.boss.stage3", true, 7, 1, 20, 27, 8, 13, 65, 65, 3);
            AddMonster(table, 1904, "monster.boss.stage4", true, 9, 1, 25, 34, 10, 14, 90, 90, 3);
            AddMonster(
                table,
                1905,
                "monster.boss.stage5",
                true,
                11,
                1,
                30,
                42,
                12,
                16,
                125,
                125,
                3
            );
            AddMonster(table, 1906, "monster.boss.final", true, 13, 1, 40, 55, 16, 18, 180, 180, 3);
            return table;
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
        private static Dictionary<int, SkillData> BuildFallbackSkills()
        {
            return new Dictionary<int, SkillData>
            {
                [1] = new SkillData
                {
                    Id = 1,
                    NameKey = "skill.slash",
                    SkillType = SkillType.Active,
                    AcquireType = AcquireType.Default,
                    MpCost = 0,
                    DamageScale = 1.2f,
                    TargetType = SkillTargetType.EnemySingle,
                    ActionDuration = 0.35f,
                    EffectKey = "Destruction_air_normal",
                },
                [2] = new SkillData
                {
                    Id = 2,
                    NameKey = "skill.fire",
                    SkillType = SkillType.Active,
                    AcquireType = AcquireType.Default,
                    MpCost = 4,
                    DamageScale = 0.85f,
                    TargetType = SkillTargetType.EnemyAll,
                    ActionDuration = 0.5f,
                    CooldownRounds = 1,
                    EffectKey = "explosion_3",
                },
                [3] = new SkillData
                {
                    Id = 3,
                    NameKey = "skill.heal",
                    SkillType = SkillType.Active,
                    AcquireType = AcquireType.Default,
                    MpCost = 3,
                    HealScale = 1.1f,
                    TargetType = SkillTargetType.AllySingle,
                    ActionDuration = 0.45f,
                    EffectKey = "Destruction_air_blue",
                },
                [4] = new SkillData
                {
                    Id = 4,
                    NameKey = "skill.power_strike",
                    SkillType = SkillType.Active,
                    AcquireType = AcquireType.Shop,
                    PurchasePrice = 1,
                    MpCost = 5,
                    DamageScale = 2.0f,
                    TargetType = SkillTargetType.EnemySingle,
                    ActionDuration = 0.45f,
                    CooldownRounds = 1,
                    EffectKey = "Shotgun_hit_normal",
                },
                [5] = new SkillData
                {
                    Id = 5,
                    NameKey = "skill.cleave",
                    SkillType = SkillType.Active,
                    AcquireType = AcquireType.Shop,
                    PurchasePrice = 1,
                    MpCost = 5,
                    DamageScale = 1.25f,
                    TargetType = SkillTargetType.EnemyAll,
                    ActionDuration = 0.5f,
                    CooldownRounds = 1,
                    EffectKey = "Destruction_air_normal",
                },
                [6] = new SkillData
                {
                    Id = 6,
                    NameKey = "skill.execution",
                    SkillType = SkillType.Active,
                    AcquireType = AcquireType.Shop,
                    PurchasePrice = 2,
                    MpCost = 8,
                    DamageScale = 2.8f,
                    TargetType = SkillTargetType.EnemySingle,
                    ActionDuration = 0.55f,
                    CooldownRounds = 2,
                    EffectKey = "Shotgun_hit_normal",
                },
                [7] = new SkillData
                {
                    Id = 7,
                    NameKey = "skill.flame_burst",
                    SkillType = SkillType.Active,
                    AcquireType = AcquireType.Shop,
                    PurchasePrice = 2,
                    MpCost = 7,
                    DamageScale = 1.7f,
                    TargetType = SkillTargetType.EnemyAll,
                    ActionDuration = 0.6f,
                    CooldownRounds = 2,
                    EffectKey = "explosion_3",
                },
                [900] = new SkillData
                {
                    Id = 900,
                    NameKey = "skill.bite",
                    SkillType = SkillType.Active,
                    AcquireType = AcquireType.MonsterOnly,
                    MpCost = 0,
                    DamageScale = 1.05f,
                    TargetType = SkillTargetType.EnemySingle,
                    ActionDuration = 0.35f,
                    EffectKey = "Destruction_air_blue",
                },
            };
        }

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

        private static Dictionary<int, ItemData> BuildFallbackItems()
        {
            return new Dictionary<int, ItemData>
            {
                [1] = new ItemData
                {
                    Id = 1,
                    NameKey = "item.hp_potion",
                    Category = ItemCategory.Consumable,
                    SubCategory = "HP_Potion",
                    ConsumeEffectKey = "HealHP",
                    ParamValue = 30f,
                    BasePrice = 1,
                    Stackable = true,
                    MaxStack = 99,
                },
                [2] = new ItemData
                {
                    Id = 2,
                    NameKey = "item.mp_potion",
                    Category = ItemCategory.Consumable,
                    SubCategory = "MP_Potion",
                    ConsumeEffectKey = "HealMP",
                    ParamValue = 12f,
                    BasePrice = 1,
                    Stackable = true,
                    MaxStack = 99,
                },
                [3] = new ItemData
                {
                    Id = 3,
                    NameKey = "item.escape",
                    Category = ItemCategory.Consumable,
                    SubCategory = "Escape",
                    ConsumeEffectKey = "Escape",
                    ParamValue = 0f,
                    BasePrice = 30,
                    Stackable = true,
                    MaxStack = 10,
                    IsRetreat = true,
                },
                [101] = new ItemData
                {
                    Id = 101,
                    NameKey = "item.t1_sword",
                    Category = ItemCategory.Equipment,
                    SubCategory = "Weapon",
                    EquipSlot = EquipmentSlotId.Weapon,
                    EquipMod = new EquipmentStatMod { ATK = 4 },
                    ParamValue = 4f,
                    BasePrice = 40,
                    Stackable = false,
                    MaxStack = 1,
                },
                [102] = new ItemData
                {
                    Id = 102,
                    NameKey = "item.t1_body",
                    Category = ItemCategory.Equipment,
                    SubCategory = "ArmorBody",
                    EquipSlot = EquipmentSlotId.ArmorBody,
                    EquipMod = new EquipmentStatMod { HP = 16, DEF = 2 },
                    ParamValue = 2f,
                    BasePrice = 38,
                    Stackable = false,
                    MaxStack = 1,
                },
                [103] = new ItemData
                {
                    Id = 103,
                    NameKey = "item.t1_arms",
                    Category = ItemCategory.Equipment,
                    SubCategory = "ArmorArms",
                    EquipSlot = EquipmentSlotId.ArmorArms,
                    EquipMod = new EquipmentStatMod { ATK = 1, DEF = 2 },
                    ParamValue = 2f,
                    BasePrice = 34,
                    Stackable = false,
                    MaxStack = 1,
                },
                [104] = new ItemData
                {
                    Id = 104,
                    NameKey = "item.t1_legs",
                    Category = ItemCategory.Equipment,
                    SubCategory = "ArmorLegs",
                    EquipSlot = EquipmentSlotId.ArmorLegs,
                    EquipMod = new EquipmentStatMod { DEF = 1, SPD = 2 },
                    ParamValue = 2f,
                    BasePrice = 32,
                    Stackable = false,
                    MaxStack = 1,
                },
            };
        }

        private static Dictionary<int, RuneData> BuildFallbackRunes()
        {
            return new Dictionary<int, RuneData>
            {
                [1] = new RuneData
                {
                    Id = 1,
                    NameKey = "rune.dealer.start",
                    RuneType = RuneNodeType.MainNode,
                    ClassId = RuneClass.Dealer,
                    RequiredRuneId = 0,
                    PointCost = 0,
                    EffectType = RuneEffectType.AddATK,
                    EffectValue = 2f,
                },
                [2] = new RuneData
                {
                    Id = 2,
                    NameKey = "rune.tanker.start",
                    RuneType = RuneNodeType.MainNode,
                    ClassId = RuneClass.Tanker,
                    RequiredRuneId = 0,
                    PointCost = 0,
                    EffectType = RuneEffectType.AddDEF,
                    EffectValue = 2f,
                },
                [3] = new RuneData
                {
                    Id = 3,
                    NameKey = "rune.magic.start",
                    RuneType = RuneNodeType.MainNode,
                    ClassId = RuneClass.MagicDealer,
                    RequiredRuneId = 0,
                    PointCost = 0,
                    EffectType = RuneEffectType.AddMaxMP,
                    EffectValue = 8f,
                },
                [4] = new RuneData
                {
                    Id = 4,
                    NameKey = "rune.support.start",
                    RuneType = RuneNodeType.MainNode,
                    ClassId = RuneClass.Supporter,
                    RequiredRuneId = 0,
                    PointCost = 0,
                    EffectType = RuneEffectType.AddMaxHP,
                    EffectValue = 12f,
                },
            };
        }

        private static Dictionary<int, List<DropEntry>> BuildFallbackDropTables()
        {
            return new Dictionary<int, List<DropEntry>>
            {
                [1] = new List<DropEntry>
                {
                    new DropEntry
                    {
                        Id = 1001,
                        DropTableId = 1,
                        ItemId = 1,
                        MinCount = 1,
                        MaxCount = 1,
                        DropRate = 0.5f,
                    },
                },
                [2] = new List<DropEntry>
                {
                    new DropEntry
                    {
                        Id = 2001,
                        DropTableId = 2,
                        ItemId = 1,
                        MinCount = 1,
                        MaxCount = 2,
                        DropRate = 0.45f,
                    },
                    new DropEntry
                    {
                        Id = 2002,
                        DropTableId = 2,
                        ItemId = 2,
                        MinCount = 1,
                        MaxCount = 1,
                        DropRate = 0.25f,
                    },
                },
                [3] = new List<DropEntry>
                {
                    new DropEntry
                    {
                        Id = 3001,
                        DropTableId = 3,
                        ItemId = 101,
                        MinCount = 1,
                        MaxCount = 1,
                        DropRate = 0.2f,
                    },
                    new DropEntry
                    {
                        Id = 3002,
                        DropTableId = 3,
                        ItemId = 201,
                        MinCount = 1,
                        MaxCount = 1,
                        DropRate = 0.2f,
                    },
                },
            };
        }

        private static WorldData BuildFallbackWorld()
        {
            return new WorldData
            {
                FloorGen = new FloorGenRule
                {
                    MaxFloor = 49,
                    MinNodesPerFloor = 1,
                    MaxNodesPerFloor = 3,
                    TutorialNodeCounts = new List<int> { 1, 2, 1 },
                    MonstersMin = 1,
                    MonstersMax = 3,
                },
                Stages = new List<StageDef>
                {
                    new StageDef
                    {
                        StageIndex = 1,
                        FloorStart = 1,
                        FloorEnd = 2,
                        BossFloor = 3,
                        UnlocksSafeZoneIndex = 1,
                        DifficultyMin = 1,
                        DifficultyMax = 3,
                    },
                    new StageDef
                    {
                        StageIndex = 2,
                        FloorStart = 5,
                        FloorEnd = 10,
                        BossFloor = 11,
                        UnlocksSafeZoneIndex = 2,
                        DifficultyMin = 5,
                        DifficultyMax = 7,
                    },
                    new StageDef
                    {
                        StageIndex = 3,
                        FloorStart = 13,
                        FloorEnd = 18,
                        BossFloor = 19,
                        UnlocksSafeZoneIndex = 3,
                        DifficultyMin = 9,
                        DifficultyMax = 11,
                    },
                    new StageDef
                    {
                        StageIndex = 4,
                        FloorStart = 21,
                        FloorEnd = 28,
                        BossFloor = 29,
                        UnlocksSafeZoneIndex = 4,
                        DifficultyMin = 13,
                        DifficultyMax = 15,
                    },
                    new StageDef
                    {
                        StageIndex = 5,
                        FloorStart = 31,
                        FloorEnd = 38,
                        BossFloor = 39,
                        UnlocksSafeZoneIndex = 5,
                        DifficultyMin = 17,
                        DifficultyMax = 19,
                    },
                    new StageDef
                    {
                        StageIndex = 6,
                        FloorStart = 41,
                        FloorEnd = 48,
                        BossFloor = 49,
                        UnlocksSafeZoneIndex = 5,
                        DifficultyMin = 21,
                        DifficultyMax = 23,
                    },
                },
                SafeZones = new List<SafeZoneDef>
                {
                    new SafeZoneDef
                    {
                        Index = 0,
                        FloorNumber = 0,
                        FeatureKeys = new List<string>(),
                    },
                    new SafeZoneDef
                    {
                        Index = 1,
                        FloorNumber = 4,
                        FeatureKeys = new List<string> { "Inn", "Shop", "Guild", "Temple" },
                    },
                    new SafeZoneDef
                    {
                        Index = 2,
                        FloorNumber = 12,
                        FeatureKeys = new List<string> { "ErosionAltar" },
                    },
                    new SafeZoneDef
                    {
                        Index = 3,
                        FloorNumber = 20,
                        FeatureKeys = new List<string> { "Mine" },
                    },
                    new SafeZoneDef
                    {
                        Index = 4,
                        FloorNumber = 30,
                        FeatureKeys = new List<string> { "Mine" },
                    },
                    new SafeZoneDef
                    {
                        Index = 5,
                        FloorNumber = 40,
                        FeatureKeys = new List<string> { "Mine" },
                    },
                },
                MonsterPoolWeights = new List<int> { 1, 1, 1 },
            };
        }

        private static BalanceData BuildFallbackBalance()
        {
            return new BalanceData
            {
                ExpToNextLevel = new List<int> { 0, 10, 25, 45, 70, 100, 140, 190, 250, 320, 400 },
                RunePointPerLevel = 1,
                ErosionCurve = new ErosionCurve
                {
                    DailyBase = 1f,
                    ExpBase = 1.05f,
                    InflectionDay = 10,
                },
                ErosionMonsterMultiplier = 1.5f,
                InflationCoef = 0.6f,
                SellRatio = 0.45f,
                EnhanceMultiplier = 0.1f,
                EnhanceCostBase = 0.5f,
                EnhanceCostPerLevel = 0.5f,
                EnhanceBaseSuccessRate = 0.95f,
                EnhanceSuccessRateDecayPerLevel = 0.07f,
                EnhanceMinSuccessRate = 0.2f,
                EnhancePityFailCount = 5,
                MineDailyGain = new List<int> { 12, 20, 30 },
                MineActivationCost = 90,
                ErosionAltarReduction = 12f,
                ErosionAltarCost = 35,
                MinActionTimeSec = 0.1f,
                RuneResetRefundRate = 0.8f,
                RuneResetCostGold = 60,
                RuneClassChangeCostGold = 100,
                TavernLodgingCostPerPerson = 6,
                TavernStorageActivationCost = 30,
                TavernStorageUpgradeBaseCost = 45,
                TavernStorageUpgradeCostStep = 25,
                FirstNonPlayerActionDelaySec = 2f,
                AttackActionTimeSec = 0.3f,
                SkillActionFallbackSec = 0.5f,
                DefendActionTimeSec = 0.1f,
                CombatGeneratedSpriteSize = 64,
                CombatGeneratedSpritePixelsPerUnit = 48f,
                UseErosionMonsterMultiplierCurve = false,
                ErosionMonsterMultiplierCurvePower = 1f,
            };
        }
    }
}

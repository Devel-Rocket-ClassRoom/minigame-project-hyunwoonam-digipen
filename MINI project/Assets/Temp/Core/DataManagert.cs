using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 정적 데이터(CSV/JSON) 일괄 로드 및 조회. 런타임에 데이터를 수정하지 않는다.
    /// </summary>
    public sealed class DataManagert
    {
        /// <summary>몬스터 테이블.</summary>
        public IReadOnlyDictionary<int, MonsterDatat> Monsters => monsters;

        /// <summary>스킬 테이블.</summary>
        public IReadOnlyDictionary<int, SkillDatat> Skills => skills;

        /// <summary>아이템 테이블.</summary>
        public IReadOnlyDictionary<int, ItemDatat> Items => items;

        /// <summary>룬 테이블.</summary>
        public IReadOnlyDictionary<int, RuneDatat> Runes => runes;

        /// <summary>월드/노드/단계 설정.</summary>
        public WorldDatat World => world;

        /// <summary>동료 모집/기본 데이터.</summary>
        public IReadOnlyDictionary<int, CompanionDatat> Companions => companions;

        /// <summary>밸런스 곡선(EXP, 침식, 가격 인플레이션).</summary>
        public BalanceDatat Balance => balance;

        /// <summary>언어 리소스.</summary>
        public LanguageDatat Language => language;

        /// <summary>
        /// 드랍 테이블. 키 = DropTableId, 값 = 해당 그룹 항목 목록.
        /// MonsterDatat.DropTableId == 0 이면 이 테이블에 항목 없음.
        /// </summary>
        public IReadOnlyDictionary<int, List<DropEntryt>> DropTables => dropTables;

        private Dictionary<int, MonsterDatat> monsters;
        private Dictionary<int, SkillDatat> skills;
        private Dictionary<int, ItemDatat> items;
        private Dictionary<int, RuneDatat> runes;
        private WorldDatat world;
        private Dictionary<int, CompanionDatat> companions;
        private BalanceDatat balance;
        private LanguageDatat language;
        private Dictionary<int, List<DropEntryt>> dropTables;

        /// <summary>
        /// 모든 데이터 파일을 일괄 로드.
        /// </summary>
        public void LoadAll()
        {
            // 동작 요약:
            // - LoadCsv("MonsterStatusTable.csv") → monsters (MonsterDatat).
            // - LoadCsv("SkillTable.csv") → skills (SkillDatat).
            // - LoadCsv("ItemTable.csv") → items (ItemDatat).
            // - LoadCsv("DropTable.csv") → dropTables (DropEntryt 목록, DropTableId 기준 그룹화).
            // - LoadCsv("RuneTable.csv") → runes (RuneDatat).
            // - LoadCsv("LocalizationTable.csv") → language (LanguageDatat).
            // - LoadCsv("PlayerLevelTable.csv") → balance.LevelTable.
            // - LoadJson("World.json") → world.
            // - LoadJson("Balance.json") → balance (나머지 필드).
            // - LoadCsv("Companions.csv") → companions.
            // - 필수 필드 검증 실패 시 DebugLoggert.Error + 예외.
            //TODO: monsters   = ParseCsv<MonsterDatat>("MonsterStatusTable");
            //TODO: skills     = ParseCsv<SkillDatat>("SkillTable");
            //TODO: items      = ParseCsv<ItemDatat>("ItemTable");
            //TODO: runes      = ParseCsv<RuneDatat>("RuneTable");
            //TODO: companions = ParseCsv<CompanionDatat>("Companions");
            //TODO: // DropTable CSV → dropTables 그룹화
            //TODO: var dropRows = ParseCsvRows("DropTable");
            //TODO: dropTables = new Dictionary<int, List<DropEntryt>>();
            //TODO: foreach (var row in dropRows)
            //TODO: {
            //TODO:     var entry = DropEntryt.Parse(row);
            //TODO:     if (!dropTables.ContainsKey(entry.DropTableId)) dropTables[entry.DropTableId] = new List<DropEntryt>();
            //TODO:     dropTables[entry.DropTableId].Add(entry);
            //TODO: }
            //TODO: world    = LoadJson<WorldDatat>("World");
            //TODO: balance  = LoadJson<BalanceDatat>("Balance");
            //TODO: language = ParseCsv<LanguageDatat>("LocalizationTable");
            //TODO: // 검증
            //TODO: if (monsters.Count == 0) throw new System.Exception("[DataManagert] MonsterStatusTable 비어 있음");
            monsters = BuildFallbackMonsters(); //Wave0write
            skills = BuildFallbackSkills(); //Wave0write
            items = BuildFallbackItems(); //Wave0write
            runes = BuildFallbackRunes(); //Wave0write
            companions = new Dictionary<int, CompanionDatat>(); //Wave0write
            dropTables = BuildFallbackDropTables(); //Wave0write
            world = BuildFallbackWorld(); //Wave0write
            balance = BuildFallbackBalance(); //Wave0write
            language = new LanguageDatat(); //Wave0write
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
            //TODO: var pool = new List<MonsterDatat>();
            //TODO: foreach (var m in monsters.Values)
            //TODO:     if (m.Difficulty == difficulty && !m.IsBoss) pool.Add(m);
            //TODO: var result = new List<int>();
            //TODO: var weights = pool.ConvertAll(m => (float)m.SpawnWeight);
            //TODO: for (int i = 0; i < count; i++)
            //TODO: {
            //TODO:     int idx = WeightedRandomt.PickIndex(weights);
            //TODO:     result.Add(pool[idx].Id);
            //TODO:     if (!world.MonsterPoolDuplicateAllowed) { pool.RemoveAt(idx); weights.RemoveAt(idx); }
            //TODO:     if (pool.Count == 0) break;
            //TODO: }
            //TODO: return result;
            var result = new List<int>(); //Wave0write
            if (monsters == null || monsters.Count == 0 || count <= 0) //Wave0write
            { //Wave0write
                return result; //Wave0write
            } //Wave0write

            var pool = new List<MonsterDatat>(); //Wave0write
            foreach (MonsterDatat monster in monsters.Values) //Wave0write
            { //Wave0write
                if (!monster.IsBoss && monster.Difficulty == difficulty) //Wave0write
                { //Wave0write
                    pool.Add(monster); //Wave0write
                } //Wave0write
            } //Wave0write

            if (pool.Count == 0) //Wave0write
            { //Wave0write
                foreach (MonsterDatat monster in monsters.Values) //Wave0write
                { //Wave0write
                    if (!monster.IsBoss) //Wave0write
                    { //Wave0write
                        pool.Add(monster); //Wave0write
                    } //Wave0write
                } //Wave0write
            } //Wave0write

            for (int i = 0; i < count && pool.Count > 0; i++) //Wave0write
            { //Wave0write
                int index = UnityEngine.Random.Range(0, pool.Count); //Wave0write
                result.Add(pool[index].Id); //Wave0write
            } //Wave0write

            return result; //Wave0write
        }

        /// <summary>
        /// 드랍 테이블을 확률로 해결해 실제 드랍 결과를 반환한다.
        /// </summary>
        /// <param name="dropTableId">MonsterDatat.DropTableId.</param>
        /// <returns>드랍된 아이템 스택 목록(빈 목록이면 드랍 없음).</returns>
        public List<DroppedItemStackt> ResolveDrops(int dropTableId)
        {
            // 동작 요약:
            // - dropTableId == 0 이면 빈 목록 반환.
            // - dropTables[dropTableId] 항목 순회.
            // - 각 항목: Random.value <= DropRate 이면 드랍 결정.
            //   드랍 수량 = Random.Range(MinCount, MaxCount + 1).
            // - DroppedItemStackt 목록으로 수집하여 반환.
            //TODO: var result = new List<DroppedItemStackt>();
            //TODO: if (dropTableId == 0) return result;
            //TODO: if (!dropTables.TryGetValue(dropTableId, out var entries)) return result;
            //TODO: foreach (var entry in entries)
            //TODO: {
            //TODO:     if (UnityEngine.Random.value <= entry.DropRate)
            //TODO:     {
            //TODO:         int count = UnityEngine.Random.Range(entry.MinCount, entry.MaxCount + 1);
            //TODO:         result.Add(new DroppedItemStackt { ItemId = entry.ItemId, Count = count });
            //TODO:     }
            //TODO: }
            //TODO: return result;
            var result = new List<DroppedItemStackt>(); //Wave0write
            if (dropTableId == 0 || dropTables == null) //Wave0write
            { //Wave0write
                return result; //Wave0write
            } //Wave0write

            if (!dropTables.TryGetValue(dropTableId, out List<DropEntryt> entries)) //Wave0write
            { //Wave0write
                return result; //Wave0write
            } //Wave0write

            foreach (DropEntryt entry in entries) //Wave0write
            { //Wave0write
                if (UnityEngine.Random.value > entry.DropRate) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                int min = System.Math.Max(1, entry.MinCount); //Wave0write
                int max = System.Math.Max(min, entry.MaxCount); //Wave0write
                result.Add(new DroppedItemStackt //Wave0write
                { //Wave0write
                    ItemId = entry.ItemId, //Wave0write
                    Count = UnityEngine.Random.Range(min, max + 1), //Wave0write
                }); //Wave0write
            } //Wave0write

            return result; //Wave0write
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
            //TODO: float coef = balance.InflationCoef;
            //TODO: return 1f + (erosionRate / 100f) * coef;
            float coef = balance != null ? balance.InflationCoef : 0f; //Wave0write
            return 1f + UnityEngine.Mathf.Clamp01(erosionRate / 100f) * coef; //Wave0write
        }

        private static Dictionary<int, MonsterDatat> BuildFallbackMonsters() //Wave0write
        { //Wave0write
            var table = new Dictionary<int, MonsterDatat>(); //Wave0write
            AddMonster(table, 1001, "monster.slime", false, 1, 24, 4, 7, 1, 8, 5, 5, 1); //Wave0write
            AddMonster(table, 1002, "monster.imp", false, 2, 34, 6, 9, 2, 10, 7, 7, 1); //Wave0write
            AddMonster(table, 1003, "monster.guard", false, 3, 46, 8, 12, 4, 9, 10, 10, 2); //Wave0write
            AddMonster(table, 1004, "monster.knight", false, 4, 58, 10, 15, 5, 11, 14, 14, 2); //Wave0write
            AddMonster(table, 1005, "monster.wraith", false, 5, 72, 12, 18, 6, 13, 18, 18, 2); //Wave0write
            AddMonster(table, 1006, "monster.elite", false, 6, 88, 14, 22, 8, 14, 24, 24, 3); //Wave0write
            AddMonster(table, 1901, "monster.boss.stage1", true, 3, 90, 12, 14, 4, 9, 30, 30, 3); //Wave0write
            AddMonster(table, 1902, "monster.boss.stage2", true, 5, 130, 16, 20, 6, 11, 45, 45, 3); //Wave0write
            AddMonster(table, 1903, "monster.boss.stage3", true, 7, 180, 20, 27, 8, 13, 65, 65, 3); //Wave0write
            AddMonster(table, 1904, "monster.boss.stage4", true, 9, 240, 25, 34, 10, 14, 90, 90, 3); //Wave0write
            AddMonster(table, 1905, "monster.boss.stage5", true, 11, 310, 30, 42, 12, 16, 125, 125, 3); //Wave0write
            AddMonster(table, 1906, "monster.boss.final", true, 13, 420, 40, 55, 16, 18, 180, 180, 3); //Wave0write
            return table; //Wave0write
        } //Wave0write

        private static void AddMonster(Dictionary<int, MonsterDatat> table, int id, string nameKey, bool isBoss, int difficulty, int hp, int mp, int atk, int def, int spd, int exp, int gold, int dropTableId) //Wave0write
        { //Wave0write
            table[id] = new MonsterDatat //Wave0write
            { //Wave0write
                Id = id, //Wave0write
                NameKey = nameKey, //Wave0write
                IsBoss = isBoss, //Wave0write
                Difficulty = difficulty, //Wave0write
                MaxHP = hp, //Wave0write
                MaxMP = mp, //Wave0write
                ATK = atk, //Wave0write
                DEF = def, //Wave0write
                SPD = spd, //Wave0write
                RewardExp = exp, //Wave0write
                RewardGold = gold, //Wave0write
                DropTableId = dropTableId, //Wave0write
                SkillIds = new List<int> { 900 }, //Wave0write
                ActionWeights = new ActionWeightTablet { Attack = 80, Skill = 10, Defend = 10 }, //Wave0write
                PrefabKey = string.Empty, //Wave0write
                ErosionShaderKey = string.Empty, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static Dictionary<int, SkillDatat> BuildFallbackSkills() //Wave0write
        { //Wave0write
            return new Dictionary<int, SkillDatat> //Wave0write
            { //Wave0write
                [1] = new SkillDatat { Id = 1, NameKey = "skill.slash", SkillType = SkillTypet.Active, AcquireType = AcquireTypet.Default, MpCost = 0, DamageScale = 1.2f, TargetType = SkillTargetTypet.EnemySingle, ActionDuration = 0.35f }, //Wave0write
                [2] = new SkillDatat { Id = 2, NameKey = "skill.fire", SkillType = SkillTypet.Active, AcquireType = AcquireTypet.Default, MpCost = 4, DamageScale = 0.85f, TargetType = SkillTargetTypet.EnemyAll, ActionDuration = 0.5f, CooldownRounds = 1 }, //Wave0write
                [3] = new SkillDatat { Id = 3, NameKey = "skill.heal", SkillType = SkillTypet.Active, AcquireType = AcquireTypet.Default, MpCost = 3, HealScale = 1.1f, TargetType = SkillTargetTypet.AllySingle, ActionDuration = 0.45f }, //Wave0write
                [900] = new SkillDatat { Id = 900, NameKey = "skill.bite", SkillType = SkillTypet.Active, AcquireType = AcquireTypet.MonsterOnly, MpCost = 0, DamageScale = 1.05f, TargetType = SkillTargetTypet.EnemySingle, ActionDuration = 0.35f }, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static Dictionary<int, ItemDatat> BuildFallbackItems() //Wave0write
        { //Wave0write
            return new Dictionary<int, ItemDatat> //Wave0write
            { //Wave0write
                [1] = new ItemDatat { Id = 1, NameKey = "item.hp_potion", Category = ItemCategoryt.Consumable, SubCategory = "HP_Potion", ConsumeEffectKey = "HealHP", ParamValue = 30f, BaseBuyPrice = 15, BaseSellPrice = 7, Stackable = true, MaxStack = 99 }, //Wave0write
                [2] = new ItemDatat { Id = 2, NameKey = "item.mp_potion", Category = ItemCategoryt.Consumable, SubCategory = "MP_Potion", ConsumeEffectKey = "HealMP", ParamValue = 12f, BaseBuyPrice = 15, BaseSellPrice = 7, Stackable = true, MaxStack = 99 }, //Wave0write
                [3] = new ItemDatat { Id = 3, NameKey = "item.escape", Category = ItemCategoryt.Consumable, SubCategory = "Escape", ConsumeEffectKey = "Escape", ParamValue = 0f, BaseBuyPrice = 30, BaseSellPrice = 15, Stackable = true, MaxStack = 10, IsRetreat = true }, //Wave0write
                [101] = new ItemDatat { Id = 101, NameKey = "item.training_sword", Category = ItemCategoryt.Equipment, SubCategory = "Weapon", EquipSlot = EquipmentSlotIdt.Weapon, EquipMod = new EquipmentStatModt { ATK = 3 }, ParamValue = 3f, BaseBuyPrice = 40, BaseSellPrice = 20, Stackable = false, MaxStack = 1 }, //Wave0write
                [201] = new ItemDatat { Id = 201, NameKey = "item.cloth_armor", Category = ItemCategoryt.Equipment, SubCategory = "ArmorBody", EquipSlot = EquipmentSlotIdt.ArmorBody, EquipMod = new EquipmentStatModt { HP = 10, DEF = 1 }, ParamValue = 1f, BaseBuyPrice = 35, BaseSellPrice = 17, Stackable = false, MaxStack = 1 }, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static Dictionary<int, RuneDatat> BuildFallbackRunes() //Wave0write
        { //Wave0write
            return new Dictionary<int, RuneDatat> //Wave0write
            { //Wave0write
                [1] = new RuneDatat { Id = 1, NameKey = "rune.dealer.start", RuneType = RuneNodeTypet.MainNode, ClassId = RuneClasst.Dealer, RequiredRuneId = 0, PointCost = 0, EffectType = RuneEffectTypet.AddATK, EffectValue = 2f }, //Wave0write
                [2] = new RuneDatat { Id = 2, NameKey = "rune.tanker.start", RuneType = RuneNodeTypet.MainNode, ClassId = RuneClasst.Tanker, RequiredRuneId = 0, PointCost = 0, EffectType = RuneEffectTypet.AddDEF, EffectValue = 2f }, //Wave0write
                [3] = new RuneDatat { Id = 3, NameKey = "rune.magic.start", RuneType = RuneNodeTypet.MainNode, ClassId = RuneClasst.MagicDealer, RequiredRuneId = 0, PointCost = 0, EffectType = RuneEffectTypet.AddMaxMP, EffectValue = 8f }, //Wave0write
                [4] = new RuneDatat { Id = 4, NameKey = "rune.support.start", RuneType = RuneNodeTypet.MainNode, ClassId = RuneClasst.Supporter, RequiredRuneId = 0, PointCost = 0, EffectType = RuneEffectTypet.AddMaxHP, EffectValue = 12f }, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static Dictionary<int, List<DropEntryt>> BuildFallbackDropTables() //Wave0write
        { //Wave0write
            return new Dictionary<int, List<DropEntryt>> //Wave0write
            { //Wave0write
                [1] = new List<DropEntryt> { new DropEntryt { Id = 1001, DropTableId = 1, ItemId = 1, MinCount = 1, MaxCount = 1, DropRate = 0.5f } }, //Wave0write
                [2] = new List<DropEntryt> { new DropEntryt { Id = 2001, DropTableId = 2, ItemId = 1, MinCount = 1, MaxCount = 2, DropRate = 0.45f }, new DropEntryt { Id = 2002, DropTableId = 2, ItemId = 2, MinCount = 1, MaxCount = 1, DropRate = 0.25f } }, //Wave0write
                [3] = new List<DropEntryt> { new DropEntryt { Id = 3001, DropTableId = 3, ItemId = 101, MinCount = 1, MaxCount = 1, DropRate = 0.2f }, new DropEntryt { Id = 3002, DropTableId = 3, ItemId = 201, MinCount = 1, MaxCount = 1, DropRate = 0.2f } }, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static WorldDatat BuildFallbackWorld() //Wave0write
        { //Wave0write
            return new WorldDatat //Wave0write
            { //Wave0write
                FloorGen = new FloorGenRulet //Wave0write
                { //Wave0write
                    MaxFloor = 49, //Wave0write
                    MinNodesPerFloor = 1, //Wave0write
                    MaxNodesPerFloor = 3, //Wave0write
                    TutorialNodeCounts = new List<int> { 1, 2, 1 }, //Wave0write
                    MonstersMin = 1, //Wave0write
                    MonstersMax = 3, //Wave0write
                }, //Wave0write
                Stages = new List<StageDeft> //Wave0write
                { //Wave0write
                    new StageDeft { StageIndex = 1, FloorStart = 1, FloorEnd = 2, BossFloor = 3, UnlocksSafeZoneIndex = 1, DifficultyMin = 1, DifficultyMax = 2 }, //Wave0write
                    new StageDeft { StageIndex = 2, FloorStart = 5, FloorEnd = 10, BossFloor = 11, UnlocksSafeZoneIndex = 2, DifficultyMin = 2, DifficultyMax = 4 }, //Wave0write
                    new StageDeft { StageIndex = 3, FloorStart = 13, FloorEnd = 18, BossFloor = 19, UnlocksSafeZoneIndex = 3, DifficultyMin = 3, DifficultyMax = 6 }, //Wave0write
                    new StageDeft { StageIndex = 4, FloorStart = 21, FloorEnd = 28, BossFloor = 29, UnlocksSafeZoneIndex = 4, DifficultyMin = 4, DifficultyMax = 8 }, //Wave0write
                    new StageDeft { StageIndex = 5, FloorStart = 31, FloorEnd = 38, BossFloor = 39, UnlocksSafeZoneIndex = 5, DifficultyMin = 5, DifficultyMax = 10 }, //Wave0write
                    new StageDeft { StageIndex = 6, FloorStart = 41, FloorEnd = 48, BossFloor = 49, UnlocksSafeZoneIndex = 5, DifficultyMin = 6, DifficultyMax = 12 }, //Wave0write
                }, //Wave0write
                SafeZones = new List<SafeZoneDeft> //Wave0write
                { //Wave0write
                    new SafeZoneDeft { Index = 0, FloorNumber = 0, FeatureKeys = new List<string>() }, //Wave0write
                    new SafeZoneDeft { Index = 1, FloorNumber = 4, FeatureKeys = new List<string> { "Inn", "Shop", "Guild", "Temple" } }, //Wave0write
                    new SafeZoneDeft { Index = 2, FloorNumber = 12, FeatureKeys = new List<string> { "ErosionAltar" } }, //Wave0write
                    new SafeZoneDeft { Index = 3, FloorNumber = 20, FeatureKeys = new List<string> { "Mine" } }, //Wave0write
                    new SafeZoneDeft { Index = 4, FloorNumber = 30, FeatureKeys = new List<string> { "Mine" } }, //Wave0write
                    new SafeZoneDeft { Index = 5, FloorNumber = 40, FeatureKeys = new List<string> { "Mine" } }, //Wave0write
                }, //Wave0write
                MonsterPoolWeights = new List<int> { 1, 1, 1 }, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static BalanceDatat BuildFallbackBalance() //Wave0write
        { //Wave0write
            return new BalanceDatat //Wave0write
            { //Wave0write
                ExpToNextLevel = new List<int> { 0, 10, 25, 45, 70, 100, 140, 190, 250, 320, 400 }, //Wave0write
                RunePointPerLevel = 1, //Wave0write
                ErosionCurve = new ErosionCurvet { DailyBase = 1f, ExpBase = 1.05f, InflectionDay = 10 }, //Wave0write
                ErosionMonsterMultiplier = 1.5f, //Wave0write
                InflationCoef = 0.5f, //Wave0write
                MineDailyGain = new List<int> { 1, 2, 3 }, //Wave0write
                ErosionAltarReduction = 10f, //Wave0write
                ErosionAltarCost = 3, //Wave0write
                MinActionTimeSec = 0.1f, //Wave0write
            }; //Wave0write
        } //Wave0write
    }
}

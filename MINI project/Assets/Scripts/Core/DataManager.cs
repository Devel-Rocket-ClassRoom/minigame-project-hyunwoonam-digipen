using System.Collections.Generic;

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
            // 동작 요약:
            // - LoadCsv("MonsterStatusTable.csv") → monsters (MonsterData).
            // - LoadCsv("SkillTable.csv") → skills (SkillData).
            // - LoadCsv("ItemTable.csv") → items (ItemData).
            // - LoadCsv("DropTable.csv") → dropTables (DropEntry 목록, DropTableId 기준 그룹화).
            // - LoadCsv("RuneTable.csv") → runes (RuneData).
            // - LoadCsv("LocalizationTable.csv") → language (LanguageData).
            // - LoadCsv("PlayerLevelTable.csv") → balance.LevelTable.
            // - LoadJson("World.json") → world.
            // - LoadJson("Balance.json") → balance (나머지 필드).
            // - LoadCsv("Companions.csv") → companions.
            // - 필수 필드 검증 실패 시 DebugLogger.Error + 예외.
            //TODO: monsters   = ParseCsv<MonsterData>("MonsterStatusTable");
            //TODO: skills     = ParseCsv<SkillData>("SkillTable");
            //TODO: items      = ParseCsv<ItemData>("ItemTable");
            //TODO: runes      = ParseCsv<RuneData>("RuneTable");
            //TODO: companions = ParseCsv<CompanionData>("Companions");
            //TODO: // DropTable CSV → dropTables 그룹화
            //TODO: var dropRows = ParseCsvRows("DropTable");
            //TODO: dropTables = new Dictionary<int, List<DropEntry>>();
            //TODO: foreach (var row in dropRows)
            //TODO: {
            //TODO:     var entry = DropEntry.Parse(row);
            //TODO:     if (!dropTables.ContainsKey(entry.DropTableId)) dropTables[entry.DropTableId] = new List<DropEntry>();
            //TODO:     dropTables[entry.DropTableId].Add(entry);
            //TODO: }
            //TODO: world    = LoadJson<WorldData>("World");
            //TODO: balance  = LoadJson<BalanceData>("Balance");
            //TODO: language = ParseCsv<LanguageData>("LocalizationTable");
            //TODO: // 검증
            //TODO: if (monsters.Count == 0) throw new System.Exception("[DataManager] MonsterStatusTable 비어 있음");
            monsters = BuildFallbackMonsters(); //Wave0write
            skills = BuildFallbackSkills(); //Wave0write
            items = BuildFallbackItems(); //Wave0write
            runes = BuildFallbackRunes(); //Wave0write
            companions = new Dictionary<int, CompanionData>(); //Wave0write
            dropTables = BuildFallbackDropTables(); //Wave0write
            world = BuildFallbackWorld(); //Wave0write
            balance = BuildFallbackBalance(); //Wave0write
            language = new LanguageData(); //Wave0write
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
            //TODO: var pool = new List<MonsterData>();
            //TODO: foreach (var m in monsters.Values)
            //TODO:     if (m.Difficulty == difficulty && !m.IsBoss) pool.Add(m);
            //TODO: var result = new List<int>();
            //TODO: var weights = pool.ConvertAll(m => (float)m.SpawnWeight);
            //TODO: for (int i = 0; i < count; i++)
            //TODO: {
            //TODO:     int idx = WeightedRandom.PickIndex(weights);
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

            var pool = new List<MonsterData>(); //Wave0write
            foreach (MonsterData monster in monsters.Values) //Wave0write
            { //Wave0write
                if (!monster.IsBoss && monster.Difficulty == difficulty) //Wave0write
                { //Wave0write
                    pool.Add(monster); //Wave0write
                } //Wave0write
            } //Wave0write

            if (pool.Count == 0) //Wave0write
            { //Wave0write
                foreach (MonsterData monster in monsters.Values) //Wave0write
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
            //TODO: var result = new List<DroppedItemStack>();
            //TODO: if (dropTableId == 0) return result;
            //TODO: if (!dropTables.TryGetValue(dropTableId, out var entries)) return result;
            //TODO: foreach (var entry in entries)
            //TODO: {
            //TODO:     if (UnityEngine.Random.value <= entry.DropRate)
            //TODO:     {
            //TODO:         int count = UnityEngine.Random.Range(entry.MinCount, entry.MaxCount + 1);
            //TODO:         result.Add(new DroppedItemStack { ItemId = entry.ItemId, Count = count });
            //TODO:     }
            //TODO: }
            //TODO: return result;
            var result = new List<DroppedItemStack>(); //Wave0write
            if (dropTableId == 0 || dropTables == null) //Wave0write
            { //Wave0write
                return result; //Wave0write
            } //Wave0write

            if (!dropTables.TryGetValue(dropTableId, out List<DropEntry> entries)) //Wave0write
            { //Wave0write
                return result; //Wave0write
            } //Wave0write

            foreach (DropEntry entry in entries) //Wave0write
            { //Wave0write
                if (UnityEngine.Random.value > entry.DropRate) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                int min = System.Math.Max(1, entry.MinCount); //Wave0write
                int max = System.Math.Max(min, entry.MaxCount); //Wave0write
                result.Add(new DroppedItemStack //Wave0write
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

        private static Dictionary<int, MonsterData> BuildFallbackMonsters() //Wave0write
        { //Wave0write
            var table = new Dictionary<int, MonsterData>(); //Wave0write
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

        private static void AddMonster(Dictionary<int, MonsterData> table, int id, string nameKey, bool isBoss, int difficulty, int hp, int mp, int atk, int def, int spd, int exp, int gold, int dropTableId) //Wave0write
        { //Wave0write
            table[id] = new MonsterData //Wave0write
            { //Wave0write
                Id = id, //Wave0write
                NameKey = nameKey, //Wave0write
                IsBoss = isBoss, //Wave0write
                Difficulty = difficulty, //Wave0write
                MaxHP = 1, //Wave0write
                MaxMP = mp, //Wave0write
                ATK = atk, //Wave0write
                DEF = def, //Wave0write
                SPD = spd, //Wave0write
                RewardExp = exp, //Wave0write
                RewardGold = gold, //Wave0write
                DropTableId = dropTableId, //Wave0write
                SkillIds = new List<int> { 900 }, //Wave0write
                ActionWeights = new ActionWeightTable { Attack = 80, Skill = 10, Defend = 10 }, //Wave0write
                PrefabKey = string.Empty, //Wave0write
                ErosionShaderKey = string.Empty, //Wave0write
            }; //Wave0write
        } //Wave0write

        // Guid3 §9.F.2 2026-05-27: AcquireType.Shop 인 스킬 4개(SkillId 4~7) 추가.
        // 길드 구매 시나리오 검증을 위한 단일/범위 공격 풀.
        private static Dictionary<int, SkillData> BuildFallbackSkills() //Wave0write
        { //Wave0write
            return new Dictionary<int, SkillData> //Wave0write
            { //Wave0write
                [1] = new SkillData { Id = 1, NameKey = "skill.slash", SkillType = SkillType.Active, AcquireType = AcquireType.Default, MpCost = 0, DamageScale = 1.2f, TargetType = SkillTargetType.EnemySingle, ActionDuration = 0.35f, EffectKey = "Destruction_air_normal" }, //Wave0write
                [2] = new SkillData { Id = 2, NameKey = "skill.fire", SkillType = SkillType.Active, AcquireType = AcquireType.Default, MpCost = 4, DamageScale = 0.85f, TargetType = SkillTargetType.EnemyAll, ActionDuration = 0.5f, CooldownRounds = 1, EffectKey = "explosion_3" }, //Wave0write
                [3] = new SkillData { Id = 3, NameKey = "skill.heal", SkillType = SkillType.Active, AcquireType = AcquireType.Default, MpCost = 3, HealScale = 1.1f, TargetType = SkillTargetType.AllySingle, ActionDuration = 0.45f, EffectKey = "Destruction_air_blue" }, //Wave0write
                [4] = new SkillData { Id = 4, NameKey = "skill.power_strike", SkillType = SkillType.Active, AcquireType = AcquireType.Shop, PurchasePrice = 1, MpCost = 5, DamageScale = 2.0f, TargetType = SkillTargetType.EnemySingle, ActionDuration = 0.45f, CooldownRounds = 1, EffectKey = "Shotgun_hit_normal" },
                [5] = new SkillData { Id = 5, NameKey = "skill.cleave", SkillType = SkillType.Active, AcquireType = AcquireType.Shop, PurchasePrice = 1, MpCost = 5, DamageScale = 1.25f, TargetType = SkillTargetType.EnemyAll, ActionDuration = 0.5f, CooldownRounds = 1, EffectKey = "Destruction_air_normal" },
                [6] = new SkillData { Id = 6, NameKey = "skill.execution", SkillType = SkillType.Active, AcquireType = AcquireType.Shop, PurchasePrice = 2, MpCost = 8, DamageScale = 2.8f, TargetType = SkillTargetType.EnemySingle, ActionDuration = 0.55f, CooldownRounds = 2, EffectKey = "Shotgun_hit_normal" },
                [7] = new SkillData { Id = 7, NameKey = "skill.flame_burst", SkillType = SkillType.Active, AcquireType = AcquireType.Shop, PurchasePrice = 2, MpCost = 7, DamageScale = 1.7f, TargetType = SkillTargetType.EnemyAll, ActionDuration = 0.6f, CooldownRounds = 2, EffectKey = "explosion_3" },
                [900] = new SkillData { Id = 900, NameKey = "skill.bite", SkillType = SkillType.Active, AcquireType = AcquireType.MonsterOnly, MpCost = 0, DamageScale = 1.05f, TargetType = SkillTargetType.EnemySingle, ActionDuration = 0.35f, EffectKey = "Destruction_air_blue" }, //Wave0write
            }; //Wave0write
        } //Wave0write

        // Guid3 §9.F.1 2026-05-27: 직업별 시작 스킬 ID 매핑. Player.ApplyStartingClass 가 호출.
        // 추후 SkillTable.csv / RuneTable.csv 에 시작 스킬 정의가 들어오면 그쪽 권위로 교체.
        /// <summary>직업별 시작 스킬 ID 2개. 0 은 빈 슬롯.</summary>
        public int[] GetStartingSkillIds(RuneClass cls)
        {
            switch (cls)
            {
                case RuneClass.Dealer:      return new int[] { 1, 0 };
                case RuneClass.Tanker:      return new int[] { 1, 0 };
                case RuneClass.MagicDealer: return new int[] { 2, 0 };
                case RuneClass.Supporter:   return new int[] { 3, 0 };
                default:                    return new int[] { 0, 0 };
            }
        }

        private static Dictionary<int, ItemData> BuildFallbackItems() //Wave0write
        { //Wave0write
            return new Dictionary<int, ItemData> //Wave0write
            { //Wave0write
                [1] = new ItemData { Id = 1, NameKey = "item.hp_potion", Category = ItemCategory.Consumable, SubCategory = "HP_Potion", ConsumeEffectKey = "HealHP", ParamValue = 30f, BasePrice = 1, Stackable = true, MaxStack = 99 }, //Wave0write
                [2] = new ItemData { Id = 2, NameKey = "item.mp_potion", Category = ItemCategory.Consumable, SubCategory = "MP_Potion", ConsumeEffectKey = "HealMP", ParamValue = 12f, BasePrice = 1, Stackable = true, MaxStack = 99 }, //Wave0write
                [3] = new ItemData { Id = 3, NameKey = "item.escape", Category = ItemCategory.Consumable, SubCategory = "Escape", ConsumeEffectKey = "Escape", ParamValue = 0f, BasePrice = 30, Stackable = true, MaxStack = 10, IsRetreat = true }, //Wave0write
                [101] = new ItemData { Id = 101, NameKey = "item.training_sword", Category = ItemCategory.Equipment, SubCategory = "Weapon", EquipSlot = EquipmentSlotId.Weapon, EquipMod = new EquipmentStatMod { ATK = 3 }, ParamValue = 3f, BasePrice = 40, Stackable = false, MaxStack = 1 }, //Wave0write
                [201] = new ItemData { Id = 201, NameKey = "item.cloth_armor", Category = ItemCategory.Equipment, SubCategory = "ArmorBody", EquipSlot = EquipmentSlotId.ArmorBody, EquipMod = new EquipmentStatMod { HP = 10, DEF = 1 }, ParamValue = 1f, BasePrice = 35, Stackable = false, MaxStack = 1 }, //Wave0write
                [901] = new ItemData { Id = 901, NameKey = "test.weapon", Category = ItemCategory.Equipment, SubCategory = "Weapon", EquipSlot = EquipmentSlotId.Weapon, EquipMod = new EquipmentStatMod { ATK = 5 }, ParamValue = 5f, BasePrice = 1, Stackable = false, MaxStack = 1 }, //Wave0write
                [902] = new ItemData { Id = 902, NameKey = "test.body", Category = ItemCategory.Equipment, SubCategory = "ArmorBody", EquipSlot = EquipmentSlotId.ArmorBody, EquipMod = new EquipmentStatMod { HP = 15, DEF = 2 }, ParamValue = 2f, BasePrice = 1, Stackable = false, MaxStack = 1 }, //Wave0write
                [903] = new ItemData { Id = 903, NameKey = "test.arms", Category = ItemCategory.Equipment, SubCategory = "ArmorArms", EquipSlot = EquipmentSlotId.ArmorArms, EquipMod = new EquipmentStatMod { ATK = 1, DEF = 1 }, ParamValue = 1f, BasePrice = 1, Stackable = false, MaxStack = 1 }, //Wave0write
                [904] = new ItemData { Id = 904, NameKey = "test.legs", Category = ItemCategory.Equipment, SubCategory = "ArmorLegs", EquipSlot = EquipmentSlotId.ArmorLegs, EquipMod = new EquipmentStatMod { SPD = 2, DEF = 1 }, ParamValue = 2f, BasePrice = 1, Stackable = false, MaxStack = 1 }, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static Dictionary<int, RuneData> BuildFallbackRunes() //Wave0write
        { //Wave0write
            return new Dictionary<int, RuneData> //Wave0write
            { //Wave0write
                [1] = new RuneData { Id = 1, NameKey = "rune.dealer.start", RuneType = RuneNodeType.MainNode, ClassId = RuneClass.Dealer, RequiredRuneId = 0, PointCost = 0, EffectType = RuneEffectType.AddATK, EffectValue = 2f }, //Wave0write
                [2] = new RuneData { Id = 2, NameKey = "rune.tanker.start", RuneType = RuneNodeType.MainNode, ClassId = RuneClass.Tanker, RequiredRuneId = 0, PointCost = 0, EffectType = RuneEffectType.AddDEF, EffectValue = 2f }, //Wave0write
                [3] = new RuneData { Id = 3, NameKey = "rune.magic.start", RuneType = RuneNodeType.MainNode, ClassId = RuneClass.MagicDealer, RequiredRuneId = 0, PointCost = 0, EffectType = RuneEffectType.AddMaxMP, EffectValue = 8f }, //Wave0write
                [4] = new RuneData { Id = 4, NameKey = "rune.support.start", RuneType = RuneNodeType.MainNode, ClassId = RuneClass.Supporter, RequiredRuneId = 0, PointCost = 0, EffectType = RuneEffectType.AddMaxHP, EffectValue = 12f }, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static Dictionary<int, List<DropEntry>> BuildFallbackDropTables() //Wave0write
        { //Wave0write
            return new Dictionary<int, List<DropEntry>> //Wave0write
            { //Wave0write
                [1] = new List<DropEntry> { new DropEntry { Id = 1001, DropTableId = 1, ItemId = 1, MinCount = 1, MaxCount = 1, DropRate = 0.5f } }, //Wave0write
                [2] = new List<DropEntry> { new DropEntry { Id = 2001, DropTableId = 2, ItemId = 1, MinCount = 1, MaxCount = 2, DropRate = 0.45f }, new DropEntry { Id = 2002, DropTableId = 2, ItemId = 2, MinCount = 1, MaxCount = 1, DropRate = 0.25f } }, //Wave0write
                [3] = new List<DropEntry> { new DropEntry { Id = 3001, DropTableId = 3, ItemId = 101, MinCount = 1, MaxCount = 1, DropRate = 0.2f }, new DropEntry { Id = 3002, DropTableId = 3, ItemId = 201, MinCount = 1, MaxCount = 1, DropRate = 0.2f } }, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static WorldData BuildFallbackWorld() //Wave0write
        { //Wave0write
            return new WorldData //Wave0write
            { //Wave0write
                FloorGen = new FloorGenRule //Wave0write
                { //Wave0write
                    MaxFloor = 49, //Wave0write
                    MinNodesPerFloor = 1, //Wave0write
                    MaxNodesPerFloor = 3, //Wave0write
                    TutorialNodeCounts = new List<int> { 1, 2, 1 }, //Wave0write
                    MonstersMin = 1, //Wave0write
                    MonstersMax = 3, //Wave0write
                }, //Wave0write
                Stages = new List<StageDef> //Wave0write
                { //Wave0write
                    new StageDef { StageIndex = 1, FloorStart = 1, FloorEnd = 2, BossFloor = 3, UnlocksSafeZoneIndex = 1, DifficultyMin = 1, DifficultyMax = 2 }, //Wave0write
                    new StageDef { StageIndex = 2, FloorStart = 5, FloorEnd = 10, BossFloor = 11, UnlocksSafeZoneIndex = 2, DifficultyMin = 2, DifficultyMax = 4 }, //Wave0write
                    new StageDef { StageIndex = 3, FloorStart = 13, FloorEnd = 18, BossFloor = 19, UnlocksSafeZoneIndex = 3, DifficultyMin = 3, DifficultyMax = 6 }, //Wave0write
                    new StageDef { StageIndex = 4, FloorStart = 21, FloorEnd = 28, BossFloor = 29, UnlocksSafeZoneIndex = 4, DifficultyMin = 4, DifficultyMax = 8 }, //Wave0write
                    new StageDef { StageIndex = 5, FloorStart = 31, FloorEnd = 38, BossFloor = 39, UnlocksSafeZoneIndex = 5, DifficultyMin = 5, DifficultyMax = 10 }, //Wave0write
                    new StageDef { StageIndex = 6, FloorStart = 41, FloorEnd = 48, BossFloor = 49, UnlocksSafeZoneIndex = 5, DifficultyMin = 6, DifficultyMax = 12 }, //Wave0write
                }, //Wave0write
                SafeZones = new List<SafeZoneDef> //Wave0write
                { //Wave0write
                    new SafeZoneDef { Index = 0, FloorNumber = 0, FeatureKeys = new List<string>() }, //Wave0write
                    new SafeZoneDef { Index = 1, FloorNumber = 4, FeatureKeys = new List<string> { "Inn", "Shop", "Guild", "Temple" } }, //Wave0write
                    new SafeZoneDef { Index = 2, FloorNumber = 12, FeatureKeys = new List<string> { "ErosionAltar" } }, //Wave0write
                    new SafeZoneDef { Index = 3, FloorNumber = 20, FeatureKeys = new List<string> { "Mine" } }, //Wave0write
                    new SafeZoneDef { Index = 4, FloorNumber = 30, FeatureKeys = new List<string> { "Mine" } }, //Wave0write
                    new SafeZoneDef { Index = 5, FloorNumber = 40, FeatureKeys = new List<string> { "Mine" } }, //Wave0write
                }, //Wave0write
                MonsterPoolWeights = new List<int> { 1, 1, 1 }, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static BalanceData BuildFallbackBalance() //Wave0write
        { //Wave0write
            return new BalanceData //Wave0write
            { //Wave0write
                ExpToNextLevel = new List<int> { 0, 10, 25, 45, 70, 100, 140, 190, 250, 320, 400 }, //Wave0write
                RunePointPerLevel = 1, //Wave0write
                ErosionCurve = new ErosionCurve { DailyBase = 1f, ExpBase = 1.05f, InflectionDay = 10 }, //Wave0write
                ErosionMonsterMultiplier = 1.5f, //Wave0write
                InflationCoef = 0.5f, //Wave0write
                SellRatio = 0.5f, //Wave0write
                EnhanceMultiplier = 0.1f, //Wave0write
                MineDailyGain = new List<int> { 1, 2, 3 }, //Wave0write
                ErosionAltarReduction = 10f, //Wave0write
                ErosionAltarCost = 3, //Wave0write
                MinActionTimeSec = 0.1f, //Wave0write
            }; //Wave0write
        } //Wave0write
    }
}


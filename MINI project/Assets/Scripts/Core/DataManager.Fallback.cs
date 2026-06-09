using System.Collections.Generic;
using UnityEngine;

public sealed partial class DataManager
{
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
                EffectKey = "slash",
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
                EffectKey = "fire",
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
                EffectKey = "first_aid",
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
                EffectKey = "power_strike",
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
                EffectKey = "cleave",
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
                EffectKey = "execution",
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
                EffectKey = "flame_burst",
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
                EffectKey = "guard_aegis",
            },
        };
    }

    private static Dictionary<int, SkillRuneModifierData> BuildFallbackSkillRuneModifiers()
    {
        return new Dictionary<int, SkillRuneModifierData>();
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
            FirstNonPlayerActionDelaySec = 0.9f,
            AttackActionTimeSec = 0.3f,
            SkillActionFallbackSec = 0.5f,
            DefendActionTimeSec = 0.1f,
            CombatGeneratedSpriteSize = 64,
            CombatGeneratedSpritePixelsPerUnit = 48f,
            UseErosionMonsterMultiplierCurve = false,
            ErosionMonsterMultiplierCurvePower = 1f,
            StartingLoadout = new StartingLoadout
            {
                Gold = 1000,
                BaseMaxHP = 90,
                BaseMaxMP = 20,
                BaseATK = 10,
                BaseDEF = 2,
                BaseSPD = 10,
                EquipmentItemIds = new List<int> { 101, 102, 103, 104 },
                InventoryStacks = new List<StartingItemStack>
                {
                    new StartingItemStack { ItemId = 1, Count = 2 },
                    new StartingItemStack { ItemId = 3, Count = 1 },
                },
                ConsumableSlotItemIds = new List<int> { 1, 3 },
            },
        };
    }

}

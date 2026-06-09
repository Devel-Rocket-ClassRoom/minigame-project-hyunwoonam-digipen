using System.Collections.Generic;
using NUnit.Framework;

public sealed class SkillRuneSystemTests
{
    [Test]
    public void SkillData_PassiveColumns_DefaultAndParsed()
    {
        var row = new Dictionary<string, string>
        {
            ["Id"] = "1201",
            ["NameKey"] = "skill.passive.test",
            ["SkillType"] = "Passive",
            ["AcquireType"] = "Rune",
            ["MpCost"] = "0",
            ["TargetType"] = "Self",
        };

        SkillData defaults = SkillData.FromRow(row);
        Assert.AreEqual(PassiveStatType.None, defaults.PassiveStatType);
        Assert.AreEqual(0, defaults.PassiveFlatValue);
        Assert.AreEqual(0f, defaults.PassivePercentValue);

        row["PassiveStatType"] = "ATK";
        row["PassiveFlatValue"] = "3";
        row["PassivePercentValue"] = "0.2";

        SkillData parsed = SkillData.FromRow(row);
        Assert.AreEqual(PassiveStatType.ATK, parsed.PassiveStatType);
        Assert.AreEqual(3, parsed.PassiveFlatValue);
        Assert.AreEqual(0.2f, parsed.PassivePercentValue);
    }

    [Test]
    public void SkillRuneModifierData_ParsedWhenPresent()
    {
        var row = new Dictionary<string, string>
        {
            ["Id"] = "1",
            ["SkillId"] = "7",
            ["RuneClass"] = "MagicDealer",
            ["DamageScaleMul"] = "1.18",
            ["HealScaleMul"] = "1",
            ["ShieldScaleMul"] = "1",
            ["MpCostDelta"] = "2",
            ["CooldownDelta"] = "-1",
            ["EffectKeyOverride"] = "Destruction_air_blue",
            ["SfxKeyOverride"] = "",
            ["DescAppendKey"] = "skill.mod.magic_aoe",
        };

        SkillRuneModifierData data = SkillRuneModifierData.FromRow(row);
        Assert.IsNotNull(data);
        Assert.AreEqual(1, data.Id);
        Assert.AreEqual(7, data.SkillId);
        Assert.AreEqual(RuneClass.MagicDealer, data.RuneClass);
        Assert.AreEqual(1.18f, data.DamageScaleMul);
        Assert.AreEqual(2, data.MpCostDelta);
        Assert.AreEqual(-1, data.CooldownDelta);
        Assert.AreEqual("Destruction_air_blue", data.EffectKeyOverride);
        Assert.AreEqual("skill.mod.magic_aoe", data.DescAppendKey);
    }

    [Test]
    public void SkillRuntimeResolver_ResolvesModifierWithoutMutatingSource()
    {
        var source = new SkillData
        {
            Id = 7,
            NameKey = "skill.flame_burst",
            SkillType = SkillType.Active,
            AcquireType = AcquireType.Shop,
            MpCost = 7,
            DamageScale = 1.7f,
            TargetType = SkillTargetType.EnemyAll,
            CooldownRounds = 2,
            EffectKey = "explosion_3",
        };
        var modifier = new SkillRuneModifierData
        {
            SkillId = 7,
            RuneClass = RuneClass.MagicDealer,
            DamageScaleMul = 1.18f,
            HealScaleMul = 1f,
            ShieldScaleMul = 1f,
            MpCostDelta = 2,
            CooldownDelta = -1,
            EffectKeyOverride = "Destruction_air_blue",
        };

        SkillData effective = SkillRuntimeResolver.Resolve(source, RuneClass.MagicDealer, modifier);

        Assert.AreNotSame(source, effective);
        Assert.AreEqual(1.7f, source.DamageScale);
        Assert.AreEqual(7, source.MpCost);
        Assert.AreEqual(2, source.CooldownRounds);
        Assert.AreEqual("explosion_3", source.EffectKey);
        Assert.AreEqual(1.7f * 1.18f, effective.DamageScale);
        Assert.AreEqual(9, effective.MpCost);
        Assert.AreEqual(1, effective.CooldownRounds);
        Assert.AreEqual("Destruction_air_blue", effective.EffectKey);
    }

    [Test]
    public void SkillRuntimeResolver_ClampsMpAndCooldownToZero()
    {
        var source = new SkillData
        {
            Id = 3,
            NameKey = "skill.heal",
            SkillType = SkillType.Active,
            AcquireType = AcquireType.Default,
            MpCost = 1,
            HealScale = 1.4f,
            TargetType = SkillTargetType.AllySingle,
            CooldownRounds = 0,
        };
        var modifier = new SkillRuneModifierData
        {
            SkillId = 3,
            RuneClass = RuneClass.Supporter,
            DamageScaleMul = 1f,
            HealScaleMul = 1.25f,
            ShieldScaleMul = 1f,
            MpCostDelta = -5,
            CooldownDelta = -2,
        };

        SkillData effective = SkillRuntimeResolver.Resolve(source, RuneClass.Supporter, modifier);

        Assert.AreEqual(0, effective.MpCost);
        Assert.AreEqual(0, effective.CooldownRounds);
        Assert.AreEqual(1.4f * 1.25f, effective.HealScale);
    }

    [Test]
    public void DataManager_LoadAll_LoadsRuneModifiersAndPassiveUnlocks()
    {
        var data = new DataManager();
        data.LoadAll();

        Assert.IsTrue(data.TryGetSkillRuneModifier(7, RuneClass.MagicDealer, out SkillRuneModifierData modifier));
        Assert.AreEqual(1.18f, modifier.DamageScaleMul);
        Assert.AreEqual(2, modifier.MpCostDelta);

        Assert.IsTrue(data.Skills.TryGetValue(1101, out SkillData passive));
        Assert.AreEqual(SkillType.Passive, passive.SkillType);
        Assert.AreEqual(PassiveStatType.ATK, passive.PassiveStatType);

        Assert.IsTrue(data.Runes.TryGetValue(106, out RuneData rune));
        Assert.AreEqual(RuneEffectType.UnlockSkill, rune.EffectType);
        Assert.AreEqual(1101, (int)rune.EffectValue);
    }
}

using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// MonsterData/SkillData 신규 차별화 컬럼(AttackEffectKey/AttackAnimIndex/AttackSfxKey/SfxKey)
/// 파싱 회귀 테스트. 미지정 시 기본값(빈 문자열 / 0 / 무음) → 동작 보존 보장.
/// 모델에 컬럼 추가 후 매핑 누락 시 실패로 노출. Unity Test Runner(EditMode).
/// </summary>
public sealed class MonsterDataColumnsTests
{
    private static Dictionary<string, string> BaseMonsterRow()
    {
        // FromRow 가 요구하는 최소 컬럼.
        return new Dictionary<string, string>
        {
            ["Id"] = "9001",
            ["NameKey"] = "monster.test",
            ["IsBoss"] = "false",
            ["Difficulty"] = "1",
            ["MaxHP"] = "10",
            ["MaxMP"] = "0",
            ["ATK"] = "3",
            ["DEF"] = "1",
            ["SPD"] = "5",
            ["RewardExp"] = "5",
            ["RewardGold"] = "5",
            ["DropTableId"] = "0",
        };
    }

    [Test]
    public void MonsterData_NewColumns_DefaultWhenAbsent()
    {
        MonsterData data = MonsterData.FromRow(BaseMonsterRow());
        Assert.IsNotNull(data);
        Assert.AreEqual(string.Empty, data.AttackEffectKey ?? string.Empty);
        Assert.AreEqual(0, data.AttackAnimIndex);
        Assert.AreEqual(string.Empty, data.AttackSfxKey ?? string.Empty);
    }

    [Test]
    public void MonsterData_NewColumns_ParsedWhenPresent()
    {
        Dictionary<string, string> row = BaseMonsterRow();
        row["AttackEffectKey"] = "slash";
        row["AttackAnimIndex"] = "2";
        row["AttackSfxKey"] = "sfx_slash";

        MonsterData data = MonsterData.FromRow(row);
        Assert.AreEqual("slash", data.AttackEffectKey);
        Assert.AreEqual(2, data.AttackAnimIndex);
        Assert.AreEqual("sfx_slash", data.AttackSfxKey);
    }

    [Test]
    public void SkillData_SfxKey_DefaultAndParsed()
    {
        Dictionary<string, string> row = new Dictionary<string, string>
        {
            ["Id"] = "9001",
            ["NameKey"] = "skill.test",
            ["SkillType"] = "Active",
            ["AcquireType"] = "Default",
            ["MpCost"] = "0",
            ["TargetType"] = "EnemySingle",
        };
        SkillData noSfx = SkillData.FromRow(row);
        Assert.AreEqual(string.Empty, noSfx.SfxKey ?? string.Empty);

        row["SfxKey"] = "sfx_fire";
        SkillData withSfx = SkillData.FromRow(row);
        Assert.AreEqual("sfx_fire", withSfx.SfxKey);
    }
}

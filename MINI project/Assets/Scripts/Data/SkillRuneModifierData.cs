using System.Collections.Generic;

/// <summary>
/// 액티브 스킬이 룬 직업에 따라 받는 런타임 보정 데이터.
/// 원본 SkillData는 유지하고, 전투 행동 1회마다 effective SkillData를 계산할 때만 사용한다.
/// </summary>
public sealed class SkillRuneModifierData : DataTable
{
    /// <summary>보정 대상 스킬 ID.</summary>
    public int SkillId;

    /// <summary>보정이 적용되는 룬 직업.</summary>
    public RuneClass RuneClass;

    /// <summary>데미지 배율 보정. 기본 1.</summary>
    public float DamageScaleMul = 1f;

    /// <summary>회복 배율 보정. 기본 1.</summary>
    public float HealScaleMul = 1f;

    /// <summary>보호막 배율 보정. 기본 1.</summary>
    public float ShieldScaleMul = 1f;

    /// <summary>MP 비용 델타.</summary>
    public int MpCostDelta;

    /// <summary>쿨다운 라운드 델타.</summary>
    public int CooldownDelta;

    /// <summary>이펙트 키 대체값. 빈 값이면 원본 SkillData.EffectKey 사용.</summary>
    public string EffectKeyOverride;

    /// <summary>SFX 키 대체값. 빈 값이면 원본 SkillData.SfxKey 사용.</summary>
    public string SfxKeyOverride;

    /// <summary>UI 설명에 덧붙일 로컬라이즈 키.</summary>
    public string DescAppendKey;

    /// <inheritdoc/>
    public override void Parse(string[] cells)
    {
        Id = cells.Length > 0 && CsvParser.TryParseInt(cells[0], out int id) ? id : 0;
        SkillId = cells.Length > 1 && CsvParser.TryParseInt(cells[1], out int skillId) ? skillId : 0;
        RuneClass =
            cells.Length > 2 && System.Enum.TryParse(cells[2], true, out RuneClass runeClass)
                ? runeClass
                : RuneClass.None;
        DamageScaleMul = cells.Length > 3 && CsvParser.TryParseFloat(cells[3], out float damageMul) ? damageMul : 1f;
        HealScaleMul = cells.Length > 4 && CsvParser.TryParseFloat(cells[4], out float healMul) ? healMul : 1f;
        ShieldScaleMul = cells.Length > 5 && CsvParser.TryParseFloat(cells[5], out float shieldMul) ? shieldMul : 1f;
        MpCostDelta = cells.Length > 6 && CsvParser.TryParseInt(cells[6], out int mpDelta) ? mpDelta : 0;
        CooldownDelta = cells.Length > 7 && CsvParser.TryParseInt(cells[7], out int cooldownDelta) ? cooldownDelta : 0;
        EffectKeyOverride = cells.Length > 8 ? cells[8] : string.Empty;
        SfxKeyOverride = cells.Length > 9 ? cells[9] : string.Empty;
        DescAppendKey = cells.Length > 10 ? cells[10] : string.Empty;
    }

    public static SkillRuneModifierData FromRow(IDictionary<string, string> row)
    {
        if (!CsvParser.HasColumns(row, nameof(SkillRuneModifierData), "Id", "SkillId", "RuneClass"))
        {
            return null;
        }

        return new SkillRuneModifierData
        {
            Id = CsvParser.GetInt(row, "Id"),
            SkillId = CsvParser.GetInt(row, "SkillId"),
            RuneClass = CsvParser.GetEnum(row, "RuneClass", RuneClass.None),
            DamageScaleMul = CsvParser.GetFloat(row, "DamageScaleMul", 1f),
            HealScaleMul = CsvParser.GetFloat(row, "HealScaleMul", 1f),
            ShieldScaleMul = CsvParser.GetFloat(row, "ShieldScaleMul", 1f),
            MpCostDelta = CsvParser.GetInt(row, "MpCostDelta"),
            CooldownDelta = CsvParser.GetInt(row, "CooldownDelta"),
            EffectKeyOverride = CsvParser.GetString(row, "EffectKeyOverride"),
            SfxKeyOverride = CsvParser.GetString(row, "SfxKeyOverride"),
            DescAppendKey = CsvParser.GetString(row, "DescAppendKey"),
        };
    }
}

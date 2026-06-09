using UnityEngine;

/// <summary>
/// 룬 직업에 따른 액티브 스킬 보정을 전투 행동 단위의 effective SkillData로 해석한다.
/// </summary>
public static class SkillRuntimeResolver
{
    public static SkillData Resolve(Skill skill, EntityBase caster)
    {
        DataManager data = GameSystemManager.TryGetInstance(out GameSystemManager gsm) ? gsm.Data : null;
        return Resolve(skill, caster, data);
    }

    public static SkillData Resolve(Skill skill, EntityBase caster, DataManager data)
    {
        if (skill?.Data == null)
        {
            return null;
        }

        RuneClass runeClass = ResolveRuneClass(caster);
        SkillRuneModifierData modifier = null;
        data?.TryGetSkillRuneModifier(skill.Data.Id, runeClass, out modifier);
        return Resolve(skill.Data, runeClass, modifier);
    }

    public static SkillData Resolve(SkillData source, RuneClass runeClass, SkillRuneModifierData modifier)
    {
        if (source == null)
        {
            return null;
        }

        if (
            modifier == null
            || runeClass == RuneClass.None
            || modifier.SkillId != source.Id
            || modifier.RuneClass != runeClass
        )
        {
            return source;
        }

        SkillData effective = Clone(source);
        effective.DamageScale = Mathf.Max(0f, source.DamageScale * SafeMul(modifier.DamageScaleMul));
        effective.HealScale = Mathf.Max(0f, source.HealScale * SafeMul(modifier.HealScaleMul));
        effective.ShieldScale = Mathf.Max(0f, source.ShieldScale * SafeMul(modifier.ShieldScaleMul));
        effective.MpCost = Mathf.Max(0, source.MpCost + modifier.MpCostDelta);
        effective.CooldownRounds = Mathf.Max(0, source.CooldownRounds + modifier.CooldownDelta);
        if (!string.IsNullOrEmpty(modifier.EffectKeyOverride))
        {
            effective.EffectKey = modifier.EffectKeyOverride;
        }

        if (!string.IsNullOrEmpty(modifier.SfxKeyOverride))
        {
            effective.SfxKey = modifier.SfxKeyOverride;
        }

        effective.RuntimeDescAppendKey = modifier.DescAppendKey;
        return effective;
    }

    public static SkillData Resolve(SkillData source, RuneClass runeClass, DataManager data)
    {
        SkillRuneModifierData modifier = null;
        data?.TryGetSkillRuneModifier(source != null ? source.Id : 0, runeClass, out modifier);
        return Resolve(source, runeClass, modifier);
    }

    public static RuneClass ResolveRuneClass(EntityBase caster)
    {
        if (caster is Player player)
        {
            if (player.Rune != null && player.Rune.ClassId != RuneClass.None)
            {
                return player.Rune.ClassId;
            }

            return player.StartingClass;
        }

        if (caster is TeamBase companion && companion.Rune != null)
        {
            return companion.Rune.ClassId;
        }

        return RuneClass.None;
    }

    public static RuneClass ResolveRuneClass(PlayerState player)
    {
        if (player?.Rune != null && player.Rune.ClassId != RuneClass.None)
        {
            return player.Rune.ClassId;
        }

        return player != null ? player.StartingClass : RuneClass.None;
    }

    private static float SafeMul(float value)
    {
        return value > 0f ? value : 1f;
    }

    private static SkillData Clone(SkillData source)
    {
        return new SkillData
        {
            Id = source.Id,
            NameKey = source.NameKey,
            DescKey = source.DescKey,
            SkillType = source.SkillType,
            AcquireType = source.AcquireType,
            PurchasePrice = source.PurchasePrice,
            MpCost = source.MpCost,
            DamageScale = source.DamageScale,
            HealScale = source.HealScale,
            ShieldScale = source.ShieldScale,
            TargetType = source.TargetType,
            AnimationKey = source.AnimationKey,
            EffectKey = source.EffectKey,
            SfxKey = source.SfxKey,
            ActionDuration = source.ActionDuration,
            CooldownRounds = source.CooldownRounds,
            ShieldDurationRounds = source.ShieldDurationRounds,
            PassiveStatType = source.PassiveStatType,
            PassiveFlatValue = source.PassiveFlatValue,
            PassivePercentValue = source.PassivePercentValue,
            RuntimeDescAppendKey = source.RuntimeDescAppendKey,
        };
    }
}

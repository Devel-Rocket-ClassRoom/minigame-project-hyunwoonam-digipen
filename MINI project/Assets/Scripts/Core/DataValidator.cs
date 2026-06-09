using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DataValidator
{
    public static void Validate(DataManager data)
    {
        var errors = new List<string>();

        if (data == null)
        {
            errors.Add("DataManager is null.");
            FailIfNeeded(errors);

            return;
        }

        ValidateRequiredTables(data, errors);
        ValidateReferences(data, errors);
        ValidateRanges(data, errors);
        ValidateBalance(data.Balance, errors);
        FailIfNeeded(errors);
    }

    private static void ValidateRequiredTables(DataManager data, List<string> errors)
    {
        if (data.Monsters == null || data.Monsters.Count == 0)
        { 
            errors.Add("Monsters table is empty.");
        }
        if (data.Skills == null || data.Skills.Count == 0)
        { 
            errors.Add("Skills table is empty.");
        }
        if (data.SkillRuneModifiers == null)
        {
            errors.Add("SkillRuneModifiers table is null.");
        }
        if (data.Items == null || data.Items.Count == 0) 
        { 
            errors.Add("Items table is empty."); 
        }
        if (data.Runes == null || data.Runes.Count == 0) 
        { 
            errors.Add("Runes table is empty.");
        }
        if (data.DropTables == null)
        { 
            errors.Add("DropTables table is null.");
        }
        if (data.World == null) 
        { 
            errors.Add("World data is null."); 
        }
        if (data.Balance == null) 
        { 
            errors.Add("Balance data is null.");
        }
        if (data.Language == null) 
        { 
            errors.Add("Language data is null.");
        }
    }

    private static void ValidateReferences(DataManager data, List<string> errors)
    {
        if (data.Monsters != null)
        {
            foreach (MonsterData monster in data.Monsters.Values)
            {
                if (monster == null)
                {
                    errors.Add("Monster row is null.");
                    continue;
                }

                if (monster.DropTableId != 0 && (data.DropTables == null || !data.DropTables.ContainsKey(monster.DropTableId)))
                {
                    errors.Add("Monster " + monster.Id + " references missing DropTableId " + monster.DropTableId + ".");
                }

                ValidateEffectKey(errors, "Monster " + monster.Id + " AttackEffectKey", monster.AttackEffectKey);
                ValidateSfxKey(errors, "Monster " + monster.Id + " AttackSfxKey", monster.AttackSfxKey);

                if (monster.SkillIds == null)
                {
                    continue;
                }

                for (int i = 0; i < monster.SkillIds.Count; i++)
                {
                    int skillId = monster.SkillIds[i];

                    if (data.Skills == null || !data.Skills.ContainsKey(skillId))
                    {
                        errors.Add("Monster " + monster.Id + " references missing SkillId " + skillId + ".");
                    }
                }
            }
        }

        if (data.DropTables != null)
        {
            foreach (KeyValuePair<int, List<DropEntry>> group in data.DropTables)
            {
                if (group.Value == null)
                {
                    errors.Add("DropTable " + group.Key + " entries are null.");
                    continue;
                }

                for (int i = 0; i < group.Value.Count; i++)
                {
                    DropEntry entry = group.Value[i];

                    if (entry == null)
                    {
                        errors.Add("DropTable " + group.Key + " has null entry.");
                        continue;
                    }

                    if (data.Items == null || !data.Items.ContainsKey(entry.ItemId))
                    {
                        errors.Add("DropEntry " + entry.Id + " references missing ItemId " + entry.ItemId + ".");
                    }
                }
            }
        }

        if (data.Runes != null)
        {
            foreach (RuneData rune in data.Runes.Values)
            {
                if (rune == null)
                {
                    errors.Add("Rune row is null.");
                    continue;
                }

                if (rune.RequiredRuneId != 0 && !data.Runes.ContainsKey(rune.RequiredRuneId))
                {
                    errors.Add("Rune " + rune.Id + " references missing RequiredRuneId " + rune.RequiredRuneId + ".");
                }

                if (rune.EffectType == RuneEffectType.UnlockSkill)
                {
                    int skillId = (int)rune.EffectValue;
                    if (data.Skills == null || !data.Skills.TryGetValue(skillId, out SkillData skill))
                    {
                        errors.Add("Rune " + rune.Id + " references missing unlock SkillId " + skillId + ".");
                    }
                    else if (skill.SkillType != SkillType.Passive)
                    {
                        errors.Add("Rune " + rune.Id + " UnlockSkill must reference Passive skill " + skillId + ".");
                    }
                }
            }
        }

        if (data.Skills != null)
        {
            foreach (SkillData skill in data.Skills.Values)
            {
                if (skill == null)
                {
                    continue;
                }

                ValidateEffectKey(errors, "Skill " + skill.Id + " EffectKey", skill.EffectKey);
                ValidateSfxKey(errors, "Skill " + skill.Id + " SfxKey", skill.SfxKey);
            }
        }

        if (data.SkillRuneModifiers != null)
        {
            var seenPairs = new HashSet<string>();
            foreach (SkillRuneModifierData modifier in data.SkillRuneModifiers.Values)
            {
                if (modifier == null)
                {
                    errors.Add("SkillRuneModifier row is null.");
                    continue;
                }

                if (modifier.RuneClass == RuneClass.None)
                {
                    errors.Add("SkillRuneModifier " + modifier.Id + " has invalid RuneClass None.");
                }

                if (data.Skills == null || !data.Skills.TryGetValue(modifier.SkillId, out SkillData skill))
                {
                    errors.Add("SkillRuneModifier " + modifier.Id + " references missing SkillId " + modifier.SkillId + ".");
                }
                else if (skill.SkillType != SkillType.Active)
                {
                    errors.Add("SkillRuneModifier " + modifier.Id + " must reference Active skill " + modifier.SkillId + ".");
                }

                string pairKey = modifier.SkillId + ":" + modifier.RuneClass;
                if (!seenPairs.Add(pairKey))
                {
                    errors.Add("Duplicate SkillRuneModifier pair " + pairKey + ".");
                }

                ValidateEffectKey(errors, "SkillRuneModifier " + modifier.Id + " EffectKeyOverride", modifier.EffectKeyOverride);
                ValidateSfxKey(errors, "SkillRuneModifier " + modifier.Id + " SfxKeyOverride", modifier.SfxKeyOverride);
            }
        }
    }

    private static void ValidateRanges(DataManager data, List<string> errors)
    {
        if (data.World != null)
        {
            if (data.World.FloorGen == null)
            {
                errors.Add("World.FloorGen is null.");
            }

            if (data.World.Stages == null || data.World.Stages.Count == 0)
            {
                errors.Add("World.Stages is empty.");
            }
            else
            {
                for (int i = 0; i < data.World.Stages.Count; i++)
                {
                    StageDef stage = data.World.Stages[i];
                    if (stage == null)
                    {
                        errors.Add("World.Stages[" + i + "] is null.");
                        continue;
                    }

                    if (stage.FloorStart >= stage.BossFloor)
                    {
                        errors.Add("Stage " + stage.StageIndex + " FloorStart must be less than BossFloor.");
                    }

                    if (i + 1 < data.World.Stages.Count && stage.BossFloor > data.World.Stages[i + 1].FloorStart)
                    {
                        errors.Add("Stage " + stage.StageIndex + " overlaps next stage floor range.");
                    }
                }
            }

            if (data.World.SafeZones != null)
            {
                for (int i = 0; i < data.World.SafeZones.Count; i++)
                {
                    SafeZoneDef safeZone = data.World.SafeZones[i];
                    if (safeZone != null && safeZone.Index != i)
                    {
                        errors.Add("SafeZone index mismatch at list index " + i + ": " + safeZone.Index + ".");
                    }
                }
            }
        }

        if (data.Items != null)
        {
            foreach (ItemData item in data.Items.Values)
            {
                if (item == null)
                {
                    continue;
                }

                if (!System.Enum.IsDefined(typeof(EquipmentSlotId), item.EquipSlot))
                {
                    errors.Add("Item " + item.Id + " has invalid EquipSlot.");
                }

                if (item.Stackable && item.MaxStack < 1)
                {
                    errors.Add("Item " + item.Id + " has invalid MaxStack.");
                }
            }
        }

        if (data.Skills != null)
        {
            foreach (SkillData skill in data.Skills.Values)
            {
                if (skill == null)
                {
                    continue;
                }

                if (skill.MpCost < 0)
                { 
                    errors.Add("Skill " + skill.Id + " has negative MpCost.");
                }
                if (skill.CooldownRounds < 0) 
                { 
                    errors.Add("Skill " + skill.Id + " has negative CooldownRounds.");
                }
                if (skill.SkillType == SkillType.Passive && skill.PassiveStatType != PassiveStatType.None)
                {
                    if (skill.PassiveFlatValue == 0 && skill.PassivePercentValue == 0f)
                    {
                        errors.Add("Passive skill " + skill.Id + " has PassiveStatType but no passive value.");
                    }
                }
                if (skill.SkillType == SkillType.Active && skill.PassiveStatType != PassiveStatType.None)
                {
                    errors.Add("Active skill " + skill.Id + " must not define PassiveStatType.");
                }
            }
        }

        if (data.SkillRuneModifiers != null)
        {
            foreach (SkillRuneModifierData modifier in data.SkillRuneModifiers.Values)
            {
                if (modifier == null)
                {
                    continue;
                }

                if (modifier.DamageScaleMul < 0f)
                {
                    errors.Add("SkillRuneModifier " + modifier.Id + " has negative DamageScaleMul.");
                }
                if (modifier.HealScaleMul < 0f)
                {
                    errors.Add("SkillRuneModifier " + modifier.Id + " has negative HealScaleMul.");
                }
                if (modifier.ShieldScaleMul < 0f)
                {
                    errors.Add("SkillRuneModifier " + modifier.Id + " has negative ShieldScaleMul.");
                }
            }
        }
    }

    private static void ValidateBalance(BalanceData balance, List<string> errors)
    {
        if (balance == null)
        {
            return;
        }

        if (balance.ExpToNextLevel == null || balance.ExpToNextLevel.Count < 1)
        {
            errors.Add("Balance.ExpToNextLevel is empty.");
        }

        if (balance.MineDailyGain == null || balance.MineDailyGain.Count != 3)
        {
            errors.Add("Balance.MineDailyGain must contain exactly 3 values.");
        }

        if (balance.ErosionCurve == null)
        {
            errors.Add("Balance.ErosionCurve is null.");
        }
        else
        {
            if (balance.ErosionCurve.DailyBase <= 0f) 
            { 
                errors.Add("Balance.ErosionCurve.DailyBase must be greater than 0.");
            }
            if (balance.ErosionCurve.ExpBase <= 1f)
            { 
                errors.Add("Balance.ErosionCurve.ExpBase must be greater than 1.");
            }
            if (balance.ErosionCurve.InflectionDay < 0)
            { 
                errors.Add("Balance.ErosionCurve.InflectionDay must be non-negative."); 
            }
        }
    }

    private static void ValidateEffectKey(List<string> errors, string owner, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (Resources.Load<GameObject>("Effects/" + key) == null)
        {
            errors.Add(owner + " references missing effect prefab Resources/Effects/" + key + ".");
        }
    }

    private static void ValidateSfxKey(List<string> errors, string owner, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (Resources.Load<AudioClip>("Sfx/" + key) == null)
        {
            errors.Add(owner + " references missing SFX clip Resources/Sfx/" + key + ".");
        }
    }

    private static void FailIfNeeded(List<string> errors)
    {
        if (errors.Count == 0)
        {
            return;
        }

        GameLog.LogError("[DataValidator] 데이터 검증 실패 " + errors.Count + "건:\n" + string.Join("\n", errors.ToArray()));
        throw new InvalidDataException("[DataManager] 데이터 검증 실패. 위 로그 참조.");
    }
}

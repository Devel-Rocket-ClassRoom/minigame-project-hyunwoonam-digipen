using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class ShortcutInfoPanelController
{
    private Button FindButton(string path)
    {
        return FindComponent<Button>(path);
    }

    private TMP_Text FindText(string path)
    {
        return FindText(transform, path);
    }

    private T FindComponent<T>(string path)
        where T : Component
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<T>() : null;
    }

    private static TMP_Text FindText(Transform root, string path)
    {
        Transform found = root != null ? root.Find(path) : null;
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }

    private static bool TryGetSkill(int skillId, DataManager data, out SkillData skill)
    {
        skill = null;
        return data?.Skills != null && data.Skills.TryGetValue(skillId, out skill);
    }

    private static bool ContainsSkill(List<SkillEntry> entries, int skillId)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].SkillId == skillId)
            {
                return true;
            }
        }

        return false;
    }

    private static SkillEntry ToEntry(SkillData skill)
    {
        return new SkillEntry
        {
            SkillId = skill.Id,
            Name = SafeText(skill.NameKey, "Skill " + skill.Id),
        };
    }

    private static string BuildSkillDescription(SkillData skill)
    {
        string text;
        if (!string.IsNullOrWhiteSpace(skill.DescKey))
        {
            text = skill.DescKey;
        }
        else if (skill.DamageScale > 0f)
        {
            text = "Deal " + FormatPercent(skill.DamageScale) + " ATK damage to one selected enemy.";
        }
        else if (skill.HealScale > 0f)
        {
            text = "Restore " + FormatPercent(skill.HealScale) + " HP.";
        }
        else if (skill.ShieldScale > 0f)
        {
            text = "Gain shield based on " + FormatPercent(skill.ShieldScale) + " DEF.";
        }
        else
        {
            text = skill.SkillType == SkillType.Passive ? "Passive effect." : "Utility skill.";
        }

        if (!string.IsNullOrEmpty(skill.RuntimeDescAppendKey))
        {
            text += "\n" + Loc.Get(skill.RuntimeDescAppendKey);
        }

        return text;
    }

    private static string BuildSkillNote(SkillData skill)
    {
        if (skill.SkillType == SkillType.Passive)
        {
            return "Passive effects are applied automatically.";
        }

        return string.Empty;
    }

    private static string BuildSkillEffectText(SkillData skill)
    {
        if (skill.DamageScale > 0f)
        {
            return "Damage " + FormatPercent(skill.DamageScale) + " ATK";
        }

        if (skill.HealScale > 0f)
        {
            return "Heal " + FormatPercent(skill.HealScale) + " HP";
        }

        if (skill.ShieldScale > 0f)
        {
            return "Shield " + FormatPercent(skill.ShieldScale) + " DEF";
        }

        return "Utility";
    }

    private static string FormatPercent(float scale)
    {
        return Mathf.RoundToInt(scale * 100f) + "%";
    }

    private static string FormatFloor(GameRunState run)
    {
        if (run == null)
        {
            return string.Empty;
        }

        return run.CurrentFloor > 0 ? run.CurrentFloor.ToString() : "Safe0";
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}

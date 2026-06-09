using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class SafeZone1FacilityMockupUI
{
    /// <summary>SLOT 버튼을 안쪽(왼쪽)으로 추가 이동하는 비율(버튼 너비 기준). 0이면 이동 없음.</summary>
    private const float SlotButtonInwardFactor = 0.55f;

    private static void CacheShopRows(Transform rowsRoot, List<ShopRowView> rows)
    {
        if (rowsRoot == null)
        {
            return;
        }

        for (int i = 0; i < rowsRoot.childCount; i++)
        {
            rows.Add(CreateShopRowView(rowsRoot.GetChild(i)));
        }
    }

    private static ShopRowView CreateShopRowView(Transform row)
    {
        EnsureShopRowLayout(row);

        Graphic background = row.GetComponent<Graphic>();
        if (background == null)
        {
            Image image = row.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.01f);
            image.raycastTarget = true;
            background = image;
        }

        background.raycastTarget = true;
        background.color = ShopRowBaseColor;

        Outline border = row.GetComponent<Outline>();
        if (border == null)
        {
            border = row.gameObject.AddComponent<Outline>();
        }

        border.effectColor = ShopRowBorderColor;
        border.effectDistance = new Vector2(1f, -1f);
        border.useGraphicAlpha = false;

        Button button = row.GetComponent<Button>();
        if (button == null)
        {
            button = row.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = background;

        return new ShopRowView
        {
            Root = row.gameObject,
            Button = button,
            Background = background,
            BaseColor = ShopRowBaseColor,
            Border = border,
            Name = FindChildText(row, "Name"),
            Role = FindChildText(row, "Role"),
            Description = FindChildText(row, "Description"),
            Cost = FindChildText(row, "Cost"),
        };
    }

    private static void CacheGuildRows(Transform rowsRoot, List<SkillRowView> rows)
    {
        if (rowsRoot == null)
        {
            return;
        }

        for (int i = 0; i < rowsRoot.childCount; i++)
        {
            rows.Add(CreateGuildRowView(rowsRoot.GetChild(i)));
        }
    }

    private static SkillRowView CreateGuildRowView(Transform row)
    {
        Graphic background = row.GetComponent<Graphic>();
        if (background == null)
        {
            Image image = row.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.01f);
            image.raycastTarget = true;
            background = image;
        }

        background.raycastTarget = true;

        Button button = row.GetComponent<Button>();
        if (button == null)
        {
            button = row.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = background;

        TextMeshProUGUI label = row.GetComponentInChildren<TextMeshProUGUI>(true);
        return new SkillRowView
        {
            Root = row.gameObject,
            Button = button,
            Background = background,
            BaseColor = background.color,
            Label = label,
            LabelBaseColor = label != null ? label.color : Color.white,
        };
    }

    private static void EnsureRowCount(List<ShopRowView> rows, Transform rowsRoot, int count)
    {
        if (rowsRoot == null || rows.Count == 0)
        {
            return;
        }

        GameObject template = rows[0].Root;
        while (rows.Count < count)
        {
            GameObject clone = Instantiate(template, rowsRoot);
            clone.name = template.name + "_Generated_" + rows.Count;
            rows.Add(CreateShopRowView(clone.transform));
        }
    }

    private void EnsureGuildRowCount(int count)
    {
        if (guildSkillRowsRoot == null || guildSkillRows.Count == 0)
        {
            return;
        }

        GameObject template = guildSkillRows[0].Root;
        while (guildSkillRows.Count < count)
        {
            GameObject clone = Instantiate(template, guildSkillRowsRoot);
            clone.name = template.name + "_Generated_" + guildSkillRows.Count;
            guildSkillRows.Add(CreateGuildRowView(clone.transform));
        }
    }

    private void EnsureGuildSlot2Button()
    {
        if (guildPrimaryButton == null || guildSlot2Button != null)
        {
            return;
        }

        GameObject clone = Instantiate(
            guildPrimaryButton.gameObject,
            guildPrimaryButton.transform.parent
        );
        clone.name = "PrimaryButton_SLOT_2";
        guildSlot2Button = clone.GetComponent<Button>();
        guildSlot2ButtonLabel = clone.GetComponentInChildren<TextMeshProUGUI>(true);

        RectTransform primaryRect = guildPrimaryButton.GetComponent<RectTransform>();
        RectTransform slot2Rect = clone.GetComponent<RectTransform>();
        if (primaryRect != null && slot2Rect != null)
        {
            primaryRect.sizeDelta = new Vector2(
                primaryRect.sizeDelta.x * 0.48f,
                primaryRect.sizeDelta.y
            );
            slot2Rect.sizeDelta = primaryRect.sizeDelta;

            // 빌드(에디터와 다른 화면 비율)에서 SLOT 버튼이 우측 모서리에 잘려 보이는 문제 방지.
            // 두 버튼을 버튼 너비 기준 안쪽(왼쪽)으로 추가 이동한다.
            float inwardInset = primaryRect.sizeDelta.x * SlotButtonInwardFactor;
            primaryRect.anchoredPosition += new Vector2(
                -primaryRect.sizeDelta.x * 0.53f - inwardInset,
                0f
            );
            slot2Rect.anchoredPosition =
                primaryRect.anchoredPosition + new Vector2(primaryRect.sizeDelta.x * 1.12f, 0f);
        }

        ConfigureGuildButton(
            guildSlot2Button,
            guildSlot2ButtonLabel,
            string.Empty,
            false,
            null
        );
    }

    private static void CacheDetailLines(Transform body, List<TextMeshProUGUI> lines)
    {
        if (body == null)
        {
            return;
        }

        TextMeshProUGUI[] texts = body.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text != null && text.name.StartsWith("Line_", StringComparison.Ordinal))
            {
                lines.Add(text);
            }
        }
    }

    private static TextMeshProUGUI FindChildText(Transform root, string childName)
    {
        Transform child = root != null ? root.Find(childName) : null;
        return child != null ? child.GetComponentInChildren<TextMeshProUGUI>(true) : null;
    }

    private static Transform FindFirstExisting(Transform root, params string[] paths)
    {
        if (root == null || paths == null)
        {
            return null;
        }

        for (int i = 0; i < paths.Length; i++)
        {
            Transform found = root.Find(paths[i]);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static void EnsureRowsScrollContent(Transform rowsRoot)
    {
        if (rowsRoot == null || !(rowsRoot is RectTransform rowsRect))
        {
            return;
        }

        EnsureShopRowsContainerLayout(rowsRoot);
        ScrollRect scrollRect = rowsRoot.GetComponentInParent<ScrollRect>(true);
        if (scrollRect != null && scrollRect.content != rowsRect)
        {
            scrollRect.content = rowsRect;
        }
    }

    private static void EnsureShopRowsContainerLayout(Transform rowsRoot)
    {
        if (rowsRoot == null)
        {
            return;
        }

        VerticalLayoutGroup layout = rowsRoot.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = rowsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childScaleWidth = false;
        layout.childScaleHeight = false;
        layout.spacing = Mathf.Max(0f, layout.spacing);

        ContentSizeFitter fitter = rowsRoot.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = rowsRoot.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static void EnsureShopRowLayout(Transform row)
    {
        if (row == null)
        {
            return;
        }

        LayoutElement layout = row.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = row.gameObject.AddComponent<LayoutElement>();
        }

        float height = ShopRowPreferredHeight;
        if (row is RectTransform rect && rect.sizeDelta.y > 0f)
        {
            height = Mathf.Max(ShopRowPreferredHeight, rect.sizeDelta.y);
        }

        layout.ignoreLayout = false;
        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleHeight = 0f;
    }

    private static Transform FindDescendant(Transform root, string targetName)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == targetName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDescendant(root.GetChild(i), targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static string BuildItemRole(ItemData item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        return item.Category == ItemCategory.Equipment
            ? item.Category + " / " + item.EquipSlot
            : item.Category + " / " + item.SubCategory;
    }

    private static string BuildItemDescription(ItemData item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(item.DescKey))
        {
            return LocalizeKey(item.DescKey);
        }

        if (item.Category == ItemCategory.Equipment)
        {
            return BuildEquipmentStatText(item.EquipMod);
        }

        if (item.ConsumeEffectKey == "HealHP")
        {
            return "Restores HP by " + Mathf.RoundToInt(item.ParamValue);
        }

        if (item.ConsumeEffectKey == "HealMP")
        {
            return "Restores MP by " + Mathf.RoundToInt(item.ParamValue);
        }

        if (item.IsRetreat)
        {
            return "Returns to the current safe zone.";
        }

        return LocalizeKey(item.NameKey);
    }

    private static string BuildEquipmentStatText(EquipmentStatMod mod)
    {
        if (mod == null)
        {
            return "No stat modifier.";
        }

        List<string> parts = new List<string>();
        if (mod.HP != 0)
            parts.Add("HP +" + mod.HP);
        if (mod.MP != 0)
            parts.Add("MP +" + mod.MP);
        if (mod.ATK != 0)
            parts.Add("ATK +" + mod.ATK);
        if (mod.DEF != 0)
            parts.Add("DEF +" + mod.DEF);
        if (mod.SPD != 0)
            parts.Add("SPD +" + mod.SPD);
        return parts.Count > 0 ? string.Join(" / ", parts) : "No stat modifier.";
    }

    private void SetShopRow(
        ShopRowView row,
        ItemData item,
        string role,
        string description,
        string cost,
        bool selected
    )
    {
        row.Root.SetActive(true);
        if (row.Name != null)
            row.Name.text = item != null ? LocalizeKey(item.NameKey) : string.Empty;
        if (row.Role != null)
            row.Role.text = role;
        if (row.Description != null)
            row.Description.text = description;
        if (row.Cost != null)
            row.Cost.text = cost;
        SetRowSelected(row, selected);
    }

    private void SetGuildRow(
        SkillRowView row,
        SkillData skill,
        string suffix,
        bool selected,
        bool purchased
    )
    {
        row.Root.SetActive(true);
        if (row.Label != null)
        {
            row.Label.text = LocalizeKey(skill.NameKey) + "  " + suffix;
            row.Label.color = purchased ? Color.red : row.LabelBaseColor;
        }

        if (row.Background != null)
        {
            row.Background.color = selected ? selectedShopRow : row.BaseColor;
        }
    }

    private void SetRowSelected(ShopRowView row, bool selected)
    {
        if (row == null)
        {
            return;
        }

        if (row.Background != null)
        {
            row.Background.color = selected ? ShopRowSelectedColor : ShopRowBaseColor;
        }

        if (row.Border != null)
        {
            row.Border.effectColor = selected
                ? ShopRowSelectedBorderColor
                : ShopRowBorderColor;
        }

        SetTextColor(row.Name, ShopRowTextColor);
        SetTextColor(row.Role, ShopRowTextColor);
        SetTextColor(row.Description, ShopRowTextColor);
        SetTextColor(row.Cost, ShopRowTextColor);
    }

    private static void SetTextColor(TextMeshProUGUI label, Color color)
    {
        if (label != null)
        {
            label.color = color;
        }
    }

    private static void SetDetailBody(Transform body, bool active)
    {
        if (body != null)
        {
            body.gameObject.SetActive(active);
        }
    }

    private static void SetDetailLines(
        List<TextMeshProUGUI> lines,
        string name,
        string role,
        string description,
        string priceLabel,
        string price
    )
    {
        if (lines.Count > 0)
            lines[0].text = name;
        if (lines.Count > 1)
            lines[1].text = role;
        if (lines.Count > 2)
            lines[2].text = description;
        if (lines.Count > 3)
            lines[3].text = priceLabel;
        if (lines.Count > 4)
            lines[4].text = price;
    }

    private static string LocalizeKey(string key)
    {
        return string.IsNullOrEmpty(key) ? string.Empty : Loc.Get(key);
    }

    private static void ConfigureGuildButton(
        Button button,
        TextMeshProUGUI label,
        string text,
        bool interactable,
        UnityEngine.Events.UnityAction action
    )
    {
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(!string.IsNullOrEmpty(text));
        button.interactable = interactable && action != null;
        button.onClick.RemoveAllListeners();
        if (interactable && action != null)
        {
            button.onClick.AddListener(action);
        }

        if (label != null)
        {
            label.text = text ?? string.Empty;
        }
    }

    private static bool IsSkillEquipped(GameRunState run, int skillId)
    {
        int[] slots = run?.Player?.ActiveSlotSkillIds;
        return slots != null
            && (
                (slots.Length > 0 && slots[0] == skillId)
                || (slots.Length > 1 && slots[1] == skillId)
            );
    }

    private static string BuildSkillEffectText(SkillData skill)
    {
        if (skill.DamageScale > 0f)
            return "Damage x" + skill.DamageScale + " / MP " + skill.MpCost;
        if (skill.HealScale > 0f)
            return "Heal x" + skill.HealScale + " / MP " + skill.MpCost;
        if (skill.ShieldScale > 0f)
            return "Shield x" + skill.ShieldScale + " / MP " + skill.MpCost;
        return "MP " + skill.MpCost;
    }

    private static string BuildActiveSlotText(GameRunState run, DataManager data)
    {
        int[] slots = run?.Player?.ActiveSlotSkillIds;
        if (slots == null || slots.Length != 2)
        {
            return "Slot 1: Empty / Slot 2: Empty";
        }

        return "Slot 1: "
            + SkillName(slots[0], data)
            + " / Slot 2: "
            + SkillName(slots[1], data);
    }

    private static string SkillName(int skillId, DataManager data)
    {
        if (skillId == 0)
        {
            return "Empty";
        }

        return data.Skills.TryGetValue(skillId, out SkillData skill)
            ? skill.NameKey
            : "Missing " + skillId;
    }

    private static List<Button> FindButtons(Transform row)
    {
        List<Button> buttons = new List<Button>();
        if (row == null)
            return buttons;

        for (int i = 0; i < row.childCount; i++)
        {
            if (row.GetChild(i).TryGetComponent(out Button button))
            {
                buttons.Add(button);
            }
        }

        return buttons;
    }

    private static List<Button> FindNamedButtons(Transform root, string buttonName)
    {
        List<Button> buttons = new List<Button>();
        if (root == null)
            return buttons;

        Button[] candidates = root.GetComponentsInChildren<Button>(true);
        foreach (Button button in candidates)
        {
            if (button != null && button.name == buttonName)
            {
                buttons.Add(button);
            }
        }

        return buttons;
    }

    private static void AddUniqueButtons(List<Button> target, List<Button> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null && !target.Contains(source[i]))
            {
                target.Add(source[i]);
            }
        }
    }

    private static int DefaultSubFunctionIndex(string facility)
    {
        if (facility == "TAVERN")
            return 1;
        if (facility == "GUILD")
            return 2;
        return 0;
    }

    private static List<GameObject> FindContentPanels(Transform area)
    {
        List<GameObject> panels = new List<GameObject>();
        if (area == null)
            return panels;

        for (int i = 0; i < area.childCount; i++)
        {
            panels.Add(area.GetChild(i).gameObject);
        }

        return panels;
    }

    private void SetTabVisual(Button button, bool active)
    {
        SetGraphicColor(button.targetGraphic, Color.clear);
        SetChildTextColor(button.transform, active ? activeText : inactiveText);

        Transform underline = button.transform.Find("GoldUnderline");
        if (underline != null)
        {
            underline.gameObject.SetActive(active);
        }
    }

    private void SetSubButtonVisual(Button button, bool active)
    {
        SetGraphicColor(button.targetGraphic, active ? activeButton : inactiveButton);
        SetChildTextColor(button.transform, active ? redText : inactiveText);

        Outline outline = button.GetComponent<Outline>();
        if (outline != null)
        {
            outline.effectColor = active
                ? new Color(1f, 0.20f, 0.27f, 0.55f)
                : new Color(0.47f, 0.47f, 0.47f, 0.35f);
        }

        Transform underline = button.transform.Find("ActiveUnderline");
        if (underline != null)
        {
            underline.gameObject.SetActive(active);
        }
    }

    private static void SetGraphicColor(Graphic graphic, Color color)
    {
        if (graphic != null)
        {
            graphic.color = color;
        }
    }

    private static void SetChildTextColor(Transform root, Color color)
    {
        TextMeshProUGUI text = root.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            text.color = color;
        }
    }
}

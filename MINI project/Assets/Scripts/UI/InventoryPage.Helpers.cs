using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class InventoryPage
{
    private void ApplyTabVisibility()
    {
        quickItemsPanel.SetActive(activeTab == InventoryTab.QuickItems);
        equippedPanel.SetActive(activeTab == InventoryTab.Equipped);
        SetButtonColor(
            quickTabButton,
            activeTab == InventoryTab.QuickItems
                ? selectedTabColor
                : OriginalColorOf(quickTabButton)
        );
        SetButtonColor(
            equippedTabButton,
            activeTab == InventoryTab.Equipped
                ? selectedTabColor
                : OriginalColorOf(equippedTabButton)
        );
    }

    private void ApplySelectionColors()
    {
        for (int i = 0; i < consumableSlotButtons.Length; i++)
        {
            SetButtonColor(
                consumableSlotButtons[i],
                i == selectedQuickSlotIndex
                    ? selectedSlotColor
                    : OriginalColorOf(consumableSlotButtons[i])
            );
        }

        SetButtonColor(
            weaponSlotButton,
            selectedEquipmentSlot == EquipmentSlotId.Weapon
                ? selectedSlotColor
                : OriginalColorOf(weaponSlotButton)
        );
        SetButtonColor(
            armorBodySlotButton,
            selectedEquipmentSlot == EquipmentSlotId.ArmorBody
                ? selectedSlotColor
                : OriginalColorOf(armorBodySlotButton)
        );
        SetButtonColor(
            armorArmsSlotButton,
            selectedEquipmentSlot == EquipmentSlotId.ArmorArms
                ? selectedSlotColor
                : OriginalColorOf(armorArmsSlotButton)
        );
        SetButtonColor(
            armorLegsSlotButton,
            selectedEquipmentSlot == EquipmentSlotId.ArmorLegs
                ? selectedSlotColor
                : OriginalColorOf(armorLegsSlotButton)
        );
    }

    private void CacheButtonColors()
    {
        CacheColor(quickTabButton);
        CacheColor(equippedTabButton);
        CacheColor(weaponSlotButton);
        CacheColor(armorBodySlotButton);
        CacheColor(armorArmsSlotButton);
        CacheColor(armorLegsSlotButton);
        foreach (Button button in consumableSlotButtons)
        {
            CacheColor(button);
        }
    }

    private void CacheColor(Button button)
    {
        if (button?.image != null && !originalButtonColors.ContainsKey(button))
        {
            originalButtonColors.Add(button, button.image.color);
        }
    }

    private Color OriginalColorOf(Button button)
    {
        if (button != null && originalButtonColors.TryGetValue(button, out Color color))
        {
            return color;
        }

        return Color.white;
    }

    private static void SetButtonColor(Button button, Color color)
    {
        if (button?.image != null)
        {
            button.image.color = color;
        }
    }

    private void ResetScroll()
    {
        if (inventoryScrollRect != null)
        {
            inventoryScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void ClearAllSlots()
    {
        RefreshGold(0);

        foreach (TMP_Text label in consumableSlotLabels)
        {
            if (label != null)
                label.text = Loc.Get("ui_empty");
        }

        weaponSlotLabel.text = Loc.Get("ui_empty");
        armorBodySlotLabel.text = Loc.Get("ui_empty");
        armorArmsSlotLabel.text = Loc.Get("ui_empty");
        armorLegsSlotLabel.text = Loc.Get("ui_empty");
        weaponSlotStatsLabel.text = "-";
        armorBodySlotStatsLabel.text = "-";
        armorArmsSlotStatsLabel.text = "-";
        armorLegsSlotStatsLabel.text = "-";
        foreach (TMP_Text label in inventorySlotLabels)
        {
            if (label != null)
                label.text = string.Empty;
        }
    }

    private void RefreshGold(int gold)
    {
        goldLabel.text = "G: " + Mathf.Max(0, gold);
    }

    private static Item GetEquippedItem(EquipmentSlots equipment, EquipmentSlotId slot)
    {
        if (equipment == null)
        {
            return null;
        }

        switch (slot)
        {
            case EquipmentSlotId.Weapon:
                return equipment.Weapon;
            case EquipmentSlotId.ArmorBody:
                return equipment.ArmorBody;
            case EquipmentSlotId.ArmorArms:
                return equipment.ArmorArms;
            case EquipmentSlotId.ArmorLegs:
                return equipment.ArmorLegs;
            default:
                return null;
        }
    }

    private static bool IsCombatScene()
    {
        return GameSystemManager.TryGetInstance(out GameSystemManager gsm)
            && (
                gsm.CombatContext != null
                || (gsm.Scenes != null && gsm.Scenes.CurrentSceneId == SceneId.Combat)
            );
    }

    private static bool TryGetRunData(out GameRunState run, out DataManager data)
    {
        run = null;
        data = null;
        if (
            !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
            || gsm.CurrentRun?.Player == null
            || gsm.Data == null
        )
        {
            return false;
        }

        run = gsm.CurrentRun;
        data = gsm.Data;
        return true;
    }

    private bool ValidateReferences()
    {
        bool valid =
            root != null
            && quickTabButton != null
            && equippedTabButton != null
            && quickItemsPanel != null
            && equippedPanel != null
            && goldLabel != null
            && weaponSlotButton != null
            && armorBodySlotButton != null
            && armorArmsSlotButton != null
            && armorLegsSlotButton != null
            && weaponSlotLabel != null
            && armorBodySlotLabel != null
            && armorArmsSlotLabel != null
            && armorLegsSlotLabel != null
            && weaponSlotStatsLabel != null
            && armorBodySlotStatsLabel != null
            && armorArmsSlotStatsLabel != null
            && armorLegsSlotStatsLabel != null
            && inventoryScrollRect != null
            && consumableSlotButtons != null
            && consumableSlotButtons.Length == ConsumableSlots.SlotCount
            && consumableSlotLabels != null
            && consumableSlotLabels.Length == ConsumableSlots.SlotCount
            && inventorySlotButtons != null
            && inventorySlotButtons.Length > 0
            && inventorySlotLabels != null
            && inventorySlotLabels.Length == inventorySlotButtons.Length
            && infoPanel != null
            && simpleDiscardPanel != null
            && simpleDiscardCategoryLabel != null
            && simpleDiscardNameLabel != null
            && simpleDiscardStatsLabel != null
            && simpleDiscardQuantityLabel != null
            && simpleDiscardWarningLabel != null
            && simpleDiscardConfirmButton != null
            && simpleDiscardCancelButton != null
            && simpleDiscardCloseButton != null
            && quantityDiscardPanel != null
            && quantityDiscardCategoryLabel != null
            && quantityDiscardNameLabel != null
            && quantityDiscardDescriptionLabel != null
            && quantityDiscardOwnedLabel != null
            && quantityDiscardCountLabel != null
            && quantityDiscardValueLabel != null
            && quantityDiscardWarningLabel != null
            && quantityDiscardConfirmLabel != null
            && quantityDiscardMinusButton != null
            && quantityDiscardPlusButton != null
            && quantityDiscardMinButton != null
            && quantityDiscardMaxButton != null
            && quantityDiscardConfirmButton != null
            && quantityDiscardCancelButton != null
            && quantityDiscardCloseButton != null;
        if (!valid)
        {
            GameLog.LogError(
                "[InventoryPage] 필수 UI 참조가 Boot 씬에서 직접 할당되어 있지 않습니다."
            );
            return false;
        }

        for (int i = 0; i < consumableSlotButtons.Length; i++)
        {
            if (consumableSlotButtons[i] == null || consumableSlotLabels[i] == null)
            {
                GameLog.LogError("[InventoryPage] Quick Items 슬롯 참조 누락: " + i);
                return false;
            }
        }

        for (int i = 0; i < inventorySlotButtons.Length; i++)
        {
            if (inventorySlotButtons[i] == null || inventorySlotLabels[i] == null)
            {
                GameLog.LogError("[InventoryPage] 중앙 인벤토리 슬롯 참조 누락: " + i);
                return false;
            }
        }

        return true;
    }
}

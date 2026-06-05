using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tempt
{
    public enum ItemDetailContext
    {
        Inventory,
        Locker,
        Shop,
    }

    /// <summary>
    /// 선택된 아이템 정보와 현재 컨텍스트에서 가능한 액션을 표시한다.
    /// </summary>
    public sealed class ItemInfoPanel : MonoBehaviour
    {
        [SerializeField]
        private GameObject root;

        [SerializeField]
        private TMP_Text nameLabel;

        [SerializeField]
        private TMP_Text descLabel;

        [SerializeField]
        private TMP_Text categoryLabel;

        [SerializeField]
        private TMP_Text statModLabel;

        [SerializeField]
        private TMP_Text priceLabel;

        [SerializeField]
        private TMP_Text ownedCountLabel;

        [SerializeField]
        private Button primaryButton;

        [SerializeField]
        private Button secondaryButton;

        [SerializeField]
        private Button tertiaryButton;

        [SerializeField]
        private TMP_Text primaryButtonLabel;

        [SerializeField]
        private TMP_Text secondaryButtonLabel;

        [SerializeField]
        private TMP_Text tertiaryButtonLabel;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            Hide();
        }

        public void Show(int itemId, ItemDetailContext context)
        {
            if (
                !TryGetRunData(out GameRunState run, out DataManager data)
                || !data.Items.TryGetValue(itemId, out ItemData itemData)
            )
            {
                Debug.LogError("[ItemInfoPanel.Show] 아이템 ID 없음: " + itemId);
                Hide();
                return;
            }

            root.SetActive(true);
            SetBaseText(itemData);
            statModLabel.text = FormatItemEffect(itemData);
            ConfigureCommonVisibility(context);

            switch (context)
            {
                case ItemDetailContext.Inventory:
                    ConfigureInventoryStack(run, itemData);
                    break;
                case ItemDetailContext.Locker:
                    ConfigureLockerStack(run, itemData);
                    break;
                case ItemDetailContext.Shop:
                    ConfigureShopStack(run, data, itemData);
                    break;
            }
        }

        public void ShowEquip(Item item, ItemDetailContext context)
        {
            if (item?.Data == null || !TryGetRunData(out GameRunState run, out DataManager data))
            {
                Debug.LogError("[ItemInfoPanel.ShowEquip] item/Data 또는 런 상태 참조가 없습니다.");
                Hide();
                return;
            }

            root.SetActive(true);
            SetBaseText(item.Data);
            statModLabel.text = FormatEquipMod(item);
            ConfigureCommonVisibility(context);

            switch (context)
            {
                case ItemDetailContext.Inventory:
                    ConfigureInventoryEquip(run, item);
                    break;
                case ItemDetailContext.Locker:
                    ConfigureLockerEquip(run, item);
                    break;
                case ItemDetailContext.Shop:
                    ConfigureShopEquip(run, data, item);
                    break;
            }
        }

        public void ShowCustomStack(
            ItemData itemData,
            int ownedCount,
            string ownedText,
            string primaryText,
            UnityAction primaryAction,
            bool showPrimary,
            string secondaryText,
            UnityAction secondaryAction,
            bool showSecondary
        )
        {
            if (itemData == null)
            {
                Hide();
                return;
            }

            root.SetActive(true);
            SetBaseText(itemData);
            ConfigureCommonVisibility(ItemDetailContext.Inventory);
            ownedCountLabel.text = string.IsNullOrEmpty(ownedText)
                ? Loc.Format("item_owned_fmt", ownedCount)
                : ownedText;
            statModLabel.text = FormatItemEffect(itemData);
            ConfigureButton(
                primaryButton,
                primaryButtonLabel,
                primaryText,
                primaryAction,
                showPrimary
            );
            ConfigureButton(
                secondaryButton,
                secondaryButtonLabel,
                secondaryText,
                secondaryAction,
                showSecondary
            );
        }

        public void ShowCustomEquip(
            Item item,
            string ownedText,
            string primaryText,
            UnityAction primaryAction,
            bool showPrimary,
            string secondaryText,
            UnityAction secondaryAction,
            bool showSecondary
        )
        {
            if (item?.Data == null)
            {
                Hide();
                return;
            }

            root.SetActive(true);
            SetBaseText(item.Data);
            ConfigureCommonVisibility(ItemDetailContext.Inventory);
            ownedCountLabel.text = ownedText ?? string.Empty;
            statModLabel.text = FormatEquipMod(item);
            ConfigureButton(
                primaryButton,
                primaryButtonLabel,
                primaryText,
                primaryAction,
                showPrimary
            );
            ConfigureButton(
                secondaryButton,
                secondaryButtonLabel,
                secondaryText,
                secondaryAction,
                showSecondary
            );
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            ClearButton(primaryButton);
            ClearButton(secondaryButton);
            ClearButton(tertiaryButton);
        }

        public static string FormatItemEffect(ItemData itemData)
        {
            if (itemData == null)
            {
                return string.Empty;
            }

            if (itemData.Category == ItemCategory.Consumable)
            {
                return itemData.ConsumeEffectKey + " " + itemData.ParamValue;
            }

            return string.Empty;
        }

        public static string FormatEquipMod(Item item)
        {
            if (item?.Data == null)
            {
                return string.Empty;
            }

            EquipmentStatMod mod = item.GetFinalMod();
            return $"HP {mod.HP} / MP {mod.MP} / ATK {mod.ATK} / DEF {mod.DEF} / SPD {mod.SPD}";
        }

        private bool ValidateReferences()
        {
            bool valid =
                root != null
                && nameLabel != null
                && descLabel != null
                && categoryLabel != null
                && statModLabel != null
                && priceLabel != null
                && ownedCountLabel != null
                && primaryButton != null
                && secondaryButton != null
                && primaryButtonLabel != null
                && secondaryButtonLabel != null;
            if (!valid)
            {
                Debug.LogError(
                    "[ItemInfoPanel] 필수 UI 참조가 Inspector 에 직접 할당되어 있지 않습니다."
                );
            }

            return valid;
        }

        private static bool TryGetRunData(out GameRunState run, out DataManager data)
        {
            run = null;
            data = null;
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun == null
                || gsm.Data == null
            )
            {
                Debug.LogError(
                    "[ItemInfoPanel] GameSystemManager / CurrentRun / Data 참조가 없습니다."
                );
                return false;
            }

            run = gsm.CurrentRun;
            data = gsm.Data;
            return true;
        }

        private void SetBaseText(ItemData itemData)
        {
            nameLabel.text = LocalizeKey(itemData.NameKey);
            descLabel.text = string.IsNullOrEmpty(itemData.DescKey)
                ? LocalizeKey(itemData.NameKey)
                : LocalizeKey(itemData.DescKey);
            categoryLabel.text = itemData.Category + " / " + itemData.SubCategory;
        }

        private static string LocalizeKey(string key)
        {
            return string.IsNullOrEmpty(key) ? string.Empty : Loc.Get(key);
        }

        private void ConfigureCommonVisibility(ItemDetailContext context)
        {
            priceLabel.gameObject.SetActive(context == ItemDetailContext.Shop);
            ownedCountLabel.gameObject.SetActive(context != ItemDetailContext.Shop);
            ClearButton(primaryButton);
            ClearButton(secondaryButton);
            ClearButton(tertiaryButton);
        }

        private void ConfigureInventoryStack(GameRunState run, ItemData itemData)
        {
            InventoryState inv = run.Player?.Inventory;
            ConsumableSlots slots = run.Player?.Consumables;
            int count = inv != null ? inv.CountOf(itemData.Id) : 0;
            ownedCountLabel.text = Loc.Format("item_owned_fmt", count);

            if (itemData.Category == ItemCategory.Consumable)
            {
                ConfigureButton(
                    primaryButton,
                    primaryButtonLabel,
                    "USE",
                    () =>
                    {
                        int slot = FindAssignableConsumableSlot(slots, itemData.Id);
                        if (slot >= 0 && slots.TrySetSlot(slot, itemData.Id, inv))
                        {
                            Show(itemData.Id, ItemDetailContext.Inventory);
                        }
                    },
                    true
                );
            }
        }

        private void ConfigureLockerStack(GameRunState run, ItemData itemData)
        {
            InventoryState inv = run.Player?.Inventory;
            LockerState locker = run.Player?.Locker;
            ownedCountLabel.text = Loc.Format(
                "item_stored_fmt",
                locker != null ? locker.CountOf(itemData.Id) : 0
            );
            ConfigureButton(
                primaryButton,
                primaryButtonLabel,
                "TAKE",
                () =>
                {
                    if (inv.MoveFromLocker(locker, itemData.Id, 1))
                    {
                        Show(itemData.Id, ItemDetailContext.Locker);
                    }
                },
                true
            );
        }

        private void ConfigureShopStack(GameRunState run, DataManager data, ItemData itemData)
        {
            int buy = Shop.GetBuyPrice(itemData.Id, run, data);
            int sell = Shop.GetSellPrice(itemData.Id, run, data, data.Balance);
            priceLabel.text = Loc.Format("item_buy_sell_fmt", buy, sell);
            ConfigureButton(
                primaryButton,
                primaryButtonLabel,
                "BUY",
                () =>
                {
                    if (itemData.Category == ItemCategory.Equipment || !itemData.Stackable)
                    {
                        Shop.TryBuyEquip(itemData.Id, run, data);
                    }
                    else
                    {
                        Shop.TryBuy(itemData.Id, 1, run, data);
                    }

                    Show(itemData.Id, ItemDetailContext.Shop);
                },
                true
            );

            bool canSell = itemData.Stackable && run.Player?.Inventory?.CountOf(itemData.Id) > 0;
            ConfigureButton(
                secondaryButton,
                secondaryButtonLabel,
                "SELL",
                () =>
                {
                    if (Shop.TrySell(itemData.Id, 1, run, data, data.Balance))
                    {
                        Show(itemData.Id, ItemDetailContext.Shop);
                    }
                },
                canSell
            );
        }

        private void ConfigureInventoryEquip(GameRunState run, Item item)
        {
            EquipmentSlotId equippedSlot = FindEquippedSlot(run.Player?.Equipment, item);
            if (equippedSlot != EquipmentSlotId.None)
            {
                ownedCountLabel.text = Loc.Get("ui_equipped");
                ConfigureButton(
                    primaryButton,
                    primaryButtonLabel,
                    "UNEQUIP",
                    () =>
                    {
                        if (EquipFlow.Unequip(run.Player, equippedSlot))
                        {
                            ShowEquip(item, ItemDetailContext.Inventory);
                        }
                    },
                    true
                );
                return;
            }

            ownedCountLabel.text = Loc.Get("ui_inventory");
            ConfigureButton(
                primaryButton,
                primaryButtonLabel,
                "EQUIP",
                () =>
                {
                    if (EquipFlow.Equip(run.Player, item))
                    {
                        ShowEquip(item, ItemDetailContext.Inventory);
                    }
                },
                true
            );

            ConfigureButton(
                secondaryButton,
                secondaryButtonLabel,
                "DISCARD",
                () =>
                {
                    if (run.Player.Inventory.DiscardEquip(item))
                    {
                        Hide();
                    }
                },
                true
            );
        }

        private void ConfigureLockerEquip(GameRunState run, Item item)
        {
            ownedCountLabel.text = Loc.Get("ui_stored");
            ConfigureButton(
                primaryButton,
                primaryButtonLabel,
                "TAKE",
                () =>
                {
                    if (run.Player.Inventory.MoveEquipFromLocker(run.Player.Locker, item))
                    {
                        Hide();
                    }
                },
                true
            );
        }

        private void ConfigureShopEquip(GameRunState run, DataManager data, Item item)
        {
            int sell = Shop.GetSellPrice(item.Data.Id, run, data, data.Balance);
            priceLabel.text = Loc.Format("item_sell_fmt", sell);
            ConfigureButton(
                secondaryButton,
                secondaryButtonLabel,
                "SELL",
                () =>
                {
                    if (Shop.TrySellEquip(item, run, data, data.Balance))
                    {
                        Hide();
                    }
                },
                true
            );
        }

        private static EquipmentSlotId FindEquippedSlot(EquipmentSlots equipment, Item item)
        {
            if (equipment == null || item == null)
                return EquipmentSlotId.None;
            if (equipment.Weapon == item)
                return EquipmentSlotId.Weapon;
            if (equipment.ArmorBody == item)
                return EquipmentSlotId.ArmorBody;
            if (equipment.ArmorArms == item)
                return EquipmentSlotId.ArmorArms;
            if (equipment.ArmorLegs == item)
                return EquipmentSlotId.ArmorLegs;
            return EquipmentSlotId.None;
        }

        private static int FindAssignableConsumableSlot(ConsumableSlots slots, int itemId)
        {
            if (slots?.SlotItemIds == null)
                return -1;
            for (int i = 0; i < slots.SlotItemIds.Length; i++)
            {
                if (slots.SlotItemIds[i] == itemId)
                    return i;
            }

            for (int i = 0; i < slots.SlotItemIds.Length; i++)
            {
                if (slots.SlotItemIds[i] == 0)
                    return i;
            }

            return slots.SlotItemIds.Length > 0 ? 0 : -1;
        }

        private static void ConfigureButton(
            Button button,
            TMP_Text label,
            string text,
            UnityAction action,
            bool visible
        )
        {
            if (button == null)
            {
                return;
            }

            button.gameObject.SetActive(visible);
            button.interactable = visible && action != null;
            button.onClick.RemoveAllListeners();
            if (visible && action != null)
            {
                button.onClick.AddListener(action);
            }

            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }

        private static void ClearButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }
}

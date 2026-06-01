using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// Boot 전역 Overlay의 인벤토리 페이지. 직접 배치된 슬롯과 DetailCard만 사용한다.
    /// </summary>
    public sealed class InventoryPage : MonoBehaviour
    {
        private enum InventoryTab
        {
            QuickItems,
            Equipped,
        }

        private enum CenterEntryType
        {
            Stack,
            Equipment,
        }

        private sealed class CenterEntry
        {
            public CenterEntryType Type;
            public int ItemId;
            public int Count;
            public Item Item;
        }

        [Header("Root")]
        [SerializeField] private GameObject root;
        [SerializeField] private Button quickTabButton;
        [SerializeField] private Button equippedTabButton;
        [SerializeField] private GameObject quickItemsPanel;
        [SerializeField] private GameObject equippedPanel;

        [Header("Header")]
        [SerializeField] private TMP_Text goldLabel;

        [Header("Quick Items")]
        [SerializeField] private Button[] consumableSlotButtons = new Button[ConsumableSlots.SlotCount];
        [SerializeField] private TMP_Text[] consumableSlotLabels = new TMP_Text[ConsumableSlots.SlotCount];

        [Header("Equipped")]
        [SerializeField] private Button weaponSlotButton;
        [SerializeField] private Button armorBodySlotButton;
        [SerializeField] private Button armorArmsSlotButton;
        [SerializeField] private Button armorLegsSlotButton;
        [SerializeField] private TMP_Text weaponSlotLabel;
        [SerializeField] private TMP_Text armorBodySlotLabel;
        [SerializeField] private TMP_Text armorArmsSlotLabel;
        [SerializeField] private TMP_Text armorLegsSlotLabel;
        [SerializeField] private TMP_Text weaponSlotStatsLabel;
        [SerializeField] private TMP_Text armorBodySlotStatsLabel;
        [SerializeField] private TMP_Text armorArmsSlotStatsLabel;
        [SerializeField] private TMP_Text armorLegsSlotStatsLabel;

        [Header("Inventory Grid")]
        [SerializeField] private ScrollRect inventoryScrollRect;
        [SerializeField] private Button[] inventorySlotButtons;
        [SerializeField] private TMP_Text[] inventorySlotLabels;

        [Header("Detail")]
        [SerializeField] private ItemInfoPanel infoPanel;

        [Header("Simple Discard Confirm Panel")]
        [SerializeField] private GameObject simpleDiscardPanel;
        [SerializeField] private TMP_Text simpleDiscardCategoryLabel;
        [SerializeField] private TMP_Text simpleDiscardNameLabel;
        [SerializeField] private TMP_Text simpleDiscardStatsLabel;
        [SerializeField] private TMP_Text simpleDiscardQuantityLabel;
        [SerializeField] private TMP_Text simpleDiscardWarningLabel;
        [SerializeField] private Button simpleDiscardConfirmButton;
        [SerializeField] private Button simpleDiscardCancelButton;
        [SerializeField] private Button simpleDiscardCloseButton;

        [Header("Quantity Discard Panel")]
        [SerializeField] private GameObject quantityDiscardPanel;
        [SerializeField] private TMP_Text quantityDiscardCategoryLabel;
        [SerializeField] private TMP_Text quantityDiscardNameLabel;
        [SerializeField] private TMP_Text quantityDiscardDescriptionLabel;
        [SerializeField] private TMP_Text quantityDiscardOwnedLabel;
        [SerializeField] private TMP_Text quantityDiscardCountLabel;
        [SerializeField] private TMP_Text quantityDiscardValueLabel;
        [SerializeField] private TMP_Text quantityDiscardWarningLabel;
        [SerializeField] private TMP_Text quantityDiscardConfirmLabel;
        [SerializeField] private Button quantityDiscardMinusButton;
        [SerializeField] private Button quantityDiscardPlusButton;
        [SerializeField] private Button quantityDiscardMinButton;
        [SerializeField] private Button quantityDiscardMaxButton;
        [SerializeField] private Button quantityDiscardConfirmButton;
        [SerializeField] private Button quantityDiscardCancelButton;
        [SerializeField] private Button quantityDiscardCloseButton;

        [Header("Colors")]
        [SerializeField] private Color selectedSlotColor = new Color(1f, 0.86f, 0.1f, 1f);
        [SerializeField] private Color selectedTabColor = new Color(0.78f, 0.78f, 0.78f, 1f);

        private readonly List<CenterEntry> centerEntries = new List<CenterEntry>();
        private readonly Dictionary<Button, Color> originalButtonColors = new Dictionary<Button, Color>();
        private InventoryTab activeTab = InventoryTab.Equipped;
        private int selectedQuickSlotIndex = -1;
        private EquipmentSlotId selectedEquipmentSlot = EquipmentSlotId.None;
        private int quantityDiscardItemId;
        private int quantityDiscardOwnedCount;
        private int quantityDiscardAmount = 1;
        private Item simpleDiscardItem;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            CacheButtonColors();
            WireStaticButtons();
            CloseDiscardPanels();
            root.SetActive(false);
        }

        public void OnOpen()
        {
            if (!enabled)
            {
                return;
            }

            root.SetActive(true);
            SubscribeEvents();
            Refresh();
        }

        public void OnClose()
        {
            UnsubscribeEvents();
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        public void Refresh()
        {
            if (!enabled)
            {
                return;
            }

            ApplyTabVisibility();
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                ClearAllSlots();
                infoPanel.Hide();
                return;
            }

            PlayerState player = run.Player;
            RefreshGold(run.Gold);
            SetEquipmentSlot(EquipmentSlotId.Weapon, weaponSlotButton, weaponSlotLabel, weaponSlotStatsLabel, player.Equipment?.Weapon);
            SetEquipmentSlot(EquipmentSlotId.ArmorBody, armorBodySlotButton, armorBodySlotLabel, armorBodySlotStatsLabel, player.Equipment?.ArmorBody);
            SetEquipmentSlot(EquipmentSlotId.ArmorArms, armorArmsSlotButton, armorArmsSlotLabel, armorArmsSlotStatsLabel, player.Equipment?.ArmorArms);
            SetEquipmentSlot(EquipmentSlotId.ArmorLegs, armorLegsSlotButton, armorLegsSlotLabel, armorLegsSlotStatsLabel, player.Equipment?.ArmorLegs);
            RefreshQuickSlots(player, data);
            RefreshInventoryGrid(player, data);
            ApplySelectionColors();
        }

        private void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnInventoryChanged -= Refresh;
                gsm.Events.OnEquipmentChanged -= Refresh;
                gsm.Events.OnGoldChanged -= RefreshGold;
                gsm.Events.OnInventoryChanged += Refresh;
                gsm.Events.OnEquipmentChanged += Refresh;
                gsm.Events.OnGoldChanged += RefreshGold;
            }
        }

        private void UnsubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnInventoryChanged -= Refresh;
                gsm.Events.OnEquipmentChanged -= Refresh;
                gsm.Events.OnGoldChanged -= RefreshGold;
            }
        }

        private void WireStaticButtons()
        {
            quickTabButton.onClick.RemoveAllListeners();
            quickTabButton.onClick.AddListener(SelectQuickTab);
            equippedTabButton.onClick.RemoveAllListeners();
            equippedTabButton.onClick.AddListener(SelectEquippedTab);

            WireEquipmentSlot(weaponSlotButton, EquipmentSlotId.Weapon);
            WireEquipmentSlot(armorBodySlotButton, EquipmentSlotId.ArmorBody);
            WireEquipmentSlot(armorArmsSlotButton, EquipmentSlotId.ArmorArms);
            WireEquipmentSlot(armorLegsSlotButton, EquipmentSlotId.ArmorLegs);
            WireDiscardPanelButtons();
        }

        private void WireDiscardPanelButtons()
        {
            simpleDiscardCancelButton.onClick.RemoveAllListeners();
            simpleDiscardCancelButton.onClick.AddListener(CloseDiscardPanels);
            simpleDiscardCloseButton.onClick.RemoveAllListeners();
            simpleDiscardCloseButton.onClick.AddListener(CloseDiscardPanels);
            simpleDiscardConfirmButton.onClick.RemoveAllListeners();
            simpleDiscardConfirmButton.onClick.AddListener(ConfirmSimpleDiscard);

            quantityDiscardMinusButton.onClick.RemoveAllListeners();
            quantityDiscardMinusButton.onClick.AddListener(() => SetQuantityDiscardAmount(quantityDiscardAmount - 1));
            quantityDiscardPlusButton.onClick.RemoveAllListeners();
            quantityDiscardPlusButton.onClick.AddListener(() => SetQuantityDiscardAmount(quantityDiscardAmount + 1));
            quantityDiscardMinButton.onClick.RemoveAllListeners();
            quantityDiscardMinButton.onClick.AddListener(() => SetQuantityDiscardAmount(1));
            quantityDiscardMaxButton.onClick.RemoveAllListeners();
            quantityDiscardMaxButton.onClick.AddListener(() => SetQuantityDiscardAmount(quantityDiscardOwnedCount));
            quantityDiscardCancelButton.onClick.RemoveAllListeners();
            quantityDiscardCancelButton.onClick.AddListener(CloseDiscardPanels);
            quantityDiscardCloseButton.onClick.RemoveAllListeners();
            quantityDiscardCloseButton.onClick.AddListener(CloseDiscardPanels);
            quantityDiscardConfirmButton.onClick.RemoveAllListeners();
            quantityDiscardConfirmButton.onClick.AddListener(ConfirmQuantityDiscard);
        }

        private void SelectQuickTab()
        {
            activeTab = InventoryTab.QuickItems;
            selectedEquipmentSlot = EquipmentSlotId.None;
            selectedQuickSlotIndex = -1;
            infoPanel.Hide();
            Refresh();
            ResetScroll();
        }

        private void SelectEquippedTab()
        {
            activeTab = InventoryTab.Equipped;
            selectedEquipmentSlot = EquipmentSlotId.None;
            selectedQuickSlotIndex = -1;
            infoPanel.Hide();
            Refresh();
            ResetScroll();
        }

        private void WireEquipmentSlot(Button button, EquipmentSlotId slot)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => HandleEquipmentSlotClicked(slot));
        }

        private void RefreshQuickSlots(PlayerState player, DataManager data)
        {
            for (int i = 0; i < consumableSlotButtons.Length; i++)
            {
                int index = i;
                Button button = consumableSlotButtons[i];
                TMP_Text label = consumableSlotLabels[i];
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => HandleQuickSlotClicked(index));

                int itemId = player.Consumables != null && player.Consumables.SlotItemIds != null && index < player.Consumables.SlotItemIds.Length
                    ? player.Consumables.SlotItemIds[index]
                    : 0;
                if (itemId == 0 || data.Items == null || !data.Items.TryGetValue(itemId, out ItemData itemData))
                {
                    label.text = "EMPTY";
                    continue;
                }

                int count = player.Inventory != null ? player.Inventory.CountOf(itemId) : 0;
                label.text = itemData.NameKey + "\nx" + count;
            }
        }

        private void SetEquipmentSlot(EquipmentSlotId slot, Button button, TMP_Text nameLabel, TMP_Text statsLabel, Item item)
        {
            button.interactable = true;
            if (item?.Data == null)
            {
                nameLabel.text = "EMPTY";
                statsLabel.text = "-";
                return;
            }

            nameLabel.text = item.Data.NameKey + " +" + item.Enhancement;
            statsLabel.text = ItemInfoPanel.FormatEquipMod(item);
        }

        private void RefreshInventoryGrid(PlayerState player, DataManager data)
        {
            centerEntries.Clear();
            AddStackEntries(player, data);
            AddEquipmentEntries(player);

            for (int i = 0; i < inventorySlotButtons.Length; i++)
            {
                int index = i;
                Button button = inventorySlotButtons[i];
                TMP_Text label = inventorySlotLabels[i];
                button.onClick.RemoveAllListeners();

                if (index >= centerEntries.Count)
                {
                    label.text = string.Empty;
                    button.interactable = false;
                    continue;
                }

                CenterEntry entry = centerEntries[index];
                label.text = FormatCenterEntry(entry, data);
                button.interactable = true;
                button.onClick.AddListener(() => HandleInventorySlotClicked(index));
            }
        }

        private void AddStackEntries(PlayerState player, DataManager data)
        {
            if (player.Inventory == null || data.Items == null)
            {
                return;
            }

            var stacks = new List<KeyValuePair<int, int>>(player.Inventory.StackableItems);
            stacks.Sort((a, b) => a.Key.CompareTo(b.Key));
            foreach (KeyValuePair<int, int> stack in stacks)
            {
                if (stack.Value <= 0 || !data.Items.TryGetValue(stack.Key, out ItemData itemData))
                {
                    continue;
                }

                centerEntries.Add(new CenterEntry
                {
                    Type = CenterEntryType.Stack,
                    ItemId = stack.Key,
                    Count = stack.Value,
                });
            }
        }

        private void AddEquipmentEntries(PlayerState player)
        {
            if (player.Inventory?.EquipItems == null)
            {
                return;
            }

            foreach (Item item in player.Inventory.EquipItems)
            {
                if (item?.Data == null)
                {
                    continue;
                }

                centerEntries.Add(new CenterEntry
                {
                    Type = CenterEntryType.Equipment,
                    Item = item,
                });
            }
        }

        private string FormatCenterEntry(CenterEntry entry, DataManager data)
        {
            if (entry.Type == CenterEntryType.Stack)
            {
                if (data.Items.TryGetValue(entry.ItemId, out ItemData itemData))
                {
                    return itemData.NameKey + "\nx" + entry.Count;
                }

                return "MISSING\n#" + entry.ItemId;
            }

            return entry.Item.Data.NameKey + "\n+" + entry.Item.Enhancement;
        }

        private void HandleQuickSlotClicked(int index)
        {
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            selectedQuickSlotIndex = index;
            selectedEquipmentSlot = EquipmentSlotId.None;
            ApplySelectionColors();

            int itemId = run.Player.Consumables?.SlotItemIds != null && index < run.Player.Consumables.SlotItemIds.Length
                ? run.Player.Consumables.SlotItemIds[index]
                : 0;
            if (itemId == 0 || !data.Items.TryGetValue(itemId, out ItemData itemData))
            {
                infoPanel.Hide();
                return;
            }

            int count = run.Player.Inventory != null ? run.Player.Inventory.CountOf(itemId) : 0;
            bool canEdit = !IsCombatScene();
            infoPanel.ShowCustomStack(
                itemData,
                count,
                "Quick Slot " + (index + 1) + " / Owned " + count,
                null,
                null,
                false,
                "DISCARD",
                canEdit ? () =>
                {
                    if (run.Player.Consumables.TrySetSlot(index, 0, run.Player.Inventory))
                    {
                        infoPanel.Hide();
                        Refresh();
                    }
                } : null,
                true);
        }

        private void HandleEquipmentSlotClicked(EquipmentSlotId slot)
        {
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            selectedEquipmentSlot = slot;
            selectedQuickSlotIndex = -1;
            ApplySelectionColors();

            Item item = GetEquippedItem(run.Player.Equipment, slot);
            if (item?.Data == null)
            {
                infoPanel.Hide();
                return;
            }

            bool canEdit = !IsCombatScene();
            infoPanel.ShowCustomEquip(
                item,
                "Equipped",
                "UNEQUIP",
                canEdit ? () =>
                {
                    if (EquipFlow.Unequip(run.Player, slot))
                    {
                        infoPanel.Hide();
                        Refresh();
                    }
                } : null,
                true,
                null,
                null,
                false);
        }

        private void HandleInventorySlotClicked(int index)
        {
            if (index < 0 || index >= centerEntries.Count || !TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            CenterEntry entry = centerEntries[index];
            if (entry.Type == CenterEntryType.Stack)
            {
                ShowStackCenterEntry(entry, run, data);
            }
            else
            {
                ShowEquipmentCenterEntry(entry, run);
            }
        }

        private void ShowStackCenterEntry(CenterEntry entry, GameRunState run, DataManager data)
        {
            if (!data.Items.TryGetValue(entry.ItemId, out ItemData itemData))
            {
                infoPanel.Hide();
                return;
            }

            bool canUse = itemData.Category == ItemCategory.Consumable
                && activeTab == InventoryTab.QuickItems
                && selectedQuickSlotIndex >= 0
                && !IsCombatScene();
            infoPanel.ShowCustomStack(
                itemData,
                entry.Count,
                "Inventory x" + entry.Count,
                "USE",
                canUse ? () =>
                {
                    if (run.Player.Consumables.TrySetSlot(selectedQuickSlotIndex, entry.ItemId, run.Player.Inventory))
                    {
                        Refresh();
                        HandleQuickSlotClicked(selectedQuickSlotIndex);
                    }
                } : null,
                true,
                "DISCARD",
                !IsCombatScene() ? () => OpenQuantityDiscardPanel(itemData, entry.Count) : null,
                true);
        }

        private void ShowEquipmentCenterEntry(CenterEntry entry, GameRunState run)
        {
            Item item = entry.Item;
            bool slotMatches = activeTab == InventoryTab.Equipped
                && selectedEquipmentSlot != EquipmentSlotId.None
                && item?.Data != null
                && item.Data.EquipSlot == selectedEquipmentSlot;
            bool canEquip = slotMatches && !IsCombatScene();
            infoPanel.ShowCustomEquip(
                item,
                "Inventory",
                "EQUIP",
                canEquip ? () =>
                {
                    if (EquipFlow.Equip(run.Player, item))
                    {
                        infoPanel.Hide();
                        Refresh();
                    }
                } : null,
                true,
                "DISCARD",
                !IsCombatScene() ? () => OpenSimpleDiscardPanel(item) : null,
                true);
        }

        private void OpenSimpleDiscardPanel(Item item)
        {
            if (item?.Data == null)
            {
                return;
            }

            simpleDiscardItem = item;
            CloseDiscardPanels();
            simpleDiscardCategoryLabel.text = item.Data.Category + " / " + item.Data.EquipSlot;
            simpleDiscardNameLabel.text = item.Data.NameKey + " +" + item.Enhancement;
            simpleDiscardStatsLabel.text = ItemInfoPanel.FormatEquipMod(item);
            simpleDiscardQuantityLabel.text = "x1";
            simpleDiscardWarningLabel.text = "Discard this equipment?\n<color=#F87171>Discarded items cannot be recovered.</color>";
            simpleDiscardPanel.SetActive(true);
            simpleDiscardPanel.transform.SetAsLastSibling();
        }

        private void OpenQuantityDiscardPanel(ItemData itemData, int ownedCount)
        {
            if (itemData == null || ownedCount <= 0)
            {
                return;
            }

            quantityDiscardItemId = itemData.Id;
            quantityDiscardOwnedCount = ownedCount;
            quantityDiscardAmount = 1;
            CloseDiscardPanels();
            quantityDiscardCategoryLabel.text = itemData.SubCategory;
            quantityDiscardNameLabel.text = itemData.NameKey;
            quantityDiscardDescriptionLabel.text = string.IsNullOrEmpty(itemData.DescKey) ? ItemInfoPanel.FormatItemEffect(itemData) : itemData.DescKey;
            quantityDiscardOwnedLabel.text = "x" + ownedCount;
            quantityDiscardPanel.SetActive(true);
            quantityDiscardPanel.transform.SetAsLastSibling();
            RefreshQuantityDiscardPanel();
        }

        private void SetQuantityDiscardAmount(int amount)
        {
            quantityDiscardAmount = Mathf.Clamp(amount, 1, Mathf.Max(1, quantityDiscardOwnedCount));
            RefreshQuantityDiscardPanel();
        }

        private void RefreshQuantityDiscardPanel()
        {
            quantityDiscardAmount = Mathf.Clamp(quantityDiscardAmount, 1, Mathf.Max(1, quantityDiscardOwnedCount));
            quantityDiscardCountLabel.text = quantityDiscardAmount + " / " + quantityDiscardOwnedCount;
            quantityDiscardValueLabel.text = quantityDiscardAmount.ToString();
            quantityDiscardWarningLabel.text = "Discard <color=#EAB308>x" + quantityDiscardAmount + "</color> items?\nDiscarded items cannot be recovered.";
            quantityDiscardConfirmLabel.text = "DISCARD x" + quantityDiscardAmount;
            quantityDiscardMinusButton.interactable = quantityDiscardAmount > 1;
            quantityDiscardMinButton.interactable = quantityDiscardAmount > 1;
            quantityDiscardPlusButton.interactable = quantityDiscardAmount < quantityDiscardOwnedCount;
            quantityDiscardMaxButton.interactable = quantityDiscardAmount < quantityDiscardOwnedCount;
        }

        private void ConfirmSimpleDiscard()
        {
            if (simpleDiscardItem == null || !TryGetRunData(out GameRunState run, out _))
            {
                CloseDiscardPanels();
                return;
            }

            if (run.Player.Inventory.DiscardEquip(simpleDiscardItem))
            {
                simpleDiscardItem = null;
                infoPanel.Hide();
                CloseDiscardPanels();
                Refresh();
            }
        }

        private void ConfirmQuantityDiscard()
        {
            if (quantityDiscardItemId <= 0 || quantityDiscardAmount <= 0 || !TryGetRunData(out GameRunState run, out _))
            {
                CloseDiscardPanels();
                return;
            }

            if (run.Player.Inventory.Discard(quantityDiscardItemId, quantityDiscardAmount))
            {
                ClearQuickSlotsIfStackEmpty(run.Player, quantityDiscardItemId);
                infoPanel.Hide();
                CloseDiscardPanels();
                Refresh();
            }
        }

        private void ClearQuickSlotsIfStackEmpty(PlayerState player, int itemId)
        {
            if (player?.Inventory == null || player.Consumables?.SlotItemIds == null || player.Inventory.CountOf(itemId) > 0)
            {
                return;
            }

            for (int i = 0; i < player.Consumables.SlotItemIds.Length; i++)
            {
                if (player.Consumables.SlotItemIds[i] == itemId)
                {
                    player.Consumables.TrySetSlot(i, 0, player.Inventory);
                }
            }
        }

        private void CloseDiscardPanels()
        {
            if (simpleDiscardPanel != null)
            {
                simpleDiscardPanel.SetActive(false);
            }

            if (quantityDiscardPanel != null)
            {
                quantityDiscardPanel.SetActive(false);
            }
        }

        private void ApplyTabVisibility()
        {
            quickItemsPanel.SetActive(activeTab == InventoryTab.QuickItems);
            equippedPanel.SetActive(activeTab == InventoryTab.Equipped);
            SetButtonColor(quickTabButton, activeTab == InventoryTab.QuickItems ? selectedTabColor : OriginalColorOf(quickTabButton));
            SetButtonColor(equippedTabButton, activeTab == InventoryTab.Equipped ? selectedTabColor : OriginalColorOf(equippedTabButton));
        }

        private void ApplySelectionColors()
        {
            for (int i = 0; i < consumableSlotButtons.Length; i++)
            {
                SetButtonColor(consumableSlotButtons[i], i == selectedQuickSlotIndex ? selectedSlotColor : OriginalColorOf(consumableSlotButtons[i]));
            }

            SetButtonColor(weaponSlotButton, selectedEquipmentSlot == EquipmentSlotId.Weapon ? selectedSlotColor : OriginalColorOf(weaponSlotButton));
            SetButtonColor(armorBodySlotButton, selectedEquipmentSlot == EquipmentSlotId.ArmorBody ? selectedSlotColor : OriginalColorOf(armorBodySlotButton));
            SetButtonColor(armorArmsSlotButton, selectedEquipmentSlot == EquipmentSlotId.ArmorArms ? selectedSlotColor : OriginalColorOf(armorArmsSlotButton));
            SetButtonColor(armorLegsSlotButton, selectedEquipmentSlot == EquipmentSlotId.ArmorLegs ? selectedSlotColor : OriginalColorOf(armorLegsSlotButton));
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
                if (label != null) label.text = "EMPTY";
            }

            weaponSlotLabel.text = "EMPTY";
            armorBodySlotLabel.text = "EMPTY";
            armorArmsSlotLabel.text = "EMPTY";
            armorLegsSlotLabel.text = "EMPTY";
            weaponSlotStatsLabel.text = "-";
            armorBodySlotStatsLabel.text = "-";
            armorArmsSlotStatsLabel.text = "-";
            armorLegsSlotStatsLabel.text = "-";
            foreach (TMP_Text label in inventorySlotLabels)
            {
                if (label != null) label.text = string.Empty;
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
                case EquipmentSlotId.Weapon: return equipment.Weapon;
                case EquipmentSlotId.ArmorBody: return equipment.ArmorBody;
                case EquipmentSlotId.ArmorArms: return equipment.ArmorArms;
                case EquipmentSlotId.ArmorLegs: return equipment.ArmorLegs;
                default: return null;
            }
        }

        private static bool IsCombatScene()
        {
            return GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                && (gsm.CombatContext != null || (gsm.Scenes != null && gsm.Scenes.CurrentSceneId == SceneId.Combat));
        }

        private static bool TryGetRunData(out GameRunState run, out DataManager data)
        {
            run = null;
            data = null;
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.CurrentRun?.Player == null || gsm.Data == null)
            {
                return false;
            }

            run = gsm.CurrentRun;
            data = gsm.Data;
            return true;
        }

        private bool ValidateReferences()
        {
            bool valid = root != null
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
                Debug.LogError("[InventoryPage] 필수 UI 참조가 Boot 씬에서 직접 할당되어 있지 않습니다.");
                return false;
            }

            for (int i = 0; i < consumableSlotButtons.Length; i++)
            {
                if (consumableSlotButtons[i] == null || consumableSlotLabels[i] == null)
                {
                    Debug.LogError("[InventoryPage] Quick Items 슬롯 참조 누락: " + i);
                    return false;
                }
            }

            for (int i = 0; i < inventorySlotButtons.Length; i++)
            {
                if (inventorySlotButtons[i] == null || inventorySlotLabels[i] == null)
                {
                    Debug.LogError("[InventoryPage] 중앙 인벤토리 슬롯 참조 누락: " + i);
                    return false;
                }
            }

            return true;
        }
    }
}

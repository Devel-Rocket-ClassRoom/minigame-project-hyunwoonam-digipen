using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// Safe1 Content_STORAGE 표시/입력 계층. 모든 UI 참조와 Button OnClick은 Inspector에서 연결한다.
    /// </summary>
    public sealed class TavernStorageUI : MonoBehaviour
    {
        private enum SelectionSource
        {
            None,
            Inventory,
            Storage,
        }

        private sealed class Entry
        {
            public int ItemId;
            public int Count;
            public Item Item;
            public bool IsEquipment => Item != null;
        }

        [Header("Status")]
        [SerializeField] private TMP_Text stateValue;
        [SerializeField] private TMP_Text storageValue;
        [SerializeField] private TMP_Text upgradeLabel;
        [SerializeField] private TMP_Text upgradeValue;
        [SerializeField] private TMP_Text inventoryCount;
        [SerializeField] private TMP_Text storageCount;

        [Header("Item Detail")]
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailMeta;
        [SerializeField] private TMP_Text detailDescription;
        [SerializeField] private TMP_Text amountText;

        [Header("Actions")]
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button sendToStorageButton;
        [SerializeField] private Button takeToInventoryButton;
        [SerializeField] private Button discardButton;
        [SerializeField] private Button upgradeStorageButton;
        [SerializeField] private TMP_Text upgradeStorageButtonLabel;

        [Header("Slots")]
        [SerializeField] private TavernStorageSlotView[] inventorySlots;
        [SerializeField] private TavernStorageSlotView[] storageSlots;

        [Header("Colors")]
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color maxTextColor = new Color(1f, 0.72f, 0.12f, 1f);

        private readonly List<Entry> inventoryEntries = new List<Entry>();
        private readonly List<Entry> storageEntries = new List<Entry>();
        private SelectionSource selectionSource;
        private Entry selectedEntry;
        private int amount;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            SubscribeEvents();
            Refresh();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void Refresh()
        {
            if (!enabled || !TryGetData(out GameRunState run, out DataManager data))
            {
                return;
            }

            run.Player.Locker.NormalizeCapacity();
            BuildEntries(run, data);
            ValidateSelection();
            RefreshStatus(run);
            RefreshSlots();
            RefreshDetail(run, data);
            RefreshActions(run);
        }

        public void SelectInventorySlot(int index)
        {
            SelectEntry(SelectionSource.Inventory, inventoryEntries, index);
        }

        public void SelectStorageSlot(int index)
        {
            SelectEntry(SelectionSource.Storage, storageEntries, index);
        }

        public void IncreaseAmount()
        {
            if (selectedEntry == null)
            {
                return;
            }

            amount = Mathf.Min(MaxAmount(), amount + 1);
            Refresh();
        }

        public void DecreaseAmount()
        {
            if (selectedEntry == null)
            {
                return;
            }

            amount = Mathf.Max(1, amount - 1);
            Refresh();
        }

        public void SendToStorage()
        {
            if (!TryGetData(out GameRunState run, out _) || selectionSource != SelectionSource.Inventory)
            {
                return;
            }

            bool moved = selectedEntry.IsEquipment
                ? TavernStorage.TryMoveEquipToStorage(run, selectedEntry.Item)
                : TavernStorage.TryMoveStackToStorage(run, selectedEntry.ItemId, amount);
            AfterMutation(moved);
        }

        public void TakeToInventory()
        {
            if (!TryGetData(out GameRunState run, out _) || selectionSource != SelectionSource.Storage)
            {
                return;
            }

            bool moved = selectedEntry.IsEquipment
                ? TavernStorage.TryMoveEquipToInventory(run, selectedEntry.Item)
                : TavernStorage.TryMoveStackToInventory(run, selectedEntry.ItemId, amount);
            AfterMutation(moved);
        }

        public void DiscardSelected()
        {
            if (!TryGetData(out GameRunState run, out _) || selectedEntry == null)
            {
                return;
            }

            bool fromStorage = selectionSource == SelectionSource.Storage;
            bool discarded = selectedEntry.IsEquipment
                ? TavernStorage.TryDiscardEquip(run, fromStorage, selectedEntry.Item)
                : TavernStorage.TryDiscardStack(run, fromStorage, selectedEntry.ItemId, amount);
            AfterMutation(discarded);
        }

        public void ActivateOrUpgradeStorage()
        {
            if (TryGetData(out GameRunState run, out _) && TavernStorage.TryActivateOrUpgrade(run))
            {
                Refresh();
            }
        }

        private void SelectEntry(SelectionSource source, List<Entry> entries, int index)
        {
            if (index < 0 || index >= entries.Count)
            {
                return;
            }

            selectionSource = source;
            selectedEntry = entries[index];
            amount = 1;
            Refresh();
        }

        private void AfterMutation(bool changed)
        {
            if (changed)
            {
                selectionSource = SelectionSource.None;
                selectedEntry = null;
                amount = 0;
            }

            Refresh();
        }

        private void BuildEntries(GameRunState run, DataManager data)
        {
            inventoryEntries.Clear();
            storageEntries.Clear();
            AddEntries(inventoryEntries, run.Player.Inventory, data);
            AddEntries(storageEntries, run.Player.Locker, data);
        }

        private static void AddEntries(List<Entry> entries, StackableContainer container, DataManager data)
        {
            foreach (KeyValuePair<int, int> stack in container.StackableItems)
            {
                if (stack.Value > 0 && data.Items.ContainsKey(stack.Key))
                {
                    entries.Add(new Entry { ItemId = stack.Key, Count = stack.Value });
                }
            }

            entries.Sort((a, b) => a.ItemId.CompareTo(b.ItemId));
            for (int i = 0; i < container.EquipItems.Count; i++)
            {
                Item item = container.EquipItems[i];
                if (item?.Data != null)
                {
                    entries.Add(new Entry { ItemId = item.Data.Id, Count = 1, Item = item });
                }
            }
        }

        private void ValidateSelection()
        {
            if (selectedEntry == null)
            {
                amount = 0;
                return;
            }

            List<Entry> entries =
                selectionSource == SelectionSource.Inventory ? inventoryEntries : storageEntries;
            Entry match = entries.Find(entry =>
                selectedEntry.IsEquipment
                    ? entry.Item == selectedEntry.Item
                    : entry.ItemId == selectedEntry.ItemId
            );
            selectedEntry = match;
            if (selectedEntry == null)
            {
                selectionSource = SelectionSource.None;
                amount = 0;
            }
            else
            {
                amount = Mathf.Clamp(amount, 1, MaxAmount());
            }
        }

        private void RefreshStatus(GameRunState run)
        {
            LockerState locker = run.Player.Locker;
            int nextCost = TavernStorage.GetNextUpgradeCost(locker);
            stateValue.text = locker.Unlocked ? "OPEN" : "CLOSE";
            storageValue.text = locker.UsedSlots + " / " + locker.Capacity;
            storageCount.text = storageValue.text;
            inventoryCount.text =
                (run.Player.Inventory.UsedStackableSlots + run.Player.Inventory.EquipItems.Count)
                + " / "
                + (InventoryState.MaxStackableSlots + InventoryState.MaxEquipSlots);

            bool max = locker.IsMaxCapacity;
            upgradeLabel.text = max ? "MAX" : locker.Unlocked ? "UPGRADE" : "UNLOCK";
            upgradeLabel.color = max ? maxTextColor : normalTextColor;
            upgradeValue.text = max ? "MAX" : nextCost + " G";
            upgradeValue.color = max ? maxTextColor : normalTextColor;
            upgradeStorageButtonLabel.text =
                max ? "MAX"
                : locker.Unlocked ? "UPGRADE STORAGE - " + nextCost + " G"
                : "ACTIVATE STORAGE - " + nextCost + " G";
            upgradeStorageButtonLabel.color = max ? maxTextColor : normalTextColor;
            upgradeStorageButton.interactable = !max && run.Gold >= nextCost;
        }

        private void RefreshSlots()
        {
            RefreshSlotList(inventorySlots, inventoryEntries, inventorySlots.Length, SelectionSource.Inventory);
            int visibleStorageSlots = TryGetData(out GameRunState run, out _)
                ? run.Player.Locker.Capacity
                : 0;
            RefreshSlotList(storageSlots, storageEntries, visibleStorageSlots, SelectionSource.Storage);
        }

        private void RefreshSlotList(
            TavernStorageSlotView[] slots,
            List<Entry> entries,
            int visibleSlots,
            SelectionSource source
        )
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i >= visibleSlots)
                {
                    slots[i].SetEmpty(false);
                }
                else if (i >= entries.Count)
                {
                    slots[i].SetEmpty(true);
                }
                else
                {
                    Entry entry = entries[i];
                    bool selected =
                        selectionSource == source
                        && selectedEntry != null
                        && (entry.IsEquipment
                            ? entry.Item == selectedEntry.Item
                            : !selectedEntry.IsEquipment && entry.ItemId == selectedEntry.ItemId);
                    slots[i].SetItem(ResolveItemData(entry), entry.Item, entry.Count, selected);
                }
            }
        }

        private static ItemData ResolveItemData(Entry entry)
        {
            if (entry?.Item?.Data != null)
            {
                return entry.Item.Data;
            }

            if (
                entry != null
                && GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                && gsm.Data?.Items != null
                && gsm.Data.Items.TryGetValue(entry.ItemId, out ItemData data)
            )
            {
                return data;
            }

            return null;
        }

        private void RefreshDetail(GameRunState run, DataManager data)
        {
            amountText.text = "AMOUNT " + amount;
            if (selectedEntry == null || !data.Items.TryGetValue(selectedEntry.ItemId, out ItemData itemData))
            {
                detailName.text = "NO ITEM SELECTED";
                detailMeta.text = string.Empty;
                detailDescription.text = "Select an item from Player Inventory or Tavern Storage.";
                return;
            }

            detailName.text = itemData.NameKey;
            detailMeta.text =
                (selectionSource == SelectionSource.Inventory ? "INVENTORY" : "STORAGE")
                + " / "
                + itemData.Category.ToString().ToUpperInvariant()
                + " / "
                + (selectedEntry.IsEquipment ? "+" + selectedEntry.Item.Enhancement : "x" + selectedEntry.Count);
            detailDescription.text = BuildDescription(itemData, selectedEntry.Item);
        }

        private static string BuildDescription(ItemData data, Item item)
        {
            if (!string.IsNullOrEmpty(data.DescKey))
            {
                return data.DescKey;
            }

            if (item != null)
            {
                return ItemInfoPanel.FormatEquipMod(item);
            }

            string effect = ItemInfoPanel.FormatItemEffect(data);
            return string.IsNullOrEmpty(effect) ? data.SubCategory : effect;
        }

        private void RefreshActions(GameRunState run)
        {
            bool selected = selectedEntry != null;
            bool inventorySelected = selected && selectionSource == SelectionSource.Inventory;
            bool storageSelected = selected && selectionSource == SelectionSource.Storage;
            minusButton.interactable = selected && amount > 1;
            plusButton.interactable = selected && amount < MaxAmount();
            sendToStorageButton.interactable =
                inventorySelected
                && run.Player.Locker.Unlocked
                && (selectedEntry.IsEquipment
                    ? run.Player.Locker.CanAddEquip()
                    : run.Player.Locker.CanAddStack(selectedEntry.ItemId));
            takeToInventoryButton.interactable =
                storageSelected
                && (selectedEntry.IsEquipment
                    ? !run.Player.Inventory.IsEquipFull()
                    : run.Player.Inventory.CanAcceptStack(selectedEntry.ItemId, amount));
            discardButton.interactable = selected;
        }

        private int MaxAmount()
        {
            return selectedEntry == null || selectedEntry.IsEquipment ? 1 : Mathf.Max(1, selectedEntry.Count);
        }

        private bool ValidateReferences()
        {
            bool valid =
                stateValue != null
                && storageValue != null
                && upgradeLabel != null
                && upgradeValue != null
                && inventoryCount != null
                && storageCount != null
                && detailName != null
                && detailMeta != null
                && detailDescription != null
                && amountText != null
                && minusButton != null
                && plusButton != null
                && sendToStorageButton != null
                && takeToInventoryButton != null
                && discardButton != null
                && upgradeStorageButton != null
                && upgradeStorageButtonLabel != null
                && ValidateSlots(inventorySlots)
                && ValidateSlots(storageSlots);
            if (!valid)
            {
                Debug.LogError("[TavernStorageUI] 필수 UI 참조가 Inspector에 직접 할당되어 있지 않습니다.");
            }

            return valid;
        }

        private static bool ValidateSlots(TavernStorageSlotView[] slots)
        {
            if (slots == null || slots.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null || !slots[i].IsConfigured)
                {
                    return false;
                }
            }

            return true;
        }

        private void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnInventoryChanged -= Refresh;
                gsm.Events.OnGoldChanged -= HandleGoldChanged;
                gsm.Events.OnInventoryChanged += Refresh;
                gsm.Events.OnGoldChanged += HandleGoldChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnInventoryChanged -= Refresh;
                gsm.Events.OnGoldChanged -= HandleGoldChanged;
            }
        }

        private void HandleGoldChanged(int gold)
        {
            Refresh();
        }

        private static bool TryGetData(out GameRunState run, out DataManager data)
        {
            run = null;
            data = null;
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun?.Player?.Inventory == null
                || gsm.CurrentRun.Player.Locker == null
                || gsm.Data?.Items == null
            )
            {
                return false;
            }

            run = gsm.CurrentRun;
            data = gsm.Data;
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// Safe1 mockup interaction layer. Keeps the static design inspectable while making
    /// facility tabs and sub-function buttons switch the matching RectTransform groups.
    /// </summary>
    public sealed class SafeZone1FacilityMockupUI : MonoBehaviour
    {
        private enum ShopMode
        {
            Buy,
            Sell,
        }

        private sealed class ShopRowView
        {
            public GameObject Root;
            public Button Button;
            public Graphic Background;
            public Color BaseColor;
            public TextMeshProUGUI Name;
            public TextMeshProUGUI Role;
            public TextMeshProUGUI Description;
            public TextMeshProUGUI Cost;
        }

        private sealed class ShopSellEntry
        {
            public int ItemId;
            public int Count;
            public Item EquipmentItem;

            public bool IsEquipment => EquipmentItem != null;
        }

        private sealed class SkillRowView
        {
            public GameObject Root;
            public Button Button;
            public Graphic Background;
            public Color BaseColor;
            public TextMeshProUGUI Label;
            public Color LabelBaseColor;
        }

        private static readonly string[] Facilities =
        {
            "TAVERN",
            "SHOP",
            "GUILD",
            "FORGE",
            "SHRINE",
        };
        private const int Safe1ShopSellPrice = 1;

        private readonly Dictionary<string, GameObject> facilityGroups =
            new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Button> facilityTabs = new Dictionary<string, Button>();
        private readonly Dictionary<string, List<Button>> subButtons =
            new Dictionary<string, List<Button>>();
        private readonly Dictionary<string, List<GameObject>> subContents =
            new Dictionary<string, List<GameObject>>();
        private readonly Dictionary<string, List<Button>> closeButtons =
            new Dictionary<string, List<Button>>();
        private readonly List<ShopRowView> shopBuyRows = new List<ShopRowView>();
        private readonly List<ShopRowView> shopSellRows = new List<ShopRowView>();
        private readonly List<TextMeshProUGUI> shopBuyDetailLines = new List<TextMeshProUGUI>();
        private readonly List<TextMeshProUGUI> shopSellDetailLines = new List<TextMeshProUGUI>();
        private readonly List<SkillRowView> guildSkillRows = new List<SkillRowView>();
        private readonly List<TextMeshProUGUI> guildSkillDetailLines = new List<TextMeshProUGUI>();

        private GameObject mainFacilityPanel;
        private Button enterFloorMapButton;
        private Transform shopBuyRowsRoot;
        private Transform shopSellRowsRoot;
        private Transform shopBuyDetailBody;
        private Transform shopSellDetailBody;
        private Button shopPurchaseButton;
        private Button shopSellButton;
        private TextMeshProUGUI shopPurchaseButtonLabel;
        private TextMeshProUGUI shopSellButtonLabel;
        private Transform guildSkillRowsRoot;
        private Transform guildSkillDetailBody;
        private Button guildPrimaryButton;
        private Button guildSlot2Button;
        private TextMeshProUGUI guildPrimaryButtonLabel;
        private TextMeshProUGUI guildSlot2ButtonLabel;
        private ShrineRuneController shrineRuneController;
        private ForgeEnhanceController forgeEnhanceController;
        private ShopMode currentShopMode = ShopMode.Buy;
        private ShopStockEntry selectedBuyStock;
        private ShopSellEntry selectedSellEntry;
        private int selectedGuildSkillId;

        private Color activeText = new Color(0.94f, 0.94f, 0.94f, 1f);
        private Color inactiveText = new Color(0.46f, 0.46f, 0.52f, 1f);
        private Color activeButton = new Color(0.16f, 0.035f, 0.047f, 1f);
        private Color inactiveButton = new Color(0.09f, 0.09f, 0.09f, 1f);
        private Color redText = new Color(1f, 0.20f, 0.27f, 1f);
        private Color selectedShopRow = new Color(0.42f, 0.30f, 0.08f, 0.92f);

        private void Awake()
        {
            InitializeMockup();
        }

        public void InitializeMockup()
        {
            CacheHierarchy();
            WireButtons();
            ClearShopDetails();
            CloseFacilities();
        }

        public void ShowTavern() => ShowFacility("TAVERN");

        public void ShowShop() => ShowFacility("SHOP");

        public void ShowGuild() => ShowFacility("GUILD");

        public void ShowForge() => ShowFacility("FORGE");

        public void ShowShrine() => ShowFacility("SHRINE");

        public void ShowFacility(string facility)
        {
            if (mainFacilityPanel != null)
            {
                mainFacilityPanel.SetActive(true);
            }

            foreach (string key in Facilities)
            {
                bool active = key == facility;
                if (facilityGroups.TryGetValue(key, out GameObject group))
                {
                    group.SetActive(active);
                }

                if (facilityTabs.TryGetValue(key, out Button tab))
                {
                    SetTabVisual(tab, active);
                }
            }

            ShowSubFunction(facility, DefaultSubFunctionIndex(facility));
        }

        public void CloseFacilities()
        {
            foreach (string key in Facilities)
            {
                if (facilityGroups.TryGetValue(key, out GameObject group))
                {
                    group.SetActive(false);
                }

                if (facilityTabs.TryGetValue(key, out Button tab))
                {
                    SetTabVisual(tab, false);
                }
            }

            ClearShopDetails();

            if (mainFacilityPanel != null)
            {
                mainFacilityPanel.SetActive(false);
            }
        }

        public bool TryCloseTopPanel()
        {
            CacheHierarchy();
            if (mainFacilityPanel == null || !mainFacilityPanel.activeSelf)
            {
                return false;
            }

            CloseFacilities();
            return true;
        }

        public void ShowSubFunction(string facility, int index)
        {
            if (subContents.TryGetValue(facility, out List<GameObject> contents))
            {
                for (int i = 0; i < contents.Count; i++)
                {
                    contents[i].SetActive(i == index);
                }
            }

            if (subButtons.TryGetValue(facility, out List<Button> buttons))
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    SetSubButtonVisual(buttons[i], i == index);
                }
            }

            if (facility == "SHOP")
            {
                currentShopMode = index == 1 ? ShopMode.Sell : ShopMode.Buy;
                RefreshShopPanel();
            }

            if (facility == "GUILD" && index == 2)
            {
                RefreshGuildSkillPanel();
            }

            if (facility == "SHRINE")
            {
                shrineRuneController?.ShowTab(index);
            }

            if (facility == "FORGE")
            {
                forgeEnhanceController?.Refresh();
            }
        }

        private void CacheHierarchy()
        {
            facilityGroups.Clear();
            facilityTabs.Clear();
            subButtons.Clear();
            subContents.Clear();
            closeButtons.Clear();

            Transform mainPanel = transform.Find("MainFacilityPanel");
            mainFacilityPanel = mainPanel != null ? mainPanel.gameObject : null;
            Transform enterFloorMap = transform.Find("BottomNavigationBar/EnterFloorMapButton");
            enterFloorMapButton =
                enterFloorMap != null ? enterFloorMap.GetComponent<Button>() : null;

            foreach (string facility in Facilities)
            {
                Transform group = mainPanel != null ? mainPanel.Find("Facility_" + facility) : null;
                if (group != null)
                {
                    facilityGroups[facility] = group.gameObject;
                    subButtons[facility] = FindButtons(group.Find("SubFunctionRow"));
                    subContents[facility] = FindContentPanels(group.Find("MiddleContentArea"));
                    closeButtons[facility] = FindNamedButtons(group, "CloseButton_X");
                    AddUniqueButtons(
                        closeButtons[facility],
                        FindNamedButtons(group, "CloseButton")
                    );
                }

                Transform tab = transform.Find(
                    "BottomNavigationBar/FacilityTabs/FacilityTab_" + facility
                );
                if (tab != null && tab.TryGetComponent(out Button button))
                {
                    facilityTabs[facility] = button;
                }
            }

            CacheShopHierarchy();
            CacheGuildHierarchy();
            CacheForgeHierarchy();
            CacheShrineHierarchy();
        }

        private void CacheShopHierarchy()
        {
            shopBuyRows.Clear();
            shopSellRows.Clear();
            shopBuyDetailLines.Clear();
            shopSellDetailLines.Clear();

            shopBuyRowsRoot = null;
            shopSellRowsRoot = null;
            shopBuyDetailBody = null;
            shopSellDetailBody = null;
            shopPurchaseButton = null;
            shopSellButton = null;
            shopPurchaseButtonLabel = null;
            shopSellButtonLabel = null;

            if (!facilityGroups.TryGetValue("SHOP", out GameObject shopGroup))
            {
                return;
            }

            Transform root = shopGroup.transform;
            shopBuyRowsRoot = root.Find(
                "MiddleContentArea/Content_BUY/LeftColumn/MERCHANT_STOCK_Section/Rows"
            );
            shopSellRowsRoot = root.Find(
                "MiddleContentArea/Content_SELL/LeftColumn/SELLABLE_ITEMS_Section/Rows"
            );
            shopBuyDetailBody = root.Find(
                "MiddleContentArea/Content_BUY/RightColumn/ITEM_DETAIL_Section/Body"
            );
            shopSellDetailBody = root.Find(
                "MiddleContentArea/Content_SELL/RightColumn/SELL_DETAIL_Section/Body"
            );

            CacheShopRows(shopBuyRowsRoot, shopBuyRows);
            CacheShopRows(shopSellRowsRoot, shopSellRows);
            CacheDetailLines(shopBuyDetailBody, shopBuyDetailLines);
            CacheDetailLines(shopSellDetailBody, shopSellDetailLines);

            Transform purchase = FindDescendant(shopBuyDetailBody, "PrimaryButton_PURCHASE");
            if (purchase != null)
            {
                shopPurchaseButton = purchase.GetComponent<Button>();
                shopPurchaseButtonLabel = purchase.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            Transform sell = FindDescendant(shopSellDetailBody, "PrimaryButton_SELL");
            if (sell != null)
            {
                shopSellButton = sell.GetComponent<Button>();
                shopSellButtonLabel = sell.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        private void CacheGuildHierarchy()
        {
            guildSkillRows.Clear();
            guildSkillDetailLines.Clear();
            guildSkillRowsRoot = null;
            guildSkillDetailBody = null;
            guildPrimaryButton = null;
            guildPrimaryButtonLabel = null;
            guildSlot2Button = null;
            guildSlot2ButtonLabel = null;

            if (!facilityGroups.TryGetValue("GUILD", out GameObject guildGroup))
            {
                return;
            }

            Transform root = guildGroup.transform;
            guildSkillRowsRoot = root.Find(
                "MiddleContentArea/Content_SKILLS/LeftColumn/SKILL_SHOP_Section/Rows"
            );
            guildSkillDetailBody = root.Find(
                "MiddleContentArea/Content_SKILLS/RightColumn/SKILL_DETAIL_Section/Body"
            );

            CacheGuildRows(guildSkillRowsRoot, guildSkillRows);
            CacheDetailLines(guildSkillDetailBody, guildSkillDetailLines);

            Transform primary = FindDescendant(
                guildSkillDetailBody,
                "PrimaryButton_PURCHASE_SKILL"
            );
            if (primary != null)
            {
                guildPrimaryButton = primary.GetComponent<Button>();
                guildPrimaryButtonLabel = primary.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            EnsureGuildSlot2Button();
        }

        private void CacheShrineHierarchy()
        {
            shrineRuneController = null;
            if (!facilityGroups.TryGetValue("SHRINE", out GameObject shrineGroup))
            {
                return;
            }

            shrineRuneController = shrineGroup.GetComponent<ShrineRuneController>();
            if (shrineRuneController == null)
            {
                shrineRuneController = shrineGroup.GetComponentInChildren<ShrineRuneController>(
                    true
                );
            }
        }

        private void CacheForgeHierarchy()
        {
            forgeEnhanceController = null;
            if (!facilityGroups.TryGetValue("FORGE", out GameObject forgeGroup))
            {
                return;
            }

            forgeEnhanceController = forgeGroup.GetComponent<ForgeEnhanceController>();
            if (forgeEnhanceController == null)
            {
                forgeEnhanceController = forgeGroup.GetComponentInChildren<ForgeEnhanceController>(
                    true
                );
            }

            if (forgeEnhanceController == null)
            {
                forgeEnhanceController = forgeGroup.AddComponent<ForgeEnhanceController>();
            }

            forgeEnhanceController.Initialize();
        }

        private void WireButtons()
        {
            if (enterFloorMapButton != null)
            {
                enterFloorMapButton.onClick.RemoveAllListeners();
                enterFloorMapButton.onClick.AddListener(EnterFloorMap);
            }

            foreach (string facility in Facilities)
            {
                if (facilityTabs.TryGetValue(facility, out Button tab))
                {
                    tab.onClick.RemoveAllListeners();
                    string capturedFacility = facility;
                    tab.onClick.AddListener(() => ShowFacility(capturedFacility));
                }

                if (!subButtons.TryGetValue(facility, out List<Button> buttons))
                {
                    continue;
                }

                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].onClick.RemoveAllListeners();
                    string capturedFacility = facility;
                    int capturedIndex = i;
                    buttons[i]
                        .onClick.AddListener(() =>
                            ShowSubFunction(capturedFacility, capturedIndex)
                        );
                }

                if (!closeButtons.TryGetValue(facility, out List<Button> closeList))
                {
                    continue;
                }

                for (int i = 0; i < closeList.Count; i++)
                {
                    closeList[i].onClick.RemoveAllListeners();
                    closeList[i].onClick.AddListener(CloseFacilities);
                }
            }
        }

        private static void EnterFloorMap()
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.Scenes == null)
            {
                Debug.LogError(
                    "[SafeZone1FacilityMockupUI] GameSystemManager.Scenes 참조가 없습니다."
                );
                return;
            }

            gsm.LoadFloorMapFromSafe(1);
        }

        private void RefreshShopPanel()
        {
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            EnsureShopStock(run);

            if (currentShopMode == ShopMode.Sell)
            {
                RefreshSellList(run, data);
            }
            else
            {
                RefreshBuyList(run, data);
            }
        }

        private void RefreshGuildSkillPanel()
        {
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            List<SkillData> skills = BuildGuildSkillList(run, data);
            EnsureGuildRowCount(skills.Count);

            bool selectedValid =
                selectedGuildSkillId != 0
                && skills.Exists(skill => skill.Id == selectedGuildSkillId);
            for (int i = 0; i < guildSkillRows.Count; i++)
            {
                SkillRowView row = guildSkillRows[i];
                if (i >= skills.Count)
                {
                    row.Root.SetActive(false);
                    continue;
                }

                SkillData skill = skills[i];
                bool purchased =
                    run.Player?.OwnedSkillIds != null
                    && run.Player.OwnedSkillIds.Contains(skill.Id)
                    && skill.AcquireType == AcquireType.Shop;
                SetGuildRow(
                    row,
                    skill,
                    BuildGuildSkillRowSuffix(skill, run, data),
                    selectedValid && selectedGuildSkillId == skill.Id,
                    purchased
                );

                row.Button.onClick.RemoveAllListeners();
                int capturedSkillId = skill.Id;
                row.Button.onClick.AddListener(() => HandleGuildSkillSelected(capturedSkillId));
            }

            if (selectedValid)
            {
                ShowGuildSkillDetail(selectedGuildSkillId, run, data);
            }
            else
            {
                ClearGuildSkillDetail();
            }
        }

        private static List<SkillData> BuildGuildSkillList(GameRunState run, DataManager data)
        {
            var skills = new List<SkillData>();
            if (data?.Skills == null)
            {
                return skills;
            }

            foreach (SkillData skill in data.Skills.Values)
            {
                if (
                    skill == null
                    || skill.SkillType != SkillType.Active
                    || skill.AcquireType == AcquireType.MonsterOnly
                )
                {
                    continue;
                }

                bool owned =
                    run.Player?.OwnedSkillIds != null
                    && run.Player.OwnedSkillIds.Contains(skill.Id);
                if (skill.AcquireType == AcquireType.Shop || owned)
                {
                    skills.Add(skill);
                }
            }

            skills.Sort((a, b) => a.Id.CompareTo(b.Id));
            return skills;
        }

        private static string BuildGuildSkillRowSuffix(
            SkillData skill,
            GameRunState run,
            DataManager data
        )
        {
            bool owned =
                run.Player?.OwnedSkillIds != null && run.Player.OwnedSkillIds.Contains(skill.Id);
            if (IsSkillEquipped(run, skill.Id))
            {
                return "EQUIPPED";
            }

            if (owned)
            {
                return "OWNED";
            }

            int price = Guild.GetSkillBuyPrice(skill.Id, run, data);
            return price > 0 ? price + " G" : "LOCKED";
        }

        private void RefreshBuyList(GameRunState run, DataManager data)
        {
            List<ShopStockEntry> entries = new List<ShopStockEntry>();
            if (run.ShopStock?.Entries != null)
            {
                for (int i = 0; i < run.ShopStock.Entries.Count; i++)
                {
                    ShopStockEntry entry = run.ShopStock.Entries[i];
                    if (entry == null || !entry.CanPurchase)
                    {
                        continue;
                    }

                    if (!data.Items.ContainsKey(entry.ItemId))
                    {
                        Debug.LogError(
                            "[SafeZone1FacilityMockupUI] 상점 재고 아이템 ID 없음: " + entry.ItemId
                        );
                        continue;
                    }

                    entries.Add(entry);
                }
            }

            EnsureRowCount(shopBuyRows, shopBuyRowsRoot, entries.Count);

            bool selectionValid =
                selectedBuyStock != null
                && selectedBuyStock.CanPurchase
                && entries.Contains(selectedBuyStock);
            for (int i = 0; i < shopBuyRows.Count; i++)
            {
                ShopRowView row = shopBuyRows[i];
                if (i >= entries.Count)
                {
                    row.Root.SetActive(false);
                    continue;
                }

                ShopStockEntry entry = entries[i];
                ItemData item = data.Items[entry.ItemId];
                int price = Shop.GetStockBuyPrice(entry, run, data);
                SetShopRow(
                    row,
                    item,
                    BuildItemRole(item),
                    BuildItemDescription(item),
                    price + " G",
                    selectionValid && selectedBuyStock == entry
                );

                row.Button.onClick.RemoveAllListeners();
                ShopStockEntry captured = entry;
                row.Button.onClick.AddListener(() => HandleBuyRowSelected(captured));
            }

            if (selectionValid)
            {
                ShowBuyDetail(selectedBuyStock, run, data);
            }
            else
            {
                ClearBuyDetail();
            }
        }

        private void RefreshSellList(GameRunState run, DataManager data)
        {
            List<ShopSellEntry> entries = BuildSellEntries(run, data);
            EnsureRowCount(shopSellRows, shopSellRowsRoot, entries.Count);

            bool selectionValid = IsSellSelectionValid(selectedSellEntry, run);
            for (int i = 0; i < shopSellRows.Count; i++)
            {
                ShopRowView row = shopSellRows[i];
                if (i >= entries.Count)
                {
                    row.Root.SetActive(false);
                    continue;
                }

                ShopSellEntry entry = entries[i];
                ItemData item = entry.IsEquipment
                    ? entry.EquipmentItem.Data
                    : data.Items[entry.ItemId];
                string suffix = entry.IsEquipment
                    ? "+ " + entry.EquipmentItem.Enhancement
                    : "x" + entry.Count;
                SetShopRow(
                    row,
                    item,
                    BuildItemRole(item),
                    BuildItemDescription(item),
                    suffix + " / " + Safe1ShopSellPrice + " G",
                    selectionValid && IsSameSellEntry(selectedSellEntry, entry)
                );

                row.Button.onClick.RemoveAllListeners();
                ShopSellEntry captured = entry;
                row.Button.onClick.AddListener(() => HandleSellRowSelected(captured));
            }

            if (selectionValid)
            {
                ShowSellDetail(selectedSellEntry, run, data);
            }
            else
            {
                ClearSellDetail();
            }
        }

        private List<ShopSellEntry> BuildSellEntries(GameRunState run, DataManager data)
        {
            var entries = new List<ShopSellEntry>();
            InventoryState inventory = run?.Player?.Inventory;
            if (inventory == null)
            {
                return entries;
            }

            foreach (KeyValuePair<int, int> stack in inventory.StackableItems)
            {
                if (stack.Value <= 0)
                {
                    continue;
                }

                if (!data.Items.ContainsKey(stack.Key))
                {
                    Debug.LogError(
                        "[SafeZone1FacilityMockupUI] 판매 가능 아이템 ID 없음: " + stack.Key
                    );
                    continue;
                }

                entries.Add(new ShopSellEntry { ItemId = stack.Key, Count = stack.Value });
            }

            for (int i = 0; i < inventory.EquipItems.Count; i++)
            {
                Item item = inventory.EquipItems[i];
                if (item?.Data == null)
                {
                    Debug.LogError("[SafeZone1FacilityMockupUI] 판매 장비의 Data 참조가 없습니다.");
                    continue;
                }

                entries.Add(
                    new ShopSellEntry
                    {
                        ItemId = item.Data.Id,
                        Count = 1,
                        EquipmentItem = item,
                    }
                );
            }

            return entries;
        }

        private void HandleBuyRowSelected(ShopStockEntry stock)
        {
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            selectedBuyStock = stock;
            RefreshBuyList(run, data);
        }

        private void HandleSellRowSelected(ShopSellEntry entry)
        {
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            selectedSellEntry = entry;
            RefreshSellList(run, data);
        }

        private void HandleGuildSkillSelected(int skillId)
        {
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            selectedGuildSkillId = skillId;
            RefreshGuildSkillPanel();
            ShowGuildSkillDetail(skillId, run, data);
        }

        private void ShowBuyDetail(ShopStockEntry stock, GameRunState run, DataManager data)
        {
            if (stock == null || !data.Items.TryGetValue(stock.ItemId, out ItemData item))
            {
                ClearBuyDetail();
                return;
            }

            selectedBuyStock = stock;
            int price = Shop.GetStockBuyPrice(stock, run, data);
            SetDetailBody(shopBuyDetailBody, true);
            SetDetailLines(
                shopBuyDetailLines,
                item.NameKey,
                BuildItemRole(item),
                BuildItemDescription(item),
                "Buy Price",
                price + " G"
            );

            if (shopPurchaseButton != null)
            {
                shopPurchaseButton.onClick.RemoveAllListeners();
                shopPurchaseButton.onClick.AddListener(HandlePurchaseClicked);
                shopPurchaseButton.interactable =
                    stock.CanPurchase
                    && run.Gold >= price
                    && CanFitInventory(run.Player?.Inventory, item);
            }

            if (shopPurchaseButtonLabel != null)
            {
                shopPurchaseButtonLabel.text = "PURCHASE";
            }
        }

        private void ShowSellDetail(ShopSellEntry entry, GameRunState run, DataManager data)
        {
            if (entry == null)
            {
                ClearSellDetail();
                return;
            }

            ItemData item = entry.IsEquipment
                ? entry.EquipmentItem.Data
                : (data.Items.TryGetValue(entry.ItemId, out ItemData stackItem) ? stackItem : null);
            if (item == null)
            {
                ClearSellDetail();
                return;
            }

            selectedSellEntry = entry;
            SetDetailBody(shopSellDetailBody, true);
            SetDetailLines(
                shopSellDetailLines,
                item.NameKey,
                BuildItemRole(item),
                BuildItemDescription(item),
                "Sell Price",
                Safe1ShopSellPrice + " G"
            );

            if (shopSellButton != null)
            {
                shopSellButton.onClick.RemoveAllListeners();
                shopSellButton.onClick.AddListener(HandleSellClicked);
                shopSellButton.interactable = IsSellSelectionValid(entry, run);
            }

            if (shopSellButtonLabel != null)
            {
                shopSellButtonLabel.text = "SELL";
            }
        }

        private void ShowGuildSkillDetail(int skillId, GameRunState run, DataManager data)
        {
            if (!data.Skills.TryGetValue(skillId, out SkillData skill))
            {
                ClearGuildSkillDetail();
                return;
            }

            selectedGuildSkillId = skillId;
            SetDetailBody(guildSkillDetailBody, true);
            SetDetailLines(
                guildSkillDetailLines,
                skill.NameKey,
                skill.SkillType + " / " + skill.AcquireType,
                BuildSkillEffectText(skill),
                "Slots",
                BuildActiveSlotText(run, data)
            );

            bool owned =
                run.Player?.OwnedSkillIds != null && run.Player.OwnedSkillIds.Contains(skillId);
            bool canEquip = owned;
            bool buyable = skill.AcquireType == AcquireType.Shop && !owned;

            if (buyable)
            {
                ConfigureGuildButton(
                    guildPrimaryButton,
                    guildPrimaryButtonLabel,
                    "PURCHASE",
                    Guild.CanBuy(skillId, run, data),
                    () => HandleGuildPurchaseClicked(skillId)
                );
                ConfigureGuildButton(
                    guildSlot2Button,
                    guildSlot2ButtonLabel,
                    string.Empty,
                    false,
                    null
                );
                return;
            }

            ConfigureGuildButton(
                guildPrimaryButton,
                guildPrimaryButtonLabel,
                "SLOT 1",
                canEquip,
                () => HandleGuildEquipClicked(0, skillId)
            );
            ConfigureGuildButton(
                guildSlot2Button,
                guildSlot2ButtonLabel,
                "SLOT 2",
                canEquip,
                () => HandleGuildEquipClicked(1, skillId)
            );
        }

        private void HandlePurchaseClicked()
        {
            if (
                selectedBuyStock == null
                || !TryGetRunData(out GameRunState run, out DataManager data)
            )
            {
                return;
            }

            if (Shop.TryBuyStock(selectedBuyStock, run, data))
            {
                SaveAfterShopTransaction();
                RefreshBuyList(run, data);
            }
        }

        private void HandleSellClicked()
        {
            if (
                selectedSellEntry == null
                || !TryGetRunData(out GameRunState run, out DataManager data)
            )
            {
                return;
            }

            bool sold = selectedSellEntry.IsEquipment
                ? Shop.TrySellEquipForPrice(
                    selectedSellEntry.EquipmentItem,
                    Safe1ShopSellPrice,
                    run,
                    data
                )
                : Shop.TrySellForPrice(selectedSellEntry.ItemId, 1, Safe1ShopSellPrice, run, data);

            if (sold)
            {
                SaveAfterShopTransaction();
                RefreshSellList(run, data);
            }
        }

        private void HandleGuildPurchaseClicked(int skillId)
        {
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            if (Guild.TryBuySkill(skillId, run, data))
            {
                SaveAfterShopTransaction();
                RefreshGuildSkillPanel();
            }
        }

        private void HandleGuildEquipClicked(int slotIndex, int skillId)
        {
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            if (SkillSwap.TrySetSlot(slotIndex, skillId, run, data))
            {
                SaveAfterShopTransaction();
                RefreshGuildSkillPanel();
            }
        }

        private void ClearShopDetails()
        {
            ClearBuyDetail();
            ClearSellDetail();
        }

        private void ClearBuyDetail()
        {
            selectedBuyStock = null;
            SetDetailBody(shopBuyDetailBody, false);
            if (shopPurchaseButton != null)
            {
                shopPurchaseButton.onClick.RemoveAllListeners();
                shopPurchaseButton.interactable = false;
            }
        }

        private void ClearSellDetail()
        {
            selectedSellEntry = null;
            SetDetailBody(shopSellDetailBody, false);
            if (shopSellButton != null)
            {
                shopSellButton.onClick.RemoveAllListeners();
                shopSellButton.interactable = false;
            }
        }

        private void ClearGuildSkillDetail()
        {
            selectedGuildSkillId = 0;
            SetDetailBody(guildSkillDetailBody, false);
            ConfigureGuildButton(
                guildPrimaryButton,
                guildPrimaryButtonLabel,
                string.Empty,
                false,
                null
            );
            ConfigureGuildButton(
                guildSlot2Button,
                guildSlot2ButtonLabel,
                string.Empty,
                false,
                null
            );
        }

        private static void EnsureShopStock(GameRunState run)
        {
            if (run.ShopStock == null)
            {
                run.ShopStock = ShopStockState.CreateDefaultSafe1Stock();
                return;
            }

            run.ShopStock.EnsureDefaultSafe1Stock();
        }

        private static bool CanFitInventory(InventoryState inventory, ItemData item)
        {
            return inventory != null && item != null && !inventory.IsFull(item);
        }

        private static bool IsSellSelectionValid(ShopSellEntry entry, GameRunState run)
        {
            InventoryState inventory = run?.Player?.Inventory;
            if (entry == null || inventory == null)
            {
                return false;
            }

            return entry.IsEquipment
                ? inventory.EquipItems.Contains(entry.EquipmentItem)
                : inventory.CountOf(entry.ItemId) > 0;
        }

        private static bool IsSameSellEntry(ShopSellEntry left, ShopSellEntry right)
        {
            if (left == null || right == null || left.IsEquipment != right.IsEquipment)
            {
                return false;
            }

            return left.IsEquipment
                ? left.EquipmentItem == right.EquipmentItem
                : left.ItemId == right.ItemId;
        }

        private static void SaveAfterShopTransaction()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Save?.SaveSnapshot();
            }
        }

        private static bool TryGetRunData(out GameRunState run, out DataManager data)
        {
            run = null;
            data = null;
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun?.Player?.Inventory == null
                || gsm.Data?.Items == null
                || gsm.Data?.Skills == null
            )
            {
                Debug.LogError(
                    "[SafeZone1FacilityMockupUI] GameSystemManager / CurrentRun.Player.Inventory / Data.Items / Data.Skills 참조가 없습니다."
                );
                return false;
            }

            run = gsm.CurrentRun;
            data = gsm.Data;
            return true;
        }

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

            return new ShopRowView
            {
                Root = row.gameObject,
                Button = button,
                Background = background,
                BaseColor = background.color,
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
                primaryRect.anchoredPosition += new Vector2(-primaryRect.sizeDelta.x * 0.53f, 0f);
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
                return item.DescKey;
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

            return item.NameKey;
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
                row.Name.text = item != null ? item.NameKey : string.Empty;
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
                row.Label.text = skill.NameKey + "  " + suffix;
                row.Label.color = purchased ? Color.red : row.LabelBaseColor;
            }

            if (row.Background != null)
            {
                row.Background.color = selected ? selectedShopRow : row.BaseColor;
            }
        }

        private void SetRowSelected(ShopRowView row, bool selected)
        {
            if (row?.Background != null)
            {
                row.Background.color = selected ? selectedShopRow : row.BaseColor;
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
}

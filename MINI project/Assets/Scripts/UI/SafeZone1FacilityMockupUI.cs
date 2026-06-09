using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Safe1 mockup interaction layer. Keeps the static design inspectable while making
/// facility tabs and sub-function buttons switch the matching RectTransform groups.
/// </summary>
public sealed partial class SafeZone1FacilityMockupUI : MonoBehaviour
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
        public Outline Border;
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
    private const float ShopRowPreferredHeight = 78f;
    private const int Safe1ShopSellPrice = 1;
    private static readonly Color ShopRowBaseColor = new Color(0.09f, 0.09f, 0.09f, 0.96f);
    private static readonly Color ShopRowSelectedColor = new Color(0.42f, 0.30f, 0.08f, 0.96f);
    private static readonly Color ShopRowBorderColor = new Color(0.70f, 0.62f, 0.42f, 1f);
    private static readonly Color ShopRowSelectedBorderColor = new Color(1f, 0.76f, 0.18f, 1f);
    private static readonly Color ShopRowTextColor = new Color(0.88f, 0.88f, 0.86f, 1f);

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
    private TavernRecruitmentController tavernRecruitmentController;
    private GuildPartyController guildPartyController;
    private GuildCompanionsController guildCompanionsController;
    private ShopMode currentShopMode = ShopMode.Buy;
    private ShopStockEntry selectedBuyStock;
    private ShopSellEntry selectedSellEntry;
    private int selectedGuildSkillId;

    private Color activeText = new Color(0.94f, 0.94f, 0.94f, 1f);
    private Color inactiveText = new Color(0.46f, 0.46f, 0.52f, 1f);
    private Color activeButton = new Color(0.16f, 0.035f, 0.047f, 1f);
    private Color inactiveButton = new Color(0.09f, 0.09f, 0.09f, 1f);
    private Color redText = new Color(1f, 0.20f, 0.27f, 1f);
    private Color selectedShopRow = ShopRowSelectedColor;

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
        guildCompanionsController?.CloseRuneTree();
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
        if (guildCompanionsController != null && guildCompanionsController.TryCloseRuneTree())
        {
            return true;
        }

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

        if (facility == "GUILD" && index == 0)
        {
            guildPartyController?.Refresh();
        }

        if (facility == "TAVERN")
        {
            tavernRecruitmentController?.Refresh();
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
        CacheTavernHierarchy();
        CacheGuildPartyHierarchy();
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
        shopBuyRowsRoot = FindFirstExisting(
            root,
            "MiddleContentArea/Content_BUY/LeftColumn/MERCHANT_STOCK_Section/Rows_ScrollView/Rows",
            "MiddleContentArea/Content_BUY/LeftColumn/MERCHANT_STOCK_Section/Rows"
        );
        shopSellRowsRoot = FindFirstExisting(
            root,
            "MiddleContentArea/Content_SELL/LeftColumn/SELLABLE_ITEMS_Section/Rows_ScrollView/Rows",
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
        EnsureRowsScrollContent(shopBuyRowsRoot);
        EnsureRowsScrollContent(shopSellRowsRoot);
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
    }

    private void CacheTavernHierarchy()
    {
        tavernRecruitmentController = null;
        if (!facilityGroups.TryGetValue("TAVERN", out GameObject tavernGroup))
        {
            return;
        }

        tavernRecruitmentController = tavernGroup.GetComponent<TavernRecruitmentController>();
    }

    private void CacheGuildPartyHierarchy()
    {
        guildPartyController = null;
        guildCompanionsController = null;
        if (!facilityGroups.TryGetValue("GUILD", out GameObject guildGroup))
        {
            return;
        }

        guildPartyController = guildGroup.GetComponent<GuildPartyController>();
        guildCompanionsController =
            guildGroup.GetComponentInChildren<GuildCompanionsController>(true);
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
            GameLog.LogError(
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
                    GameLog.LogError(
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
                GameLog.LogError(
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
                GameLog.LogError("[SafeZone1FacilityMockupUI] 판매 장비의 Data 참조가 없습니다.");
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
            LocalizeKey(item.NameKey),
            BuildItemRole(item),
            BuildItemDescription(item),
            Loc.Get("shop_buy_price"),
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
            shopPurchaseButtonLabel.text = Loc.Get("shop_purchase");
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
            LocalizeKey(item.NameKey),
            BuildItemRole(item),
            BuildItemDescription(item),
            Loc.Get("shop_sell_price"),
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
            shopSellButtonLabel.text = Loc.Get("shop_sell");
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
        SkillData displaySkill = SkillRuntimeResolver.Resolve(
            skill,
            SkillRuntimeResolver.ResolveRuneClass(run?.Player),
            data
        );
        SetDetailBody(guildSkillDetailBody, true);
        SetDetailLines(
            guildSkillDetailLines,
            LocalizeKey(skill.NameKey),
            skill.SkillType + " / " + skill.AcquireType,
            BuildSkillEffectText(displaySkill),
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
            GameLog.LogError(
                "[SafeZone1FacilityMockupUI] GameSystemManager / CurrentRun.Player.Inventory / Data.Items / Data.Skills 참조가 없습니다."
            );
            return false;
        }

        run = gsm.CurrentRun;
        data = gsm.Data;
        return true;
    }

}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Guid2 §11 2026-05-28: 상점 화면 단일 진입점.
// - 구매 목록 + 판매 후보 목록 + ItemInfoPanel.
// - 데이터 변경은 직접 하지 않고 Shop 정적 유틸과 EventBus 만 사용.
// - fallback 금지. Inspector 미연결 시 enabled = false.
/// <summary>
/// Safe1 상점 화면. 구매/판매 목록을 표시하고 선택 항목은 ItemInfoPanel 로 넘긴다.
/// </summary>
public sealed class ShopPage : MonoBehaviour
{
    private const float ListEntryPreferredHeight = 78f;

    [SerializeField]
    private GameObject root;

    [SerializeField]
    private Transform buyListRoot;

    [SerializeField]
    private Transform sellListRoot;

    [SerializeField]
    private Button itemEntryPrefab;

    [SerializeField]
    private Text goldLabel;

    [SerializeField]
    private ItemInfoPanel infoPanel;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        root.SetActive(false);
    }

    /// <summary>상점 화면을 열고 EventBus 구독을 시작한다.</summary>
    public void OnOpen()
    {
        if (!enabled)
        {
            return;
        }

        root.SetActive(true);
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
        {
            gsm.Events.OnInventoryChanged -= Refresh;
            gsm.Events.OnGoldChanged -= OnGoldChanged;
            gsm.Events.OnInventoryChanged += Refresh;
            gsm.Events.OnGoldChanged += OnGoldChanged;
        }

        Refresh();
    }

    /// <summary>상점 화면을 닫고 EventBus 구독을 해제한다.</summary>
    public void OnClose()
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
        {
            gsm.Events.OnInventoryChanged -= Refresh;
            gsm.Events.OnGoldChanged -= OnGoldChanged;
        }

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    /// <summary>구매 가능 목록, 판매 후보 목록, 골드 표시를 현재 런 상태로 다시 그린다.</summary>
    public void Refresh()
    {
        if (!TryGetRunData(out GameRunState run, out DataManager data))
        {
            return;
        }

        goldLabel.text = Loc.Format("shop_gold_fmt", run.Gold);
        ClearList(buyListRoot);
        ClearList(sellListRoot);

        var buyItems = new List<ItemData>(data.Items.Values);
        buyItems.Sort((a, b) => a.Id.CompareTo(b.Id));
        foreach (ItemData itemData in buyItems)
        {
            int price = Shop.GetBuyPrice(itemData.Id, run, data);
            Button entry = UIListEntryFactory.SpawnListEntry(
                itemEntryPrefab,
                buyListRoot,
                LocalizeKey(itemData.NameKey) + "  " + price + "G",
                () => infoPanel.Show(itemData.Id, ItemDetailContext.Shop),
                "[ShopPage] itemEntryPrefab 하위 텍스트 참조가 없습니다."
            );
            EnsureListEntryLayout(entry.transform);
        }

        InventoryState inv = run.Player.Inventory;
        var stacks = new List<KeyValuePair<int, int>>(inv.StackableItems);
        stacks.Sort((a, b) => a.Key.CompareTo(b.Key));
        foreach (KeyValuePair<int, int> entry in stacks)
        {
            if (data.Items.TryGetValue(entry.Key, out ItemData itemData))
            {
                int price = Shop.GetSellPrice(entry.Key, run, data, data.Balance);
                Button listEntry = UIListEntryFactory.SpawnListEntry(
                    itemEntryPrefab,
                    sellListRoot,
                    LocalizeKey(itemData.NameKey)
                        + " x"
                        + entry.Value
                        + "  "
                        + price
                        + "G",
                    () => infoPanel.Show(entry.Key, ItemDetailContext.Shop),
                    "[ShopPage] itemEntryPrefab 하위 텍스트 참조가 없습니다."
                );
                EnsureListEntryLayout(listEntry.transform);
            }
        }

        foreach (Item item in inv.EquipItems)
        {
            if (item?.Data != null)
            {
                int price = Shop.GetSellPrice(item.Data.Id, run, data, data.Balance);
                Button entry = UIListEntryFactory.SpawnListEntry(
                    itemEntryPrefab,
                    sellListRoot,
                    LocalizeKey(item.Data.NameKey)
                        + " +"
                        + item.Enhancement
                        + "  "
                        + price
                        + "G",
                    () => infoPanel.ShowEquip(item, ItemDetailContext.Shop),
                    "[ShopPage] itemEntryPrefab 하위 텍스트 참조가 없습니다."
                );
                EnsureListEntryLayout(entry.transform);
            }
        }
    }

    private bool ValidateReferences()
    {
        ResolveRowsRoots();

        bool valid =
            root != null
            && buyListRoot != null
            && sellListRoot != null
            && itemEntryPrefab != null
            && goldLabel != null
            && infoPanel != null;
        if (!valid)
        {
            GameLog.LogError(
                "[ShopPage] 필수 UI 참조가 Inspector 에 직접 할당되어 있지 않습니다."
            );
        }

        return valid;
    }

    private void ResolveRowsRoots()
    {
        if (root == null)
        {
            return;
        }

        Transform rootTransform = root.transform;
        buyListRoot = ResolveRowsRoot(
            rootTransform,
            buyListRoot,
            "Content_BUY",
            "MERCHANT_STOCK_Section"
        );
        sellListRoot = ResolveRowsRoot(
            rootTransform,
            sellListRoot,
            "Content_SELL",
            "SELLABLE_ITEMS_Section"
        );
    }

    private static Transform ResolveRowsRoot(
        Transform root,
        Transform current,
        string contentName,
        string sectionName
    )
    {
        Transform resolved = FindRowsUnderSection(root, contentName, sectionName);
        if (resolved != null)
        {
            EnsureRowsScrollContent(resolved);
            return resolved;
        }

        EnsureRowsScrollContent(current);
        return current;
    }

    private static Transform FindRowsUnderSection(
        Transform root,
        string contentName,
        string sectionName
    )
    {
        Transform section = FindDescendant(root, sectionName);
        if (section == null || !HasAncestor(section, contentName))
        {
            return null;
        }

        Transform rows = section.Find("Rows_ScrollView/Rows");
        if (rows != null)
        {
            return rows;
        }

        return section.Find("Rows");
    }

    private static bool HasAncestor(Transform transform, string ancestorName)
    {
        for (Transform current = transform; current != null; current = current.parent)
        {
            if (current.name == ancestorName)
            {
                return true;
            }
        }

        return false;
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

    private static void EnsureRowsScrollContent(Transform rowsRoot)
    {
        if (rowsRoot == null || !(rowsRoot is RectTransform rowsRect))
        {
            return;
        }

        EnsureRowsContainerLayout(rowsRoot);
        ScrollRect scrollRect = rowsRoot.GetComponentInParent<ScrollRect>(true);
        if (scrollRect != null && scrollRect.content != rowsRect)
        {
            scrollRect.content = rowsRect;
        }
    }

    private static void EnsureRowsContainerLayout(Transform rowsRoot)
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

        ContentSizeFitter fitter = rowsRoot.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = rowsRoot.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static void EnsureListEntryLayout(Transform entry)
    {
        if (entry == null)
        {
            return;
        }

        LayoutElement layout = entry.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = entry.gameObject.AddComponent<LayoutElement>();
        }

        float height = ListEntryPreferredHeight;
        if (entry is RectTransform rect && rect.sizeDelta.y > 0f)
        {
            height = Mathf.Max(ListEntryPreferredHeight, rect.sizeDelta.y);
        }

        layout.ignoreLayout = false;
        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleHeight = 0f;
    }

    private static string LocalizeKey(string key)
    {
        return string.IsNullOrEmpty(key) ? string.Empty : Loc.Get(key);
    }

    private static bool TryGetRunData(out GameRunState run, out DataManager data)
    {
        run = null;
        data = null;
        if (
            !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
            || gsm.CurrentRun?.Player?.Inventory == null
            || gsm.Data?.Items == null
        )
        {
            GameLog.LogError(
                "[ShopPage] GameSystemManager / CurrentRun.Player.Inventory / Data.Items 참조가 없습니다."
            );
            return false;
        }

        run = gsm.CurrentRun;
        data = gsm.Data;
        return true;
    }

    private void OnGoldChanged(int value)
    {
        goldLabel.text = Loc.Format("shop_gold_fmt", value);
    }

    private static void ClearList(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}

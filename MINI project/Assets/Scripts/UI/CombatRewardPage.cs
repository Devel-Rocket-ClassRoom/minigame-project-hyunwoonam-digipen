using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CombatRewardPage : MonoBehaviour
{
    private sealed class ItemRowView
    {
        public GameObject Root;
        public Button Button;
        public Graphic Background;
        public Color BaseColor;
        public TMP_Text Icon;
        public TMP_Text Name;
        public TMP_Text Quantity;
    }

    [SerializeField]
    private TMP_Text statusText;

    [SerializeField]
    private TMP_Text expValueText;

    [SerializeField]
    private TMP_Text goldValueText;

    [SerializeField]
    private Transform itemRowsRoot;

    [SerializeField]
    private Button[] itemRowButtons = Array.Empty<Button>();

    [SerializeField]
    private Button getItemsButton;

    [SerializeField]
    private Button doneButton;

    [SerializeField]
    private Color selectedRowColor = new Color(0.42f, 0.30f, 0.08f, 0.92f);

    private readonly List<ItemRowView> rows = new List<ItemRowView>();
    private Action onCloseCallback;
    private CombatRewardClaimSession claimSession;
    private DataManager data;
    private bool initialized;
    private bool closing;

    public bool Show(
        NodeRewardSummary summary,
        CombatRewardClaimSession session,
        DataManager rewardData,
        Action onClose
    )
    {
        if (!EnsureInitialized() || session == null)
        {
            return false;
        }

        claimSession = session;
        data = rewardData;
        onCloseCallback = onClose;
        closing = false;

        gameObject.SetActive(true);
        BindSummary(summary);
        EnsureRowCount(session.Entries.Count);
        WireButtons();
        RefreshRows();
        SetStatus("Select item rewards to claim.");
        return true;
    }

    public void Hide()
    {
        RemoveButtonListeners();
        gameObject.SetActive(false);
    }

    private void BindSummary(NodeRewardSummary summary)
    {
        int exp = summary != null ? summary.TotalExp : 0;
        int gold = summary != null ? summary.TotalGold : 0;
        expValueText.text = "+" + exp;
        goldValueText.text = "+" + gold;
    }

    private void WireButtons()
    {
        RemoveButtonListeners();
        getItemsButton.onClick.AddListener(HandleGetItemsClicked);
        doneButton.onClick.AddListener(HandleDoneClicked);
    }

    private void RemoveButtonListeners()
    {
        getItemsButton?.onClick.RemoveAllListeners();
        doneButton?.onClick.RemoveAllListeners();

        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].Button?.onClick.RemoveAllListeners();
        }
    }

    private void RefreshRows()
    {
        if (claimSession == null)
        {
            return;
        }

        IReadOnlyList<CombatRewardClaimSession.Entry> entries = claimSession.Entries;
        for (int i = 0; i < rows.Count; i++)
        {
            ItemRowView row = rows[i];
            if (i >= entries.Count || entries[i].RemainingCount <= 0)
            {
                row.Root.SetActive(false);
                continue;
            }

            CombatRewardClaimSession.Entry entry = entries[i];
            row.Root.SetActive(true);
            row.Background.color = entry.IsSelected ? selectedRowColor : row.BaseColor;
            row.Name.text = ResolveItemName(entry.ItemId);
            row.Quantity.text = "x" + entry.RemainingCount;
            if (row.Icon != null)
            {
                row.Icon.text = "#" + entry.ItemId;
            }

            row.Button.onClick.RemoveAllListeners();
            int capturedItemId = entry.ItemId;
            row.Button.onClick.AddListener(() =>
            {
                claimSession.ToggleSelection(capturedItemId);
                RefreshRows();
            });
        }

        getItemsButton.interactable = HasRemainingItems();
    }

    private void HandleGetItemsClicked()
    {
        if (closing || claimSession == null)
        {
            return;
        }

        bool hadSelection = HasSelectedItems();
        int claimedCount = claimSession.ClaimSelected();
        RefreshRows();

        if (claimedCount > 0)
        {
            SetStatus("Claimed " + claimedCount + " item(s).");
        }
        else if (hadSelection)
        {
            SetStatus("Inventory space is insufficient.");
        }
        else
        {
            SetStatus("No items were selected.");
        }
    }

    private void HandleDoneClicked()
    {
        if (closing)
        {
            return;
        }

        closing = true;
        if (claimSession != null && !claimSession.GetItemsPressed)
        {
            claimSession.ClaimRemainingSequentially();
        }

        Action callback = onCloseCallback;
        onCloseCallback = null;
        Hide();
        callback?.Invoke();
    }

    private bool EnsureInitialized()
    {
        if (initialized)
        {
            return true;
        }

        bool valid =
            statusText != null
            && expValueText != null
            && goldValueText != null
            && itemRowsRoot != null
            && itemRowButtons != null
            && itemRowButtons.Length > 0
            && getItemsButton != null
            && doneButton != null;

        if (!valid)
        {
            GameLog.LogError(
                "[CombatRewardPage] RewardPanel 필수 UI 참조가 Inspector 에 연결되지 않았습니다."
            );
            return false;
        }

        rows.Clear();
        for (int i = 0; i < itemRowButtons.Length; i++)
        {
            ItemRowView row = CreateRowView(itemRowButtons[i]);
            if (row == null)
            {
                GameLog.LogError(
                    "[CombatRewardPage] ItemRow 버튼 또는 ItemName/Quantity 참조가 누락되었습니다: "
                        + i
                );
                return false;
            }

            rows.Add(row);
        }

        initialized = true;
        return true;
    }

    private void EnsureRowCount(int count)
    {
        UIRowPool.EnsureCount(
            rows,
            itemRowsRoot,
            count,
            v => v.Root,
            clone => CreateRowView(clone.GetComponent<Button>())
        );
    }

    private ItemRowView CreateRowView(Button button)
    {
        if (button == null)
        {
            return null;
        }

        Transform root = button.transform;
        Graphic background = button.targetGraphic != null
            ? button.targetGraphic
            : root.GetComponent<Graphic>();
        TMP_Text name = FindChildText(root, "ItemName");
        TMP_Text quantity = FindChildText(root, "Quantity");
        if (background == null || name == null || quantity == null)
        {
            return null;
        }

        button.targetGraphic = background;
        button.transition = Selectable.Transition.None;
        return new ItemRowView
        {
            Root = root.gameObject,
            Button = button,
            Background = background,
            BaseColor = background.color,
            Icon = FindChildText(root, "IconMark"),
            Name = name,
            Quantity = quantity,
        };
    }

    private string ResolveItemName(int itemId)
    {
        if (
            data?.Items != null
            && data.Items.TryGetValue(itemId, out ItemData itemData)
            && itemData != null
        )
        {
            return itemData.NameKey;
        }

        return "Missing Item #" + itemId;
    }

    private bool HasSelectedItems()
    {
        if (claimSession == null)
        {
            return false;
        }

        IReadOnlyList<CombatRewardClaimSession.Entry> entries = claimSession.Entries;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].IsSelected && entries[i].RemainingCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasRemainingItems()
    {
        if (claimSession == null)
        {
            return false;
        }

        IReadOnlyList<CombatRewardClaimSession.Entry> entries = claimSession.Entries;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].RemainingCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message ?? string.Empty;
        }
    }

    private static TMP_Text FindChildText(Transform root, string childName)
    {
        TMP_Text[] labels = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i].name == childName)
            {
                return labels[i];
            }
        }

        return null;
    }
}

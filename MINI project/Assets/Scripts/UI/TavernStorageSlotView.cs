using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TavernStorageSlotView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image selectionBorder;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text iconMark;
    [SerializeField] private TMP_Text quantity;

    private static readonly Color SelectedColor = new Color(1f, 0.72f, 0.12f, 1f);

    public bool IsConfigured =>
        button != null
        && selectionBorder != null
        && itemName != null
        && iconMark != null
        && quantity != null;

    public void SetEmpty(bool visible)
    {
        gameObject.SetActive(visible);
        if (!visible)
        {
            return;
        }

        button.interactable = false;
        itemName.text = string.Empty;
        iconMark.text = string.Empty;
        quantity.text = string.Empty;
        selectionBorder.color = Color.white;
    }

    public void SetItem(ItemData data, Item item, int count, bool selected)
    {
        gameObject.SetActive(true);
        button.interactable = true;
        itemName.text = data != null ? Loc.Get(data.NameKey) : string.Empty;
        iconMark.text = BuildIcon(data);
        quantity.text = item != null ? "+" + item.Enhancement : "x" + count;
        selectionBorder.color = selected ? SelectedColor : Color.white;
    }

    private static string BuildIcon(ItemData data)
    {
        if (data == null || string.IsNullOrEmpty(data.NameKey))
        {
            return string.Empty;
        }

        return data.NameKey.Substring(0, 1).ToUpperInvariant();
    }
}

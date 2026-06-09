using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryDiscardModalController : MonoBehaviour
{
[Header("Common References")]
[SerializeField] private CanvasGroup modalLayerCanvasGroup;
[SerializeField] private GameObject dimOverlay;
[SerializeField] private GameObject simpleConfirmPanel;
[SerializeField] private GameObject quantitySelectPanel;

[Header("Simple Confirm UI")]
[SerializeField] private TextMeshProUGUI simpleItemNameText;
[SerializeField] private TextMeshProUGUI simpleCategoryText;
[SerializeField] private TextMeshProUGUI simpleStatText;
[SerializeField] private TextMeshProUGUI simpleQuantityText;
[SerializeField] private Image simpleItemIcon;

[Header("Quantity Select UI")]
[SerializeField] private TextMeshProUGUI quantityItemNameText;
[SerializeField] private TextMeshProUGUI quantityCategoryText;
[SerializeField] private TextMeshProUGUI quantityDescriptionText;
[SerializeField] private TextMeshProUGUI quantityOwnedAmountText;
[SerializeField] private TextMeshProUGUI quantitySelectedValueText;
[SerializeField] private TextMeshProUGUI quantityCountHeaderLabel;
[SerializeField] private TextMeshProUGUI quantityWarningText;
[SerializeField] private TextMeshProUGUI quantityDiscardButtonText;
[SerializeField] private Image quantityItemIcon;
[SerializeField] private Button quantityMinusButton;
[SerializeField] private Button quantityPlusButton;

private int _selectedCount = 1;
private int _maxCount = 1;
private bool _isStackable = false;

private void Awake()
{
    CloseModal();
}

public void OpenSimpleConfirm(string itemName, string category, string stats, Sprite icon)
{
    _isStackable = false;
    simpleItemNameText.text = itemName;
    simpleCategoryText.text = category;
    simpleStatText.text = stats;
    simpleItemIcon.sprite = icon;
    simpleQuantityText.text = "x1";

    simpleConfirmPanel.SetActive(true);
    quantitySelectPanel.SetActive(false);
    ShowModalLayer();
}

public void OpenQuantitySelect(string itemName, string category, string description, int ownedCount, Sprite icon)
{
    _isStackable = true;
    _maxCount = ownedCount;
    _selectedCount = 1;

    quantityItemNameText.text = itemName;
    quantityCategoryText.text = category;
    quantityDescriptionText.text = description;
    quantityOwnedAmountText.text = $"x{ownedCount}";
    quantityItemIcon.sprite = icon;

    RefreshQuantityUI();

    simpleConfirmPanel.SetActive(false);
    quantitySelectPanel.SetActive(true);
    ShowModalLayer();
}

private void ShowModalLayer()
{
    modalLayerCanvasGroup.alpha = 1f;
    modalLayerCanvasGroup.blocksRaycasts = true;
    modalLayerCanvasGroup.interactable = true;
    dimOverlay.SetActive(true);
}

public void CloseModal()
{
    modalLayerCanvasGroup.alpha = 0f;
    modalLayerCanvasGroup.blocksRaycasts = false;
    modalLayerCanvasGroup.interactable = false;
    simpleConfirmPanel.SetActive(false);
    quantitySelectPanel.SetActive(false);
    dimOverlay.SetActive(false);
}

public void ConfirmDiscard()
{
    if (_isStackable)
    {
        GameLog.Log($"Discarded {_selectedCount} items.");
    }
    else
    {
        GameLog.Log($"Discarded item.");
    }
    CloseModal();
}

public void IncreaseCount()
{
    _selectedCount = Mathf.Min(_maxCount, _selectedCount + 1);
    RefreshQuantityUI();
}

public void DecreaseCount()
{
    _selectedCount = Mathf.Max(1, _selectedCount - 1);
    RefreshQuantityUI();
}

public void SetMinCount()
{
    _selectedCount = 1;
    RefreshQuantityUI();
}

public void SetMaxCount()
{
    _selectedCount = _maxCount;
    RefreshQuantityUI();
}

private void RefreshQuantityUI()
{
    quantitySelectedValueText.text = _selectedCount.ToString();
    quantityCountHeaderLabel.text = $"{_selectedCount} / {_maxCount}";
    
    // Update warning text with colored count (English)
    quantityWarningText.text = $"Discard <color=#EAB308>x{_selectedCount}</color> items?\nDiscarded items cannot be recovered.";
    
    quantityDiscardButtonText.text = $"DISCARD x{_selectedCount}";

    quantityMinusButton.interactable = _selectedCount > 1;
    quantityPlusButton.interactable = _selectedCount < _maxCount;
}
}

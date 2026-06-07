using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>Safe1 FORGE 강화 탭을 런 상태와 연결한다.</summary>
    public sealed class ForgeEnhanceController : UIEventPageBase
    {
        private sealed class ForgeRowView
        {
            public GameObject Root;
            public Button Button;
            public Graphic Background;
            public Color BaseColor;
            public TextMeshProUGUI Label;
        }

        private sealed class ForgeItemEntry
        {
            public Item Item;
            public string SourceLabel;
        }

        private readonly List<ForgeRowView> rows = new List<ForgeRowView>();
        private readonly List<ForgeItemEntry> entries = new List<ForgeItemEntry>();
        private readonly List<TextMeshProUGUI> currentCardLines = new List<TextMeshProUGUI>();
        private readonly List<TextMeshProUGUI> nextCardLines = new List<TextMeshProUGUI>();

        [SerializeField]
        private Transform rowsRoot;

        [SerializeField]
        private Transform currentCard;

        [SerializeField]
        private Transform nextCard;

        [SerializeField]
        private Button enhanceButton;

        [SerializeField]
        private TextMeshProUGUI enhanceButtonLabel;

        [SerializeField]
        private TextMeshProUGUI priceText;
        private Color priceBaseColor = Color.white;
        private Item selectedItem;
        private string lastResultMessage = string.Empty;

        private readonly Color selectedRowColor = new Color(0.42f, 0.30f, 0.08f, 0.92f);
        private readonly Color disabledPriceColor = new Color(1f, 0.20f, 0.27f, 1f);

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            priceBaseColor = priceText.color;
            CacheRows();
            CacheCardLines(currentCard, currentCardLines);
            CacheCardLines(nextCard, nextCardLines);
        }

        public override void Refresh()
        {
            if (!enabled)
            {
                return;
            }

            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                ClearPanel();
                return;
            }

            BuildEntries(run);
            EnsureRowCount(entries.Count);
            if (!entries.Exists(entry => entry.Item == selectedItem))
            {
                selectedItem = entries.Count > 0 ? entries[0].Item : null;
            }

            RefreshRows();
            RefreshDetail(run, data);
        }

        protected override void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnInventoryChanged -= Refresh;
                gsm.Events.OnEquipmentChanged -= Refresh;
                gsm.Events.OnGoldChanged -= HandleGoldChanged;
                gsm.Events.OnEnhanceResult -= HandleEnhanceResult;

                gsm.Events.OnInventoryChanged += Refresh;
                gsm.Events.OnEquipmentChanged += Refresh;
                gsm.Events.OnGoldChanged += HandleGoldChanged;
                gsm.Events.OnEnhanceResult += HandleEnhanceResult;
            }
        }

        protected override void UnsubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnInventoryChanged -= Refresh;
                gsm.Events.OnEquipmentChanged -= Refresh;
                gsm.Events.OnGoldChanged -= HandleGoldChanged;
                gsm.Events.OnEnhanceResult -= HandleEnhanceResult;
            }
        }

        private void HandleGoldChanged(int gold)
        {
            Refresh();
        }

        private void HandleEnhanceResult(EnhanceResult result)
        {
            if (result.Outcome == EnhanceOutcome.Success)
            {
                lastResultMessage = result.PityForced ? "PITY SUCCESS" : "SUCCESS";
            }
            else if (result.Outcome == EnhanceOutcome.Fail)
            {
                lastResultMessage = "FAILED";
            }
            else
            {
                lastResultMessage = "BLOCKED";
            }

            Refresh();
        }

        private void BuildEntries(GameRunState run)
        {
            entries.Clear();
            EquipmentSlots equipment = run?.Player?.Equipment;
            AddEntry(equipment?.Weapon, "Equipped Weapon");
            AddEntry(equipment?.ArmorBody, "Equipped Body");
            AddEntry(equipment?.ArmorArms, "Equipped Arms");
            AddEntry(equipment?.ArmorLegs, "Equipped Legs");

            List<Item> inventoryItems = run?.Player?.Inventory?.EquipItems;
            if (inventoryItems == null)
            {
                return;
            }

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                AddEntry(inventoryItems[i], "Inventory");
            }
        }

        private void AddEntry(Item item, string sourceLabel)
        {
            if (item?.Data == null || item.Data.Category != ItemCategory.Equipment)
            {
                return;
            }

            entries.Add(new ForgeItemEntry { Item = item, SourceLabel = sourceLabel });
        }

        private void RefreshRows()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                ForgeRowView row = rows[i];
                if (i >= entries.Count)
                {
                    row.Root.SetActive(false);
                    continue;
                }

                ForgeItemEntry entry = entries[i];
                row.Root.SetActive(true);
                if (row.Label != null)
                {
                    row.Label.text = BuildRowLabel(entry);
                }

                bool selected = entry.Item == selectedItem;
                if (row.Background != null)
                {
                    row.Background.color = selected ? selectedRowColor : row.BaseColor;
                }

                row.Button.onClick.RemoveAllListeners();
                Item capturedItem = entry.Item;
                row.Button.onClick.AddListener(() => SelectItem(capturedItem));
            }
        }

        private void SelectItem(Item item)
        {
            selectedItem = item;
            lastResultMessage = string.Empty;
            Refresh();
        }

        private void RefreshDetail(GameRunState run, DataManager data)
        {
            if (selectedItem == null)
            {
                SetCardLines(
                    currentCardLines,
                    "CURRENT",
                    "No equipment",
                    string.Empty,
                    string.Empty,
                    string.Empty
                );
                SetCardLines(
                    nextCardLines,
                    "NEXT",
                    "Select equipment",
                    string.Empty,
                    string.Empty,
                    string.Empty
                );
                ConfigureEnhanceButton(false, null);
                SetPriceText("-");
                return;
            }

            int cost = Forge.GetEnhanceCost(selectedItem, data);
            float rate = Forge.GetSuccessRate(selectedItem, data);
            int pityRemaining = Forge.GetPityRemaining(selectedItem, data);
            bool canEnhance = Forge.CanEnhance(selectedItem, run, data);

            SetCardLines(
                currentCardLines,
                "CURRENT +" + selectedItem.Enhancement,
                Loc.Get(selectedItem.Data.NameKey),
                BuildStatText(selectedItem.GetFinalMod()),
                "Fail Streak " + selectedItem.EnhanceFailStreak,
                BuildSourceLabel(selectedItem)
            );

            EquipmentStatMod nextMod = BuildNextMod(selectedItem);
            SetCardLines(
                nextCardLines,
                "NEXT +" + (selectedItem.Enhancement + 1),
                Loc.Get(selectedItem.Data.NameKey),
                BuildStatText(nextMod),
                "Success " + Mathf.RoundToInt(rate * 100f) + "%",
                BuildPityLine(pityRemaining, lastResultMessage)
            );

            SetPriceText(cost + " G", canEnhance);
            ConfigureEnhanceButton(canEnhance, () => Forge.TryEnhance(selectedItem, run, data));
        }

        private void ConfigureEnhanceButton(
            bool interactable,
            UnityEngine.Events.UnityAction action
        )
        {
            if (enhanceButton == null)
            {
                return;
            }

            enhanceButton.interactable = interactable;
            enhanceButton.onClick.RemoveAllListeners();
            if (interactable && action != null)
            {
                enhanceButton.onClick.AddListener(action);
            }

            if (enhanceButtonLabel != null)
            {
                enhanceButtonLabel.text = interactable ? "ENHANCE" : "LOCKED";
            }
        }

        private void SetPriceText(string text, bool enoughGold = true)
        {
            if (priceText == null)
            {
                return;
            }

            priceText.text = text;
            priceText.color = enoughGold ? priceBaseColor : disabledPriceColor;
        }

        private void ClearPanel()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].Root.SetActive(false);
            }

            SetCardLines(
                currentCardLines,
                "CURRENT",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty
            );
            SetCardLines(
                nextCardLines,
                "NEXT",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty
            );
            SetPriceText("-");
            ConfigureEnhanceButton(false, null);
        }

        private bool ValidateReferences()
        {
            bool valid =
                rowsRoot != null
                && currentCard != null
                && nextCard != null
                && enhanceButton != null
                && enhanceButtonLabel != null
                && priceText != null;
            if (!valid)
            {
                GameLog.LogError(
                    "[ForgeEnhanceController] 필수 UI 참조가 Inspector 에 직접 할당되어 있지 않습니다."
                );
            }

            return valid;
        }

        private void CacheRows()
        {
            rows.Clear();

            for (int i = 0; i < rowsRoot.childCount; i++)
            {
                rows.Add(CreateRowView(rowsRoot.GetChild(i)));
            }
        }

        private void EnsureRowCount(int count)
        {
            UIRowPool.EnsureCount(
                rows,
                rowsRoot,
                count,
                v => v.Root,
                clone => CreateRowView(clone.transform)
            );
        }

        private static ForgeRowView CreateRowView(Transform row)
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

            return new ForgeRowView
            {
                Root = row.gameObject,
                Button = button,
                Background = background,
                BaseColor = background.color,
                Label = label,
            };
        }

        private static void CacheCardLines(Transform card, List<TextMeshProUGUI> lines)
        {
            lines.Clear();
            if (card == null)
            {
                return;
            }

            TextMeshProUGUI[] texts = card.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                lines.Add(texts[i]);
            }
        }

        private static void SetCardLines(
            List<TextMeshProUGUI> lines,
            string line1,
            string line2,
            string line3,
            string line4,
            string line5
        )
        {
            string[] values = { line1, line2, line3, line4, line5 };
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].text = i < values.Length ? values[i] : string.Empty;
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
            )
            {
                GameLog.LogError(
                    "[ForgeEnhanceController] GameSystemManager / CurrentRun.Player.Inventory / Data.Items 참조가 없습니다."
                );
                return false;
            }

            run = gsm.CurrentRun;
            data = gsm.Data;
            return true;
        }

        private static string BuildRowLabel(ForgeItemEntry entry)
        {
            return Loc.Get(entry.Item.Data.NameKey)
                + " +"
                + entry.Item.Enhancement
                + "  "
                + entry.SourceLabel;
        }

        private string BuildSourceLabel(Item item)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Item == item)
                {
                    return entries[i].SourceLabel;
                }
            }

            return string.Empty;
        }

        private static EquipmentStatMod BuildNextMod(Item item)
        {
            int originalLevel = item.Enhancement;
            item.Enhancement = Mathf.Max(0, originalLevel) + 1;
            EquipmentStatMod mod = item.GetFinalMod();
            item.Enhancement = originalLevel;
            return mod;
        }

        private static string BuildStatText(EquipmentStatMod mod)
        {
            if (mod == null)
            {
                return "No stat modifier";
            }

            List<string> parts = new List<string>();
            AddPart(parts, "HP", mod.HP);
            AddPart(parts, "MP", mod.MP);
            AddPart(parts, "ATK", mod.ATK);
            AddPart(parts, "DEF", mod.DEF);
            AddPart(parts, "SPD", mod.SPD);
            return parts.Count > 0 ? string.Join(" / ", parts) : "No stat modifier";
        }

        private static void AddPart(List<string> parts, string label, int value)
        {
            if (value != 0)
            {
                parts.Add(label + " +" + value);
            }
        }

        private static string BuildPityLine(int pityRemaining, string resultMessage)
        {
            string pity = pityRemaining == 0 ? "Pity ready" : "Pity " + pityRemaining + " fails";
            return string.IsNullOrEmpty(resultMessage) ? pity : resultMessage + " / " + pity;
        }
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// Safe1 주점 RECRUITMENT 탭. 동료 목록 표시 및 모집 처리.
    /// Facility_TAVERN GameObject 에 직접 부착되고 Inspector 참조로 바인딩된다.
    /// </summary>
    public sealed class TavernRecruitmentController : UIEventPageBase
    {
        private sealed class CompanionRowView
        {
            public GameObject Root;
            public Button Button;
            public Graphic Background;
            public Color BaseColor;
            public Outline Border;
            public Image Portrait;
            public TextMeshProUGUI Name;
            public TextMeshProUGUI Rune;
            public TextMeshProUGUI Description;
            public TextMeshProUGUI Cost;
        }

        private readonly List<CompanionRowView> rows = new List<CompanionRowView>();
        private readonly List<CompanionData> companions = new List<CompanionData>();
        private readonly List<TextMeshProUGUI> detailLines = new List<TextMeshProUGUI>();

        [SerializeField]
        private Transform rowsRoot;

        [SerializeField]
        private Transform detailBody;
        private Transform detailStatGrid;
        private Button recruitButton;
        private TextMeshProUGUI recruitButtonLabel;
        private int selectedCompanionId;

        private static readonly Color RowBaseColor = new Color(0.09f, 0.09f, 0.09f, 0.96f);
        private static readonly Color RowSelectedColor = new Color(0.42f, 0.30f, 0.08f, 0.96f);
        private static readonly Color RowInactiveColor = new Color(0.06f, 0.06f, 0.065f, 0.84f);
        private static readonly Color RowBorderColor = new Color(0.70f, 0.62f, 0.42f, 1f);
        private static readonly Color RowSelectedBorderColor = new Color(1f, 0.76f, 0.18f, 1f);
        private static readonly Color RowInactiveBorderColor = new Color(0.30f, 0.30f, 0.32f, 1f);
        private static readonly Color RowTextColor = new Color(0.88f, 0.88f, 0.86f, 1f);

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            CacheRows();
            CacheDetailPanel();
        }

        public override void Refresh()
        {
            if (!enabled)
            {
                return;
            }

            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            BuildCompanionList(data);
            EnsureRowCount(companions.Count);
            RefreshRows(run, data);

            bool selectionValid =
                selectedCompanionId != 0 && companions.Exists(c => c.Id == selectedCompanionId);
            if (selectionValid)
            {
                ShowDetail(selectedCompanionId, run, data);
            }
            else
            {
                ClearDetail();
            }
        }

        protected override void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnGoldChanged -= HandleGoldChanged;
                gsm.Events.OnRosterChanged -= HandleRosterChanged;
                gsm.Events.OnGoldChanged += HandleGoldChanged;
                gsm.Events.OnRosterChanged += HandleRosterChanged;
            }
        }

        protected override void UnsubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnGoldChanged -= HandleGoldChanged;
                gsm.Events.OnRosterChanged -= HandleRosterChanged;
            }
        }

        private void HandleGoldChanged(int gold)
        {
            Refresh();
        }

        private void HandleRosterChanged(int companionId, bool joined)
        {
            Refresh();
        }

        private void BuildCompanionList(DataManager data)
        {
            companions.Clear();
            if (data?.Companions == null)
            {
                return;
            }

            foreach (CompanionData c in data.Companions.Values)
            {
                if (c != null)
                {
                    companions.Add(c);
                }
            }

            companions.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        private void RefreshRows(GameRunState run, DataManager data)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                CompanionRowView row = rows[i];
                if (i >= companions.Count)
                {
                    row.Root.SetActive(false);
                    continue;
                }

                CompanionData companion = companions[i];
                bool recruited =
                    run.Roster != null && Tavern.IsAlreadyRecruited(companion.Id, run.Roster);
                bool locked = run.CurrentFloor < companion.RequiredFloor;
                int price = Tavern.GetRecruitPrice(companion.Id, run, data);
                string runeLabel = GetStartingRuneName(companion.ClassId, data);
                string costLabel =
                    recruited ? Loc.Get("tavern_recruited")
                    : locked ? Loc.Get("tavern_locked")
                    : price + " G";

                row.Root.SetActive(true);
                SetText(row.Name, LocalizeKey(companion.NameKey));
                SetText(row.Rune, runeLabel);
                SetText(row.Description, LocalizeKey(companion.DescKey));
                SetText(row.Cost, costLabel);

                bool selected = companion.Id == selectedCompanionId;
                ApplyRowVisual(row, selected, !locked && !recruited);

                row.Button.onClick.RemoveAllListeners();
                int capturedId = companion.Id;
                row.Button.onClick.AddListener(() => HandleRowSelected(capturedId));
            }
        }

        private void HandleRowSelected(int companionId)
        {
            selectedCompanionId = companionId;
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            RefreshRows(run, data);
            ShowDetail(companionId, run, data);
        }

        private void ShowDetail(int companionId, GameRunState run, DataManager data)
        {
            if (!data.Companions.TryGetValue(companionId, out CompanionData companion))
            {
                ClearDetail();
                return;
            }

            selectedCompanionId = companionId;
            bool recruited =
                run.Roster != null && Tavern.IsAlreadyRecruited(companionId, run.Roster);
            bool locked = run.CurrentFloor < companion.RequiredFloor;
            int price = Tavern.GetRecruitPrice(companionId, run, data);
            string runeLabel = GetStartingRuneName(companion.ClassId, data);
            string costValue =
                recruited ? Loc.Get("tavern_recruited")
                : locked ? Loc.Get("tavern_locked")
                : price + " G";

            SetDetailLine(0, LocalizeKey(companion.NameKey));
            SetDetailLine(1, runeLabel);
            SetDetailLine(2, LocalizeKey(companion.DescKey));
            // lines[3] = "Cost:" label — static, skip
            SetDetailLine(4, costValue);

            UpdateStatGrid(companion.BaseStats);

            bool canRecruit = !recruited && !locked && Tavern.CanRecruit(companionId, run, data);
            ConfigureRecruitButton(
                canRecruit,
                () =>
                {
                    if (Tavern.TryRecruit(companionId, run, data))
                    {
                        Refresh();
                    }
                }
            );
        }

        private void ClearDetail()
        {
            for (int i = 0; i < detailLines.Count; i++)
            {
                detailLines[i].text = string.Empty;
            }

            UpdateStatGrid(null);
            ConfigureRecruitButton(false, null);
        }

        private void SetDetailLine(int index, string text)
        {
            if (index >= 0 && index < detailLines.Count)
            {
                detailLines[index].text = text;
            }
        }

        private void UpdateStatGrid(EquipmentStatMod stats)
        {
            if (detailStatGrid == null)
            {
                return;
            }

            UpdateStatChip(detailStatGrid, "HP", stats != null ? stats.HP : 0);
            UpdateStatChip(detailStatGrid, "MP", stats != null ? stats.MP : 0);
            UpdateStatChip(detailStatGrid, "ATK", stats != null ? stats.ATK : 0);
            UpdateStatChip(detailStatGrid, "DEF", stats != null ? stats.DEF : 0);
            UpdateStatChip(detailStatGrid, "SPD", stats != null ? stats.SPD : 0);
        }

        private static void UpdateStatChip(Transform grid, string statName, int value)
        {
            for (int i = 0; i < grid.childCount; i++)
            {
                Transform chip = grid.GetChild(i);
                if (chip.name.StartsWith("StatChip_" + statName, System.StringComparison.Ordinal))
                {
                    TextMeshProUGUI label = chip.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (label != null)
                    {
                        // 칩의 기존(로컬라이즈된) 라벨 첫 줄을 보존하고 값만 갱신한다.
                        // 예: "체력\n160" → "체력\n70". 라벨이 없으면 statName 사용.
                        string[] parts = label.text.Split('\n');
                        string head = parts.Length > 0 && !string.IsNullOrEmpty(parts[0]) ? parts[0] : statName;
                        label.text = head + "\n" + value;
                    }

                    return;
                }
            }
        }

        private void ConfigureRecruitButton(
            bool interactable,
            UnityEngine.Events.UnityAction action
        )
        {
            if (recruitButton == null)
            {
                return;
            }

            recruitButton.interactable = interactable;
            recruitButton.onClick.RemoveAllListeners();
            if (interactable && action != null)
            {
                recruitButton.onClick.AddListener(action);
            }

            if (recruitButtonLabel != null)
            {
                recruitButtonLabel.text = interactable
                    ? Loc.Get("tavern_recruit")
                    : Loc.Get("tavern_locked");
            }
        }

        private bool ValidateReferences()
        {
            bool valid = rowsRoot != null && detailBody != null;
            if (!valid)
            {
                GameLog.LogError(
                    "[TavernRecruitmentController] 필수 UI 참조가 Inspector 에 직접 할당되어 있지 않습니다."
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

        private static CompanionRowView CreateRowView(Transform row)
        {
            EnsureRowLayout(row);

            Graphic background = row.GetComponent<Graphic>();
            if (background == null)
            {
                Image img = row.gameObject.AddComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0.01f);
                img.raycastTarget = true;
                background = img;
            }

            background.raycastTarget = true;
            background.color = RowBaseColor;

            Outline border = row.GetComponent<Outline>();
            if (border == null)
            {
                border = row.gameObject.AddComponent<Outline>();
            }

            border.effectColor = RowBorderColor;
            border.effectDistance = new Vector2(1f, -1f);
            border.useGraphicAlpha = false;

            Button button = row.GetComponent<Button>();
            if (button == null)
            {
                button = row.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = background;

            return new CompanionRowView
            {
                Root = row.gameObject,
                Button = button,
                Background = background,
                BaseColor = RowBaseColor,
                Border = border,
                Portrait = FindChildImage(row, "Portrait"),
                Name = FindChildText(row, "Name"),
                Rune = FindChildText(row, "Rune"),
                Description = FindChildText(row, "Description"),
                Cost = FindChildText(row, "Cost"),
            };
        }

        private void CacheDetailPanel()
        {
            detailLines.Clear();
            detailStatGrid = null;
            recruitButton = null;
            recruitButtonLabel = null;

            TextMeshProUGUI[] allTexts = detailBody.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in allTexts)
            {
                if (text != null && text.name.StartsWith("Line_", System.StringComparison.Ordinal))
                {
                    detailLines.Add(text);
                }
            }

            // StatGrid 는 레이아웃 컴포넌트가 없을 수 있으므로 GridLayoutGroup 이 아닌 이름으로 탐색한다.
            detailStatGrid = FindDescendant(detailBody, "StatGrid");

            Button[] buttons = detailBody.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                recruitButton = buttons[0];
                recruitButtonLabel = recruitButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        private static string GetStartingRuneName(RuneClass cls, DataManager data)
        {
            int startId =
                cls == RuneClass.Dealer ? 1
                : cls == RuneClass.Tanker ? 2
                : cls == RuneClass.MagicDealer ? 3
                : 4;

            if (data?.Runes != null && data.Runes.TryGetValue(startId, out RuneData rune))
            {
                return LocalizeKey(rune.NameKey);
            }

            return cls.ToString();
        }

        private static void ApplyRowVisual(CompanionRowView row, bool selected, bool active)
        {
            if (row == null)
            {
                return;
            }

            Color background =
                selected ? RowSelectedColor
                : active ? RowBaseColor
                : RowInactiveColor;
            Color border =
                selected ? RowSelectedBorderColor
                : active ? RowBorderColor
                : RowInactiveBorderColor;
            Color text = active || selected ? RowTextColor : new Color(0.46f, 0.46f, 0.52f, 1f);

            if (row.Background != null)
            {
                row.Background.color = background;
            }

            if (row.Border != null)
            {
                row.Border.effectColor = border;
            }

            SetTextColor(row.Name, text);
            SetTextColor(row.Rune, text);
            SetTextColor(row.Description, text);
            SetTextColor(row.Cost, text);
        }

        private static void EnsureRowLayout(Transform row)
        {
            if (row == null)
            {
                return;
            }

            LayoutElement layout = row.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = row.gameObject.AddComponent<LayoutElement>();
            }

            if (row is RectTransform rect && rect.sizeDelta.y > 0f)
            {
                layout.minHeight = rect.sizeDelta.y;
                layout.preferredHeight = rect.sizeDelta.y;
            }

            layout.flexibleHeight = 0f;
        }

        private static void SetTextColor(TextMeshProUGUI label, Color color)
        {
            if (label != null)
            {
                label.color = color;
            }
        }

        private static string LocalizeKey(string key)
        {
            return string.IsNullOrEmpty(key) ? string.Empty : Loc.Get(key);
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                {
                    return t;
                }
            }

            return null;
        }

        private static bool TryGetRunData(out GameRunState run, out DataManager data)
        {
            run = null;
            data = null;
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun?.Player == null
                || gsm.Data?.Companions == null
            )
            {
                return false;
            }

            run = gsm.CurrentRun;
            data = gsm.Data;
            return true;
        }

        private static TextMeshProUGUI FindChildText(Transform root, string childName)
        {
            Transform child = root != null ? root.Find(childName) : null;
            return child != null ? child.GetComponentInChildren<TextMeshProUGUI>(true) : null;
        }

        private static Image FindChildImage(Transform root, string childName)
        {
            Transform child = root != null ? root.Find(childName) : null;
            return child != null ? child.GetComponent<Image>() : null;
        }

        private static void SetText(TextMeshProUGUI label, string text)
        {
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }
    }
}

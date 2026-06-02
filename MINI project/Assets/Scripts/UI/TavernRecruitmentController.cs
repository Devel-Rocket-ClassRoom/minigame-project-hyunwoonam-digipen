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
    public sealed class TavernRecruitmentController : MonoBehaviour
    {
        private sealed class CompanionRowView
        {
            public GameObject Root;
            public Button Button;
            public Graphic Background;
            public Color BaseColor;
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
        private Image detailPortrait;
        private Transform detailStatGrid;
        private Button recruitButton;
        private TextMeshProUGUI recruitButtonLabel;
        private int selectedCompanionId;

        private readonly Color selectedRowColor = new Color(0.42f, 0.30f, 0.08f, 0.92f);
        private readonly Color lockedTextColor = new Color(0.46f, 0.46f, 0.52f, 1f);

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

        private void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnGoldChanged -= HandleGoldChanged;
                gsm.Events.OnRosterChanged -= HandleRosterChanged;
                gsm.Events.OnGoldChanged += HandleGoldChanged;
                gsm.Events.OnRosterChanged += HandleRosterChanged;
            }
        }

        private void UnsubscribeEvents()
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
                    recruited ? "RECRUITED"
                    : locked ? "LOCKED"
                    : price + " G";

                row.Root.SetActive(true);
                SetText(row.Name, companion.NameKey);
                SetText(row.Rune, runeLabel);
                SetText(row.Description, companion.DescKey);
                SetText(row.Cost, costLabel);

                bool selected = companion.Id == selectedCompanionId;
                if (row.Background != null)
                {
                    row.Background.color = selected ? selectedRowColor : row.BaseColor;
                }

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
                recruited ? "RECRUITED"
                : locked ? "LOCKED"
                : price + " G";

            SetDetailLine(0, companion.NameKey);
            SetDetailLine(1, runeLabel);
            SetDetailLine(2, companion.DescKey);
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
                        label.text = statName + "\n" + value;
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
                recruitButtonLabel.text = interactable ? "RECRUIT" : "LOCKED";
            }
        }

        private bool ValidateReferences()
        {
            bool valid = rowsRoot != null && detailBody != null;
            if (!valid)
            {
                Debug.LogError(
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
            if (rowsRoot == null || rows.Count == 0)
            {
                return;
            }

            GameObject template = rows[0].Root;
            while (rows.Count < count)
            {
                GameObject clone = Instantiate(template, rowsRoot);
                clone.name = template.name + "_Generated_" + rows.Count;
                rows.Add(CreateRowView(clone.transform));
            }
        }

        private static CompanionRowView CreateRowView(Transform row)
        {
            Graphic background = row.GetComponent<Graphic>();
            if (background == null)
            {
                Image img = row.gameObject.AddComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0.01f);
                img.raycastTarget = true;
                background = img;
            }

            background.raycastTarget = true;

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
                BaseColor = background.color,
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
            detailPortrait = null;
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

            Image[] images = detailBody.GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image != null && image.name == "LargePortrait")
                {
                    detailPortrait = image;
                    break;
                }
            }

            GridLayoutGroup[] grids = detailBody.GetComponentsInChildren<GridLayoutGroup>(true);
            if (grids.Length > 0)
            {
                detailStatGrid = grids[0].transform;
            }

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
                return rune.NameKey;
            }

            return cls.ToString();
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

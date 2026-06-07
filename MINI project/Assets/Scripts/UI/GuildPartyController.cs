using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// Safe1 길드 PARTY 탭. 대기(Bench) 동료를 파티(Active)에 배치하거나 제외한다.
    /// Facility_GUILD GameObject 에 직접 부착되고 Inspector 참조로 바인딩된다.
    /// </summary>
    public sealed class GuildPartyController : UIEventPageBase
    {
        private sealed class SlotRowView
        {
            public GameObject Root;
            public Button Button;
            public Graphic Background;
            public Color BaseColor;
            public TextMeshProUGUI Label;
        }

        private readonly List<SlotRowView> rosterRows = new List<SlotRowView>();
        private readonly List<SlotRowView> partySlotRows = new List<SlotRowView>();
        private readonly List<TextMeshProUGUI> detailLines = new List<TextMeshProUGUI>();
        private Transform detailStatGrid;

        [SerializeField]
        private Transform rosterRowsRoot;

        [SerializeField]
        private Transform partyRowsRoot;

        [SerializeField]
        private Transform detailBody;

        [SerializeField]
        private TextMeshProUGUI rosterCountLabel;

        [SerializeField]
        private TextMeshProUGUI partyCountLabel;
        private Button assignButton;
        private TextMeshProUGUI assignButtonLabel;

        private int selectedCompanionId;
        private bool selectedFromBench;

        private readonly Color selectedRowColor = new Color(0.42f, 0.30f, 0.08f, 0.92f);

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            CacheRosterRows();
            CachePartySlotRows();
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

            CompanionRosterState roster = run.Roster ?? new CompanionRosterState();
            RefreshRosterRows(roster, data);
            RefreshPartySlotRows(roster, run, data);
            UpdateCountLabels(roster);

            bool selectionValid = IsSelectionValid(selectedCompanionId, roster);
            if (selectionValid)
            {
                ShowDetail(selectedCompanionId, run, data, roster);
            }
            else
            {
                selectedCompanionId = 0;
                ClearDetail();
            }
        }

        private bool ValidateReferences()
        {
            bool valid =
                rosterRowsRoot != null
                && partyRowsRoot != null
                && detailBody != null
                && rosterCountLabel != null
                && partyCountLabel != null;
            if (!valid)
            {
                GameLog.LogError(
                    "[GuildPartyController] 필수 UI 참조가 Inspector 에 직접 할당되어 있지 않습니다."
                );
            }

            return valid;
        }

        protected override void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnRosterChanged -= HandleRosterChanged;
                gsm.Events.OnRosterChanged += HandleRosterChanged;
            }
        }

        protected override void UnsubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnRosterChanged -= HandleRosterChanged;
            }
        }

        private void HandleRosterChanged(int companionId, bool joined)
        {
            Refresh();
        }

        private void RefreshRosterRows(CompanionRosterState roster, DataManager data)
        {
            List<CompanionInstance> bench = roster.Bench ?? new List<CompanionInstance>();
            EnsureRosterRowCount(bench.Count);

            for (int i = 0; i < rosterRows.Count; i++)
            {
                SlotRowView row = rosterRows[i];
                if (i >= bench.Count)
                {
                    row.Root.SetActive(false);
                    continue;
                }

                CompanionInstance inst = bench[i];
                row.Root.SetActive(true);
                SetText(row.Label, BuildCompanionLabel(inst, data));

                bool selected = selectedFromBench && inst.CompanionDataId == selectedCompanionId;
                SetRowColor(row, selected);

                row.Button.onClick.RemoveAllListeners();
                int capturedId = inst.CompanionDataId;
                row.Button.onClick.AddListener(() =>
                {
                    selectedCompanionId = capturedId;
                    selectedFromBench = true;
                    Refresh();
                });
            }
        }

        private void RefreshPartySlotRows(
            CompanionRosterState roster,
            GameRunState run,
            DataManager data
        )
        {
            List<CompanionInstance> active = roster.Active ?? new List<CompanionInstance>();

            // Slot 0 = Player (always fixed), slots 1-3 = companions
            for (int i = 0; i < partySlotRows.Count; i++)
            {
                SlotRowView row = partySlotRows[i];
                if (i == 0)
                {
                    SetText(row.Label, run.Player?.Name ?? "Player");
                    SetRowColor(row, false);
                    row.Button.onClick.RemoveAllListeners();
                    continue;
                }

                int companionSlot = i - 1;
                if (companionSlot < active.Count)
                {
                    CompanionInstance inst = active[companionSlot];
                    row.Root.SetActive(true);
                    SetText(row.Label, BuildCompanionLabel(inst, data));

                    bool selected =
                        !selectedFromBench && inst.CompanionDataId == selectedCompanionId;
                    SetRowColor(row, selected);

                    row.Button.onClick.RemoveAllListeners();
                    int capturedId = inst.CompanionDataId;
                    row.Button.onClick.AddListener(() =>
                    {
                        selectedCompanionId = capturedId;
                        selectedFromBench = false;
                        Refresh();
                    });
                }
                else
                {
                    row.Root.SetActive(true);
                    SetText(row.Label, "(Empty)");
                    SetRowColor(row, false);
                    row.Button.onClick.RemoveAllListeners();
                }
            }
        }

        private void UpdateCountLabels(CompanionRosterState roster)
        {
            int benchCount = roster.Bench?.Count ?? 0;
            int activeCount = roster.Active?.Count ?? 0;

            if (rosterCountLabel != null)
            {
                rosterCountLabel.text = benchCount.ToString();
            }

            if (partyCountLabel != null)
            {
                partyCountLabel.text = activeCount + "/3";
            }
        }

        private void ShowDetail(
            int companionId,
            GameRunState run,
            DataManager data,
            CompanionRosterState roster
        )
        {
            if (
                data?.Companions == null
                || !data.Companions.TryGetValue(companionId, out CompanionData companion)
            )
            {
                ClearDetail();
                return;
            }

            bool inBench = IsInBench(companionId, roster);
            bool inActive = IsInActive(companionId, roster);

            SetDetailLine(0, LocalizeKey(companion.NameKey));
            SetDetailLine(1, companion.ClassId.ToString());
            SetDetailLine(2, LocalizeKey(companion.DescKey));
            UpdateStatGrid(companion.BaseStats);

            if (inBench)
            {
                bool canAssign = roster.Active == null || roster.Active.Count < 3;
                ConfigureAssignButton(
                    "ASSIGN TO PARTY",
                    canAssign,
                    () =>
                    {
                        if (roster.Promote(companionId))
                        {
                            selectedCompanionId = 0;
                            SaveAndRaise(companionId, true);
                        }
                    }
                );
            }
            else if (inActive)
            {
                ConfigureAssignButton(
                    "REMOVE FROM PARTY",
                    true,
                    () =>
                    {
                        if (roster.Demote(companionId))
                        {
                            selectedCompanionId = 0;
                            SaveAndRaise(companionId, false);
                        }
                    }
                );
            }
            else
            {
                ConfigureAssignButton(string.Empty, false, null);
            }
        }

        private void ClearDetail()
        {
            for (int i = 0; i < detailLines.Count; i++)
            {
                detailLines[i].text = string.Empty;
            }

            UpdateStatGrid(null);
            ConfigureAssignButton(string.Empty, false, null);
        }

        private void SetDetailLine(int index, string text)
        {
            if (index >= 0 && index < detailLines.Count)
            {
                detailLines[index].text = text;
            }
        }

        private void ConfigureAssignButton(
            string label,
            bool interactable,
            UnityEngine.Events.UnityAction action
        )
        {
            if (assignButton == null)
            {
                return;
            }

            assignButton.gameObject.SetActive(!string.IsNullOrEmpty(label));
            assignButton.interactable = interactable && action != null;
            assignButton.onClick.RemoveAllListeners();
            if (interactable && action != null)
            {
                assignButton.onClick.AddListener(action);
            }

            if (assignButtonLabel != null)
            {
                assignButtonLabel.text = label ?? string.Empty;
            }
        }

        private static void SaveAndRaise(int companionId, bool joined)
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return;
            }

            gsm.Save?.SaveSnapshot();
            gsm.Events?.RaiseRosterChanged(companionId, joined);
        }

        private void CacheRosterRows()
        {
            rosterRows.Clear();

            for (int i = 0; i < rosterRowsRoot.childCount; i++)
            {
                rosterRows.Add(CreateSlotRowView(rosterRowsRoot.GetChild(i)));
            }
        }

        private void CachePartySlotRows()
        {
            partySlotRows.Clear();

            for (int i = 0; i < partyRowsRoot.childCount; i++)
            {
                partySlotRows.Add(CreateSlotRowView(partyRowsRoot.GetChild(i)));
            }
        }

        private void EnsureRosterRowCount(int count)
        {
            UIRowPool.EnsureCount(
                rosterRows,
                rosterRowsRoot,
                count,
                v => v.Root,
                clone => CreateSlotRowView(clone.transform)
            );
        }

        private static SlotRowView CreateSlotRowView(Transform row)
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

            TextMeshProUGUI label = row.GetComponentInChildren<TextMeshProUGUI>(true);
            return new SlotRowView
            {
                Root = row.gameObject,
                Button = button,
                Background = background,
                BaseColor = background.color,
                Label = label,
            };
        }

        private void CacheDetailPanel()
        {
            detailLines.Clear();
            detailStatGrid = null;
            assignButton = null;
            assignButtonLabel = null;

            TextMeshProUGUI[] allTexts = detailBody.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in allTexts)
            {
                if (text != null && text.name.StartsWith("Line_", System.StringComparison.Ordinal))
                {
                    detailLines.Add(text);
                }
            }

            // StatGrid 는 레이아웃 컴포넌트가 없을 수 있으므로 이름으로 탐색한다.
            detailStatGrid = FindDescendant(detailBody, "StatGrid");

            Button[] buttons = detailBody.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                assignButton = buttons[0];
                assignButtonLabel = assignButton.GetComponentInChildren<TextMeshProUGUI>(true);
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
                        // 칩의 기존(로컬라이즈된) 라벨 첫 줄 보존, 값만 갱신.
                        string[] parts = label.text.Split('\n');
                        string head = parts.Length > 0 && !string.IsNullOrEmpty(parts[0]) ? parts[0] : statName;
                        label.text = head + "\n" + value;
                    }

                    return;
                }
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

        private static string BuildCompanionLabel(CompanionInstance inst, DataManager data)
        {
            if (inst == null)
            {
                return string.Empty;
            }

            string name = inst.CompanionDataId.ToString();
            if (
                data?.Companions != null
                && data.Companions.TryGetValue(inst.CompanionDataId, out CompanionData cd)
            )
            {
                name = LocalizeKey(cd.NameKey);
            }

            return name + "  Lv." + inst.Level;
        }

        private static bool IsInBench(int companionId, CompanionRosterState roster)
        {
            if (roster?.Bench == null)
            {
                return false;
            }

            for (int i = 0; i < roster.Bench.Count; i++)
            {
                if (roster.Bench[i]?.CompanionDataId == companionId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsInActive(int companionId, CompanionRosterState roster)
        {
            if (roster?.Active == null)
            {
                return false;
            }

            for (int i = 0; i < roster.Active.Count; i++)
            {
                if (roster.Active[i]?.CompanionDataId == companionId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSelectionValid(int companionId, CompanionRosterState roster)
        {
            if (companionId == 0)
            {
                return false;
            }

            return IsInBench(companionId, roster) || IsInActive(companionId, roster);
        }

        private void SetRowColor(SlotRowView row, bool selected)
        {
            if (row?.Background != null)
            {
                row.Background.color = selected ? selectedRowColor : row.BaseColor;
            }
        }

        private static bool TryGetRunData(out GameRunState run, out DataManager data)
        {
            run = null;
            data = null;
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun?.Player == null
                || gsm.Data == null
            )
            {
                return false;
            }

            run = gsm.CurrentRun;
            data = gsm.Data;
            return true;
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

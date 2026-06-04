using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// Safe1 길드 COMPANIONS 탭. 현재 파티와 선택한 파티원의 읽기 전용 룬 트리를 표시한다.
    /// </summary>
    public sealed class GuildCompanionsController : MonoBehaviour
    {
        private const int PartySlotCount = 4;

        [Header("Party Rows")]
        [SerializeField]
        private Button[] partyRowButtons = new Button[PartySlotCount];

        [SerializeField]
        private TMP_Text[] partyRowLabels = new TMP_Text[PartySlotCount];

        [SerializeField]
        private TMP_Text partyCountLabel;

        [Header("Status")]
        [SerializeField]
        private TMP_Text levelLabel;

        [SerializeField]
        private TMP_Text expLabel;

        [SerializeField]
        private TMP_Text runeLabel;

        [SerializeField]
        private TMP_Text descriptionLabel;

        [SerializeField]
        private Button openRuneTreeButton;

        [Header("Rune Tree")]
        [SerializeField]
        private GameObject runeTreePanel;

        [SerializeField]
        private TMP_Text runeTreeNameLabel;

        [SerializeField]
        private RuneTreeView runeTreeView;

        [SerializeField]
        private Color selectedRowColor = new Color(0.42f, 0.30f, 0.08f, 0.92f);

        private readonly Color[] rowBaseColors = new Color[PartySlotCount];
        private int selectedSlotIndex;

        public bool IsRuneTreeOpen => runeTreePanel != null && runeTreePanel.activeSelf;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            CacheRowColors();
            WireButtons();
            CloseRuneTree();
        }

        private void OnEnable()
        {
            SubscribeEvents();
            Refresh();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            CloseRuneTree();
        }

        public void Refresh()
        {
            if (!enabled || !TryGetRunData(out GameRunState run, out DataManager data))
            {
                ClearRowsAndStatus();
                return;
            }

            int occupiedCount = 1 + Mathf.Min(3, run.Roster?.Active?.Count ?? 0);
            if (selectedSlotIndex < 0 || selectedSlotIndex >= occupiedCount)
            {
                selectedSlotIndex = 0;
            }

            for (int i = 0; i < PartySlotCount; i++)
            {
                bool occupied = i < occupiedCount;
                partyRowButtons[i].interactable = occupied;
                partyRowLabels[i].text = occupied
                    ? BuildPartyRowLabel(i, run, data)
                    : "SLOT " + (i + 1) + " EMPTY";
                SetRowSelected(i, occupied && i == selectedSlotIndex);
            }

            partyCountLabel.text = occupiedCount + " / " + PartySlotCount;
            ShowSelectedStatus(run, data);
        }

        public void SelectSlot(int slotIndex)
        {
            if (
                slotIndex < 0
                || slotIndex >= PartySlotCount
                || !TryGetRunData(out GameRunState run, out DataManager data)
                || !IsOccupiedSlot(slotIndex, run)
            )
            {
                return;
            }

            selectedSlotIndex = slotIndex;
            CloseRuneTree();
            Refresh();
        }

        public void OpenRuneTree()
        {
            if (
                !TryGetRunData(out GameRunState run, out DataManager data)
                || !TryBuildSelectedRuneView(
                    selectedSlotIndex,
                    run,
                    data,
                    out string memberName,
                    out PlayerRuneState runeState
                )
            )
            {
                return;
            }

            runeTreeView.Bind(runeState, RuneTreeView.Mode.ViewUnlock, true);
            runeTreeNameLabel.text = "Name : " + memberName;
            runeTreePanel.SetActive(true);
        }

        public void CloseRuneTree()
        {
            if (runeTreePanel != null)
            {
                runeTreePanel.SetActive(false);
            }
        }

        public bool TryCloseRuneTree()
        {
            if (!IsRuneTreeOpen)
            {
                return false;
            }

            CloseRuneTree();
            return true;
        }

        private bool ValidateReferences()
        {
            bool valid =
                partyRowButtons != null
                && partyRowButtons.Length == PartySlotCount
                && partyRowLabels != null
                && partyRowLabels.Length == PartySlotCount
                && partyCountLabel != null
                && levelLabel != null
                && expLabel != null
                && runeLabel != null
                && descriptionLabel != null
                && openRuneTreeButton != null
                && runeTreePanel != null
                && runeTreeNameLabel != null
                && runeTreeView != null;

            for (int i = 0; i < PartySlotCount && valid; i++)
            {
                valid = partyRowButtons[i] != null && partyRowLabels[i] != null;
            }

            if (!valid)
            {
                Debug.LogError(
                    "[GuildCompanionsController] 필수 UI 참조가 Inspector 에 직접 할당되어 있지 않습니다."
                );
            }

            return valid;
        }

        private void CacheRowColors()
        {
            for (int i = 0; i < PartySlotCount; i++)
            {
                if (partyRowButtons[i].targetGraphic != null)
                {
                    partyRowButtons[i].targetGraphic.raycastTarget = true;
                }

                rowBaseColors[i] =
                    partyRowButtons[i].targetGraphic != null
                        ? partyRowButtons[i].targetGraphic.color
                        : Color.white;
            }
        }

        private void WireButtons()
        {
            for (int i = 0; i < PartySlotCount; i++)
            {
                int capturedSlot = i;
                partyRowButtons[i].onClick.RemoveAllListeners();
                partyRowButtons[i].onClick.AddListener(() => SelectSlot(capturedSlot));
            }

            openRuneTreeButton.onClick.RemoveAllListeners();
            openRuneTreeButton.onClick.AddListener(OpenRuneTree);
        }

        private void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnRosterChanged -= HandleRosterChanged;
                gsm.Events.OnRosterChanged += HandleRosterChanged;
            }
        }

        private void UnsubscribeEvents()
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

        private void ShowSelectedStatus(GameRunState run, DataManager data)
        {
            if (selectedSlotIndex == 0)
            {
                PlayerState player = run.Player;
                SetStatus(
                    player.Level,
                    player.Exp,
                    player.Rune?.ClassId ?? player.StartingClass,
                    "PLAYER",
                    player.Rune != null
                );
                return;
            }

            CompanionInstance companion = GetActiveCompanion(run, selectedSlotIndex - 1);
            if (companion == null)
            {
                ClearStatus();
                return;
            }

            CompanionData companionData = FindCompanionData(companion, data);
            RuneClass runeClass =
                companion.Rune != null
                    ? companion.Rune.ClassId
                    : companionData?.ClassId ?? RuneClass.None;
            bool hasRuneTree =
                runeClass != RuneClass.None && data?.Runes != null && data.Runes.Count > 0;

            SetStatus(
                companion.Level,
                companion.Exp,
                runeClass,
                companionData?.DescKey ?? string.Empty,
                hasRuneTree
            );
        }

        private void SetStatus(
            int level,
            int exp,
            RuneClass runeClass,
            string description,
            bool canOpenRuneTree
        )
        {
            int requiredExp = 0;
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                requiredExp = RunProgression.RequiredExpForLevel(gsm.Data, level);
            }

            levelLabel.text = "Level: " + Mathf.Max(1, level);
            expLabel.text = "EXP: " + Mathf.Max(0, exp) + " / " + Mathf.Max(0, requiredExp);
            runeLabel.text = "Rune: " + runeClass;
            descriptionLabel.text = "Description: " + (description ?? string.Empty);
            openRuneTreeButton.interactable = canOpenRuneTree;
        }

        private void ClearRowsAndStatus()
        {
            for (int i = 0; i < PartySlotCount; i++)
            {
                if (partyRowButtons != null && i < partyRowButtons.Length && partyRowButtons[i] != null)
                {
                    partyRowButtons[i].interactable = false;
                }

                if (partyRowLabels != null && i < partyRowLabels.Length && partyRowLabels[i] != null)
                {
                    partyRowLabels[i].text = "SLOT " + (i + 1) + " EMPTY";
                }
            }

            if (partyCountLabel != null)
            {
                partyCountLabel.text = "0 / " + PartySlotCount;
            }

            ClearStatus();
        }

        private void ClearStatus()
        {
            SetText(levelLabel, string.Empty);
            SetText(expLabel, string.Empty);
            SetText(runeLabel, string.Empty);
            SetText(descriptionLabel, string.Empty);
            if (openRuneTreeButton != null)
            {
                openRuneTreeButton.interactable = false;
            }
        }

        private void SetRowSelected(int index, bool selected)
        {
            Graphic graphic = partyRowButtons[index].targetGraphic;
            if (graphic != null)
            {
                graphic.color = selected ? selectedRowColor : rowBaseColors[index];
            }
        }

        private static bool IsOccupiedSlot(int slotIndex, GameRunState run)
        {
            return slotIndex == 0 || GetActiveCompanion(run, slotIndex - 1) != null;
        }

        private static string BuildPartyRowLabel(int slotIndex, GameRunState run, DataManager data)
        {
            if (slotIndex == 0)
            {
                return "SLOT 1  " + (run.Player?.Name ?? "Player") + "  LV." + run.Player.Level;
            }

            CompanionInstance companion = GetActiveCompanion(run, slotIndex - 1);
            string name = ResolveCompanionName(companion, data);
            return "SLOT "
                + (slotIndex + 1)
                + "  "
                + name
                + "  LV."
                + Mathf.Max(1, companion?.Level ?? 1);
        }

        private static bool TryBuildSelectedRuneView(
            int slotIndex,
            GameRunState run,
            DataManager data,
            out string memberName,
            out PlayerRuneState runeState
        )
        {
            memberName = string.Empty;
            runeState = null;

            if (slotIndex == 0)
            {
                memberName = run.Player?.Name ?? "Player";
                runeState = run.Player?.Rune;
                return runeState?.Tree != null;
            }

            CompanionInstance companion = GetActiveCompanion(run, slotIndex - 1);
            if (companion == null)
            {
                return false;
            }

            CompanionData companionData = FindCompanionData(companion, data);
            memberName = ResolveCompanionName(companion, data);
            runeState = BuildCompanionRuneView(companion, companionData, data);
            return runeState?.Tree != null;
        }

        private static PlayerRuneState BuildCompanionRuneView(
            CompanionInstance companion,
            CompanionData companionData,
            DataManager data
        )
        {
            RuneClass runeClass =
                companion?.Rune != null
                    ? companion.Rune.ClassId
                    : companionData?.ClassId ?? RuneClass.None;
            if (runeClass == RuneClass.None || data?.Runes == null)
            {
                return null;
            }

            RuneTree tree = RuneTree.BuildFromData(runeClass, data.Runes.Values);
            var state = new PlayerRuneState
            {
                ClassId = runeClass,
                Tree = tree,
                RunePoints = 0,
                UnlockedIds = new HashSet<int>(),
                InvestedPointsByNode = new Dictionary<int, int>(),
            };

            CompanionRuneState source = companion?.Rune;
            if (source?.FixedSequence != null)
            {
                int unlockedCount = Mathf.Clamp(
                    source.UnlockedCount,
                    0,
                    source.FixedSequence.Count
                );
                for (int i = 0; i < unlockedCount; i++)
                {
                    int nodeId = source.FixedSequence[i];
                    if (!tree.AllNodes.TryGetValue(nodeId, out RuneNode node))
                    {
                        continue;
                    }

                    state.UnlockedIds.Add(nodeId);
                    int invested = state.InvestedPointsByNode.TryGetValue(
                        nodeId,
                        out int currentInvested
                    )
                        ? currentInvested
                        : 0;
                    state.InvestedPointsByNode[nodeId] = Mathf.Min(
                        node.RequiredPoints,
                        invested + 1
                    );
                }
            }

            state.UnlockStarter();
            state.SyncTreeStateFromProgress();
            return state;
        }

        private static CompanionInstance GetActiveCompanion(GameRunState run, int activeIndex)
        {
            if (
                run?.Roster?.Active == null
                || activeIndex < 0
                || activeIndex >= run.Roster.Active.Count
            )
            {
                return null;
            }

            return run.Roster.Active[activeIndex];
        }

        private static CompanionData FindCompanionData(CompanionInstance companion, DataManager data)
        {
            if (
                companion == null
                || data?.Companions == null
                || !data.Companions.TryGetValue(companion.CompanionDataId, out CompanionData result)
            )
            {
                return null;
            }

            return result;
        }

        private static string ResolveCompanionName(CompanionInstance companion, DataManager data)
        {
            CompanionData companionData = FindCompanionData(companion, data);
            return companionData != null
                ? companionData.NameKey
                : "Companion " + (companion?.CompanionDataId ?? 0);
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

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = value ?? string.Empty;
            }
        }
    }
}

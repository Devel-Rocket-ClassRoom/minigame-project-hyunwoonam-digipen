using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    public sealed class ShrineRuneController : MonoBehaviour
    {
        private enum ShrineTab
        {
            RuneTree = 0,
            Change = 1,
            Reset = 2,
        }

        private sealed class ChangeClassOption
        {
            public RuneClass ClassId;
            public GameObject Root;
            public Button Button;
            public Graphic Background;
            public TextMeshProUGUI Title;
            public TextMeshProUGUI Tag;
            public TextMeshProUGUI Role;
            public TextMeshProUGUI Desc;
            public TextMeshProUGUI Bonus;
            public Color BaseColor;
        }

        private static readonly RuneClass[] ChangeClasses =
        {
            RuneClass.Dealer,
            RuneClass.Tanker,
            RuneClass.MagicDealer,
            RuneClass.Supporter,
        };

        [Header("Tabs")]
        [SerializeField]
        private Button runeTreeTabButton;

        [SerializeField]
        private Button changeTabButton;

        [SerializeField]
        private Button resetTabButton;

        [Header("Content")]
        [SerializeField]
        private GameObject runeTreeContent;

        [SerializeField]
        private GameObject changeContent;

        [SerializeField]
        private GameObject resetContent;

        [Header("Rune Tree")]
        [SerializeField]
        private RuneTreeView runeTreeView;

        [SerializeField]
        private TMP_Text runePointSummaryLabel;

        [Header("Change")]
        [SerializeField]
        private Transform changeCardGrid;

        [SerializeField]
        private TMP_Text changeTitleLabel;

        [SerializeField]
        private TMP_Text changeRoleLabel;

        [SerializeField]
        private TMP_Text changeDescLabel;

        [SerializeField]
        private TMP_Text changeValueLabel;

        [SerializeField]
        private Button confirmChangeButton;

        [SerializeField]
        private TMP_Text confirmChangeButtonLabel;

        [Header("Reset")]
        [SerializeField]
        private TMP_Text resetRefundLabel;

        [SerializeField]
        private TMP_Text resetDescriptionLabel;

        [SerializeField]
        private TMP_Text resetCostLabel;

        [SerializeField]
        private Button resetButton;

        [SerializeField]
        private TMP_Text resetButtonLabel;

        private readonly List<ChangeClassOption> changeOptions = new List<ChangeClassOption>();
        private ShrineTab currentTab;
        private RuneClass selectedClass;
        private bool initialized;

        private Color activeText = new Color(0.94f, 0.94f, 0.94f, 1f);
        private Color inactiveText = new Color(0.46f, 0.46f, 0.52f, 1f);
        private Color selectedCardColor = new Color(0.42f, 0.30f, 0.08f, 0.92f);

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Initialize()
        {
            ResolveReferences();
            CacheChangeOptions();
            WireButtons();
            initialized = true;
            ShowTab((int)ShrineTab.RuneTree);
        }

        public void ShowTab(int index)
        {
            if (!initialized)
            {
                ResolveReferences();
                CacheChangeOptions();
                WireButtons();
                initialized = true;
            }

            currentTab = (ShrineTab)Mathf.Clamp(index, 0, 2);
            SetContentActive(runeTreeContent, currentTab == ShrineTab.RuneTree);
            SetContentActive(changeContent, currentTab == ShrineTab.Change);
            SetContentActive(resetContent, currentTab == ShrineTab.Reset);

            SetTabVisual(runeTreeTabButton, currentTab == ShrineTab.RuneTree);
            SetTabVisual(changeTabButton, currentTab == ShrineTab.Change);
            SetTabVisual(resetTabButton, currentTab == ShrineTab.Reset);
            Refresh();
        }

        public void Refresh()
        {
            if (!initialized)
            {
                return;
            }

            RefreshRunePointSummary();
            switch (currentTab)
            {
                case ShrineTab.RuneTree:
                    RefreshRuneTree();
                    break;
                case ShrineTab.Change:
                    RefreshChange();
                    break;
                case ShrineTab.Reset:
                    RefreshReset();
                    break;
            }
        }

        public static bool TryChangeRuneClass(
            GameRunState run,
            DataManager data,
            RuneClass newClass
        )
        {
            if (
                !TryGetTransactionInputs(
                    run,
                    data,
                    out PlayerRuneState rune,
                    out BalanceData balance
                )
                || newClass == RuneClass.None
                || newClass == rune.ClassId
            )
            {
                return false;
            }

            int cost = Mathf.Max(0, balance.RuneClassChangeCostGold);
            if (run.Gold < cost)
            {
                return false;
            }

            if (!rune.ChangeRuneClass(newClass, balance, data.Runes.Values))
            {
                return false;
            }

            run.Gold -= cost;
            run.Player.StartingClass = newClass;
            return true;
        }

        public static bool TryResetRuneTree(GameRunState run, DataManager data)
        {
            if (
                !TryGetTransactionInputs(
                    run,
                    data,
                    out PlayerRuneState rune,
                    out BalanceData balance
                )
            )
            {
                return false;
            }

            int cost = Mathf.Max(0, balance.RuneResetCostGold);
            if (run.Gold < cost || !HasResettableInvestment(rune))
            {
                return false;
            }

            run.Gold -= cost;
            rune.ResetTree(balance);
            return true;
        }

        private void RefreshRuneTree()
        {
            if (!TryGetRuneInputs(out _, out _, out _, out PlayerRuneState rune))
            {
                runeTreeView?.Bind(null, RuneTreeView.Mode.Shrine);
                return;
            }

            runeTreeView?.Bind(rune, RuneTreeView.Mode.Shrine);
        }

        private void RefreshChange()
        {
            if (
                !TryGetRuneInputs(
                    out _,
                    out GameRunState run,
                    out DataManager data,
                    out PlayerRuneState rune
                )
            )
            {
                SetChangeInteractable(false);
                return;
            }

            RuneClass current = rune.ClassId;
            if (selectedClass == RuneClass.None || selectedClass == current)
            {
                selectedClass = FirstChangeCandidate(current);
            }

            for (int i = 0; i < changeOptions.Count; i++)
            {
                ChangeClassOption option = changeOptions[i];
                bool visible = option.ClassId != current;
                if (option.Root != null)
                {
                    option.Root.SetActive(visible);
                }

                SetClassOption(option, option.ClassId == selectedClass);
            }

            ShowSelectedChangeDetail(run, data, current);
        }

        private void RefreshReset()
        {
            if (
                !TryGetRuneInputs(
                    out _,
                    out GameRunState run,
                    out DataManager data,
                    out PlayerRuneState rune
                )
            )
            {
                SetResetInteractable(false, "UNAVAILABLE");
                return;
            }

            int cost = Mathf.Max(0, data.Balance.RuneResetCostGold);
            int refund = PreviewResetRefund(rune, data.Balance);
            bool hasInvestedNode = HasResettableInvestment(rune);
            bool enoughGold = run.Gold >= cost;

            SetText(resetRefundLabel, "RUNE POINT REFUND +" + refund);
            SetText(
                resetDescriptionLabel,
                "Current rune tree progress will be reset. Root node remains unlocked."
            );
            SetText(resetCostLabel, cost + " Gold");

            if (!hasInvestedNode)
            {
                SetResetInteractable(false, "NO INVESTED NODE");
            }
            else if (!enoughGold)
            {
                SetResetInteractable(false, "NOT ENOUGH GOLD");
            }
            else
            {
                SetResetInteractable(true, "RESET RUNE TREE");
            }
        }

        private void HandleChangeConfirmed()
        {
            if (
                !TryGetRuneInputs(
                    out GameSystemManager gsm,
                    out GameRunState run,
                    out DataManager data,
                    out PlayerRuneState rune
                )
                || selectedClass == RuneClass.None
                || selectedClass == rune.ClassId
            )
            {
                return;
            }

            RuneClass changedClass = selectedClass;
            if (!TryChangeRuneClass(run, data, changedClass))
            {
                RefreshChange();
                return;
            }

            if (gsm.ActivePlayer != null && ReferenceEquals(gsm.ActivePlayer.Rune, rune))
            {
                gsm.ActivePlayer.StartingClass = changedClass;
            }

            gsm.Events?.RaiseGoldChanged(run.Gold);
            RuneRuntimeApplier.ApplyToCurrentPlayer(rune);
            gsm.Save?.SaveSnapshot();
            RefreshRuneTree();
            RefreshChange();
        }

        private void HandleResetConfirmed()
        {
            if (
                !TryGetRuneInputs(
                    out GameSystemManager gsm,
                    out GameRunState run,
                    out DataManager data,
                    out PlayerRuneState rune
                )
            )
            {
                return;
            }

            if (!TryResetRuneTree(run, data))
            {
                RefreshReset();
                return;
            }

            gsm.Events?.RaiseGoldChanged(run.Gold);
            RuneRuntimeApplier.ApplyToCurrentPlayer(rune);
            gsm.Save?.SaveSnapshot();
            RefreshRuneTree();
            RefreshReset();
        }

        private bool TryGetRuneInputs(
            out GameSystemManager gsm,
            out GameRunState run,
            out DataManager data,
            out PlayerRuneState rune
        )
        {
            gsm = null;
            run = null;
            data = null;
            rune = null;

            if (
                !GameSystemManager.TryGetInstance(out gsm)
                || gsm.CurrentRun?.Player?.Rune == null
                || gsm.Data?.Runes == null
                || gsm.Data.Balance == null
            )
            {
                Debug.LogError(
                    "[ShrineRuneController] GameSystemManager / CurrentRun.Player.Rune / Data.Runes / Balance 참조가 없습니다."
                );
                return false;
            }

            run = gsm.CurrentRun;
            data = gsm.Data;
            rune = run.Player.Rune;
            EnsureRuneTree(rune, data);
            return rune.ClassId != RuneClass.None && rune.Tree?.AllNodes != null;
        }

        private static void EnsureRuneTree(PlayerRuneState rune, DataManager data)
        {
            if (rune == null || rune.ClassId == RuneClass.None || data?.Runes == null)
            {
                return;
            }

            if (rune.Tree?.AllNodes == null)
            {
                rune.Tree = RuneTree.BuildFromData(rune.ClassId, data.Runes.Values);
            }

            if (rune.UnlockedIds == null)
            {
                rune.UnlockedIds = new HashSet<int>();
            }

            if (rune.InvestedPointsByNode == null)
            {
                rune.InvestedPointsByNode = new Dictionary<int, int>();
            }

            if (rune.Tree?.AllNodes == null)
            {
                return;
            }

            rune.SyncTreeStateFromProgress();
        }

        private void ResolveReferences()
        {
            Transform tabBar = transform.Find("TabBar");
            runeTreeTabButton =
                runeTreeTabButton != null
                    ? runeTreeTabButton
                    : tabBar?.Find("RUNE TREETab")?.GetComponent<Button>();
            changeTabButton =
                changeTabButton != null
                    ? changeTabButton
                    : tabBar?.Find("CHANGETab")?.GetComponent<Button>();
            resetTabButton =
                resetTabButton != null
                    ? resetTabButton
                    : tabBar?.Find("RESETTab")?.GetComponent<Button>();

            Transform contentArea = transform.Find("ContentArea");
            runeTreeContent =
                runeTreeContent != null
                    ? runeTreeContent
                    : contentArea?.Find("RuneTreeContent")?.gameObject;
            changeContent =
                changeContent != null
                    ? changeContent
                    : contentArea?.Find("ChangeContent")?.gameObject;
            resetContent =
                resetContent != null ? resetContent : contentArea?.Find("ResetContent")?.gameObject;

            runeTreeView =
                runeTreeView != null ? runeTreeView : runeTreeContent?.GetComponent<RuneTreeView>();
            runePointSummaryLabel =
                runePointSummaryLabel != null
                    ? runePointSummaryLabel
                    : transform.Find("Header/RunePointSummary")?.GetComponent<TMP_Text>();

            Transform changeRoot = changeContent != null ? changeContent.transform : null;
            changeCardGrid = changeCardGrid != null ? changeCardGrid : changeRoot?.Find("CardGrid");
            Transform changeDetail = changeRoot?.Find("ChangeDetail");
            changeTitleLabel =
                changeTitleLabel != null
                    ? changeTitleLabel
                    : changeDetail?.Find("Title")?.GetComponent<TMP_Text>();
            changeRoleLabel =
                changeRoleLabel != null
                    ? changeRoleLabel
                    : changeDetail?.Find("Role")?.GetComponent<TMP_Text>();
            changeDescLabel =
                changeDescLabel != null
                    ? changeDescLabel
                    : changeDetail?.Find("Desc")?.GetComponent<TMP_Text>();
            changeValueLabel =
                changeValueLabel != null
                    ? changeValueLabel
                    : changeDetail?.Find("Value")?.GetComponent<TMP_Text>();
            Transform confirm = changeDetail?.Find("CONFIRM RUNEBtn");
            confirmChangeButton =
                confirmChangeButton != null ? confirmChangeButton : confirm?.GetComponent<Button>();
            confirmChangeButtonLabel =
                confirmChangeButtonLabel != null
                    ? confirmChangeButtonLabel
                    : confirm?.Find("Lbl")?.GetComponent<TMP_Text>();

            Transform resetRoot = resetContent != null ? resetContent.transform : null;
            resetRefundLabel =
                resetRefundLabel != null
                    ? resetRefundLabel
                    : resetRoot?.Find("Overview/Refund")?.GetComponent<TMP_Text>();
            resetDescriptionLabel =
                resetDescriptionLabel != null
                    ? resetDescriptionLabel
                    : resetRoot?.Find("Overview/Expl")?.GetComponent<TMP_Text>();
            resetCostLabel =
                resetCostLabel != null
                    ? resetCostLabel
                    : resetRoot?.Find("Warning/Reset CostRow/R")?.GetComponent<TMP_Text>();
            Transform reset = resetRoot?.Find("Warning/RESET RUNE TREEBtn");
            resetButton = resetButton != null ? resetButton : reset?.GetComponent<Button>();
            resetButtonLabel =
                resetButtonLabel != null
                    ? resetButtonLabel
                    : reset?.Find("Lbl")?.GetComponent<TMP_Text>();
        }

        private void CacheChangeOptions()
        {
            changeOptions.Clear();
            if (changeCardGrid == null)
            {
                return;
            }

            int count = Mathf.Min(ChangeClasses.Length, changeCardGrid.childCount);
            for (int i = 0; i < count; i++)
            {
                Transform card = changeCardGrid.GetChild(i);
                Button button = card.GetComponent<Button>();
                if (button == null)
                {
                    button = card.gameObject.AddComponent<Button>();
                }

                Graphic background = card.GetComponent<Graphic>();
                ChangeClassOption option = new ChangeClassOption
                {
                    ClassId = ChangeClasses[i],
                    Root = card.gameObject,
                    Button = button,
                    Background = background,
                    BaseColor = background != null ? background.color : Color.white,
                    Title = card.Find("Content/Title")?.GetComponent<TextMeshProUGUI>(),
                    Tag = card.Find("Content/Tag")?.GetComponent<TextMeshProUGUI>(),
                    Role = card.Find("Content/Role")?.GetComponent<TextMeshProUGUI>(),
                    Desc = card.Find("Content/Desc")?.GetComponent<TextMeshProUGUI>(),
                    Bonus = card.Find("Content/Bonus")?.GetComponent<TextMeshProUGUI>(),
                };

                changeOptions.Add(option);
            }
        }

        private void WireButtons()
        {
            WireTab(runeTreeTabButton, ShrineTab.RuneTree);
            WireTab(changeTabButton, ShrineTab.Change);
            WireTab(resetTabButton, ShrineTab.Reset);

            for (int i = 0; i < changeOptions.Count; i++)
            {
                ChangeClassOption option = changeOptions[i];
                if (option.Button == null)
                {
                    continue;
                }

                option.Button.onClick.RemoveAllListeners();
                RuneClass capturedClass = option.ClassId;
                option.Button.onClick.AddListener(() => SelectChangeClass(capturedClass));
            }

            if (confirmChangeButton != null)
            {
                confirmChangeButton.onClick.RemoveAllListeners();
                confirmChangeButton.onClick.AddListener(HandleChangeConfirmed);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(HandleResetConfirmed);
            }
        }

        private void WireTab(Button button, ShrineTab tab)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ShowTab((int)tab));
        }

        private void SelectChangeClass(RuneClass runeClass)
        {
            selectedClass = runeClass;
            RefreshChange();
        }

        private void ShowSelectedChangeDetail(
            GameRunState run,
            DataManager data,
            RuneClass currentClass
        )
        {
            RuneClass selected = selectedClass;
            int cost = Mathf.Max(0, data.Balance.RuneClassChangeCostGold);
            bool valid = selected != RuneClass.None && selected != currentClass;
            bool enoughGold = run.Gold >= cost;

            SetText(changeTitleLabel, ClassTitle(selected));
            SetText(changeRoleLabel, ClassRole(selected));
            SetText(changeDescLabel, ClassDescription(selected));
            SetText(changeValueLabel, ClassBonus(selected) + " / COST " + cost + " GOLD");

            if (!valid)
            {
                SetChangeInteractable(false, "SELECT RUNE");
            }
            else if (!enoughGold)
            {
                SetChangeInteractable(false, "NOT ENOUGH GOLD");
            }
            else
            {
                SetChangeInteractable(true, "CONFIRM RUNE");
            }
        }

        private void SetClassOption(ChangeClassOption option, bool selected)
        {
            SetText(option.Title, ClassTitle(option.ClassId));
            SetText(option.Tag, ClassTag(option.ClassId));
            SetText(option.Role, ClassRole(option.ClassId));
            SetText(option.Desc, ClassDescription(option.ClassId));
            SetText(option.Bonus, ClassBonus(option.ClassId));

            if (option.Button != null)
            {
                option.Button.interactable = option.Root != null && option.Root.activeSelf;
            }

            if (option.Background != null)
            {
                option.Background.color = selected ? selectedCardColor : option.BaseColor;
            }
        }

        private void SetChangeInteractable(bool interactable)
        {
            SetChangeInteractable(interactable, interactable ? "CONFIRM RUNE" : "UNAVAILABLE");
        }

        private void SetChangeInteractable(bool interactable, string label)
        {
            if (confirmChangeButton != null)
            {
                confirmChangeButton.interactable = interactable;
            }

            SetText(confirmChangeButtonLabel, label);
        }

        private void SetResetInteractable(bool interactable, string label)
        {
            if (resetButton != null)
            {
                resetButton.interactable = interactable;
            }

            SetText(resetButtonLabel, label);
        }

        private static RuneClass FirstChangeCandidate(RuneClass current)
        {
            for (int i = 0; i < ChangeClasses.Length; i++)
            {
                if (ChangeClasses[i] != current)
                {
                    return ChangeClasses[i];
                }
            }

            return RuneClass.None;
        }

        public static int PreviewResetRefund(PlayerRuneState rune, BalanceData balance)
        {
            return rune != null ? rune.PreviewResetRefund(balance) : 0;
        }

        public static bool HasResettableInvestment(PlayerRuneState rune)
        {
            return rune != null && rune.HasResettableInvestment();
        }

        private static bool TryGetTransactionInputs(
            GameRunState run,
            DataManager data,
            out PlayerRuneState rune,
            out BalanceData balance
        )
        {
            rune = run?.Player?.Rune;
            balance = data?.Balance;
            bool valid =
                run?.Player != null
                && rune?.Tree?.AllNodes != null
                && data?.Runes != null
                && balance != null;
            if (valid)
            {
                rune.SyncTreeStateFromProgress();
            }

            return valid;
        }

        private void RefreshRunePointSummary()
        {
            int runePoints = 0;
            if (
                GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                && gsm.CurrentRun?.Player?.Rune != null
            )
            {
                runePoints = gsm.CurrentRun.Player.Rune.RunePoints;
            }

            SetText(runePointSummaryLabel, "RUNE POINTS " + runePoints);
        }

        private void SetTabVisual(Button button, bool active)
        {
            if (button == null)
            {
                return;
            }

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            SetTextColor(label, active ? activeText : inactiveText);
            Transform indicator = button.transform.Find("Indicator");
            if (indicator != null)
            {
                indicator.gameObject.SetActive(active);
            }
        }

        private static void SetContentActive(GameObject content, bool active)
        {
            if (content != null)
            {
                content.SetActive(active);
            }
        }

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        private static void SetTextColor(TMP_Text label, Color color)
        {
            if (label != null)
            {
                label.color = color;
            }
        }

        private static string ClassTitle(RuneClass runeClass)
        {
            switch (runeClass)
            {
                case RuneClass.Dealer:
                    return "CRIMSON OATH";
                case RuneClass.Tanker:
                    return "IRON VOW";
                case RuneClass.MagicDealer:
                    return "ASHEN SCRIPT";
                case RuneClass.Supporter:
                    return "SILVER HYMN";
                default:
                    return "SELECT RUNE";
            }
        }

        private static string ClassTag(RuneClass runeClass)
        {
            return runeClass == RuneClass.MagicDealer
                ? "MAGIC DEALER"
                : runeClass.ToString().ToUpperInvariant();
        }

        private static string ClassRole(RuneClass runeClass)
        {
            return ClassTag(runeClass) + " RUNE";
        }

        private static string ClassDescription(RuneClass runeClass)
        {
            switch (runeClass)
            {
                case RuneClass.Dealer:
                    return "A blood-bound oath enhancing physical prowess.";
                case RuneClass.Tanker:
                    return "A defensive vow focused on survival and armor.";
                case RuneClass.MagicDealer:
                    return "An ashen script strengthening spell pressure.";
                case RuneClass.Supporter:
                    return "A silver hymn improving recovery and stability.";
                default:
                    return string.Empty;
            }
        }

        private static string ClassBonus(RuneClass runeClass)
        {
            switch (runeClass)
            {
                case RuneClass.Dealer:
                    return "ATK + SPD";
                case RuneClass.Tanker:
                    return "HP + DEF";
                case RuneClass.MagicDealer:
                    return "MP + ATK";
                case RuneClass.Supporter:
                    return "HP + MP";
                default:
                    return string.Empty;
            }
        }
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// Safe2 성소 UI. 단계 카드, 상세 패널, 정화 버튼을 표시한다.
    /// </summary>
    public sealed class SanctuaryUI : MonoBehaviour
    {
        private sealed class StageCard
        {
            public GameObject Root;
            public Button Button;
            public Slider Slider;
            public TMP_Text Percent;
            public TMP_Text Risk;
            public TMP_Text Cost;
            public TMP_Text Status;
            public Graphic Background;
            public Color BaseColor;
        }

        private readonly List<StageCard> cards = new List<StageCard>();
        private readonly List<TMP_Text> summaryValues = new List<TMP_Text>();
        private readonly Color selectedColor = new Color(0.22f, 0.74f, 0.97f, 0.22f);
        private readonly Color highRiskColor = new Color(1f, 0.2f, 0.27f, 1f);
        private readonly Color midRiskColor = new Color(0.98f, 0.8f, 0.08f, 1f);
        private readonly Color lowRiskColor = new Color(0.22f, 0.77f, 0.37f, 1f);

        [SerializeField]
        private SanctuaryController controller;

        private TMP_Text headerTitle;
        private TMP_Text headerSubtitle;
        private TMP_Text descriptionText;
        private Button purifyButton;
        private Button cancelButton;
        private TMP_Text purifyButtonLabel;
        private TMP_Text goldText;
        private int selectedStage = 1;
        private bool initialized;

        private void Awake()
        {
            initialized = CacheHierarchy();
            if (!initialized)
            {
                enabled = false;
                return;
            }

            WireButtons();
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
            if (!initialized)
            {
                return;
            }

            if (!TryGetRuntime(out GameSystemManager gsm))
            {
                return;
            }

            int maxStage = gsm.Erosion != null ? gsm.Erosion.MaxStage : ErosionSystem.GetMaxStage(gsm.Data?.World);
            selectedStage = Mathf.Clamp(selectedStage, 1, Mathf.Max(1, maxStage));
            for (int i = 0; i < cards.Count; i++)
            {
                int stage = i + 1;
                bool visible = stage <= maxStage;
                if (cards[i] == null)
                {
                    continue;
                }

                cards[i].Root.SetActive(visible);
                if (visible)
                {
                    RefreshCard(cards[i], stage, gsm);
                }
            }

            RefreshDetail(gsm);
            UpdateGold(gsm.CurrentRun != null ? gsm.CurrentRun.Gold : 0);
        }

        private bool CacheHierarchy()
        {
            if (controller == null)
            {
                Debug.LogError("[SanctuaryUI] SanctuaryController 참조가 Inspector 에 연결되어 있지 않습니다.");
                return false;
            }

            Transform stageGrid = transform.Find("ContentArea/LeftColumn/StageGrid");
            Transform rightColumn = transform.Find("ContentArea/RightColumn");
            if (stageGrid == null || rightColumn == null)
            {
                Debug.LogError("[SanctuaryUI] StageGrid 또는 RightColumn 경로를 찾지 못했습니다.");
                return false;
            }

            cards.Clear();
            for (int i = 1; i <= 6; i++)
            {
                Transform cardRoot = stageGrid.Find("STAGE " + i + "_Card");
                if (cardRoot == null)
                {
                    Debug.LogError("[SanctuaryUI] STAGE " + i + "_Card 를 찾지 못했습니다.");
                    return false;
                }

                StageCard card = CreateCard(cardRoot);
                if (card == null)
                {
                    return false;
                }

                cards.Add(card);
            }

            Transform header = rightColumn.Find("Header");
            headerTitle = FindText(header, "StageTitle");
            headerSubtitle = FindText(header, "Subtitle");
            descriptionText = FindText(rightColumn.Find("Description"), "Desc");
            Transform actionButtons = rightColumn.Find("ActionButtons");
            purifyButton = FindButton(actionButtons, "PurifyButton");
            cancelButton = FindButton(actionButtons, "CancelButton");
            purifyButtonLabel = purifyButton != null ? purifyButton.GetComponentInChildren<TMP_Text>(true) : null;
            goldText = FindText(transform, "Gold");

            CacheSummaryValues(rightColumn.Find("SummaryValues"));
            if (purifyButton == null)
            {
                Debug.LogError("[SanctuaryUI] PurifyButton 을 찾지 못했습니다.");
                return false;
            }

            return true;
        }

        private StageCard CreateCard(Transform root)
        {
            Graphic background = root.GetComponent<Graphic>();
            Button button = root.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("[SanctuaryUI] " + root.name + " Button 참조가 없습니다.");
                return null;
            }

            if (background != null)
            {
                button.targetGraphic = background;
            }

            return new StageCard
            {
                Root = root.gameObject,
                Button = button,
                Slider = FindComponentByName<Slider>(root, "ErosionSlider"),
                Percent = FindText(root, "Percent"),
                Risk = FindText(root, "Risk"),
                Cost = FindText(root, "GoldText"),
                Status = FindText(root, "Status"),
                Background = background,
                BaseColor = background != null ? background.color : Color.clear,
            };
        }

        private void CacheSummaryValues(Transform root)
        {
            summaryValues.Clear();
            if (root == null)
            {
                return;
            }

            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && texts[i].name == "Value")
                {
                    summaryValues.Add(texts[i]);
                }
            }
        }

        private void WireButtons()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == null)
                {
                    continue;
                }

                int stage = i + 1;
                cards[i].Button.onClick.RemoveAllListeners();
                cards[i].Button.onClick.AddListener(() => SelectStage(stage));
            }

            purifyButton.onClick.RemoveAllListeners();
            purifyButton.onClick.AddListener(() =>
            {
                if (controller.TryPurify(selectedStage))
                {
                    Refresh();
                }
            });

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(Refresh);
            }
        }

        private void SelectStage(int stage)
        {
            selectedStage = stage;
            Refresh();
        }

        private void RefreshCard(StageCard card, int stage, GameSystemManager gsm)
        {
            float rate = GetRate(gsm, stage);
            if (card.Slider != null)
            {
                card.Slider.value = Mathf.Clamp01(rate / 100f);
            }

            SetText(card.Percent, Mathf.RoundToInt(rate) + "%");
            SetText(card.Risk, RiskTier(rate));
            SetText(card.Cost, controller.GetPurifyCost(stage) + "G");
            SetText(card.Status, StatusText(gsm, stage));

            if (card.Risk != null)
            {
                card.Risk.color = RiskColor(rate);
            }

            if (card.Background != null)
            {
                card.Background.color = stage == selectedStage ? selectedColor : card.BaseColor;
            }
        }

        private void RefreshDetail(GameSystemManager gsm)
        {
            float rate = GetRate(gsm, selectedStage);
            int closingSafeIndex = StageIndexResolver.SafeIndexForStage(selectedStage, gsm.Data?.World);
            StageIndexResolver.TryGetFloorRange(selectedStage, gsm.Data?.World, out int floorStart, out int floorEnd);

            SetText(headerTitle, "STAGE " + selectedStage);
            SetText(headerSubtitle, "Floor " + floorStart + " - " + floorEnd + " / Safe" + closingSafeIndex + " risk");
            SetText(descriptionText, "Spend Gold to purify this Stage before erosion reaches 100%.");
            SetSummary(0, Mathf.RoundToInt(rate) + "%");
            SetSummary(1, (gsm.Data?.Balance != null ? gsm.Data.Balance.ErosionAltarReduction : 0f).ToString("0") + "%");
            SetSummary(2, controller.GetPurifyCost(selectedStage) + "G");
            SetSummary(3, "Safe" + closingSafeIndex);
            SetSummary(4, RiskTier(rate));

            bool canPurify = controller.CanPurify(selectedStage);
            purifyButton.interactable = canPurify;
            SetText(purifyButtonLabel, canPurify ? "PURIFY" : "LOCKED");
        }

        private void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnStageErosionChanged -= HandleStageErosionChanged;
                gsm.Events.OnErosionActivated -= Refresh;
                gsm.Events.OnStageFullyEroded -= HandleStageFullyEroded;
                gsm.Events.OnGoldChanged -= UpdateGold;
                gsm.Events.OnSafeZoneLockChanged -= HandleSafeZoneLockChanged;
                gsm.Events.OnStageErosionChanged += HandleStageErosionChanged;
                gsm.Events.OnErosionActivated += Refresh;
                gsm.Events.OnStageFullyEroded += HandleStageFullyEroded;
                gsm.Events.OnGoldChanged += UpdateGold;
                gsm.Events.OnSafeZoneLockChanged += HandleSafeZoneLockChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnStageErosionChanged -= HandleStageErosionChanged;
                gsm.Events.OnErosionActivated -= Refresh;
                gsm.Events.OnStageFullyEroded -= HandleStageFullyEroded;
                gsm.Events.OnGoldChanged -= UpdateGold;
                gsm.Events.OnSafeZoneLockChanged -= HandleSafeZoneLockChanged;
            }
        }

        private void HandleStageErosionChanged(int stage, float rate)
        {
            Refresh();
        }

        private void HandleStageFullyEroded(int stage)
        {
            Refresh();
        }

        private void HandleSafeZoneLockChanged(int index, bool locked)
        {
            Refresh();
        }

        private void UpdateGold(int gold)
        {
            SetText(goldText, gold + " G");
        }

        private static float GetRate(GameSystemManager gsm, int stage)
        {
            return gsm?.Erosion?.Model != null ? gsm.Erosion.Model.GetRate(stage) : 0f;
        }

        private static string StatusText(GameSystemManager gsm, int stage)
        {
            if (gsm?.Erosion == null)
            {
                return "안정";
            }

            if (gsm.Erosion.IsStageFullyEroded(stage))
            {
                return "침식 완료";
            }

            return gsm.Erosion.Model != null && gsm.Erosion.Model.CurrentEroddingStage == stage ? "침식 진행" : "안정";
        }

        private static string RiskTier(float rate)
        {
            if (rate >= 66f)
            {
                return "HIGH";
            }

            return rate >= 33f ? "MID" : "LOW";
        }

        private Color RiskColor(float rate)
        {
            if (rate >= 66f)
            {
                return highRiskColor;
            }

            return rate >= 33f ? midRiskColor : lowRiskColor;
        }

        private void SetSummary(int index, string value)
        {
            if (index >= 0 && index < summaryValues.Count)
            {
                summaryValues[index].text = value;
            }
        }

        private static bool TryGetRuntime(out GameSystemManager gsm)
        {
            return GameSystemManager.TryGetInstance(out gsm) && gsm.CurrentRun != null;
        }

        private static T FindComponentByName<T>(Transform root, string childName)
            where T : Component
        {
            Transform child = FindDescendant(root, childName);
            return child != null ? child.GetComponent<T>() : null;
        }

        private static TMP_Text FindText(Transform root, string childName)
        {
            Transform child = FindDescendant(root, childName);
            return child != null ? child.GetComponentInChildren<TMP_Text>(true) : null;
        }

        private static Button FindButton(Transform root, string childName)
        {
            Transform child = FindDescendant(root, childName);
            return child != null ? child.GetComponent<Button>() : null;
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendant(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }
    }
}

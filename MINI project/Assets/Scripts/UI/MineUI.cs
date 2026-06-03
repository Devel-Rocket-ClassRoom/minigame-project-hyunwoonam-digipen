using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// Safe3~5 광산 UI. 활성화, 누적 골드 표시, 수령 버튼을 담당한다.
    /// </summary>
    public sealed class MineUI : MonoBehaviour
    {
        private sealed class ValueBinding
        {
            public string RowName;
            public string Label;
            public TMP_Text Value;
        }

        private readonly List<ValueBinding> infoValues = new List<ValueBinding>();
        private readonly List<ValueBinding> collectionValues = new List<ValueBinding>();

        [SerializeField]
        private MineController controller;

        private GameObject activationPanel;
        private GameObject collectionPanel;
        private TMP_Text mineName;
        private TMP_Text descriptionText;
        private TMP_Text bottomGuideText;
        private TMP_Text activationDailyGold;
        private TMP_Text activationCost;
        private TMP_Text storedGoldHero;
        private TMP_Text ownedGold;
        private TMP_Text totalGold;
        private Button activateButton;
        private Button activationCancelButton;
        private Button collectButton;
        private Button closeButton;
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

            if (!TryGetRun(out GameRunState run))
            {
                return;
            }

            bool active = controller.IsActivated;
            activationPanel.SetActive(!active);
            collectionPanel.SetActive(active);

            int dailyGain = controller.GetDailyGain();
            int stored = controller.GetStoredGold();
            int activationPrice = controller.GetActivationCost();

            SetText(mineName, "Mine " + (controller.MineIndex + 1));
            SetText(descriptionText, "Activate this mine to store daily Gold for later collection.");
            SetText(bottomGuideText, active ? "Collect stored Gold before leaving the sanctuary route." : "Activate this mine to unlock daily Gold production.");
            SetText(activationDailyGold, dailyGain + " G");
            SetText(activationCost, activationPrice + " G");
            SetText(storedGoldHero, stored + " G");

            SetInfoValue("daily", 0, dailyGain + " G");
            SetInfoValue("stored", 1, stored + " G");
            SetInfoValue("status", 2, active ? "가동" : "미가동");
            SetInfoValue("safe", 3, "Safe" + controller.SafeIndex);

            SetCollectionValue("stored", 0, stored + " G");
            SetCollectionValue("daily", 1, dailyGain + " G");
            SetCollectionValue("status", 2, active ? "가동 중" : "미가동");
            SetCollectionValue("after", 3, (run.Gold + stored) + " G");

            UpdateGold(run.Gold);
            activateButton.interactable = controller.CanActivate();
            collectButton.interactable = active && stored > 0;
        }

        private bool CacheHierarchy()
        {
            if (controller == null)
            {
                Debug.LogError("[MineUI] MineController 참조가 Inspector 에 연결되어 있지 않습니다.");
                return false;
            }

            Transform mainMinePanel = transform.Find("MainMinePanel");
            Transform mineInfoPanel = mainMinePanel != null ? mainMinePanel.Find("MineInfoPanel") : null;
            Transform statGrid = mineInfoPanel != null ? mineInfoPanel.Find("StatGrid") : null;
            Transform activation = transform.Find("ActivationInfoPanel");
            Transform collection = transform.Find("CollectionPanel");

            if (mineInfoPanel == null || activation == null || collection == null)
            {
                Debug.LogError("[MineUI] MineInfoPanel / ActivationInfoPanel / CollectionPanel 경로를 찾지 못했습니다.");
                return false;
            }

            activationPanel = activation.gameObject;
            collectionPanel = collection.gameObject;
            mineName = FindText(mineInfoPanel, "MineName");
            descriptionText = FindText(mineInfoPanel, "Text");
            bottomGuideText = FindText(transform.Find("BottomGuideBox"), "Text");
            activateButton = FindButton(activation, "ActivateButton");
            activationCancelButton = FindButton(activation, "CancelButton");
            collectButton = FindButton(collection, "Btn_CollectGold");
            closeButton = FindButton(collection, "Btn_Close");
            activationDailyGold = FindRowValue(activation, "Row_DailyGold");
            activationCost = FindRowValue(activation, "ActivationCost");
            storedGoldHero = FindText(collection, "Amount");
            if (storedGoldHero == null)
            {
                storedGoldHero = FindText(collection, "Value");
            }

            ownedGold = FindText(transform.root, "OwnedGold");
            totalGold = FindText(transform.root, "TotalGold");
            if (ownedGold == null)
            {
                ownedGold = FindText(transform.root, "Gold");
            }

            CacheValues(statGrid, infoValues);
            CacheValues(FindDescendant(collection, "CollectionSummaryValues"), collectionValues);

            if (activateButton == null || collectButton == null)
            {
                Debug.LogError("[MineUI] ActivateButton 또는 Btn_CollectGold 를 찾지 못했습니다.");
                return false;
            }

            return true;
        }

        private void WireButtons()
        {
            activateButton.onClick.RemoveAllListeners();
            activateButton.onClick.AddListener(() =>
            {
                if (controller.TryActivateMine())
                {
                    Refresh();
                }
            });

            collectButton.onClick.RemoveAllListeners();
            collectButton.onClick.AddListener(() =>
            {
                if (controller.TryCollectStoredGold())
                {
                    Refresh();
                }
            });

            if (activationCancelButton != null)
            {
                activationCancelButton.onClick.RemoveAllListeners();
                activationCancelButton.onClick.AddListener(ClosePanel);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(ClosePanel);
            }
        }

        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }

        private void SubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnGoldChanged -= UpdateGold;
                gsm.Events.OnDayChanged -= HandleDayChanged;
                gsm.Events.OnGoldChanged += UpdateGold;
                gsm.Events.OnDayChanged += HandleDayChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnGoldChanged -= UpdateGold;
                gsm.Events.OnDayChanged -= HandleDayChanged;
            }
        }

        private void HandleDayChanged(int day)
        {
            Refresh();
        }

        private void UpdateGold(int gold)
        {
            SetText(ownedGold, gold + " G");
            SetText(totalGold, gold + " G");
        }

        private void SetInfoValue(string key, int fallbackIndex, string value)
        {
            ValueBinding binding = FindBinding(infoValues, key);
            if (binding == null && fallbackIndex >= 0 && fallbackIndex < infoValues.Count)
            {
                binding = infoValues[fallbackIndex];
            }

            if (binding?.Value != null)
            {
                binding.Value.text = value;
            }
        }

        private void SetCollectionValue(string key, int fallbackIndex, string value)
        {
            ValueBinding binding = FindBinding(collectionValues, key);
            if (binding == null && fallbackIndex >= 0 && fallbackIndex < collectionValues.Count)
            {
                binding = collectionValues[fallbackIndex];
            }

            if (binding?.Value != null)
            {
                binding.Value.text = value;
            }
        }

        private static void CacheValues(Transform root, List<ValueBinding> target)
        {
            target.Clear();
            if (root == null)
            {
                return;
            }

            Transform[] rows = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < rows.Length; i++)
            {
                TMP_Text value = FindDirectText(rows[i], "Value");
                if (value == null)
                {
                    continue;
                }

                TMP_Text label = FindDirectText(rows[i], "Label");
                target.Add(
                    new ValueBinding
                    {
                        RowName = rows[i].name,
                        Label = label != null ? label.text : string.Empty,
                        Value = value,
                    }
                );
            }
        }

        private static TMP_Text FindRowValue(Transform root, string rowName)
        {
            Transform row = FindDescendant(root, rowName);
            return FindText(row, "Value");
        }

        private static bool TryGetRun(out GameRunState run)
        {
            run = null;
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.CurrentRun == null)
            {
                return false;
            }

            run = gsm.CurrentRun;
            run.EnsureMineState();
            return true;
        }

        private static TMP_Text FindText(Transform root, string childName)
        {
            Transform child = FindDescendant(root, childName);
            return child != null ? child.GetComponentInChildren<TMP_Text>(true) : null;
        }

        private static TMP_Text FindDirectText(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child.GetComponent<TMP_Text>() ?? child.GetComponentInChildren<TMP_Text>(true);
                }
            }

            return null;
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

        private static ValueBinding FindBinding(List<ValueBinding> bindings, string key)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                string rowName = bindings[i].RowName != null ? bindings[i].RowName.ToLowerInvariant() : string.Empty;
                string label = bindings[i].Label != null ? bindings[i].Label.ToLowerInvariant() : string.Empty;
                if (MatchesKey(rowName, key) || MatchesKey(label, key))
                {
                    return bindings[i];
                }
            }

            return null;
        }

        private static bool MatchesKey(string source, string key)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            switch (key)
            {
                case "daily":
                    return source.Contains("daily") || source.Contains("day") || source.Contains("일일");
                case "stored":
                    return source.Contains("stored") || source.Contains("storage") || source.Contains("저장");
                case "status":
                    return source.Contains("status") || source.Contains("state") || source.Contains("상태");
                case "safe":
                    return source.Contains("safe") || source.Contains("zone") || source.Contains("위치");
                case "after":
                    return source.Contains("after") || source.Contains("collection") || source.Contains("수령");
                default:
                    return false;
            }
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

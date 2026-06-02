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
        private readonly List<TMP_Text> infoValues = new List<TMP_Text>();
        private readonly List<TMP_Text> collectionValues = new List<TMP_Text>();

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

            SetInfoValue(0, dailyGain + " G");
            SetInfoValue(1, stored + " G");
            SetInfoValue(2, active ? "가동" : "미가동");
            SetInfoValue(3, "Safe" + controller.SafeIndex);

            SetCollectionValue(0, stored + " G");
            SetCollectionValue(1, dailyGain + " G");
            SetCollectionValue(2, active ? "가동 중" : "미가동");
            SetCollectionValue(3, (run.Gold + stored) + " G");

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
                activationCancelButton.onClick.AddListener(() => activationPanel.SetActive(false));
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => collectionPanel.SetActive(false));
            }
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

        private void SetInfoValue(int index, string value)
        {
            if (index >= 0 && index < infoValues.Count)
            {
                infoValues[index].text = value;
            }
        }

        private void SetCollectionValue(int index, string value)
        {
            if (index >= 0 && index < collectionValues.Count)
            {
                collectionValues[index].text = value;
            }
        }

        private static void CacheValues(Transform root, List<TMP_Text> target)
        {
            target.Clear();
            if (root == null)
            {
                return;
            }

            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && texts[i].name == "Value")
                {
                    target.Add(texts[i]);
                }
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

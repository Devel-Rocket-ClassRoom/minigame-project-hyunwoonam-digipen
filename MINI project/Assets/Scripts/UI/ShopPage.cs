using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Tempt
{
    // Guid2 §11 2026-05-28: 상점 화면 단일 진입점.
    // - 구매 목록 + 판매 후보 목록 + ItemInfoPanel.
    // - 데이터 변경은 직접 하지 않고 Shop 정적 유틸과 EventBus 만 사용.
    // - fallback 금지. Inspector 미연결 시 enabled = false.
    /// <summary>
    /// Safe1 상점 화면. 구매/판매 목록을 표시하고 선택 항목은 ItemInfoPanel 로 넘긴다.
    /// </summary>
    public sealed class ShopPage : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Transform buyListRoot;
        [SerializeField] private Transform sellListRoot;
        [SerializeField] private Button itemEntryPrefab;
        [SerializeField] private Text goldLabel;
        [SerializeField] private ItemInfoPanel infoPanel;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            root.SetActive(false);
        }

        /// <summary>상점 화면을 열고 EventBus 구독을 시작한다.</summary>
        public void OnOpen()
        {
            if (!enabled)
            {
                return;
            }

            root.SetActive(true);
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnInventoryChanged -= Refresh;
                gsm.Events.OnGoldChanged -= OnGoldChanged;
                gsm.Events.OnInventoryChanged += Refresh;
                gsm.Events.OnGoldChanged += OnGoldChanged;
            }

            Refresh();
        }

        /// <summary>상점 화면을 닫고 EventBus 구독을 해제한다.</summary>
        public void OnClose()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                gsm.Events.OnInventoryChanged -= Refresh;
                gsm.Events.OnGoldChanged -= OnGoldChanged;
            }

            if (root != null)
            {
                root.SetActive(false);
            }
        }

        /// <summary>구매 가능 목록, 판매 후보 목록, 골드 표시를 현재 런 상태로 다시 그린다.</summary>
        public void Refresh()
        {
            if (!TryGetRunData(out GameRunState run, out DataManager data))
            {
                return;
            }

            goldLabel.text = "Gold " + run.Gold;
            ClearList(buyListRoot);
            ClearList(sellListRoot);

            var buyItems = new List<ItemData>(data.Items.Values);
            buyItems.Sort((a, b) => a.Id.CompareTo(b.Id));
            foreach (ItemData itemData in buyItems)
            {
                int price = Shop.GetBuyPrice(itemData.Id, run, data);
                SpawnEntry(buyListRoot, itemData.NameKey + "  " + price + "G", () => infoPanel.Show(itemData.Id, ItemDetailContext.Shop));
            }

            InventoryState inv = run.Player.Inventory;
            var stacks = new List<KeyValuePair<int, int>>(inv.StackableItems);
            stacks.Sort((a, b) => a.Key.CompareTo(b.Key));
            foreach (KeyValuePair<int, int> entry in stacks)
            {
                if (data.Items.TryGetValue(entry.Key, out ItemData itemData))
                {
                    int price = Shop.GetSellPrice(entry.Key, run, data, data.Balance);
                    SpawnEntry(sellListRoot, itemData.NameKey + " x" + entry.Value + "  " + price + "G", () => infoPanel.Show(entry.Key, ItemDetailContext.Shop));
                }
            }

            foreach (Item item in inv.EquipItems)
            {
                if (item?.Data != null)
                {
                    int price = Shop.GetSellPrice(item.Data.Id, run, data, data.Balance);
                    SpawnEntry(sellListRoot, item.Data.NameKey + " +" + item.Enhancement + "  " + price + "G", () => infoPanel.ShowEquip(item, ItemDetailContext.Shop));
                }
            }
        }

        private bool ValidateReferences()
        {
            bool valid = root != null
                && buyListRoot != null
                && sellListRoot != null
                && itemEntryPrefab != null
                && goldLabel != null
                && infoPanel != null;
            if (!valid)
            {
                Debug.LogError("[ShopPage] 필수 UI 참조가 Inspector 에 직접 할당되어 있지 않습니다.");
            }

            return valid;
        }

        private static bool TryGetRunData(out GameRunState run, out DataManager data)
        {
            run = null;
            data = null;
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.CurrentRun?.Player?.Inventory == null || gsm.Data?.Items == null)
            {
                Debug.LogError("[ShopPage] GameSystemManager / CurrentRun.Player.Inventory / Data.Items 참조가 없습니다.");
                return false;
            }

            run = gsm.CurrentRun;
            data = gsm.Data;
            return true;
        }

        private void OnGoldChanged(int value)
        {
            goldLabel.text = "Gold " + value;
        }

        private void SpawnEntry(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            Button entry = Instantiate(itemEntryPrefab, parent);
            entry.gameObject.SetActive(true);
            entry.onClick.RemoveAllListeners();
            entry.onClick.AddListener(action);
            Text text = entry.GetComponentInChildren<Text>(true);
            if (text == null)
            {
                Debug.LogError("[ShopPage] itemEntryPrefab 하위 Text 참조가 없습니다.");
                entry.interactable = false;
                return;
            }

            text.text = label;
        }

        private static void ClearList(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}

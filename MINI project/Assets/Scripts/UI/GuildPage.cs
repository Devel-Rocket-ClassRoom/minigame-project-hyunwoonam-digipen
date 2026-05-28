using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    // Guid3 §8 2026-05-27: 길드 화면 단일 진입점.
    // 구매 탭 + 장착 탭 + SkillInfoPanel.
    // 데이터 변경은 Guild.* / SkillSwap.* / EventBus 만 사용. UI 는 갱신만.
    // fallback 금지: Inspector 미연결 시 Awake 에서 enabled = false.
    /// <summary>
    /// 길드 화면. AcquireType.Shop 인 스킬 구매 + 보유 스킬을 ActiveSkills 슬롯 2칸에 배치.
    /// </summary>
    public sealed class GuildPage : MonoBehaviour
    {
        private enum GuildTab
        {
            Buy,
            Slot,
        }

        [Header("Root")]
        [SerializeField] private GameObject root;
        [SerializeField] private Button buyTabButton;
        [SerializeField] private Button slotTabButton;
        [SerializeField] private GameObject buyPanel;
        [SerializeField] private GameObject slotPanel;

        [Header("Buy List")]
        [SerializeField] private Transform buyListRoot;
        [SerializeField] private Button skillEntryPrefab;

        [Header("Slot Panel")]
        [SerializeField] private Transform ownedListRoot;
        [SerializeField] private Button slot1Button;
        [SerializeField] private Button slot2Button;
        [SerializeField] private TMP_Text slot1Label;
        [SerializeField] private TMP_Text slot2Label;
        [SerializeField] private Button slot1ClearButton;
        [SerializeField] private Button slot2ClearButton;

        [Header("Header")]
        [SerializeField] private TMP_Text goldLabel;

        [Header("Detail")]
        [SerializeField] private SkillInfoPanel infoPanel;

        private GuildTab activeTab = GuildTab.Buy;

        /// <summary>현재 화면이 열려 있는가.</summary>
        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            // 본문 의사코드: Guid3 §8.3 Awake.
            // if (!ValidateReferences()) { Debug.LogError(...); enabled = false; return; }
            // WireStaticButtons();
            // root.SetActive(false);
        }

        /// <summary>화면 열기 + 이벤트 구독 + Refresh.</summary>
        public void OnOpen()
        {
            // 본문 의사코드: Guid3 §8.3 OnOpen.
        }

        /// <summary>이벤트 해제 + 화면 닫기.</summary>
        public void OnClose()
        {
            // 본문 의사코드: Guid3 §8.3 OnClose.
        }

        /// <summary>현재 PlayerState / DataManager 상태로 두 탭의 리스트와 슬롯 라벨을 새로 그린다.</summary>
        public void Refresh()
        {
            // 본문 의사코드: Guid3 §8.3 Refresh.
            // - RebuildBuyList(run, data).
            // - RebuildSlotPanel(run, data).
            // - OnGoldChanged(run.Gold).
        }

        private bool ValidateReferences()
        {
            // 본문 의사코드: Guid2 §3 ValidateReferences 표준 패턴.
            // root, 두 탭 버튼, 두 패널, buy/slot 의 리스트 루트와 prefab, 슬롯 버튼 4개,
            // 슬롯 라벨 2개, goldLabel, infoPanel 모두 검사.
            return false;
        }
    }
}

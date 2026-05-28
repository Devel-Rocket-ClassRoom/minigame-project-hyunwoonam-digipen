using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// 스킬 정보 패널 컨텍스트.
    /// </summary>
    public enum SkillDetailContext
    {
        /// <summary>길드 구매 탭 — 구매 버튼.</summary>
        GuildBuy,

        /// <summary>길드 장착 탭의 보유 스킬 클릭 — 슬롯 1/2 장착 버튼.</summary>
        GuildSlot,

        /// <summary>읽기 전용(스탯/룬 페이지, 전투 HUD 등) — 버튼 없음.</summary>
        Readonly,
    }

    // Guid3 §7 2026-05-27: ItemInfoPanel 과 별개의 스킬 정보 패널.
    // 컨텍스트(GuildBuy / GuildSlot / Readonly) 에 따라 버튼 라벨/핸들러가 달라진다.
    // fallback 금지: Inspector 필수 참조 누락은 Awake 에서 enabled = false.
    /// <summary>
    /// 선택된 스킬의 정보(이름/설명/타입/MP/효과/쿨다운/구매가) + 컨텍스트별 액션 버튼 표시.
    /// </summary>
    public sealed class SkillInfoPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text descLabel;
        [SerializeField] private TMP_Text typeLabel;
        [SerializeField] private TMP_Text mpCostLabel;
        [SerializeField] private TMP_Text effectLabel;
        [SerializeField] private TMP_Text cooldownLabel;
        [SerializeField] private TMP_Text priceLabel;
        [SerializeField] private TMP_Text ownedLabel;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TMP_Text primaryButtonLabel;
        [SerializeField] private TMP_Text secondaryButtonLabel;

        private void Awake()
        {
            // 본문 의사코드: Guid3 §7.4 Awake.
            // if (!ValidateReferences()) { Debug.LogError(...); enabled = false; return; }
            // Hide();
        }

        /// <summary>스킬 정보를 컨텍스트에 맞춰 표시한다.</summary>
        public void Show(int skillId, SkillDetailContext context)
        {
            // 본문 의사코드: Guid3 §7.4 Show.
            // - data.Skills.TryGetValue 실패 시 LogError + Hide.
            // - 컨텍스트 분기: ConfigureGuildBuy / ConfigureGuildSlot / ConfigureReadonly.
        }

        /// <summary>패널을 숨긴다.</summary>
        public void Hide()
        {
            // 본문 의사코드: Guid3 §7.4 Hide.
            // root.SetActive(false); ClearButton(primary/secondary).
        }

        private bool ValidateReferences()
        {
            // 본문 의사코드: Guid2 §3 ValidateReferences 표준 패턴.
            return false;
        }
    }
}

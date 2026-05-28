namespace Tempt
{
    // Guid3 §5 2026-05-27: 길드의 스킬 구매 / 가격 산식 단일 진입점.
    // Shop 과 같은 정책: BasePrice 위치는 SkillData.PurchasePrice, Inflation 은 Shop 산식 그대로 사용.
    // fallback 금지: run / data / Skill 데이터 누락은 LogError + 0 / false.
    /// <summary>
    /// 길드 스킬 거래. 구매 후 PlayerState.OwnedSkillIds 에 등록한다.
    /// 스킬 판매 / 환불은 Wave0 합의 범위 밖.
    /// </summary>
    public static class Guild
    {
        /// <summary>현재 단계 침식률을 적용한 스킬 구매가. 데이터/상태 누락 시 0.</summary>
        public static int GetSkillBuyPrice(int skillId, GameRunState run, DataManager data)
        {
            // 본문 의사코드: Guid3 §5.3 GetSkillBuyPrice.
            return 0;
        }

        /// <summary>스킬이 현재 구매 가능한지(보유 여부 + 골드 + 데이터 검증). UI 버튼 활성 표시용.</summary>
        public static bool CanBuy(int skillId, GameRunState run, DataManager data)
        {
            // 본문 의사코드: Guid3 §5.3 CanBuy.
            return false;
        }

        /// <summary>스킬을 골드로 구매. AcquireType == Shop 인 스킬만 허용. 실패 시 변동 없음.</summary>
        public static bool TryBuySkill(int skillId, GameRunState run, DataManager data)
        {
            // 본문 의사코드: Guid3 §5.3 TryBuySkill.
            // - SkillData.AcquireType == Shop 만 허용.
            // - 이미 OwnedSkillIds 에 있으면 false.
            // - Gold 차감 → OwnedSkillIds.Add → EventBus.RaiseGoldChanged + RaiseSkillsChanged.
            return false;
        }
    }
}

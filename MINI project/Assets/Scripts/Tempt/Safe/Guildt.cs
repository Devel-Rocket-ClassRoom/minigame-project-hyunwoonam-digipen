namespace Tempt
{
    /// <summary>
    /// 길드. 동료 등록(파티 구성), 스킬 구매/장착. 최대 3명까지 파티 편성.
    /// </summary>
    public sealed class Guildt
    {
        /// <summary>
        /// 길드 UI 열기.
        /// </summary>
        public void Open()
        {
            // 동작 요약:
            // - Roster.Bench와 Roster.Active 표시.
            // - 파티 슬롯 3칸(Active.Count ≤ 3) 편성 UI.
            // - 스킬 구매 목록(SkillDatat에서 캐릭터 직업으로 필터).
        }

        /// <summary>
        /// 동료 파티 합류.
        /// </summary>
        public bool TryAddToParty(int companionId)
        {
            // 동작 요약:
            // - Roster.Active.Count < 3 검사.
            // - Bench → Active 이동.
            return false;
        }

        /// <summary>
        /// 동료 파티 제외.
        /// </summary>
        public bool TryRemoveFromParty(int companionId)
        {
            // 동작 요약: Active → Bench 이동.
            return false;
        }

        /// <summary>
        /// 스킬 구매(플레이어 또는 동료에게 학습 — 설계 보류 항목).
        /// </summary>
        public bool TryBuySkill(int skillId, int targetCharacterId)
        {
            // 동작 요약:
            // - 가격 차감.
            // - 대상 캐릭터의 액티브 슬롯에 장착 가능 여부 확인 후 SetActiveSkill.
            return false;
        }
    }
}

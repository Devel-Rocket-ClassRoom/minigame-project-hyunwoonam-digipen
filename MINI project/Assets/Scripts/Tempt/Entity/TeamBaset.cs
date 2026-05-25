namespace Tempt
{
    /// <summary>
    /// 동료 베이스. 게임 시작 시 룬 트리가 무작위로 고정되고, 레벨업 시 정해진 트리 순서로 자동 해금.
    /// 행동 결정은 직업별 고정 우선순위 규칙(CompanionActionSelectort)을 사용.
    /// </summary>
    public abstract class TeamBaset : CharacterBaset
    {
        /// <summary>동료 데이터 ID(CompanionDatat 참조).</summary>
        public int CompanionDataId;

        /// <summary>고정 룬 트리 상태.</summary>
        public CompanionRuneStatet Rune;

        /// <summary>장비 슬롯(동료도 장비 가능).</summary>
        public EquipmentSlotst Equipment;

        /// <summary>행동 규칙 키(직업별 우선순위 식별자).</summary>
        public string ActionRuleKey;

        /// <summary>
        /// 모집 직후 1회 호출. 트리 무작위 생성 + 시작 룬 해금.
        /// </summary>
        public void Initialize(int companionDataId, int seed)
        {
            // 동작 요약:
            // - CompanionDataId = companionDataId.
            // - Rune = RuneTreeGeneratort.GenerateFixedTree(data, seed).
            // - Rune.UnlockStarter().
            // - 직업별 ActionRuleKey 설정.
            // - 기본 스탯 설정.
        }

        /// <inheritdoc/>
        protected override void OnLeveledUp()
        {
            // 동작 요약:
            // - 직업별 성장 적용.
            // - Rune.UnlockNextNodeIfPossible() — 미해금 노드 1개 자동 진행.
        }
    }
}

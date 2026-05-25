namespace Tempt
{
    /// <summary>
    /// 플레이어 캐릭터. 사용자가 직접 룬 트리를 해금하고 장비/소모를 관리한다.
    /// </summary>
    public sealed class Playert : CharacterBaset
    {
        /// <summary>선택한 시작 직업.</summary>
        public RuneClasst StartingClass;

        /// <summary>플레이어 룬 상태.</summary>
        public PlayerRuneStatet Rune;

        /// <summary>인벤토리.</summary>
        public InventoryStatet Inventory;

        /// <summary>장비 슬롯.</summary>
        public EquipmentSlotst Equipment;

        /// <summary>전투 소모 4칸.</summary>
        public ConsumableSlotst Consumables;

        /// <summary>보관함(주점 구매 후 활성).</summary>
        public LockerStatet Locker;

        /// <summary>
        /// Safe0에서 시작 룬을 선택한 시점에 1회 호출.
        /// </summary>
        public void ApplyStartingClass(RuneClasst pickedClass)
        {
            // 동작 요약:
            // - StartingClass = pickedClass.
            // - Rune = new PlayerRuneStatet(pickedClass).
            // - Rune.UnlockStarter().
        }

        /// <summary>
        /// 장비 변경 시 호출. 스탯 보정 재계산.
        /// </summary>
        public void RecalcBonusStats()
        {
            // 동작 요약:
            // - Equipment.AggregateStatMod() + Rune.AggregateStatMod() → Stats.Bonus.
            // - Stats.ClampToMax().
        }

        /// <inheritdoc/>
        protected override void OnLeveledUp()
        {
            // 동작 요약:
            // - 직업별 기본 성장량 적용.
            // - Rune.AddRunePoint(BalanceDatat.RunePointPerLevel).
            // - EventBust.RaisePlayerLevelUp(Level).
        }

        /// <summary>
        /// 후퇴 아이템 사용 시 분기. 전투 중에는 CombatControllert가 위임.
        /// </summary>
        public void UseRetreatItem()
        {
            // 동작 요약:
            // - Inventory에서 후퇴 아이템 1개 차감.
            // - GameSystemManagert.Instance.EndCombat(Retreat) 호출.
        }
    }
}

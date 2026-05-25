namespace Tempt
{
    /// <summary>
    /// 캐릭터(플레이어 + 동료)의 공통 베이스. 룬/EXP/레벨을 가짐.
    /// Monster는 이 클래스를 사용하지 않는다.
    /// </summary>
    public abstract class CharacterBaset : EntityBaset
    {
        /// <summary>현재 레벨(1부터 시작).</summary>
        public int Level { get; protected set; } = 1;

        /// <summary>현재 EXP.</summary>
        public int CurrentExp { get; protected set; }

        /// <summary>현 레벨의 필요 EXP(BalanceDatat 참조).</summary>
        public int RequiredExp { get; protected set; }

        /// <summary>EXP를 지급한다. 도달 시 레벨업.</summary>
        public void GainExp(int amount)
        {
            // 동작 요약:
            // - CurrentExp += amount.
            // - while (CurrentExp >= RequiredExp) → LevelUp().
            // - EventBust.RaisePlayerExpChanged(CurrentExp, RequiredExp).
            // - 동료는 별도 이벤트(이 클래스는 일단 같은 흐름, 발행은 파생에서).
        }

        /// <summary>
        /// 레벨업 처리. 룬 포인트 적립, 스탯 갱신.
        /// </summary>
        protected virtual void LevelUp()
        {
            // 동작 요약:
            // - CurrentExp -= RequiredExp.
            // - Level += 1.
            // - RequiredExp = BalanceDatat.ExpToNextLevel[Level].
            // - 스탯 성장(파생 클래스가 OnLeveledUp로 처리).
            // - 룬 포인트 적립(Player는 PlayerRuneStatet.AddPoint; Companion은 고정 트리 자동 해금).
            // - 이벤트 발행.
            OnLeveledUp();
        }

        /// <summary>레벨업 후 파생 클래스가 처리할 후훅.</summary>
        protected abstract void OnLeveledUp();

        /// <summary>
        /// 노드 클리어 시 합산 EXP를 받는다. CombatControllert가 호출.
        /// </summary>
        public void GainNodeReward(int totalExp)
        {
            // 동작 요약: GainExp(totalExp).
        }
    }
}

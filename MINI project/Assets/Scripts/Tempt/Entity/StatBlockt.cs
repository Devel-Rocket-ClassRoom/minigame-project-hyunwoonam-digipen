namespace Tempt
{
    /// <summary>
    /// 6대 핵심 스탯 + 현재 자원. EntityBaset의 런타임 수치를 표현.
    /// 8스탯 시스템은 제거됨. EXP는 CharacterBaset 전용으로 분리.
    /// </summary>
    public sealed class StatBlockt
    {
        /// <summary>최대 HP.</summary>
        public int MaxHP;

        /// <summary>현재 HP.</summary>
        public int CurrentHP;

        /// <summary>최대 MP.</summary>
        public int MaxMP;

        /// <summary>현재 MP.</summary>
        public int CurrentMP;

        /// <summary>공격력.</summary>
        public int ATK;

        /// <summary>방어력.</summary>
        public int DEF;

        /// <summary>공격속도. 라운드 큐 순서/추가 공격 확률에 영향.</summary>
        public int SPD;

        /// <summary>입력 보정 합산을 위한 누적 보정값(장비/룬/버프).</summary>
        public EquipmentStatModt Bonus;

        /// <summary>
        /// 최대치 변경 후 현재값을 범위 내로 보정.
        /// </summary>
        public void ClampToMax()
        {
            // 동작 요약: CurrentHP/CurrentMP를 [0, max] 범위로 제한.
        }

        /// <summary>
        /// HP/MP를 완전 회복.
        /// </summary>
        public void RestoreToFull()
        {
            // 동작 요약: CurrentHP = MaxHP, CurrentMP = MaxMP.
        }

        /// <summary>
        /// 입력 데미지를 적용하고 실제 차감 피해 반환.
        /// </summary>
        public int TakeDamage(int damage, bool isDefending)
        {
            // 동작 요약:
            // - DamageCalculatort가 이미 방어 적용 여부를 처리한 경우 isDefending=false로 호출.
            // - isDefending=true이면 DEF만큼 추가 경감.
            // - 결과 데미지를 CurrentHP에서 차감.
            return 0;
        }

        /// <summary>
        /// MP가 충분하면 차감.
        /// </summary>
        public bool TrySpendMP(int cost)
        {
            // 동작 요약: CurrentMP >= cost일 때만 차감하고 true 반환.
            return false;
        }
    }

    /// <summary>스탯 타입.</summary>
    public enum StatTypet
    {
        /// <summary>HP.</summary>
        HP,

        /// <summary>MP.</summary>
        MP,

        /// <summary>공격력.</summary>
        ATK,

        /// <summary>방어력.</summary>
        DEF,

        /// <summary>공격속도.</summary>
        SPD,
    }
}

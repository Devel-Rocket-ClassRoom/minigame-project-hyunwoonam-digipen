namespace Tempt
{
    /// <summary>
    /// 스킬 런타임 객체. SkillData 참조 + 쿨다운 등 동적 상태.
    /// Active: 플레이어/동료가 전투 중 슬롯(ActiveSkills[0~1])에서 능동적으로 사용.
    /// Passive: 룬 해금으로만 취득, EntityBase.PassiveSkills에 자동 등록, 직접 선택/제거 불가.
    ///          MP 소비 없음, 쿨다운 없음, 전투 시작 시 ApplyAllPassiveEffects()에서 스탯에 일괄 적용.
    /// </summary>
    public sealed class Skill
    {
        /// <summary>참조하는 정적 데이터.</summary>
        public SkillData Data;

        /// <summary>남은 쿨다운 라운드 수.</summary>
        public int CooldownRemaining;

        /// <summary>현재 사용 가능 여부.</summary>
        public bool IsReady => CooldownRemaining <= 0;

        /// <summary>빈 스킬 객체 생성.</summary>
        public Skill()
        {
        }

        /// <summary>정적 데이터 참조를 가진 스킬 객체 생성.</summary>
        public Skill(SkillData data)
        {
            Data = data;
        }

        /// <summary>
        /// 사용 가능 조건 검사(MP, 쿨다운).
        /// </summary>
        public bool CanUse(EntityBase user)
        {
            // 동작 요약: IsReady && user.Stats.CurrentMP >= Data.MpCost.
            return Data != null && user?.Stats != null && IsReady && user.Stats.CurrentMP >= Data.MpCost;
        }

        /// <summary>
        /// 사용 직후 호출. MP 차감, 쿨다운 시작.
        /// </summary>
        public void ConsumeForUse(EntityBase user)
        {
            // 동작 요약: user.Stats.TrySpendMP(Data.MpCost); CooldownRemaining = Data.CooldownRounds.
            if (Data == null || user?.Stats == null)
            {
                return;
            }

            user.Stats.TrySpendMP(Data.MpCost);
            CooldownRemaining = Data.CooldownRounds;
        }

        /// <summary>
        /// 라운드 종료 시 쿨다운 감소.
        /// </summary>
        public void TickCooldown()
        {
            // 동작 요약: CooldownRemaining = max(0, CooldownRemaining - 1).
            CooldownRemaining = UnityEngine.Mathf.Max(0, CooldownRemaining - 1);
        }
    }

    /// <summary>
    /// 스킬 타겟 타입. 모든 스킬은 이 enum 하나로 타겟팅 UI 흐름이 결정된다.
    /// 단일(Single) 타겟만 마우스 호버 + 클릭, 그 외는 즉시 시전.
    /// </summary>
    public enum SkillTargetType
    {
        /// <summary>적 단일 — 호버+클릭.</summary>
        EnemySingle,

        /// <summary>적 전체 — 즉시 시전.</summary>
        EnemyAll,

        /// <summary>아군 단일 — 호버+클릭.</summary>
        AllySingle,

        /// <summary>아군 전체 — 즉시 시전.</summary>
        AllyAll,

        /// <summary>자기 자신 — 즉시 시전.</summary>
        Self,
    }
}


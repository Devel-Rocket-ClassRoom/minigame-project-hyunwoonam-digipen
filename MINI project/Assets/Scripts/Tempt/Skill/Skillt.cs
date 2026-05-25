namespace Tempt
{
    /// <summary>
    /// 액티브 스킬 런타임 객체. SkillDatat 참조 + 쿨다운 등 동적 상태.
    /// 패시브는 제거되어 룬으로 대체됨.
    /// </summary>
    public sealed class Skillt
    {
        /// <summary>참조하는 정적 데이터.</summary>
        public SkillDatat Data;

        /// <summary>남은 쿨다운 라운드 수.</summary>
        public int CooldownRemaining;

        /// <summary>현재 사용 가능 여부.</summary>
        public bool IsReady => CooldownRemaining <= 0;

        /// <summary>
        /// 사용 가능 조건 검사(MP, 쿨다운).
        /// </summary>
        public bool CanUse(EntityBaset user)
        {
            // 동작 요약: IsReady && user.Stats.CurrentMP >= Data.MpCost.
            return false;
        }

        /// <summary>
        /// 사용 직후 호출. MP 차감, 쿨다운 시작.
        /// </summary>
        public void ConsumeForUse(EntityBaset user)
        {
            // 동작 요약: user.Stats.TrySpendMP(Data.MpCost); CooldownRemaining = Data.CooldownRounds.
        }

        /// <summary>
        /// 라운드 종료 시 쿨다운 감소.
        /// </summary>
        public void TickCooldown()
        {
            // 동작 요약: CooldownRemaining = max(0, CooldownRemaining - 1).
        }
    }

    /// <summary>
    /// 스킬 타겟 타입. 모든 스킬은 이 enum 하나로 타겟팅 UI 흐름이 결정된다.
    /// 단일(Single) 타겟만 마우스 호버 + 클릭, 그 외는 즉시 시전.
    /// </summary>
    public enum SkillTargetTypet
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

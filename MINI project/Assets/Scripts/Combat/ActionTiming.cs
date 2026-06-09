/// <summary>
/// 액션 연출 페이즈 시간. Anticipation(준비) → Impact(경계) → Recovery(복귀).
/// Impact 는 Anticipation 종료 시점이며, 결과(데미지/이펙트)는 이 시점에 적용된다.
/// </summary>
public struct ActionPhaseTiming
{
    /// <summary>준비 구간(초). 종료 시점에 Impact 발생.</summary>
    public float AnticipationSec;

    /// <summary>복귀 구간(초). Impact 이후 idle 복귀까지의 여운.</summary>
    public float RecoverySec;

    /// <summary>전체 연출 시간(초).</summary>
    public float TotalSec => AnticipationSec + RecoverySec;
}

/// <summary>
/// 행동의 연출 시간 계산. 기본 최소 시간 + 애니메이션/이펙트(준비) + 복귀 여운.
/// 데이터(Balance/Skill)에 명시값이 있으면 우선한다.
/// </summary>
public static class ActionTiming
{
    /// <summary>최소 기본 시간(초).</summary>
    private const float DefaultMinBaseSec = 0.1f;
    private const float DefaultAttackActionSec = 0.3f;
    private const float DefaultSkillFallbackSec = 0.5f;
    private const float DefaultDefendActionSec = 0.1f;

    // 연출 타임라인 분리(2026-06-05): 동일 행위자가 라운드 끝-시작에 연속 행동해도
    // 한 동작으로 뭉쳐 보이지 않도록 Impact 이후 복귀 여운을 둔다.
    private const float DefaultAttackRecoverySec = 0.32f;
    private const float DefaultSkillRecoverySec = 0.4f;
    private const float DefaultDefendRecoverySec = 0.2f;

    /// <summary>
    /// CombatAction 의 페이즈 시간 계산.
    /// AnticipationSec 종료 시 Impact 적용, 이후 RecoverySec 만큼 대기.
    /// </summary>
    public static ActionPhaseTiming ComputePhases(CombatAction action)
    {
        // 동작 요약:
        // - 공통 최소 시간(min)을 Anticipation 에 더한다.
        // - Attack: Anticipation = min + 공격 애니, Recovery = 기본 복귀.
        // - Skill : Anticipation = min + Skill.Data.ActionDuration(설정값) 또는 fallback, Recovery = 기본 복귀.
        // - Defend: Anticipation = min + 방어 즉시, Recovery = 기본 복귀.
        float min = GetBalanceValue(balance => balance.MinActionTimeSec, DefaultMinBaseSec);

        if (action == null)
        {
            return new ActionPhaseTiming { AnticipationSec = min, RecoverySec = 0f };
        }

        switch (action.Type)
        {
            case CombatActionType.Attack:
                return new ActionPhaseTiming
                {
                    AnticipationSec = min + GetBalanceValue(balance => balance.AttackActionTimeSec, DefaultAttackActionSec),
                    RecoverySec = DefaultAttackRecoverySec,
                };
            case CombatActionType.Skill:
                SkillData skill = action.ResolvedSkillData;
                float active = skill != null && skill.ActionDuration > 0f
                    ? skill.ActionDuration
                    : GetBalanceValue(balance => balance.SkillActionFallbackSec, DefaultSkillFallbackSec);
                return new ActionPhaseTiming
                {
                    AnticipationSec = min + active,
                    RecoverySec = DefaultSkillRecoverySec,
                };
            case CombatActionType.Defend:
                return new ActionPhaseTiming
                {
                    AnticipationSec = min + GetBalanceValue(balance => balance.DefendActionTimeSec, DefaultDefendActionSec),
                    RecoverySec = DefaultDefendRecoverySec,
                };
            default:
                return new ActionPhaseTiming { AnticipationSec = min, RecoverySec = 0f };
        }
    }

    private static float GetBalanceValue(System.Func<BalanceData, float> getter, float fallback)
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Data?.Balance != null)
        {
            float value = getter(gsm.Data.Balance);

            return value > 0f ? value : fallback;
        }

        return fallback;
    }
}

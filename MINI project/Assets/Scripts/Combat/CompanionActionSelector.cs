using System.Collections.Generic;

// Wave0refactor 2026-05-27 (F.4 + F.8):
// - Alive / LowestHp / LowestHpRatio / FillTargets / PickUsableSkill 헬퍼 정리.
//   타겟 선택 공통은 CombatTargeting 유틸로 위임.
// - 단일 타겟은 SingleSelect.LowestHp 전략 사용(동료는 위협을 빠르게 줄이는 방향이 일반적이라는 합의).
/// <summary>
/// 동료 1명의 행동 결정. 직업별 고정 우선순위 규칙.
/// 사용자 확정 규칙 예시:
///  - Dealer/Tanker: 적 단일 공격 우선 → 사용 가능 스킬 → 방어.
///  - MagicDealer  : 범위 스킬 우선 → 단일 스킬 → 공격.
///  - Supporter    : 아군 HP&lt;임계 회복 → 버프 스킬 → 공격.
/// </summary>
public sealed class CompanionActionSelector
{
    private readonly List<EntityBase> aliveAlliesBuffer = new List<EntityBase>();
    private readonly List<EntityBase> aliveEnemiesBuffer = new List<EntityBase>();

    /// <summary>
    /// 행동 결정. CombatFlow 가 호출.
    /// </summary>
    public CombatAction Pick(TeamBase companion, List<EntityBase> allies, List<EntityBase> enemies)
    {
        // 동작 요약:
        // - 살아있는 적/아군 버퍼를 CombatTargeting.Alive 로 채운다(필드 재사용).
        // - 직업 우선순위 규칙으로 행동 1개 결정:
        //   * Tanker 가 본인 HP <= 50% 면 즉시 방어.
        //   * 그 외엔 사용 가능 스킬 중 직업 규칙에 가장 잘 맞는 1개 선택.
        //   * 스킬이 없으면 LowestHp 적 공격.
        // - 단일 타겟 선택 전략은 SingleSelect.LowestHp 고정.
        CombatTargeting.Alive(allies, aliveAlliesBuffer);
        CombatTargeting.Alive(enemies, aliveEnemiesBuffer);
        if (companion == null || aliveEnemiesBuffer.Count == 0)
        {
            return null;
        }

        string rule = string.IsNullOrEmpty(companion.ActionRuleKey) ? "Dealer" : companion.ActionRuleKey;
        var action = new CombatAction { Actor = companion, Targets = new List<EntityBase>(), ConsumesTurn = true };

        // Tanker 자가 방어 분기
        if (rule == "Tanker" && companion.Stats != null && companion.Stats.CurrentHP <= companion.Stats.MaxHP / 2)
        {
            action.Type = CombatActionType.Defend;
            action.Targets.Add(companion);
            return action;
        }

        // 직업 규칙에 맞는 사용 가능 스킬 선택
        SkillPick skillPick = PickUsableSkill(companion, rule, aliveAlliesBuffer, aliveEnemiesBuffer);
        if (skillPick.Skill != null)
        {
            action.Type = CombatActionType.Skill;
            action.Skill = skillPick.Skill;
            action.EffectiveSkillData = skillPick.EffectiveData;
            CombatTargeting.FillByTargetType(
                action.Targets,
                skillPick.EffectiveData.TargetType,
                companion,
                aliveAlliesBuffer,
                aliveEnemiesBuffer,
                CombatTargeting.SingleSelect.LowestHp);
            return action;
        }

        // 기본: LowestHp 적 단일 공격
        action.Type = CombatActionType.Attack;
        EntityBase target = CombatTargeting.LowestHp(aliveEnemiesBuffer);
        if (target != null) action.Targets.Add(target);
        return action;
    }

    private static SkillPick PickUsableSkill(
        TeamBase companion,
        string rule,
        List<EntityBase> allies,
        List<EntityBase> enemies)
    {
        // 동작 요약:
        // - 모든 액티브 슬롯 순회.
        // - CanUse 통과한 첫 스킬을 fallback 으로 저장.
        // - 직업별 우선 매칭이 있으면 즉시 반환:
        //   * MagicDealer: 적이 2 이상이고 EnemyAll 스킬이면 우선.
        //   * Supporter:   아군 HpRatio 가 0.5 미만이고 HealScale > 0 인 스킬이면 우선.
        if (companion.ActiveSkills == null)
        {
            return SkillPick.Empty;
        }

        SkillPick fallback = SkillPick.Empty;
        for (int i = 0; i < companion.ActiveSkills.Length; i++)
        {
            Skill s = companion.ActiveSkills[i];
            SkillData effectiveData = SkillRuntimeResolver.Resolve(s, companion);
            if (s == null || !s.CanUse(companion, effectiveData))
            {
                continue;
            }

            var current = new SkillPick { Skill = s, EffectiveData = effectiveData };
            if (fallback.Skill == null)
            {
                fallback = current;
            }

            if (rule == "MagicDealer"
                && enemies.Count >= 2
                && effectiveData.TargetType == SkillTargetType.EnemyAll)
            {
                return current;
            }

            if (rule == "Supporter"
                && effectiveData.HealScale > 0f
                && CombatTargeting.LowestHpRatio(allies) < 0.5f)
            {
                return current;
            }
        }

        return fallback;
    }

    private struct SkillPick
    {
        public static readonly SkillPick Empty = new SkillPick();

        public Skill Skill;
        public SkillData EffectiveData;
    }
}

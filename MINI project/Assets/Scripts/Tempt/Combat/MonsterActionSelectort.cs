using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 몬스터 1마리의 행동 결정. 가중치 + 조건/쿨다운 기반.
    /// </summary>
    public sealed class MonsterActionSelectort
    {
        /// <summary>
        /// 행동 결정. CombatFlowt가 호출.
        /// </summary>
        public CombatActiont Pick(MonsterBaset monster, List<EntityBaset> allies, List<EntityBaset> enemies)
        {
            // 동작 요약:
            // - 후보 = [Attack] + 사용 가능 스킬 + [Defend].
            // - 가중치 = monster.ActionWeights.{Attack, Skill, Defend}.
            // - 스킬은 MP/쿨다운/조건 검사 후 가중치 합산.
            // - WeightedRandomt로 1개 선택.
            // - 타겟은 Skill.TargetType에 맞춰 자동 선택(EnemySingle = 무작위 또는 최저 HP 등 규칙).
            // - CombatActiont 반환(ConsumesTurn=true).
            return null;
        }
    }
}

using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 동료 1명의 행동 결정. 직업별 고정 우선순위 규칙.
    /// 사용자 확정 규칙 예시:
    ///  - Dealer/Tanker: 적 단일 공격 우선 → 사용 가능 스킬 → 방어.
    ///  - MagicDealer  : 범위 스킬 우선 → 단일 스킬 → 공격.
    ///  - Supporter    : 아군 HP&lt;임계 회복 → 버프 스킬 → 공격.
    /// </summary>
    public sealed class CompanionActionSelectort
    {
        /// <summary>
        /// 행동 결정.
        /// </summary>
        public CombatActiont Pick(TeamBaset companion, List<EntityBaset> allies, List<EntityBaset> enemies)
        {
            // 동작 요약:
            // - ActionRuleKey 분기:
            //   * "Dealer" → 가장 HP 낮은 적 공격.
            //   * "Tanker" → HP 절반 이하면 방어, 아니면 가장 ATK 높은 적 공격.
            //   * "MagicDealer" → 적 ≥2면 범위 스킬, 아니면 단일 스킬, 둘 다 불가면 공격.
            //   * "Supporter" → 아군 중 HP비 가장 낮은 자가 임계 이하면 회복, 아니면 공격.
            // - 결정된 행동을 CombatActiont로 반환.
            return null;
        }
    }
}

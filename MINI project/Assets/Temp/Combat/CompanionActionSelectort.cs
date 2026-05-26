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
            //TODO: var action = new CombatActiont { Actor = companion, ConsumesTurn = true };
            //TODO: action.Targets = new List<EntityBaset>();
            //TODO: switch (companion.Data.ActionRuleKey)
            //TODO: {
            //TODO:     case "Dealer":
            //TODO:         action.Type = CombatActionTypet.Attack;
            //TODO:         // 가장 현재 HP 낮은 적
            //TODO:         EntityBaset lowestHpEnemy = enemies[0];
            //TODO:         foreach (var e in enemies) if (e.Stats.CurrentHP < lowestHpEnemy.Stats.CurrentHP) lowestHpEnemy = e;
            //TODO:         action.Targets.Add(lowestHpEnemy);
            //TODO:         break;
            //TODO:     case "Tanker":
            //TODO:         if (companion.Stats.CurrentHP <= companion.Stats.MaxHP / 2)
            //TODO:         { action.Type = CombatActionTypet.Defend; action.Targets.Add(companion); }
            //TODO:         else
            //TODO:         { action.Type = CombatActionTypet.Attack; action.Targets.Add(enemies[0]); }
            //TODO:         break;
            //TODO:     case "MagicDealer":
            //TODO:         Skillt aoeSkill = companion.ActiveSkills.Find(s => s != null && s.CanUse(companion) && s.Data.TargetType == SkillTargetTypet.EnemyAll);
            //TODO:         Skillt singleSkill = companion.ActiveSkills.Find(s => s != null && s.CanUse(companion) && s.Data.TargetType == SkillTargetTypet.EnemySingle);
            //TODO:         if (enemies.Count >= 2 && aoeSkill != null)
            //TODO:         { action.Type = CombatActionTypet.Skill; action.Skill = aoeSkill; action.Targets.AddRange(enemies); }
            //TODO:         else if (singleSkill != null)
            //TODO:         { action.Type = CombatActionTypet.Skill; action.Skill = singleSkill; action.Targets.Add(enemies[0]); }
            //TODO:         else
            //TODO:         { action.Type = CombatActionTypet.Attack; action.Targets.Add(enemies[0]); }
            //TODO:         break;
            //TODO:     case "Supporter":
            //TODO:         EntityBaset criticalAlly = null;
            //TODO:         float lowestRatio = 1f;
            //TODO:         foreach (var a in allies)
            //TODO:         {
            //TODO:             float ratio = (float)a.Stats.CurrentHP / a.Stats.MaxHP;
            //TODO:             if (ratio < lowestRatio) { lowestRatio = ratio; criticalAlly = a; }
            //TODO:         }
            //TODO:         Skillt healSkill = companion.ActiveSkills.Find(s => s != null && s.CanUse(companion) && s.Data.EffectType == SkillEffectTypet.Heal);
            //TODO:         if (lowestRatio < 0.4f && healSkill != null && criticalAlly != null)
            //TODO:         { action.Type = CombatActionTypet.Skill; action.Skill = healSkill; action.Targets.Add(criticalAlly); }
            //TODO:         else
            //TODO:         { action.Type = CombatActionTypet.Attack; action.Targets.Add(enemies[0]); }
            //TODO:         break;
            //TODO:     default:
            //TODO:         action.Type = CombatActionTypet.Attack;
            //TODO:         action.Targets.Add(enemies[0]);
            //TODO:         break;
            //TODO: }
            //TODO: return action;
            var aliveEnemies = Alive(enemies); //Wave0write
            var aliveAllies = Alive(allies); //Wave0write
            if (companion == null || aliveEnemies.Count == 0) //Wave0write
            { //Wave0write
                return null; //Wave0write
            } //Wave0write

            var action = new CombatActiont { Actor = companion, Targets = new List<EntityBaset>(), ConsumesTurn = true }; //Wave0write
            string rule = string.IsNullOrEmpty(companion.ActionRuleKey) ? "Dealer" : companion.ActionRuleKey; //Wave0write
            if (rule == "Tanker" && companion.Stats.CurrentHP <= companion.Stats.MaxHP / 2) //Wave0write
            { //Wave0write
                action.Type = CombatActionTypet.Defend; //Wave0write
                action.Targets.Add(companion); //Wave0write
                return action; //Wave0write
            } //Wave0write

            Skillt skill = PickUsableSkill(companion, rule, aliveAllies, aliveEnemies); //Wave0write
            if (skill != null) //Wave0write
            { //Wave0write
                action.Type = CombatActionTypet.Skill; //Wave0write
                action.Skill = skill; //Wave0write
                FillTargets(action, companion, aliveAllies, aliveEnemies); //Wave0write
                return action; //Wave0write
            } //Wave0write

            action.Type = CombatActionTypet.Attack; //Wave0write
            action.Targets.Add(LowestHp(aliveEnemies)); //Wave0write
            return action; //Wave0write
        }

        private static Skillt PickUsableSkill(TeamBaset companion, string rule, List<EntityBaset> allies, List<EntityBaset> enemies) //Wave0write
        { //Wave0write
            Skillt fallback = null; //Wave0write
            foreach (Skillt skill in companion.ActiveSkills) //Wave0write
            { //Wave0write
                if (skill == null || !skill.CanUse(companion)) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                if (fallback == null) //Wave0write
                { //Wave0write
                    fallback = skill; //Wave0write
                } //Wave0write
                if (rule == "MagicDealer" && enemies.Count >= 2 && skill.Data.TargetType == SkillTargetTypet.EnemyAll) return skill; //Wave0write
                if (rule == "Supporter" && skill.Data.HealScale > 0f && LowestHpRatio(allies) < 0.5f) return skill; //Wave0write
            } //Wave0write

            return fallback; //Wave0write
        } //Wave0write

        private static void FillTargets(CombatActiont action, EntityBaset actor, List<EntityBaset> allies, List<EntityBaset> enemies) //Wave0write
        { //Wave0write
            switch (action.Skill.Data.TargetType) //Wave0write
            { //Wave0write
                case SkillTargetTypet.EnemySingle: action.Targets.Add(LowestHp(enemies)); break; //Wave0write
                case SkillTargetTypet.EnemyAll: action.Targets.AddRange(enemies); break; //Wave0write
                case SkillTargetTypet.AllySingle: action.Targets.Add(LowestHp(allies)); break; //Wave0write
                case SkillTargetTypet.AllyAll: action.Targets.AddRange(allies); break; //Wave0write
                case SkillTargetTypet.Self: action.Targets.Add(actor); break; //Wave0write
            } //Wave0write
        } //Wave0write

        private static EntityBaset LowestHp(List<EntityBaset> entities) //Wave0write
        { //Wave0write
            EntityBaset best = entities[0]; //Wave0write
            foreach (EntityBaset entity in entities) if (entity.Stats.CurrentHP < best.Stats.CurrentHP) best = entity; //Wave0write
            return best; //Wave0write
        } //Wave0write

        private static float LowestHpRatio(List<EntityBaset> entities) //Wave0write
        { //Wave0write
            if (entities == null || entities.Count == 0) return 1f; //Wave0write
            float best = 1f; //Wave0write
            foreach (EntityBaset entity in entities) best = UnityEngine.Mathf.Min(best, (float)entity.Stats.CurrentHP / UnityEngine.Mathf.Max(1, entity.Stats.MaxHP)); //Wave0write
            return best; //Wave0write
        } //Wave0write

        private static List<EntityBaset> Alive(List<EntityBaset> source) //Wave0write
        { //Wave0write
            var result = new List<EntityBaset>(); //Wave0write
            if (source == null) return result; //Wave0write
            foreach (EntityBaset entity in source) if (entity != null && !entity.IsDead) result.Add(entity); //Wave0write
            return result; //Wave0write
        } //Wave0write
    }
}

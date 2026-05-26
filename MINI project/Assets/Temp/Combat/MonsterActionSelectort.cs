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
            //TODO: var candidates = new List<(CombatActionTypet type, Skillt skill)>();
            //TODO: var weights = new List<float>();
            //TODO: // 공격 후보
            //TODO: candidates.Add((CombatActionTypet.Attack, null));
            //TODO: weights.Add(monster.ActionWeights.Attack);
            //TODO: // 스킬 후보 — CanUse 검사
            //TODO: foreach (var skill in monster.ActiveSkills)
            //TODO: {
            //TODO:     if (skill != null && skill.CanUse(monster))
            //TODO:     {
            //TODO:         candidates.Add((CombatActionTypet.Skill, skill));
            //TODO:         weights.Add(monster.ActionWeights.Skill);
            //TODO:     }
            //TODO: }
            //TODO: // 방어 후보
            //TODO: candidates.Add((CombatActionTypet.Defend, null));
            //TODO: weights.Add(monster.ActionWeights.Defend);
            //TODO: int idx = WeightedRandomt.PickIndex(weights);
            //TODO: var chosen = candidates[idx];
            //TODO: var action = new CombatActiont { Actor = monster, Type = chosen.type, Skill = chosen.skill, ConsumesTurn = true };
            //TODO: action.Targets = new List<EntityBaset>();
            //TODO: // 타겟 자동 선택
            //TODO: if (chosen.type == CombatActionTypet.Attack)
            //TODO:     action.Targets.Add(enemies[UnityEngine.Random.Range(0, enemies.Count)]);
            //TODO: else if (chosen.type == CombatActionTypet.Skill && chosen.skill != null)
            //TODO: {
            //TODO:     switch (chosen.skill.Data.TargetType)
            //TODO:     {
            //TODO:         case SkillTargetTypet.EnemySingle: action.Targets.Add(enemies[UnityEngine.Random.Range(0, enemies.Count)]); break;
            //TODO:         case SkillTargetTypet.AllySingle:  action.Targets.Add(allies[UnityEngine.Random.Range(0, allies.Count)]); break;
            //TODO:         case SkillTargetTypet.EnemyAll:    action.Targets.AddRange(enemies); break;
            //TODO:         case SkillTargetTypet.AllyAll:     action.Targets.AddRange(allies); break;
            //TODO:         case SkillTargetTypet.Self:        action.Targets.Add(monster); break;
            //TODO:     }
            //TODO: }
            //TODO: else if (chosen.type == CombatActionTypet.Defend)
            //TODO:     action.Targets.Add(monster);
            //TODO: return action;
            var aliveEnemies = Alive(enemies); //Wave0write
            var aliveAllies = Alive(allies); //Wave0write
            if (monster == null || aliveEnemies.Count == 0) //Wave0write
            { //Wave0write
                return null; //Wave0write
            } //Wave0write

            ActionWeightTablet weights = monster.ActionWeights ?? new ActionWeightTablet { Attack = 80, Skill = 0, Defend = 20 }; //Wave0write
            var candidates = new List<CombatActiont>(); //Wave0write
            var candidateWeights = new List<int>(); //Wave0write

            candidates.Add(new CombatActiont { Actor = monster, Type = CombatActionTypet.Attack, Targets = new List<EntityBaset> { PickRandom(aliveEnemies) }, ConsumesTurn = true }); //Wave0write
            candidateWeights.Add(weights.Attack); //Wave0write

            foreach (Skillt skill in monster.ActiveSkills) //Wave0write
            { //Wave0write
                if (skill != null && skill.CanUse(monster)) //Wave0write
                { //Wave0write
                    candidates.Add(BuildSkillAction(monster, skill, aliveAllies, aliveEnemies)); //Wave0write
                    candidateWeights.Add(weights.Skill); //Wave0write
                } //Wave0write
            } //Wave0write

            candidates.Add(new CombatActiont { Actor = monster, Type = CombatActionTypet.Defend, Targets = new List<EntityBaset> { monster }, ConsumesTurn = true }); //Wave0write
            candidateWeights.Add(weights.Defend); //Wave0write
            int index = WeightedRandomt.PickIndex(candidateWeights); //Wave0write
            return index >= 0 && index < candidates.Count ? candidates[index] : candidates[0]; //Wave0write
        }

        private static CombatActiont BuildSkillAction(MonsterBaset monster, Skillt skill, List<EntityBaset> allies, List<EntityBaset> enemies) //Wave0write
        { //Wave0write
            var action = new CombatActiont { Actor = monster, Type = CombatActionTypet.Skill, Skill = skill, Targets = new List<EntityBaset>(), ConsumesTurn = true }; //Wave0write
            switch (skill.Data.TargetType) //Wave0write
            { //Wave0write
                case SkillTargetTypet.EnemySingle: action.Targets.Add(PickRandom(enemies)); break; //Wave0write
                case SkillTargetTypet.EnemyAll: action.Targets.AddRange(enemies); break; //Wave0write
                case SkillTargetTypet.AllySingle: action.Targets.Add(PickRandom(allies)); break; //Wave0write
                case SkillTargetTypet.AllyAll: action.Targets.AddRange(allies); break; //Wave0write
                case SkillTargetTypet.Self: action.Targets.Add(monster); break; //Wave0write
            } //Wave0write

            return action; //Wave0write
        } //Wave0write

        private static List<EntityBaset> Alive(List<EntityBaset> source) //Wave0write
        { //Wave0write
            var result = new List<EntityBaset>(); //Wave0write
            if (source == null) return result; //Wave0write
            foreach (EntityBaset entity in source) if (entity != null && !entity.IsDead) result.Add(entity); //Wave0write
            return result; //Wave0write
        } //Wave0write

        private static EntityBaset PickRandom(List<EntityBaset> entities) //Wave0write
        { //Wave0write
            return entities == null || entities.Count == 0 ? null : entities[UnityEngine.Random.Range(0, entities.Count)]; //Wave0write
        } //Wave0write
    }
}

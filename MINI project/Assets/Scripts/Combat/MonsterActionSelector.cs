using System.Collections.Generic;

namespace Tempt
{
    // Wave0refactor 2026-05-27 (F.4 + F.8):
    // - Alive / PickRandom 헬퍼 제거 → CombatTargeting 정적 유틸로 위임.
    // - 후보 CombatAction 사전 생성 제거 → Candidate struct + reusable buffer 로 선택 후에만 1개 alloc.
    /// <summary>
    /// 몬스터 1마리의 행동 결정. 가중치 + 사용 가능 조건 + 쿨다운 기반.
    /// </summary>
    public sealed class MonsterActionSelector
    {
        /// <summary>후보 1건의 메타. 실제 CombatAction 은 선택된 후에만 1번 만든다.</summary>
        private struct Candidate
        {
            public CombatActionType Type;
            public Skill Skill;
        }

        private readonly List<Candidate> candidates = new List<Candidate>(4);
        private readonly List<int> weights = new List<int>(4);
        private readonly List<EntityBase> aliveAlliesBuffer = new List<EntityBase>();
        private readonly List<EntityBase> aliveEnemiesBuffer = new List<EntityBase>();

        /// <summary>
        /// 행동 결정. CombatFlow 가 호출.
        /// </summary>
        public CombatAction Pick(MonsterBase monster, List<EntityBase> allies, List<EntityBase> enemies)
        {
            // 동작 요약:
            // - aliveAlliesBuffer / aliveEnemiesBuffer 를 비우고 살아있는 항목으로 채운다(클래스 필드 재사용).
            // - Attack 후보 1, 사용 가능한 Skill 후보 N, Defend 후보 1 을 메타로만 쌓는다.
            // - WeightedRandom.PickIndex 로 인덱스를 하나 뽑은 뒤에만 CombatAction 인스턴스를 만든다.
            // - 타겟은 CombatTargeting.FillByTargetType(strategy: Random) 으로 채운다.
            CombatTargeting.Alive(allies, aliveAlliesBuffer);
            CombatTargeting.Alive(enemies, aliveEnemiesBuffer);
            if (monster == null || aliveEnemiesBuffer.Count == 0)
            {
                return null;
            }

            ActionWeightTable w = monster.ActionWeights ?? new ActionWeightTable { Attack = 80, Skill = 0, Defend = 20 };
            candidates.Clear();
            weights.Clear();

            candidates.Add(new Candidate { Type = CombatActionType.Attack, Skill = null });
            weights.Add(w.Attack);

            if (monster.ActiveSkills != null)
            {
                for (int i = 0; i < monster.ActiveSkills.Length; i++)
                {
                    Skill s = monster.ActiveSkills[i];
                    if (s != null && s.CanUse(monster))
                    {
                        candidates.Add(new Candidate { Type = CombatActionType.Skill, Skill = s });
                        weights.Add(w.Skill);
                    }
                }
            }

            candidates.Add(new Candidate { Type = CombatActionType.Defend, Skill = null });
            weights.Add(w.Defend);

            int picked = WeightedRandom.PickIndex(weights);
            if (picked < 0 || picked >= candidates.Count)
            {
                picked = 0;
            }

            return BuildAction(monster, candidates[picked]);
        }

        private CombatAction BuildAction(MonsterBase monster, Candidate c)
        {
            // 동작 요약:
            // - Attack/Skill/Defend 별로 CombatAction 인스턴스 1번 alloc.
            // - 타겟은 CombatTargeting 의 SingleSelect.Random 전략으로 채운다.
            var action = new CombatAction
            {
                Actor = monster,
                Type = c.Type,
                Skill = c.Skill,
                Targets = new List<EntityBase>(),
                ConsumesTurn = true,
            };

            switch (c.Type)
            {
                case CombatActionType.Attack:
                    EntityBase target = CombatTargeting.RandomAlive(aliveEnemiesBuffer);
                    if (target != null) action.Targets.Add(target);
                    break;
                case CombatActionType.Skill:
                    CombatTargeting.FillByTargetType(
                        action.Targets,
                        c.Skill.Data.TargetType,
                        monster,
                        aliveAlliesBuffer,
                        aliveEnemiesBuffer,
                        CombatTargeting.SingleSelect.Random);
                    break;
                case CombatActionType.Defend:
                    action.Targets.Add(monster);
                    break;
            }

            return action;
        }
    }
}

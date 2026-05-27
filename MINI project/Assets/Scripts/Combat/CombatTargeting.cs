using System.Collections.Generic;

namespace Tempt
{
    // Wave0refactor 2026-05-27 (F.4): MonsterActionSelector / CompanionActionSelector /
    // CombatController 의 Alive / FirstAlive / RandomAlive / LowestHp / LowestHpRatio /
    // FillTargets 헬퍼를 한 곳으로 모은 정적 유틸.
    // 매 라운드 List 신규 할당을 줄이기 위해 reuse 인자를 받는 API 를 함께 제공한다.
    /// <summary>
    /// 전투 타겟 선택 공통 유틸. 정적이므로 인스턴스화하지 않는다.
    /// 호출자는 가능한 한 reuse 리스트(클래스 필드 보유)를 전달해 GC 압력을 줄인다.
    /// </summary>
    public static class CombatTargeting
    {
        /// <summary>
        /// 단일 타겟 선택 전략. EnemySingle / AllySingle 같은 단일 타겟 타입에서만 의미를 가진다.
        /// </summary>
        public enum SingleSelect
        {
            /// <summary>살아있는 후보 중 무작위.</summary>
            Random,

            /// <summary>현재 HP 가 가장 낮은 살아있는 후보.</summary>
            LowestHp,
        }

        /// <summary>
        /// source 중 살아있는 항목만 결과 리스트에 채워서 반환한다.
        /// reuse 가 null 아니면 그 리스트를 비우고 재사용한다.
        /// </summary>
        public static List<EntityBase> Alive(IList<EntityBase> source, List<EntityBase> reuse = null)
        {
            // 동작 요약:
            // - reuse 가 null 이면 새 List 할당.
            // - source 가 null 이면 빈 리스트 반환.
            // - 살아있는(IsDead == false) 항목만 순서대로 추가.
            List<EntityBase> result = reuse ?? new List<EntityBase>();
            result.Clear();
            if (source == null)
            {
                return result;
            }

            for (int i = 0; i < source.Count; i++)
            {
                EntityBase entity = source[i];
                if (entity != null && !entity.IsDead)
                {
                    result.Add(entity);
                }
            }

            return result;
        }

        /// <summary>살아있는 첫 항목. 없으면 null.</summary>
        public static EntityBase FirstAlive(IList<EntityBase> source)
        {
            // 동작 요약: 순차 탐색 후 살아있는 첫 entity 반환.
            if (source == null)
            {
                return null;
            }

            for (int i = 0; i < source.Count; i++)
            {
                EntityBase entity = source[i];
                if (entity != null && !entity.IsDead)
                {
                    return entity;
                }
            }

            return null;
        }

        /// <summary>살아있는 항목 중 무작위 하나. 없으면 null.</summary>
        public static EntityBase RandomAlive(IList<EntityBase> source)
        {
            // 동작 요약:
            // - 살아있는 후보 수를 한 번 세고, 그 안에서 Random.Range 인덱스 선택.
            // - 새 List 할당 없이 두 패스로 처리한다(라운드당 호출 빈도가 높음).
            if (source == null)
            {
                return null;
            }

            int aliveCount = 0;
            for (int i = 0; i < source.Count; i++)
            {
                EntityBase entity = source[i];
                if (entity != null && !entity.IsDead)
                {
                    aliveCount++;
                }
            }

            if (aliveCount == 0)
            {
                return null;
            }

            int pick = UnityEngine.Random.Range(0, aliveCount);
            int seen = 0;
            for (int i = 0; i < source.Count; i++)
            {
                EntityBase entity = source[i];
                if (entity == null || entity.IsDead)
                {
                    continue;
                }

                if (seen == pick)
                {
                    return entity;
                }

                seen++;
            }

            return null;
        }

        /// <summary>현재 HP 가 가장 낮은 살아있는 항목. 없으면 null.</summary>
        public static EntityBase LowestHp(IList<EntityBase> source)
        {
            // 동작 요약: 살아있는 후보를 순회하며 Stats.CurrentHP 가 최소인 항목을 추적.
            if (source == null)
            {
                return null;
            }

            EntityBase best = null;
            int lowest = int.MaxValue;
            for (int i = 0; i < source.Count; i++)
            {
                EntityBase entity = source[i];
                if (entity == null || entity.IsDead || entity.Stats == null)
                {
                    continue;
                }

                if (entity.Stats.CurrentHP < lowest)
                {
                    lowest = entity.Stats.CurrentHP;
                    best = entity;
                }
            }

            return best;
        }

        /// <summary>살아있는 후보 중 현재 HP 비율(0~1) 의 최소값. 없으면 1f.</summary>
        public static float LowestHpRatio(IList<EntityBase> source)
        {
            // 동작 요약: 후보 순회하며 (CurrentHP / max(1, MaxHP)) 최소값 추적.
            if (source == null)
            {
                return 1f;
            }

            float best = 1f;
            for (int i = 0; i < source.Count; i++)
            {
                EntityBase entity = source[i];
                if (entity == null || entity.IsDead || entity.Stats == null)
                {
                    continue;
                }

                int max = UnityEngine.Mathf.Max(1, entity.Stats.MaxHP);
                float ratio = (float)entity.Stats.CurrentHP / max;
                if (ratio < best)
                {
                    best = ratio;
                }
            }

            return best;
        }

        /// <summary>
        /// SkillTargetType 에 따라 targets 리스트를 비우고 채운다.
        /// EnemyAll / AllyAll 은 strategy 무시. Self 는 self 1개 추가.
        /// EnemySingle / AllySingle 은 strategy 에 따라 단일 후보 선택.
        /// </summary>
        public static void FillByTargetType(
            List<EntityBase> targets,
            SkillTargetType type,
            EntityBase self,
            IList<EntityBase> aliveAllies,
            IList<EntityBase> aliveEnemies,
            SingleSelect strategy)
        {
            // 동작 요약:
            // - targets null 이면 즉시 반환. 그 외에는 Clear 후 채운다.
            // - EnemySingle / AllySingle 은 strategy 분기.
            // - EnemyAll / AllyAll 은 살아있는 전부 추가.
            // - Self 는 self 한 명 추가(self == null 이면 추가 안 함).
            if (targets == null)
            {
                return;
            }

            targets.Clear();
            switch (type)
            {
                case SkillTargetType.EnemySingle:
                    {
                        EntityBase pick = strategy == SingleSelect.LowestHp
                            ? LowestHp(aliveEnemies)
                            : RandomAlive(aliveEnemies);
                        if (pick != null) targets.Add(pick);
                        break;
                    }
                case SkillTargetType.AllySingle:
                    {
                        EntityBase pick = strategy == SingleSelect.LowestHp
                            ? LowestHp(aliveAllies)
                            : RandomAlive(aliveAllies);
                        if (pick != null) targets.Add(pick);
                        break;
                    }
                case SkillTargetType.EnemyAll:
                    if (aliveEnemies != null)
                    {
                        for (int i = 0; i < aliveEnemies.Count; i++)
                        {
                            EntityBase e = aliveEnemies[i];
                            if (e != null && !e.IsDead) targets.Add(e);
                        }
                    }
                    break;
                case SkillTargetType.AllyAll:
                    if (aliveAllies != null)
                    {
                        for (int i = 0; i < aliveAllies.Count; i++)
                        {
                            EntityBase a = aliveAllies[i];
                            if (a != null && !a.IsDead) targets.Add(a);
                        }
                    }
                    break;
                case SkillTargetType.Self:
                    if (self != null) targets.Add(self);
                    break;
            }
        }
    }
}

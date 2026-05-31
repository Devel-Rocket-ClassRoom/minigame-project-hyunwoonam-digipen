using System.Collections.Generic;
using System.Linq;

namespace Tempt
{
    /// <summary>
    /// 라운드 단위 행동 순서 큐. 라운드 시작 시 모든 살아있는 참가자가
    /// SPD + 가중치 보정으로 점수를 계산해 정렬하여 큐에 들어간다.
    /// </summary>
    public sealed class ActionQueue
    {
        /// <summary>현재 라운드의 정렬된 행동 순서.</summary>
        public List<TurnEntry> Entries = new List<TurnEntry>();

        /// <summary>다음 행동자 인덱스.</summary>
        public int Cursor;

        /// <summary>
        /// 라운드 시작 시 호출. 모든 참가자의 점수를 계산해 정렬한다.
        /// </summary>
        public void BuildRound(IEnumerable<EntityBase> participants)
        {
            // 동작 요약:
            // - 살아있는 참가자만 필터.
            // - 각자 score = entity.Stats.SPD + WeightedRandom.Sample(-RandomRange, +RandomRange).
            // - score 내림차순 정렬.
            // - Entries 갱신.
            // - Cursor = 0.
            Entries.Clear();
            if (participants == null)
            {
                Cursor = 0;
                return;
            }

            const int randomRange = 5;
            foreach (EntityBase entity in participants)
            {
                if (entity == null || entity.IsDead || entity.Stats == null)
                {
                    continue;
                }

                Entries.Add(new TurnEntry { Actor = entity, Score = entity.Stats.SPD + WeightedRandom.Sample(-randomRange, randomRange) });
            }

            Entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            Cursor = 0;
        }

        /// <summary>
        /// 다음 행동자 반환(없으면 null = 라운드 종료).
        /// </summary>
        public TurnEntry PeekNext()
        {
            // 동작 요약:
            // - Cursor < Entries.Count이면 Entries[Cursor] 반환.
            // - 그 외 null.
            return Cursor >= 0 && Cursor < Entries.Count ? Entries[Cursor] : null;
        }

        /// <summary>
        /// 현재 행동자를 소비하고 커서 전진.
        /// </summary>
        public void ConsumeCurrent()
        {
            // 동작 요약: Cursor += 1.
            Cursor += 1;
        }

        /// <summary>
        /// 라운드 종료 여부.
        /// </summary>
        public bool IsRoundFinished()
        {
            // 동작 요약: Cursor >= Entries.Count 반환.
            return Cursor >= Entries.Count;
        }

        /// <summary>
        /// 큐에서 사망자 즉시 제거(라운드 중 사망자 처리).
        /// </summary>
        public void RemoveDeadEntries()
        {
            // 동작 요약:
            // - Cursor 이후 항목 중 IsDead인 항목 제거.
            // - Cursor 자체는 이미 시작된 행동이므로 보존.
            for (int i = Entries.Count - 1; i > Cursor; i--)
            {
                if (Entries[i].Actor == null || Entries[i].Actor.IsDead)
                {
                    Entries.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>큐 한 항목.</summary>
    public sealed class TurnEntry
    {
        /// <summary>행위자.</summary>
        public EntityBase Actor;

        /// <summary>이번 라운드 점수.</summary>
        public int Score;
    }
}


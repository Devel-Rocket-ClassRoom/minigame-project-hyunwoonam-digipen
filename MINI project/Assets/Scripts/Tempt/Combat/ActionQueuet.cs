using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 라운드 단위 행동 순서 큐. 라운드 시작 시 모든 살아있는 참가자가
    /// SPD + 가중치 보정으로 점수를 계산해 정렬하여 큐에 들어간다.
    /// </summary>
    public sealed class ActionQueuet
    {
        /// <summary>현재 라운드의 정렬된 행동 순서.</summary>
        public List<TurnEntryt> Entries = new List<TurnEntryt>();

        /// <summary>다음 행동자 인덱스.</summary>
        public int Cursor;

        /// <summary>
        /// 라운드 시작 시 호출. 모든 참가자의 점수를 계산해 정렬한다.
        /// </summary>
        public void BuildRound(IEnumerable<EntityBaset> participants)
        {
            // 동작 요약:
            // - 살아있는 참가자만 필터.
            // - 각자 score = entity.Stats.SPD + WeightedRandomt.Sample(-RandomRange, +RandomRange).
            // - score 내림차순 정렬.
            // - Entries 갱신.
            // - Cursor = 0.
        }

        /// <summary>
        /// 다음 행동자 반환(없으면 null = 라운드 종료).
        /// </summary>
        public TurnEntryt PeekNext()
        {
            // 동작 요약:
            // - Cursor < Entries.Count이면 Entries[Cursor] 반환.
            // - 그 외 null.
            return null;
        }

        /// <summary>
        /// 현재 행동자를 소비하고 커서 전진.
        /// </summary>
        public void ConsumeCurrent()
        {
            // 동작 요약: Cursor += 1.
        }

        /// <summary>
        /// 라운드 종료 여부.
        /// </summary>
        public bool IsRoundFinished()
        {
            // 동작 요약: Cursor >= Entries.Count 반환.
            return false;
        }

        /// <summary>
        /// 큐에서 사망자 즉시 제거(라운드 중 사망자 처리).
        /// </summary>
        public void RemoveDeadEntries()
        {
            // 동작 요약:
            // - Cursor 이후 항목 중 IsDead인 항목 제거.
            // - Cursor 자체는 이미 시작된 행동이므로 보존.
        }
    }

    /// <summary>큐 한 항목.</summary>
    public sealed class TurnEntryt
    {
        /// <summary>행위자.</summary>
        public EntityBaset Actor;

        /// <summary>이번 라운드 점수.</summary>
        public int Score;
    }
}

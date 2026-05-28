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
            //TODO: Entries.Clear();
            //TODO: const int RandomRange = 5;
            //TODO: foreach (EntityBase entity in participants)
            //TODO: {
            //TODO:     if (entity.IsDead) continue;
            //TODO:     int score = entity.Stats.SPD + WeightedRandom.Sample(-RandomRange, RandomRange);
            //TODO:     Entries.Add(new TurnEntry { Actor = entity, Score = score });
            //TODO: }
            //TODO: Entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            //TODO: Cursor = 0;
            Entries.Clear(); //Wave0write
            if (participants == null) //Wave0write
            { //Wave0write
                Cursor = 0; //Wave0write
                return; //Wave0write
            } //Wave0write

            const int randomRange = 5; //Wave0write
            foreach (EntityBase entity in participants) //Wave0write
            { //Wave0write
                if (entity == null || entity.IsDead || entity.Stats == null) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                Entries.Add(new TurnEntry { Actor = entity, Score = entity.Stats.SPD + WeightedRandom.Sample(-randomRange, randomRange) }); //Wave0write
            } //Wave0write

            Entries.Sort((a, b) => b.Score.CompareTo(a.Score)); //Wave0write
            Cursor = 0; //Wave0write
        }

        /// <summary>
        /// 다음 행동자 반환(없으면 null = 라운드 종료).
        /// </summary>
        public TurnEntry PeekNext()
        {
            // 동작 요약:
            // - Cursor < Entries.Count이면 Entries[Cursor] 반환.
            // - 그 외 null.
            //TODO: if (Cursor < Entries.Count) return Entries[Cursor];
            //TODO: return null;
            return Cursor >= 0 && Cursor < Entries.Count ? Entries[Cursor] : null; //Wave0write
        }

        /// <summary>
        /// 현재 행동자를 소비하고 커서 전진.
        /// </summary>
        public void ConsumeCurrent()
        {
            // 동작 요약: Cursor += 1.
            //TODO: Cursor += 1;
            Cursor += 1; //Wave0write
        }

        /// <summary>
        /// 라운드 종료 여부.
        /// </summary>
        public bool IsRoundFinished()
        {
            // 동작 요약: Cursor >= Entries.Count 반환.
            //TODO: return Cursor >= Entries.Count;
            return Cursor >= Entries.Count; //Wave0write
        }

        /// <summary>
        /// 큐에서 사망자 즉시 제거(라운드 중 사망자 처리).
        /// </summary>
        public void RemoveDeadEntries()
        {
            // 동작 요약:
            // - Cursor 이후 항목 중 IsDead인 항목 제거.
            // - Cursor 자체는 이미 시작된 행동이므로 보존.
            //TODO: for (int i = Entries.Count - 1; i > Cursor; i--)
            //TODO: {
            //TODO:     if (Entries[i].Actor.IsDead)
            //TODO:         Entries.RemoveAt(i);
            //TODO: }
            for (int i = Entries.Count - 1; i > Cursor; i--) //Wave0write
            { //Wave0write
                if (Entries[i].Actor == null || Entries[i].Actor.IsDead) //Wave0write
                { //Wave0write
                    Entries.RemoveAt(i); //Wave0write
                } //Wave0write
            } //Wave0write
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


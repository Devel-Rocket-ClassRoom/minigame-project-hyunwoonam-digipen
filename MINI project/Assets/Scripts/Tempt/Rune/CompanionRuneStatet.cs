using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 동료 룬 상태. 게임 시작 시 정해진 노드 순서대로 레벨업할 때마다 자동 해금.
    /// </summary>
    public sealed class CompanionRuneStatet
    {
        /// <summary>직업.</summary>
        public RuneClasst ClassId;

        /// <summary>고정 트리.</summary>
        public RuneTreet Tree;

        /// <summary>해금 순서 시퀀스(시작 룬 포함).</summary>
        public List<int> FixedSequence;

        /// <summary>현재까지 해금된 개수(시퀀스 인덱스).</summary>
        public int UnlockedCount;

        /// <summary>
        /// 시작 룬 1개 해금.
        /// </summary>
        public void UnlockStarter()
        {
            // 동작 요약: FixedSequence[0] 해금, UnlockedCount = 1.
        }

        /// <summary>
        /// 다음 노드 자동 해금. 레벨업 시 1회 호출.
        /// </summary>
        public bool UnlockNextNodeIfPossible()
        {
            // 동작 요약:
            // - UnlockedCount < FixedSequence.Count 검사.
            // - 다음 노드 Unlocked = true; UnlockedCount += 1.
            return false;
        }

        /// <summary>
        /// 해금된 룬의 스탯 보정 합산.
        /// </summary>
        public EquipmentStatModt AggregateStatMod()
        {
            // 동작 요약: 해금된 노드들의 Effect.StatMod 합산.
            return null;
        }
    }
}

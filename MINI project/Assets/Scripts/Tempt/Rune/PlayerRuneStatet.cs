using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 플레이어 룬 진행 상태. 사용자가 직접 노드 선택 + 룬 포인트 사용으로 해금.
    /// </summary>
    public sealed class PlayerRuneStatet
    {
        /// <summary>직업.</summary>
        public RuneClasst ClassId;

        /// <summary>전체 트리.</summary>
        public RuneTreet Tree;

        /// <summary>보유 룬 포인트.</summary>
        public int RunePoints;

        /// <summary>해금된 노드 ID 집합.</summary>
        public HashSet<int> UnlockedIds;

        /// <summary>
        /// 시작 룬 1개 해금. ApplyStartingClass 시점 호출.
        /// </summary>
        public void UnlockStarter()
        {
            // 동작 요약: Tree.Starter.Unlocked = true; UnlockedIds.Add(Tree.Starter.Data.Id).
        }

        /// <summary>
        /// 사용자가 노드 1개를 해금 시도. 신전(룬 변경/초기화)과 자동 호출 흐름에서 사용.
        /// </summary>
        public bool TryUnlock(int nodeId)
        {
            // 동작 요약:
            // - Tree.AllNodes[nodeId] 조회.
            // - Tree.CanUnlock(node) 검사.
            // - RunePoints >= node.Data.PointCost 검사.
            // - 성공 시 차감 + Unlocked 표시.
            return false;
        }

        /// <summary>
        /// 신전에서 룬 초기화. 모든 노드 잠금 + 포인트 환급.
        /// </summary>
        public void ResetTree()
        {
            // 동작 요약:
            // - 모든 노드 Unlocked = false.
            // - 시작 룬은 다시 자동 해금.
            // - 환급 정책(전액 환급 / 일부 손실)은 BalanceDatat로.
        }

        /// <summary>
        /// 레벨업 시 포인트 적립.
        /// </summary>
        public void AddRunePoint(int amount)
        {
            // 동작 요약: RunePoints += amount.
        }

        /// <summary>
        /// 현재 해금된 룬의 스탯 보정 합산.
        /// </summary>
        public EquipmentStatModt AggregateStatMod()
        {
            // 동작 요약:
            // - UnlockedIds 순회하며 node.Data.Effect.StatMod 합산.
            return null;
        }
    }
}

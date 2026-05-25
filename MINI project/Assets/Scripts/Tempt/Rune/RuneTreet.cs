using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 한 직업의 룬 트리. 시작 노드 1개 + 자식 분기 다수.
    /// </summary>
    public sealed class RuneTreet
    {
        /// <summary>이 트리의 직업.</summary>
        public RuneClasst ClassId;

        /// <summary>시작 노드.</summary>
        public RuneNodet Starter;

        /// <summary>모든 노드(ID 조회용).</summary>
        public Dictionary<int, RuneNodet> AllNodes;

        /// <summary>
        /// 데이터에서 직업별 트리를 구성한다.
        /// </summary>
        public static RuneTreet BuildFromData(RuneClasst classId, IEnumerable<RuneDatat> allRunes)
        {
            // 동작 요약:
            // - classId에 해당하는 룬만 필터링.
            // - 시작 룬(IsStarter=true) 1개를 Starter로 지정.
            // - 각 RuneDatat.NextNodeIds로 그래프 연결.
            // - 사이클 검사.
            return null;
        }

        /// <summary>
        /// 특정 노드가 해금 가능한지 검사(부모 중 하나 이상 Unlocked).
        /// </summary>
        public bool CanUnlock(RuneNodet node)
        {
            // 동작 요약:
            // - node.Data.IsStarter면 항상 true.
            // - 아니면 AllNodes 순회해 자식이 node인 노드 중 Unlocked가 하나라도 있으면 true.
            return false;
        }
    }
}

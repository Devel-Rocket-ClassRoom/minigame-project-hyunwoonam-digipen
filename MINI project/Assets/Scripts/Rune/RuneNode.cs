using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 룬 트리의 한 노드 런타임 표현. RuneData 참조 + 해금 상태.
    /// </summary>
    public sealed class RuneNode
    {
        /// <summary>정적 데이터.</summary>
        public RuneData Data;

        /// <summary>해금됨 여부.</summary>
        public bool Unlocked;

        /// <summary>이 노드에서 다음으로 갈 수 있는 노드들(트리 구조).</summary>
        public List<RuneNode> Next;
    }
}


using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 룬 트리의 한 노드 런타임 표현. RuneData 참조 + 투자/마스터 상태.
    /// </summary>
    public sealed class RuneNode
    {
        /// <summary>정적 데이터.</summary>
        public RuneData Data;

        /// <summary>마스터됨 여부. PointCost가 0인 루트는 시작 시 true.</summary>
        public bool Unlocked;

        /// <summary>현재 투자된 룬 포인트.</summary>
        public int InvestedPoints;

        public int RequiredPoints => Data != null ? System.Math.Max(0, Data.PointCost) : 0;

        public bool HasInvestment => InvestedPoints > 0 || Unlocked;

        /// <summary>이 노드에서 다음으로 갈 수 있는 노드들(트리 구조).</summary>
        public List<RuneNode> Next;
    }
}

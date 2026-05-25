using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 전체 플로어 맵. 새 게임 시 1회 생성, 같은 런에서 변경되지 않음.
    /// </summary>
    public sealed class FloorMapModelt
    {
        /// <summary>맵 시드.</summary>
        public int Seed;

        /// <summary>층 번호 → 그 층의 노드 목록.</summary>
        public Dictionary<int, List<FloorNodet>> NodesByFloor = new Dictionary<int, List<FloorNodet>>();

        /// <summary>노드 ID → 노드 빠른 조회.</summary>
        public Dictionary<int, FloorNodet> NodesById = new Dictionary<int, FloorNodet>();

        /// <summary>다음 활성 층(플레이어의 현재 위치 다음).</summary>
        public int NextSelectableFloor;

        /// <summary>
        /// 노드 클리어 등록.
        /// </summary>
        public void MarkCleared(int nodeId)
        {
            // 동작 요약:
            // - NodesById[nodeId].IsCleared = true.
            // - 그 층이 마지막 노드(보스)였다면 다음 단계 해금.
            // - NextSelectableFloor 갱신.
        }

        /// <summary>
        /// 특정 층이 클리어됐는가(보스 노드 클리어 기준).
        /// </summary>
        public bool IsStageCleared(int stageIndex)
        {
            // 동작 요약: 해당 stage의 보스 노드.IsCleared 반환.
            return false;
        }
    }
}

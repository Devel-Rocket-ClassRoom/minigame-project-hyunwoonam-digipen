namespace Tempt
{
    /// <summary>
    /// 새 게임 시 전체 플로어 맵 1회 생성. 시드로 결정적 결과 보장.
    /// 튜토리얼 단계는 노드 수 고정(1층=1, 2층=2, 3층=1보스).
    /// 4/12/20/30/40층은 안전지대 진입 층(설계 확정).
    /// </summary>
    public static class FloorMapCreatort
    {
        /// <summary>
        /// 맵 생성.
        /// </summary>
        public static FloorMapModelt Generate(WorldDatat world, int seed)
        {
            // 동작 요약:
            // - new Random(seed) 사용.
            // - 모든 일반 층(world.Stages 순회):
            //   * 튜토리얼 단계는 TutorialNodeCounts에 따라 노드 수 고정.
            //   * 일반은 [MinNodesPerFloor, MaxNodesPerFloor] 무작위.
            //   * 각 노드의 난이도는 stage 범위 내 무작위.
            //   * 보스 층은 노드 1개, IsBoss=true.
            // - 층 간 연결(NextNodeIds): 다음 층의 노드 인덱스를 적절히 분산.
            // - 4/12/20/30/40층은 노드 없이 안전지대 진입 층으로 표시(맵 모델에 placeholder 노드 또는 빈 floor).
            // - 결과 FloorMapModelt 반환.
            return null;
        }
    }
}

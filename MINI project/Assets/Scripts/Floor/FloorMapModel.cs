using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 전체 플로어 맵. 새 게임 시 1회 생성되고, 이후 저장 시 전체 노드 구조가 JSON에 직접 기록된다.
    /// 이어하기는 seed 재생성이 아니라 SaveSnapshot.FloorMap.Nodes를 통해 이 모델을 복원한다.
    /// </summary>
    public sealed class FloorMapModel
    {
        /// <summary>층 번호 → 그 층의 노드 목록.</summary>
        public Dictionary<int, List<FloorNode>> NodesByFloor = new Dictionary<int, List<FloorNode>>();

        /// <summary>노드 ID → 노드 빠른 조회.</summary>
        public Dictionary<int, FloorNode> NodesById = new Dictionary<int, FloorNode>();

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
            //TODO: if (!NodesById.TryGetValue(nodeId, out var node)) return;
            //TODO: node.IsCleared = true;
            //TODO: // 현재 층의 모든 노드가 클리어됐는지 확인(또는 보스 클리어 기준)
            //TODO: if (node.IsBoss)
            //TODO: {
            //TODO:     NextSelectableFloor = node.Floor + 1; // 다음 층 해금
            //TODO: }
            //TODO: else
            //TODO: {
            //TODO:     // 일반 전투 노드: 같은 층에서 다음 선택 가능 노드 없으면 다음 층으로
            //TODO:     bool anyUncleared = NodesByFloor[node.Floor].Exists(n => !n.IsCleared);
            //TODO:     if (!anyUncleared) NextSelectableFloor = node.Floor + 1;
            //TODO: }
            if (!NodesById.TryGetValue(nodeId, out FloorNode node)) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            node.IsCleared = true; //Wave0write
            NextSelectableFloor = FindNextSelectableFloor(node.Floor); //Wave0write
        }

        /// <summary>
        /// 특정 층이 클리어됐는가(보스 노드 클리어 기준).
        /// </summary>
        public bool IsStageCleared(int stageIndex)
        {
            // 동작 요약: 해당 stage의 보스 노드.IsCleared 반환.
            // - 단계별 보스층: 1단계=3, 2단계=11, 3단계=19, 4단계=29, 5단계=39, 6단계=49.
            //TODO: int[] bossFloors = { 0, 3, 11, 19, 29, 39, 49 };
            //TODO: if (stageIndex < 1 || stageIndex >= bossFloors.Length) return false;
            //TODO: int bossFloor = bossFloors[stageIndex];
            //TODO: if (!NodesByFloor.TryGetValue(bossFloor, out var nodes)) return false;
            //TODO: return nodes.Count > 0 && nodes[0].IsBoss && nodes[0].IsCleared;
            int bossFloor = BossFloorOfStage(stageIndex); //Wave0write
            if (bossFloor <= 0 || !NodesByFloor.TryGetValue(bossFloor, out List<FloorNode> nodes)) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            foreach (FloorNode node in nodes) //Wave0write
            { //Wave0write
                if (node.IsBoss && node.IsCleared) //Wave0write
                { //Wave0write
                    return true; //Wave0write
                } //Wave0write
            } //Wave0write

            return false; //Wave0write
        }

        private int FindNextSelectableFloor(int currentFloor) //Wave0write
        { //Wave0write
            int next = int.MaxValue; //Wave0write
            foreach (int floor in NodesByFloor.Keys) //Wave0write
            { //Wave0write
                if (floor <= currentFloor) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                if (NodesByFloor.TryGetValue(floor, out List<FloorNode> nodes)) //Wave0write
                { //Wave0write
                    bool hasUncleared = nodes.Exists(n => !n.IsCleared); //Wave0write
                    if (hasUncleared && floor < next) //Wave0write
                    { //Wave0write
                        next = floor; //Wave0write
                    } //Wave0write
                } //Wave0write
            } //Wave0write

            return next == int.MaxValue ? currentFloor + 1 : next; //Wave0write
        } //Wave0write

        private static int BossFloorOfStage(int stageIndex) //Wave0write
        { //Wave0write
            switch (stageIndex) //Wave0write
            { //Wave0write
                case 1: return 3; //Wave0write
                case 2: return 11; //Wave0write
                case 3: return 19; //Wave0write
                case 4: return 29; //Wave0write
                case 5: return 39; //Wave0write
                case 6: return 49; //Wave0write
                default: return 0; //Wave0write
            } //Wave0write
        } //Wave0write
    }
}


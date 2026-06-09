using System.Collections.Generic;

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
        if (!NodesById.TryGetValue(nodeId, out FloorNode node))
        {
            return;
        }

        if (node.IsSafeZone)
        {
            return;
        }

        node.IsCleared = true;
        NextSelectableFloor = FindNextSelectableFloor(node.Floor);
    }

    public void ResetStageProgression(int stageIndex)
    {
        int firstFloor = int.MaxValue;
        foreach (List<FloorNode> nodes in NodesByFloor.Values)
        {
            if (nodes == null)
            {
                continue;
            }

            foreach (FloorNode node in nodes)
            {
                if (node == null || node.IsSafeZone || node.StageIndex != stageIndex)
                {
                    continue;
                }

                node.IsCleared = false;
                if (node.Floor < firstFloor)
                {
                    firstFloor = node.Floor;
                }
            }
        }

        if (firstFloor != int.MaxValue)
        {
            NextSelectableFloor = firstFloor;
        }
    }

    /// <summary>
    /// 특정 층이 클리어됐는가(보스 노드 클리어 기준).
    /// </summary>
    public bool IsStageCleared(int stageIndex)
    {
        // 동작 요약: 해당 stage의 보스 노드.IsCleared 반환.
        // - 단계별 보스층: 1단계=3, 2단계=11, 3단계=19, 4단계=29, 5단계=39, 6단계=49.
        int bossFloor = BossFloorOfStage(stageIndex);
        if (bossFloor <= 0 || !NodesByFloor.TryGetValue(bossFloor, out List<FloorNode> nodes))
        {
            return false;
        }

        foreach (FloorNode node in nodes)
        {
            if (!node.IsSafeZone && node.IsBoss && node.IsCleared)
            {
                return true;
            }
        }

        return false;
    }

    private int FindNextSelectableFloor(int currentFloor)
    {
        int next = int.MaxValue;
        foreach (int floor in NodesByFloor.Keys)
        {
            if (floor <= currentFloor)
            {
                continue;
            }

            if (NodesByFloor.TryGetValue(floor, out List<FloorNode> nodes))
            {
                bool hasUncleared = nodes.Exists(n => n != null && !n.IsSafeZone && !n.IsCleared);
                if (hasUncleared && floor < next)
                {
                    next = floor;
                }
            }
        }

        return next == int.MaxValue ? currentFloor + 1 : next;
    }

    private static int BossFloorOfStage(int stageIndex)
    {
        switch (stageIndex)
        {
            case 1: return 3;
            case 2: return 11;
            case 3: return 19;
            case 4: return 29;
            case 5: return 39;
            case 6: return 49;
            default: return 0;
        }
    }
}


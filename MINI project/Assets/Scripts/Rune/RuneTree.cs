using System.Collections.Generic;

/// <summary>
/// 한 직업의 룬 트리. 시작 노드 1개 + 자식 분기 다수.
/// </summary>
public sealed class RuneTree
{
    /// <summary>이 트리의 직업.</summary>
    public RuneClass ClassId;

    /// <summary>시작 노드(RequiredRuneId == 0인 노드).</summary>
    public RuneNode Starter;

    /// <summary>모든 노드(ID → RuneNode, 조회용).</summary>
    public Dictionary<int, RuneNode> AllNodes;

    /// <summary>
    /// 데이터에서 직업별 트리를 구성한다.
    /// </summary>
    public static RuneTree BuildFromData(RuneClass classId, IEnumerable<RuneData> allRunes)
    {
        // 동작 요약:
        // - classId에 해당하는 룬 + SubFragment 포함하여 필터링.
        // - RequiredRuneId == 0 인 노드를 Starter로 지정(직업당 1개 보장).
        // - AllNodes를 먼저 Id → RuneNode 로 구성.
        // - 각 RuneData.RequiredRuneId 를 부모 키로 사용해
        //   AllNodes[RequiredRuneId].Next.Add(현재 노드) 로 자식 연결.
        // - 사이클 검사(BFS/DFS).
        var tree = new RuneTree
        {
            ClassId = classId,
            AllNodes = new Dictionary<int, RuneNode>(),
        };

        if (allRunes == null)
        {
            return tree;
        }

        foreach (RuneData data in allRunes)
        {
            if (data.ClassId != classId && data.RuneType != RuneNodeType.SubFragment)
            {
                continue;
            }

            var node = new RuneNode { Data = data, Next = new List<RuneNode>() };
            tree.AllNodes[data.Id] = node;
            if (data.RequiredRuneId == 0 && data.ClassId == classId && tree.Starter == null)
            {
                tree.Starter = node;
            }
        }

        foreach (RuneNode node in tree.AllNodes.Values)
        {
            int parentId = node.Data.RequiredRuneId;
            if (parentId != 0 && tree.AllNodes.TryGetValue(parentId, out RuneNode parent))
            {
                parent.Next.Add(node);
            }
        }

        return tree;
    }

    /// <summary>
    /// 특정 노드에 투자 가능한지 검사.
    /// 시작 노드는 항상 가능. 그 외에는 부모(RequiredRuneId) 노드에 최소 1포인트 이상 투자되어 있어야 한다.
    /// </summary>
    public bool CanUnlock(RuneNode node)
    {
        // 동작 요약:
        // - node.Data.RequiredRuneId == 0 이면 시작 노드 → true.
        // - 아니면 AllNodes[node.Data.RequiredRuneId] 에 투자/마스터 진행이 있는지 반환.
        if (node == null || node.Data == null)
        {
            return false;
        }

        if (node.Data.RequiredRuneId == 0)
        {
            return true;
        }

        return AllNodes != null
            && AllNodes.TryGetValue(node.Data.RequiredRuneId, out RuneNode parent)
            && parent.HasInvestment;
    }
}

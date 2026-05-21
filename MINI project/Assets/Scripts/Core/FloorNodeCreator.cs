using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 던전 노드 생성을 담당하는 임시 시스템입니다.
/// </summary>
/// <remarks>
/// 1주차 데모에서는 일반 전투 2개와 더미 보스 1개로 구성된 3노드 던전만 생성합니다.
/// 이후 층 구조, 안전지대, 침식 규칙은 이 클래스의 생성 규칙으로 확장합니다.
/// </remarks>
public class FloorNodeCreator
{
    private DataManager dataManager;
    private bool isInitialized;
    private readonly List<FloorNodeData> nodes = new List<FloorNodeData>();

    /// <summary>
    /// 던전 생성 시스템 초기화 여부입니다.
    /// </summary>
    public bool IsInitialized => isInitialized;

    /// <summary>
    /// 현재 생성된 층 노드 목록입니다.
    /// </summary>
    public IReadOnlyList<FloorNodeData> Nodes => nodes;

    /// <summary>
    /// 던전 생성 시스템을 초기화합니다.
    /// </summary>
    public void Initialize(DataManager dataManager)
    {
        if (isInitialized)
        {
            return;
        }

        this.dataManager = dataManager;
        isInitialized = true;
        Debug.Log("[DungeonCreator] Initialized.");
    }

    public void GenerateDemoFloorNode()
    {
        nodes.Clear();
        int nextNodeIndex = 0;

        nextNodeIndex = AddNormalFloorNodes(nextNodeIndex, 1);
        nextNodeIndex = AddNormalFloorNodes(nextNodeIndex, 2);

        nodes.Add(
            new FloorNodeData(
                nextNodeIndex,
                3,
                0,
                "3층 임시 보스",
                true,
                false,
                3,
                1
            )
        );

        Debug.Log($"[FloorNodeCreator] Demo floor nodes created: {nodes.Count}");
    }

    public bool HasNodes()
    {
        return nodes.Count > 0;
    }

    public bool TryGetNode(int nodeIndex, out FloorNodeData node)
    {
        node = null;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].NodeIndex == nodeIndex)
            {
                node = nodes[i];
                return true;
            }
        }

        return false;
    }

    public bool MarkNodeCleared(int nodeIndex)
    {
        if (!TryGetNode(nodeIndex, out FloorNodeData node))
        {
            return false;
        }

        node.MarkCleared();
        return true;
    }

    private int AddNormalFloorNodes(int nextNodeIndex, int floor)
    {
        int nodeCount = Random.Range(1, 4);
        for (int slot = 0; slot < nodeCount; slot++)
        {
            int difficulty = Random.Range(1, 4);
            nodes.Add(
                new FloorNodeData(
                    nextNodeIndex,
                    floor,
                    slot,
                    $"{floor}층-{slot + 1}",
                    false,
                    false,
                    difficulty,
                    difficulty
                )
            );
            nextNodeIndex++;
        }

        return nextNodeIndex;
    }
}

/// <summary>
/// FloorMap에서 표시하고 선택할 임시 층 노드 데이터입니다.
/// </summary>
public sealed class FloorNodeData
{
    public FloorNodeData(
        int nodeIndex,
        int floor,
        int nodeSlot,
        string displayName,
        bool isBossNode,
        bool isSafeNode,
        int difficulty,
        int monsterCount
    )
    {
        NodeIndex = nodeIndex;
        Floor = floor;
        NodeSlot = nodeSlot;
        DisplayName = displayName;
        IsBossNode = isBossNode;
        IsSafeNode = isSafeNode;
        Difficulty = Mathf.Clamp(difficulty, 1, 3);
        MonsterCount = Mathf.Max(1, monsterCount);
    }

    public int NodeIndex { get; }
    public int Floor { get; }
    public int NodeSlot { get; }
    public string DisplayName { get; }
    public bool IsBossNode { get; }
    public bool IsSafeNode { get; }
    public int Difficulty { get; }
    public int MonsterCount { get; }
    public bool IsCleared { get; private set; }

    public void MarkCleared()
    {
        IsCleared = true;
    }
}

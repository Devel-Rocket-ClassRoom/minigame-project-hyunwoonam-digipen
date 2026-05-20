using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private readonly ReadOnlyCollection<FloorNodeData> readOnlyNodes;

    public FloorNodeCreator()
    {
        readOnlyNodes = nodes.AsReadOnly();
    }

    /// <summary>
    /// 던전 생성 시스템 초기화 여부입니다.
    /// </summary>
    public bool IsInitialized => isInitialized;

    /// <summary>
    /// 현재 생성된 데모 층 노드 목록입니다.
    /// </summary>
    public IReadOnlyList<FloorNodeData> Nodes => readOnlyNodes;

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
        nodes.Add(new FloorNodeData(0, 1, "1층", false, true));
        nodes.Add(new FloorNodeData(1, 2, "2층", false, true));
        nodes.Add(new FloorNodeData(2, 3, "3층 임시 보스", true, true));

        Debug.Log("[FloorNodeCreator] Demo floor nodes created: 1F, 2F, 3F Boss");
    }

    public bool TryGetNode(int nodeIndex, out FloorNodeData node)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].NodeIndex == nodeIndex)
            {
                node = nodes[i];
                return true;
            }
        }

        node = null;
        return false;
    }
}

/// <summary>
/// 1주차 데모용 던전 층 노드 데이터입니다.
/// </summary>
public sealed class FloorNodeData
{
    public FloorNodeData(int nodeIndex, int floor, string displayName, bool isBossNode, bool isUnlocked)
    {
        NodeIndex = nodeIndex;
        Floor = floor;
        DisplayName = displayName;
        IsBossNode = isBossNode;
        IsUnlocked = isUnlocked;
    }

    public int NodeIndex { get; }

    public int Floor { get; }

    public string DisplayName { get; }

    public bool IsBossNode { get; }

    public bool IsUnlocked { get; }
}

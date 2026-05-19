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

    /// <summary>
    /// 던전 생성 시스템 초기화 여부입니다.
    /// </summary>
    public bool IsInitialized => isInitialized;

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
        Debug.Log("FloorNode Create");
    }

}

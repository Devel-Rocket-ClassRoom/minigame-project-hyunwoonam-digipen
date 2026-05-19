using UnityEngine;

/// <summary>
/// Continue 저장과 비석/묘비 기록을 함께 관리하는 임시 시스템입니다.
/// </summary>
/// <remarks>
/// 실제 파일 입출력은 이후 구현하고, 현재는 GameSystemManager와 연결될 함수 이름과
/// 데이터 흐름만 준비합니다.
/// </remarks>
public class SaveLoader
{
    private bool isInitialized;
    private bool hasContinueData;
    private int savedFloor;
    private int savedNodeIndex;
    private bool savedRunActive;

    /// <summary>
    /// 저장 시스템 초기화 여부입니다.
    /// </summary>
    public bool IsInitialized => isInitialized;

    /// <summary>
    /// Continue 데이터가 존재하는지 여부입니다.
    /// </summary>
    public bool HasContinueData => hasContinueData;

    /// <summary>
    /// 임시 저장된 층입니다.
    /// </summary>
    public int SavedFloor => savedFloor;

    /// <summary>
    /// 임시 저장된 던전 노드 인덱스입니다.
    /// </summary>
    public int SavedNodeIndex => savedNodeIndex;

    /// <summary>
    /// 임시 저장된 도전 진행 여부입니다.
    /// </summary>
    public bool SavedRunActive => savedRunActive;

    /// <summary>
    /// 저장 시스템을 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
        Debug.Log("[SaveLoader] Initialized.");
    }

    /// <summary>
    /// Continue 데이터를 저장하는 임시 함수입니다.
    /// </summary>
    public void SaveContinue(int floor, int nodeIndex, bool isRunActive)
    {
        savedFloor = floor;
        savedNodeIndex = nodeIndex;
        savedRunActive = isRunActive;
        hasContinueData = true;

        Debug.Log($"[SaveLoader] SaveContinue floor={floor}, node={nodeIndex}, active={isRunActive}");
    }

    /// <summary>
    /// Continue 데이터를 불러오는 임시 함수입니다.
    /// </summary>
    public bool TryLoadContinue()
    {
        Debug.Log($"[SaveLoader] TryLoadContinue result={hasContinueData}");
        return hasContinueData;
    }

    /// <summary>
    /// 플레이어 사망 기록을 저장하는 임시 함수입니다.
    /// </summary>
    public void SaveDeathRecord(int floor, int nodeIndex)
    {
        Debug.Log($"[SaveLoader] SaveDeathRecord floor={floor}, node={nodeIndex}");
    }

    /// <summary>
    /// 클리어 기록을 저장하는 임시 함수입니다.
    /// </summary>
    public void SaveClearRecord(int floor, int nodeIndex)
    {
        Debug.Log($"[SaveLoader] SaveClearRecord floor={floor}, node={nodeIndex}");
    }
}

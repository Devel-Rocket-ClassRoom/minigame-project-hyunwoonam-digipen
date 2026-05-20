using UnityEngine;

/// <summary>
/// CSV/JSON 기반 정적 데이터를 관리하는 임시 시스템입니다.
/// </summary>
/// <remarks>
/// 1주차 데모에서는 실제 파싱을 구현하지 않고, 이후 CSV/JSON 로더를 붙일 수 있도록
/// 초기화 순서와 책임 경계만 먼저 고정합니다.
/// </remarks>
public class DataManager
{
    private const string DataTableRootPath = "DataTables";

    private bool isInitialized;

    /// <summary>
    /// 데이터 시스템 초기화 여부입니다.
    /// </summary>
    public bool IsInitialized => isInitialized;

    /// <summary>
    /// 데이터 테이블 루트 경로입니다.
    /// </summary>
    public string RootPath => DataTableRootPath;

    /// <summary>
    /// 정적 데이터 테이블 로드 준비를 수행합니다.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
        Debug.Log("[DataManager] Initialized.");
    }

}

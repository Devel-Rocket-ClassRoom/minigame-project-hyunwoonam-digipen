using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 임시 전투 결과 타입입니다.
/// </summary>
public enum DemoCombatResult
{
    None,
    Victory,
    Defeat,
    Retreat
}

public class GameSystemManager : Singleton<GameSystemManager>
{
    [SerializeField]
    private GameSceneId startScene = GameSceneId.MainMenu;

    [SerializeField]
    private bool enableDebugSceneHotkeys = true;

    private DataManager dataManager;
    private SaveLoader saveLoader;
    private FloorNodeCreator floorNodeCreator;
    private GameSceneManager gameSceneManager;

    private bool isInitialized;
    private bool isRunActive;
    private int currentFloor;
    private int currentDungeonNodeIndex;
    private DemoCombatResult lastCombatResult;

    /// <summary>
    /// CSV/JSON 기반 정적 데이터 관리 시스템입니다.
    /// </summary>
    public DataManager DataManager => dataManager;

    /// <summary>
    /// Continue 저장과 기록 저장을 담당하는 시스템입니다.
    /// </summary>
    public SaveLoader SaveLoader => saveLoader;

    /// <summary>
    /// 층계 노드 생성을 담당하는 시스템입니다.
    /// </summary>
    public FloorNodeCreator FloorNodeCreator => floorNodeCreator;

    /// <summary>
    /// Unity 씬 전환 FSM 역할을 준비하는 임시 씬 매니저입니다.
    /// </summary>
    public GameSceneManager GameSceneManager => gameSceneManager;

    /// <summary>
    /// 현재 데모 도전이 진행 중인지 여부입니다.
    /// </summary>
    public bool IsRunActive => isRunActive;

    /// <summary>
    /// 현재 데모 층입니다.
    /// </summary>
    public int CurrentFloor => currentFloor;

    /// <summary>
    /// 현재 던전 노드 인덱스입니다.
    /// </summary>
    public int CurrentDungeonNodeIndex => currentDungeonNodeIndex;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        InitializeSystems();
    }

    protected override void OnDestroy()
    {
        if (Instance == this)
        {
            ShutdownSystems();
        }

        base.OnDestroy();
    }

    /// <summary>
    /// Unity Start 단계에서 선택적으로 초기 씬을 로드합니다.
    /// </summary>
    private void Start()
    {
        LoadScene(startScene);
    }

    /// <summary>
    /// Play Mode 임시 테스트용 씬 전환 단축키를 처리합니다.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GoToMainMenu();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            GoToSafeZone();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            GoToBattle();
        }
    }

    /// <summary>
    /// 하위 시스템을 생성하고 초기화합니다.
    /// </summary>
    public void InitializeSystems()
    {
        if (isInitialized)
        {
            return;
        }

        dataManager = new DataManager();
        saveLoader = new SaveLoader();
        floorNodeCreator = new FloorNodeCreator();
        gameSceneManager = new GameSceneManager();

        dataManager.Initialize();
        saveLoader.Initialize();
        floorNodeCreator.Initialize(dataManager);
        gameSceneManager.Initialize(GameSceneId.None);

        isInitialized = true;
    }

    /// <summary>
    /// 시스템 종료 시점에 필요한 정리 작업을 수행합니다.
    /// </summary>
    public void ShutdownSystems()
    {
        isInitialized = false;
        isRunActive = false;
    }

    /// <summary>
    /// 새 게임 데모 흐름을 시작합니다.
    /// </summary>
    public void StartNewGame()
    {
        InitializeSystems();

        isRunActive = true;
        currentFloor = 0;
        currentDungeonNodeIndex = 0;
        lastCombatResult = DemoCombatResult.None;

        floorNodeCreator.GenerateDemoFloorNode();
        GoToSafeZone();
    }

    /// <summary>
    /// Continue 데이터를 불러오는 임시 진입점입니다.
    /// </summary>
    public void ContinueGame()
    {
        InitializeSystems();

        if (saveLoader.TryLoadContinue())
        {
            isRunActive = true;
            GoToSafeZone();
            return;
        }

        StartNewGame();
    }

    /// <summary>
    /// 현재 데모 진행 상황을 저장하는 임시 진입점입니다.
    /// </summary>
    public void SaveCurrentGame()
    {

    }

    /// <summary>
    /// MainMenu 씬으로 이동합니다.
    /// </summary>
    public void GoToMainMenu()
    {
        LoadScene(GameSceneId.MainMenu);
    }

    /// <summary>
    /// Safe0 씬으로 이동합니다.
    /// </summary>
    public void GoToSafeZone()
    {
        LoadScene(GameSceneId.Safe0);
    }

    /// <summary>
    /// Battle 씬으로 이동합니다.
    /// </summary>
    public void GoToBattle()
    {
        LoadScene(GameSceneId.Combat);
    }

    /// <summary>
    /// 던전 노드의 전투를 시작합니다.
    /// </summary>
    public void StartCombatNode(int nodeIndex)
    {
        InitializeSystems();

        currentDungeonNodeIndex = nodeIndex;
        GoToBattle();
    }

    /// <summary>
    /// 전투 결과를 받아 다음 임시 흐름으로 이동합니다.
    /// </summary>
    public void EndCombat(DemoCombatResult result)
    {

    }

    /// <summary>
    /// 지정한 게임 씬으로 이동합니다.
    /// </summary>
    public void LoadScene(GameSceneId sceneId)
    {
        InitializeSystems();
        gameSceneManager.LoadScene(sceneId);
    }

}

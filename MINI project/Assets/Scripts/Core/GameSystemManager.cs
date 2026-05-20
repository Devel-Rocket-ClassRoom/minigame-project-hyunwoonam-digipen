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
    Retreat,
}

public class GameSystemManager : Singleton<GameSystemManager>
{
    [SerializeField]
    private GameSceneId startScene = GameSceneId.MainMenu;

    private DataManager dataManager;
    private SaveLoader saveLoader;
    private FloorNodeCreator floorNodeCreator;
    private GameSceneManager gameSceneManager;

    private bool isInitialized;
    private bool isRunActive;
    private int currentFloor;
    private int currentDungeonNodeIndex;
    private DemoCombatResult lastCombatResult;
    private bool hasPlayerCombatState;
    private int playerCurrentHP;
    private int playerCurrentMP;

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
    /// Unity 씬 전환을 담당하는 씬 매니저입니다. 모든 씬 전환은 이 객체를 통해서만 수행한다.
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

    /// <summary>
    /// 마지막으로 전달받은 전투 결과입니다.
    /// </summary>
    public DemoCombatResult LastCombatResult => lastCombatResult;

    /// <summary>
    /// 현재 도전에서 이어갈 Player HP가 기록되어 있는지 여부입니다.
    /// </summary>
    public bool HasPlayerCombatState => hasPlayerCombatState;

    /// <summary>
    /// 현재 도전에서 이어갈 Player HP입니다.
    /// </summary>
    public int PlayerCurrentHP => playerCurrentHP;

    /// <summary>
    /// 현재 도전에서 이어갈 Player MP입니다.
    /// </summary>
    public int PlayerCurrentMP => playerCurrentMP;

    protected override void Awake()
    {
        // TODO:
        // - 목표: Singleton 인스턴스를 확정하고 하위 시스템을 초기화한다.
        // - 의도: 모든 씬에서 GameSystemManager가 게임 진행의 단일 진입점으로 동작하게 한다.
        // - 구현해야 할 것: base.Awake() 호출, 중복 인스턴스 방어, InitializeSystems() 호출을 순서대로 수행한다.

        base.Awake();

        if (Instance != this)
        {
            return;
        }

        InitializeSystems();
    }

    protected override void OnDestroy()
    {
        // TODO:
        // - 목표: Singleton 인스턴스가 파괴될 때 하위 시스템 상태를 정리한다.
        // - 의도: Play Mode 종료나 씬 오브젝트 파괴 시 정리 흐름을 명확히 한다.
        // - 구현해야 할 것: 현재 인스턴스일 때 ShutdownSystems()를 호출하고 base.OnDestroy()로 Singleton 상태를 정리한다.

        if (IsSingletonInstance)
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
        // TODO:
        // - 목표: 게임 시작 시 설정된 시작 씬으로 진입한다.
        // - 의도: Editor Play Mode에서 MainMenu 등 지정된 진입 씬을 자동 로드한다.
        // - 구현해야 할 것: startScene 값을 GameSceneManager.LoadScene 경로로 전달한다. None이면 자동 진입을 생략한다.

        if (startScene == GameSceneId.None)
        {
            return;
        }

        gameSceneManager.LoadScene(startScene);
    }

    /// <summary>
    /// Play Mode 임시 테스트용 씬 전환 단축키를 처리합니다.
    /// </summary>
    private void Update()
    {
        // TODO:
        // - 목표: Play Mode 테스트 중 키 입력으로 주요 씬 전환을 빠르게 확인한다.
        // - 의도: UI 연결 전에도 MainMenu/Safe0/FloorMap/Combat 이동을 GameSceneManager 경로로 수동 검증할 수 있게 한다.
        // - 구현해야 할 것: Q/W/R/E 키에 각각 gameSceneManager 호출을 연결한다.

        if (Input.GetKeyDown(KeyCode.Q))
        {
            gameSceneManager.LoadMainMenu();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            gameSceneManager.LoadSafe0();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            EnterFloorMap();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            gameSceneManager.LoadCombat();
        }
    }

    /// <summary>
    /// 하위 시스템을 생성하고 초기화합니다.
    /// </summary>
    public void InitializeSystems()
    {
        // TODO:
        // - 목표: DataManager, SaveLoader, FloorNodeCreator, GameSceneManager를 생성하고 초기화한다.
        // - 의도: GameSystemManager만 Singleton으로 두고 하위 시스템 소유권을 집중한다.
        // - 구현해야 할 것: 중복 초기화를 막고 각 시스템의 Initialize를 의존 순서에 맞춰 호출한 뒤 isInitialized를 true로 설정한다.

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
        // TODO:
        // - 목표: 런타임 시스템 상태를 종료 상태로 되돌린다.
        // - 의도: Play Mode 종료나 GameSystemManager 파괴 후 다음 실행에 이전 상태가 남지 않게 한다.
        // - 구현해야 할 것: 초기화 여부와 도전 진행 상태를 false로 초기화하고 필요한 하위 시스템 정리를 추가한다.

        isInitialized = false;
        isRunActive = false;
    }

    /// <summary>
    /// 새 게임 데모 흐름을 시작합니다.
    /// </summary>
    public void StartNewGame()
    {
        // TODO:
        // - 목표: 새 도전의 초기 상태를 만들고 Safe0으로 진입한다.
        // - 의도: MainMenu의 New Game 입력이 1주차 수직 슬라이스 시작점으로 연결되게 한다.
        // - 전제: 하위 시스템 초기화는 Awake의 InitializeSystems()에서 한 번만 수행된다.
        // - 구현해야 할 것: 런 상태/층/노드/전투 결과 초기화, 데모 노드 생성, Safe0 씬 이동을 수행한다.

        isRunActive = true;
        currentFloor = 0;
        currentDungeonNodeIndex = 0;
        lastCombatResult = DemoCombatResult.None;
        ClearPlayerCombatState();

        floorNodeCreator.GenerateDemoFloorNode();
        gameSceneManager.LoadSafe0();
    }

    /// <summary>
    /// Continue 데이터를 불러오는 임시 진입점입니다.
    /// </summary>
    public void ContinueGame()
    {
        // TODO:
        // - 목표: 저장된 Continue 데이터가 있으면 복원하고 없으면 새 게임 흐름으로 대체한다.
        // - 의도: MainMenu의 Continue 입력을 현재 SaveLoader 정책과 연결한다.
        // - 전제: 하위 시스템 초기화는 Awake의 InitializeSystems()에서 한 번만 수행된다.
        // - 구현해야 할 것: TryLoadContinue 결과 처리, 복원 성공 시 저장된 상태 적용, 실패 시 StartNewGame 호출.

        if (saveLoader.TryLoadContinue())
        {
            isRunActive = true;
            gameSceneManager.LoadSafe0();
            return;
        }

        StartNewGame();
    }

    /// <summary>
    /// 현재 데모 노드를 준비하고 FloorMap으로 이동합니다.
    /// </summary>
    public void EnterFloorMap()
    {
        // TODO:
        // - 목표: Safe0에서 던전 노드 맵으로 이동하는 임시 진입점을 제공한다.
        // - 의도: FloorMap 씬이 노드 UI를 보여주고 선택 결과를 StartCombatNode로 돌려보내게 한다.
        // - 구현해야 할 것: 런 상태와 데모 노드 목록을 보장한 뒤 FloorMap 씬을 로드한다.
        if (!isRunActive)
        {
            isRunActive = true;
            currentFloor = 0;
        }

        if (floorNodeCreator.Nodes.Count == 0)
        {
            floorNodeCreator.GenerateDemoFloorNode();
        }

        gameSceneManager.LoadFloorNode();
    }

    /// <summary>
    /// 현재 데모 진행 상황을 저장하는 임시 진입점입니다.
    /// </summary>
    public void SaveContinueData()
    {
        // TODO:
        // - 목표: 현재 도전 상태를 Continue 저장 데이터로 기록한다.
        // - 의도: HANDOFF3의 전체 시점 복원 정책을 SaveLoader와 연결한다.
        // - 한계: 현재 SaveLoader.SaveContinue는 floor/node/run 3필드만 받는다.
        //   전투 런타임 복원은 SaveLoader 인터페이스 확장 후 다시 채운다.
        // - 구현해야 할 것: 현재 층, 노드, 진행 여부를 SaveLoader.SaveContinue로 전달한다.

        Debug.Log("SaveContinue");
    }

    public void RecordPlayerCombatState(Player player)
    {
        // TODO:
        // - 목표: CombatFlow가 전투 종료 직전 Player의 현재 HP/MP를 현재 도전 상태에 기록한다.
        // - 의도: Combat 씬이 다시 로드되어 Player 오브젝트가 새로 생겨도 체력과 MP가 다음 전투로 이어지게 한다.
        // - 한계: 현재는 메모리 기반 런타임 상태이며, SaveLoader 전체 스냅샷 구현 시 저장 계층으로 이동한다.
        if (player == null)
        {
            Debug.LogWarning(
                "[GameSystemManager] Cannot record Player combat state because player is null."
            );
            return;
        }

        hasPlayerCombatState = true;
        playerCurrentHP = player.CurrentHP;
        playerCurrentMP = player.CurrentMP;
        Debug.Log(
            $"[GameSystemManager] Player combat state recorded. HP={playerCurrentHP}, MP={playerCurrentMP}"
        );
    }

    public bool TryApplyPlayerCombatState(Player player)
    {
        // TODO:
        // - 목표: 현재 도전에 저장된 Player HP/MP가 있으면 Combat 씬의 Player에 복원한다.
        // - 의도: 전투 시작마다 Player가 최대 HP로 회복되는 흐름을 막고, Safe0/FloorNode/Combat 사이 상태를 이어간다.
        // - 한계: 새 게임 직후처럼 기록이 없으면 Player의 씬/초기값을 그대로 사용한다.
        if (!hasPlayerCombatState)
        {
            return false;
        }

        if (player == null)
        {
            Debug.LogWarning(
                "[GameSystemManager] Cannot apply Player combat state because player is null."
            );
            return false;
        }

        player.SetCurrentResources(playerCurrentHP, playerCurrentMP);
        Debug.Log(
            $"[GameSystemManager] Player combat state applied. HP={player.CurrentHP}, MP={player.CurrentMP}"
        );
        return true;
    }

    private void ClearPlayerCombatState()
    {
        // TODO:
        // - 목표: 새 도전 시작 시 이전 도전의 Player HP/MP 스냅샷을 제거한다.
        // - 의도: New Game이 이전 전투의 체력 상태를 이어받지 않게 한다.
        hasPlayerCombatState = false;
        playerCurrentHP = 0;
        playerCurrentMP = 0;
    }

    /// <summary>
    /// 던전 노드의 전투를 시작합니다.
    /// </summary>
    public void StartCombatNode(int nodeIndex)
    {
        // 기존 구현:
        // InitializeSystems();
        //
        // currentDungeonNodeIndex = nodeIndex;
        // GoToBattle();

        // TODO:
        // - 목표: 선택한 FloorNode 정보를 기록하고 Combat 씬으로 진입한다.
        // - 의도: FloorNodeController가 노드 선택 결과를 GameSystemManager에 전달하게 한다.
        // - 구현해야 할 것: 시스템 초기화, currentDungeonNodeIndex 저장, CombatContext 설정, Combat 씬 이동을 수행한다.
        if (floorNodeCreator.Nodes.Count == 0)
        {
            floorNodeCreator.GenerateDemoFloorNode();
        }

        if (!floorNodeCreator.TryGetNode(nodeIndex, out FloorNodeData node))
        {
            Debug.LogWarning($"[GameSystemManager] Unknown FloorNode index: {nodeIndex}");
            return;
        }

        isRunActive = true;
        currentDungeonNodeIndex = node.NodeIndex;
        currentFloor = node.Floor;
        lastCombatResult = DemoCombatResult.None;

        Debug.Log(
            $"[GameSystemManager] StartCombatNode index={node.NodeIndex}, floor={node.Floor}, boss={node.IsBossNode}"
        );
        gameSceneManager.LoadCombat();
    }

    /// <summary>
    /// 전투 결과를 받아 다음 임시 흐름으로 이동합니다.
    /// </summary>
    public void EndCombat(DemoCombatResult result)
    {
        // 기존 구현:
        // lastCombatResult = result;
        // Debug.Log($"[GameSystemManager] EndCombat result={lastCombatResult}");

        // TODO:
        // - 목표: CombatFlow에서 전달한 전투 결과를 저장하고 다음 씬 흐름을 결정한다.
        // - 의도: CombatFlow가 직접 씬을 로드하지 않고 GameSystemManager에 결과만 보고하게 한다.
        // - 구현해야 할 것: lastCombatResult 저장, Defeat/Victory/Retreat 분기, CombatContext 기반 MainMenu/Safe/FloorNode 전환을 구현한다.
        lastCombatResult = result;
        Debug.Log($"[GameSystemManager] EndCombat result={lastCombatResult}");

        switch (result)
        {
            case DemoCombatResult.Victory:
                Debug.Log("[GameSystemManager] Victory -> Safe0 (Week 1 fallback)");
                gameSceneManager.LoadSafe0();
                break;
            case DemoCombatResult.Defeat:
                isRunActive = false;
                Debug.Log("[GameSystemManager] Defeat -> MainMenu");
                gameSceneManager.LoadMainMenu();
                break;
            case DemoCombatResult.Retreat:
                Debug.Log("[GameSystemManager] Retreat -> Safe0");
                gameSceneManager.LoadSafe0();
                break;
            default:
                Debug.LogWarning($"[GameSystemManager] Unsupported combat result: {result}");
                break;
        }
    }
}

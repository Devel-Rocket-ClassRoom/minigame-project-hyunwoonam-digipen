using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

#pragma warning disable CS0649

/// <summary>
/// Unity 씬 전환을 담당하는 임시 클래스입니다.
/// </summary>
/// <remarks>
/// Unity 기본 SceneManager와 이름 충돌을 피하기 위해 실제 호출은 별칭인
/// UnitySceneManager를 사용합니다. 현재는 동기 LoadScene만 사용하고, 이후
/// Loading/Enter/Exit 단계 확장합니다.
/// </remarks>
public class GameSceneManager
{
    private GameSceneId currentScene;
    private GameSceneId previousScene;
    private GameSceneTransitionState transitionState;
    private string lastLoadedSceneName;

    /// <summary>
    /// 현재 게임 씬 ID입니다.
    /// </summary>
    public GameSceneId CurrentScene => currentScene;

    /// <summary>
    /// 직전 게임 씬 ID입니다.
    /// </summary>
    public GameSceneId PreviousScene => previousScene;

    /// <summary>
    /// 현재 씬 전환 상태입니다.
    /// </summary>
    public GameSceneTransitionState TransitionState => transitionState;

    /// <summary>
    /// 마지막으로 로드 요청한 Unity 씬 이름입니다.
    /// </summary>
    public string LastLoadedSceneName => lastLoadedSceneName;

    /// <summary>
    /// 씬 매니저를 초기화합니다.
    /// </summary>
    public void Initialize(GameSceneId initialScene)
    {
        // 기존 구현:
        // currentScene = initialScene;
        // previousScene = GameSceneId.None;
        // transitionState = GameSceneTransitionState.Idle;
        // lastLoadedSceneName = string.Empty;

        // TODO:
        // - 목표: 씬 매니저의 현재/이전 씬과 전환 상태를 초기화한다.
        // - 의도: GameSystemManager 초기화 시 씬 FSM의 기준 상태를 명확히 한다.
        // - 구현해야 할 것: currentScene, previousScene, transitionState, lastLoadedSceneName을 초기값으로 설정한다.
    }

    /// <summary>
    /// 지정한 게임 씬으로 전환합니다.
    /// </summary>
    public void LoadScene(GameSceneId sceneId)
    {
        // 기존 구현:
        // string sceneName = GetUnitySceneName(sceneId);
        //
        // if (string.IsNullOrEmpty(sceneName))
        // {
        //     transitionState = GameSceneTransitionState.Failed;
        //     Debug.LogWarning($"[GameSceneManager] Unknown scene id: {sceneId}");
        //     return;
        // }
        //
        // previousScene = currentScene;
        // currentScene = sceneId;
        // lastLoadedSceneName = sceneName;
        // transitionState = GameSceneTransitionState.Loading;
        //
        // Debug.Log($"[GameSceneManager] LoadScene {sceneName}");
        // UnitySceneManager.LoadScene(sceneName);
        //
        // transitionState = GameSceneTransitionState.Loaded;

        // TODO:
        // - 목표: GameSceneId를 Unity 씬 이름으로 변환해 씬을 로드한다.
        // - 의도: UnityEngine.SceneManagement.SceneManager 직접 호출을 이 클래스 안으로 제한한다.
        // - 구현해야 할 것: 씬 이름 검증, current/previous 상태 갱신, Loading/Loaded/Failed 상태 전환, UnitySceneManager.LoadScene 호출을 수행한다.
    }

    /// <summary>
    /// MainMenu 씬으로 이동합니다.
    /// </summary>
    public void LoadMainMenu()
    {
        // 기존 구현:
        // LoadScene(GameSceneId.MainMenu);

        // TODO:
        // - 목표: MainMenu 씬 전환을 명시적 API로 제공한다.
        // - 의도: 호출자가 GameSceneId를 직접 다루지 않아도 주요 씬으로 이동할 수 있게 한다.
        // - 구현해야 할 것: LoadScene(GameSceneId.MainMenu)를 호출한다.
    }

    /// <summary>
    /// Safe0 씬으로 이동합니다.
    /// </summary>
    public void LoadSafe0()
    {
        // 기존 구현:
        // LoadScene(GameSceneId.Safe0);

        // TODO:
        // - 목표: Safe0 씬 전환을 명시적 API로 제공한다.
        // - 의도: Week 1 안전지대 이동 코드를 간단하게 호출하게 한다.
        // - 구현해야 할 것: LoadScene(GameSceneId.Safe0)를 호출한다.
    }

    /// <summary>
    /// Combat 씬으로 이동합니다.
    /// </summary>
    public void LoadCombat()
    {
        // 기존 구현:
        // LoadScene(GameSceneId.Combat);

        // TODO:
        // - 목표: Combat 씬 전환을 명시적 API로 제공한다.
        // - 의도: 전투 진입 코드가 Unity 씬 이름 문자열에 직접 의존하지 않게 한다.
        // - 구현해야 할 것: LoadScene(GameSceneId.Combat)을 호출한다.
    }

    /// <summary>
    /// 게임 씬 ID에 대응하는 Unity 씬 이름을 반환합니다.
    /// </summary>
    public string GetUnitySceneName(GameSceneId sceneId)
    {
        // 기존 구현:
        // switch (sceneId)
        // {
        //     case GameSceneId.MainMenu:
        //         return "MainMenu";
        //     case GameSceneId.Safe0:
        //         return "Safe0";
        //     case GameSceneId.Combat:
        //         return "Combat";
        //     default:
        //         return string.Empty;
        // }

        // TODO:
        // - 목표: GameSceneId enum을 실제 Unity 씬 이름 문자열로 변환한다.
        // - 의도: 씬 이름 오타와 Unity 기본 SceneManager 직접 의존을 줄인다.
        // - 구현해야 할 것: MainMenu/Safe0/Combat/FloorNode 등 등록된 씬 ID를 문자열로 매핑하고 알 수 없는 값은 빈 문자열로 처리한다.
        return default;
    }
}

/// <summary>
/// 게임에서 직접 관리하는 씬 ID입니다.
/// </summary>
public enum GameSceneId
{
    None,
    MainMenu,
    Safe0,
    Combat
}

/// <summary>
/// 씬 전환 임시 상태입니다.
/// </summary>
public enum GameSceneTransitionState
{
    Idle,
    Loading,
    Loaded,
    Failed
}

#pragma warning restore CS0649

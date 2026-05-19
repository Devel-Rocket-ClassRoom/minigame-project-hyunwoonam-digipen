using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

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
        currentScene = initialScene;
        previousScene = GameSceneId.None;
        transitionState = GameSceneTransitionState.Idle;
        lastLoadedSceneName = string.Empty;
    }

    /// <summary>
    /// 지정한 게임 씬으로 전환합니다.
    /// </summary>
    public void LoadScene(GameSceneId sceneId)
    {
        string sceneName = GetUnitySceneName(sceneId);

        if (string.IsNullOrEmpty(sceneName))
        {
            transitionState = GameSceneTransitionState.Failed;
            Debug.LogWarning($"[GameSceneManager] Unknown scene id: {sceneId}");
            return;
        }

        previousScene = currentScene;
        currentScene = sceneId;
        lastLoadedSceneName = sceneName;
        transitionState = GameSceneTransitionState.Loading;

        Debug.Log($"[GameSceneManager] LoadScene {sceneName}");
        UnitySceneManager.LoadScene(sceneName);

        transitionState = GameSceneTransitionState.Loaded;
    }

    /// <summary>
    /// MainMenu 씬으로 이동합니다.
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene(GameSceneId.MainMenu);
    }

    /// <summary>
    /// Safe0 씬으로 이동합니다.
    /// </summary>
    public void LoadSafe0()
    {
        LoadScene(GameSceneId.Safe0);
    }

    /// <summary>
    /// Combat 씬으로 이동합니다.
    /// </summary>
    public void LoadCombat()
    {
        LoadScene(GameSceneId.Combat);
    }

    /// <summary>
    /// 게임 씬 ID에 대응하는 Unity 씬 이름을 반환합니다.
    /// </summary>
    public string GetUnitySceneName(GameSceneId sceneId)
    {
        switch (sceneId)
        {
            case GameSceneId.MainMenu:
                return "MainMenu";
            case GameSceneId.Safe0:
                return "Safe0";
            case GameSceneId.Combat:
                return "Combat";
            default:
                return string.Empty;
        }
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

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

/// <summary>
/// MainMenu 씬의 New Game / Continue / Quit 버튼을 런타임에 GameSystemManager에 연결합니다.
/// </summary>
/// <remarks>
/// 이 컨트롤러는 SafeZoneController와 동일한 패턴을 사용합니다.
/// MainMenu 씬에 GameSystemManager 컴포넌트를 직접 두고 인스펙터 OnClick으로 연결하면
/// DontDestroyOnLoad와 Singleton 중복 인스턴스 검사가 충돌하여 씬 재로드 시 참조가 깨집니다.
/// 이를 피하기 위해 버튼을 이름으로 찾아 onClick에 GameSystemManager.Instance 호출을 직접 등록합니다.
/// </remarks>
public class MainMenuController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const string NewGameButtonName = "NewGameButton";
    private const string ContinueButtonName = "ContinueButton";
    private const string QuitButtonName = "QuitButton";

    private Button newGameButton;
    private Button continueButton;
    private Button quitButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoaded()
    {
        UnitySceneManager.sceneLoaded -= OnSceneLoaded;
        UnitySceneManager.sceneLoaded += OnSceneLoaded;
        EnsureController(UnitySceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureController(scene);
    }

    private static void EnsureController(Scene scene)
    {
        if (scene.name != MainMenuSceneName)
        {
            return;
        }

        if (FindAnyObjectByType<MainMenuController>() != null)
        {
            return;
        }

        GameObject controller = new GameObject("MainMenuController");
        controller.AddComponent<MainMenuController>();
    }

    private void Start()
    {
        BindNewGameButton();
        BindContinueButton();
        BindQuitButton();
    }

    private void BindNewGameButton()
    {
        newGameButton = FindButton(NewGameButtonName);
        if (newGameButton == null)
        {
            return;
        }

        newGameButton.onClick.RemoveListener(OnNewGameClicked);
        newGameButton.onClick.AddListener(OnNewGameClicked);
        Debug.Log("[MainMenuController] NewGameButton bound to StartNewGame.");
    }

    private void BindContinueButton()
    {
        continueButton = FindButton(ContinueButtonName);
        if (continueButton == null)
        {
            return;
        }

        continueButton.onClick.RemoveListener(OnContinueClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
        Debug.Log("[MainMenuController] ContinueButton bound to ContinueGame.");
    }

    private void BindQuitButton()
    {
        quitButton = FindButton(QuitButtonName);
        if (quitButton == null)
        {
            return;
        }

        quitButton.onClick.RemoveListener(OnQuitClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        Debug.Log("[MainMenuController] QuitButton bound to Application.Quit.");
    }

    private Button FindButton(string buttonName)
    {
        GameObject buttonObject = GameObject.Find(buttonName);
        if (buttonObject == null)
        {
            Debug.LogWarning($"[MainMenuController] MainMenu button was not found: {buttonName}");
            return null;
        }

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning(
                $"[MainMenuController] MainMenu object has no Button component: {buttonName}"
            );
            return null;
        }

        return button;
    }

    private void OnNewGameClicked()
    {
        GameSystemManager.Instance.StartNewGame();
    }

    private void OnContinueClicked()
    {
        GameSystemManager.Instance.ContinueGame();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

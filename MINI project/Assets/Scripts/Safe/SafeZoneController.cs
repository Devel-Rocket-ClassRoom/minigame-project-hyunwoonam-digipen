using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

/// <summary>
/// Safe0 씬의 임시 안전지대 진입/이탈 UI를 연결합니다.
/// </summary>
public class SafeZoneController : MonoBehaviour
{
    private const string Safe0SceneName = "Safe0";
    private const int Safe0Floor = 0;
    private Button mapButton;

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
        if (scene.name != Safe0SceneName)
        {
            return;
        }

        if (FindAnyObjectByType<SafeZoneController>() != null)
        {
            return;
        }

        GameObject controller = new GameObject("SafeZoneController");
        controller.AddComponent<SafeZoneController>();
    }

    private void Start()
    {
        NotifySafeZoneEntered();
        BindMapButton();
    }

    private void NotifySafeZoneEntered()
    {
        if (!GameSystemManager.TryGetInstance(out GameSystemManager gameSystemManager))
        {
            Debug.LogWarning("[SafeZoneController] GameSystemManager is missing on Safe0 entry.");
            return;
        }

        gameSystemManager.NotifySafeZoneEntered(Safe0Floor);
    }

    private void BindMapButton()
    {
        GameObject mapObject = GameObject.Find("Map");
        if (mapObject == null)
        {
            Debug.LogWarning("[SafeZoneController] Safe0 Map button was not found.");
            return;
        }

        mapButton = mapObject.GetComponent<Button>();
        if (mapButton == null)
        {
            Debug.LogWarning("[SafeZoneController] Safe0 Map object has no Button component.");
            return;
        }

        mapButton.onClick.RemoveListener(OnMapButtonClicked);
        mapButton.onClick.AddListener(OnMapButtonClicked);
        Debug.Log("[SafeZoneController] Safe0 Map button bound to EnterFloorMap.");
    }

    private void OnMapButtonClicked()
    {
        if (!GameSystemManager.TryGetInstance(out GameSystemManager gameSystemManager))
        {
            Debug.LogWarning(
                "[SafeZoneController] Cannot enter FloorMap because GameSystemManager is missing."
            );
            return;
        }

        gameSystemManager.EnterFloorMap();
    }
}

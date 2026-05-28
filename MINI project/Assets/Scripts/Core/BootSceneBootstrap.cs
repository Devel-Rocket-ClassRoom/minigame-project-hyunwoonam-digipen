using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// Boot 씬에서 전역 시스템/Overlay 초기화가 끝난 뒤 MainMenu로 진입한다.
    /// </summary>
    public sealed class BootSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameSystemManager gameSystemManager;
        [SerializeField] private bool loadMainMenuOnStart = true;

        private void Start()
        {
            if (!loadMainMenuOnStart)
            {
                return;
            }

            if (gameSystemManager == null)
            {
                Debug.LogError("[BootSceneBootstrap] gameSystemManager 참조가 Boot 씬에서 직접 할당되어야 합니다.");
                enabled = false;
                return;
            }

            if (gameSystemManager.Scenes == null)
            {
                Debug.LogError("[BootSceneBootstrap] GameSystemManager.Scenes 가 초기화되지 않았습니다.");
                enabled = false;
                return;
            }

            gameSystemManager.Scenes.LoadMainMenu();
        }
    }
}

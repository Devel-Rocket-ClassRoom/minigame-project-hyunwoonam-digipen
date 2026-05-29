using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// Boot 씬에서 전역 시스템/Overlay 초기화가 끝난 뒤 MainMenu로 진입한다.
    /// </summary>
    public sealed class BootSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameSystemManager gameSystemManager;
        [SerializeField] private GameObject tempGameOverPanel;
        [SerializeField] private bool loadMainMenuOnStart = true;

        private static BootSceneBootstrap instance;

        private void Awake()
        {
            instance = this;

            // Guid5 §10.B 2026-05-29 — Singleton fallback 폐기로 Boot 직렬화 참조를 강제 검증.
            if (gameSystemManager == null)
            {
                Debug.LogError("[BootSceneBootstrap] gameSystemManager 직렬화 누락 — Boot 씬의 GameObject가 손상되었을 가능성.");
                enabled = false;
                return;
            }

            if (gameSystemManager.gameObject.scene != gameObject.scene)
            {
                Debug.LogError("[BootSceneBootstrap] gameSystemManager가 Boot 씬이 아닌 다른 씬의 인스턴스를 참조합니다.");
                enabled = false;
            }
        }

        private void Start()
        {
            if (!loadMainMenuOnStart)
            {
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

        // Guid4 §9.I 2026-05-29 — TEMP: 정식 게임오버 패널 컨트롤러 도입 전 임시 활용.
        public static void ShowTempGameOverPanel()
        {
            // TEMP: H13-W1 보류 임시 처리. 정식 게임오버 패널 컨트롤러로 교체 필요.
            if (instance == null)
            {
                Debug.LogError("[BootSceneBootstrap] TEMP GameOver 패널 호출 — Boot 씬 부트스트랩 인스턴스 없음");
                return;
            }

            if (instance.tempGameOverPanel == null)
            {
                Debug.LogError("[BootSceneBootstrap] tempGameOverPanel 직렬화 누락");
                return;
            }

            instance.tempGameOverPanel.SetActive(true);
        }
    }
}

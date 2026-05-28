using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// Boot 씬에 직접 배치된 전역 Overlay UI를 씬 전환 후에도 유지하고 글로벌 단축키와 연결한다.
    /// </summary>
    public sealed class GlobalOverlayController : MonoBehaviour
    {
        private static GlobalOverlayController current;

        [SerializeField] private GameObject persistentRoot;
        [SerializeField] private InventoryPage inventoryPage;
        [SerializeField] private GameObject gameOverPanel;

        private HotkeyManager subscribedHotkey;
        private bool gameOverOpen;
        private float gameOverInputReadyTime;

        public bool HasHotkeySubscription => subscribedHotkey != null;

        public bool IsInventoryOpen => inventoryPage != null && inventoryPage.IsOpen;

        public static bool TryGetInstance(out GlobalOverlayController controller)
        {
            if (current == null)
            {
                current = FindFirstObjectByType<GlobalOverlayController>(FindObjectsInactive.Include);
            }

            controller = current;
            return controller != null;
        }

        private void Awake()
        {
            current = this;
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            DontDestroyOnLoad(persistentRoot);
            inventoryPage.OnClose();
            HideGameOver();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (current == this)
            {
                current = null;
            }
        }

        private void Update()
        {
            if (!gameOverOpen || Time.unscaledTime < gameOverInputReadyTime || !Input.anyKeyDown)
            {
                return;
            }

            HideGameOver();
        }

        public void ShowGameOver()
        {
            if (!TryResolveGameOverPanel())
            {
                Debug.LogError("[GlobalOverlayController] Boot GameOver 패널을 찾을 수 없습니다.");
                return;
            }

            inventoryPage.OnClose();
            gameOverPanel.SetActive(true);
            gameOverOpen = true;
            gameOverInputReadyTime = Time.unscaledTime + 0.2f;
        }

        public void HideGameOver()
        {
            if (!TryResolveGameOverPanel())
            {
                gameOverOpen = false;
                return;
            }

            gameOverPanel.SetActive(false);
            gameOverOpen = false;
        }

        private bool ValidateReferences()
        {
            if (persistentRoot == null || inventoryPage == null)
            {
                Debug.LogError("[GlobalOverlayController] persistentRoot / inventoryPage 참조가 Boot 씬에서 직접 할당되어야 합니다.");
                return false;
            }

            return true;
        }

        private bool TryResolveGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                return true;
            }

            if (persistentRoot == null)
            {
                return false;
            }

            Transform found = FindChildRecursive(persistentRoot.transform, "GameOver");
            if (found == null)
            {
                return false;
            }

            gameOverPanel = found.gameObject;
            return true;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void TrySubscribe()
        {
            if (!enabled || subscribedHotkey != null)
            {
                return;
            }

            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.Hotkey == null)
            {
                return;
            }

            subscribedHotkey = gsm.Hotkey;
            subscribedHotkey.OnTogglePage += HandleTogglePage;
        }

        private void Unsubscribe()
        {
            if (subscribedHotkey == null)
            {
                return;
            }

            subscribedHotkey.OnTogglePage -= HandleTogglePage;
            subscribedHotkey = null;
        }

        private void HandleTogglePage(HotkeyPageId pageId)
        {
            if (pageId != HotkeyPageId.Inventory)
            {
                return;
            }

            if (inventoryPage.IsOpen)
            {
                inventoryPage.OnClose();
            }
            else
            {
                inventoryPage.OnOpen();
            }
        }
    }
}

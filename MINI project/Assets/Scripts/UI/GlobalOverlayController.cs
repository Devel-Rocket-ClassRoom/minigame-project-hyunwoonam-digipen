using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// Boot 씬에 직접 배치된 전역 Overlay UI를 씬 전환 후에도 유지하고 글로벌 단축키와 연결한다.
    /// </summary>
    public sealed class GlobalOverlayController : MonoBehaviour
    {
        [SerializeField] private GameObject persistentRoot;
        [SerializeField] private InventoryPage inventoryPage;

        private HotkeyManager subscribedHotkey;

        public bool HasHotkeySubscription => subscribedHotkey != null;

        public bool IsInventoryOpen => inventoryPage != null && inventoryPage.IsOpen;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            DontDestroyOnLoad(persistentRoot);
            inventoryPage.OnClose();
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

using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// Boot 씬에 직접 배치된 전역 Overlay UI를 씬 전환 후에도 유지하고 글로벌 단축키와 연결한다.
    /// </summary>
    public sealed class GlobalOverlayController : MonoBehaviour
    {
        private static GlobalOverlayController current;

        [SerializeField]
        private GameObject persistentRoot;

        [SerializeField]
        private InventoryPage inventoryPage;

        [SerializeField]
        private GameObject runePageRoot;

        [SerializeField]
        private GameObject skillPageRoot;

        [SerializeField]
        private GameObject statRunePageRoot;

        [SerializeField]
        private RuneTreeView runeTreeView;

        [SerializeField]
        private GameObject gameOverPanel;

        [SerializeField]
        private QuitConfirmPopup quitConfirmPopup;

        private bool gameOverOpen;
        private float gameOverInputReadyTime;

        public bool IsInventoryOpen => inventoryPage != null && inventoryPage.IsOpen;

        public bool IsRuneOpen => runePageRoot != null && runePageRoot.activeSelf;

        public bool IsSkillOpen => skillPageRoot != null && skillPageRoot.activeSelf;

        public bool IsStatRuneOpen => statRunePageRoot != null && statRunePageRoot.activeSelf;

        public bool IsQuitConfirmOpen => quitConfirmPopup != null && quitConfirmPopup.IsOpen;

        public static bool TryGetInstance(out GlobalOverlayController controller)
        {
            if (current == null)
            {
                current = FindFirstObjectByType<GlobalOverlayController>(
                    FindObjectsInactive.Include
                );
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
            HideRunePage();
            HideSkillPage();
            HideStatRunePage();
            HideGameOver();
            HideQuitConfirm();
        }

        private void OnDestroy()
        {
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
            HideRunePage();
            HideSkillPage();
            HideStatRunePage();
            gameOverPanel.SetActive(true);
            gameOverOpen = true;
            gameOverInputReadyTime = Time.unscaledTime + 0.2f;
        }

        public void ShowAllStagesEroded()
        {
            ShowGameOver();
        }

        public void ShowQuitConfirm(System.Action onConfirm)
        {
            if (TryCloseTopEscapePanel())
            {
                return;
            }

            if (!TryResolveQuitConfirmPopup())
            {
                Debug.LogError(
                    "[GlobalOverlayController] ExitGamePanel / QuitConfirmPopup 을 찾을 수 없습니다."
                );
                return;
            }

            quitConfirmPopup.Show(onConfirm, HideQuitConfirm);
        }

        public void HideQuitConfirm()
        {
            quitConfirmPopup?.Hide();
        }

        public bool TryCloseTopEscapePanel()
        {
            if (quitConfirmPopup != null && quitConfirmPopup.IsOpen)
            {
                HideQuitConfirm();
                return true;
            }

            if (inventoryPage != null && inventoryPage.IsOpen)
            {
                inventoryPage.OnClose();
                return true;
            }

            if (IsRuneOpen)
            {
                HideRunePage();
                return true;
            }

            if (IsSkillOpen)
            {
                HideSkillPage();
                return true;
            }

            if (IsStatRuneOpen)
            {
                HideStatRunePage();
                return true;
            }

            if (TryCloseSafe1Panel())
            {
                return true;
            }

            if (TryCloseSafe0Panel())
            {
                return true;
            }

            return false;
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

        public void ShowRunePage()
        {
            if (!TryResolveRunePage())
            {
                Debug.LogError(
                    "[GlobalOverlayController] RuneTreePanel / RuneTreeView 참조가 Boot 씬에서 직접 할당되어 있지 않습니다."
                );
                return;
            }

            inventoryPage.OnClose();
            HideSkillPage();
            HideStatRunePage();
            runePageRoot.SetActive(true);
            BindRuneTree();
        }

        public void HideRunePage()
        {
            if (runePageRoot != null)
            {
                runePageRoot.SetActive(false);
            }
        }

        public void ShowSkillPage()
        {
            if (!TryResolveSkillPage())
            {
                Debug.LogError(
                    "[GlobalOverlayController] SkillsPanel / MainSkillsPanel 을 찾을 수 없습니다."
                );
                return;
            }

            inventoryPage.OnClose();
            HideRunePage();
            HideStatRunePage();
            skillPageRoot.SetActive(true);
        }

        public void HideSkillPage()
        {
            if (skillPageRoot != null)
            {
                skillPageRoot.SetActive(false);
            }
        }

        public void ShowStatRunePage()
        {
            if (!TryResolveStatRunePage())
            {
                Debug.LogError(
                    "[GlobalOverlayController] StatusPanel / StatRunePanel 을 찾을 수 없습니다."
                );
                return;
            }

            inventoryPage.OnClose();
            HideRunePage();
            HideSkillPage();
            statRunePageRoot.SetActive(true);
        }

        public void HideStatRunePage()
        {
            if (statRunePageRoot != null)
            {
                statRunePageRoot.SetActive(false);
            }
        }

        private bool ValidateReferences()
        {
            if (persistentRoot == null || inventoryPage == null)
            {
                Debug.LogError(
                    "[GlobalOverlayController] persistentRoot / inventoryPage 참조가 Boot 씬에서 직접 할당되어야 합니다."
                );
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

        private bool TryResolveQuitConfirmPopup()
        {
            if (quitConfirmPopup != null)
            {
                return true;
            }

            if (persistentRoot == null)
            {
                return false;
            }

            quitConfirmPopup = persistentRoot.GetComponentInChildren<QuitConfirmPopup>(true);
            if (quitConfirmPopup != null)
            {
                return true;
            }

            Transform exitGamePanel = FindChildRecursive(persistentRoot.transform, "ExitGamePanel");
            if (exitGamePanel == null)
            {
                return false;
            }

            quitConfirmPopup = exitGamePanel.GetComponent<QuitConfirmPopup>();
            if (quitConfirmPopup == null)
            {
                quitConfirmPopup = exitGamePanel.gameObject.AddComponent<QuitConfirmPopup>();
            }

            return quitConfirmPopup != null;
        }

        private bool TryResolveRunePage()
        {
            if (runePageRoot != null && runeTreeView != null)
            {
                return true;
            }

            if (persistentRoot == null)
            {
                return false;
            }

            if (runePageRoot == null)
            {
                Transform found = FindChildRecursive(persistentRoot.transform, "RuneTreePanel");
                runePageRoot = found != null ? found.gameObject : null;
            }

            if (runeTreeView == null && runePageRoot != null)
            {
                runeTreeView = runePageRoot.GetComponentInChildren<RuneTreeView>(true);
            }

            return runePageRoot != null && runeTreeView != null;
        }

        private bool TryResolveSkillPage()
        {
            if (skillPageRoot != null)
            {
                return true;
            }

            if (persistentRoot == null)
            {
                return false;
            }

            Transform found = FindChildRecursive(persistentRoot.transform, "SkillsPanel");
            if (found == null)
            {
                found = FindChildRecursive(persistentRoot.transform, "MainSkillsPanel");
            }

            skillPageRoot = found != null ? found.gameObject : null;
            return skillPageRoot != null;
        }

        private bool TryResolveStatRunePage()
        {
            if (statRunePageRoot != null)
            {
                return true;
            }

            if (persistentRoot == null)
            {
                return false;
            }

            Transform found = FindChildRecursive(persistentRoot.transform, "StatusPanel");
            if (found == null)
            {
                found = FindChildRecursive(persistentRoot.transform, "StatRunePanel");
            }

            statRunePageRoot = found != null ? found.gameObject : null;
            return statRunePageRoot != null;
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

        public void HandleTogglePage(HotkeyPageId pageId)
        {
            if (IsSafe0RuneSelectionBlocking())
            {
                return;
            }

            if (pageId == HotkeyPageId.Inventory)
            {
                ToggleInventoryPage();
                return;
            }

            if (pageId == HotkeyPageId.Skill)
            {
                ToggleSkillPage();
                return;
            }

            if (pageId == HotkeyPageId.StatRune)
            {
                ToggleStatRunePage();
                return;
            }

            if (pageId == HotkeyPageId.Rune)
            {
                ToggleRunePage();
            }
        }

        private void ToggleInventoryPage()
        {
            if (inventoryPage.IsOpen)
            {
                inventoryPage.OnClose();
            }
            else
            {
                HideRunePage();
                HideSkillPage();
                HideStatRunePage();
                inventoryPage.OnOpen();
            }
        }

        private void ToggleRunePage()
        {
            if (IsRuneOpen)
            {
                HideRunePage();
            }
            else
            {
                ShowRunePage();
            }
        }

        private void ToggleSkillPage()
        {
            if (IsSkillOpen)
            {
                HideSkillPage();
            }
            else
            {
                ShowSkillPage();
            }
        }

        private void ToggleStatRunePage()
        {
            if (IsStatRuneOpen)
            {
                HideStatRunePage();
            }
            else
            {
                ShowStatRunePage();
            }
        }

        private void BindRuneTree()
        {
            if (!TryResolveRunePage())
            {
                return;
            }

            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.CurrentRun?.Player?.Rune == null
            )
            {
                runeTreeView.Bind(null, RuneTreeView.Mode.ViewUnlock, true);
                return;
            }

            runeTreeView.Bind(
                gsm.CurrentRun.Player.Rune,
                RuneTreeView.Mode.ViewUnlock,
                IsCombatScene()
            );
        }

        private static bool TryCloseSafe1Panel()
        {
            SafeZone1FacilityMockupUI safe1Ui = FindFirstObjectByType<SafeZone1FacilityMockupUI>(
                FindObjectsInactive.Include
            );
            return safe1Ui != null && safe1Ui.TryCloseTopPanel();
        }

        private static bool TryCloseSafe0Panel()
        {
            SafeZone0SanctuaryMockupUI safe0Ui = FindFirstObjectByType<SafeZone0SanctuaryMockupUI>(
                FindObjectsInactive.Include
            );
            return safe0Ui != null && safe0Ui.TryCloseTopPanel();
        }

        private static bool IsSafe0RuneSelectionBlocking()
        {
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.Scenes == null
                || gsm.Scenes.CurrentSceneId != SceneId.Safe0
            )
            {
                return false;
            }

            SafeZone0SanctuaryMockupUI safe0Ui = FindFirstObjectByType<SafeZone0SanctuaryMockupUI>(
                FindObjectsInactive.Exclude
            );
            return safe0Ui != null
                && safe0Ui.isActiveAndEnabled
                && safe0Ui.IsRuneSelectionBlocking();
        }

        private static bool IsCombatScene()
        {
            return GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                && (
                    gsm.CombatContext != null
                    || (gsm.Scenes != null && gsm.Scenes.CurrentSceneId == SceneId.Combat)
                );
        }
    }
}

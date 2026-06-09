using UnityEngine;

/// <summary>
/// Boot 씬에 직접 배치된 전역 Overlay UI를 씬 전환 후에도 유지하고 글로벌 단축키와 연결한다.
/// </summary>
public sealed partial class GlobalOverlayController : MonoBehaviour
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

    private ShortcutInfoPanelController skillPageController;
    private ShortcutInfoPanelController statPageController;

    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private GameObject erosionGameOverPanel;

    [SerializeField]
    private QuitConfirmPopup quitConfirmPopup;

    private GameObject activeGameOverPanel;
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
            GameLog.LogError("[GlobalOverlayController] Boot GameOver 패널을 찾을 수 없습니다.");
            return;
        }

        ShowGameOverPanel(gameOverPanel);
    }

    public void ShowAllStagesEroded()
    {
        if (!TryResolveErosionGameOverPanel())
        {
            GameLog.LogError(
                "[GlobalOverlayController] Boot ErosionGameOverRoot 패널을 찾을 수 없습니다."
            );
            return;
        }

        ShowGameOverPanel(erosionGameOverPanel);
    }

    private void ShowGameOverPanel(GameObject panel)
    {
        inventoryPage.OnClose();
        HideRunePage();
        HideSkillPage();
        HideStatRunePage();
        HideGameOverPanels();
        panel.SetActive(true);
        activeGameOverPanel = panel;
        gameOverOpen = true;
        gameOverInputReadyTime = Time.unscaledTime + 0.2f;
    }

    public void ShowQuitConfirm(System.Action onConfirm)
    {
        if (TryCloseTopEscapePanel())
        {
            return;
        }

        if (!TryResolveQuitConfirmPopup())
        {
            GameLog.LogError(
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
        HideGameOverPanels();
        activeGameOverPanel = null;
        gameOverOpen = false;
    }

    public void ShowRunePage()
    {
        if (!TryResolveRunePage())
        {
            GameLog.LogError(
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
            GameLog.LogError(
                "[GlobalOverlayController] SkillsPanel / MainSkillsPanel 을 찾을 수 없습니다."
            );
            return;
        }

        inventoryPage.OnClose();
        HideRunePage();
        HideStatRunePage();
        EnsureShortcutPanelSizing(skillPageRoot);
        skillPageRoot.SetActive(true);
        EnsureShortcutPanelController(
            skillPageRoot,
            ShortcutInfoPanelController.PageKind.Skills,
            ref skillPageController
        )
            ?.Show(HideSkillPage);
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
            GameLog.LogError(
                "[GlobalOverlayController] StatusPanel / StatRunePanel 을 찾을 수 없습니다."
            );
            return;
        }

        inventoryPage.OnClose();
        HideRunePage();
        HideSkillPage();
        EnsureShortcutPanelSizing(statRunePageRoot);
        statRunePageRoot.SetActive(true);
        EnsureShortcutPanelController(
            statRunePageRoot,
            ShortcutInfoPanelController.PageKind.Status,
            ref statPageController
        )
            ?.Show(HideStatRunePage);
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
            GameLog.LogError(
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

    private bool TryResolveErosionGameOverPanel()
    {
        if (erosionGameOverPanel != null)
        {
            return true;
        }

        if (persistentRoot == null)
        {
            return false;
        }

        Transform found = FindChildRecursive(persistentRoot.transform, "ErosionGameOverRoot");
        erosionGameOverPanel = found != null ? found.gameObject : null;
        return erosionGameOverPanel != null;
    }

    private void HideGameOverPanels()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (erosionGameOverPanel != null)
        {
            erosionGameOverPanel.SetActive(false);
        }

        if (activeGameOverPanel != null)
        {
            activeGameOverPanel.SetActive(false);
        }
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

        Transform found = FindChildRecursiveWithDescendant(
            persistentRoot.transform,
            "MainSkillsPanel",
            "Content/LeftCol/Scroll/Viewport/ListContent"
        );
        if (found == null)
        {
            found = FindChildRecursive(persistentRoot.transform, "SkillsPanel");
        }

        if (found == null)
        {
            found = FindChildRecursive(persistentRoot.transform, "MainSkillsPanel");
        }

        skillPageRoot = found != null ? found.gameObject : null;
        if (skillPageRoot != null)
        {
            EnsureShortcutPanelSizing(skillPageRoot);
            EnsureShortcutPanelController(
                skillPageRoot,
                ShortcutInfoPanelController.PageKind.Skills,
                ref skillPageController
            );
        }

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
        if (statRunePageRoot != null)
        {
            EnsureShortcutPanelSizing(statRunePageRoot);
            EnsureShortcutPanelController(
                statRunePageRoot,
                ShortcutInfoPanelController.PageKind.Status,
                ref statPageController
            );
        }

        return statRunePageRoot != null;
    }

}

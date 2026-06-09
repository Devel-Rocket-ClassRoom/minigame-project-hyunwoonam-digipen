using UnityEngine;

public sealed partial class GlobalOverlayController
{
    private static ShortcutInfoPanelController EnsureShortcutPanelController(
        GameObject root,
        ShortcutInfoPanelController.PageKind kind,
        ref ShortcutInfoPanelController cached
    )
    {
        if (root == null)
        {
            cached = null;
            return null;
        }

        if (cached == null || cached.gameObject != root)
        {
            cached = root.GetComponent<ShortcutInfoPanelController>();
            if (cached == null)
            {
                cached = root.AddComponent<ShortcutInfoPanelController>();
            }
        }

        cached.Kind = kind;
        return cached;
    }

    private static Transform FindChildRecursiveWithDescendant(
        Transform root,
        string childName,
        string descendantPath
    )
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == childName && root.Find(descendantPath) != null)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursiveWithDescendant(
                root.GetChild(i),
                childName,
                descendantPath
            );
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static void EnsureShortcutPanelSizing(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        ResponsiveOverlayPanelSizer sizer = root.GetComponent<ResponsiveOverlayPanelSizer>();
        if (sizer == null)
        {
            sizer = root.AddComponent<ResponsiveOverlayPanelSizer>();
        }

        sizer.ReferenceSize = new Vector2(980f, 610f);
        sizer.ViewportRatio = new Vector2(0.88f, 0.88f);
        sizer.MinSize = new Vector2(680f, 420f);
        sizer.MaxSize = new Vector2(1180f, 740f);
        sizer.PreserveAspect = true;
        sizer.ApplySize();
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

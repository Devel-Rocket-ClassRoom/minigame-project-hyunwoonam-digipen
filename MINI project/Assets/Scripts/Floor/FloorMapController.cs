using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

/// <summary>
/// FloorMap 씬에서 전체 데모 층계 노드를 표시하고 선택을 처리합니다.
/// </summary>
public class FloorMapController : MonoBehaviour
{
    private const string FloorMapSceneName = "FloorMap";
    private const float RowHeight = 118f;
    private const float ButtonWidth = 172f;
    private const float ButtonHeight = 76f;

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
        if (scene.name != FloorMapSceneName)
        {
            return;
        }

        if (FindAnyObjectByType<FloorMapController>() != null)
        {
            return;
        }

        GameObject controller = new GameObject("FloorMapController");
        controller.AddComponent<FloorMapController>();
    }

    private void Start()
    {
        EnsureEventSystem();
        BuildNodeView();
    }

    private void BuildNodeView()
    {
        GameSystemManager gameSystemManager = FindAnyObjectByType<GameSystemManager>();
        FloorNodeCreator creator = GetFloorNodeCreator(gameSystemManager);

        if (creator.Nodes.Count == 0)
        {
            creator.GenerateDemoFloorNode();
        }

        Canvas canvas = CreateCanvas();
        CreateHeader(canvas.transform);
        RectTransform content = CreateScrollView(canvas.transform);
        RenderFloorRows(content, creator.Nodes, gameSystemManager);

        Debug.Log($"[FloorMapController] Floor nodes rendered: {creator.Nodes.Count}");
    }

    private FloorNodeCreator GetFloorNodeCreator(GameSystemManager gameSystemManager)
    {
        if (gameSystemManager != null)
        {
            return gameSystemManager.FloorNodeCreator;
        }

        FloorNodeCreator creator = new FloorNodeCreator();
        creator.Initialize(new DataManager());
        Debug.LogWarning(
            "[FloorMapController] GameSystemManager is missing. Using local demo nodes."
        );
        return creator;
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("FloorMapCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void CreateHeader(Transform parent)
    {
        Text header = CreateText(parent, "Floor Map", 30, TextAnchor.MiddleCenter);
        RectTransform rectTransform = header.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -32f);
        rectTransform.sizeDelta = new Vector2(520f, 48f);
    }

    private RectTransform CreateScrollView(Transform parent)
    {
        GameObject scrollObject = new GameObject("FloorMapScroll");
        scrollObject.transform.SetParent(parent, false);
        RectTransform scrollRectTransform = scrollObject.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchoredPosition = new Vector2(0f, -24f);
        scrollRectTransform.sizeDelta = new Vector2(920f, 520f);

        Image background = scrollObject.AddComponent<Image>();
        background.color = new Color(0.06f, 0.07f, 0.08f, 0.88f);

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewportObject = new GameObject("Viewport");
        viewportObject.transform.SetParent(scrollObject.transform, false);
        RectTransform viewport = viewportObject.AddComponent<RectTransform>();
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(16f, 16f);
        viewport.offsetMax = new Vector2(-16f, -16f);

        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.12f);
        viewportObject.AddComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content");
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform content = contentObject.AddComponent<RectTransform>();
        content.anchorMin = new Vector2(0.5f, 1f);
        content.anchorMax = new Vector2(0.5f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(860f, 0f);

        scrollRect.viewport = viewport;
        scrollRect.content = content;
        return content;
    }

    private void RenderFloorRows(
        RectTransform content,
        IReadOnlyList<FloorNodeData> nodes,
        GameSystemManager gameSystemManager
    )
    {
        SortedDictionary<int, List<FloorNodeData>> nodesByFloor = GroupNodesByFloor(nodes);
        int rowIndex = 0;

        foreach (KeyValuePair<int, List<FloorNodeData>> floorNodes in nodesByFloor)
        {
            RectTransform row = CreateFloorRow(content, floorNodes.Key, rowIndex);
            List<FloorNodeData> floorNodeList = floorNodes.Value;

            for (int i = 0; i < floorNodeList.Count; i++)
            {
                CreateNodeButton(row, floorNodeList[i], i, floorNodeList.Count, gameSystemManager);
            }

            rowIndex++;
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, Mathf.Max(1, rowIndex) * RowHeight);
    }

    private SortedDictionary<int, List<FloorNodeData>> GroupNodesByFloor(
        IReadOnlyList<FloorNodeData> nodes
    )
    {
        SortedDictionary<int, List<FloorNodeData>> result =
            new SortedDictionary<int, List<FloorNodeData>>();

        for (int i = 0; i < nodes.Count; i++)
        {
            FloorNodeData node = nodes[i];
            if (!result.TryGetValue(node.Floor, out List<FloorNodeData> floorNodes))
            {
                floorNodes = new List<FloorNodeData>();
                result.Add(node.Floor, floorNodes);
            }

            floorNodes.Add(node);
        }

        return result;
    }

    private RectTransform CreateFloorRow(RectTransform parent, int floor, int rowIndex)
    {
        GameObject rowObject = new GameObject($"FloorRow_{floor}");
        rowObject.transform.SetParent(parent, false);

        RectTransform row = rowObject.AddComponent<RectTransform>();
        row.anchorMin = new Vector2(0.5f, 1f);
        row.anchorMax = new Vector2(0.5f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.anchoredPosition = new Vector2(0f, -rowIndex * RowHeight);
        row.sizeDelta = new Vector2(840f, RowHeight);

        Image rowImage = rowObject.AddComponent<Image>();
        rowImage.color =
            rowIndex % 2 == 0 ? new Color(1f, 1f, 1f, 0.04f) : new Color(1f, 1f, 1f, 0.02f);

        Text floorLabel = CreateText(rowObject.transform, $"{floor}F", 22, TextAnchor.MiddleLeft);
        RectTransform labelRect = floorLabel.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(18f, 0f);
        labelRect.sizeDelta = new Vector2(92f, RowHeight);

        return row;
    }

    private void CreateNodeButton(
        RectTransform parent,
        FloorNodeData node,
        int visualIndex,
        int floorNodeCount,
        GameSystemManager gameSystemManager
    )
    {
        GameObject buttonObject = new GameObject($"FloorNode_{node.Floor}_{node.NodeSlot}");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

        float startX = -(floorNodeCount - 1) * (ButtonWidth + 24f) * 0.5f;
        rectTransform.anchoredPosition = new Vector2(
            startX + visualIndex * (ButtonWidth + 24f),
            0f
        );

        bool canSelect =
            gameSystemManager != null
                ? gameSystemManager.CanSelectFloorNode(node)
                : !node.IsCleared;

        Image image = buttonObject.AddComponent<Image>();
        image.color = GetNodeColor(node, canSelect);

        Button button = buttonObject.AddComponent<Button>();
        button.interactable = canSelect;

        FloorNodeData capturedNode = node;
        button.onClick.AddListener(() => OnNodeButtonClicked(capturedNode));

        string nodeType = node.IsBossNode ? "Boss" : "Combat";
        string clearText = node.IsCleared ? "Cleared" : nodeType;
        string label =
            $"{node.DisplayName}\n{clearText}\nD{node.Difficulty} / M{node.MonsterCount}";

        Text text = CreateText(buttonObject.transform, label, 17, TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(6f, 4f);
        textRect.offsetMax = new Vector2(-6f, -4f);
    }

    private Color GetNodeColor(FloorNodeData node, bool canSelect)
    {
        if (!canSelect)
        {
            return node.IsCleared
                ? new Color(0.18f, 0.32f, 0.2f, 0.75f)
                : new Color(0.18f, 0.18f, 0.18f, 0.75f);
        }

        if (node.IsBossNode)
        {
            return new Color(0.52f, 0.12f, 0.12f, 0.96f);
        }

        return new Color(0.14f, 0.23f, 0.34f, 0.96f);
    }

    private Text CreateText(Transform parent, string value, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private void OnNodeButtonClicked(FloorNodeData node)
    {
        GameSystemManager gameSystemManager = FindAnyObjectByType<GameSystemManager>();

        if (gameSystemManager == null)
        {
            Debug.LogWarning(
                $"[FloorMapController] Cannot enter combat for floor {node.Floor} because GameSystemManager is missing."
            );
            return;
        }

        Debug.Log($"[FloorMapController] Selected floor {node.Floor}, node={node.NodeIndex}");
        gameSystemManager.StartCombatNode(node.NodeIndex);
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

/// <summary>
/// FloorMap 씬에서 1주차 데모용 FloorNode 버튼을 표시하고 선택을 처리합니다.
/// </summary>
public class FloorMapController : MonoBehaviour
{
    private const string FloorMapSceneName = "FloorMap";
    private RectTransform nodeContainer;

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
        FloorNodeCreator creator = GetFloorNodeCreator();
        IReadOnlyList<FloorNodeData> nodes = creator.Nodes;

        if (nodes.Count == 0)
        {
            creator.GenerateDemoFloorNode();
            nodes = creator.Nodes;
        }

        Canvas canvas = CreateCanvas();
        CreateHeader(canvas.transform);

        nodeContainer = CreateNodeContainer(canvas.transform);

        for (int i = 0; i < nodes.Count; i++)
        {
            FloorNodeData node = nodes[i];
            CreateNodeButton(nodeContainer, node, i);
        }

        Debug.Log($"[FloorMapController] Floor nodes rendered: {nodes.Count}");
    }

    private FloorNodeCreator GetFloorNodeCreator()
    {
        GameSystemManager gameSystemManager = FindAnyObjectByType<GameSystemManager>();

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
        canvasObject.AddComponent<CanvasScaler>();
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
        rectTransform.anchoredPosition = new Vector2(0f, -48f);
        rectTransform.sizeDelta = new Vector2(520f, 48f);
    }

    private RectTransform CreateNodeContainer(Transform parent)
    {
        GameObject containerObject = new GameObject("NodeContainer");
        containerObject.transform.SetParent(parent, false);
        RectTransform rectTransform = containerObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(720f, 220f);
        return rectTransform;
    }

    private void CreateNodeButton(RectTransform parent, FloorNodeData node, int visualIndex)
    {
        GameObject buttonObject = new GameObject($"FloorNode_{node.Floor}");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(180f, 88f);
        rectTransform.anchoredPosition = new Vector2((visualIndex - 1) * 230f, 0f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = node.IsBossNode
            ? new Color(0.45f, 0.12f, 0.12f, 0.95f)
            : new Color(0.14f, 0.2f, 0.25f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        button.interactable = node.IsUnlocked;

        FloorNodeData capturedNode = node;
        button.onClick.AddListener(() => OnNodeButtonClicked(capturedNode));

        string label = node.IsBossNode
            ? $"{node.DisplayName}\nCombat"
            : $"{node.DisplayName}\nCombat";
        Text text = CreateText(buttonObject.transform, label, 20, TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
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

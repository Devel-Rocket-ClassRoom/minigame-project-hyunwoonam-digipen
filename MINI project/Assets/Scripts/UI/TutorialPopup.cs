using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 이미지 튜토리얼 팝업. 프리팹 없이 절차적으로 생성된다.
/// 닫기 버튼으로 즉시 닫고, 다음 버튼으로 페이지를 넘긴다(마지막 장에서는 다음 버튼 숨김).
/// 현재 씬에 생성되므로 씬 언로드 시 함께 파괴되며, 파괴 시에도 onClosed가 1회 보장 호출된다.
/// </summary>
public sealed class TutorialPopup : MonoBehaviour
{
    private const int CanvasSortingOrder = 600;
    private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
    private static readonly Vector2 ImageAreaSize = new Vector2(1280f, 720f);
    private static readonly Vector2 ButtonSize = new Vector2(180f, 64f);
    private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.6f);
    private static readonly Color ButtonColor = new Color(0.18f, 0.18f, 0.22f, 0.95f);

    private IReadOnlyList<string> imagePaths;
    private Action onClosed;
    private bool closedInvoked;

    private RawImage pageImage;
    private AspectRatioFitter pageFitter;
    private TextMeshProUGUI pageLabel;
    private GameObject nextButtonRoot;
    private int pageIndex;

    /// <summary>
    /// 튜토리얼 팝업을 생성해 표시한다.
    /// </summary>
    /// <param name="imagePaths">Resources 경로(확장자 제외) 목록. 1~4장.</param>
    /// <param name="onClosed">닫힘(파괴 포함) 시 1회 호출.</param>
    public static TutorialPopup Show(IReadOnlyList<string> imagePaths, Action onClosed)
    {
        if (imagePaths == null || imagePaths.Count == 0)
        {
            onClosed?.Invoke();
            return null;
        }

        GameObject go = new GameObject("TutorialPopup");
        TutorialPopup popup = go.AddComponent<TutorialPopup>();
        popup.imagePaths = imagePaths;
        popup.onClosed = onClosed;
        popup.BuildUi();
        popup.SetPage(0);

        return popup;
    }

    /// <summary>현재 팝업을 닫는다.</summary>
    public void Close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // 씬 언로드로 파괴되는 경우에도 콜백을 1회 보장한다.
        if (!closedInvoked)
        {
            closedInvoked = true;
            onClosed?.Invoke();
        }
    }

    private void OnNextClicked()
    {
        if (pageIndex + 1 < imagePaths.Count)
        {
            SetPage(pageIndex + 1);
        }
    }

    private void SetPage(int index)
    {
        pageIndex = index;
        string path = imagePaths[index];
        Texture2D texture = Resources.Load<Texture2D>(path);

        if (texture == null)
        {
            GameLog.LogError("[TutorialPopup] 튜토리얼 이미지 로드 실패: Resources/" + path);
        }
        else
        {
            pageImage.texture = texture;
            pageFitter.aspectRatio = (float)texture.width / texture.height;
        }

        pageLabel.text = (index + 1) + " / " + imagePaths.Count;
        nextButtonRoot.SetActive(index + 1 < imagePaths.Count);
    }

    private void BuildUi()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasSortingOrder;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;

        gameObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        // 배경 딤(뒤 UI 클릭 차단)
        Image dim = CreateChild("Dim", transform).AddComponent<Image>();
        dim.color = DimColor;
        Stretch(dim.rectTransform);

        // 이미지 영역(원본 비율 유지 FitInParent)
        RectTransform area = (RectTransform)CreateChild("ImageArea", transform).transform;
        area.sizeDelta = ImageAreaSize;

        pageImage = CreateChild("PageImage", area).AddComponent<RawImage>();
        pageFitter = pageImage.gameObject.AddComponent<AspectRatioFitter>();
        pageFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

        // 페이지 표시 (예: 1 / 3)
        pageLabel = CreateChild("PageLabel", transform).AddComponent<TextMeshProUGUI>();
        pageLabel.fontSize = 32f;
        pageLabel.alignment = TextAlignmentOptions.Center;
        pageLabel.color = Color.white;
        RectTransform labelRect = pageLabel.rectTransform;
        labelRect.anchoredPosition = new Vector2(0f, -(ImageAreaSize.y * 0.5f + 50f));
        labelRect.sizeDelta = new Vector2(200f, 40f);

        // 버튼: 닫기(항상), 다음(마지막 장 제외)
        CreateButton(
            "CloseButton",
            Loc.Get("ui_close"),
            new Vector2(-(ButtonSize.x * 0.5f + 12f), -(ImageAreaSize.y * 0.5f + 110f)),
            Close
        );
        nextButtonRoot = CreateButton(
            "NextButton",
            Loc.Get("tutorial_next"),
            new Vector2(ButtonSize.x * 0.5f + 12f, -(ImageAreaSize.y * 0.5f + 110f)),
            OnNextClicked
        );
    }

    private GameObject CreateButton(
        string name,
        string label,
        Vector2 anchoredPosition,
        UnityEngine.Events.UnityAction onClick
    )
    {
        GameObject buttonGo = CreateChild(name, transform);

        Image background = buttonGo.AddComponent<Image>();
        background.color = ButtonColor;

        RectTransform rect = background.rectTransform;
        rect.sizeDelta = ButtonSize;
        rect.anchoredPosition = anchoredPosition;

        Button button = buttonGo.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);

        TextMeshProUGUI text = CreateChild("Label", rect).AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 30f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        Stretch(text.rectTransform);

        return buttonGo;
    }

    private static GameObject CreateChild(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        return go;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}

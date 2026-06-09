using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class ResponsiveOverlayPanelSizer : MonoBehaviour
{
    [SerializeField]
    private Vector2 referenceSize = new Vector2(1080f, 610f);

    [SerializeField]
    private Vector2 viewportRatio = new Vector2(0.86f, 0.86f);

    [SerializeField]
    private Vector2 minSize = new Vector2(760f, 460f);

    [SerializeField]
    private Vector2 maxSize = new Vector2(1320f, 820f);

    [SerializeField]
    private bool preserveAspect = true;

    [SerializeField]
    private bool applyOnUpdate;

    private RectTransform rectTransform;
    private RectTransform parentRect;

    public Vector2 ReferenceSize
    {
        get => referenceSize;
        set => referenceSize = value;
    }

    public Vector2 ViewportRatio
    {
        get => viewportRatio;
        set => viewportRatio = value;
    }

    public Vector2 MinSize
    {
        get => minSize;
        set => minSize = value;
    }

    public Vector2 MaxSize
    {
        get => maxSize;
        set => maxSize = value;
    }

    public bool PreserveAspect
    {
        get => preserveAspect;
        set => preserveAspect = value;
    }

    private void Awake()
    {
        CacheReferences();
        ApplySize();
    }

    private void OnEnable()
    {
        CacheReferences();
        ApplySize();
    }

    private void LateUpdate()
    {
        if (applyOnUpdate)
        {
            ApplySize();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        CacheReferences();
        ApplySize();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheReferences();

        if (isActiveAndEnabled)
        {
            ApplySize();
        }
    }
#endif

    public void ApplySize()
    {
        CacheReferences();
        if (rectTransform == null || parentRect == null)
        {
            return;
        }

        Vector2 parentSize = parentRect.rect.size;
        if (parentSize.x <= 0f || parentSize.y <= 0f)
        {
            return;
        }

        Vector2 target = new Vector2(
            Mathf.Clamp(parentSize.x * viewportRatio.x, minSize.x, maxSize.x),
            Mathf.Clamp(parentSize.y * viewportRatio.y, minSize.y, maxSize.y)
        );

        if (referenceSize.x <= 0f || referenceSize.y <= 0f)
        {
            return;
        }

        float scaleX = target.x / referenceSize.x;
        float scaleY = target.y / referenceSize.y;
        float scale = preserveAspect ? Mathf.Min(scaleX, scaleY) : Mathf.Max(scaleX, scaleY);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = preserveAspect ? referenceSize : target;
        rectTransform.localScale = preserveAspect ? new Vector3(scale, scale, 1f) : Vector3.one;
    }

    private void CacheReferences()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        parentRect = transform.parent as RectTransform;
    }
}

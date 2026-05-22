using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 중 엔티티 머리 위에 붙는 임시 월드 UI입니다.
/// </summary>
public class EntityWorldUI : MonoBehaviour
{
    [SerializeField]
    private Vector3 worldOffset = new Vector3(0f, 1.25f, 0f);

    [SerializeField]
    private Vector3 effectOffset = new Vector3(0f, 0.7f, 0f);

    [SerializeField]
    private float canvasScale = 0.01f;

    [SerializeField]
    private float actionVisibleSeconds = 1f;

    [SerializeField]
    private bool showHealthSlider;

    private EntityBase entity;
    private Canvas worldCanvas;
    private Image actionImage;
    private Slider healthSlider;
    private Transform effectAnchor;
    private float hideActionAt = -1f;

    public Canvas WorldCanvas => worldCanvas;
    public Image ActionImage => actionImage;
    public Slider HealthSlider => healthSlider;
    public Transform EffectAnchor => effectAnchor;

    public static EntityWorldUI EnsureFor(EntityBase target, bool includeHealthSlider)
    {
        if (target == null)
        {
            return null;
        }

        EntityWorldUI worldUI = target.GetComponent<EntityWorldUI>();
        if (worldUI == null)
        {
            worldUI = target.gameObject.AddComponent<EntityWorldUI>();
        }

        worldUI.entity = target;
        worldUI.showHealthSlider = includeHealthSlider;
        worldUI.EnsureInitialized();
        worldUI.SetHealthSliderVisible(includeHealthSlider);
        worldUI.UpdateHealthDisplay();
        return worldUI;
    }

    private void Awake()
    {
        entity = GetComponent<EntityBase>();
        EnsureInitialized();
        SetHealthSliderVisible(showHealthSlider);
    }

    private void LateUpdate()
    {
        EnsureInitialized();
        UpdateHealthDisplay();
        FaceCamera();

        if (
            Application.isPlaying
            && actionImage != null
            && actionImage.gameObject.activeSelf
            && hideActionAt >= 0f
            && Time.time >= hideActionAt
        )
        {
            actionImage.gameObject.SetActive(false);
            hideActionAt = -1f;
        }
    }

    public void ShowAction(CombatActionType actionType)
    {
        ShowAction(actionType, actionVisibleSeconds);
    }

    public void ShowAction(CombatActionType actionType, float visibleSeconds)
    {
        EnsureInitialized();

        if (actionImage == null)
        {
            return;
        }

        actionImage.color = actionType == CombatActionType.Defend ? Color.blue : Color.red;
        actionImage.gameObject.SetActive(true);
        float safeVisibleSeconds = Mathf.Max(0f, visibleSeconds);
        hideActionAt = Application.isPlaying ? Time.time + safeVisibleSeconds : -1f;
    }

    public void UpdateHealthDisplay()
    {
        if (entity == null || healthSlider == null)
        {
            return;
        }

        healthSlider.minValue = 0f;
        healthSlider.maxValue = Mathf.Max(1, entity.MaxHP);
        healthSlider.value = Mathf.Clamp(entity.CurrentHP, 0, entity.MaxHP);
    }

    private void EnsureInitialized()
    {
        entity = entity != null ? entity : GetComponent<EntityBase>();
        EnsureEffectAnchor();
        EnsureCanvas();
        EnsureActionImage();
        EnsureHealthSlider();
    }

    private void EnsureEffectAnchor()
    {
        if (effectAnchor != null)
        {
            return;
        }

        Transform existing = transform.Find("EffectAnchor");
        if (existing != null)
        {
            effectAnchor = existing;
            return;
        }

        GameObject anchorObject = new GameObject("EffectAnchor");
        effectAnchor = anchorObject.transform;
        effectAnchor.SetParent(transform, false);
        effectAnchor.localPosition = effectOffset;
    }

    private void EnsureCanvas()
    {
        if (worldCanvas != null)
        {
            return;
        }

        Transform existing = transform.Find("WorldUICanvas");
        if (existing != null)
        {
            worldCanvas = existing.GetComponent<Canvas>();
            if (worldCanvas != null)
            {
                return;
            }
        }

        GameObject canvasObject = new GameObject("WorldUICanvas");
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = worldOffset;
        canvasObject.transform.localScale = Vector3.one * canvasScale;

        worldCanvas = canvasObject.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 30;

        RectTransform rectTransform = canvasObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(140f, 72f);
    }

    private void EnsureActionImage()
    {
        if (actionImage != null)
        {
            return;
        }

        Transform existing = worldCanvas.transform.Find("ActionImage");
        if (existing != null)
        {
            actionImage = existing.GetComponent<Image>();
            if (actionImage != null)
            {
                return;
            }
        }

        GameObject imageObject = new GameObject("ActionImage");
        imageObject.transform.SetParent(worldCanvas.transform, false);

        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0f, 22f);
        rectTransform.sizeDelta = new Vector2(32f, 32f);

        actionImage = imageObject.AddComponent<Image>();
        actionImage.raycastTarget = false;
        actionImage.color = Color.red;
        actionImage.gameObject.SetActive(false);
    }

    private void EnsureHealthSlider()
    {
        if (healthSlider != null)
        {
            return;
        }

        Transform existing = worldCanvas.transform.Find("HealthSlider");
        if (existing != null)
        {
            healthSlider = existing.GetComponent<Slider>();
            if (healthSlider != null)
            {
                return;
            }
        }

        GameObject sliderObject = new GameObject("HealthSlider");
        sliderObject.transform.SetParent(worldCanvas.transform, false);

        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchoredPosition = new Vector2(0f, -12f);
        sliderRect.sizeDelta = new Vector2(96f, 10f);

        Image background = sliderObject.AddComponent<Image>();
        background.color = new Color(0.12f, 0.02f, 0.02f, 0.85f);
        background.raycastTarget = false;

        healthSlider = sliderObject.AddComponent<Slider>();
        healthSlider.transition = Selectable.Transition.None;
        healthSlider.interactable = false;
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.value = 1f;

        RectTransform fillArea = CreateSliderChild("Fill Area", sliderObject.transform);
        fillArea.anchorMin = new Vector2(0f, 0f);
        fillArea.anchorMax = new Vector2(1f, 1f);
        fillArea.offsetMin = new Vector2(1f, 1f);
        fillArea.offsetMax = new Vector2(-1f, -1f);

        RectTransform fill = CreateSliderChild("Fill", fillArea);
        fill.anchorMin = new Vector2(0f, 0f);
        fill.anchorMax = new Vector2(1f, 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        Image fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.color = new Color(0.8f, 0.08f, 0.08f, 1f);
        fillImage.raycastTarget = false;

        healthSlider.fillRect = fill;
        healthSlider.targetGraphic = fillImage;
    }

    private static RectTransform CreateSliderChild(string objectName, Transform parent)
    {
        GameObject child = new GameObject(objectName);
        child.transform.SetParent(parent, false);
        return child.AddComponent<RectTransform>();
    }

    private void SetHealthSliderVisible(bool visible)
    {
        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(visible);
        }
    }

    private void FaceCamera()
    {
        if (worldCanvas == null || Camera.main == null)
        {
            return;
        }

        worldCanvas.transform.rotation = Camera.main.transform.rotation;
    }
}

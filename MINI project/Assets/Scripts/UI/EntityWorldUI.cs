using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// 엔티티 머리 위 World Space Canvas. HP/MP 슬라이더 + 행동 아이콘(빨강/파랑).
    /// </summary>
    public sealed class EntityWorldUI : MonoBehaviour
    {
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.25f, 0f);
        [SerializeField] private Vector3 effectOffset = new Vector3(0f, 0.7f, 0f);
        [SerializeField] private float canvasScale = 0.01f;
        [SerializeField] private float actionVisibleSeconds = 1f;
        [SerializeField] private bool showHealthSlider = true;
        [SerializeField] private bool showManaSlider;

        /// <summary>대응 엔티티.</summary>
        public EntityBase Entity;

        /// <summary>HP 슬라이더 컴포넌트(Inspector 연결).</summary>
        public Slider HpSlider;

        /// <summary>MP 슬라이더 컴포넌트(Inspector 연결).</summary>
        public Slider MpSlider;

        /// <summary>액션 아이콘.</summary>
        public ActionIconUI ActionIcon;

        private Canvas worldCanvas;
        private Image targetHighlight;
        private Transform effectAnchor;
        private Coroutine flashRoutine;

        public Vector3 EffectAnchorPosition
        {
            get
            {
                EnsureInitialized();
                return effectAnchor != null ? effectAnchor.position : transform.position + effectOffset;
            }
        }

        public static EntityWorldUI EnsureFor(EntityBase target, bool includeManaSlider)
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

            worldUI.Entity = target;
            worldUI.showHealthSlider = true;
            worldUI.showManaSlider = includeManaSlider;
            worldUI.EnsureInitialized();
            worldUI.UpdateBars();
            target.WorldUI = worldUI;
            return worldUI;
        }

        private void Awake()
        {
            Entity = Entity != null ? Entity : GetComponent<EntityBase>();
            EnsureInitialized();
            HideActionIcon();
        }

        private void LateUpdate()
        {
            EnsureInitialized();
            UpdateBars();
            FaceCamera();
        }

        /// <summary>
        /// 공격/스킬 시 빨강 아이콘 표시.
        /// </summary>
        public void ShowAttackIcon()
        {
            EnsureInitialized();
            ActionIcon?.SetMode(ActionIconMode.AttackRed);
            CancelInvoke(nameof(HideActionIcon));
            Invoke(nameof(HideActionIcon), Mathf.Max(0.1f, actionVisibleSeconds));
        }

        /// <summary>
        /// 방어 시 파랑 아이콘 표시.
        /// </summary>
        public void ShowDefendIcon()
        {
            EnsureInitialized();
            CancelInvoke(nameof(HideActionIcon));
            ActionIcon?.SetMode(ActionIconMode.DefendBlue);
        }

        /// <summary>
        /// 아이콘 숨김.
        /// </summary>
        public void HideActionIcon()
        {
            CancelInvoke(nameof(HideActionIcon));
            ActionIcon?.Hide();
        }

        public void SetTargetHighlight(bool highlighted)
        {
            EnsureInitialized();
            if (targetHighlight != null)
            {
                targetHighlight.enabled = highlighted;
            }
        }

        /// <summary>피격 연출.</summary>
        public void PlayHitFx()
        {
            StartFlash(new Color(1f, 0.18f, 0.12f, 1f));
        }

        /// <summary>회복 연출.</summary>
        public void PlayHealFx()
        {
            StartFlash(new Color(0.2f, 1f, 0.45f, 1f));
        }

        private void EnsureInitialized()
        {
            Entity = Entity != null ? Entity : GetComponent<EntityBase>();
            EnsureEffectAnchor();
            EnsureCanvas();
            EnsureTargetHighlight();
            EnsureActionIcon();
            EnsureSlider(ref HpSlider, "HP", new Vector2(0f, -14f), new Color(0.85f, 0.08f, 0.08f, 1f));
            EnsureSlider(ref MpSlider, "MP", new Vector2(0f, -28f), new Color(0.12f, 0.34f, 0.95f, 1f));

            if (HpSlider != null)
            {
                HpSlider.gameObject.SetActive(showHealthSlider);
            }

            if (MpSlider != null)
            {
                MpSlider.gameObject.SetActive(showManaSlider);
            }
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
            if (existing != null && existing.TryGetComponent(out Canvas existingCanvas))
            {
                worldCanvas = existingCanvas;
                return;
            }

            GameObject canvasObject = new GameObject("WorldUICanvas");
            canvasObject.transform.SetParent(transform, false);
            canvasObject.transform.localPosition = worldOffset;
            canvasObject.transform.localScale = Vector3.one * canvasScale;

            worldCanvas = canvasObject.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingOrder = 50;

            RectTransform rect = canvasObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(132f, 80f);
        }

        private void EnsureTargetHighlight()
        {
            if (targetHighlight != null)
            {
                return;
            }

            Transform existing = worldCanvas.transform.Find("TargetHighlight");
            if (existing != null && existing.TryGetComponent(out Image existingImage))
            {
                targetHighlight = existingImage;
                targetHighlight.enabled = false;
                return;
            }

            GameObject highlightObject = new GameObject("TargetHighlight");
            highlightObject.transform.SetParent(worldCanvas.transform, false);

            RectTransform rect = highlightObject.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, -20f);
            rect.sizeDelta = new Vector2(118f, 46f);

            targetHighlight = highlightObject.AddComponent<Image>();
            targetHighlight.color = new Color(1f, 0.82f, 0.12f, 0.28f);
            targetHighlight.raycastTarget = false;
            targetHighlight.enabled = false;
        }

        private void EnsureActionIcon()
        {
            if (ActionIcon != null)
            {
                return;
            }

            Transform existing = worldCanvas.transform.Find("ActionIcon");
            if (existing != null && existing.TryGetComponent(out ActionIconUI existingIcon))
            {
                ActionIcon = existingIcon;
                return;
            }

            GameObject iconObject = new GameObject("ActionIcon");
            iconObject.transform.SetParent(worldCanvas.transform, false);

            RectTransform rect = iconObject.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, 22f);
            rect.sizeDelta = new Vector2(52f, 24f);

            ActionIcon = iconObject.AddComponent<ActionIconUI>();
            ActionIcon.Hide();
        }

        private void EnsureSlider(ref Slider slider, string label, Vector2 anchoredPosition, Color fillColor)
        {
            if (slider != null)
            {
                return;
            }

            string objectName = label + "Slider";
            Transform existing = worldCanvas.transform.Find(objectName);
            if (existing != null && existing.TryGetComponent(out Slider existingSlider))
            {
                slider = existingSlider;
                return;
            }

            GameObject sliderObject = new GameObject(objectName);
            sliderObject.transform.SetParent(worldCanvas.transform, false);

            RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
            sliderRect.anchoredPosition = anchoredPosition;
            sliderRect.sizeDelta = new Vector2(96f, 9f);

            Image background = sliderObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.08f, 0.88f);
            background.raycastTarget = false;

            slider = sliderObject.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.interactable = false;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            RectTransform fillArea = CreateRect("Fill Area", sliderObject.transform);
            fillArea.anchorMin = new Vector2(0f, 0f);
            fillArea.anchorMax = new Vector2(1f, 1f);
            fillArea.offsetMin = new Vector2(1f, 1f);
            fillArea.offsetMax = new Vector2(-1f, -1f);

            RectTransform fill = CreateRect("Fill", fillArea);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(1f, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;

            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.raycastTarget = false;

            slider.fillRect = fill;
            slider.targetGraphic = fillImage;
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject child = new GameObject(objectName);
            child.transform.SetParent(parent, false);
            return child.AddComponent<RectTransform>();
        }

        private void UpdateBars()
        {
            if (Entity?.Stats == null)
            {
                return;
            }

            if (HpSlider != null)
            {
                HpSlider.value = Entity.Stats.MaxHP > 0 ? Mathf.Clamp01((float)Entity.Stats.CurrentHP / Entity.Stats.MaxHP) : 0f;
            }

            if (MpSlider != null)
            {
                MpSlider.value = Entity.Stats.MaxMP > 0 ? Mathf.Clamp01((float)Entity.Stats.CurrentMP / Entity.Stats.MaxMP) : 0f;
            }
        }

        private void FaceCamera()
        {
            Camera camera = Camera.main;
            if (worldCanvas == null || camera == null)
            {
                return;
            }

            worldCanvas.transform.rotation = camera.transform.rotation;
        }

        private void StartFlash(Color flashColor)
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine(flashColor));
        }

        private IEnumerator FlashRoutine(Color flashColor)
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            Color[] originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].color;
                renderers[i].color = flashColor;
            }

            yield return new WaitForSeconds(0.16f);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = originalColors[i];
                }
            }

            flashRoutine = null;
        }
    }
}

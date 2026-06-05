using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// 메인 메뉴 옵션 패널 컨트롤러.
    /// 직렬화 참조만 사용한다(런타임 hierarchy 경로 검색 없음).
    /// </summary>
    public sealed class OptionsPage : MonoBehaviour
    {
        [SerializeField]
        private GameObject panelRoot;

        [SerializeField]
        private Toggle fullscreenToggle;

        [SerializeField]
        private Slider volumeSlider;

        [SerializeField]
        private Button koreanButton;

        [SerializeField]
        private Button englishButton;

        [SerializeField]
        private Button applyButton;

        [SerializeField]
        private Button closeButton;

        private string pendingLanguage;
        private bool openingPanel;

        private static readonly Color ActiveLangColor = new Color(1f, 0.85f, 0.2f);
        private static readonly Color InactiveLangColor = Color.white;

        private void Awake()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            if (!openingPanel)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            WireButtons();
        }

        /// <summary>패널을 열고 현재 옵션 값으로 UI를 초기화한다.</summary>
        public void Open()
        {
            if (panelRoot != null)
            {
                panelRoot.transform.SetAsLastSibling();
                openingPanel = true;
                panelRoot.SetActive(true);
                openingPanel = false;
            }

            PopulateFromCurrent();
        }

        /// <summary>패널을 닫는다. 미적용 변경은 버려진다.</summary>
        public void Close()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        /// <summary>UI 값을 OptionsService에 적용하고 저장한다.</summary>
        public void OnApply()
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.Options == null)
            {
                Close();
                return;
            }

            OptionSnapshot current = gsm.Options.Current;
            bool isFullscreen =
                fullscreenToggle != null ? fullscreenToggle.isOn : current?.Fullscreen ?? true;
            var snapshot = new OptionSnapshot
            {
                LanguageCode = pendingLanguage ?? current?.LanguageCode ?? "ko",
                MasterVolume =
                    volumeSlider != null ? volumeSlider.value : current?.MasterVolume ?? 1f,
                Fullscreen = isFullscreen,
                ResolutionWidth = isFullscreen ? (current?.ResolutionWidth ?? 1920) : 1280,
                ResolutionHeight = isFullscreen ? (current?.ResolutionHeight ?? 1080) : 720,
            };

            gsm.Options.Apply(snapshot);
            Close();
        }

        private void PopulateFromCurrent()
        {
            if (
                !GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                || gsm.Options?.Current == null
            )
            {
                return;
            }

            OptionSnapshot opts = gsm.Options.Current;

            if (fullscreenToggle != null)
            {
                fullscreenToggle.SetIsOnWithoutNotify(opts.Fullscreen);
            }

            if (volumeSlider != null)
            {
                volumeSlider.SetValueWithoutNotify(opts.MasterVolume);
            }

            pendingLanguage = opts.LanguageCode;
            RefreshLanguageButtons();
        }

        private void WireButtons()
        {
            if (applyButton != null)
            {
                applyButton.onClick.RemoveListener(OnApply);
                applyButton.onClick.AddListener(OnApply);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }

            if (koreanButton != null)
            {
                koreanButton.onClick.RemoveListener(SelectKorean);
                koreanButton.onClick.AddListener(SelectKorean);
            }

            if (englishButton != null)
            {
                englishButton.onClick.RemoveListener(SelectEnglish);
                englishButton.onClick.AddListener(SelectEnglish);
            }
        }

        private void SelectKorean()
        {
            pendingLanguage = "ko";
            RefreshLanguageButtons();
        }

        private void SelectEnglish()
        {
            pendingLanguage = "en";
            RefreshLanguageButtons();
        }

        private void RefreshLanguageButtons()
        {
            SetButtonColor(koreanButton, pendingLanguage == "ko");
            SetButtonColor(englishButton, pendingLanguage == "en");
        }

        private static void SetButtonColor(Button btn, bool active)
        {
            if (btn == null)
            {
                return;
            }

            var img = btn.targetGraphic as Image;
            if (img != null)
            {
                img.color = active ? ActiveLangColor : InactiveLangColor;
            }
        }
    }
}

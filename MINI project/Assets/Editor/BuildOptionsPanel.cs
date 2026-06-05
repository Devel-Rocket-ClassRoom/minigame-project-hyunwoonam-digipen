using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tempt.Editor
{
    /// <summary>
    /// MainMenu 씬에 옵션 패널 계층과 직렬화 참조를 생성/연결한다.
    /// </summary>
    public static class BuildOptionsPanel
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("Tempt/Build Options Panel")]
        public static void Build()
        {
            OpenMainMenuScene();

            Canvas canvas = FindSceneObject<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[BuildOptionsPanel] MainMenu 씬에서 Canvas를 찾지 못했습니다.");
                return;
            }

            GameObject panel = RecreatePanel(canvas.transform);
            OptionsPage optionsPage = panel.AddComponent<OptionsPage>();

            Button closeButton;
            Button koreanButton;
            Button englishButton;
            Button applyButton;
            Toggle fullscreenToggle;
            Slider volumeSlider;
            BuildPanelContent(
                panel.transform,
                out fullscreenToggle,
                out volumeSlider,
                out koreanButton,
                out englishButton,
                out applyButton,
                out closeButton
            );

            WireOptionsPage(
                optionsPage,
                panel,
                fullscreenToggle,
                volumeSlider,
                koreanButton,
                englishButton,
                applyButton,
                closeButton
            );
            WireMainMenuController(optionsPage);
            WireMainMenuButtonLocalization();

            panel.SetActive(false);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[BuildOptionsPanel] OptionsPanel 생성 및 연결 완료.");
        }

        private static void OpenMainMenuScene()
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.path == ScenePath)
            {
                return;
            }

            if (active.isDirty && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                throw new System.OperationCanceledException("Scene save canceled.");
            }

            EditorSceneManager.OpenScene(ScenePath);
        }

        private static GameObject RecreatePanel(Transform canvas)
        {
            Transform existing = canvas.Find("OptionsPanel");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject panel = CreateRect(
                "OptionsPanel",
                canvas,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.zero
            );
            panel.transform.SetAsLastSibling();
            return panel;
        }

        private static void BuildPanelContent(
            Transform root,
            out Toggle fullscreenToggle,
            out Slider volumeSlider,
            out Button koreanButton,
            out Button englishButton,
            out Button applyButton,
            out Button closeButton
        )
        {
            AddImage(
                CreateRect(
                    "Background",
                    root,
                    Vector2.zero,
                    Vector2.one,
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    Vector2.zero
                ),
                new Color(0f, 0f, 0f, 0.72f)
            );

            GameObject body = CreateRect(
                "PanelBody",
                root,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(640f, 500f)
            );
            AddImage(body, new Color(0.075f, 0.075f, 0.085f, 0.98f));

            TMP_Text title = CreateLabel(
                "TitleLabel",
                body.transform,
                "OPTIONS",
                "opt_title",
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -32f),
                new Vector2(420f, 48f),
                28f,
                TextAlignmentOptions.Center
            );
            title.fontStyle = FontStyles.Bold;

            closeButton = CreateButton(
                "CloseButton",
                body.transform,
                "X",
                string.Empty,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-26f, -24f),
                new Vector2(42f, 36f)
            );

            AddImage(
                CreateRect(
                    "Divider",
                    body.transform,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -84f),
                    new Vector2(560f, 2f)
                ),
                new Color(0.35f, 0.35f, 0.38f, 1f)
            );

            GameObject displayRow = CreateRow(body.transform, "DisplayRow", -135f);
            CreateRowLabel(displayRow.transform, "DisplayLabel", "DISPLAY", "opt_display");
            CreateRowLabel(displayRow.transform, "FSLabel", "FULLSCREEN", "opt_fullscreen");
            fullscreenToggle = CreateToggle(displayRow.transform, "FullscreenToggle");

            GameObject audioRow = CreateRow(body.transform, "AudioRow", -205f);
            CreateRowLabel(audioRow.transform, "AudioLabel", "AUDIO", "opt_audio");
            CreateRowLabel(audioRow.transform, "VolLabel", "VOLUME", "opt_volume");
            volumeSlider = CreateSlider(audioRow.transform, "VolumeSlider");

            GameObject langRow = CreateRow(body.transform, "LangRow", -275f);
            CreateRowLabel(langRow.transform, "LangLabel", "LANGUAGE", "opt_language");
            koreanButton = CreateButton(
                "KoreanButton",
                langRow.transform,
                "한국어",
                string.Empty,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(300f, 0f),
                new Vector2(110f, 42f)
            );
            englishButton = CreateButton(
                "EnglishButton",
                langRow.transform,
                "ENGLISH",
                string.Empty,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(430f, 0f),
                new Vector2(110f, 42f)
            );

            applyButton = CreateButton(
                "ApplyButton",
                body.transform,
                "APPLY",
                "opt_apply",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 48f),
                new Vector2(180f, 48f)
            );
        }

        private static GameObject CreateRow(Transform parent, string name, float y)
        {
            GameObject row = CreateRect(
                name,
                parent,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, y),
                new Vector2(560f, 52f)
            );
            row.AddComponent<HorizontalLayoutGroup>().childControlWidth = false;
            return row;
        }

        private static void CreateRowLabel(Transform parent, string name, string text, string key)
        {
            CreateLabel(
                name,
                parent,
                text,
                key,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                name.EndsWith("Label") ? new Vector2(0f, 0f) : new Vector2(170f, 0f),
                new Vector2(name.EndsWith("Label") ? 150f : 150f, 42f),
                18f,
                TextAlignmentOptions.Left
            );
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string text,
            string localizationKey,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size
        )
        {
            GameObject root = CreateRect(
                name,
                parent,
                anchorMin,
                anchorMax,
                pivot,
                anchoredPosition,
                size
            );
            Image image = AddImage(root, new Color(0.16f, 0.16f, 0.18f, 1f));
            Button button = root.AddComponent<Button>();
            button.targetGraphic = image;

            CreateLabel(
                "Label",
                root.transform,
                text,
                localizationKey,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.zero,
                18f,
                TextAlignmentOptions.Center
            );
            return button;
        }

        private static Toggle CreateToggle(Transform parent, string name)
        {
            GameObject root = CreateRect(
                name,
                parent,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(410f, 0f),
                new Vector2(36f, 36f)
            );
            Toggle toggle = root.AddComponent<Toggle>();
            Image background = AddImage(root, new Color(0.18f, 0.18f, 0.2f, 1f));
            GameObject checkmark = CreateRect(
                "Checkmark",
                root.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(22f, 22f)
            );
            Image checkImage = AddImage(checkmark, new Color(1f, 0.82f, 0.24f, 1f));
            toggle.targetGraphic = background;
            toggle.graphic = checkImage;
            return toggle;
        }

        private static Slider CreateSlider(Transform parent, string name)
        {
            GameObject root = CreateRect(
                name,
                parent,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(300f, 0f),
                new Vector2(230f, 28f)
            );
            Slider slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            GameObject background = CreateRect(
                "Background",
                root.transform,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.zero
            );
            AddImage(background, new Color(0.15f, 0.15f, 0.16f, 1f));

            GameObject fillArea = CreateRect(
                "Fill Area",
                root.transform,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(-22f, 0f)
            );
            GameObject fill = CreateRect(
                "Fill",
                fillArea.transform,
                Vector2.zero,
                Vector2.one,
                new Vector2(0f, 0.5f),
                Vector2.zero,
                Vector2.zero
            );
            AddImage(fill, new Color(1f, 0.82f, 0.24f, 1f));

            GameObject handleArea = CreateRect(
                "Handle Slide Area",
                root.transform,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(-22f, 0f)
            );
            GameObject handle = CreateRect(
                "Handle",
                handleArea.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(22f, 28f)
            );
            Image handleImage = AddImage(handle, new Color(0.9f, 0.9f, 0.92f, 1f));

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImage;
            return slider;
        }

        private static TMP_Text CreateLabel(
            string name,
            Transform parent,
            string text,
            string localizationKey,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size,
            float fontSize,
            TextAlignmentOptions alignment
        )
        {
            GameObject root = CreateRect(
                name,
                parent,
                anchorMin,
                anchorMax,
                pivot,
                anchoredPosition,
                size
            );
            TextMeshProUGUI label = root.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = alignment;

            if (!string.IsNullOrEmpty(localizationKey))
            {
                LocalizedText localized = root.AddComponent<LocalizedText>();
                var serialized = new SerializedObject(localized);
                serialized.FindProperty("key").stringValue = localizationKey;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return label;
        }

        private static GameObject CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size
        )
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return root;
        }

        private static Image AddImage(GameObject root, Color color)
        {
            Image image = root.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static void WireOptionsPage(
            OptionsPage optionsPage,
            GameObject panel,
            Toggle fullscreenToggle,
            Slider volumeSlider,
            Button koreanButton,
            Button englishButton,
            Button applyButton,
            Button closeButton
        )
        {
            var serialized = new SerializedObject(optionsPage);
            serialized.FindProperty("panelRoot").objectReferenceValue = panel;
            serialized.FindProperty("fullscreenToggle").objectReferenceValue = fullscreenToggle;
            serialized.FindProperty("volumeSlider").objectReferenceValue = volumeSlider;
            serialized.FindProperty("koreanButton").objectReferenceValue = koreanButton;
            serialized.FindProperty("englishButton").objectReferenceValue = englishButton;
            serialized.FindProperty("applyButton").objectReferenceValue = applyButton;
            serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireMainMenuController(OptionsPage optionsPage)
        {
            MainMenuController controller = FindSceneObject<MainMenuController>();
            if (controller == null)
            {
                Debug.LogError("[BuildOptionsPanel] MainMenuController를 찾지 못했습니다.");
                return;
            }

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("optionsPage").objectReferenceValue = optionsPage;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireMainMenuButtonLocalization()
        {
            WireButtonText("NewGameButton", "menu_new_game");
            WireButtonText("ContinueButton", "menu_continue");
            WireButtonText("OptionButton", "menu_options");
            WireButtonText("QuitButton", "menu_exit");
        }

        private static void WireButtonText(string buttonName, string localizationKey)
        {
            GameObject button = GameObject.Find(buttonName);
            TMP_Text label = button != null ? button.GetComponentInChildren<TMP_Text>(true) : null;
            if (label == null)
            {
                Debug.LogWarning(
                    "[BuildOptionsPanel] 버튼 텍스트를 찾지 못했습니다: " + buttonName
                );
                return;
            }

            LocalizedText localized = label.GetComponent<LocalizedText>();
            if (localized == null)
            {
                localized = label.gameObject.AddComponent<LocalizedText>();
            }

            var serialized = new SerializedObject(localized);
            serialized.FindProperty("key").stringValue = localizationKey;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T FindSceneObject<T>()
            where T : Object
        {
            T[] objects = Resources.FindObjectsOfTypeAll<T>();
            Scene active = SceneManager.GetActiveScene();
            foreach (T obj in objects)
            {
                if (obj is Component component && component.gameObject.scene == active)
                {
                    return obj;
                }
            }

            return null;
        }
    }
}

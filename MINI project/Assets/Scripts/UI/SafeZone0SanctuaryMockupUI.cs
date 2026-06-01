using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    public sealed class SafeZone0SanctuaryMockupUI : MonoBehaviour
    {
        [SerializeField]
        private Safe0Controller controller;

        private readonly List<Button> runeButtons = new List<Button>();

        private GameObject runeSelectPanel;
        private GameObject blackStelePanel;
        private GameObject graveyardPanel;
        private Button blackSteleHotspot;
        private Button graveyardHotspot;
        private Button confirmRuneButton;
        private Button enterFloorMapButton;
        private int selectedRuneIndex;

        private Color selectedCard = new Color(0.16f, 0.035f, 0.047f, 0.96f);
        private Color normalCard = new Color(0.09f, 0.09f, 0.09f, 0.94f);
        private Color selectedBorder = new Color(1f, 0.20f, 0.27f, 0.85f);
        private Color normalBorder = new Color(0.47f, 0.47f, 0.47f, 0.45f);
        private Color activeText = new Color(0.94f, 0.94f, 0.94f, 1f);
        private Color inactiveText = new Color(0.46f, 0.46f, 0.52f, 1f);

        private void Awake()
        {
            InitializeMockup();
        }

        public void InitializeMockup()
        {
            CacheHierarchy();
            WireButtons();
            selectedRuneIndex = 0;
            SetRuneSelection(0);
            if (IsStartingRuneConfirmed())
            {
                HideRuneSelect();
            }
            else
            {
                ShowRuneSelect();
            }
        }

        public void ShowRuneSelect()
        {
            SetPanelActive(runeSelectPanel, true);
            SetPanelActive(blackStelePanel, false);
            SetPanelActive(graveyardPanel, false);
            SetHotspotsInteractable(false);
            SetEnterFloorMapInteractable(false);
        }

        public void ConfirmRune()
        {
            if (controller == null)
            {
                Debug.LogError(
                    "[SafeZone0SanctuaryMockupUI] Safe0Controller 참조가 씬에 직접 할당되어 있지 않습니다."
                );
                return;
            }

            controller.ApplyStartingRuneClass(RuneClassFromSelection(selectedRuneIndex));
            if (IsStartingRuneConfirmed())
            {
                HideRuneSelect();
            }
        }

        public void OpenBlackStele()
        {
            if (runeSelectPanel != null && runeSelectPanel.activeSelf)
                return;
            SetPanelActive(blackStelePanel, true);
            SetPanelActive(graveyardPanel, false);
        }

        public void CloseBlackStele()
        {
            SetPanelActive(blackStelePanel, false);
        }

        public void OpenGraveyard()
        {
            if (runeSelectPanel != null && runeSelectPanel.activeSelf)
                return;
            SetPanelActive(graveyardPanel, true);
            SetPanelActive(blackStelePanel, false);
        }

        public void CloseGraveyard()
        {
            SetPanelActive(graveyardPanel, false);
        }

        public bool TryCloseTopPanel()
        {
            CacheHierarchy();

            if (blackStelePanel != null && blackStelePanel.activeSelf)
            {
                CloseBlackStele();
                return true;
            }

            if (graveyardPanel != null && graveyardPanel.activeSelf)
            {
                CloseGraveyard();
                return true;
            }

            return false;
        }

        public bool IsRuneSelectionBlocking()
        {
            return !IsStartingRuneConfirmed();
        }

        public void EnterFloorMap()
        {
            if (controller == null)
            {
                Debug.LogError(
                    "[SafeZone0SanctuaryMockupUI] Safe0Controller 참조가 씬에 직접 할당되어 있지 않습니다."
                );
                return;
            }

            if (IsRuneSelectionBlocking())
            {
                ShowRuneSelect();
                return;
            }

            controller.DepartToFloorMap();
        }

        public void SetRuneSelection(int index)
        {
            selectedRuneIndex = Mathf.Clamp(index, 0, runeButtons.Count - 1);
            for (int i = 0; i < runeButtons.Count; i++)
            {
                bool selected = i == selectedRuneIndex;
                Button button = runeButtons[i];
                Graphic graphic = button.targetGraphic;
                if (graphic != null)
                    graphic.color = selected ? selectedCard : normalCard;

                Outline outline = button.GetComponent<Outline>();
                if (outline != null)
                    outline.effectColor = selected ? selectedBorder : normalBorder;

                TextMeshProUGUI[] texts = button.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI text in texts)
                {
                    if (text.name == "RuneName")
                    {
                        text.color = selected ? activeText : inactiveText;
                    }
                }
            }
        }

        private void CacheHierarchy()
        {
            runeButtons.Clear();
            Transform panels = transform.Find("Panels");
            runeSelectPanel = panels != null ? panels.Find("RuneSelectPanel")?.gameObject : null;
            blackStelePanel = panels != null ? panels.Find("BlackStelePanel")?.gameObject : null;
            graveyardPanel = panels != null ? panels.Find("GraveyardPanel")?.gameObject : null;

            blackSteleHotspot = FindButton("BackgroundHotspots/BlackSteleHotspotButton");
            graveyardHotspot = FindButton("BackgroundHotspots/GraveyardHotspotButton");
            confirmRuneButton = FindButton(
                "Panels/RuneSelectPanel/SelectedRuneDetail/DetailCard/ConfirmRuneButton"
            );
            enterFloorMapButton = FindButton("EnterFloorMapButton");

            Transform grid = transform.Find("Panels/RuneSelectPanel/RuneCardGrid");
            if (grid != null)
            {
                for (int i = 0; i < grid.childCount; i++)
                {
                    if (grid.GetChild(i).TryGetComponent(out Button button))
                    {
                        runeButtons.Add(button);
                    }
                }
            }
        }

        private void WireButtons()
        {
            if (blackSteleHotspot != null)
            {
                blackSteleHotspot.onClick.RemoveAllListeners();
                blackSteleHotspot.onClick.AddListener(OpenBlackStele);
            }

            if (graveyardHotspot != null)
            {
                graveyardHotspot.onClick.RemoveAllListeners();
                graveyardHotspot.onClick.AddListener(OpenGraveyard);
            }

            if (confirmRuneButton != null)
            {
                confirmRuneButton.onClick.RemoveAllListeners();
                confirmRuneButton.onClick.AddListener(ConfirmRune);
            }

            if (enterFloorMapButton != null)
            {
                enterFloorMapButton.onClick.RemoveAllListeners();
                enterFloorMapButton.onClick.AddListener(EnterFloorMap);
            }

            for (int i = 0; i < runeButtons.Count; i++)
            {
                int capturedIndex = i;
                runeButtons[i].onClick.RemoveAllListeners();
                runeButtons[i].onClick.AddListener(() => SetRuneSelection(capturedIndex));
            }

            Button closeStele = FindButton("Panels/BlackStelePanel/Header/CloseButton");
            if (closeStele != null)
            {
                closeStele.onClick.RemoveAllListeners();
                closeStele.onClick.AddListener(CloseBlackStele);
            }

            Button closeGraveyard = FindButton("Panels/GraveyardPanel/Header/CloseButton");
            if (closeGraveyard != null)
            {
                closeGraveyard.onClick.RemoveAllListeners();
                closeGraveyard.onClick.AddListener(CloseGraveyard);
            }
        }

        private Button FindButton(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private void HideRuneSelect()
        {
            SetPanelActive(runeSelectPanel, false);
            SetHotspotsInteractable(true);
            SetEnterFloorMapInteractable(true);
        }

        private bool IsStartingRuneConfirmed()
        {
            if (controller != null)
            {
                return controller.HasConfirmedStartingRune();
            }

            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return false;
            }

            PlayerState player = gsm.CurrentRun?.Player;
            return player != null
                && player.StartingClass != RuneClass.None
                && player.Rune != null
                && player.Rune.ClassId != RuneClass.None;
        }

        private static RuneClass RuneClassFromSelection(int index)
        {
            switch (index)
            {
                case 1:
                    return RuneClass.Tanker;
                case 2:
                    return RuneClass.MagicDealer;
                case 3:
                    return RuneClass.Supporter;
                default:
                    return RuneClass.Dealer;
            }
        }

        private void SetHotspotsInteractable(bool interactable)
        {
            if (blackSteleHotspot != null)
                blackSteleHotspot.interactable = interactable;
            if (graveyardHotspot != null)
                graveyardHotspot.interactable = interactable;
        }

        private void SetEnterFloorMapInteractable(bool interactable)
        {
            if (enterFloorMapButton != null)
            {
                enterFloorMapButton.interactable = interactable;
            }
        }

        private static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }
    }
}

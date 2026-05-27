using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// Safe1 mockup interaction layer. Keeps the static design inspectable while making
    /// facility tabs and sub-function buttons switch the matching RectTransform groups.
    /// </summary>
    public sealed class SafeZone1FacilityMockupUI : MonoBehaviour
    {
        private static readonly string[] Facilities = { "TAVERN", "SHOP", "GUILD", "FORGE", "SHRINE" };

        private readonly Dictionary<string, GameObject> facilityGroups = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Button> facilityTabs = new Dictionary<string, Button>();
        private readonly Dictionary<string, List<Button>> subButtons = new Dictionary<string, List<Button>>();
        private readonly Dictionary<string, List<GameObject>> subContents = new Dictionary<string, List<GameObject>>();

        private Color activeText = new Color(0.94f, 0.94f, 0.94f, 1f);
        private Color inactiveText = new Color(0.46f, 0.46f, 0.52f, 1f);
        private Color activeButton = new Color(0.16f, 0.035f, 0.047f, 1f);
        private Color inactiveButton = new Color(0.09f, 0.09f, 0.09f, 1f);
        private Color redText = new Color(1f, 0.20f, 0.27f, 1f);

        private void Awake()
        {
            InitializeMockup();
        }

        public void InitializeMockup()
        {
            CacheHierarchy();
            WireButtons();
            ShowFacility("TAVERN");
        }

        public void ShowTavern() => ShowFacility("TAVERN");
        public void ShowShop() => ShowFacility("SHOP");
        public void ShowGuild() => ShowFacility("GUILD");
        public void ShowForge() => ShowFacility("FORGE");
        public void ShowShrine() => ShowFacility("SHRINE");

        public void ShowFacility(string facility)
        {
            foreach (string key in Facilities)
            {
                bool active = key == facility;
                if (facilityGroups.TryGetValue(key, out GameObject group))
                {
                    group.SetActive(active);
                }

                if (facilityTabs.TryGetValue(key, out Button tab))
                {
                    SetTabVisual(tab, active);
                }
            }

            ShowSubFunction(facility, DefaultSubFunctionIndex(facility));
        }

        public void ShowSubFunction(string facility, int index)
        {
            if (subContents.TryGetValue(facility, out List<GameObject> contents))
            {
                for (int i = 0; i < contents.Count; i++)
                {
                    contents[i].SetActive(i == index);
                }
            }

            if (subButtons.TryGetValue(facility, out List<Button> buttons))
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    SetSubButtonVisual(buttons[i], i == index);
                }
            }
        }

        private void CacheHierarchy()
        {
            facilityGroups.Clear();
            facilityTabs.Clear();
            subButtons.Clear();
            subContents.Clear();

            foreach (string facility in Facilities)
            {
                Transform group = transform.Find("MainFacilityPanel/Facility_" + facility);
                if (group != null)
                {
                    facilityGroups[facility] = group.gameObject;
                    subButtons[facility] = FindButtons(group.Find("SubFunctionRow"));
                    subContents[facility] = FindContentPanels(group.Find("MiddleContentArea"));
                }

                Transform tab = transform.Find("BottomNavigationBar/FacilityTabs/FacilityTab_" + facility);
                if (tab != null && tab.TryGetComponent(out Button button))
                {
                    facilityTabs[facility] = button;
                }
            }
        }

        private void WireButtons()
        {
            foreach (string facility in Facilities)
            {
                if (facilityTabs.TryGetValue(facility, out Button tab))
                {
                    tab.onClick.RemoveAllListeners();
                    string capturedFacility = facility;
                    tab.onClick.AddListener(() => ShowFacility(capturedFacility));
                }

                if (!subButtons.TryGetValue(facility, out List<Button> buttons))
                {
                    continue;
                }

                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].onClick.RemoveAllListeners();
                    string capturedFacility = facility;
                    int capturedIndex = i;
                    buttons[i].onClick.AddListener(() => ShowSubFunction(capturedFacility, capturedIndex));
                }
            }
        }

        private static List<Button> FindButtons(Transform row)
        {
            List<Button> buttons = new List<Button>();
            if (row == null) return buttons;

            for (int i = 0; i < row.childCount; i++)
            {
                if (row.GetChild(i).TryGetComponent(out Button button))
                {
                    buttons.Add(button);
                }
            }

            return buttons;
        }

        private static int DefaultSubFunctionIndex(string facility)
        {
            return facility == "TAVERN" ? 1 : 0;
        }

        private static List<GameObject> FindContentPanels(Transform area)
        {
            List<GameObject> panels = new List<GameObject>();
            if (area == null) return panels;

            for (int i = 0; i < area.childCount; i++)
            {
                panels.Add(area.GetChild(i).gameObject);
            }

            return panels;
        }

        private void SetTabVisual(Button button, bool active)
        {
            SetGraphicColor(button.targetGraphic, Color.clear);
            SetChildTextColor(button.transform, active ? activeText : inactiveText);

            Transform underline = button.transform.Find("GoldUnderline");
            if (underline != null)
            {
                underline.gameObject.SetActive(active);
            }
        }

        private void SetSubButtonVisual(Button button, bool active)
        {
            SetGraphicColor(button.targetGraphic, active ? activeButton : inactiveButton);
            SetChildTextColor(button.transform, active ? redText : inactiveText);

            Outline outline = button.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = active ? new Color(1f, 0.20f, 0.27f, 0.55f) : new Color(0.47f, 0.47f, 0.47f, 0.35f);
            }

            Transform underline = button.transform.Find("ActiveUnderline");
            if (underline != null)
            {
                underline.gameObject.SetActive(active);
            }
        }

        private static void SetGraphicColor(Graphic graphic, Color color)
        {
            if (graphic != null)
            {
                graphic.color = color;
            }
        }

        private static void SetChildTextColor(Transform root, Color color)
        {
            TextMeshProUGUI text = root.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                text.color = color;
            }
        }
    }
}

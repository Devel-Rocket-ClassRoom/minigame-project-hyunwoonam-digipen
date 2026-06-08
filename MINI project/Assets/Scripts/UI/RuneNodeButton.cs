using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class RuneNodeButton : MonoBehaviour
    {
        public enum VisualState
        {
            Locked,
            Unlockable,
            Unlocked,
        }

        [Header("References")]
        [SerializeField]
        private Button button;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Image frame;

        [SerializeField]
        private TMP_Text costLabel;

        [SerializeField]
        private TMP_Text nodeLabel;

        [Header("Colors")]
        [SerializeField]
        private Color lockedColor = new Color(0.20f, 0.20f, 0.23f, 0.85f);

        [SerializeField]
        private Color unlockableColor = new Color(0.83f, 0.73f, 0.40f, 1f);

        [SerializeField]
        private Color unlockedColor = new Color(0.82f, 0.82f, 0.82f, 1f);

        [SerializeField]
        private Color selectedColor = new Color(0.95f, 0.25f, 0.28f, 1f);

        private RuneTreeView owner;

        public int NodeId { get; private set; }

        private void Awake()
        {
            EnsureButton();
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        public void Initialize(RuneTreeView view, RuneData runeData)
        {
            EnsureButton();
            owner = view;
            NodeId = runeData != null ? runeData.Id : 0;

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
                button.onClick.AddListener(HandleClicked);
            }

            if (nodeLabel != null)
            {
                nodeLabel.text = NodeId != 0 ? NodeId.ToString() : string.Empty;
            }
        }

        public void SetState(
            VisualState visualState,
            int investedPoints,
            int requiredPoints,
            bool selected,
            bool inspectableWhenLocked = false
        )
        {
            EnsureButton();
            bool interactable =
                visualState != VisualState.Locked || inspectableWhenLocked;
            if (button != null)
            {
                button.interactable = interactable;
            }

            Color color = ColorFor(visualState);
            SetGraphicColor(icon, color);
            SetGraphicColor(frame, selected ? selectedColor : color);
            if (button?.targetGraphic != null)
            {
                button.targetGraphic.color = color;
            }

            if (costLabel != null)
            {
                costLabel.text =
                    System.Math.Max(0, investedPoints) + "/" + System.Math.Max(0, requiredPoints);
            }
        }

        private Color ColorFor(VisualState visualState)
        {
            switch (visualState)
            {
                case VisualState.Unlockable:
                    return unlockableColor;
                case VisualState.Unlocked:
                    return unlockedColor;
                default:
                    return lockedColor;
            }
        }

        private void HandleClicked()
        {
            owner?.OnNodeClicked(NodeId);
        }

        private void EnsureButton()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
        }

        private static void SetGraphicColor(Graphic graphic, Color color)
        {
            if (graphic != null)
            {
                graphic.color = color;
            }
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// FloorMap 에 표시되는 단일 노드 버튼.
    /// </summary>
    public sealed class FloorNodeUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI detailLabel;

        private FloorNode node;
        private Action<int> onClicked;
        private Outline outline;
        private bool safeLocked;
        private float erosionRate;
        private bool fullyEroded;

        /// <summary>RectTransform 캐시.</summary>
        public RectTransform RectTransform => (RectTransform)transform;

        public int StageIndex => node != null ? node.StageIndex : 0;

        public bool IsSafeZone => node != null && node.IsSafeZone;

        /// <summary>
        /// 노드 데이터와 클릭 콜백을 연결한다.
        /// </summary>
        public void Bind(FloorNode floorNode, Action<int> clickHandler)
        {
            node = floorNode;
            onClicked = clickHandler;
            CacheVisuals();
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
                if (background != null)
                {
                    button.targetGraphic = background;
                }
            }

            Refresh();
        }

        /// <summary>
        /// 버튼 hit area 를 명확한 최소 크기로 고정한다.
        /// </summary>
        public void SetHitSize(Vector2 size)
        {
            RectTransform.sizeDelta = size;
            if (background != null)
            {
                background.raycastTarget = true;
            }
        }

        /// <summary>
        /// 선택 가능 여부를 버튼 상태에 반영한다.
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }

            if (background != null)
            {
                background.color = BackgroundColor(interactable);
            }

            if (outline != null)
            {
                outline.enabled = interactable || (node != null && (node.IsCleared || node.IsSafeZone));
                outline.effectColor = fullyEroded && node != null && !node.IsSafeZone
                    ? new Color(0.82f, 0.44f, 1f, 0.95f)
                    : node != null && node.IsSafeZone
                    ? new Color(0.50f, 0.92f, 0.92f, 0.90f)
                    : interactable
                    ? new Color(1f, 0.78f, 0.34f, 0.95f)
                    : new Color(0.44f, 0.56f, 0.48f, 0.75f);
                outline.effectDistance = new Vector2(3f, -3f);
            }

            if (titleLabel != null)
            {
                titleLabel.color = interactable ? Color.white : new Color(0.58f, 0.60f, 0.66f, 1f);
            }

            if (detailLabel != null && node != null)
            {
                detailLabel.text = DetailText(interactable);
                detailLabel.color = interactable ? new Color(1f, 0.82f, 0.45f, 1f) : new Color(0.55f, 0.57f, 0.62f, 1f);
            }
        }

        public void SetErosionRate(float rate)
        {
            erosionRate = Mathf.Clamp(rate, 0f, 100f);
            SetFullyEroded(erosionRate >= 100f);
        }

        public void SetFullyEroded(bool eroded)
        {
            fullyEroded = eroded;
            if (button != null)
            {
                SetInteractable(button.interactable);
            }

            RefreshDetailOnly();
        }

        public void SetSafeLocked(bool locked)
        {
            safeLocked = locked;
            RefreshDetailOnly();
            if (background != null && node != null && node.IsSafeZone && locked)
            {
                background.color = new Color(0.12f, 0.07f, 0.13f, 0.95f);
            }
        }

        private void Refresh()
        {
            if (node == null)
            {
                return;
            }

            if (titleLabel != null)
            {
                if (node.IsSafeZone)
                {
                    titleLabel.text = $"SAFE{node.StageIndex}";
                }
                else
                {
                    titleLabel.text = node.IsBoss ? $"F{node.Floor} BOSS" : $"F{node.Floor}";
                }
            }

            if (detailLabel != null)
            {
                detailLabel.text = node.IsSafeZone ? SafeDetailText(false) : node.IsCleared ? "CLEARED" : ErosionDetailText();
            }
        }

        private string DetailText(bool interactable)
        {
            if (node == null)
            {
                return string.Empty;
            }

            if (node.IsSafeZone)
            {
                return interactable ? SafeDetailText(true) : SafeDetailText(false);
            }

            if (node.IsCleared)
            {
                return interactable ? "RETRY" : "CLEARED";
            }

            if (fullyEroded)
            {
                return interactable ? "ERODED READY" : "ERODED";
            }

            return interactable ? "READY" : string.Empty;
        }

        private string SafeDetailText(bool interactable)
        {
            if (safeLocked)
            {
                return $"LOCKED STAGE {node.StageIndex} {erosionRate:0.#}%";
            }

            return interactable ? $"ENTER STAGE {node.StageIndex} {erosionRate:0.#}%" : $"STAGE {node.StageIndex} {erosionRate:0.#}%";
        }

        private string ErosionDetailText()
        {
            return fullyEroded ? "ERODED" : string.Empty;
        }

        private Color BackgroundColor(bool interactable)
        {
            if (node != null && node.IsSafeZone)
            {
                return interactable
                    ? new Color(0.08f, 0.28f, 0.30f, 1f)
                    : new Color(0.08f, 0.15f, 0.17f, 0.88f);
            }

            if (fullyEroded)
            {
                return interactable
                    ? new Color(0.38f, 0.13f, 0.55f, 1f)
                    : new Color(0.22f, 0.10f, 0.32f, 0.95f);
            }

            if (node != null && node.IsCleared)
            {
                return new Color(0.18f, 0.32f, 0.24f, 0.98f);
            }

            return interactable
                ? new Color(0.36f, 0.08f, 0.12f, 1f)
                : new Color(0.09f, 0.10f, 0.13f, 0.88f);
        }

        private void RefreshDetailOnly()
        {
            if (detailLabel == null || node == null)
            {
                return;
            }

            detailLabel.text = node.IsSafeZone
                ? SafeDetailText(button != null && button.interactable)
                : node.IsCleared
                    ? "CLEARED"
                    : ErosionDetailText();
        }

        private void CacheVisuals()
        {
            if (background == null)
            {
                return;
            }

            outline = background.GetComponent<Outline>();
            if (outline == null)
            {
                outline = background.gameObject.AddComponent<Outline>();
            }
        }

        private void HandleClick()
        {
            if (node == null)
            {
                return;
            }

            onClicked?.Invoke(node.NodeId);
        }
    }
}

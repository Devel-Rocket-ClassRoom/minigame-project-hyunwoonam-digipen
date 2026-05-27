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

        /// <summary>RectTransform 캐시.</summary>
        public RectTransform RectTransform => (RectTransform)transform;

        /// <summary>
        /// 노드 데이터와 클릭 콜백을 연결한다.
        /// </summary>
        public void Bind(FloorNode floorNode, Action<int> clickHandler)
        {
            node = floorNode;
            onClicked = clickHandler;
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
            }

            Refresh();
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
                if (node != null && node.IsCleared)
                {
                    background.color = new Color(0.20f, 0.30f, 0.24f, 0.95f);
                }
                else
                {
                    background.color = interactable
                        ? new Color(0.18f, 0.09f, 0.10f, 0.98f)
                        : new Color(0.10f, 0.10f, 0.12f, 0.82f);
                }
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
                titleLabel.text = node.IsBoss ? $"F{node.Floor} BOSS" : $"F{node.Floor}";
            }

            if (detailLabel != null)
            {
                detailLabel.text = node.IsCleared ? "CLEARED" : $"D{node.Difficulty}  x{node.MonsterCount}";
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

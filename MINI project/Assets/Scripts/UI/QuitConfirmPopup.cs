using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    public sealed class QuitConfirmPopup : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action confirmAction;
        private Action cancelAction;

        private void Awake()
        {
            ResolveReferences();
            Hide();
        }

        public void Show(Action onConfirm, Action onCancel)
        {
            ResolveReferences();
            confirmAction = onConfirm;
            cancelAction = onCancel;
            WireButtons();
            if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void Hide()
        {
            if (root == null)
            {
                root = gameObject;
            }

            root.SetActive(false);
        }

        public void Confirm()
        {
            Action action = confirmAction;
            Hide();
            action?.Invoke();
        }

        public void Cancel()
        {
            Action action = cancelAction;
            Hide();
            action?.Invoke();
        }

        private void ResolveReferences()
        {
            if (root == null)
            {
                root = gameObject;
            }

            if (confirmButton == null)
            {
                Transform found = transform.Find("ConfirmButton");
                if (found != null)
                {
                    confirmButton = found.GetComponent<Button>();
                }
            }

            if (cancelButton == null)
            {
                Transform found = transform.Find("CancelButton");
                if (found != null)
                {
                    cancelButton = found.GetComponent<Button>();
                }
            }
        }

        private void WireButtons()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(Confirm);
                confirmButton.onClick.AddListener(Confirm);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(Cancel);
                cancelButton.onClick.AddListener(Cancel);
            }
        }
    }
}

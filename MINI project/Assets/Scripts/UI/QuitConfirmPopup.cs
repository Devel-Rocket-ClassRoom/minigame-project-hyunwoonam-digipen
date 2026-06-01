using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    public sealed class QuitConfirmPopup : MonoBehaviour
    {
        [SerializeField]
        private GameObject root;

        [SerializeField]
        private Button confirmButton;

        [SerializeField]
        private Button cancelButton;

        private Action confirmAction;
        private Action cancelAction;

        public bool IsOpen => root != null && root.activeSelf;

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
                root.transform.SetAsLastSibling();
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

            if (confirmButton == null)
            {
                confirmButton = FindButtonByName("YES", "ConfirmButton", "ExitButton");
            }

            if (cancelButton == null)
            {
                cancelButton = FindButtonByName("NO", "CancelButton");
            }
        }

        private Button FindButtonByName(params string[] names)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                string buttonName = buttons[i].name;
                for (int j = 0; j < names.Length; j++)
                {
                    if (string.Equals(buttonName, names[j], StringComparison.OrdinalIgnoreCase))
                    {
                        return buttons[i];
                    }
                }
            }

            return null;
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

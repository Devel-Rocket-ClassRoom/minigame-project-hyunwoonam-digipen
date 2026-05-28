using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// 머리 위 액션 아이콘. 빨강(공격/스킬) / 파랑(방어) 두 모드.
    /// </summary>
    public sealed class ActionIconUI : MonoBehaviour
    {
        /// <summary>빨강 이미지(Inspector 연결).</summary>
        public Image RedImage;

        /// <summary>파랑 이미지.</summary>
        public Image BlueImage;

        /// <summary>
        /// 모드 설정.
        /// </summary>
        public void SetMode(ActionIconMode mode)
        {
            EnsureImages();
            RedImage.enabled = mode == ActionIconMode.AttackRed;
            BlueImage.enabled = mode == ActionIconMode.DefendBlue;
        }

        /// <summary>모두 숨김.</summary>
        public void Hide()
        {
            EnsureImages();
            RedImage.enabled = false;
            BlueImage.enabled = false;
        }

        private void EnsureImages()
        {
            RedImage = RedImage != null
                ? RedImage
                : CreateImage("AttackRed", new Color(0.9f, 0.08f, 0.04f, 0.95f), new Vector2(-12f, 0f));
            BlueImage = BlueImage != null
                ? BlueImage
                : CreateImage("DefendBlue", new Color(0.08f, 0.28f, 0.95f, 0.95f), new Vector2(12f, 0f));
        }

        private Image CreateImage(string objectName, Color color, Vector2 anchoredPosition)
        {
            Transform existing = transform.Find(objectName);
            if (existing != null && existing.TryGetComponent(out Image existingImage))
            {
                existingImage.raycastTarget = false;
                existingImage.color = color;
                return existingImage;
            }

            GameObject imageObject = new GameObject(objectName);
            imageObject.transform.SetParent(transform, false);

            RectTransform rect = imageObject.AddComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(20f, 20f);

            Image image = imageObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.color = color;
            return image;
        }
    }

    /// <summary>액션 아이콘 모드.</summary>
    public enum ActionIconMode
    {
        /// <summary>없음.</summary>
        Hidden,

        /// <summary>공격/스킬(빨강).</summary>
        AttackRed,

        /// <summary>방어(파랑).</summary>
        DefendBlue,
    }
}

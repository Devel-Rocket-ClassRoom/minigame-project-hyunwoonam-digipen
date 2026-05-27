using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 머리 위 액션 아이콘. 빨강(공격/스킬) / 파랑(방어) 두 모드.
    /// </summary>
    public sealed class ActionIconUI : MonoBehaviour
    {
        /// <summary>빨강 이미지(Inspector 연결).</summary>
        public UnityEngine.UI.Image RedImage;

        /// <summary>파랑 이미지.</summary>
        public UnityEngine.UI.Image BlueImage;

        /// <summary>
        /// 모드 설정.
        /// </summary>
        public void SetMode(ActionIconMode mode)
        {
            // 동작 요약:
            // - AttackRed → Red 켜기, Blue 끄기.
            // - DefendBlue → Blue 켜기, Red 끄기.
            // - Hidden → 둘 다 끄기.
            //TODO: RedImage.enabled  = mode == ActionIconMode.AttackRed;
            //TODO: BlueImage.enabled = mode == ActionIconMode.DefendBlue;
        }

        /// <summary>모두 숨김.</summary>
        public void Hide()
        {
            // 동작 요약: 둘 다 비활성.
            //TODO: RedImage.enabled  = false;
            //TODO: BlueImage.enabled = false;
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


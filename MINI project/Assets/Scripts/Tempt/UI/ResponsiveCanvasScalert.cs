using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 화면비/해상도 변경 시 UI 비율 유지 + 글자 크기 자동 보정.
    /// </summary>
    public sealed class ResponsiveCanvasScalert : MonoBehaviour
    {
        /// <summary>기준 해상도.</summary>
        public Vector2 ReferenceResolution = new Vector2(1920, 1080);

        /// <summary>
        /// 해상도 변경 시 호출 또는 매 프레임 검사.
        /// </summary>
        public void Apply()
        {
            // 동작 요약:
            // - CanvasScaler 모드를 'Scale With Screen Size'로 설정.
            // - matchWidthOrHeight = 0.5 등(가로/세로 균형).
            // - TextMeshPro autoSize 또는 각 폰트의 자동 줄바꿈/축소 설정 일괄 적용.
            // - 옵션 페이지에서 해상도 변경 시 호출.
        }
    }
}

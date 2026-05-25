using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 타겟팅 검은 직선. 하단 캔버스에 마우스 호버 타겟까지 라인 표시.
    /// </summary>
    public sealed class TargetingLineUIt : MonoBehaviour
    {
        /// <summary>라인 렌더러(Canvas의 UI Line).</summary>
        public UnityEngine.UI.RawImage LineImage;

        /// <summary>
        /// 보이기/숨기기.
        /// </summary>
        public void Show(bool visible)
        {
            // 동작 요약: LineImage.enabled = visible.
        }

        /// <summary>
        /// 시작점과 끝점 갱신(스크린 좌표).
        /// </summary>
        public void SetPoints(Vector2 fromScreen, Vector2 toScreen)
        {
            // 동작 요약:
            // - 두 점 사이에 LineImage 위치/회전/길이 적용(2D 변환 행렬).
        }
    }
}

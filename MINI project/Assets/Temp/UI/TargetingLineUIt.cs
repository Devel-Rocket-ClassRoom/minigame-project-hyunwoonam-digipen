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
            //TODO: LineImage.enabled = visible;
        }

        /// <summary>
        /// 시작점과 끝점 갱신(스크린 좌표).
        /// </summary>
        public void SetPoints(Vector2 fromScreen, Vector2 toScreen)
        {
            // 동작 요약:
            // - 두 점 사이에 LineImage 위치/회전/길이 적용(2D 변환 행렬).
            //TODO: Vector2 dir = toScreen - fromScreen;
            //TODO: float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //TODO: float length = dir.magnitude;
            //TODO: RectTransform rt = LineImage.GetComponent<RectTransform>();
            //TODO: rt.anchoredPosition = fromScreen + dir * 0.5f;
            //TODO: rt.localEulerAngles = new Vector3(0, 0, angle);
            //TODO: rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length);
        }
    }
}

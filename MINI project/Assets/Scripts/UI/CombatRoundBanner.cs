using System.Collections;
using TMPro;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 라운드 전환 시 화면 중앙에 "Round N" 배너를 잠깐 띄우는 1회용 연출.
    /// 프리팹/Inspector 배선 없이 절차적 스크린 스페이스 오버레이로 생성되며 종료 시 자동 파괴된다.
    /// 같은 행위자가 라운드 끝-시작에 연속 행동(bookend)해도 라운드 경계를 명확히 인지시킨다.
    /// </summary>
    public sealed class CombatRoundBanner : MonoBehaviour
    {
        private const float FadeInSec = 0.15f;
        private const float HoldSec = 0.45f;
        private const float FadeOutSec = 0.2f;

        private TextMeshProUGUI label;
        private Color baseColor;

        /// <summary>"Round N" 배너 1회 표시.</summary>
        public static void Show(int roundNumber)
        {
            GameObject go = new GameObject("CombatRoundBanner");
            CombatRoundBanner banner = go.AddComponent<CombatRoundBanner>();
            banner.Build(roundNumber);
        }

        private void Build(int roundNumber)
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            GameObject textObject = new GameObject("Label");
            textObject.transform.SetParent(transform, false);

            label = textObject.AddComponent<TextMeshProUGUI>();
            label.text = "Round " + roundNumber;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.fontSize = 64f;
            label.raycastTarget = false;
            baseColor = new Color(1f, 0.95f, 0.7f, 1f);
            label.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);

            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 120f);
            rect.sizeDelta = new Vector2(640f, 140f);

            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            yield return Fade(0f, 1f, FadeInSec);
            yield return new WaitForSeconds(HoldSec);
            yield return Fade(1f, 0f, FadeOutSec);
            Destroy(gameObject);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(from, to, duration > 0f ? elapsed / duration : 1f));
                yield return null;
            }

            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            if (label != null)
            {
                label.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            }
        }
    }
}

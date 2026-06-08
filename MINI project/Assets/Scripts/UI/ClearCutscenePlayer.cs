using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Tempt
{
    /// <summary>
    /// 최종 승리 연출. Resources/Clear.mp4 를 풀스크린으로 1회 재생한 뒤 콜백을 호출한다.
    /// 씬/프리팹 의존 없이 런타임에 오버레이 캔버스 + VideoPlayer 를 코드로 구성한다.
    /// 클립이 없거나 재생 오류 시 즉시 콜백으로 폴백한다.
    /// </summary>
    public sealed class ClearCutscenePlayer : MonoBehaviour
    {
        private const string ClipResourceName = "Clear";

        private Action onFinished;
        private bool finished;
        private VideoPlayer videoPlayer;
        private RenderTexture renderTexture;
        private GameObject canvasGo;

        /// <summary>풀스크린 연출을 시작한다. 재생/오류/클립부재 어떤 경우에도 onFinished 가 1회 호출된다.</summary>
        public static void Play(Action onFinished)
        {
            var go = new GameObject("ClearCutscenePlayer");
            DontDestroyOnLoad(go);
            go.AddComponent<ClearCutscenePlayer>().Begin(onFinished);
        }

        private void Begin(Action callback)
        {
            onFinished = callback;

            VideoClip clip = Resources.Load<VideoClip>(ClipResourceName);
            if (clip == null)
            {
                GameLog.LogError(
                    "[ClearCutscenePlayer] Resources/Clear 비디오 클립을 찾을 수 없습니다."
                );
                Finish();
                return;
            }

            BuildOverlay(clip);
        }

        private void BuildOverlay(VideoClip clip)
        {
            int w = Mathf.Max(16, Screen.width);
            int h = Mathf.Max(16, Screen.height);
            renderTexture = new RenderTexture(w, h, 0);

            canvasGo = new GameObject("ClearCutsceneCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            canvasGo.AddComponent<CanvasScaler>();

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bg = bgGo.AddComponent<Image>();
            bg.color = Color.black;
            StretchFull(bg.rectTransform);

            var rawGo = new GameObject("Video");
            rawGo.transform.SetParent(canvasGo.transform, false);
            var raw = rawGo.AddComponent<RawImage>();
            raw.texture = renderTexture;
            StretchFull(raw.rectTransform);

            var audio = gameObject.AddComponent<AudioSource>();

            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = clip;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audio);
            videoPlayer.loopPointReached += OnLoopPointReached;
            videoPlayer.errorReceived += OnErrorReceived;
            videoPlayer.Play();
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void OnLoopPointReached(VideoPlayer vp)
        {
            Finish();
        }

        private void OnErrorReceived(VideoPlayer vp, string message)
        {
            GameLog.LogError("[ClearCutscenePlayer] 비디오 재생 오류: " + message);
            Finish();
        }

        private void Finish()
        {
            if (finished)
            {
                return;
            }

            finished = true;

            Action cb = onFinished;
            onFinished = null;

            Cleanup();
            cb?.Invoke();
            Destroy(gameObject);
        }

        private void Cleanup()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnLoopPointReached;
                videoPlayer.errorReceived -= OnErrorReceived;
                videoPlayer.Stop();
            }

            if (canvasGo != null)
            {
                canvasGo.SetActive(false);
            }

            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }
        }

        private void OnDestroy()
        {
            // 안전망: Finish 를 거치지 않고 파괴돼도 콜백을 1회 보장.
            if (finished)
            {
                return;
            }

            finished = true;
            Cleanup();
            Action cb = onFinished;
            onFinished = null;
            cb?.Invoke();
        }
    }
}

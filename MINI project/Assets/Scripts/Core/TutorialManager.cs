using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 씬/층 최초 진입 시 이미지 튜토리얼 팝업을 1회 표시하는 매니저.
    /// 열람 기록은 CurrentRun.Tutorial(런 상태)에 저장된다 — 새 게임마다 초기화되어 다시 표시되고,
    /// 이어하기에서는 save.json 복원으로 유지된다. 팝업이 열리는 순간 본 것으로 기록한다(중간 닫기 포함).
    /// </summary>
    public sealed class TutorialManager
    {
        /// <summary>튜토리얼 기능 전체 스위치. 임시 비활성화 상태(2026-06-08). 재활성화 시 true.</summary>
        private static readonly bool FeatureEnabled = false;

        /// <summary>
        /// 튜토리얼 이미지 테이블.
        /// - 키: 진입 지점. 값: Resources 경로(확장자 제외) 배열, 키당 1~4장.
        /// - 실제 이미지 적용 시 "temp"만 교체하면 된다. 예: "Tutorials/safe0_01"
        /// </summary>
        private static readonly Dictionary<string, string[]> SequencePages = new Dictionary<
            string,
            string[]
        >
        {
            { "Safe0", new[] { "temp", "temp" } },
            { "FloorMap", new[] { "temp", "temp" } },
            { "Floor1", new[] { "temp" } },
            { "Floor2", new[] { "temp" } },
            { "Floor3", new[] { "temp" } },
            { "Safe1", new[] { "temp", "temp" } },
        };

        private readonly Queue<string> pendingKeys = new Queue<string>();
        private GameSceneManager scenes;
        private bool isShowing;

        /// <summary>씬 전환 이벤트를 구독한다.</summary>
        public void Bind(GameSceneManager sceneManager)
        {
            if (!FeatureEnabled || sceneManager == null)
            {
                return;
            }

            scenes = sceneManager;
            scenes.OnSceneChanged += HandleSceneChanged;
        }

        /// <summary>씬 전환 이벤트 구독을 해제한다. GameSystemManager 파괴 시 호출.</summary>
        public void Unbind()
        {
            if (scenes != null)
            {
                scenes.OnSceneChanged -= HandleSceneChanged;
                scenes = null;
            }

            pendingKeys.Clear();
            isShowing = false;
        }

        private void HandleSceneChanged(SceneId from, SceneId to)
        {
            // 동작 요약:
            // - 이전 씬에서 남은 큐 폐기(팝업 자체는 씬 언로드로 함께 파괴됨).
            // - 런이 없으면(메인 메뉴 등) 표시하지 않는다.
            // - 진입 지점 키 수집 → 미열람 키만 큐에 적재 → 순차 표시 시작.
            pendingKeys.Clear();
            isShowing = false;

            TutorialProgressState seen = ResolveSeenState();
            if (seen == null)
            {
                return;
            }

            foreach (string key in CollectKeysFor(to))
            {
                if (SequencePages.ContainsKey(key) && !seen.IsCompleted(key))
                {
                    pendingKeys.Enqueue(key);
                }
            }

            ShowNext();
        }

        /// <summary>현재 런의 튜토리얼 열람 상태를 얻는다. 런이 없으면 null.</summary>
        private static TutorialProgressState ResolveSeenState()
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return null;
            }

            if (gsm.CurrentRun != null && gsm.CurrentRun.Tutorial == null)
            {
                gsm.CurrentRun.Tutorial = new TutorialProgressState();
            }

            return gsm.CurrentRun?.Tutorial;
        }

        private static IEnumerable<string> CollectKeysFor(SceneId to)
        {
            // 동작 요약:
            // - Safe0/Safe1/FloorMap → 씬 키(최초 진입 1회).
            // - Combat → 진입한 층 기준 "Floor{n}" 키(층별 첫 전투 1회, 테이블에 1~3층만 존재).
            switch (to)
            {
                case SceneId.Safe0:
                    yield return "Safe0";
                    break;
                case SceneId.Safe1:
                    yield return "Safe1";
                    break;
                case SceneId.FloorMap:
                    yield return "FloorMap";
                    break;
                case SceneId.Combat:
                    if (
                        GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                        && gsm.CurrentRun != null
                    )
                    {
                        yield return "Floor" + gsm.CurrentRun.CurrentFloor;
                    }
                    break;
            }
        }

        private void ShowNext()
        {
            // 동작 요약:
            // - 표시 중이거나 큐가 비었으면 종료.
            // - 씬이 진입/실행 상태가 아니면(전환 중 팝업 파괴 등) 종료.
            // - 키를 본 것으로 즉시 기록 후 팝업 표시. 닫히면 다음 키 표시.
            if (isShowing || pendingKeys.Count == 0)
            {
                return;
            }

            if (
                scenes == null
                || (scenes.State != SceneFsmState.Entering && scenes.State != SceneFsmState.Running)
            )
            {
                return;
            }

            TutorialProgressState seen = ResolveSeenState();
            if (seen == null)
            {
                pendingKeys.Clear();
                return;
            }

            string key = pendingKeys.Dequeue();
            seen.MarkCompleted(key);

            isShowing = true;
            TutorialPopup.Show(
                SequencePages[key],
                () =>
                {
                    isShowing = false;
                    ShowNext();
                }
            );
        }
    }
}

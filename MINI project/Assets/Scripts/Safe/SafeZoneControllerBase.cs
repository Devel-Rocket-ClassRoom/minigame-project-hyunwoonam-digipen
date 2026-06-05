namespace Tempt
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 안전지대 씬 컨트롤러 공통 베이스.
    /// 단일 씬 재사용이 아닌 6개 분리(Safe0~5)이므로 베이스에는 공통 기능만 둔다.
    /// </summary>
    public abstract class SafeZoneControllerBase : SceneControllerBase
    {
        /// <summary>이 안전지대 인덱스(0~5).</summary>
        public int SafeIndex;

        /// <summary>이 안전지대 정의(WorldData.SafeZones[SafeIndex]).</summary>
        public SafeZoneDef Definition;

        [SerializeField]
        private TextMeshProUGUI dayLabel;

        [SerializeField]
        private Button enterFloorMapButton;

        /// <inheritdoc/>
        public override void OnEnter()
        {
            // 동작 요약:
            // - GameSystemManager.Instance.CurrentRun.Player 참조.
            // - 안전지대 잠금 검사(SafeUnlocks). 잠겨 있으면 진입 거부 + 메인메뉴/메시지.
            // - UIManager.SetConsumablesEditable(true) 활성.
            // - 마석 일일 지급(SafeIndex가 3~5 광산이면).
            // - 단계별 가격 인플레이션 적용.
            // - SaveLoader.SaveSnapshot 자동 저장.
            // - 파생 컨트롤러의 SetupZoneFeatures() 호출.
            GameSystemManager gsm = GameSystemManager.Instance;
            GameRunState run = gsm.CurrentRun;
            if (run == null)
            {
                gsm.Scenes.LoadMainMenu();
                return;
            }

            if (run.SafeUnlocks != null && !run.SafeUnlocks.IsUnlocked(SafeIndex))
            {
                gsm.Scenes.LoadFloorMap();
                return;
            }

            Definition =
                gsm.Data?.World?.SafeZones != null
                && SafeIndex >= 0
                && SafeIndex < gsm.Data.World.SafeZones.Count
                    ? gsm.Data.World.SafeZones[SafeIndex]
                    : null;
            SubscribeDayChanged(gsm.Events);
            UpdateDayText(run.CurrentDay);
            WireEnterFloorMapButton();
            gsm.Save?.SaveSnapshot();
            SetupZoneFeatures();
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            // 동작 요약: 정리.
            UnwireEnterFloorMapButton();
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                UnsubscribeDayChanged(gsm.Events);
            }
        }

        /// <summary>
        /// 파생 컨트롤러가 이 안전지대 고유 기능을 셋업.
        /// </summary>
        protected abstract void SetupZoneFeatures();

        /// <summary>
        /// 플로어 맵으로 출발.
        /// </summary>
        public virtual void DepartToFloorMap()
        {
            // 동작 요약:
            // - 플레이어가 출발 가능한 단계인지 확인.
            // - GameSystemManager.Instance.Scenes.LoadFloorMap().
            GameSystemManager.Instance.LoadFloorMapFromSafe(SafeIndex);
        }

        private void SubscribeDayChanged(EventBus events)
        {
            if (events == null)
            {
                return;
            }

            events.OnDayChanged -= UpdateDayText;
            events.OnDayChanged += UpdateDayText;
        }

        private void UnsubscribeDayChanged(EventBus events)
        {
            if (events == null)
            {
                return;
            }

            events.OnDayChanged -= UpdateDayText;
        }

        private void UpdateDayText(int day)
        {
            if (dayLabel == null)
            {
                return;
            }

            dayLabel.text = Loc.Format("safe_day_fmt", day);
        }

        private void WireEnterFloorMapButton()
        {
            if (enterFloorMapButton == null)
            {
                return;
            }

            enterFloorMapButton.onClick.RemoveListener(DepartToFloorMap);
            enterFloorMapButton.onClick.AddListener(DepartToFloorMap);
        }

        private void UnwireEnterFloorMapButton()
        {
            if (enterFloorMapButton == null)
            {
                return;
            }

            enterFloorMapButton.onClick.RemoveListener(DepartToFloorMap);
        }
    }
}

namespace Tempt
{
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
            //TODO: var gsm = GameSystemManager.Instance;
            //TODO: var run = gsm.CurrentRun;
            //TODO: if (run.SafeUnlocks.TryGetValue(SafeIndex, out bool unlocked) && !unlocked)
            //TODO: {
            //TODO:     ToastUI.Show("침식으로 안전지대 잠김");
            //TODO:     gsm.Scenes.LoadFloorMap();
            //TODO:     return;
            //TODO: }
            //TODO: gsm.UI.SetConsumablesEditable(true);
            //TODO: gsm.Save.SaveSnapshot(run);
            GameSystemManager gsm = GameSystemManager.Instance; //Wave0write
            GameRunState run = gsm.CurrentRun; //Wave0write
            if (run == null) //Wave0write
            { //Wave0write
                gsm.Scenes.LoadMainMenu(); //Wave0write
                return; //Wave0write
            } //Wave0write

            if (run.SafeUnlocks != null && !run.SafeUnlocks.IsUnlocked(SafeIndex)) //Wave0write
            { //Wave0write
                gsm.Scenes.LoadFloorMap(); //Wave0write
                return; //Wave0write
            } //Wave0write

            Definition = gsm.Data?.World?.SafeZones != null && SafeIndex >= 0 && SafeIndex < gsm.Data.World.SafeZones.Count ? gsm.Data.World.SafeZones[SafeIndex] : null; //Wave0write
            gsm.Save?.SaveSnapshot(); //Wave0write
            SetupZoneFeatures();
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            // 동작 요약: 정리.
        }

        /// <summary>
        /// 파생 컨트롤러가 이 안전지대 고유 기능을 셋업.
        /// </summary>
        protected abstract void SetupZoneFeatures();

        /// <summary>
        /// 플로어 맵으로 출발.
        /// </summary>
        public void DepartToFloorMap()
        {
            // 동작 요약:
            // - 플레이어가 출발 가능한 단계인지 확인.
            // - GameSystemManager.Instance.Scenes.LoadFloorMap().
            //TODO: GameSystemManager.Instance.Scenes.LoadFloorMap();
            GameSystemManager.Instance.LoadFloorMapFromSafe(SafeIndex); //Wave0write
        }
    }
}


namespace Tempt
{
    // Wave0refactor 2026-05-27: BUG-3 보강.
    // 1~3층 보스(층 3) 클리어 후 GameSystemManager.FinishVictoryAfterReward 는
    // SafeUnlocks.Unlock(safeIndex) 직후 Scenes.LoadSafeZone(safeIndex) 를 호출한다.
    // safeIndex = min(5, node.StageIndex) = 1 이므로 Safe1 씬으로 진입한다.
    // Safe1Controller 가 활성 Scripts 에 없으면 GameSceneManager.LoadSceneRoutine 의
    // FindAnyObjectByType<SceneControllerBase> 가 Safe1 씬 내의 컨트롤러를 잡지 못하거나
    // 이전 씬 컨트롤러를 잘못 잡아 OnEnter 가 호출되지 않는다. 이를 막기 위한 최소 stub.
    //
    // 시설(Inn / Shop / Guild / Temple / Forge) 본 기능은 Wave12 에서 채운다.
    // 이 stub 은 SafeZoneControllerBase.OnEnter 의 잠금 검사 / 자동 저장만 신뢰한다.
    /// <summary>
    /// 안전지대 1: 5개 시설(주점/상점/길드/대장간/신전) + 튜토리얼 시작 위치.
    /// Wave0 범위에서는 SafeIndex 등록과 진입/이탈 라이프사이클만 보장한다.
    /// 시설 기능 구현은 Wave12 에서 진행한다.
    /// </summary>
    public sealed class Safe1Controller : SafeZoneControllerBase
    {
        /// <summary>
        /// 생성 직후 SafeIndex 를 1 로 고정한다.
        /// Inspector 에서 잘못 변경되더라도 OnEnter 이전에 한 번 더 보정한다.
        /// </summary>
        private void Reset()
        {
            // 동작 요약: Unity Inspector 에서 컴포넌트가 처음 추가되거나 Reset 메뉴를 사용했을 때
            // SafeIndex 를 안전한 기본값으로 강제.
            SafeIndex = 1;
        }

        /// <inheritdoc/>
        public override void OnEnter()
        {
            // Wave0refactor 2026-05-27: 런타임 AddComponent 경로에서도 잠금 검사 전에 SafeIndex를 보정한다.
            SafeIndex = 1;
            base.OnEnter();
        }

        /// <summary>
        /// 씬에 따라 다르게 셋업할 시설/튜토리얼 트리거를 준비한다.
        /// 현재는 stub. SafeIndex 강제와 진입 로그만 남긴다.
        /// </summary>
        protected override void SetupZoneFeatures()
        {
            // 동작 요약:
            // - SafeIndex 가 1 이 아닐 경우 강제 보정(베이스의 잠금 검사 신뢰).
            // - 추후 Wave12 에서 Inn / Shop / Guild / Temple / Forge 인스펙터 참조를 받아
            //   각 시설 UI 의 OnEnter 핸들러를 등록한다.
            // - 튜토리얼 트리거(TutorialProgressState.IsCompleted 검사 후 시작)는 Wave11/12 에서.
            SafeIndex = 1;
        }
    }
}

namespace Tempt
{
    /// <summary>
    /// 플로어 맵 씬 컨트롤러. 전체 노드 스크롤 렌더링, 선택 가능 노드만 활성화,
    /// 단계별 침식률 표시, 재도전 모드 진입 가능 여부 처리.
    /// </summary>
    public sealed class FloorMapControllert : SceneControllerBaset
    {
        /// <summary>이 씬에서 사용하는 맵 모델 참조.</summary>
        public FloorMapModelt Map;

        /// <summary>현재 재도전 모드(상위 안전지대 도달 후 아래층 재방문).</summary>
        public bool IsRechallengeMode;

        /// <inheritdoc/>
        public override void OnEnter()
        {
            // 동작 요약:
            // - GameSystemManagert.Instance.CurrentRun.FloorMap 참조.
            // - 모든 노드 UI 그리기(FloorNodeUIt 인스턴스화).
            // - 클리어된 노드는 색상 변경.
            // - 활성 가능 노드만 클릭 가능(현재 층 다음 + IsRechallenge면 아래층 전체).
            // - 침식 상태에 따라 안전지대 잠금/노드 비활성 표시.
            // - 단계 침식률 HUD 갱신.
            //TODO: Map = GameSystemManagert.Instance.CurrentRun.FloorMap;
            //TODO: foreach (var kvp in Map.NodesByFloor)
            //TODO: {
            //TODO:     foreach (var node in kvp.Value)
            //TODO:     {
            //TODO:         var ui = Instantiate(FloorNodeUIPrefab, NodeContainer);
            //TODO:         ui.Bind(node, OnNodeClicked);
            //TODO:         ui.SetCleared(node.IsCleared);
            //TODO:         bool selectable = node.Floor == Map.NextSelectableFloor
            //TODO:             || (IsRechallengeMode && node.Floor < Map.NextSelectableFloor);
            //TODO:         ui.SetInteractable(selectable && !node.IsCleared);
            //TODO:     }
            //TODO: }
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: int stageIdx = run.CurrentFloor / 8;
            //TODO: float erosionRate = run.Erosion.GetRate(stageIdx);
            //TODO: ErosionHUD.SetRate(stageIdx, erosionRate);
            GameSystemManagert gsm = GameSystemManagert.Instance; //Wave0write
            Map = gsm.CurrentRun?.FloorMap; //Wave0write
            IsRechallengeMode = false; //Wave0write
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            // 동작 요약: UI 정리.
            //TODO: foreach (Transform child in NodeContainer) Destroy(child.gameObject);
            Map = null; //Wave0write
        }

        /// <summary>
        /// 노드 선택 콜백(FloorNodeUIt → 컨트롤러).
        /// </summary>
        public void OnNodeClicked(int nodeId)
        {
            // 동작 요약:
            // - 노드 활성 가능 검증.
            // - GameSystemManagert.Instance.StartCombatNode(node, IsRechallengeMode).
            //TODO: if (!Map.NodesById.TryGetValue(nodeId, out var node)) return;
            //TODO: bool selectable = node.Floor == Map.NextSelectableFloor
            //TODO:     || (IsRechallengeMode && node.Floor < Map.NextSelectableFloor);
            //TODO: if (!selectable || node.IsCleared) return;
            //TODO: GameSystemManagert.Instance.StartCombatNode(node, IsRechallengeMode);
            if (Map == null || !Map.NodesById.TryGetValue(nodeId, out FloorNodet node)) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            bool selectable = node.Floor == Map.NextSelectableFloor || (IsRechallengeMode && node.Floor < Map.NextSelectableFloor); //Wave0write
            if (!selectable || (!IsRechallengeMode && node.IsCleared)) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            GameSystemManagert.Instance.StartCombatNode(node, IsRechallengeMode); //Wave0write
        }

        /// <summary>
        /// 후퇴(안전지대로 복귀) 가능 여부.
        /// 단계 보스 클리어 전에는 잠김(설계변경).
        /// </summary>
        public bool CanRetreatToSafe()
        {
            // 동작 요약:
            // - 현재 진행 중인 단계의 보스 클리어 여부 검사.
            // - 보스 미클리어이면 false.
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: int stageIdx = run.CurrentFloor / 8;
            //TODO: return Map.IsStageCleared(stageIdx);
            GameRunStatet run = GameSystemManagert.Instance.CurrentRun; //Wave0write
            if (run == null || Map == null) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            int stageIndex = StageIndexFromFloor(run.CurrentFloor); //Wave0write
            return Map.IsStageCleared(stageIndex); //Wave0write
        }

        /// <summary>
        /// 안전지대로 복귀 시도. 가능하면 GSM.Scenes.LoadSafeZone 호출.
        /// </summary>
        public void RequestReturnToSafe()
        {
            // 동작 요약:
            // - CanRetreatToSafe() 검사.
            // - 가능하면 LoadSafeZone(현재 단계가 해금한 안전지대).
            //TODO: if (!CanRetreatToSafe()) return;
            //TODO: var run = GameSystemManagert.Instance.CurrentRun;
            //TODO: int safeZoneIndex = run.CurrentFloor / 8; // 단계 인덱스 = 안전지대 인덱스
            //TODO: GameSystemManagert.Instance.Scenes.LoadSafeZone(safeZoneIndex);
            if (!CanRetreatToSafe()) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            GameRunStatet run = GameSystemManagert.Instance.CurrentRun; //Wave0write
            int safeZoneIndex = System.Math.Max(0, StageIndexFromFloor(run.CurrentFloor) - 1); //Wave0write
            GameSystemManagert.Instance.Scenes.LoadSafeZone(safeZoneIndex); //Wave0write
        }

        private static int StageIndexFromFloor(int floor) //Wave0write
        { //Wave0write
            if (floor <= 3) return 1; //Wave0write
            if (floor <= 11) return 2; //Wave0write
            if (floor <= 19) return 3; //Wave0write
            if (floor <= 29) return 4; //Wave0write
            if (floor <= 39) return 5; //Wave0write
            return 6; //Wave0write
        } //Wave0write
    }
}

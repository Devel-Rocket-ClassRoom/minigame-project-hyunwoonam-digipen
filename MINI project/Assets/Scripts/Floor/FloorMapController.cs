using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// 플로어 맵 씬 컨트롤러. 전체 노드 스크롤 렌더링, 선택 가능 노드만 활성화,
    /// 단계별 침식률 표시, 재도전 모드 진입 가능 여부 처리.
    /// </summary>
    public sealed class FloorMapController : SceneControllerBase
    {
        /// <summary>이 씬에서 사용하는 맵 모델 참조.</summary>
        public FloorMapModel Map;

        /// <summary>현재 재도전 모드(상위 안전지대 도달 후 아래층 재방문).</summary>
        public bool IsRechallengeMode;

        /// <summary>재도전 모드에서 방문 가능한 마지막 층.</summary>
        public int RechallengeMaxFloor;

        /// <summary>재도전 플로어맵을 연 안전지대 인덱스.</summary>
        public int RechallengeReturnSafeIndex;

        /// <summary>이번 플로어맵 진입에서 해금된 안전지대 노드 이동을 허용할지 여부.</summary>
        public bool CanEnterSafeZonesFromMap;

        /// <summary>플로어맵을 연 안전지대 인덱스.</summary>
        public int FloorMapSourceSafeIndex;

        [SerializeField] private RectTransform nodeContainer;
        [SerializeField] private RectTransform connectionContainer;
        [SerializeField] private FloorNodeUI nodeTemplate;
        [SerializeField] private TextMeshProUGUI headerLabel;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private float floorSpacingY = 180f;
        [SerializeField] private float nodeSpacingX = 170f;
        [SerializeField] private float edgePadding = 220f;
        [SerializeField] private Vector2 nodeHitSize = new Vector2(172f, 96f);
        [SerializeField] private float connectionThickness = 4f;

        private readonly List<FloorNodeUI> spawnedNodes = new List<FloorNodeUI>();
        private readonly List<GameObject> spawnedConnections = new List<GameObject>();
        private readonly Dictionary<int, Vector2> nodePositions = new Dictionary<int, Vector2>();
        private EventBus subscribedEvents;

        /// <inheritdoc/>
        public override void OnEnter()
        {
            GameSystemManager gsm = GameSystemManager.Instance;
            Map = gsm.CurrentRun?.FloorMap;
            IsRechallengeMode = gsm.TryConsumeFloorMapRechallenge(out int maxFloor, out int returnSafeIndex);
            RechallengeMaxFloor = maxFloor;
            RechallengeReturnSafeIndex = returnSafeIndex;
            CanEnterSafeZonesFromMap = gsm.TryConsumeFloorMapSafeTravel(out int sourceSafeIndex);
            FloorMapSourceSafeIndex = sourceSafeIndex;
            if (Map == null)
            {
                Debug.LogError("[FloorMapController] CurrentRun.FloorMap 이 없습니다.");
                return;
            }

            FloorMapCreator.EnsureSafeZoneNodes(Map, gsm.Data?.World);
            if (!ValidateUiRefs())
            {
                return;
            }

            RenderMap();
            SubscribeErosionEvents(gsm.Events);
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            ClearRenderedMap();
            UnsubscribeErosionEvents();
            Map = null;
            CanEnterSafeZonesFromMap = false;
            FloorMapSourceSafeIndex = 0;
        }

        /// <summary>
        /// 노드 선택 콜백(FloorNodeUI → 컨트롤러).
        /// </summary>
        public void OnNodeClicked(int nodeId)
        {
            if (Map == null || !Map.NodesById.TryGetValue(nodeId, out FloorNode node))
            {
                return;
            }

            if (node.IsSafeZone)
            {
                if (IsSafeZoneSelectable(node))
                {
                    GameSystemManager.Instance.EnterSafeZoneFromFloorMap(node.StageIndex);
                }
                return;
            }

            bool isRechallengeNode = IsRechallengeNode(node);
            bool isProgressionNode = IsProgressionNodeSelectable(node);
            bool selectable = isRechallengeNode || isProgressionNode;
            if (!selectable || (!IsRechallengeMode && node.IsCleared))
            {
                return;
            }

            GameSystemManager.Instance.StartCombatNode(node, isRechallengeNode);
        }

        /// <summary>
        /// 후퇴(안전지대로 복귀) 가능 여부.
        /// 단계 보스 클리어 전에는 잠김(설계변경).
        /// </summary>
        public bool CanRetreatToSafe()
        {
            GameRunState run = GameSystemManager.Instance.CurrentRun;
            if (run == null || Map == null)
            {
                return false;
            }

            int stageIndex = StageIndexResolver.FromFloor(run.CurrentFloor, GameSystemManager.Instance.Data?.World);
            return Map.IsStageCleared(stageIndex);
        }

        /// <summary>
        /// 안전지대로 복귀 시도. 가능하면 GSM.Scenes.LoadSafeZone 호출.
        /// </summary>
        public void RequestReturnToSafe()
        {
            if (!CanRetreatToSafe())
            {
                return;
            }

            GameRunState run = GameSystemManager.Instance.CurrentRun;
            int safeZoneIndex = System.Math.Max(0, StageIndexResolver.FromFloor(run.CurrentFloor, GameSystemManager.Instance.Data?.World) - 1);
            GameSystemManager.Instance.Scenes.LoadSafeZone(safeZoneIndex);
        }

        private bool ValidateUiRefs()
        {
            if (nodeContainer != null && connectionContainer != null && nodeTemplate != null && scrollRect != null && scrollRect.content != null)
            {
                return true;
            }

            Debug.LogError("[FloorMapController] nodeContainer / connectionContainer / nodeTemplate / scrollRect 참조가 씬에 직접 할당되어 있지 않습니다.");
            return false;
        }

        private void RenderMap()
        {
            ClearRenderedMap();
            nodePositions.Clear();

            List<int> floors = new List<int>(Map.NodesByFloor.Keys);
            floors.Sort();
            float widestRowWidth = CalculateWidestRowWidth(floors);
            float viewportWidth = scrollRect.viewport != null ? scrollRect.viewport.rect.width : 0f;
            float requiredWidth = Mathf.Max(viewportWidth, widestRowWidth + edgePadding * 2f);
            float requiredHeight = edgePadding * 2f + nodeHitSize.y + Mathf.Max(0, floors.Count - 1) * floorSpacingY;
            ResizeContent(requiredWidth, requiredHeight);

            for (int floorIndex = 0; floorIndex < floors.Count; floorIndex++)
            {
                int floor = floors[floorIndex];
                if (!Map.NodesByFloor.TryGetValue(floor, out List<FloorNode> nodes) || nodes == null)
                {
                    continue;
                }

                float rowY = -edgePadding - nodeHitSize.y * 0.5f - floorIndex * floorSpacingY;
                float rowStartX = -((nodes.Count - 1) * nodeSpacingX) * 0.5f;
                for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
                {
                    FloorNode node = nodes[nodeIndex];
                    if (node == null)
                    {
                        continue;
                    }

                    Vector2 anchoredPosition = new Vector2(rowStartX + nodeIndex * nodeSpacingX, rowY);
                    nodePositions[node.NodeId] = anchoredPosition;
                    FloorNodeUI ui = Instantiate(nodeTemplate, nodeContainer);
                    ui.gameObject.SetActive(true);
                    ui.SetHitSize(nodeHitSize);
                    ui.RectTransform.anchoredPosition = anchoredPosition;
                    ui.Bind(node, OnNodeClicked);
                    ui.SetInteractable(IsNodeSelectable(node));
                    ApplyErosionStateToNode(ui, node);
                    spawnedNodes.Add(ui);
                }
            }

            foreach (FloorNode node in Map.NodesById.Values)
            {
                if (node?.NextNodeIds == null || !nodePositions.TryGetValue(node.NodeId, out Vector2 from))
                {
                    continue;
                }

                foreach (int nextId in node.NextNodeIds)
                {
                    if (nodePositions.TryGetValue(nextId, out Vector2 to))
                    {
                        SpawnConnection(from, to);
                    }
                }
            }

            UpdateHeaderText();
        }

        private float CalculateWidestRowWidth(List<int> floors)
        {
            int maxNodes = 1;
            foreach (int floor in floors)
            {
                if (Map.NodesByFloor.TryGetValue(floor, out List<FloorNode> nodes) && nodes != null)
                {
                    maxNodes = Mathf.Max(maxNodes, nodes.Count);
                }
            }

            return nodeHitSize.x + Mathf.Max(0, maxNodes - 1) * nodeSpacingX;
        }

        private void ResizeContent(float requiredWidth, float requiredHeight)
        {
            RectTransform content = scrollRect.content;
            content.anchorMin = new Vector2(0.5f, 1f);
            content.anchorMax = new Vector2(0.5f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(requiredWidth, requiredHeight);
            content.anchoredPosition = Vector2.zero;

            StretchLayer(nodeContainer);
            StretchLayer(connectionContainer);
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private static void StretchLayer(RectTransform layer)
        {
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.pivot = new Vector2(0.5f, 0.5f);
            layer.offsetMin = Vector2.zero;
            layer.offsetMax = Vector2.zero;
        }

        private bool IsNodeSelectable(FloorNode node)
        {
            if (node == null)
            {
                return false;
            }

            if (node.IsSafeZone)
            {
                return IsSafeZoneSelectable(node);
            }

            return IsRechallengeNode(node) || IsProgressionNodeSelectable(node);
        }

        private bool IsProgressionNodeSelectable(FloorNode node)
        {
            if (node == null || node.IsSafeZone || node.IsCleared || node.Floor != Map.NextSelectableFloor)
            {
                return false;
            }

            return true;
        }

        private bool IsSafeZoneSelectable(FloorNode node)
        {
            if (node == null || !node.IsSafeZone)
            {
                return false;
            }

            GameRunState run = GameSystemManager.Instance.CurrentRun;
            return CanEnterSafeZonesFromMap && run?.SafeUnlocks != null && run.SafeUnlocks.IsUnlocked(node.StageIndex);
        }

        private bool IsRechallengeNode(FloorNode node)
        {
            return IsRechallengeMode
                && node != null
                && !node.IsSafeZone
                && !IsStageFullyEroded(node.StageIndex)
                && node.Floor > 0
                && node.Floor <= RechallengeMaxFloor;
        }

        private void SpawnConnection(Vector2 from, Vector2 to)
        {
            var line = new GameObject("ConnectionLine", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(connectionContainer, false);
            RectTransform rect = line.GetComponent<RectTransform>();
            Image image = line.GetComponent<Image>();
            Vector2 delta = to - from;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = from + delta * 0.5f;
            rect.sizeDelta = new Vector2(delta.magnitude, connectionThickness);
            rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            image.color = new Color(0.55f, 0.55f, 0.60f, 0.45f);
            line.transform.SetAsFirstSibling();
            spawnedConnections.Add(line);
        }

        private void ClearRenderedMap()
        {
            foreach (FloorNodeUI ui in spawnedNodes)
            {
                if (ui != null)
                {
                    Destroy(ui.gameObject);
                }
            }
            spawnedNodes.Clear();

            foreach (GameObject line in spawnedConnections)
            {
                if (line != null)
                {
                    Destroy(line);
                }
            }
            spawnedConnections.Clear();
            nodePositions.Clear();
        }

        private void SubscribeErosionEvents(EventBus events)
        {
            UnsubscribeErosionEvents();
            subscribedEvents = events;
            if (subscribedEvents == null)
            {
                return;
            }

            subscribedEvents.OnStageErosionChanged += HandleStageErosionChanged;
            subscribedEvents.OnSafeZoneLockChanged += HandleSafeLockChanged;
        }

        private void UnsubscribeErosionEvents()
        {
            if (subscribedEvents == null)
            {
                return;
            }

            subscribedEvents.OnStageErosionChanged -= HandleStageErosionChanged;
            subscribedEvents.OnSafeZoneLockChanged -= HandleSafeLockChanged;
            subscribedEvents = null;
        }

        private void HandleStageErosionChanged(int stage, float rate)
        {
            for (int i = 0; i < spawnedNodes.Count; i++)
            {
                FloorNodeUI ui = spawnedNodes[i];
                if (ui != null && ui.StageIndex == stage)
                {
                    ui.SetErosionRate(rate);
                    ui.SetFullyEroded(rate >= 100f);
                }
            }

            UpdateHeaderText();
        }

        private void HandleSafeLockChanged(int safeIndex, bool locked)
        {
            for (int i = 0; i < spawnedNodes.Count; i++)
            {
                FloorNodeUI ui = spawnedNodes[i];
                if (ui != null && ui.IsSafeZone && ui.StageIndex == safeIndex)
                {
                    ui.SetSafeLocked(locked);
                }
            }
        }

        private static void ApplyErosionStateToNode(FloorNodeUI ui, FloorNode node)
        {
            if (ui == null || node == null || !GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return;
            }

            float rate = gsm.CurrentRun?.Erosion != null ? gsm.CurrentRun.Erosion.GetRate(node.StageIndex) : 0f;
            ui.SetErosionRate(rate);
            ui.SetFullyEroded(gsm.Erosion?.IsStageFullyEroded(node.StageIndex) == true);
            if (node.IsSafeZone && gsm.CurrentRun?.SafeUnlocks != null)
            {
                ui.SetSafeLocked(!gsm.CurrentRun.SafeUnlocks.IsUnlocked(node.StageIndex));
            }
        }

        private static bool IsStageFullyEroded(int stageIndex)
        {
            return GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                && gsm.Erosion?.IsStageFullyEroded(stageIndex) == true;
        }

        private void UpdateHeaderText()
        {
            if (headerLabel == null)
            {
                return;
            }

            string modeText = IsRechallengeMode ? "FLOOR MAP - RECHALLENGE" : "FLOOR MAP";
            headerLabel.text = modeText + "\n" + BuildStageErosionSummary();
        }

        private static string BuildStageErosionSummary()
        {
            GameRunState run = GameSystemManager.TryGetInstance(out GameSystemManager gsm) ? gsm.CurrentRun : null;
            ErosionStateModel erosion = run?.Erosion;
            if (erosion == null)
            {
                return BuildZeroStageErosionSummary(gsm?.Data?.World);
            }

            int stageCount = ErosionSystem.GetMaxStage(gsm?.Data?.World);
            var parts = new List<string>();
            for (int stage = 1; stage <= stageCount; stage++)
            {
                parts.Add(string.Format("S{0} {1:0.#}%", stage, erosion.GetRate(stage)));
            }

            return "Stage Erosion: " + string.Join(" / ", parts);
        }

        private static string BuildZeroStageErosionSummary(WorldData world)
        {
            int stageCount = ErosionSystem.GetMaxStage(world);
            var parts = new List<string>();
            for (int stage = 1; stage <= stageCount; stage++)
            {
                parts.Add("S" + stage + " 0%");
            }

            return "Stage Erosion: " + string.Join(" / ", parts);
        }
    }
}

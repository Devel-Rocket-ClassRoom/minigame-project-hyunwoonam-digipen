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

        /// <inheritdoc/>
        public override void OnEnter()
        {
            GameSystemManager gsm = GameSystemManager.Instance; //Wave0write
            Map = gsm.CurrentRun?.FloorMap; //Wave0write
            IsRechallengeMode = false; //Wave0write
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
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            ClearRenderedMap();
            Map = null; //Wave0write
        }

        /// <summary>
        /// 노드 선택 콜백(FloorNodeUI → 컨트롤러).
        /// </summary>
        public void OnNodeClicked(int nodeId)
        {
            if (Map == null || !Map.NodesById.TryGetValue(nodeId, out FloorNode node)) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (node.IsSafeZone) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            bool selectable = node.Floor == Map.NextSelectableFloor || (IsRechallengeMode && node.Floor < Map.NextSelectableFloor); //Wave0write
            if (!selectable || (!IsRechallengeMode && node.IsCleared)) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            GameSystemManager.Instance.StartCombatNode(node, IsRechallengeMode); //Wave0write
        }

        /// <summary>
        /// 후퇴(안전지대로 복귀) 가능 여부.
        /// 단계 보스 클리어 전에는 잠김(설계변경).
        /// </summary>
        public bool CanRetreatToSafe()
        {
            GameRunState run = GameSystemManager.Instance.CurrentRun; //Wave0write
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
            if (!CanRetreatToSafe()) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            GameRunState run = GameSystemManager.Instance.CurrentRun; //Wave0write
            int safeZoneIndex = System.Math.Max(0, StageIndexFromFloor(run.CurrentFloor) - 1); //Wave0write
            GameSystemManager.Instance.Scenes.LoadSafeZone(safeZoneIndex); //Wave0write
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

            if (headerLabel != null)
            {
                headerLabel.text = "FLOOR MAP";
            }
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
                return false;
            }

            bool selectable = node.Floor == Map.NextSelectableFloor || (IsRechallengeMode && node.Floor < Map.NextSelectableFloor);
            return selectable && (IsRechallengeMode || !node.IsCleared);
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
    }
}

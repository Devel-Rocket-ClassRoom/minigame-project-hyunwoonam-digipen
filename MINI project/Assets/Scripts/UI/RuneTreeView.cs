using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    public sealed class RuneTreeView : MonoBehaviour
    {
        public enum Mode
        {
            ViewUnlock,
            Shrine,
        }

        [Header("References")]
        [SerializeField]
        private RuneNodeButton nodeButtonPrefab;

        [SerializeField]
        private Image connectorPrefab;

        [SerializeField]
        private RectTransform treeContainer;

        [SerializeField]
        private RectTransform connectorContainer;

        [SerializeField]
        private RuneNodeDetailPanel detailPanel;

        [SerializeField]
        private TMP_Text runePointLabel;

        [Header("Layout")]
        [SerializeField]
        private Vector2 cellSize = new Vector2(76f, 64f);

        [SerializeField]
        private float connectorThickness = 2f;

        [Header("Connector Colors")]
        [SerializeField]
        private Color lockedConnectorColor = new Color(0.22f, 0.22f, 0.25f, 1f);

        [SerializeField]
        private Color unlockableConnectorColor = new Color(0.58f, 0.50f, 0.28f, 1f);

        [SerializeField]
        private Color unlockedConnectorColor = new Color(0.72f, 0.72f, 0.72f, 1f);

        private readonly Dictionary<int, RuneNodeButton> nodeButtons =
            new Dictionary<int, RuneNodeButton>();
        private readonly List<Image> connectors = new List<Image>();
        private PlayerRuneState state;
        private EventBus subscribedEvents;
        private Mode currentMode;
        private int selectedNodeId;
        private bool viewOnly;

        public bool IsViewOnly => viewOnly;

        public Mode CurrentMode => currentMode;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void Bind(PlayerRuneState runeState, Mode mode)
        {
            Bind(runeState, mode, false);
        }

        public void Bind(PlayerRuneState runeState, Mode mode, bool isViewOnly)
        {
            UnsubscribeEvents();
            state = runeState;
            currentMode = mode;
            viewOnly = isViewOnly;
            selectedNodeId = 0;
            SubscribeEvents();
            Rebuild();
        }

        public void SetViewOnly(bool isViewOnly)
        {
            viewOnly = isViewOnly;
            RefreshNodeStates();
            ShowSelectedNode();
        }

        public void Rebuild()
        {
            ClearInstantiatedViews();
            state?.SyncTreeStateFromProgress();
            UpdateRunePointLabel();

            if (!ValidateReferences() || state?.Tree?.AllNodes == null)
            {
                detailPanel?.Hide();
                return;
            }

            EnsureSelectedNode();
            List<RuneNode> nodes = SortedNodes();
            Dictionary<int, Vector2> positions = BuildPositions(nodes);

            for (int i = 0; i < nodes.Count; i++)
            {
                RuneNode node = nodes[i];
                RuneNodeButton nodeButton = Instantiate(nodeButtonPrefab, treeContainer, false);
                nodeButton.Initialize(this, node.Data);
                if (
                    nodeButton.TryGetComponent(out RectTransform rect)
                    && positions.TryGetValue(node.Data.Id, out Vector2 position)
                )
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = position;
                }

                nodeButtons[node.Data.Id] = nodeButton;
            }

            DrawConnectors(nodes, positions);
            RefreshNodeStates();
            ShowSelectedNode();
        }

        public void OnNodeClicked(int nodeId)
        {
            selectedNodeId = nodeId;
            RefreshNodeStates();
            ShowSelectedNode();
        }

        public void OnUnlockClicked()
        {
            if (viewOnly || state?.Tree?.AllNodes == null || selectedNodeId == 0)
            {
                return;
            }

            if (state.TryUnlock(selectedNodeId))
            {
                ApplyRuneEffectsToCurrentPlayer();
                Rebuild();
            }
        }

        private void DrawConnectors(List<RuneNode> nodes, Dictionary<int, Vector2> positions)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                RuneNode child = nodes[i];
                int parentId = child.Data.RequiredRuneId;
                if (
                    parentId == 0
                    || state?.Tree?.AllNodes == null
                    || !state.Tree.AllNodes.TryGetValue(parentId, out RuneNode parent)
                    || !positions.TryGetValue(parentId, out Vector2 from)
                    || !positions.TryGetValue(child.Data.Id, out Vector2 to)
                )
                {
                    continue;
                }

                Image connector = Instantiate(connectorPrefab, connectorContainer, false);
                RectTransform rect = connector.rectTransform;
                Vector2 delta = to - from;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = from + delta * 0.5f;
                rect.sizeDelta = new Vector2(delta.magnitude, connectorThickness);
                rect.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg
                );
                connector.color = ConnectorColor(parent, child);
                connectors.Add(connector);
            }
        }

        private void RefreshNodeStates()
        {
            if (state?.Tree?.AllNodes == null)
            {
                return;
            }

            state.SyncTreeStateFromProgress();
            foreach (KeyValuePair<int, RuneNodeButton> pair in nodeButtons)
            {
                if (!state.Tree.AllNodes.TryGetValue(pair.Key, out RuneNode node))
                {
                    continue;
                }

                pair.Value.SetState(
                    NodeVisualState(node),
                    state.GetInvestedPoints(pair.Key),
                    state.GetRequiredPoints(pair.Key),
                    selectedNodeId == pair.Key,
                    viewOnly
                );
            }

            UpdateRunePointLabel();
        }

        private void ShowSelectedNode()
        {
            if (detailPanel == null || state?.Tree?.AllNodes == null || selectedNodeId == 0)
            {
                detailPanel?.Hide();
                return;
            }

            if (!state.Tree.AllNodes.TryGetValue(selectedNodeId, out RuneNode node))
            {
                detailPanel.Hide();
                return;
            }

            detailPanel.Show(
                node.Data,
                state,
                CanInvestNow(node),
                state.IsNodeMastered(selectedNodeId),
                viewOnly,
                state.GetInvestedPoints(selectedNodeId),
                state.GetRequiredPoints(selectedNodeId),
                OnUnlockClicked
            );
        }

        private RuneNodeButton.VisualState NodeVisualState(RuneNode node)
        {
            if (node == null)
            {
                return RuneNodeButton.VisualState.Locked;
            }

            if (state != null && state.IsNodeMastered(node.Data.Id))
            {
                return RuneNodeButton.VisualState.Unlocked;
            }

            return CanSelectNode(node)
                ? RuneNodeButton.VisualState.Unlockable
                : RuneNodeButton.VisualState.Locked;
        }

        private bool CanInvestNow(RuneNode node)
        {
            if (state?.Tree == null || node == null || node.Unlocked)
            {
                return false;
            }

            return state.Tree.CanUnlock(node)
                && !state.IsNodeMastered(node.Data.Id)
                && state.RunePoints > 0;
        }

        private bool CanSelectNode(RuneNode node)
        {
            if (state?.Tree == null || node?.Data == null)
            {
                return false;
            }

            return state.GetInvestedPoints(node.Data.Id) > 0
                || state.IsNodeMastered(node.Data.Id)
                || state.Tree.CanUnlock(node);
        }

        private Color ConnectorColor(RuneNode parent, RuneNode child)
        {
            if (parent != null && child != null && parent.Unlocked && child.Unlocked)
            {
                return unlockedConnectorColor;
            }

            if (parent != null && parent.HasInvestment)
            {
                return unlockableConnectorColor;
            }

            return lockedConnectorColor;
        }

        private List<RuneNode> SortedNodes()
        {
            var nodes = new List<RuneNode>(state.Tree.AllNodes.Values);
            nodes.Sort(CompareNodes);
            return nodes;
        }

        private static int CompareNodes(RuneNode left, RuneNode right)
        {
            if (left?.Data == null && right?.Data == null)
            {
                return 0;
            }

            if (left?.Data == null)
            {
                return 1;
            }

            if (right?.Data == null)
            {
                return -1;
            }

            int row = left.Data.TreeRow.CompareTo(right.Data.TreeRow);
            if (row != 0)
            {
                return row;
            }

            int col = left.Data.TreeCol.CompareTo(right.Data.TreeCol);
            return col != 0 ? col : left.Data.Id.CompareTo(right.Data.Id);
        }

        private Dictionary<int, Vector2> BuildPositions(List<RuneNode> nodes)
        {
            var positions = new Dictionary<int, Vector2>();
            if (nodes == null || nodes.Count == 0)
            {
                return positions;
            }

            int minCol = nodes[0].Data.TreeCol;
            int maxCol = nodes[0].Data.TreeCol;
            for (int i = 1; i < nodes.Count; i++)
            {
                minCol = System.Math.Min(minCol, nodes[i].Data.TreeCol);
                maxCol = System.Math.Max(maxCol, nodes[i].Data.TreeCol);
            }

            float centerCol = (minCol + maxCol) * 0.5f;
            for (int i = 0; i < nodes.Count; i++)
            {
                RuneData data = nodes[i].Data;
                positions[data.Id] = new Vector2(
                    (data.TreeCol - centerCol) * cellSize.x,
                    -data.TreeRow * cellSize.y
                );
            }

            return positions;
        }

        private void EnsureSelectedNode()
        {
            if (state?.Tree?.AllNodes == null)
            {
                selectedNodeId = 0;
                return;
            }

            if (selectedNodeId != 0 && state.Tree.AllNodes.ContainsKey(selectedNodeId))
            {
                return;
            }

            selectedNodeId = state.Tree.Starter?.Data != null ? state.Tree.Starter.Data.Id : 0;
        }

        private void UpdateRunePointLabel()
        {
            if (runePointLabel != null)
            {
                runePointLabel.text =
                    state != null ? "RUNE POINTS " + state.RunePoints : "RUNE POINTS 0";
            }
        }

        private bool ValidateReferences()
        {
            bool valid =
                nodeButtonPrefab != null
                && connectorPrefab != null
                && treeContainer != null
                && connectorContainer != null
                && detailPanel != null;
            if (!valid)
            {
                Debug.LogError("[RuneTreeView] Required UI references are not assigned.");
            }

            return valid;
        }

        private void SubscribeEvents()
        {
            if (subscribedEvents != null)
            {
                return;
            }

            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Events != null)
            {
                subscribedEvents = gsm.Events;
                subscribedEvents.OnRuneNodeUnlocked += HandleRuneNodeUnlocked;
                subscribedEvents.OnRunePointsChanged += HandleRunePointsChanged;
                subscribedEvents.OnRuneClassChanged += HandleRuneClassChanged;
                subscribedEvents.OnRuneReset += HandleRuneReset;
            }
        }

        private void UnsubscribeEvents()
        {
            if (subscribedEvents == null)
            {
                return;
            }

            subscribedEvents.OnRuneNodeUnlocked -= HandleRuneNodeUnlocked;
            subscribedEvents.OnRunePointsChanged -= HandleRunePointsChanged;
            subscribedEvents.OnRuneClassChanged -= HandleRuneClassChanged;
            subscribedEvents.OnRuneReset -= HandleRuneReset;
            subscribedEvents = null;
        }

        private void HandleRuneNodeUnlocked(int nodeId, int remainingPoints)
        {
            Rebuild();
        }

        private void HandleRunePointsChanged(int currentPoints)
        {
            RefreshNodeStates();
            ShowSelectedNode();
        }

        private void HandleRuneClassChanged(RuneClass newClass)
        {
            Rebuild();
        }

        private void HandleRuneReset(int refundedPoints, int currentPoints)
        {
            Rebuild();
        }

        private void ApplyRuneEffectsToCurrentPlayer()
        {
            RuneRuntimeApplier.ApplyToCurrentPlayer(state);
        }

        private void ClearInstantiatedViews()
        {
            foreach (RuneNodeButton button in nodeButtons.Values)
            {
                DestroyView(button != null ? button.gameObject : null);
            }

            nodeButtons.Clear();

            for (int i = 0; i < connectors.Count; i++)
            {
                DestroyView(connectors[i] != null ? connectors[i].gameObject : null);
            }

            connectors.Clear();
        }

        private static void DestroyView(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}

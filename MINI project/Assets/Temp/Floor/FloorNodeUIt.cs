using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 플로어 맵의 노드 버튼 UI 1개. 클릭 시 컨트롤러로 위임.
    /// </summary>
    public sealed class FloorNodeUIt : MonoBehaviour
    {
        /// <summary>대응 노드 ID.</summary>
        public int NodeId;

        /// <summary>참조 컨트롤러.</summary>
        public FloorMapControllert Controller;

        /// <summary>
        /// 표시 상태 갱신(클리어/활성/비활성/잠김).
        /// </summary>
        public void Refresh(FloorNodet node, bool isSelectable, bool isLocked)
        {
            // 동작 요약:
            // - 색상: cleared=회색, selectable=강조, locked=어두움, default=기본.
            // - 보스 노드면 별도 아이콘.
            // - 버튼 인터랙션 isSelectable && !isLocked.
            //TODO: NodeId = node.NodeId;
            //TODO: if (node.IsCleared)          ButtonImage.color = ClearedColor;
            //TODO: else if (isLocked)           ButtonImage.color = LockedColor;
            //TODO: else if (isSelectable)       ButtonImage.color = SelectableColor;
            //TODO: else                          ButtonImage.color = DefaultColor;
            //TODO: BossIcon.SetActive(node.IsBoss);
            //TODO: GetComponent<UnityEngine.UI.Button>().interactable = isSelectable && !isLocked;
        }

        /// <summary>
        /// 클릭 시 호출. Inspector에서 버튼 OnClick에 바인딩하거나 코드 바인딩.
        /// </summary>
        public void OnClicked()
        {
            // 동작 요약: Controller.OnNodeClicked(NodeId) 호출.
            //TODO: Controller.OnNodeClicked(NodeId);
        }
    }
}

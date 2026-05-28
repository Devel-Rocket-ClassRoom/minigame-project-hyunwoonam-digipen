using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 종료 후 보상 화면. EXP/골드/획득 아이템을 표시하고
    /// 인벤토리 초과 아이템이 있을 경우 버리기/가져오기 선택 UI를 제공한다.
    ///
    /// 흐름:
    ///   1) GameSystemManager.EndCombat → UIManager.ShowCombatReward(summary, overflowIds, onClose) 호출.
    ///   2) Show()가 summary를 패널에 바인딩.
    ///   3) overflow 있으면 OverflowPanel 활성화 → 플레이어가 각 초과 아이템을 Take 또는 Discard.
    ///   4) 모든 초과 아이템 처리 완료 → Continue 버튼 활성화.
    ///   5) Continue 클릭 → onClose 콜백 → EndCombat 후처리(씬 전환).
    /// </summary>
    public sealed class CombatRewardPage : MonoBehaviour
    {
        /// <summary>페이지가 닫힐 때 호출되는 콜백(씬 전환 트리거).</summary>
        private Action onCloseCallback;

        /// <summary>현재 표시 중인 보상 요약.</summary>
        private NodeRewardSummary currentSummary;

        /// <summary>아직 처리되지 않은 인벤토리 초과 아이템 ID 목록.</summary>
        private List<int> pendingOverflowIds = new List<int>();

        /// <summary>
        /// 보상 화면 표시. UIManagert가 호출.
        /// </summary>
        /// <param name="summary">집계된 보상 요약.</param>
        /// <param name="overflowIds">인벤토리 초과로 아직 미확정인 아이템 ID 목록.</param>
        /// <param name="onClose">화면 닫힘 콜백.</param>
        public void Show(NodeRewardSummary summary, List<int> overflowIds, Action onClose)
        {
            // 동작 요약:
            // - currentSummary = summary, pendingOverflowIds = new List(overflowIds), onCloseCallback = onClose.
            // - gameObject.SetActive(true).
            // - BindSummaryPanel(summary) — EXP/골드/획득 아이템 목록 UI에 바인딩.
            // - overflowIds.Count > 0 → OverflowPanel 활성화, RefreshOverflowPanel().
            //   아니면 → ContinueButton 즉시 활성화.
            //TODO: currentSummary = summary;
            //TODO: pendingOverflowIds = overflowIds != null ? new List<int>(overflowIds) : new List<int>();
            //TODO: onCloseCallback = onClose;
            //TODO: gameObject.SetActive(true);
            //TODO: BindSummaryPanel(summary);
            //TODO: bool hasOverflow = pendingOverflowIds.Count > 0;
            //TODO: OverflowPanel.SetActive(hasOverflow);
            //TODO: ContinueButton.interactable = !hasOverflow;
            //TODO: if (hasOverflow) RefreshOverflowPanel();
        }

        /// <summary>
        /// 화면 숨김.
        /// </summary>
        public void Hide()
        {
            // 동작 요약: gameObject.SetActive(false).
            //TODO: gameObject.SetActive(false);
        }

        /// <summary>
        /// 요약 패널에 EXP, 골드, 획득 아이템 목록을 바인딩.
        /// </summary>
        private void BindSummaryPanel(NodeRewardSummary summary)
        {
            // 동작 요약:
            // - ExpLabel.text = $"+{summary.TotalExp} EXP".
            // - GoldLabel.text = $"+{summary.TotalGold} G".
            // - summary.DroppedItemIds 순회 → 획득 아이템 셀 생성(아이콘 + 이름).
            //   (인벤토리에 이미 넣은 것과 overflow 구분 없이 전체 드랍 목록 표시)
            //TODO: ExpLabel.text  = $"+{summary.TotalExp} EXP";
            //TODO: GoldLabel.text = $"+{summary.TotalGold} G";
            //TODO: // 아이템 목록 셀 생성(기존 셀 제거 후 재생성)
            //TODO: foreach (Transform child in ItemListParent) Destroy(child.gameObject);
            //TODO: foreach (int id in summary.DroppedItemIds)
            //TODO: {
            //TODO:     ItemData data = GameSystemManager.Instance.Data.Items[id];
            //TODO:     var cell = Instantiate(ItemCellPrefab, ItemListParent);
            //TODO:     cell.Bind(data); // 아이콘 + 이름 표시
            //TODO: }
        }

        /// <summary>
        /// 초과 아이템 패널 갱신. 남은 pendingOverflowIds 각각에 대해
        /// [아이템 이름] [가져오기] [버리기] 행을 생성.
        /// </summary>
        private void RefreshOverflowPanel()
        {
            // 동작 요약:
            // - OverflowListParent 자식 오브젝트 모두 Destroy.
            // - pendingOverflowIds 순회:
            //     * ItemData data = GameSystemManager.Instance.Data.Items[itemId].
            //     * OverflowRowt 셀 Instantiate → BindRow(row, itemId, data) 호출.
            // - pendingOverflowIds.Count == 0 → OverflowPanel 비활성, ContinueButton 활성화.
            //TODO: foreach (Transform child in OverflowListParent) Destroy(child.gameObject);
            //TODO: foreach (int id in pendingOverflowIds)
            //TODO: {
            //TODO:     ItemData data = GameSystemManager.Instance.Data.Items[id];
            //TODO:     var row = Instantiate(OverflowRowPrefab, OverflowListParent);
            //TODO:     int capturedId = id; // 람다 캡처
            //TODO:     row.BindRow(data,
            //TODO:         onTake:    () => OnOverflowTakeClicked(capturedId),
            //TODO:         onDiscard: () => OnOverflowDiscardClicked(capturedId));
            //TODO: }
            //TODO: if (pendingOverflowIds.Count == 0)
            //TODO: {
            //TODO:     OverflowPanel.SetActive(false);
            //TODO:     ContinueButton.interactable = true;
            //TODO: }
        }

        /// <summary>
        /// 초과 아이템 하나를 인벤토리에 가져오기. 플레이어가 [가져오기] 클릭 시 호출.
        /// 인벤토리에 공간이 없으면 다른 아이템을 버려야 함(DiscardFromInventory 흐름).
        /// </summary>
        public void OnOverflowTakeClicked(int itemId)
        {
            // 동작 요약:
            // - InventoryState inv = GameSystemManager.Instance.CurrentRun.Player.Inventory.
            // - ItemData data = GameSystemManager.Instance.Data.Items[itemId].
            // - data.IsStackable:
            //     * inv.TryAdd(itemId, 1) 성공 → pendingOverflowIds에서 itemId 하나 제거.
            //     * 실패 → 인벤토리 꽉 찼음 토스트 표시 (버리기 필요 안내).
            // - !data.IsStackable:
            //     * inv.TryAddEquip(new Item(data)) 성공 → 제거.
            //     * 실패 → 토스트.
            // - RefreshOverflowPanel().
            //TODO: var inv  = GameSystemManager.Instance.CurrentRun.Player.Inventory;
            //TODO: var data = GameSystemManager.Instance.Data.Items[itemId];
            //TODO: bool success;
            //TODO: if (data.Stackable)
            //TODO:     success = inv.TryAdd(itemId, 1);
            //TODO: else
            //TODO:     success = inv.TryAddEquip(new Item { Data = data, Enhancement = 0 });
            //TODO: if (!success) { ToastUI.Show("인벤토리 공간 부족"); return; }
            //TODO: pendingOverflowIds.Remove(itemId); // 첫 번째 일치 항목만 제거
            //TODO: RefreshOverflowPanel();
        }

        /// <summary>
        /// 초과 아이템 하나를 버리기. 플레이어가 [버리기] 클릭 시 호출.
        /// </summary>
        public void OnOverflowDiscardClicked(int itemId)
        {
            // 동작 요약:
            // - pendingOverflowIds에서 itemId 하나 제거.
            //   (드랍된 아이템은 아직 인벤토리에 없으므로 단순 제거만 하면 됨)
            // - RefreshOverflowPanel().
            //TODO: pendingOverflowIds.Remove(itemId);
            //TODO: RefreshOverflowPanel();
        }

        /// <summary>
        /// Continue 버튼 클릭. 모든 초과 처리 완료 후에만 활성화됨.
        /// </summary>
        public void OnContinueClicked()
        {
            // 동작 요약:
            // - pendingOverflowIds.Count > 0 → 미처리 항목 있으면 무시(버튼이 비활성 상태여야 하므로 방어 코드).
            // - Hide().
            // - onCloseCallback?.Invoke().
            //TODO: if (pendingOverflowIds.Count > 0) return; // 방어 코드
            //TODO: Hide();
            //TODO: onCloseCallback?.Invoke();
        }
    }
}


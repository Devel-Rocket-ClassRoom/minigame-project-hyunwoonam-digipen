namespace Tempt
{
    /// <summary>
    /// I 단축키 페이지. 인벤토리 + 장비 슬롯 + 소모 4칸 슬롯.
    /// 전투 중에는 view-only 모드(소모 4칸 변경 차단).
    /// </summary>
    public sealed class InventoryPaget : UIPageControllerBaset
    {
        /// <inheritdoc/>
        public override void OnOpen()
        {
            // 동작 요약:
            // - Player.Inventory / Equipment / Consumables 데이터 바인딩.
            // - EventBust 구독(인벤/장비 변경 시 다시 그림).
            // - IsViewOnly = !UIManagert.ConsumablesEditable일 때 4칸 비활성.
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 구독 해제.
        }

        /// <inheritdoc/>
        public override void OnEditableChanged(bool editable)
        {
            // 동작 요약: 4칸 슬롯 드래그/클릭 활성화 토글.
        }
    }
}

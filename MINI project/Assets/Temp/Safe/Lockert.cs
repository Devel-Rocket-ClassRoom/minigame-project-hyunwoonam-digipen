namespace Tempt
{
    /// <summary>
    /// 보관함 UI 모듈. 주점 진입 시 활성. 실제 데이터는 LockerStatet에 있다.
    /// </summary>
    public sealed class Lockert
    {
        /// <summary>
        /// 보관함 UI 열기.
        /// </summary>
        public void Open(LockerStatet lockerState, InventoryStatet inv)
        {
            // 동작 요약:
            // - lockerState.Unlocked 검사. 미활성이면 잠금 메시지.
            // - 양쪽 슬롯 표시: Inventory ↔ Locker.
            // - 드래그/버튼으로 InventoryStatet.MoveToLocker 또는 역방향 호출.
            //TODO: if (!lockerState.Unlocked) { ToastUI.Show("보관함 미구매"); return; }
            //TODO: InventoryPanel.Bind(inv);
            //TODO: LockerPanel.Bind(lockerState);
            //TODO: InventoryPanel.OnItemClicked = (id, cnt) => inv.MoveToLocker(lockerState, id, cnt);
            //TODO: LockerPanel.OnItemClicked   = (id, cnt) => inv.MoveFromLocker(lockerState, id, cnt);
        }
    }
}

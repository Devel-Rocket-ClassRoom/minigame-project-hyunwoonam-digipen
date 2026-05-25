namespace Tempt
{
    /// <summary>
    /// ESC 종료 확인 페이지. 네 → 종료, 아니요 → 닫기.
    /// </summary>
    public sealed class QuitConfirmPaget : UIPageControllerBaset
    {
        /// <inheritdoc/>
        public override void OnOpen()
        {
            // 동작 요약:
            // - 확인 메시지 표시(언어키 "quit.confirm").
            // - 네 버튼 → GSM.Save.SaveSnapshot(); GSM.QuitGame().
            // - 아니요 → UIManagert.CloseTopPage().
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 정리.
        }
    }
}

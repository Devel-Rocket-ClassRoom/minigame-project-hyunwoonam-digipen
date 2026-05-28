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
            //TODO: MessageLabel.text = LanguageServicet.Get("quit.confirm");
            //TODO: YesButton.onClick.AddListener(() =>
            //TODO: {
            //TODO:     GameSystemManagert.Instance.Save.SaveSnapshot();
            //TODO:     GameSystemManagert.Instance.QuitGame();
            //TODO: });
            //TODO: NoButton.onClick.AddListener(() => UIManagert.Instance.CloseTopPage());
        }

        /// <inheritdoc/>
        public override void OnClose()
        {
            // 동작 요약: 정리.
            //TODO: YesButton.onClick.RemoveAllListeners();
            //TODO: NoButton.onClick.RemoveAllListeners();
        }
    }
}

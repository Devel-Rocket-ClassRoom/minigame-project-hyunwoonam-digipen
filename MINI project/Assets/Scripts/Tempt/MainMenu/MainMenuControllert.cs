namespace Tempt
{
    /// <summary>
    /// 메인 메뉴 컨트롤러. New Game / Continue / Option / Exit + 확인 팝업.
    /// </summary>
    public sealed class MainMenuControllert : SceneControllerBaset
    {
        /// <summary>New Game 확인 팝업.</summary>
        public NewGameConfirmPopupt NewGameConfirm;

        /// <summary>옵션 페이지(UIManagert와 협업).</summary>
        public OptionPageHostt OptionHost;

        /// <inheritdoc/>
        public override void OnEnter()
        {
            // 동작 요약:
            // - Continue 버튼 활성 = GSM.Save.HasContinue().
            // - 4개 버튼 OnClick 바인딩.
            // - 비석/묘비 미리보기 표시(선택).
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            // 동작 요약: 정리.
        }

        /// <summary>New Game 클릭.</summary>
        public void OnClickNewGame()
        {
            // 동작 요약:
            // - GSM.Save.HasContinue()이면 NewGameConfirm.Show(); 확인 시 GSM.StartNewGame() 호출.
            // - 아니면 즉시 GSM.StartNewGame().
        }

        /// <summary>Continue 클릭.</summary>
        public void OnClickContinue()
        {
            // 동작 요약: GSM.ContinueGame() 호출.
        }

        /// <summary>Option 클릭.</summary>
        public void OnClickOption()
        {
            // 동작 요약: OptionHost.Open() — 언어/소리/화면모드.
        }

        /// <summary>Exit 클릭.</summary>
        public void OnClickExit()
        {
            // 동작 요약: UIManagert.Open(QuitConfirmPaget) 또는 즉시 GSM.QuitGame().
        }
    }

    /// <summary>옵션 페이지를 메인 메뉴에서 띄울 때 사용하는 어댑터.</summary>
    public sealed class OptionPageHostt
    {
        /// <summary>옵션 열기.</summary>
        public void Open()
        {
            // 동작 요약: UIManagert.OpenPage(OptionPage) 호출.
        }
    }
}

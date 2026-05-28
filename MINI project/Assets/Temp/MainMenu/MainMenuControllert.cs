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
            //TODO: ContinueButton.interactable = GameSystemManagert.Instance.Save.HasContinue();
            //TODO: NewGameButton.onClick.AddListener(OnClickNewGame);
            //TODO: ContinueButton.onClick.AddListener(OnClickContinue);
            //TODO: OptionButton.onClick.AddListener(OnClickOption);
            //TODO: ExitButton.onClick.AddListener(OnClickExit);
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            // 동작 요약: 정리.
            //TODO: NewGameButton.onClick.RemoveAllListeners();
            //TODO: ContinueButton.onClick.RemoveAllListeners();
            //TODO: OptionButton.onClick.RemoveAllListeners();
            //TODO: ExitButton.onClick.RemoveAllListeners();
        }

        /// <summary>New Game 클릭.</summary>
        public void OnClickNewGame()
        {
            // 동작 요약:
            // - GSM.Save.HasContinue()이면 NewGameConfirm.Show(); 확인 시 GSM.StartNewGame() 호출.
            // - 아니면 즉시 GSM.StartNewGame().
            //TODO: if (GameSystemManagert.Instance.Save.HasContinue())
            //TODO: {
            //TODO:     NewGameConfirm.OnConfirmed += GameSystemManagert.Instance.StartNewGame;
            //TODO:     NewGameConfirm.Show();
            //TODO: }
            //TODO: else GameSystemManagert.Instance.StartNewGame();
            GameSystemManagert gsm = GameSystemManagert.Instance; //Wave0write
            if (gsm.Save.HasContinue()) //Wave0write
            { //Wave0write
                gsm.Save.ClearContinue(); //Wave0write
            } //Wave0write

            gsm.StartNewGame(); //Wave0write
        }

        /// <summary>Continue 클릭.</summary>
        public void OnClickContinue()
        {
            // 동작 요약: GSM.ContinueGame() 호출.
            //TODO: GameSystemManagert.Instance.ContinueGame();
            GameSystemManagert.Instance.ContinueGame(); //Wave0write
        }

        /// <summary>Option 클릭.</summary>
        public void OnClickOption()
        {
            // 동작 요약: OptionHost.Open() — 언어/소리/화면모드.
            //TODO: OptionHost.Open();
            OptionHost?.Open(); //Wave0write
        }

        /// <summary>Exit 클릭.</summary>
        public void OnClickExit()
        {
            // 동작 요약: UIManagert.Open(QuitConfirmPaget) 또는 즉시 GSM.QuitGame().
            //TODO: GameSystemManagert.Instance.UI.ShowQuitConfirm();
            GameSystemManagert.Instance.QuitGame(); //Wave0write
        }
    }

    /// <summary>옵션 페이지를 메인 메뉴에서 띄울 때 사용하는 어댑터.</summary>
    public sealed class OptionPageHostt
    {
        /// <summary>옵션 열기.</summary>
        public void Open()
        {
            // 동작 요약: UIManagert.OpenPage(OptionPage) 호출.
            //TODO: GameSystemManagert.Instance.UI.OpenPage(OptionPage);
        }
    }
}

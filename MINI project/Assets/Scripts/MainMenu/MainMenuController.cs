using UnityEngine;
using UnityEngine.UI;

namespace Tempt
{
    /// <summary>
    /// 메인 메뉴 컨트롤러. New Game / Continue / Option / Exit + 확인 팝업.
    /// </summary>
    public sealed class MainMenuController : SceneControllerBase
    {
        /// <summary>New Game 확인 팝업.</summary>
        public NewGameConfirmPopup NewGameConfirm;

        /// <summary>옵션 페이지(UIManagert와 협업).</summary>
        public OptionPageHost OptionHost;

        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button optionButton;
        [SerializeField] private Button exitButton;

        /// <inheritdoc/>
        public override void OnEnter()
        {
            if (!ValidateButtonRefs())
            {
                return;
            }

            GameSystemManager gsm = GameSystemManager.Instance;
            continueButton.interactable = gsm != null && gsm.Save != null && gsm.Save.HasContinue();
            newGameButton.onClick.AddListener(OnClickNewGame);
            continueButton.onClick.AddListener(OnClickContinue);
            optionButton.onClick.AddListener(OnClickOption);
            exitButton.onClick.AddListener(OnClickExit);
        }

        /// <inheritdoc/>
        public override void OnExit()
        {
            if (newGameButton != null) newGameButton.onClick.RemoveListener(OnClickNewGame);
            if (continueButton != null) continueButton.onClick.RemoveListener(OnClickContinue);
            if (optionButton != null) optionButton.onClick.RemoveListener(OnClickOption);
            if (exitButton != null) exitButton.onClick.RemoveListener(OnClickExit);
        }

        /// <summary>New Game 클릭.</summary>
        public void OnClickNewGame()
        {
            GameSystemManager gsm = GameSystemManager.Instance; //Wave0write
            if (gsm == null)
            {
                Debug.LogError("[MainMenuController] GameSystemManager 인스턴스를 찾을 수 없습니다.");
                return;
            }

            if (gsm.Save != null && gsm.Save.HasContinue()) //Wave0write
            { //Wave0write
                gsm.Save.ClearContinue(); //Wave0write
            } //Wave0write

            gsm.StartNewGame(); //Wave0write
        }

        /// <summary>Continue 클릭.</summary>
        public void OnClickContinue()
        {
            GameSystemManager gsm = GameSystemManager.Instance; //Wave0write
            if (gsm == null)
            {
                Debug.LogError("[MainMenuController] GameSystemManager 인스턴스를 찾을 수 없습니다.");
                return;
            }

            gsm.ContinueGame(); //Wave0write
        }

        /// <summary>Option 클릭.</summary>
        public void OnClickOption()
        {
            OptionHost?.Open(); //Wave0write
        }

        /// <summary>Exit 클릭.</summary>
        public void OnClickExit()
        {
            GameSystemManager gsm = GameSystemManager.Instance; //Wave0write
            if (gsm != null)
            {
                gsm.QuitGame(); //Wave0write
            }
        }

        private bool ValidateButtonRefs()
        {
            if (newGameButton != null && continueButton != null && optionButton != null && exitButton != null)
            {
                return true;
            }

            Debug.LogError("[MainMenuController] 버튼 참조가 씬에 직접 할당되어 있지 않습니다.");
            return false;
        }
    }

    /// <summary>옵션 페이지를 메인 메뉴에서 띄울 때 사용하는 어댑터.</summary>
    public sealed class OptionPageHost
    {
        /// <summary>옵션 열기.</summary>
        public void Open()
        {
        }
    }
}


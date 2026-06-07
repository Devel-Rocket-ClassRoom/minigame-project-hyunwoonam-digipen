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

        /// <summary>옵션 패널 컨트롤러.</summary>
        [SerializeField]
        private OptionsPage optionsPage;

        [SerializeField]
        private Button newGameButton;

        [SerializeField]
        private Button continueButton;

        [SerializeField]
        private Button optionButton;

        [SerializeField]
        private Button exitButton;

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
            if (newGameButton != null)
                newGameButton.onClick.RemoveListener(OnClickNewGame);
            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnClickContinue);
            if (optionButton != null)
                optionButton.onClick.RemoveListener(OnClickOption);
            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnClickExit);
        }

        /// <summary>New Game 클릭.</summary>
        public void OnClickNewGame()
        {
            GameSystemManager gsm = GameSystemManager.Instance;
            if (gsm == null)
            {
                GameLog.LogError(
                    "[MainMenuController] GameSystemManager 인스턴스를 찾을 수 없습니다."
                );
                return;
            }

            if (gsm.Save != null && gsm.Save.HasContinue())
            {
                gsm.Save.ClearContinue();
            }

            gsm.StartNewGame();
        }

        /// <summary>Continue 클릭.</summary>
        public void OnClickContinue()
        {
            GameSystemManager gsm = GameSystemManager.Instance;
            if (gsm == null)
            {
                GameLog.LogError(
                    "[MainMenuController] GameSystemManager 인스턴스를 찾을 수 없습니다."
                );
                return;
            }

            gsm.ContinueGame();
        }

        /// <summary>Option 클릭.</summary>
        public void OnClickOption()
        {
            optionsPage?.Open();
        }

        /// <summary>Exit 클릭.</summary>
        public void OnClickExit()
        {
            GameSystemManager gsm = GameSystemManager.Instance;
            if (gsm != null)
            {
                gsm.QuitGame();
            }
        }

        private bool ValidateButtonRefs()
        {
            if (
                newGameButton != null
                && continueButton != null
                && optionButton != null
                && exitButton != null
            )
            {
                return true;
            }

            GameLog.LogError("[MainMenuController] 버튼 참조가 씬에 직접 할당되어 있지 않습니다.");
            return false;
        }
    }
}

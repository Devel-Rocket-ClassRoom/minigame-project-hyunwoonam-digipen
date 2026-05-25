using System;

namespace Tempt
{
    /// <summary>
    /// New Game 시 Continue 데이터가 있을 때 띄우는 확인 팝업.
    /// 네 → 새 게임 시작(Continue 삭제), 아니요 → 창 닫힘.
    /// </summary>
    public sealed class NewGameConfirmPopupt
    {
        /// <summary>확인 시 호출되는 콜백(연결자가 등록).</summary>
        public event Action OnConfirmed;

        /// <summary>
        /// 팝업 표시.
        /// </summary>
        public void Show()
        {
            // 동작 요약:
            // - UIManagert.OpenPage(ConfirmPage, message=언어키 "main.confirm_new_game").
            // - 네 → GSM.Save.ClearContinue() + OnConfirmed.Invoke().
            // - 아니요 → 팝업 닫기.
        }
    }
}

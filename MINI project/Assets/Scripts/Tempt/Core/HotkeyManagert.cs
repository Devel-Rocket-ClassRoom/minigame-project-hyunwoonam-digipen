using System;

namespace Tempt
{
    /// <summary>
    /// 글로벌 단축키 입력 라우터. 어느 씬에서도 동작.
    /// 실제 페이지 표시는 UIManagert에 위임한다.
    /// </summary>
    public sealed class HotkeyManagert
    {
        /// <summary>특정 페이지 열기/닫기 요청 발생.</summary>
        public event Action<HotkeyPageIdt> OnTogglePage;

        /// <summary>종료 확인 요청 발생(ESC).</summary>
        public event Action OnRequestQuit;

        /// <summary>
        /// Update 루프에서 키 입력을 폴링한다.
        /// </summary>
        public void PollInput()
        {
            // 동작 요약:
            // - I → OnTogglePage(Inventory).
            // - K → OnTogglePage(Skill).
            // - S → OnTogglePage(StatRune).
            // - ESC → OnRequestQuit().
            // - 단, 현재 활성 씬이 Combat이고 단축키가 소비 4칸 변경을 일으키는 입력이면 차단(UIManagert가 페이지 모드를 view-only로 표시).
        }

        /// <summary>
        /// 초기 키 바인딩 등록.
        /// </summary>
        public void BindGlobalKeys()
        {
            // 동작 요약:
            // - GameSystemManagert에서 Update 폴링 호출을 등록.
            // - 키 매핑 테이블 초기화(추후 옵션에서 재바인딩 지원).
        }
    }

    /// <summary>단축키로 열리는 페이지 ID.</summary>
    public enum HotkeyPageIdt
    {
        /// <summary>I: 인벤토리/장비/소모 4칸.</summary>
        Inventory,

        /// <summary>K: 스킬 확인.</summary>
        Skill,

        /// <summary>S: 스탯/룬 확인.</summary>
        StatRune,
    }
}

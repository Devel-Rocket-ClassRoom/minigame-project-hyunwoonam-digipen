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
            //TODO: if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.I)) OnTogglePage?.Invoke(HotkeyPageIdt.Inventory);
            //TODO: if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.K)) OnTogglePage?.Invoke(HotkeyPageIdt.Skill);
            //TODO: if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.S)) OnTogglePage?.Invoke(HotkeyPageIdt.StatRune);
            //TODO: if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape)) OnRequestQuit?.Invoke();
            // 소비 4칸 변경 차단은 UIManagert.SetConsumablesEditable(false) 상태에서 자동 처리됨
        }

        /// <summary>
        /// 초기 키 바인딩 등록.
        /// </summary>
        public void BindGlobalKeys()
        {
            // 동작 요약:
            // - GameSystemManagert에서 Update 폴링 호출을 등록.
            // - 키 매핑 테이블 초기화(추후 옵션에서 재바인딩 지원).
            //TODO: // 현재는 PollInput()이 GameSystemManagert.Update()에서 매 프레임 직접 호출됨
            //TODO: // 키 리매핑 테이블 예시: keyMap[HotkeyPageIdt.Inventory] = KeyCode.I 등
            //TODO: // 추후 옵션 화면에서 keyMap을 수정하면 PollInput에서 keyMap을 참조하도록 구조 변경
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

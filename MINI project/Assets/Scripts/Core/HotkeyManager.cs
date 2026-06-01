using System;

namespace Tempt
{
    /// <summary>
    /// 글로벌 단축키 입력 라우터. 어느 씬에서도 동작.
    /// 실제 페이지 표시는 UIManagert에 위임한다.
    /// </summary>
    public sealed class HotkeyManager
    {
        /// <summary>특정 페이지 열기/닫기 요청 발생.</summary>
        public event Action<HotkeyPageId> OnTogglePage;

        /// <summary>종료 확인 요청 발생(ESC).</summary>
        public event Action OnRequestQuit;

        /// <summary>
        /// Update 루프에서 키 입력을 폴링한다.
        /// </summary>
        public void PollInput()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.I))
            {
                OnTogglePage?.Invoke(HotkeyPageId.Inventory);
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.K))
            {
                OnTogglePage?.Invoke(HotkeyPageId.Skill);
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.S))
            {
                OnTogglePage?.Invoke(HotkeyPageId.StatRune);
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.R))
            {
                OnTogglePage?.Invoke(HotkeyPageId.Rune);
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape))
            {
                OnRequestQuit?.Invoke();
            }
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
    public enum HotkeyPageId
    {
        /// <summary>I: 인벤토리/장비/소모 4칸.</summary>
        Inventory,

        /// <summary>K: 스킬 확인.</summary>
        Skill,

        /// <summary>S: 스탯/룬 확인.</summary>
        StatRune,

        /// <summary>R: 룬 트리 확인.</summary>
        Rune,
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 글로벌 UI 매니저. 공통 페이지(인벤토리/스킬/스탯룬/종료 확인/옵션)와 단축키 라우팅,
    /// 화면비 적응, 언어 변경을 담당.
    /// 씬별 UI는 UIPageControllerBaset 파생이 처리.
    /// </summary>
    public sealed class UIManagert
    {
        /// <summary>활성 페이지 스택.</summary>
        public Stack<UIPageControllerBaset> PageStack = new Stack<UIPageControllerBaset>();

        /// <summary>현재 소모 4칸 편집 가능 여부(전투에선 false).</summary>
        public bool ConsumablesEditable { get; private set; } = true;

        /// <summary>언어 서비스.</summary>
        public LanguageServicet Language;

        /// <summary>화면 적응 스케일러.</summary>
        public ResponsiveCanvasScalert Scaler;

        /// <summary>
        /// 단축키로 호출되는 페이지 토글.
        /// </summary>
        public void TogglePage(HotkeyPageIdt pageId)
        {
            // 동작 요약:
            // - pageId → 실제 페이지 매핑(Inventory→InventoryPaget 등).
            // - 페이지가 스택 최상위면 닫기, 아니면 OpenPage.
            // - 단, ConsumablesEditable=false이면 인벤토리는 view-only 모드로 표시.
            //TODO: UIPageControllerBaset targetPage = GetPageByHotkey(pageId); // 페이지 인스턴스 참조 반환
            //TODO: if (PageStack.Count > 0 && PageStack.Peek() == targetPage) { CloseTopPage(); return; }
            //TODO: if (pageId == HotkeyPageIdt.Inventory && !ConsumablesEditable)
            //TODO:     (targetPage as InventoryPaget)?.SetViewOnly(true); // 소비 4칸 편집 비활성
            //TODO: OpenPage(targetPage);
        }

        /// <summary>
        /// 페이지 열기.
        /// </summary>
        public void OpenPage(UIPageControllerBaset page)
        {
            // 동작 요약:
            // - PageStack.Push(page).
            // - page.OnOpen() 호출.
            //TODO: PageStack.Push(page);
            //TODO: page.OnOpen();
        }

        /// <summary>
        /// 페이지 닫기(최상위).
        /// </summary>
        public void CloseTopPage()
        {
            // 동작 요약:
            // - PageStack.Pop().OnClose().
            //TODO: if (PageStack.Count == 0) return;
            //TODO: PageStack.Pop().OnClose();
        }

        /// <summary>
        /// 소모 4칸 편집 가능 여부 설정. CombatControllert가 false로 전환.
        /// </summary>
        public void SetConsumablesEditable(bool value)
        {
            // 동작 요약: ConsumablesEditable = value; 활성 페이지에 알림.
            //TODO: ConsumablesEditable = value;
            //TODO: // 현재 인벤토리 페이지가 열려있으면 즉시 반영
            //TODO: if (PageStack.Count > 0 && PageStack.Peek() is InventoryPaget inv)
            //TODO:     inv.SetViewOnly(!value);
        }

        /// <summary>
        /// ESC 종료 확인. HotkeyManagert.OnRequestQuit에서 라우팅.
        /// </summary>
        public void ShowQuitConfirm()
        {
            // 동작 요약: OpenPage(QuitConfirmPaget).
            //TODO: OpenPage(QuitConfirmPage); // QuitConfirmPage: Inspector 참조 또는 싱글톤
        }
    }
}

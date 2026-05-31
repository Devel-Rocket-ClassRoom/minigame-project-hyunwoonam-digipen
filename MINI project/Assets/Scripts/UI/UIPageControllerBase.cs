using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 페이지(팝업/오버레이/풀씬 UI) 베이스. 씬별 PageController가 파생.
    /// </summary>
    public abstract class UIPageControllerBase : MonoBehaviour
    {
        /// <summary>이 페이지의 식별 키(로그/저장 등에 사용).</summary>
        public string PageKey;

        /// <summary>이 페이지가 view-only 모드(예: 전투 중 인벤토리)인가.</summary>
        public bool IsViewOnly;

        /// <summary>페이지 열림 시 호출.</summary>
        public abstract void OnOpen();

        /// <summary>페이지 닫힘 시 호출.</summary>
        public abstract void OnClose();

        /// <summary>매 프레임 업데이트 필요 시 override.</summary>
        public virtual void OnPageUpdate()
        {
            // 동작 요약: 기본 구현 없음.
        }

        /// <summary>view-only 전환/해제 시 호출.</summary>
        public virtual void OnEditableChanged(bool editable)
        {
            // 동작 요약: 편집 컨트롤(드래그/버튼) 활성/비활성 토글.
        }
    }
}


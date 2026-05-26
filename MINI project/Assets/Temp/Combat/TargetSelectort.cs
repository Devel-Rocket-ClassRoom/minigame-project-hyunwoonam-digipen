using System;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 플레이어 입력 타겟팅. 마우스 레이캐스트 + 하단 캔버스 검은 직선 표시.
    /// SkillTargetTypet.EnemySingle / AllySingle에서만 사용.
    /// </summary>
    public sealed class TargetSelectort
    {
        /// <summary>현재 호버 모드(어떤 종류 타겟을 받는가).</summary>
        public SkillTargetTypet Mode { get; private set; }

        /// <summary>호버 중 활성 여부.</summary>
        public bool IsActive { get; private set; }

        /// <summary>타겟 호버 변경 콜백.</summary>
        public event Action<EntityBaset> OnHoverChanged;

        /// <summary>타겟 확정(클릭) 콜백.</summary>
        public event Action<EntityBaset> OnTargetConfirmed;

        private EntityBaset lastHovered; //Wave0write

        /// <summary>
        /// 타겟팅 시작.
        /// </summary>
        public void BeginHover(SkillTargetTypet mode)
        {
            // 동작 요약:
            // - Mode = mode; IsActive = true.
            // - TargetingLineUIt.Show(true).
            //TODO: Mode = mode;
            //TODO: IsActive = true;
            //TODO: TargetingLineUI.Show(true);
            Mode = mode; //Wave0write
            IsActive = true; //Wave0write
            lastHovered = null; //Wave0write
        }

        /// <summary>
        /// 매 프레임 호출. 레이캐스트로 후보 탐지.
        /// </summary>
        public void Tick()
        {
            // 동작 요약:
            // - 카메라.ScreenPointToRay에서 EntityBaset 콜라이더 검사.
            // - 후보가 Mode와 맞는 진영인지 검사(Enemy/Ally).
            // - 후보 바뀌면 OnHoverChanged 발생 + 검은 직선 위치 갱신.
            // - 마우스 클릭 + 유효 후보면 OnTargetConfirmed.
            // - ESC/우클릭으로 취소.
            //TODO: if (!IsActive) return;
            //TODO: Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //TODO: EntityBaset hovered = null;
            //TODO: if (Physics.Raycast(ray, out RaycastHit hit))
            //TODO:     hovered = hit.collider.GetComponentInParent<EntityBaset>();
            //TODO: // 진영 필터
            //TODO: bool validTarget = hovered != null &&
            //TODO:     ((Mode == SkillTargetTypet.EnemySingle && hovered is MonsterBaset) ||
            //TODO:      (Mode == SkillTargetTypet.AllySingle  && (hovered is Playert || hovered is TeamBaset)));
            //TODO: if (!validTarget) hovered = null;
            //TODO: // 호버 변경 알림
            //TODO: if (hovered != _lastHovered) { _lastHovered = hovered; OnHoverChanged?.Invoke(hovered); }
            //TODO: // 직선 위치 갱신
            //TODO: if (hovered != null) TargetingLineUI.SetPoints(Camera.main.transform.position, hovered.transform.position);
            //TODO: // 클릭 확정
            //TODO: if (Input.GetMouseButtonDown(0) && hovered != null) { OnTargetConfirmed?.Invoke(hovered); EndHover(); }
            //TODO: // 취소
            //TODO: if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) EndHover();
            if (!IsActive || Camera.main == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition); //Wave0write
            EntityBaset hovered = null; //Wave0write
            if (Physics.Raycast(ray, out RaycastHit hit)) //Wave0write
            { //Wave0write
                hovered = hit.collider.GetComponentInParent<EntityBaset>(); //Wave0write
            } //Wave0write

            bool validTarget = hovered != null && //Wave0write
                ((Mode == SkillTargetTypet.EnemySingle && hovered is MonsterBaset) || //Wave0write
                 (Mode == SkillTargetTypet.AllySingle && (hovered is Playert || hovered is TeamBaset))); //Wave0write
            if (!validTarget) //Wave0write
            { //Wave0write
                hovered = null; //Wave0write
            } //Wave0write

            if (hovered != lastHovered) //Wave0write
            { //Wave0write
                lastHovered = hovered; //Wave0write
                OnHoverChanged?.Invoke(hovered); //Wave0write
            } //Wave0write

            if (UnityEngine.Input.GetMouseButtonDown(0) && hovered != null) //Wave0write
            { //Wave0write
                OnTargetConfirmed?.Invoke(hovered); //Wave0write
                EndHover(); //Wave0write
            } //Wave0write

            if (UnityEngine.Input.GetMouseButtonDown(1) || UnityEngine.Input.GetKeyDown(KeyCode.Escape)) //Wave0write
            { //Wave0write
                EndHover(); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 타겟팅 종료.
        /// </summary>
        public void EndHover()
        {
            // 동작 요약:
            // - IsActive = false; TargetingLineUIt.Show(false).
            //TODO: IsActive = false;
            //TODO: _lastHovered = null;
            //TODO: TargetingLineUI.Show(false);
            IsActive = false; //Wave0write
            lastHovered = null; //Wave0write
            OnHoverChanged?.Invoke(null); //Wave0write
        }
    }
}

using System;
using UnityEngine;

namespace Tempt
{
    // Wave0refactor 2026-05-27 (F.9): Camera.main 호출을 매 프레임에서 BeginHover 시 1회 캐시로 변경.
    /// <summary>
    /// 플레이어 입력 타겟팅. 마우스 레이캐스트 + 하단 캔버스 검은 직선 표시.
    /// SkillTargetType.EnemySingle / AllySingle 에서만 사용.
    /// </summary>
    public sealed class TargetSelector
    {
        /// <summary>현재 호버 모드(어떤 종류 타겟을 받는가).</summary>
        public SkillTargetType Mode { get; private set; }

        /// <summary>호버 중 활성 여부.</summary>
        public bool IsActive { get; private set; }

        /// <summary>타겟 호버 변경 콜백.</summary>
        public event Action<EntityBase> OnHoverChanged;

        /// <summary>타겟 확정(클릭) 콜백.</summary>
        public event Action<EntityBase> OnTargetConfirmed;

        private EntityBase lastHovered;
        private Camera cachedCamera;

        /// <summary>
        /// 타겟팅 시작. 호버 진입 시 1회 카메라 캐시.
        /// 씬 전환 후에는 OnEnter 흐름에서 BeginHover 가 다시 호출되므로 자동으로 재캐시된다.
        /// </summary>
        public void BeginHover(SkillTargetType mode)
        {
            // 동작 요약:
            // - Mode / IsActive 설정.
            // - lastHovered 초기화.
            // - Camera.main 을 1회 캐시(매 프레임 호출 방지).
            Mode = mode;
            IsActive = true;
            lastHovered = null;
            cachedCamera = Camera.main;
        }

        /// <summary>
        /// 매 프레임 호출. 레이캐스트로 후보 탐지.
        /// </summary>
        public void Tick()
        {
            // 동작 요약:
            // - IsActive == false 또는 cachedCamera == null 이면 즉시 반환.
            // - 마우스 위치에서 레이캐스트 → 콜라이더의 EntityBase 후보 탐지.
            // - 진영 필터(EnemySingle ↔ MonsterBase, AllySingle ↔ Player/TeamBase).
            // - 후보 변경 시 OnHoverChanged 발생.
            // - 좌클릭 + 유효 후보면 OnTargetConfirmed 발생 후 EndHover.
            // - 우클릭/ESC 면 취소.
            if (!IsActive || cachedCamera == null)
            {
                return;
            }

            Ray ray = cachedCamera.ScreenPointToRay(Input.mousePosition);
            EntityBase hovered = null;
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hovered = hit.collider.GetComponentInParent<EntityBase>();
            }

            bool validTarget = hovered != null &&
                ((Mode == SkillTargetType.EnemySingle && hovered is MonsterBase) ||
                 (Mode == SkillTargetType.AllySingle && (hovered is Player || hovered is TeamBase)));
            if (!validTarget)
            {
                hovered = null;
            }

            if (hovered != lastHovered)
            {
                lastHovered = hovered;
                OnHoverChanged?.Invoke(hovered);
            }

            if (Input.GetMouseButtonDown(0) && hovered != null)
            {
                OnTargetConfirmed?.Invoke(hovered);
                EndHover();
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                EndHover();
            }
        }

        /// <summary>
        /// 타겟팅 종료.
        /// </summary>
        public void EndHover()
        {
            // 동작 요약:
            // - IsActive 해제, lastHovered 초기화, 호버 변경 알림.
            // - cachedCamera 는 유지(다음 BeginHover 가 자동 갱신).
            IsActive = false;
            lastHovered = null;
            OnHoverChanged?.Invoke(null);
        }
    }
}

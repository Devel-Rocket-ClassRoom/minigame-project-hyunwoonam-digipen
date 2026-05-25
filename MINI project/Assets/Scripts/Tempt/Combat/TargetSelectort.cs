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

        /// <summary>
        /// 타겟팅 시작.
        /// </summary>
        public void BeginHover(SkillTargetTypet mode)
        {
            // 동작 요약:
            // - Mode = mode; IsActive = true.
            // - TargetingLineUIt.Show(true).
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
        }

        /// <summary>
        /// 타겟팅 종료.
        /// </summary>
        public void EndHover()
        {
            // 동작 요약:
            // - IsActive = false; TargetingLineUIt.Show(false).
        }
    }
}

using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 엔티티 머리 위 World Space Canvas. HP 슬라이더 + 행동 아이콘(빨강/파랑).
    /// </summary>
    public sealed class EntityWorldUIt : MonoBehaviour
    {
        /// <summary>대응 엔티티.</summary>
        public EntityBaset Entity;

        /// <summary>HP 슬라이더 컴포넌트(Inspector 연결).</summary>
        public UnityEngine.UI.Slider HpSlider;

        /// <summary>액션 아이콘.</summary>
        public ActionIconUIt ActionIcon;

        /// <summary>
        /// 매 프레임 HP 슬라이더 동기화.
        /// </summary>
        private void Update()
        {
            // 동작 요약:
            // - Entity.Stats.CurrentHP / MaxHP 비율로 슬라이더 value 갱신.
            //TODO: if (Entity == null || HpSlider == null) return;
            //TODO: HpSlider.value = (float)Entity.Stats.CurrentHP / Entity.Stats.MaxHP;
        }

        /// <summary>
        /// 공격/스킬 시 빨강 아이콘 표시.
        /// </summary>
        public void ShowAttackIcon()
        {
            // 동작 요약: ActionIcon.SetMode(AttackRed) + 일정 시간 후 자동 숨김.
            //TODO: ActionIcon.SetMode(ActionIconModet.AttackRed);
            //TODO: Invoke(nameof(HideActionIcon), 0.8f); // 0.8초 후 자동 숨김
        }

        /// <summary>
        /// 방어 시 파랑 아이콘 표시.
        /// </summary>
        public void ShowDefendIcon()
        {
            // 동작 요약: ActionIcon.SetMode(DefendBlue) + 라운드 끝까지 유지.
            //TODO: ActionIcon.SetMode(ActionIconModet.DefendBlue);
            // 라운드 끝에 CombatFlowt가 HideActionIcon 호출
        }

        /// <summary>
        /// 아이콘 숨김.
        /// </summary>
        public void HideActionIcon()
        {
            // 동작 요약: ActionIcon.Hide().
            //TODO: ActionIcon.Hide();
        }

        /// <summary>피격 연출.</summary>
        public void PlayHitFx()
        {
            // 동작 요약: 셰이크 / 스플래시 이펙트 호출.
            //TODO: // 스프라이트 셰이크(DOTween 또는 코루틴)
            //TODO: StartCoroutine(ShakeRoutine(0.2f, 0.1f));
            //TODO: // 히트 스플래시 파티클(Addressables 또는 Pool)
            //TODO: VfxPoolt.Play("HitSplash", transform.position);
        }

        /// <summary>회복 연출.</summary>
        public void PlayHealFx()
        {
            // 동작 요약: 회복 파티클 호출.
            //TODO: VfxPoolt.Play("HealParticle", transform.position);
        }
    }
}

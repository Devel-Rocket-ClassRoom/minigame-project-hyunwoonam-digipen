using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 모든 전투 엔티티(Player/Companion/Monster)의 공통 베이스.
    /// 보유: 스탯, 액티브 스킬 2칸, 공격/방어/피해 처리, 머리 위 UI 훅.
    /// 패시브 슬롯은 룬으로 대체되어 제거됨.
    /// </summary>
    public abstract class EntityBaset : MonoBehaviour
    {
        /// <summary>스탯 블록.</summary>
        public StatBlockt Stats { get; protected set; }

        /// <summary>액티브 스킬 2칸.</summary>
        public Skillt[] ActiveSkills { get; protected set; } = new Skillt[2];

        /// <summary>이번 라운드 동안 방어 중인가.</summary>
        public bool IsDefending { get; private set; }

        /// <summary>사망 여부.</summary>
        public bool IsDead => Stats != null && Stats.CurrentHP <= 0;

        /// <summary>표시 이름(언어 키 또는 직접 문자열).</summary>
        public string DisplayName;

        /// <summary>머리 위 UI 핸들.</summary>
        public EntityWorldUIt WorldUI;

        /// <summary>
        /// 전투 진입 시 슬롯/상태를 정리한다.
        /// HP/MP의 완전 회복 여부는 파생 클래스 결정.
        /// </summary>
        public virtual void PrepareForCombat()
        {
            // 동작 요약:
            // - Stats.ClampToMax().
            // - IsDefending = false.
            // - 액티브 스킬 슬롯 길이 2 보정.
            // - WorldUI.HideActionIcon().
            // - 추후 룬으로 인한 패시브 효과 적용 훅 호출(파생에서).
        }

        /// <summary>
        /// 라운드 종료 시 호출. 방어 해제 등 정리.
        /// </summary>
        public virtual void OnRoundEnd()
        {
            // 동작 요약: IsDefending = false; WorldUI.HideActionIcon().
        }

        /// <summary>
        /// 데미지 적용. DamageCalculatort에서 호출.
        /// </summary>
        public int ApplyDamage(int rawDamage)
        {
            // 동작 요약:
            // - Stats.TakeDamage(rawDamage, IsDefending) 호출(IsDefending이 true면 추가 경감).
            // - WorldUI.PlayHitFx().
            // - IsDead면 GameSystemManagert.Instance.Events에 사망 이벤트는 CombatFlowt가 발행.
            // - 실제 차감 피해 반환.
            return 0;
        }

        /// <summary>
        /// 회복 적용.
        /// </summary>
        public int ApplyHeal(int rawHeal)
        {
            // 동작 요약:
            // - Stats.CurrentHP에 더하고 MaxHP로 클램프.
            // - WorldUI.PlayHealFx().
            // - 실제 회복량 반환.
            return 0;
        }

        /// <summary>
        /// 보호막 적용(이번 라운드 한정 또는 일정 시간).
        /// </summary>
        public void ApplyShield(int amount, int durationRounds)
        {
            // 동작 요약: 보호막 상태 컴포넌트(미정의 시 단순 누적값) 추가.
        }

        /// <summary>
        /// 이번 턴 방어 상태 설정.
        /// </summary>
        public void SetDefending(bool value)
        {
            // 동작 요약: IsDefending = value; 머리 위 파랑 아이콘 표시/해제.
        }

        /// <summary>
        /// 액티브 스킬 슬롯 조회.
        /// </summary>
        public Skillt GetActiveSkill(int slotIndex)
        {
            // 동작 요약: 범위 검사 후 ActiveSkills[slotIndex] 반환.
            return null;
        }

        /// <summary>
        /// 액티브 스킬 슬롯 설정. 길드/룬 변경 시 호출.
        /// </summary>
        public void SetActiveSkill(int slotIndex, Skillt skill)
        {
            // 동작 요약: 범위 검사 후 ActiveSkills[slotIndex] = skill.
        }
    }
}

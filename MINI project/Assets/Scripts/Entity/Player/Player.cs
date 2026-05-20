using UnityEngine;

/// <summary>
/// 플레이어 캐릭터입니다.
/// </summary>
/// <remarks>
/// Week 1에서는 EntityBase의 임시 전투 수치와 스킬 슬롯만 보유합니다.
/// 행동 결정과 턴 진행은 CombatFlow가 담당합니다.
/// 스킬 슬롯은 비어 있을 수 있으며, 추후 안전지대에서 장착합니다.
/// </remarks>
public class Player : EntityBase
{
    public override void ResetForNewCombat()
    {
        // TODO:
        // - 목표: 새 전투에 들어가도 Player의 현재 HP/MP를 최대치로 회복하지 않는다.
        // - 의도: 전투 사이 체력과 MP가 현재 도전 상태로 이어지게 한다.
        // - 구현해야 할 것: EntityBase.PrepareForCombat으로 슬롯/범위/방어 상태만 보정한다.
        PrepareForCombat();
    }

    /// <summary>
    /// 슬롯 1 스킬입니다.
    /// </summary>
    public ActiveSkill Skill1 => GetActiveSkill(0);

    /// <summary>
    /// 슬롯 2 스킬입니다.
    /// </summary>
    public ActiveSkill Skill2 => GetActiveSkill(1);

    public void EquipActiveSkill(int slotIndex, ActiveSkill skill)
    {
        // 기존 구현:
        // SetActiveSkill(slotIndex, skill);

        // TODO:
        // - 목표: 지정한 액티브 스킬 슬롯에 새 스킬을 장착한다.
        // - 의도: 안전지대 신전/스킬 UI에서 Player의 액티브 슬롯을 교체할 수 있게 한다.
        // - 구현해야 할 것: slotIndex 검증은 EntityBase의 슬롯 설정 함수에 위임하고 ActiveSkill 참조를 저장한다.
        SetActiveSkill(slotIndex, skill);
    }

    public void EquipPassiveSkill(int slotIndex, PassiveSkill skill)
    {
        // 기존 구현:
        // SetPassiveSkill(slotIndex, skill);

        // TODO:
        // - 목표: 지정한 패시브 스킬 슬롯에 새 스킬을 장착한다.
        // - 의도: 안전지대 신전/스킬 UI에서 Player의 패시브 슬롯을 교체할 수 있게 한다.
        // - 구현해야 할 것: slotIndex 검증은 EntityBase의 슬롯 설정 함수에 위임하고 PassiveSkill 참조를 저장한다.
        SetPassiveSkill(slotIndex, skill);
    }
}

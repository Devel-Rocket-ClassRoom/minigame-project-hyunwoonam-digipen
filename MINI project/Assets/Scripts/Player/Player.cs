using UnityEngine;

/// <summary>
/// 플레이어 캐릭터입니다.
/// </summary>
/// <remarks>
/// Week 1에서는 EntityBase의 임시 전투 수치와 기본 스킬만 보유합니다.
/// 행동 결정과 턴 진행은 CombatFlow가 담당합니다.
/// 스킬은 추후 안전지대에서 교체 가능합니다.
/// </remarks>
public class Player : EntityBase
{
    protected override void Awake()
    {
        // 기존 구현:
        // EnsureActiveSkill(
        //     0,
        //     new ActiveSkill
        //     {
        //         skillName = "강타",
        //         mpCost = 5,
        //         atkMultiplier = 1.5f,
        //     }
        // );
        // EnsureActiveSkill(
        //     1,
        //     new ActiveSkill
        //     {
        //         skillName = "맹공",
        //         mpCost = 10,
        //         atkMultiplier = 2.0f,
        //     }
        // );
        //
        // base.Awake();

        // TODO:
        // - 목표: Player가 전투 시작 전에 기본 액티브 스킬 슬롯을 보유하도록 초기화한다.
        // - 의도: 안전지대 스킬 장착 시스템이 없더라도 CombatFlow가 Skill1/Skill2를 사용할 수 있게 한다.
        // - 구현해야 할 것: 슬롯 0/1에 기본 ActiveSkill을 보장하고 EntityBase.Awake() 초기화를 호출한다.
        EnsureActiveSkill(
            0,
            new ActiveSkill
            {
                skillName = "강타",
                mpCost = 5,
                atkMultiplier = 1.5f,
            }
        );
        EnsureActiveSkill(
            1,
            new ActiveSkill
            {
                skillName = "맹공",
                mpCost = 10,
                atkMultiplier = 2.0f,
            }
        );

        base.Awake();
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

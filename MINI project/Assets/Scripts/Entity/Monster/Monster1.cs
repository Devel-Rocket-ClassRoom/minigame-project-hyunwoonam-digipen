using UnityEngine;

/// <summary>
/// 1단계 일반 몬스터입니다.
/// </summary>
/// <remarks>
/// Week 1에서는 EntityBase의 임시 전투 수치와 선택적 스킬 슬롯만 보유합니다.
/// 행동 선택과 턴 진행은 CombatFlow가 담당합니다.
/// 스킬 슬롯은 비어 있을 수 있으며, 비어 있으면 스킬 행동은 사용할 수 없습니다.
/// </remarks>
public class Monster1 : MonsterBase
{
    private void Reset()
    {
        // TODO:
        // - 목표: 새 Monster1 컴포넌트를 추가하거나 Reset할 때만 데모용 기본 전투 수치를 채운다.
        // - 의도: 런타임 Awake에서 HasStats로 인스펙터 값을 추측해 덮어쓰지 않게 한다.
        // - 구현해야 할 것: 스킬은 자동 장착하지 않고 maxHP/maxMP/atk/def만 데모 기본값으로 설정한다.
        ConfigureStats(110, 10, 7, 2);
    }

    /// <summary>
    /// 몬스터 액티브 슬롯 1 스킬입니다. 비어 있을 수 있습니다.
    /// </summary>
    public ActiveSkill Skill => GetActiveSkill(0);
}

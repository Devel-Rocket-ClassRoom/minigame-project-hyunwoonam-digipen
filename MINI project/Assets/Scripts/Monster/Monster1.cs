using UnityEngine;

/// <summary>
/// 1단계 일반 몬스터입니다.
/// </summary>
/// <remarks>
/// Week 1에서는 EntityBase의 임시 전투 수치와 단일 스킬만 보유합니다.
/// 행동 선택과 턴 진행은 CombatFlow가 담당합니다.
/// </remarks>
public class Monster1 : MonsterBase
{
    protected override void Awake()
    {
        // 기존 구현:
        // if (HasStats(150, 25, 11, 4))
        // {
        //     ConfigureStats(110, 10, 7, 2);
        // }
        //
        // EnsureActiveSkill(
        //     0,
        //     new ActiveSkill
        //     {
        //         skillName = "일격",
        //         mpCost = 0,
        //         atkMultiplier = 1.5f,
        //     }
        // );
        //
        // base.Awake();

        // TODO:
        // - 목표: Monster1의 기본 전투 수치와 기본 스킬을 초기화한다.
        // - 의도: 인스펙터 설정이 없을 때도 1단계 일반 몬스터가 데모 전투에 참여할 수 있게 한다.
        // - 구현해야 할 것: 기본 EntityBase 수치일 때 Monster1 수치로 교체하고, 슬롯 0에 기본 ActiveSkill을 보장한 뒤 base.Awake()를 호출한다.
    }

    /// <summary>
    /// 몬스터 기본 스킬입니다.
    /// </summary>
    public ActiveSkill Skill => GetActiveSkill(0);
}

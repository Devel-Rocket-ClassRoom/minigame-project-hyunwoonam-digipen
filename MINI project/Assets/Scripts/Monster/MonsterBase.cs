using UnityEngine;

/// <summary>
/// 몬스터 공통 base입니다. 기본 행동은 가중치로 결정합니다.
/// </summary>
public abstract class MonsterBase : EntityBase
{
    [Header("Action Weights")]
    [SerializeField]
    private int attackWeight = 70;

    [SerializeField]
    private int skillWeight = 20;

    [SerializeField]
    private int defendWeight = 10;

    public int AttackWeight => attackWeight;
    public int SkillWeight => skillWeight;
    public int DefendWeight => defendWeight;

    public override CombatActionType DecideAction()
    {
        // 기존 구현:
        // int totalWeight = Mathf.Max(1, attackWeight + skillWeight + defendWeight);
        // int roll = Random.Range(0, totalWeight);
        //
        // if (roll < attackWeight)
        // {
        //     return CombatActionType.Attack;
        // }
        //
        // if (roll < attackWeight + skillWeight)
        // {
        //     return CombatActionType.Skill;
        // }
        //
        // return CombatActionType.Defend;

        // TODO:
        // - 목표: 몬스터의 기본 행동을 Attack/Skill/Defend 가중치로 선택한다.
        // - 의도: Monster1 같은 단순 몬스터는 별도 FSM 없이 MonsterBase의 공통 의사결정을 재사용한다.
        // - 구현해야 할 것: 총 가중치를 계산하고 랜덤 롤을 통해 CombatActionType을 반환한다.
        int totalWeight = Mathf.Max(1, attackWeight + skillWeight + defendWeight);
        int roll = Random.Range(0, totalWeight);

        if (roll < attackWeight)
        {
            return CombatActionType.Attack;
        }

        if (roll < attackWeight + skillWeight)
        {
            return CombatActionType.Skill;
        }

        return CombatActionType.Defend;
    }
}

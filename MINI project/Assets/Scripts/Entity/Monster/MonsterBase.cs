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

    [Header("Turn Order")]
    [SerializeField]
    private int monsterGrade = 1;

    [Header("Death")]
    [Tooltip("사망 후 GameObject가 파괴되기까지 지연(초). 0이면 즉시 파괴됩니다.")]
    [SerializeField]
    private float deathDestroyDelay = 0.3f;

    private bool hasDied;

    public int AttackWeight => attackWeight;
    public int SkillWeight => skillWeight;
    public int DefendWeight => defendWeight;
    public int MonsterGrade => monsterGrade;
    public bool HasDied => hasDied;

    protected override void Awake()
    {
        // TODO:
        // - 목표: 몬스터가 Raycast 대상 선택에 맞을 수 있도록 2D Collider를 보장한다.
        // - 의도: CombatFlow의 Camera ScreenPointToRay + Physics2D raycast가 MonsterBase를 찾을 수 있게 한다.
        // - 구현해야 할 것: Collider2D가 없으면 BoxCollider2D를 추가하고 EntityBase 초기화를 이어서 호출한다.
        EnsureTargetCollider();
        base.Awake();
    }

    public virtual CombatActionType DecideAction()
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
        int effectiveSkillWeight = GetActiveSkill(0) == null ? 0 : skillWeight;
        int totalWeight = Mathf.Max(1, attackWeight + effectiveSkillWeight + defendWeight);
        int roll = Random.Range(0, totalWeight);

        if (roll < attackWeight)
        {
            return CombatActionType.Attack;
        }

        if (roll < attackWeight + effectiveSkillWeight)
        {
            return CombatActionType.Skill;
        }

        return CombatActionType.Defend;
    }

    public virtual void ExecuteAction(CombatActionType actionType, EntityBase target)
    {
        // TODO:
        // - 목표: 몬스터가 결정한 행동을 실행하는 확장 지점을 MonsterBase에 둔다.
        // - 의도: Player는 직접 입력으로 행동하므로 자동 의사결정/실행 API를 EntityBase가 아니라 MonsterBase 책임으로 제한한다.
        // - 구현해야 할 것: CombatAction 데이터 구조가 확정되면 actionType과 target 기반 실행을 CombatFlow와 연결한다.
        Debug.Log(
            $"[MonsterBase] ExecuteAction is TODO. actionType={actionType}, target={target?.name ?? "null"}"
        );
    }

    /// <summary>
    /// 사망 시 GameObject를 파괴하여 씬에서 제거합니다.
    /// </summary>
    /// <remarks>
    /// - CombatFlow.ApplyDamageToMonster에서 IsDead 판정 직후 호출됩니다.
    /// - deathDestroyDelay 만큼 지연 후 Destroy(gameObject)가 실행됩니다.
    /// - 같은 몬스터에 Die가 두 번 호출되어도 hasDied 가드로 중복 Destroy를 막습니다.
    /// - 사망 애니메이션/드롭/이펙트가 필요하면 파생 클래스에서 override 합니다.
    /// </remarks>
    public virtual void Die()
    {
        if (hasDied)
        {
            return;
        }

        hasDied = true;
        Debug.Log($"[MonsterBase] {name} Die. delay={deathDestroyDelay:0.00}s");

        float delay = Mathf.Max(0f, deathDestroyDelay);
        if (delay <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject, delay);
    }

    private void EnsureTargetCollider()
    {
        // TODO:
        // - 목표: 씬에 배치된 몬스터 오브젝트가 클릭 가능한 물리 타겟을 갖게 한다.
        // - 의도: 수동으로 Collider2D를 빠뜨려도 Week 1 데모에서 타겟 선택이 동작하게 한다.
        // - 구현해야 할 것: 기존 Collider2D가 없을 때 BoxCollider2D를 런타임에 추가한다.
        if (GetComponent<Collider2D>() != null)
        {
            return;
        }

        gameObject.AddComponent<BoxCollider2D>();
    }
}

using System;
using UnityEngine;

#pragma warning disable CS0649

/// <summary>
/// Player와 Monster가 공유하는 전투 엔티티 base입니다.
/// </summary>
public abstract class EntityBase : MonoBehaviour
{
    [SerializeField]
    protected int maxHP = 150;

    [SerializeField]
    protected int maxMP = 25;

    [SerializeField]
    protected int atk = 11;

    [SerializeField]
    protected int def = 4;

    [Header("Skill Slots")]
    [SerializeReference]
    private ActiveSkill[] activeSkills = new ActiveSkill[2];

    [SerializeReference]
    private PassiveSkill[] passiveSkills = new PassiveSkill[2];

    private int currentHP;
    private int currentMP;
    private bool isDefending;

    public int MaxHP => maxHP;
    public int MaxMP => maxMP;
    public int ATK => atk;
    public int DEF => def;
    public int CurrentHP => currentHP;
    public int CurrentMP => currentMP;
    public bool IsDefending => isDefending;
    public bool IsDead => currentHP <= 0;

    protected virtual void Awake()
    {
        // 기존 구현:
        // EnsureSkillSlots();
        // ResetForNewCombat();

        // TODO:
        // - 목표: 엔티티가 전투에 들어가기 전 스킬 슬롯과 런타임 상태를 초기화한다.
        // - 의도: Player와 Monster가 공통 초기화 흐름을 재사용하게 한다.
        // - 구현해야 할 것: 스킬 슬롯 배열을 보정하고 HP/MP/방어 상태를 새 전투 상태로 리셋한다.
    }

    public virtual void ResetForNewCombat()
    {
        // 기존 구현:
        // EnsureSkillSlots();
        // currentHP = maxHP;
        // currentMP = maxMP;
        // isDefending = false;

        // TODO:
        // - 목표: 전투 시작 또는 재시작 시 런타임 상태를 기본값으로 복원한다.
        // - 의도: 씬 재사용/전투 반복 시 이전 전투의 HP, MP, 방어 상태가 남지 않게 한다.
        // - 구현해야 할 것: 스킬 슬롯 배열을 보정하고 currentHP/currentMP/isDefending을 초기화한다.
    }

    public int TakeDamage(int damage)
    {
        // 기존 구현:
        // int appliedDamage = Mathf.Max(0, damage);
        // currentHP = Mathf.Max(0, currentHP - appliedDamage);
        // return appliedDamage;

        // TODO:
        // - 목표: 입력받은 피해량을 현재 HP에 적용하고 실제 적용 피해량을 반환한다.
        // - 의도: CombatFlow가 피해 처리와 로그 출력을 일관되게 수행할 수 있게 한다.
        // - 구현해야 할 것: 음수 피해를 0으로 보정하고 currentHP가 0 미만으로 내려가지 않도록 제한한다.
        return default;
    }

    public bool TrySpendMP(int amount)
    {
        // 기존 구현:
        // if (currentMP < amount)
        // {
        //     return false;
        // }
        //
        // currentMP -= amount;
        // return true;

        // TODO:
        // - 목표: MP가 충분할 때만 스킬 비용을 소비한다.
        // - 의도: CombatFlow가 스킬 사용 가능 여부를 EntityBase의 런타임 자원 상태로 판단하게 한다.
        // - 구현해야 할 것: currentMP와 amount를 비교하고 성공 시 currentMP를 차감한 뒤 true를 반환한다.
        return default;
    }

    public void SetDefending(bool value)
    {
        // 기존 구현:
        // isDefending = value;

        // TODO:
        // - 목표: 이번 턴 방어 상태를 설정하거나 해제한다.
        // - 의도: 데미지 계산 시 방어 중인지 CombatFlow가 확인할 수 있게 한다.
        // - 구현해야 할 것: isDefending 필드에 value를 저장한다.
    }

    public ActiveSkill GetActiveSkill(int slotIndex)
    {
        // 기존 구현:
        // EnsureSkillSlots();
        // return IsValidSlot(slotIndex, activeSkills.Length) ? activeSkills[slotIndex] : null;

        // TODO:
        // - 목표: 지정한 액티브 스킬 슬롯의 스킬을 반환한다.
        // - 의도: Player와 Monster가 공통 슬롯 구조로 액티브 스킬을 제공하게 한다.
        // - 구현해야 할 것: 슬롯 배열을 보정하고 slotIndex가 유효하면 해당 ActiveSkill을 반환한다.
        return default;
    }

    public PassiveSkill GetPassiveSkill(int slotIndex)
    {
        // 기존 구현:
        // EnsureSkillSlots();
        // return IsValidSlot(slotIndex, passiveSkills.Length) ? passiveSkills[slotIndex] : null;

        // TODO:
        // - 목표: 지정한 패시브 스킬 슬롯의 스킬을 반환한다.
        // - 의도: 추후 패시브 발동 시스템이 공통 슬롯 구조를 조회할 수 있게 한다.
        // - 구현해야 할 것: 슬롯 배열을 보정하고 slotIndex가 유효하면 해당 PassiveSkill을 반환한다.
        return default;
    }

    public virtual CombatActionType DecideAction()
    {
        // 기존 구현:
        // return CombatActionType.Attack;

        // TODO:
        // - 목표: 엔티티가 기본 행동을 결정한다.
        // - 의도: MonsterBase나 특수 몬스터가 행동 결정 로직을 override할 수 있는 확장 지점을 제공한다.
        // - 구현해야 할 것: 기본값은 Attack으로 두고, 실제 몬스터 의사결정은 하위 클래스에서 override한다.
        return default;
    }

    public virtual void ExecuteAction(CombatActionType actionType, EntityBase target)
    {
        // 기존 구현:
        // { }

        // TODO:
        // - 목표: 결정된 행동을 대상에게 실행하는 공통 확장 지점을 제공한다.
        // - 의도: CombatAction 데이터 구조가 확정된 뒤 DecideAction과 ExecuteAction을 분리한다.
        // - 구현해야 할 것: actionType과 target을 받아 공격/스킬/방어/아이템 실행을 하위 클래스 또는 전투 시스템과 연결한다.
    }

    protected void ConfigureStats(int newMaxHP, int newMaxMP, int newATK, int newDEF)
    {
        // 기존 구현:
        // maxHP = newMaxHP;
        // maxMP = newMaxMP;
        // atk = newATK;
        // def = newDEF;

        // TODO:
        // - 목표: 엔티티의 기본 전투 수치를 코드에서 일괄 설정한다.
        // - 의도: Monster1 같은 임시 데이터가 인스펙터 설정 없이도 기본값을 구성할 수 있게 한다.
        // - 구현해야 할 것: maxHP/maxMP/atk/def 필드에 인자로 받은 값을 저장한다.
    }

    protected bool HasStats(int expectedMaxHP, int expectedMaxMP, int expectedATK, int expectedDEF)
    {
        // 기존 구현:
        // return maxHP == expectedMaxHP
        //     && maxMP == expectedMaxMP
        //     && atk == expectedATK
        //     && def == expectedDEF;

        // TODO:
        // - 목표: 현재 전투 수치가 특정 기대값과 같은지 확인한다.
        // - 의도: 기본 EntityBase 수치인지 판별해 몬스터별 임시 기본값을 덮어쓸 수 있게 한다.
        // - 구현해야 할 것: maxHP/maxMP/atk/def를 기대값과 모두 비교한 결과를 반환한다.
        return default;
    }

    protected void SetActiveSkill(int slotIndex, ActiveSkill skill)
    {
        // 기존 구현:
        // EnsureSkillSlots();
        // if (IsValidSlot(slotIndex, activeSkills.Length))
        // {
        //     activeSkills[slotIndex] = skill;
        // }

        // TODO:
        // - 목표: 지정한 액티브 슬롯에 스킬을 저장한다.
        // - 의도: Player의 장착 변경과 몬스터 인스펙터/초기화 데이터를 같은 슬롯 구조로 처리한다.
        // - 구현해야 할 것: 슬롯 배열을 보정하고 유효한 slotIndex일 때 activeSkills에 skill을 대입한다.
    }

    protected void SetPassiveSkill(int slotIndex, PassiveSkill skill)
    {
        // 기존 구현:
        // EnsureSkillSlots();
        // if (IsValidSlot(slotIndex, passiveSkills.Length))
        // {
        //     passiveSkills[slotIndex] = skill;
        // }

        // TODO:
        // - 목표: 지정한 패시브 슬롯에 스킬을 저장한다.
        // - 의도: Player의 장착 변경과 추후 패시브 시스템을 같은 슬롯 구조로 처리한다.
        // - 구현해야 할 것: 슬롯 배열을 보정하고 유효한 slotIndex일 때 passiveSkills에 skill을 대입한다.
    }

    protected void EnsureActiveSkill(int slotIndex, ActiveSkill fallback)
    {
        // 기존 구현:
        // EnsureSkillSlots();
        // if (IsValidSlot(slotIndex, activeSkills.Length) && activeSkills[slotIndex] == null)
        // {
        //     activeSkills[slotIndex] = fallback;
        // }

        // TODO:
        // - 목표: 지정한 액티브 슬롯이 비어 있을 때 기본 스킬을 채운다.
        // - 의도: 인스펙터나 데이터 로딩이 없는 데모 상태에서도 필수 스킬 슬롯을 확보한다.
        // - 구현해야 할 것: 슬롯 배열을 보정하고 유효한 빈 슬롯에 fallback을 저장한다.
    }

    protected void EnsurePassiveSkill(int slotIndex, PassiveSkill fallback)
    {
        // 기존 구현:
        // EnsureSkillSlots();
        // if (IsValidSlot(slotIndex, passiveSkills.Length) && passiveSkills[slotIndex] == null)
        // {
        //     passiveSkills[slotIndex] = fallback;
        // }

        // TODO:
        // - 목표: 지정한 패시브 슬롯이 비어 있을 때 기본 패시브를 채운다.
        // - 의도: 추후 직업/룬/스킬 시스템이 기본 패시브 슬롯을 안전하게 보장할 수 있게 한다.
        // - 구현해야 할 것: 슬롯 배열을 보정하고 유효한 빈 슬롯에 fallback을 저장한다.
    }

    private void EnsureSkillSlots()
    {
        // 기존 구현:
        // if (activeSkills == null)
        // {
        //     activeSkills = new ActiveSkill[2];
        // }
        // else if (activeSkills.Length != 2)
        // {
        //     Array.Resize(ref activeSkills, 2);
        // }
        //
        // if (passiveSkills == null)
        // {
        //     passiveSkills = new PassiveSkill[2];
        // }
        // else if (passiveSkills.Length != 2)
        // {
        //     Array.Resize(ref passiveSkills, 2);
        // }

        // TODO:
        // - 목표: 액티브/패시브 스킬 슬롯 배열을 각각 2칸으로 보정한다.
        // - 의도: Player와 Monster가 Active 2 + Passive 2 슬롯 정책을 항상 만족하게 한다.
        // - 구현해야 할 것: null 배열은 새로 만들고 길이가 다르면 Array.Resize로 2칸에 맞춘다.
    }

    private static bool IsValidSlot(int slotIndex, int length)
    {
        // 기존 구현:
        // return slotIndex >= 0 && slotIndex < length;

        // TODO:
        // - 목표: 슬롯 인덱스가 배열 범위 안에 있는지 판단한다.
        // - 의도: 슬롯 접근 함수들이 중복된 범위 검사를 직접 구현하지 않게 한다.
        // - 구현해야 할 것: slotIndex가 0 이상이고 length보다 작은지 반환한다.
        return default;
    }
}

public enum CombatActionType
{
    Attack,
    Skill,
    Defend,
    Item,
}

#pragma warning restore CS0649

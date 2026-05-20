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

    protected virtual void OnValidate()
    {
        // TODO:
        // - 목표: Inspector에서 전투 수치를 수정해도 런타임 HP가 최대 HP를 넘지 않게 보정한다.
        // - 의도: Play Mode 중 maxHP를 낮췄을 때 currentHP가 maxHP보다 커지는 상태 불일치를 막는다.
        // - 구현해야 할 것: maxHP 최소값 보장, maxMP 최소값 보장, currentHP를 maxHP 이하로 제한한다.
        maxHP = Mathf.Max(1, maxHP);
        maxMP = Mathf.Max(0, maxMP);
        ClampCurrentResourcesToMax();
    }

    protected virtual void Awake()
    {
        // 기존 구현:
        // EnsureSkillSlots();
        // ResetForNewCombat();

        // TODO:
        // - 목표: 엔티티가 전투에 들어가기 전 스킬 슬롯과 런타임 상태를 초기화한다.
        // - 의도: Player와 Monster가 공통 초기화 흐름을 재사용하게 한다.
        // - 구현해야 할 것: 스킬 슬롯 배열을 보정하고 HP/MP/방어 상태를 새 전투 상태로 리셋한다.
        RestoreToFull();
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
        RestoreToFull();
    }

    public virtual void PrepareForCombat()
    {
        // TODO:
        // - 목표: 전투 진입 시 지속되어야 하는 HP/MP는 유지하고 전투 중 상태만 정리한다.
        // - 의도: Player처럼 전투 사이 HP를 이어가야 하는 엔티티가 매 전투마다 최대 HP로 회복되지 않게 한다.
        // - 구현해야 할 것: 스킬 슬롯과 수치 범위를 보정하고 방어 상태만 해제한다.
        EnsureSkillSlots();
        maxHP = Mathf.Max(1, maxHP);
        maxMP = Mathf.Max(0, maxMP);
        ClampCurrentResourcesToMax();
        isDefending = false;
    }

    public void SetCurrentResources(int hp, int mp)
    {
        // TODO:
        // - 목표: GameSystemManager/SaveLoader가 보관한 런타임 HP/MP를 엔티티에 복원한다.
        // - 의도: 씬이 바뀌어도 Player의 현재 체력과 MP가 다음 전투로 이어지게 한다.
        // - 구현해야 할 것: 입력값을 현재 maxHP/maxMP 범위로 보정해 저장한다.
        currentHP = Mathf.Clamp(hp, 0, Mathf.Max(1, maxHP));
        currentMP = Mathf.Clamp(mp, 0, Mathf.Max(0, maxMP));
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
        ClampCurrentResourcesToMax();
        int appliedDamage = Mathf.Max(0, damage);
        currentHP = Mathf.Max(0, currentHP - appliedDamage);
        ClampCurrentResourcesToMax();
        return appliedDamage;
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
        int spendAmount = Mathf.Max(0, amount);
        if (currentMP < spendAmount)
        {
            return false;
        }

        currentMP -= spendAmount;
        return true;
    }

    public void SetDefending(bool value)
    {
        // 기존 구현:
        // isDefending = value;

        // TODO:
        // - 목표: 이번 턴 방어 상태를 설정하거나 해제한다.
        // - 의도: 데미지 계산 시 방어 중인지 CombatFlow가 확인할 수 있게 한다.
        // - 구현해야 할 것: isDefending 필드에 value를 저장한다.
        isDefending = value;
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
        EnsureSkillSlots();
        return IsValidSlot(slotIndex, activeSkills.Length) ? activeSkills[slotIndex] : null;
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
        EnsureSkillSlots();
        return IsValidSlot(slotIndex, passiveSkills.Length) ? passiveSkills[slotIndex] : null;
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
        maxHP = newMaxHP;
        maxMP = newMaxMP;
        atk = newATK;
        def = newDEF;
        maxHP = Mathf.Max(1, maxHP);
        maxMP = Mathf.Max(0, maxMP);
        ClampCurrentResourcesToMax();
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
        EnsureSkillSlots();
        if (IsValidSlot(slotIndex, activeSkills.Length))
        {
            activeSkills[slotIndex] = skill;
        }
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
        EnsureSkillSlots();
        if (IsValidSlot(slotIndex, passiveSkills.Length))
        {
            passiveSkills[slotIndex] = skill;
        }
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
        if (activeSkills == null)
        {
            activeSkills = new ActiveSkill[2];
        }
        else if (activeSkills.Length != 2)
        {
            Array.Resize(ref activeSkills, 2);
        }

        if (passiveSkills == null)
        {
            passiveSkills = new PassiveSkill[2];
        }
        else if (passiveSkills.Length != 2)
        {
            Array.Resize(ref passiveSkills, 2);
        }
    }

    private static bool IsValidSlot(int slotIndex, int length)
    {
        // 기존 구현:
        // return slotIndex >= 0 && slotIndex < length;

        // TODO:
        // - 목표: 슬롯 인덱스가 배열 범위 안에 있는지 판단한다.
        // - 의도: 슬롯 접근 함수들이 중복된 범위 검사를 직접 구현하지 않게 한다.
        // - 구현해야 할 것: slotIndex가 0 이상이고 length보다 작은지 반환한다.
        return slotIndex >= 0 && slotIndex < length;
    }

    private void RestoreToFull()
    {
        // TODO:
        // - 목표: 새 엔티티 생성 또는 몬스터 재등장처럼 완전 회복이 필요한 경우 HP/MP를 최대치로 초기화한다.
        // - 의도: 전투 사이 체력을 유지해야 하는 Player는 PrepareForCombat/override를 사용하고, 새 전투마다 새 상태가 필요한 Monster는 이 흐름을 사용한다.
        // - 구현해야 할 것: 슬롯/수치 보정 후 currentHP/currentMP/isDefending을 초기 상태로 설정한다.
        EnsureSkillSlots();
        maxHP = Mathf.Max(1, maxHP);
        maxMP = Mathf.Max(0, maxMP);
        currentHP = maxHP;
        currentMP = maxMP;
        isDefending = false;
    }

    private void ClampCurrentResourcesToMax()
    {
        // TODO:
        // - 목표: currentHP/currentMP가 항상 각 최대치 범위 안에 머물도록 보정한다.
        // - 의도: Inspector 수치 변경, 코드 수치 재설정, 피해 처리 이후에도 HP/MP 표현과 판정이 일관되게 한다.
        // - 구현해야 할 것: currentHP를 0..maxHP, currentMP를 0..maxMP 범위로 제한한다.
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        currentMP = Mathf.Clamp(currentMP, 0, maxMP);
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

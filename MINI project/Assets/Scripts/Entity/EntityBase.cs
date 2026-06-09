using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 전투 엔티티(Player/Companion/Monster)의 공통 베이스.
/// 보유: 스탯, 액티브 스킬 2칸, 패시브 스킬 목록(자동 적용), 공격/방어/피해 처리, 머리 위 UI 훅.
/// 패시브 스킬은 CharacterBase.SyncPassivesFromRunes()가 룬 해금 시 자동으로 채우며
/// 플레이어/동료가 직접 선택하거나 제거할 수 없다.
/// </summary>
public abstract class EntityBase : MonoBehaviour
{
    /// <summary>스탯 블록.</summary>
    public StatBlock Stats { get; protected set; }

    /// <summary>액티브 스킬 2칸.</summary>
    public Skill[] ActiveSkills { get; protected set; } = new Skill[2];

    /// <summary>
    /// 패시브 스킬 목록.
    /// 룬 해금으로만 추가되며, 전투 시작/레벨업 시 SyncPassivesFromRunes()가 갱신한다.
    /// Monster는 이 목록을 사용하지 않는다(몬스터 스킬은 모두 ActionWeights 기반 액티브).
    /// </summary>
    public IReadOnlyList<Skill> PassiveSkills => passiveSkills;

    private List<Skill> passiveSkills = new List<Skill>();

    private static readonly PassiveEffectBinding[] PassiveEffectBindings =
    {
        new PassiveEffectBinding(skill => skill.DamageScale, stats => stats.BaseATK, StatType.ATK),
        new PassiveEffectBinding(skill => skill.HealScale, stats => stats.BaseMaxHP, StatType.HP),
        new PassiveEffectBinding(skill => skill.ShieldScale, stats => stats.BaseDEF, StatType.DEF),
    };

    /// <summary>이번 라운드 동안 방어 중인가.</summary>
    public bool IsDefending { get; private set; }

    /// <summary>현재 보호막 잔량.</summary>
    public int ShieldAmount { get; private set; }

    /// <summary>보호막 남은 라운드.</summary>
    public int ShieldRoundsRemaining { get; private set; }

    /// <summary>사망 여부.</summary>
    public bool IsDead => Stats != null && Stats.CurrentHP <= 0;

    /// <summary>표시 이름(언어 키 또는 직접 문자열).</summary>
    public string DisplayName;

    /// <summary>머리 위 UI 핸들.</summary>
    public EntityWorldUI WorldUI;

    /// <summary>SPUM 애니메이션 어댑터(전투 진입 시 자식 SPUM_Prefabs에서 1회 해석, 없으면 null).</summary>
    private CombatUnitAnimator animator;

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
        // - 패시브 효과는 전투 시작 전 SyncPassivesFromRunes()가 이미 갱신했으므로
        //   여기서 ApplyAllPassiveEffects()를 호출해 스탯에 반영한다.
        if (Stats == null)
        {
            Stats = new StatBlock();
            Stats.SetBaseStats(1, 0, 0, 0, 0);
        }

        Stats.ClampToMax();
        IsDefending = false;
        ClearShield();
        if (ActiveSkills == null || ActiveSkills.Length != 2)
        {
            ActiveSkills = new Skill[2];
        }

        WorldUI?.HideActionIcon();
        ApplyAllPassiveEffects();

        // SPUM 애니메이션 어댑터 해석 + 초기화(IDLE). 자식에 SPUM_Prefabs 없으면 null → no-op.
        animator = CombatUnitAnimator.TryCreate(gameObject);
        animator?.Initialize();
    }

    /// <summary>공격/스킬 실행 시점에 ATTACK 애니메이션 재생. CombatFlow가 호출.</summary>
    public void PlayAttackAnimation(int clipIndex)
    {
        animator?.PlayAttack(clipIndex);
    }

    /// <summary>
    /// 라운드 종료 시 호출. 방어 해제 등 정리.
    /// </summary>
    public virtual void OnRoundEnd()
    {
        IsDefending = false;
        TickShieldDuration();
        WorldUI?.HideActionIcon();
        animator?.PlayIdle();
    }

    /// <summary>
    /// 데미지 적용. DamageCalculatort에서 호출.
    /// </summary>
    public int ApplyDamage(int rawDamage)
    {
        if (Stats == null)
        {
            return 0;
        }

        int remainingDamage = Mathf.Max(0, rawDamage);
        if (ShieldAmount > 0 && remainingDamage > 0)
        {
            int absorbed = Mathf.Min(ShieldAmount, remainingDamage);
            ShieldAmount -= absorbed;
            remainingDamage -= absorbed;
            if (ShieldAmount <= 0)
            {
                ShieldAmount = 0;
                ShieldRoundsRemaining = 0;
                WorldUI?.HideShieldFx();
            }
            else
            {
                WorldUI?.ShowShieldFx(ShieldAmount);
            }
        }

        int actualDamage = Stats.TakeDamage(remainingDamage);
        bool guarded = IsDefending;
        if (guarded)
        {
            WorldUI?.PlayGuardHitFx();
        }
        else
        {
            WorldUI?.PlayHitFx();
        }

        SpawnFloatingDamage(actualDamage, guarded);

        // 피격/사망 애니메이션. 사망 시 DEATH(1회), 아니면 DAMAGED.
        if (IsDead)
        {
            animator?.PlayDeath();
        }
        else
        {
            animator?.PlayDamaged();
        }

        return actualDamage;
    }

    private void SpawnFloatingDamage(int amount, bool guarded)
    {
        if (amount <= 0)
        {
            return;
        }

        Vector3 position = WorldUI != null
            ? WorldUI.EffectAnchorPosition
            : transform.position + new Vector3(0f, 0.9f, 0f);
        CombatFloatingText.SpawnDamage(position, amount, guarded);
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
        if (Stats == null || rawHeal <= 0)
        {
            return 0;
        }

        int before = Stats.CurrentHP;
        Stats.CurrentHP = Mathf.Min(Stats.CurrentHP + rawHeal, Stats.MaxHP);
        int actualHeal = Stats.CurrentHP - before;
        WorldUI?.PlayHealFx();
        if (actualHeal > 0)
        {
            Vector3 position = WorldUI != null
                ? WorldUI.EffectAnchorPosition
                : transform.position + new Vector3(0f, 0.9f, 0f);
            CombatFloatingText.SpawnHeal(position, actualHeal);
        }

        return actualHeal;
    }

    /// <summary>
    /// 보호막 적용(이번 라운드 한정 또는 일정 시간).
    /// </summary>
    public void ApplyShield(int amount, int durationRounds)
    {
        if (amount <= 0)
        {
            return;
        }

        ShieldAmount += amount;
        ShieldRoundsRemaining = Mathf.Max(ShieldRoundsRemaining, Mathf.Max(1, durationRounds));
        WorldUI?.ShowShieldFx(ShieldAmount);
    }

    private void TickShieldDuration()
    {
        if (ShieldRoundsRemaining <= 0)
        {
            return;
        }

        ShieldRoundsRemaining--;
        if (ShieldRoundsRemaining <= 0)
        {
            ClearShield();
        }
    }

    private void ClearShield()
    {
        ShieldAmount = 0;
        ShieldRoundsRemaining = 0;
        WorldUI?.HideShieldFx();
    }

    /// <summary>
    /// 이번 턴 방어 상태 설정.
    /// </summary>
    public void SetDefending(bool value)
    {
        // 동작 요약: IsDefending = value; 머리 위 파랑 아이콘 표시/해제.
        IsDefending = value;
        if (value)
        {
            WorldUI?.ShowDefendIcon();
        }
        else
        {
            WorldUI?.HideActionIcon();
        }
    }

    /// <summary>
    /// 액티브 스킬 슬롯 조회.
    /// </summary>
    public Skill GetActiveSkill(int slotIndex)
    {
        // 동작 요약: 범위 검사 후 ActiveSkills[slotIndex] 반환.
        return ActiveSkills != null && slotIndex >= 0 && slotIndex < ActiveSkills.Length ? ActiveSkills[slotIndex] : null;
    }

    /// <summary>
    /// 액티브 스킬 슬롯 설정. 길드/룬 변경 시 호출.
    /// </summary>
    public void SetActiveSkill(int slotIndex, Skill skill)
    {
        // 동작 요약: 범위 검사 후 ActiveSkills[slotIndex] = skill.
        if (ActiveSkills == null || ActiveSkills.Length != 2)
        {
            ActiveSkills = new Skill[2];
        }

        if (slotIndex >= 0 && slotIndex < ActiveSkills.Length)
        {
            ActiveSkills[slotIndex] = skill;
        }
    }

    /// <summary>
    /// 패시브 스킬 추가. CharacterBase.SyncPassivesFromRunes()만 호출해야 한다.
    /// 외부에서 직접 호출 금지.
    /// </summary>
    internal void AddPassiveSkill(Skill skill)
    {
        // 동작 요약:
        // - skill.Data.SkillType == Passive 검증.
        // - passiveSkills에 중복 없이 추가.
        if (skill?.Data == null || skill.Data.SkillType != SkillType.Passive)
        {
            return;
        }

        if (!passiveSkills.Exists(s => s.Data != null && s.Data.Id == skill.Data.Id))
        {
            passiveSkills.Add(skill);
        }
    }

    /// <summary>
    /// 패시브 스킬 목록 초기화. SyncPassivesFromRunes() 호출 전 선행 처리.
    /// </summary>
    internal void ClearPassiveSkills()
    {
        // 동작 요약: passiveSkills.Clear().
        passiveSkills.Clear();
    }

    /// <summary>
    /// 현재 passiveSkills 목록에 등록된 모든 패시브 효과를 Stats.PassiveBonus에 적용.
    /// PrepareForCombat 시 1회 호출. 중복 적용 방지를 위해 ResetPassiveBonuses() 후 재적용.
    /// </summary>
    protected void ApplyAllPassiveEffects()
    {
        // 동작 요약:
        // - Stats.ResetPassiveBonuses().
        // - passiveSkills 순회: 각 Skillt의 Data.EffectFormula / EffectValue 기준으로 스탯 보정 적용.
        // - StatBlockt는 기본 스탯 + 장비 보정 + 룬 보정 + 패시브 보정을 합산해 최종 스탯을 갱신한다.
        if (Stats == null)
        {
            return;
        }

        Stats.ResetPassiveBonuses();
        foreach (Skill passive in passiveSkills)
        {
            SkillData data = passive?.Data;
            if (data == null)
            {
                continue;
            }

            ApplyPassiveEffect(data);
        }
    }

    private void ApplyPassiveEffect(SkillData data)
    {
        if (data.PassiveStatType != PassiveStatType.None)
        {
            ApplyExplicitPassiveEffect(data);
            return;
        }

        for (int i = 0; i < PassiveEffectBindings.Length; i++)
        {
            PassiveEffectBindings[i].Apply(Stats, data);
        }
    }

    private void ApplyExplicitPassiveEffect(SkillData data)
    {
        if (!TryMapPassiveStat(data.PassiveStatType, out StatType statType))
        {
            return;
        }

        int baseValue = BaseValueForPassive(data.PassiveStatType);
        int amount = data.PassiveFlatValue + Mathf.RoundToInt(baseValue * data.PassivePercentValue);
        if (amount != 0)
        {
            Stats.ApplyPassiveBonus(statType, amount);
        }
    }

    private int BaseValueForPassive(PassiveStatType statType)
    {
        switch (statType)
        {
            case PassiveStatType.HP:
                return Stats.BaseMaxHP;
            case PassiveStatType.MP:
                return Stats.BaseMaxMP;
            case PassiveStatType.ATK:
                return Stats.BaseATK;
            case PassiveStatType.DEF:
                return Stats.BaseDEF;
            case PassiveStatType.SPD:
                return Stats.BaseSPD;
            default:
                return 0;
        }
    }

    private static bool TryMapPassiveStat(PassiveStatType passiveStatType, out StatType statType)
    {
        switch (passiveStatType)
        {
            case PassiveStatType.HP:
                statType = StatType.HP;
                return true;
            case PassiveStatType.MP:
                statType = StatType.MP;
                return true;
            case PassiveStatType.ATK:
                statType = StatType.ATK;
                return true;
            case PassiveStatType.DEF:
                statType = StatType.DEF;
                return true;
            case PassiveStatType.SPD:
                statType = StatType.SPD;
                return true;
            default:
                statType = StatType.HP;
                return false;
        }
    }

    private sealed class PassiveEffectBinding
    {
        private readonly System.Func<SkillData, float> scaleGetter;
        private readonly System.Func<StatBlock, int> baseValueGetter;
        private readonly StatType statType;

        public PassiveEffectBinding(System.Func<SkillData, float> scaleGetter, System.Func<StatBlock, int> baseValueGetter, StatType statType)
        {
            this.scaleGetter = scaleGetter;
            this.baseValueGetter = baseValueGetter;
            this.statType = statType;
        }

        public void Apply(StatBlock stats, SkillData data)
        {
            float scale = scaleGetter(data);
            if (scale <= 0f)
            {
                return;
            }

            stats.ApplyPassiveBonus(statType, Mathf.RoundToInt(baseValueGetter(stats) * scale));
        }
    }
}


/// <summary>
/// 동료 베이스. 레벨업 시 현재 투자 가능한 룬 중 하나를 자동 선택한다.
/// 행동 결정은 직업별 고정 우선순위 규칙(CompanionActionSelector)을 사용.
/// </summary>
public abstract class TeamBase : CharacterBase
{
    private int runeSeed;

    /// <summary>동료 데이터 ID(CompanionData 참조).</summary>
    public int CompanionDataId;

    /// <summary>고정 룬 트리 상태.</summary>
    public CompanionRuneState Rune;

    /// <summary>장비 슬롯(동료도 장비 가능).</summary>
    public EquipmentSlots Equipment;

    /// <summary>행동 규칙 키(직업별 우선순위 식별자).</summary>
    public string ActionRuleKey;

    /// <summary>
    /// 모집 직후 1회 호출. 트리 무작위 생성 + 시작 룬 해금.
    /// </summary>
    public void Initialize(int companionDataId, int seed)
    {
        // 동작 요약:
        // - CompanionDataId = companionDataId.
        // - Rune = RuneTreeGenerator.GenerateFixedTree(data, seed).
        // - Rune.UnlockStarter().
        // - 직업별 ActionRuleKey 설정.
        // - 기본 스탯 설정.
        CompanionDataId = companionDataId;
        runeSeed = seed;
        Equipment = new EquipmentSlots();
        Stats = new StatBlock();
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Data.Companions.TryGetValue(companionDataId, out CompanionData data))
        {
            ActionRuleKey = data.ActionRuleKey;
            EquipmentStatMod baseStats = data.BaseStats ?? new EquipmentStatMod { HP = 70, MP = 10, ATK = 8, DEF = 2, SPD = 9 };
            Stats.SetBaseStats(baseStats.HP, baseStats.MP, baseStats.ATK, baseStats.DEF, baseStats.SPD);
            Rune = new CompanionRuneState { ClassId = data.ClassId, Tree = RuneTree.BuildFromData(data.ClassId, gsm.Data.Runes.Values), FixedSequence = new System.Collections.Generic.List<int>(), UnlockedCount = 0 };
        }
        else
        {
            ActionRuleKey = "Dealer";
            Stats.SetBaseStats(70, 10, 8, 2, 9);
            Rune = new CompanionRuneState { ClassId = RuneClass.Dealer, FixedSequence = new System.Collections.Generic.List<int>() };
        }

        Stats.RestoreToFull();
        Rune.UnlockStarter();
    }

    /// <inheritdoc/>
    protected override void OnLeveledUp()
    {
        if (Stats != null)
        {
            Stats.BaseMaxHP += 6;
            Stats.BaseATK += 1;
            Stats.BaseDEF += 1;
            Stats.BaseSPD += 1;
            Stats.RecalculateFinalStats();
        }

        if (Rune != null)
        {
            int seed = unchecked(runeSeed * 397 ^ Level * 31 ^ Rune.UnlockedCount);
            Rune.InvestRandomAvailable(new System.Random(seed));
        }
    }

    /// <inheritdoc/>
    protected override System.Collections.Generic.IReadOnlyList<int> GetUnlockedRuneNodeIds()
    {
        var result = new System.Collections.Generic.List<int>();
        if (Rune?.Tree?.AllNodes == null)
        {
            return result;
        }

        Rune.SyncTreeStateFromProgress();
        foreach (RuneNode node in Rune.Tree.AllNodes.Values)
        {
            if (node?.Data != null && node.Unlocked)
            {
                result.Add(node.Data.Id);
            }
        }

        return result;
    }
}

/// <summary>
/// 전투 진입 시 런타임으로 생성되는 기본 동료 엔티티.
/// 직업별 특수 처리는 데이터와 CompanionActionSelector가 담당한다.
/// </summary>
public sealed class CombatCompanion : TeamBase
{
    public void BindState(CompanionInstance state)
    {
        if (state == null)
        {
            return;
        }

        Initialize(state.CompanionDataId, state.Seed);
        Level = state.Level <= 0 ? 1 : state.Level;
        CurrentExp = state.Exp;
        Stats = state.Stats ?? Stats;
        state.Stats = Stats;
        Rune = state.Rune ?? Rune;
        Equipment = state.Equipment ?? Equipment ?? new EquipmentSlots();

        Stats.ResetEquipmentBonuses();
        Stats.ApplyEquipmentBonus(Equipment.AggregateStatMod());
        Stats.ResetRuneBonuses();
        EquipmentStatMod runeMod = Rune != null ? Rune.AggregateStatMod() : new EquipmentStatMod();
        Stats.ApplyRuneBonus(StatType.HP, runeMod.HP);
        Stats.ApplyRuneBonus(StatType.MP, runeMod.MP);
        Stats.ApplyRuneBonus(StatType.ATK, runeMod.ATK);
        Stats.ApplyRuneBonus(StatType.DEF, runeMod.DEF);
        Stats.ApplyRuneBonus(StatType.SPD, runeMod.SPD);

        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
        {
            SyncPassivesFromRunes(gsm.Data);
            BindStartingSkills(gsm.Data);
        }

        DisplayName = "Companion " + state.CompanionDataId;
    }

    private void BindStartingSkills(DataManager data)
    {
        if (data?.Skills == null)
        {
            return;
        }

        int[] skillIds = data.GetStartingSkillIds(Rune != null ? Rune.ClassId : RuneClass.Dealer);
        BindStartingSkillSlot(data, 0, ResolveStartingSkillId(skillIds, 0, GetFallbackCompanionSkillId(data, 0)));
        BindStartingSkillSlot(data, 1, ResolveStartingSkillId(skillIds, 1, GetFallbackCompanionSkillId(data, 1)));
    }

    private void BindStartingSkillSlot(DataManager data, int slotIndex, int skillId)
    {
        if (skillId != 0 && data.Skills.TryGetValue(skillId, out SkillData skill))
        {
            SetActiveSkill(slotIndex, new Skill(skill));
        }
    }

    private static int ResolveStartingSkillId(int[] skillIds, int index, int fallback)
    {
        return skillIds != null && index < skillIds.Length && skillIds[index] != 0 ? skillIds[index] : fallback;
    }

    private static int GetFallbackCompanionSkillId(DataManager data, int slotIndex)
    {
        RuneClass fallbackClass = slotIndex == 0 ? RuneClass.Dealer : RuneClass.MagicDealer;
        int[] fallbackSkills = data.GetStartingSkillIds(fallbackClass);
        return fallbackSkills != null && fallbackSkills.Length > 0 ? fallbackSkills[0] : 0;
    }
}


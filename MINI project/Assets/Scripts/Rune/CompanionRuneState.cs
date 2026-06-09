using System.Collections.Generic;

/// <summary>
/// 동료 룬 상태. 레벨업할 때마다 현재 투자 가능한 노드 중 하나에 자동 투자한다.
/// </summary>
public sealed class CompanionRuneState
{
    /// <summary>직업.</summary>
    public RuneClass ClassId;

    /// <summary>고정 트리.</summary>
    public RuneTree Tree;

    /// <summary>자동 투자 이력(시작 룬 포함, 같은 노드 중복 가능).</summary>
    public List<int> FixedSequence;

    /// <summary>현재까지 적용된 투자 이력 개수.</summary>
    public int UnlockedCount;

    /// <summary>
    /// 시작 룬 1개 해금.
    /// </summary>
    public void UnlockStarter()
    {
        if (Tree?.Starter?.Data == null)
        {
            return;
        }

        if (FixedSequence == null)
        {
            FixedSequence = new List<int>();
        }

        int starterId = Tree.Starter.Data.Id;
        if (FixedSequence.Count == 0)
        {
            FixedSequence.Add(starterId);
        }

        Tree.Starter.Unlocked = true;
        UnlockedCount = System.Math.Max(1, System.Math.Min(UnlockedCount, FixedSequence.Count));
    }

    /// <summary>
    /// 현재 투자 가능한 노드 중 하나에 1포인트 자동 투자한다.
    /// </summary>
    public bool InvestRandomAvailable(System.Random random)
    {
        if (random == null || Tree?.AllNodes == null)
        {
            return false;
        }

        UnlockStarter();
        SyncTreeStateFromProgress();

        var candidates = new List<RuneNode>();
        foreach (RuneNode node in Tree.AllNodes.Values)
        {
            if (
                node?.Data != null
                && node.RequiredPoints > 0
                && node.InvestedPoints < node.RequiredPoints
                && Tree.CanUnlock(node)
            )
            {
                candidates.Add(node);
            }
        }

        if (candidates.Count == 0)
        {
            return false;
        }

        candidates.Sort((left, right) => left.Data.Id.CompareTo(right.Data.Id));
        RuneNode selected = candidates[random.Next(candidates.Count)];
        if (UnlockedCount < FixedSequence.Count)
        {
            FixedSequence.RemoveRange(UnlockedCount, FixedSequence.Count - UnlockedCount);
        }

        FixedSequence.Add(selected.Data.Id);
        UnlockedCount = FixedSequence.Count;
        SyncTreeStateFromProgress();
        return true;
    }

    public void SyncTreeStateFromProgress()
    {
        if (Tree?.AllNodes == null)
        {
            return;
        }

        if (FixedSequence == null)
        {
            FixedSequence = new List<int>();
        }

        foreach (RuneNode node in Tree.AllNodes.Values)
        {
            node.InvestedPoints = 0;
            node.Unlocked = false;
        }

        int count = System.Math.Min(UnlockedCount, FixedSequence.Count);
        UnlockedCount = count;
        for (int i = 0; i < count; i++)
        {
            if (!Tree.AllNodes.TryGetValue(FixedSequence[i], out RuneNode node))
            {
                continue;
            }

            if (node.RequiredPoints <= 0)
            {
                node.Unlocked = node.Data.RequiredRuneId == 0;
                continue;
            }

            node.InvestedPoints = System.Math.Min(node.RequiredPoints, node.InvestedPoints + 1);
            node.Unlocked = node.InvestedPoints >= node.RequiredPoints;
        }

        if (Tree.Starter != null)
        {
            Tree.Starter.Unlocked = true;
        }
    }

    /// <summary>
    /// 해금된 룬 노드의 스탯 보정 합산.
    /// UnlockSkill 타입 노드는 제외(패시브 스킬은 SyncPassivesFromRunes 경로).
    /// </summary>
    public EquipmentStatMod AggregateStatMod()
    {
        // 동작 요약:
        // - FixedSequence 앞 UnlockedCount개 순회 → Tree.AllNodes[id].Data 조회.
        // - EffectType 분기:
        //   AddMaxHP/AddMaxMP/AddATK/AddDEF/AddSPD → EffectValue 합산.
        //   UnlockSkill / DamageBoost / HealBoost → 스킵.
        // - 합산 결과 EquipmentStatMod 반환.
        var result = new EquipmentStatMod();
        if (FixedSequence == null || Tree?.AllNodes == null)
        {
            return result;
        }

        SyncTreeStateFromProgress();
        foreach (RuneNode node in Tree.AllNodes.Values)
        {
            if (node?.Data == null)
            {
                continue;
            }

            float ratio =
                node.RequiredPoints > 0
                    ? node.InvestedPoints / (float)node.RequiredPoints
                    : node.Unlocked
                        ? 1f
                        : 0f;
            int value = UnityEngine.Mathf.RoundToInt(node.Data.EffectValue * ratio);
            switch (node.Data.EffectType)
            {
                case RuneEffectType.AddMaxHP: result.HP += value; break;
                case RuneEffectType.AddMaxMP: result.MP += value; break;
                case RuneEffectType.AddATK: result.ATK += value; break;
                case RuneEffectType.AddDEF: result.DEF += value; break;
                case RuneEffectType.AddSPD: result.SPD += value; break;
            }
        }

        return result;
    }
}


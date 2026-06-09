public static class RunProgression
{
    private const int CompanionSeedMultiplier = 397;
    private const int CompanionLevelMultiplier = 31;

    public static void AddExpToPlayer(
        GameRunState run,
        DataManager data,
        EventBus events,
        int amount
    )
    {
        if (run?.Player == null || amount <= 0)
        {
            return;
        }

        run.Player.Exp += amount;

        int required = RequiredExpForLevel(data, run.Player.Level);

        while (required > 0 && run.Player.Exp >= required)
        {
            run.Player.Exp -= required;
            run.Player.Level += 1;
            run.Player.Rune?.AddRunePoint(data?.Balance?.RunePointPerLevel ?? 1);

            if (run.Player.Rune != null)
            {
                events?.RaiseRunePointsChanged(run.Player.Rune.RunePoints);
            }

            GrowPlayerStats(run.Player);

            events?.RaisePlayerLevelUp(run.Player.Level);
            required = RequiredExpForLevel(data, run.Player.Level);
        }

        events?.RaisePlayerExpChanged(run.Player.Exp, required);
    }

    public static void AddExpToActiveCompanions(GameRunState run, DataManager data, int amount)
    {
        if (run?.Roster?.Active == null || amount <= 0)
        {
            return;
        }

        for (int i = 0; i < run.Roster.Active.Count; i++)
        {
            AddExpToCompanion(run.Roster.Active[i], data, amount);
        }
    }

    public static void AddExpToCompanion(CompanionInstance companion, DataManager data, int amount)
    {
        if (companion == null || amount <= 0)
        {
            return;
        }

        EnsureCompanionRune(companion, data);
        companion.Exp += amount;

        int required = RequiredExpForLevel(data, companion.Level);
        while (required > 0 && companion.Exp >= required)
        {
            companion.Exp -= required;
            companion.Level += 1;
            GrowCompanionStats(companion);

            if (companion.Rune != null)
            {
                int seed = unchecked(
                    companion.Seed * CompanionSeedMultiplier
                    ^ companion.Level * CompanionLevelMultiplier
                    ^ companion.Rune.UnlockedCount
                );
                companion.Rune.InvestRandomAvailable(new System.Random(seed));
            }

            required = RequiredExpForLevel(data, companion.Level);
        }
    }

    public static int RequiredExpForLevel(DataManager data, int level)
    {
        if (
            data?.Balance?.ExpToNextLevel != null
            && level >= 0
            && level < data.Balance.ExpToNextLevel.Count
        )
        {
            return data.Balance.ExpToNextLevel[level];
        }

        return 999999;
    }

    private static void GrowPlayerStats(PlayerState player)
    {
        if (player?.Stats == null)
        {
            return;
        }

        player.Stats.BaseMaxHP += 8;
        player.Stats.BaseMaxMP += 2;
        player.Stats.BaseATK += 2;
        player.Stats.BaseDEF += 1;
        player.Stats.BaseSPD += 1;
        player.Stats.RecalculateFinalStats();
        player.Stats.RestoreToFull();
    }

    private static void EnsureCompanionRune(CompanionInstance companion, DataManager data)
    {
        RuneClass runeClass = companion.Rune?.ClassId ?? RuneClass.None;
        if (
            runeClass == RuneClass.None
            && data?.Companions != null
            && data.Companions.TryGetValue(companion.CompanionDataId, out CompanionData companionData)
            && companionData != null
        )
        {
            runeClass = companionData.ClassId;
        }

        if (runeClass == RuneClass.None || data?.Runes == null)
        {
            return;
        }

        if (companion.Rune == null)
        {
            companion.Rune = new CompanionRuneState
            {
                ClassId = runeClass,
                FixedSequence = new System.Collections.Generic.List<int>(),
            };
        }

        companion.Rune.ClassId = runeClass;
        companion.Rune.Tree = RuneTree.BuildFromData(runeClass, data.Runes.Values);
        companion.Rune.UnlockStarter();
        companion.Rune.SyncTreeStateFromProgress();
    }

    private static void GrowCompanionStats(CompanionInstance companion)
    {
        if (companion?.Stats == null)
        {
            return;
        }

        companion.Stats.BaseMaxHP += 6;
        companion.Stats.BaseATK += 1;
        companion.Stats.BaseDEF += 1;
        companion.Stats.BaseSPD += 1;
        companion.Stats.RecalculateFinalStats();
        companion.Stats.RestoreToFull();
    }
}

namespace Tempt
{
    public static class RunProgression
    {
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
    }
}

namespace Tempt
{
    /// <summary>Safe1 주점 숙박 비용, 회복, 일자 진행 규칙.</summary>
    public static class TavernLodging
    {
        private const int DefaultCostPerPerson = 1;

        public static int GetPartySize(GameRunState run)
        {
            if (run?.Player == null)
            {
                return 0;
            }

            int companionCount = run.Roster?.Active?.Count ?? 0;
            return 1 + companionCount;
        }

        public static int GetRestCost(GameRunState run)
        {
            return GetRestCost(run, ResolveBalance());
        }

        public static int GetRestCost(GameRunState run, BalanceData balance)
        {
            return GetPartySize(run) * GetCostPerPerson(balance);
        }

        public static bool CanRest(GameRunState run)
        {
            return CanRest(run, ResolveBalance());
        }

        public static bool CanRest(GameRunState run, BalanceData balance)
        {
            int cost = GetRestCost(run, balance);
            return cost > 0 && run != null && run.Gold >= cost;
        }

        public static bool TryRest(GameRunState run)
        {
            return TryRest(run, ResolveBalance());
        }

        public static bool TryRest(GameRunState run, BalanceData balance)
        {
            if (!CanRest(run, balance))
            {
                return false;
            }

            int cost = GetRestCost(run, balance);
            run.Gold -= cost;
            run.Player.Stats?.RestoreToFull();

            if (run.Roster?.Active != null)
            {
                for (int i = 0; i < run.Roster.Active.Count; i++)
                {
                    run.Roster.Active[i]?.Stats?.RestoreToFull();
                }
            }

            run.CurrentDay += 1;
            RaiseChanged(run);
            return true;
        }

        public static int GetCostPerPerson(BalanceData balance)
        {
            return System.Math.Max(
                1,
                balance != null && balance.TavernLodgingCostPerPerson > 0
                    ? balance.TavernLodgingCostPerPerson
                    : DefaultCostPerPerson
            );
        }

        private static BalanceData ResolveBalance()
        {
            return GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                ? gsm.Data?.Balance
                : null;
        }

        private static void RaiseChanged(GameRunState run)
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return;
            }

            gsm.Events?.RaiseGoldChanged(run.Gold);
            gsm.Events?.RaiseDayChanged(run.CurrentDay);
            gsm.Erosion?.AdvanceDay(run.CurrentDay);
            gsm.Save?.SaveSnapshot();
        }
    }
}

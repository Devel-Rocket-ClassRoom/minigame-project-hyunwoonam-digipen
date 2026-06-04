namespace Tempt
{
    /// <summary>Safe1 주점 숙박 비용, 회복, 일자 진행 규칙.</summary>
    public static class TavernLodging
    {
        public const int CostPerPerson = 1;

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
            return GetPartySize(run) * CostPerPerson;
        }

        public static bool CanRest(GameRunState run)
        {
            int cost = GetRestCost(run);
            return cost > 0 && run != null && run.Gold >= cost;
        }

        public static bool TryRest(GameRunState run)
        {
            if (!CanRest(run))
            {
                return false;
            }

            int cost = GetRestCost(run);
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

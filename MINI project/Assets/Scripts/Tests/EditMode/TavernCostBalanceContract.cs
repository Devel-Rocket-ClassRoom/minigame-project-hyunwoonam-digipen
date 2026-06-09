internal static class TavernCostBalanceContract
{
    public static int PreviewLodgingCost(GameRunState run, BalanceData balance)
    {
        return TavernLodging.GetRestCost(run, balance);
    }

    public static int PreviewStorageCost(LockerState locker, BalanceData balance)
    {
        return TavernStorage.GetNextUpgradeCost(locker, balance);
    }

    public static int SumConfiguredTavernCosts(BalanceData balance)
    {
        return balance.TavernLodgingCostPerPerson
            + balance.TavernStorageActivationCost
            + balance.TavernStorageUpgradeBaseCost
            + balance.TavernStorageUpgradeCostStep;
    }
}

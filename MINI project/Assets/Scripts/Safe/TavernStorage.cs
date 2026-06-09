/// <summary>Safe1 주점 보관함의 활성화, 업그레이드, 이동, 폐기 규칙.</summary>
public static class TavernStorage
{
    private const int DefaultActivationCost = 1;
    private const int DefaultUpgradeBaseCost = 2;
    private const int DefaultUpgradeCostStep = 1;

    public static int GetNextUpgradeCost(LockerState locker)
    {
        return GetNextUpgradeCost(locker, ResolveBalance());
    }

    public static int GetNextUpgradeCost(LockerState locker, BalanceData balance)
    {
        if (locker == null || !locker.Unlocked)
        {
            return ActivationCost(balance);
        }

        locker.NormalizeCapacity();
        if (locker.IsMaxCapacity)
        {
            return 0;
        }

        int completedUpgrades =
            (locker.Capacity - LockerState.InitialCapacity) / LockerState.CapacityStep;
        return UpgradeBaseCost(balance) + completedUpgrades * UpgradeCostStep(balance);
    }

    public static bool TryActivateOrUpgrade(GameRunState run)
    {
        return TryActivateOrUpgrade(run, ResolveBalance());
    }

    public static bool TryActivateOrUpgrade(GameRunState run, BalanceData balance)
    {
        LockerState locker = run?.Player?.Locker;
        if (locker == null)
        {
            return false;
        }

        int cost = GetNextUpgradeCost(locker, balance);
        if (cost <= 0 || run.Gold < cost)
        {
            return false;
        }

        bool changed;
        if (!locker.Unlocked)
        {
            locker.Unlock();
            changed = true;
        }
        else
        {
            changed = locker.Upgrade();
        }

        if (!changed)
        {
            return false;
        }

        run.Gold -= cost;
        RaiseChanged(run);
        return true;
    }

    public static bool TryMoveStackToStorage(GameRunState run, int itemId, int count)
    {
        InventoryState inventory = run?.Player?.Inventory;
        LockerState locker = run?.Player?.Locker;
        bool moved = inventory != null && inventory.MoveToLocker(locker, itemId, count);
        if (moved)
        {
            Save();
        }

        return moved;
    }

    public static bool TryMoveStackToInventory(GameRunState run, int itemId, int count)
    {
        InventoryState inventory = run?.Player?.Inventory;
        LockerState locker = run?.Player?.Locker;
        bool moved = inventory != null && inventory.MoveFromLocker(locker, itemId, count);
        if (moved)
        {
            Save();
        }

        return moved;
    }

    public static bool TryMoveEquipToStorage(GameRunState run, Item item)
    {
        InventoryState inventory = run?.Player?.Inventory;
        LockerState locker = run?.Player?.Locker;
        bool moved = inventory != null && inventory.MoveEquipToLocker(locker, item);
        if (moved)
        {
            Save();
        }

        return moved;
    }

    public static bool TryMoveEquipToInventory(GameRunState run, Item item)
    {
        InventoryState inventory = run?.Player?.Inventory;
        LockerState locker = run?.Player?.Locker;
        bool moved = inventory != null && inventory.MoveEquipFromLocker(locker, item);
        if (moved)
        {
            Save();
        }

        return moved;
    }

    public static bool TryDiscardStack(
        GameRunState run,
        bool fromStorage,
        int itemId,
        int count
    )
    {
        bool removed = fromStorage
            ? run?.Player?.Locker?.Remove(itemId, count) == true
            : run?.Player?.Inventory?.Discard(itemId, count) == true;
        if (removed)
        {
            Save();
        }

        return removed;
    }

    public static bool TryDiscardEquip(GameRunState run, bool fromStorage, Item item)
    {
        bool removed = fromStorage
            ? run?.Player?.Locker?.RemoveEquip(item) == true
            : run?.Player?.Inventory?.DiscardEquip(item) == true;
        if (removed)
        {
            Save();
        }

        return removed;
    }

    private static void RaiseChanged(GameRunState run)
    {
        if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm))
        {
            return;
        }

        gsm.Events?.RaiseGoldChanged(run.Gold);
        gsm.Events?.RaiseInventoryChanged();
        gsm.Save?.SaveSnapshot();
    }

    private static void Save()
    {
        if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
        {
            gsm.Save?.SaveSnapshot();
        }
    }

    private static BalanceData ResolveBalance()
    {
        return GameSystemManager.TryGetInstance(out GameSystemManager gsm)
            ? gsm.Data?.Balance
            : null;
    }

    private static int ActivationCost(BalanceData balance)
    {
        return System.Math.Max(
            1,
            balance != null && balance.TavernStorageActivationCost > 0
                ? balance.TavernStorageActivationCost
                : DefaultActivationCost
        );
    }

    private static int UpgradeBaseCost(BalanceData balance)
    {
        return System.Math.Max(
            1,
            balance != null && balance.TavernStorageUpgradeBaseCost > 0
                ? balance.TavernStorageUpgradeBaseCost
                : DefaultUpgradeBaseCost
        );
    }

    private static int UpgradeCostStep(BalanceData balance)
    {
        return System.Math.Max(
            0,
            balance != null && balance.TavernStorageUpgradeCostStep >= 0
                ? balance.TavernStorageUpgradeCostStep
                : DefaultUpgradeCostStep
        );
    }
}

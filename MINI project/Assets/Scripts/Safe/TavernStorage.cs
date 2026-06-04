namespace Tempt
{
    /// <summary>Safe1 주점 보관함의 활성화, 업그레이드, 이동, 폐기 규칙.</summary>
    public static class TavernStorage
    {
        public const int ActivationCost = 1;

        public static int GetNextUpgradeCost(LockerState locker)
        {
            if (locker == null || !locker.Unlocked)
            {
                return ActivationCost;
            }

            locker.NormalizeCapacity();
            if (locker.IsMaxCapacity)
            {
                return 0;
            }

            int completedUpgrades =
                (locker.Capacity - LockerState.InitialCapacity) / LockerState.CapacityStep;
            return completedUpgrades + 2;
        }

        public static bool TryActivateOrUpgrade(GameRunState run)
        {
            LockerState locker = run?.Player?.Locker;
            if (locker == null)
            {
                return false;
            }

            int cost = GetNextUpgradeCost(locker);
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

        public static bool TryDiscardStack(GameRunState run, bool fromStorage, int itemId, int count)
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
    }
}

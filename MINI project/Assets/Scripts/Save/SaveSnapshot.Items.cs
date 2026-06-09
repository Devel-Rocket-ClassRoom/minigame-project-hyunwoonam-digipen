using System.Collections.Generic;

public sealed partial class SaveSnapshot
{
    private static InventorySnapshot FromInventory(InventoryState inv)
    {
        var snapshot = new InventorySnapshot
        {
            StackableItems = new List<InventoryEntry>(),
            EquipItems = new List<EquipItemEntry>(),
        };
        if (inv == null)
        {
            return snapshot;
        }

        foreach (KeyValuePair<int, int> entry in inv.StackableItems)
        {
            snapshot.StackableItems.Add(
                new InventoryEntry { ItemId = entry.Key, Count = entry.Value }
            );
        }

        foreach (Item item in inv.EquipItems)
        {
            if (item?.Data != null)
            {
                snapshot.EquipItems.Add(
                    new EquipItemEntry
                    {
                        ItemId = item.Data.Id,
                        Enhancement = item.Enhancement,
                        EnhanceFailStreak = item.EnhanceFailStreak,
                    }
                );
            }
        }

        return snapshot;
    }

    private static InventoryState ToInventory(InventorySnapshot snapshot, DataManager data)
    {
        var inv = new InventoryState();
        if (snapshot == null)
        {
            return inv;
        }

        if (snapshot.StackableItems != null)
        {
            foreach (InventoryEntry entry in snapshot.StackableItems)
            {
                inv.StackableItems[entry.ItemId] = entry.Count;
            }
        }

        if (snapshot.EquipItems != null && data != null)
        {
            foreach (EquipItemEntry entry in snapshot.EquipItems)
            {
                if (data.Items.TryGetValue(entry.ItemId, out ItemData itemData))
                {
                    inv.EquipItems.Add(
                        new Item
                        {
                            Data = itemData,
                            Enhancement = entry.Enhancement,
                            EnhanceFailStreak = entry.EnhanceFailStreak,
                        }
                    );
                }
            }
        }

        return inv;
    }

    private static EquipmentSnapshot FromEquipment(EquipmentSlots equipment)
    {
        return new EquipmentSnapshot
        {
            WeaponId = equipment?.Weapon?.Data != null ? equipment.Weapon.Data.Id : 0,
            WeaponEnhancement = equipment?.Weapon != null ? equipment.Weapon.Enhancement : 0,
            WeaponEnhanceFailStreak =
                equipment?.Weapon != null ? equipment.Weapon.EnhanceFailStreak : 0,
            ArmorBodyId = equipment?.ArmorBody?.Data != null ? equipment.ArmorBody.Data.Id : 0,
            ArmorBodyEnhancement =
                equipment?.ArmorBody != null ? equipment.ArmorBody.Enhancement : 0,
            ArmorBodyEnhanceFailStreak =
                equipment?.ArmorBody != null ? equipment.ArmorBody.EnhanceFailStreak : 0,
            ArmorArmsId = equipment?.ArmorArms?.Data != null ? equipment.ArmorArms.Data.Id : 0,
            ArmorArmsEnhancement =
                equipment?.ArmorArms != null ? equipment.ArmorArms.Enhancement : 0,
            ArmorArmsEnhanceFailStreak =
                equipment?.ArmorArms != null ? equipment.ArmorArms.EnhanceFailStreak : 0,
            ArmorLegsId = equipment?.ArmorLegs?.Data != null ? equipment.ArmorLegs.Data.Id : 0,
            ArmorLegsEnhancement =
                equipment?.ArmorLegs != null ? equipment.ArmorLegs.Enhancement : 0,
            ArmorLegsEnhanceFailStreak =
                equipment?.ArmorLegs != null ? equipment.ArmorLegs.EnhanceFailStreak : 0,
        };
    }

    private static EquipmentSlots ToEquipment(EquipmentSnapshot snapshot, DataManager data)
    {
        var equipment = new EquipmentSlots();
        if (snapshot == null || data == null)
        {
            return equipment;
        }

        equipment.Weapon = CreateItem(
            snapshot.WeaponId,
            snapshot.WeaponEnhancement,
            snapshot.WeaponEnhanceFailStreak,
            data
        );
        equipment.ArmorBody = CreateItem(
            snapshot.ArmorBodyId,
            snapshot.ArmorBodyEnhancement,
            snapshot.ArmorBodyEnhanceFailStreak,
            data
        );
        equipment.ArmorArms = CreateItem(
            snapshot.ArmorArmsId,
            snapshot.ArmorArmsEnhancement,
            snapshot.ArmorArmsEnhanceFailStreak,
            data
        );
        equipment.ArmorLegs = CreateItem(
            snapshot.ArmorLegsId,
            snapshot.ArmorLegsEnhancement,
            snapshot.ArmorLegsEnhanceFailStreak,
            data
        );
        return equipment;
    }

    private static LockerSnapshot FromLocker(LockerState locker)
    {
        var snapshot = new LockerSnapshot
        {
            Unlocked = locker != null && locker.Unlocked,
            Capacity = locker != null ? locker.Capacity : 0,
            StackableItems = new List<InventoryEntry>(),
            EquipItems = new List<EquipItemEntry>(),
        };
        if (locker == null)
        {
            return snapshot;
        }

        foreach (KeyValuePair<int, int> entry in locker.StackableItems)
        {
            snapshot.StackableItems.Add(
                new InventoryEntry { ItemId = entry.Key, Count = entry.Value }
            );
        }

        foreach (Item item in locker.EquipItems)
        {
            if (item?.Data != null)
            {
                snapshot.EquipItems.Add(
                    new EquipItemEntry
                    {
                        ItemId = item.Data.Id,
                        Enhancement = item.Enhancement,
                        EnhanceFailStreak = item.EnhanceFailStreak,
                    }
                );
            }
        }

        return snapshot;
    }

    private static LockerState ToLocker(LockerSnapshot snapshot, DataManager data)
    {
        var locker = new LockerState();
        if (snapshot == null)
        {
            return locker;
        }

        locker.Unlocked = snapshot.Unlocked;
        locker.Capacity = snapshot.Capacity;
        locker.NormalizeCapacity();
        if (snapshot.StackableItems != null)
        {
            foreach (InventoryEntry entry in snapshot.StackableItems)
            {
                locker.StackableItems[entry.ItemId] = entry.Count;
            }
        }

        if (snapshot.EquipItems != null && data != null)
        {
            foreach (EquipItemEntry entry in snapshot.EquipItems)
            {
                Item item = CreateItem(
                    entry.ItemId,
                    entry.Enhancement,
                    entry.EnhanceFailStreak,
                    data
                );
                if (item != null)
                {
                    locker.EquipItems.Add(item);
                }
            }
        }

        return locker;
    }

}

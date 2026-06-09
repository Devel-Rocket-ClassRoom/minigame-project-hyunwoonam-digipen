public static class RewardGrant
{
    public static void ApplyNonItemRewards(GameRunState run, DataManager data, EventBus events, NodeRewardSummary summary)
    {
        if (summary == null)
        {
            summary = new NodeRewardSummary();
        }

        RunProgression.AddExpToPlayer(run, data, events, summary.TotalExp);
        RunProgression.AddExpToActiveCompanions(run, data, summary.TotalExp);

        if (run != null)
        {
            run.Gold += summary.TotalGold;
            events?.RaiseGoldChanged(run.Gold);
        }
    }

    public static bool TryGrantItem(GameRunState run, DataManager data, int itemId)
    {
        if (
            run?.Player?.Inventory == null
            || data?.Items == null
            || !data.Items.TryGetValue(itemId, out ItemData itemData)
            || itemData == null
        )
        {
            return false;
        }

        return itemData.Stackable
            ? run.Player.Inventory.TryAdd(itemData, 1)
            : run.Player.Inventory.TryAddEquip(
                new Item { Data = itemData, Enhancement = 0 }
            );
    }
}

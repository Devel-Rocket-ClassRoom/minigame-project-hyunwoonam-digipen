using System.Collections.Generic;

namespace Tempt
{
    public static class RewardGrant
    {
        public static void ApplyCombatRewards(GameRunState run, DataManager data, EventBus events, NodeRewardSummary summary, List<int> overflowIds)
        {
            if (summary == null)
            {
                summary = new NodeRewardSummary();
            }

            RunProgression.AddExpToPlayer(run, data, events, summary.TotalExp);

            if (run != null)
            {
                run.Gold += summary.TotalGold;
                events?.RaiseGoldChanged(run.Gold);
            }

            GrantDroppedItems(run, data, summary, overflowIds);
        }

        private static void GrantDroppedItems(GameRunState run, DataManager data, NodeRewardSummary summary, List<int> overflowIds)
        {
            if (summary?.DroppedItemIds == null || run?.Player?.Inventory == null || data?.Items == null)
            {
                return;
            }

            foreach (int itemId in summary.DroppedItemIds)
            {
                if (!data.Items.TryGetValue(itemId, out ItemData itemData))
                {
                    continue;
                }

                bool added = itemData.Stackable
                    ? run.Player.Inventory.TryAdd(itemId, 1)
                    : run.Player.Inventory.TryAddEquip(new Item { Data = itemData, Enhancement = 0 });

                if (!added && overflowIds != null)
                {
                    overflowIds.Add(itemId);
                }
            }
        }
    }
}

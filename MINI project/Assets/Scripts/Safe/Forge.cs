using UnityEngine;

namespace Tempt
{
    public enum EnhanceOutcome
    {
        Success,
        Fail,
        Blocked,
    }

    public readonly struct EnhanceResult
    {
        public readonly EnhanceOutcome Outcome;
        public readonly int GoldSpent;
        public readonly int NewLevel;
        public readonly bool PityForced;

        public EnhanceResult(
            EnhanceOutcome outcome,
            int goldSpent,
            int newLevel,
            bool pityForced
        )
        {
            Outcome = outcome;
            GoldSpent = goldSpent;
            NewLevel = newLevel;
            PityForced = pityForced;
        }
    }

    /// <summary>Safe1 대장간 장비 강화 도메인 API.</summary>
    public static class Forge
    {
        public static int GetEnhanceCost(Item item, DataManager data)
        {
            if (!TryGetEnhanceInput(item, data, out BalanceData balance))
            {
                return 0;
            }

            int level = Mathf.Max(0, item.Enhancement);
            int basePrice = Mathf.Max(0, item.Data.BasePrice);
            float factor = balance.EnhanceCostBase + level * balance.EnhanceCostPerLevel;
            return Mathf.Max(1, Mathf.RoundToInt(basePrice * factor));
        }

        public static float GetSuccessRate(Item item, DataManager data)
        {
            if (!TryGetEnhanceInput(item, data, out BalanceData balance))
            {
                return 0f;
            }

            int pityCount = Mathf.Max(0, balance.EnhancePityFailCount);
            if (pityCount > 0 && item.EnhanceFailStreak >= pityCount)
            {
                return 1f;
            }

            int level = Mathf.Max(0, item.Enhancement);
            float minRate = Mathf.Clamp01(balance.EnhanceMinSuccessRate);
            float rate = balance.EnhanceBaseSuccessRate - level * balance.EnhanceSuccessRateDecayPerLevel;
            return Mathf.Clamp(rate, minRate, 1f);
        }

        public static int GetPityRemaining(Item item, DataManager data)
        {
            if (!TryGetEnhanceInput(item, data, out BalanceData balance))
            {
                return 0;
            }

            int pityCount = Mathf.Max(0, balance.EnhancePityFailCount);
            return pityCount > 0 ? Mathf.Max(0, pityCount - item.EnhanceFailStreak) : 0;
        }

        public static bool CanEnhance(Item item, GameRunState run, DataManager data)
        {
            return TryGetEnhanceInput(item, data, out _)
                && run != null
                && run.Gold >= GetEnhanceCost(item, data);
        }

        public static EnhanceResult TryEnhance(Item item, GameRunState run, DataManager data)
        {
            if (!CanEnhance(item, run, data))
            {
                return PublishResult(new EnhanceResult(EnhanceOutcome.Blocked, 0, CurrentLevel(item), false));
            }

            int cost = GetEnhanceCost(item, data);
            bool pityForced = IsPityForced(item, data);
            float successRate = GetSuccessRate(item, data);

            run.Gold -= cost;
            RaiseGoldChanged(run.Gold);

            bool success = pityForced || Random.value <= successRate;
            if (success)
            {
                item.Enhancement = Mathf.Max(0, item.Enhancement) + 1;
                item.EnhanceFailStreak = 0;
                RecalculateIfEquipped(item, run);
            }
            else
            {
                item.EnhanceFailStreak = Mathf.Max(0, item.EnhanceFailStreak) + 1;
            }

            RaiseInventoryAndEquipmentChanged();
            SaveSnapshot();

            return PublishResult(
                new EnhanceResult(
                    success ? EnhanceOutcome.Success : EnhanceOutcome.Fail,
                    cost,
                    item.Enhancement,
                    pityForced
                )
            );
        }

        private static bool TryGetEnhanceInput(Item item, DataManager data, out BalanceData balance)
        {
            balance = data?.Balance;
            if (item?.Data == null || balance == null)
            {
                GameLog.LogError("[Forge] Item.Data 또는 BalanceData 참조가 없습니다.");
                return false;
            }

            if (item.Data.Category != ItemCategory.Equipment || item.Data.EquipSlot == EquipmentSlotId.None)
            {
                return false;
            }

            return true;
        }

        private static int CurrentLevel(Item item)
        {
            return item != null ? Mathf.Max(0, item.Enhancement) : 0;
        }

        private static bool IsPityForced(Item item, DataManager data)
        {
            BalanceData balance = data?.Balance;
            int pityCount = Mathf.Max(0, balance != null ? balance.EnhancePityFailCount : 0);
            return item != null && pityCount > 0 && item.EnhanceFailStreak >= pityCount;
        }

        private static void RecalculateIfEquipped(Item item, GameRunState run)
        {
            PlayerState player = run?.Player;
            EquipmentSlots equipment = player?.Equipment;
            if (equipment == null || !equipment.Contains(item))
            {
                return;
            }

            EquipFlow.RecalculateStats(player);
        }

        private static EnhanceResult PublishResult(EnhanceResult result)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseEnhanceResult(result);
            }

            return result;
        }

        private static void RaiseGoldChanged(int gold)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseGoldChanged(gold);
            }
        }

        private static void RaiseInventoryAndEquipmentChanged()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseInventoryChanged();
                gsm.Events?.RaiseEquipmentChanged();
            }
        }

        private static void SaveSnapshot()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Save?.SaveSnapshot();
            }
        }
    }
}

using System.Collections.Generic;

namespace Tempt
{
    public sealed partial class SaveSnapshot
    {
        private static PlayerSnapshot FromPlayer(PlayerState player)
        {
            if (player == null)
            {
                return null;
            }

            return new PlayerSnapshot
            {
                Name = player.Name,
                Level = player.Level,
                Exp = player.Exp,
                Stats = FromStats(player.Stats),
                Rune = new PlayerRuneSnapshot
                {
                    StartingClassRuneId = (int)player.StartingClass,
                    UnlockedNodeIds =
                        player.Rune?.UnlockedIds != null
                            ? new List<int>(player.Rune.UnlockedIds)
                            : new List<int>(),
                    NodeInvestments = FromRuneInvestments(player.Rune),
                    RunePoints = player.Rune != null ? player.Rune.RunePoints : 0,
                },
                Inventory = FromInventory(player.Inventory),
                Equipment = FromEquipment(player.Equipment),
                ConsumableSlots =
                    player.Consumables != null
                        ? new List<int>(player.Consumables.SlotItemIds)
                        : CreateEmptyConsumableSlots(),
                Locker = FromLocker(player.Locker),
                OwnedSkillIds =
                    player.OwnedSkillIds != null
                        ? new List<int>(player.OwnedSkillIds)
                        : new List<int>(),
                ActiveSlotSkillIds =
                    player.ActiveSlotSkillIds != null
                        ? new List<int>(player.ActiveSlotSkillIds)
                        : new List<int> { 0, 0 },
            };
        }

        private static PlayerState ToPlayer(PlayerSnapshot snapshot, DataManager data)
        {
            if (snapshot == null)
            {
                return null;
            }

            var player = new PlayerState
            {
                Name = snapshot.Name,
                Level = snapshot.Level <= 0 ? 1 : snapshot.Level,
                Exp = snapshot.Exp,
                Stats = ToStats(snapshot.Stats),
                StartingClass =
                    snapshot.Rune != null
                        ? (RuneClass)snapshot.Rune.StartingClassRuneId
                        : RuneClass.Dealer,
                Inventory = ToInventory(snapshot.Inventory, data),
                Equipment = ToEquipment(snapshot.Equipment, data),
                Consumables = new ConsumableSlots(),
                Locker = ToLocker(snapshot.Locker, data),
                OwnedSkillIds =
                    snapshot.OwnedSkillIds != null
                        ? new HashSet<int>(snapshot.OwnedSkillIds)
                        : new HashSet<int>(),
                ActiveSlotSkillIds = ToActiveSlotSkillIds(snapshot.ActiveSlotSkillIds),
            };

            player.Rune = new PlayerRuneState
            {
                ClassId = player.StartingClass,
                RunePoints = snapshot.Rune != null ? snapshot.Rune.RunePoints : 0,
                UnlockedIds =
                    snapshot.Rune?.UnlockedNodeIds != null
                        ? new HashSet<int>(snapshot.Rune.UnlockedNodeIds)
                        : new HashSet<int>(),
                InvestedPointsByNode = ToRuneInvestments(snapshot.Rune),
                Tree =
                    data != null
                        ? RuneTree.BuildFromData(player.StartingClass, data.Runes.Values)
                        : null,
            };
            player.Rune.SyncTreeStateFromProgress();

            if (snapshot.ConsumableSlots != null)
            {
                for (
                    int i = 0;
                    i < player.Consumables.SlotItemIds.Length && i < snapshot.ConsumableSlots.Count;
                    i++
                )
                {
                    player.Consumables.SlotItemIds[i] = snapshot.ConsumableSlots[i];
                }
            }

            return player;
        }

        private static int[] ToActiveSlotSkillIds(List<int> values)
        {
            var result = new int[2];
            if (values == null)
            {
                return result;
            }

            for (int i = 0; i < result.Length && i < values.Count; i++)
            {
                result[i] = values[i];
            }

            return result;
        }

        private static List<RuneNodeInvestmentSnapshot> FromRuneInvestments(PlayerRuneState rune)
        {
            var result = new List<RuneNodeInvestmentSnapshot>();
            if (rune?.InvestedPointsByNode == null)
            {
                return result;
            }

            foreach (KeyValuePair<int, int> pair in rune.InvestedPointsByNode)
            {
                if (pair.Value <= 0)
                {
                    continue;
                }

                result.Add(
                    new RuneNodeInvestmentSnapshot
                    {
                        NodeId = pair.Key,
                        InvestedPoints = pair.Value,
                    }
                );
            }

            return result;
        }

        private static Dictionary<int, int> ToRuneInvestments(PlayerRuneSnapshot snapshot)
        {
            var result = new Dictionary<int, int>();
            if (snapshot?.NodeInvestments == null)
            {
                return result;
            }

            foreach (RuneNodeInvestmentSnapshot investment in snapshot.NodeInvestments)
            {
                if (investment == null || investment.NodeId == 0 || investment.InvestedPoints <= 0)
                {
                    continue;
                }

                result[investment.NodeId] = investment.InvestedPoints;
            }

            return result;
        }

        private static StatBlockSnapshot FromStats(StatBlock stats)
        {
            return stats == null
                ? null
                : new StatBlockSnapshot
                {
                    MaxHP = stats.MaxHP,
                    CurrentHP = stats.CurrentHP,
                    MaxMP = stats.MaxMP,
                    CurrentMP = stats.CurrentMP,
                    ATK = stats.ATK,
                    DEF = stats.DEF,
                    SPD = stats.SPD,
                };
        }

        private static StatBlock ToStats(StatBlockSnapshot snapshot)
        {
            var stats = new StatBlock();
            if (snapshot == null)
            {
                stats.SetBaseStats(80, 20, 10, 2, 10);
                stats.RestoreToFull();
                return stats;
            }

            stats.SetBaseStats(
                snapshot.MaxHP,
                snapshot.MaxMP,
                snapshot.ATK,
                snapshot.DEF,
                snapshot.SPD
            );
            stats.CurrentHP = snapshot.CurrentHP;
            stats.CurrentMP = snapshot.CurrentMP;
            stats.ClampToMax();
            return stats;
        }

    }
}

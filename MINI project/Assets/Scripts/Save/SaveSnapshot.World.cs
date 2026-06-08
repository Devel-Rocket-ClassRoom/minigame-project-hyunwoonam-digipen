using System.Collections.Generic;

namespace Tempt
{
    public sealed partial class SaveSnapshot
    {
        private static FloorMapSnapshot FromFloorMap(FloorMapModel map)
        {
            var snapshot = new FloorMapSnapshot
            {
                NextSelectableFloor = map != null ? map.NextSelectableFloor : 0,
                Nodes = new List<FloorNodeSnapshot>(),
            };
            if (map == null)
            {
                return snapshot;
            }

            foreach (List<FloorNode> floorNodes in map.NodesByFloor.Values)
            {
                foreach (FloorNode node in floorNodes)
                {
                    snapshot.Nodes.Add(
                        new FloorNodeSnapshot
                        {
                            NodeId = node.NodeId,
                            Floor = node.Floor,
                            StageIndex = node.StageIndex,
                            Difficulty = node.Difficulty,
                            MonsterCount = node.MonsterCount,
                            IsBoss = node.IsBoss,
                            IsSafeZone = node.IsSafeZone,
                            IsCleared = node.IsCleared,
                            NextNodeIds =
                                node.NextNodeIds != null
                                    ? new List<int>(node.NextNodeIds)
                                    : new List<int>(),
                        }
                    );
                }
            }

            return snapshot;
        }

        private static FloorMapModel ToFloorMap(FloorMapSnapshot snapshot)
        {
            var map = new FloorMapModel();
            if (snapshot == null || snapshot.Nodes == null)
            {
                return map;
            }

            map.NextSelectableFloor = snapshot.NextSelectableFloor;
            foreach (FloorNodeSnapshot src in snapshot.Nodes)
            {
                var node = new FloorNode
                {
                    NodeId = src.NodeId,
                    Floor = src.Floor,
                    StageIndex = src.StageIndex,
                    Difficulty = src.Difficulty,
                    MonsterCount = src.MonsterCount,
                    IsBoss = src.IsBoss,
                    IsSafeZone = src.IsSafeZone,
                    IsCleared = src.IsCleared,
                    NextNodeIds =
                        src.NextNodeIds != null ? new List<int>(src.NextNodeIds) : new List<int>(),
                };
                map.NodesById[node.NodeId] = node;
                if (!map.NodesByFloor.TryGetValue(node.Floor, out List<FloorNode> floorNodes))
                {
                    floorNodes = new List<FloorNode>();
                    map.NodesByFloor[node.Floor] = floorNodes;
                }

                floorNodes.Add(node);
            }

            return map;
        }

        private static ErosionSnapshot FromErosion(ErosionStateModel model)
        {
            var snapshot = new ErosionSnapshot
            {
                StageRates = new List<float>(),
                ErosionStarted = model != null && model.IsActivated,
                CurrentEroddingStage = model != null ? model.CurrentEroddingStage : 1,
                StageSafeLocked = new List<bool>(),
            };
            int stageCount = model != null ? model.GetStageCount() : 6;
            for (int i = 1; i <= stageCount; i++)
            {
                float rate = model != null ? model.GetRate(i) : 0f;
                snapshot.StageRates.Add(rate);
                snapshot.StageSafeLocked.Add(rate >= 100f);
            }

            return snapshot;
        }

        private static ErosionStateModel ToErosion(ErosionSnapshot snapshot, WorldData world)
        {
            int currentStage = snapshot != null ? snapshot.CurrentEroddingStage : 1;
            int stageCount =
                snapshot?.StageRates != null && snapshot.StageRates.Count > 0
                    ? snapshot.StageRates.Count
                    : ErosionSystem.GetMaxStage(world);
            var model = new ErosionStateModel
            {
                IsActivated = snapshot != null && snapshot.ErosionStarted,
                CurrentEroddingStage = System.Math.Max(
                    1,
                    System.Math.Min(stageCount, currentStage)
                ),
            };
            model.EnsureStageCount(stageCount);
            bool allStagesFullyEroded = true;
            for (int i = 1; i <= stageCount; i++)
            {
                int listIndex = i - 1;
                model.StageRates[i] =
                    snapshot?.StageRates != null && listIndex < snapshot.StageRates.Count
                        ? snapshot.StageRates[listIndex]
                        : 0f;
                if (model.StageRates[i] < 100f)
                {
                    allStagesFullyEroded = false;
                }
            }

            if (allStagesFullyEroded)
            {
                model.CurrentEroddingStage = stageCount;
            }

            return model;
        }

        private static List<bool> FromSafeUnlocks(SafeZoneUnlockState state)
        {
            var result = new List<bool>();
            int length = state?.Unlocked != null ? state.Unlocked.Length : 6;
            for (int i = 0; i < length; i++)
            {
                result.Add(state != null && state.IsUnlocked(i));
            }

            return result;
        }

        private static SafeZoneUnlockState ToSafeUnlocks(List<bool> values)
        {
            var state = new SafeZoneUnlockState(
                values != null && values.Count > 0 ? values.Count : 6
            );
            for (int i = 0; i < state.Unlocked.Length; i++)
            {
                state.Unlocked[i] = values != null && i < values.Count && values[i];
            }

            return state;
        }

        private static List<ShopStockEntrySnapshot> FromShopStock(ShopStockState state)
        {
            var result = new List<ShopStockEntrySnapshot>();
            if (state?.Entries == null)
            {
                return result;
            }

            foreach (ShopStockEntry entry in state.Entries)
            {
                if (entry == null)
                {
                    continue;
                }

                result.Add(
                    new ShopStockEntrySnapshot
                    {
                        ItemId = entry.ItemId,
                        Available = entry.Available,
                        RemainingCount = entry.RemainingCount,
                        InitialCount = entry.InitialCount,
                        UnitPrice = entry.UnitPrice,
                        UnlockKey = entry.UnlockKey,
                    }
                );
            }

            return result;
        }

        private static ShopStockState ToShopStock(List<ShopStockEntrySnapshot> snapshot)
        {
            var state = new ShopStockState { Entries = new List<ShopStockEntry>() };
            if (snapshot != null)
            {
                foreach (ShopStockEntrySnapshot src in snapshot)
                {
                    if (src == null)
                    {
                        continue;
                    }

                    state.Entries.Add(
                        new ShopStockEntry
                        {
                            ItemId = src.ItemId,
                            Available = src.Available,
                            RemainingCount = src.RemainingCount,
                            InitialCount = src.InitialCount,
                            UnitPrice = src.UnitPrice,
                            UnlockKey = src.UnlockKey,
                        }
                    );
                }
            }

            state.EnsureDefaultSafe1Stock();
            return state;
        }

    }
}

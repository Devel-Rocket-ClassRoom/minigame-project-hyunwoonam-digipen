using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 한 런의 전체 상태를 직렬화하기 위한 스냅샷.
    /// "종료 시점 전체 복원" 정책에 따라 모든 동적 상태를 포함.
    /// 플로어 맵은 seed 재생성이 아니라 저장 시점의 전체 노드 구조를 직접 포함한다.
    /// 저장 형식: JSON (PersistentDataPath/save.json).
    /// </summary>
    [System.Serializable]
    public sealed class SaveSnapshot
    {
        /// <summary>저장 일자(ISO).</summary>
        public string SavedAtIso;

        /// <summary>클리어 완료된 런 여부(true면 Continue 불가).</summary>
        public bool IsCompleted;

        /// <summary>현재 게임 내 일자.</summary>
        public int CurrentDay;

        /// <summary>현재 위치한 층/씬.</summary>
        public SaveLocation Location;

        /// <summary>플로어 맵 전체 구조와 진행 상태 직렬화.</summary>
        public FloorMapSnapshot FloorMap;

        /// <summary>플레이어 상태.</summary>
        public PlayerSnapshot Player;

        /// <summary>동료 명부.</summary>
        public RosterSnapshot Roster;

        /// <summary>침식 상태(단계별 침식률 + 잠금 상태).</summary>
        public ErosionSnapshot Erosion;

        /// <summary>
        /// 안전지대 해금 상태(Safe0~5). 인덱스 = 안전지대 번호.
        /// false = 잠금, true = 해금.
        /// </summary>
        public List<bool> SafeUnlocks;

        /// <summary>상점 재고와 구매 이력.</summary>
        public List<ShopStockEntrySnapshot> ShopStock;

        /// <summary>골드.</summary>
        public int Gold;

        /// <summary>마석.</summary>
        public int ManaStone;

        /// <summary>튜토리얼 진행.</summary>
        public TutorialSnapshot Tutorial;

        /// <summary>옵션/언어/볼륨/화면모드.</summary>
        public OptionSnapshot Options;

        /// <summary>
        /// 런타임 런 상태를 저장용 스냅샷으로 변환한다.
        /// </summary>
        public static SaveSnapshot FromGameRunStatet(GameRunState run, SceneId sceneId)
        {
            if (run == null)
            {
                return null;
            }

            return new SaveSnapshot
            {
                SavedAtIso = System.DateTime.Now.ToString("o"),
                IsCompleted = false,
                CurrentDay = run.CurrentDay,
                Location = new SaveLocation { SceneId = sceneId, SubLocationKey = string.Empty },
                FloorMap = FromFloorMap(run.FloorMap),
                Player = FromPlayer(run.Player),
                Roster = FromRoster(run.Roster),
                Erosion = FromErosion(run.Erosion),
                SafeUnlocks = FromSafeUnlocks(run.SafeUnlocks),
                ShopStock = FromShopStock(run.ShopStock),
                Gold = run.Gold,
                ManaStone = run.ManaStone,
                Tutorial = new TutorialSnapshot
                {
                    CompletedSteps =
                        run.Tutorial != null
                            ? new List<string>(run.Tutorial.CompletedSteps)
                            : new List<string>(),
                },
                Options = new OptionSnapshot
                {
                    LanguageCode = "ko",
                    MasterVolume = 1f,
                    Fullscreen = true,
                    ResolutionWidth = 1920,
                    ResolutionHeight = 1080,
                },
            };
        }

        /// <summary>
        /// 저장 스냅샷을 런타임 런 상태로 복원한다.
        /// </summary>
        public GameRunState ToGameRunStatet(DataManager data)
        {
            var run = new GameRunState
            {
                CurrentDay = CurrentDay,
                FloorMap = ToFloorMap(FloorMap),
                Player = ToPlayer(Player, data),
                Roster = ToRoster(Roster),
                Erosion = ToErosion(Erosion, data?.World),
                SafeUnlocks = ToSafeUnlocks(SafeUnlocks),
                ShopStock = ToShopStock(ShopStock),
                Gold = Gold,
                ManaStone = ManaStone,
                Tutorial = new TutorialProgressState
                {
                    CompletedSteps =
                        Tutorial?.CompletedSteps != null
                            ? new List<string>(Tutorial.CompletedSteps)
                            : new List<string>(),
                },
            };

            run.CurrentFloor = ResolveCurrentFloor(run.FloorMap);
            run.HighestFloor = ResolveHighestClearedFloor(run.FloorMap);
            return run;
        }

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

        private static List<int> CreateEmptyConsumableSlots()
        {
            var result = new List<int>();
            for (int i = 0; i < ConsumableSlots.SlotCount; i++)
            {
                result.Add(0);
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
                        new EquipItemEntry { ItemId = item.Data.Id, Enhancement = item.Enhancement }
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
                            new Item { Data = itemData, Enhancement = entry.Enhancement }
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
                ArmorBodyId = equipment?.ArmorBody?.Data != null ? equipment.ArmorBody.Data.Id : 0,
                ArmorBodyEnhancement =
                    equipment?.ArmorBody != null ? equipment.ArmorBody.Enhancement : 0,
                ArmorArmsId = equipment?.ArmorArms?.Data != null ? equipment.ArmorArms.Data.Id : 0,
                ArmorArmsEnhancement =
                    equipment?.ArmorArms != null ? equipment.ArmorArms.Enhancement : 0,
                ArmorLegsId = equipment?.ArmorLegs?.Data != null ? equipment.ArmorLegs.Data.Id : 0,
                ArmorLegsEnhancement =
                    equipment?.ArmorLegs != null ? equipment.ArmorLegs.Enhancement : 0,
            };
        }

        private static EquipmentSlots ToEquipment(EquipmentSnapshot snapshot, DataManager data)
        {
            var equipment = new EquipmentSlots();
            if (snapshot == null || data == null)
            {
                return equipment;
            }

            equipment.Weapon = CreateItem(snapshot.WeaponId, snapshot.WeaponEnhancement, data);
            equipment.ArmorBody = CreateItem(
                snapshot.ArmorBodyId,
                snapshot.ArmorBodyEnhancement,
                data
            );
            equipment.ArmorArms = CreateItem(
                snapshot.ArmorArmsId,
                snapshot.ArmorArmsEnhancement,
                data
            );
            equipment.ArmorLegs = CreateItem(
                snapshot.ArmorLegsId,
                snapshot.ArmorLegsEnhancement,
                data
            );
            return equipment;
        }

        private static LockerSnapshot FromLocker(LockerState locker)
        {
            var snapshot = new LockerSnapshot
            {
                Unlocked = locker != null && locker.Unlocked,
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
                        new EquipItemEntry { ItemId = item.Data.Id, Enhancement = item.Enhancement }
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
                    Item item = CreateItem(entry.ItemId, entry.Enhancement, data);
                    if (item != null)
                    {
                        locker.EquipItems.Add(item);
                    }
                }
            }

            return locker;
        }

        private static Item CreateItem(int itemId, int enhancement, DataManager data)
        {
            if (
                itemId == 0
                || data == null
                || !data.Items.TryGetValue(itemId, out ItemData itemData)
            )
            {
                return null;
            }

            return new Item { Data = itemData, Enhancement = enhancement };
        }

        private static RosterSnapshot FromRoster(CompanionRosterState roster)
        {
            var snapshot = new RosterSnapshot
            {
                Active = new List<CompanionSnapshot>(),
                Bench = new List<CompanionSnapshot>(),
            };
            if (roster == null)
            {
                return snapshot;
            }

            foreach (CompanionInstance inst in roster.Active)
                snapshot.Active.Add(FromCompanion(inst));
            foreach (CompanionInstance inst in roster.Bench)
                snapshot.Bench.Add(FromCompanion(inst));
            return snapshot;
        }

        private static CompanionRosterState ToRoster(RosterSnapshot snapshot)
        {
            var roster = new CompanionRosterState();
            if (snapshot == null)
            {
                return roster;
            }

            if (snapshot.Active != null)
                foreach (CompanionSnapshot src in snapshot.Active)
                    roster.Active.Add(ToCompanion(src));
            if (snapshot.Bench != null)
                foreach (CompanionSnapshot src in snapshot.Bench)
                    roster.Bench.Add(ToCompanion(src));
            return roster;
        }

        private static CompanionSnapshot FromCompanion(CompanionInstance inst)
        {
            return new CompanionSnapshot
            {
                CompanionId = inst.CompanionDataId,
                Level = inst.Level,
                Exp = inst.Exp,
                FixedRuneSequence =
                    inst.Rune?.FixedSequence != null
                        ? new List<int>(inst.Rune.FixedSequence)
                        : new List<int>(),
                UnlockedCount = inst.Rune != null ? inst.Rune.UnlockedCount : 0,
                Stats = FromStats(inst.Stats),
                Equipment = FromEquipment(inst.Equipment),
            };
        }

        private static CompanionInstance ToCompanion(CompanionSnapshot src)
        {
            return new CompanionInstance
            {
                CompanionDataId = src.CompanionId,
                Level = src.Level <= 0 ? 1 : src.Level,
                Exp = src.Exp,
                Stats = ToStats(src.Stats),
                Rune = new CompanionRuneState
                {
                    FixedSequence =
                        src.FixedRuneSequence != null
                            ? new List<int>(src.FixedRuneSequence)
                            : new List<int>(),
                    UnlockedCount = src.UnlockedCount,
                },
                Equipment = new EquipmentSlots(),
            };
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

        private static int ResolveCurrentFloor(FloorMapModel map)
        {
            return map != null ? System.Math.Max(0, map.NextSelectableFloor - 1) : 0;
        }

        private static int ResolveHighestClearedFloor(FloorMapModel map)
        {
            int highest = 0;
            if (map == null)
                return highest;
            foreach (List<FloorNode> nodes in map.NodesByFloor.Values)
            {
                foreach (FloorNode node in nodes)
                {
                    if (!node.IsSafeZone && node.IsCleared && node.Floor > highest)
                        highest = node.Floor;
                }
            }

            return highest;
        }
    }

    /// <summary>현재 위치 정보.</summary>
    [System.Serializable]
    public sealed class SaveLocation
    {
        /// <summary>마지막 씬 ID.</summary>
        public SceneId SceneId;

        /// <summary>안전지대 안 세부 위치 ID(주점/상점 등).</summary>
        public string SubLocationKey;
    }

    /// <summary>플레이어 직렬화 형태.</summary>
    [System.Serializable]
    public sealed class PlayerSnapshot
    {
        /// <summary>플레이어 이름.</summary>
        public string Name;

        /// <summary>레벨.</summary>
        public int Level;

        /// <summary>현재 EXP.</summary>
        public int Exp;

        /// <summary>HP/MP 등 현재 자원.</summary>
        public StatBlockSnapshot Stats;

        /// <summary>해금된 룬 ID 목록 + 미사용 룬 포인트.</summary>
        public PlayerRuneSnapshot Rune;

        /// <summary>인벤토리(소모/재료 + 장비 별도).</summary>
        public InventorySnapshot Inventory;

        /// <summary>장착 중인 장비 슬롯.</summary>
        public EquipmentSnapshot Equipment;

        /// <summary>소모 4칸 아이템 ID(0=비어있음).</summary>
        public List<int> ConsumableSlots;

        /// <summary>보관함(주점 구매 후 활성).</summary>
        public LockerSnapshot Locker;

        /// <summary>길드에서 구매/획득한 Active 스킬 ID 집합.</summary>
        public List<int> OwnedSkillIds;

        /// <summary>활성 스킬 슬롯 2칸. 0 = 비어있음.</summary>
        public List<int> ActiveSlotSkillIds;
    }

    /// <summary>동료 명부 직렬화.</summary>
    [System.Serializable]
    public sealed class RosterSnapshot
    {
        /// <summary>현재 합류 중인 동료(최대 3).</summary>
        public List<CompanionSnapshot> Active;

        /// <summary>주점/길드에서 해금됐지만 대기 중인 동료.</summary>
        public List<CompanionSnapshot> Bench;
    }

    /// <summary>동료 1명 직렬화.</summary>
    [System.Serializable]
    public sealed class CompanionSnapshot
    {
        /// <summary>동료 데이터 ID.</summary>
        public int CompanionId;

        /// <summary>레벨.</summary>
        public int Level;

        /// <summary>EXP.</summary>
        public int Exp;

        /// <summary>고정 룬 트리 노드 ID 시퀀스(게임 시작 시 시드로 결정, 불변).</summary>
        public List<int> FixedRuneSequence;

        /// <summary>현재까지 해금된 룬 노드 수(레벨업마다 +1).</summary>
        public int UnlockedCount;

        /// <summary>현재 HP/MP/스탯.</summary>
        public StatBlockSnapshot Stats;

        /// <summary>장착 중인 장비.</summary>
        public EquipmentSnapshot Equipment;
    }

    /// <summary>스탯 직렬화.</summary>
    [System.Serializable]
    public sealed class StatBlockSnapshot
    {
        /// <summary>최대 HP.</summary>
        public int MaxHP;

        /// <summary>현재 HP.</summary>
        public int CurrentHP;

        /// <summary>최대 MP.</summary>
        public int MaxMP;

        /// <summary>현재 MP.</summary>
        public int CurrentMP;

        /// <summary>공격력.</summary>
        public int ATK;

        /// <summary>방어력.</summary>
        public int DEF;

        /// <summary>공격속도.</summary>
        public int SPD;
    }

    /// <summary>플레이어 룬 진행 직렬화.</summary>
    [System.Serializable]
    public sealed class PlayerRuneSnapshot
    {
        /// <summary>선택한 시작 직업 룬 ID(딜러/탱커/마법딜러/지원가 중).</summary>
        public int StartingClassRuneId;

        /// <summary>해금된 룬 노드 ID 목록.</summary>
        public List<int> UnlockedNodeIds;

        /// <summary>노드별 투자 포인트 목록.</summary>
        public List<RuneNodeInvestmentSnapshot> NodeInvestments;

        /// <summary>현재 보유 룬 포인트(레벨업으로 적립).</summary>
        public int RunePoints;
    }

    [System.Serializable]
    public sealed class RuneNodeInvestmentSnapshot
    {
        public int NodeId;

        public int InvestedPoints;
    }

    /// <summary>인벤토리 직렬화.</summary>
    [System.Serializable]
    public sealed class InventorySnapshot
    {
        /// <summary>소모/재료 아이템(itemId → 수량).</summary>
        public List<InventoryEntry> StackableItems;

        /// <summary>장비 아이템(강화 단계 포함).</summary>
        public List<EquipItemEntry> EquipItems;
    }

    /// <summary>소모/재료 아이템 1행(itemId + 수량).</summary>
    [System.Serializable]
    public sealed class InventoryEntry
    {
        /// <summary>아이템 ID.</summary>
        public int ItemId;

        /// <summary>수량.</summary>
        public int Count;
    }

    /// <summary>
    /// 장비 아이템 1개(itemId + 강화 단계).
    /// 장비는 Stackable=false이므로 Count 개념 없음.
    /// </summary>
    [System.Serializable]
    public sealed class EquipItemEntry
    {
        /// <summary>아이템 ID.</summary>
        public int ItemId;

        /// <summary>강화 단계(0=기본).</summary>
        public int Enhancement;
    }

    /// <summary>장착 슬롯 직렬화(강화 단계 포함).</summary>
    [System.Serializable]
    public sealed class EquipmentSnapshot
    {
        /// <summary>무기 아이템 ID(0=없음).</summary>
        public int WeaponId;

        /// <summary>무기 강화 단계.</summary>
        public int WeaponEnhancement;

        /// <summary>방어구(몸통) ID.</summary>
        public int ArmorBodyId;

        /// <summary>방어구(몸통) 강화 단계.</summary>
        public int ArmorBodyEnhancement;

        /// <summary>방어구(팔) ID.</summary>
        public int ArmorArmsId;

        /// <summary>방어구(팔) 강화 단계.</summary>
        public int ArmorArmsEnhancement;

        /// <summary>방어구(다리) ID.</summary>
        public int ArmorLegsId;

        /// <summary>방어구(다리) 강화 단계.</summary>
        public int ArmorLegsEnhancement;
    }

    /// <summary>보관함 직렬화.</summary>
    [System.Serializable]
    public sealed class LockerSnapshot
    {
        /// <summary>활성화 여부(주점에서 구매 시 true).</summary>
        public bool Unlocked;

        /// <summary>보관 중인 소모/재료 아이템.</summary>
        public List<InventoryEntry> StackableItems;

        /// <summary>보관 중인 장비 아이템(강화 단계 포함).</summary>
        public List<EquipItemEntry> EquipItems;
    }

    /// <summary>상점 재고 1행 직렬화.</summary>
    [System.Serializable]
    public sealed class ShopStockEntrySnapshot
    {
        public int ItemId;
        public bool Available;
        public int RemainingCount;
        public int InitialCount;
        public int UnitPrice;
        public string UnlockKey;
    }

    /// <summary>
    /// 플로어 맵 직렬화.
    /// seed를 저장해 재생성하지 않고, 저장 시점의 전체 노드 구조를 JSON에 직접 기록한다.
    /// </summary>
    [System.Serializable]
    public sealed class FloorMapSnapshot
    {
        /// <summary>다음 선택 가능 층. FloorMapModel.NextSelectableFloor와 동일.</summary>
        public int NextSelectableFloor;

        /// <summary>
        /// 저장 시점의 전체 노드 목록.
        /// 각 노드는 층, 단계, 난이도, 보스 여부, 클리어 여부, 다음 노드 연결을 포함한다.
        /// </summary>
        public List<FloorNodeSnapshot> Nodes;
    }

    /// <summary>플로어 맵 노드 1개의 직렬화 형태.</summary>
    [System.Serializable]
    public sealed class FloorNodeSnapshot
    {
        /// <summary>전역 고유 노드 ID.</summary>
        public int NodeId;

        /// <summary>층 번호(1~49).</summary>
        public int Floor;

        /// <summary>해당 단계(1~6).</summary>
        public int StageIndex;

        /// <summary>난이도(생성 시 고정).</summary>
        public int Difficulty;

        /// <summary>몬스터 수(1~3, 생성 시 고정).</summary>
        public int MonsterCount;

        /// <summary>보스 노드 여부.</summary>
        public bool IsBoss;

        /// <summary>안전지대 표시 노드 여부.</summary>
        public bool IsSafeZone;

        /// <summary>클리어 여부.</summary>
        public bool IsCleared;

        /// <summary>다음 층의 연결 노드 ID 목록.</summary>
        public List<int> NextNodeIds;
    }

    /// <summary>침식 직렬화.</summary>
    [System.Serializable]
    public sealed class ErosionSnapshot
    {
        /// <summary>
        /// 각 단계의 침식률(단계 1~6). 인덱스 0 = 단계 1.
        /// 0.0 = 비침식, 100.0 = 완전침식.
        /// </summary>
        public List<float> StageRates;

        /// <summary>
        /// 침식 활성화 여부. Safe2 도달 전까지는 false.
        /// </summary>
        public bool ErosionStarted;

        /// <summary>
        /// 현재 침식이 누적되는 단계(1~6).
        /// </summary>
        public int CurrentEroddingStage;

        /// <summary>
        /// 각 단계의 안전지대 침식 잠금 여부(침식률 100%에 도달 시 true).
        /// 인덱스 0 = 단계 1의 안전지대(Safe1).
        /// </summary>
        public List<bool> StageSafeLocked;
    }

    /// <summary>튜토리얼 진행 직렬화.</summary>
    [System.Serializable]
    public sealed class TutorialSnapshot
    {
        /// <summary>완료된 단계 ID 목록.</summary>
        public List<string> CompletedSteps;
    }

    /// <summary>옵션 직렬화.</summary>
    [System.Serializable]
    public sealed class OptionSnapshot
    {
        /// <summary>언어 코드(ko/en 등).</summary>
        public string LanguageCode;

        /// <summary>마스터 볼륨(0~1).</summary>
        public float MasterVolume;

        /// <summary>풀스크린 여부.</summary>
        public bool Fullscreen;

        /// <summary>해상도 가로.</summary>
        public int ResolutionWidth;

        /// <summary>해상도 세로.</summary>
        public int ResolutionHeight;
    }
}

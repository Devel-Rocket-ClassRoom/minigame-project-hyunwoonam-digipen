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
        public static SaveSnapshot FromGameRunStatet(GameRunState run, SceneId sceneId) //Wave0write
        { //Wave0write
            if (run == null) //Wave0write
            { //Wave0write
                return null; //Wave0write
            } //Wave0write

            return new SaveSnapshot //Wave0write
            { //Wave0write
                SavedAtIso = System.DateTime.Now.ToString("o"), //Wave0write
                IsCompleted = false, //Wave0write
                CurrentDay = run.CurrentDay, //Wave0write
                Location = new SaveLocation { SceneId = sceneId, SubLocationKey = string.Empty }, //Wave0write
                FloorMap = FromFloorMap(run.FloorMap), //Wave0write
                Player = FromPlayer(run.Player), //Wave0write
                Roster = FromRoster(run.Roster), //Wave0write
                Erosion = FromErosion(run.Erosion), //Wave0write
                SafeUnlocks = FromSafeUnlocks(run.SafeUnlocks), //Wave0write
                ShopStock = FromShopStock(run.ShopStock), //Wave0write
                Gold = run.Gold, //Wave0write
                ManaStone = run.ManaStone, //Wave0write
                Tutorial = new TutorialSnapshot { CompletedSteps = run.Tutorial != null ? new List<string>(run.Tutorial.CompletedSteps) : new List<string>() }, //Wave0write
                Options = new OptionSnapshot { LanguageCode = "ko", MasterVolume = 1f, Fullscreen = true, ResolutionWidth = 1920, ResolutionHeight = 1080 }, //Wave0write
            }; //Wave0write
        } //Wave0write

        /// <summary>
        /// 저장 스냅샷을 런타임 런 상태로 복원한다.
        /// </summary>
        public GameRunState ToGameRunStatet(DataManager data) //Wave0write
        { //Wave0write
            var run = new GameRunState //Wave0write
            { //Wave0write
                CurrentDay = CurrentDay, //Wave0write
                FloorMap = ToFloorMap(FloorMap), //Wave0write
                Player = ToPlayer(Player, data), //Wave0write
                Roster = ToRoster(Roster), //Wave0write
                Erosion = ToErosion(Erosion), //Wave0write
                SafeUnlocks = ToSafeUnlocks(SafeUnlocks), //Wave0write
                ShopStock = ToShopStock(ShopStock), //Wave0write
                Gold = Gold, //Wave0write
                ManaStone = ManaStone, //Wave0write
                Tutorial = new TutorialProgressState { CompletedSteps = Tutorial?.CompletedSteps != null ? new List<string>(Tutorial.CompletedSteps) : new List<string>() }, //Wave0write
            }; //Wave0write

            run.CurrentFloor = ResolveCurrentFloor(run.FloorMap); //Wave0write
            run.HighestFloor = ResolveHighestClearedFloor(run.FloorMap); //Wave0write
            return run; //Wave0write
        } //Wave0write

        private static FloorMapSnapshot FromFloorMap(FloorMapModel map) //Wave0write
        { //Wave0write
            var snapshot = new FloorMapSnapshot { NextSelectableFloor = map != null ? map.NextSelectableFloor : 0, Nodes = new List<FloorNodeSnapshot>() }; //Wave0write
            if (map == null) //Wave0write
            { //Wave0write
                return snapshot; //Wave0write
            } //Wave0write

            foreach (List<FloorNode> floorNodes in map.NodesByFloor.Values) //Wave0write
            { //Wave0write
                foreach (FloorNode node in floorNodes) //Wave0write
                { //Wave0write
                    snapshot.Nodes.Add(new FloorNodeSnapshot //Wave0write
                    { //Wave0write
                        NodeId = node.NodeId, //Wave0write
                        Floor = node.Floor, //Wave0write
                        StageIndex = node.StageIndex, //Wave0write
                        Difficulty = node.Difficulty, //Wave0write
                        MonsterCount = node.MonsterCount, //Wave0write
                        IsBoss = node.IsBoss, //Wave0write
                        IsSafeZone = node.IsSafeZone, //Wave0write
                        IsCleared = node.IsCleared, //Wave0write
                        NextNodeIds = node.NextNodeIds != null ? new List<int>(node.NextNodeIds) : new List<int>(), //Wave0write
                    }); //Wave0write
                } //Wave0write
            } //Wave0write

            return snapshot; //Wave0write
        } //Wave0write

        private static FloorMapModel ToFloorMap(FloorMapSnapshot snapshot) //Wave0write
        { //Wave0write
            var map = new FloorMapModel(); //Wave0write
            if (snapshot == null || snapshot.Nodes == null) //Wave0write
            { //Wave0write
                return map; //Wave0write
            } //Wave0write

            map.NextSelectableFloor = snapshot.NextSelectableFloor; //Wave0write
            foreach (FloorNodeSnapshot src in snapshot.Nodes) //Wave0write
            { //Wave0write
                var node = new FloorNode //Wave0write
                { //Wave0write
                    NodeId = src.NodeId, //Wave0write
                    Floor = src.Floor, //Wave0write
                    StageIndex = src.StageIndex, //Wave0write
                    Difficulty = src.Difficulty, //Wave0write
                    MonsterCount = src.MonsterCount, //Wave0write
                    IsBoss = src.IsBoss, //Wave0write
                    IsSafeZone = src.IsSafeZone, //Wave0write
                    IsCleared = src.IsCleared, //Wave0write
                    NextNodeIds = src.NextNodeIds != null ? new List<int>(src.NextNodeIds) : new List<int>(), //Wave0write
                }; //Wave0write
                map.NodesById[node.NodeId] = node; //Wave0write
                if (!map.NodesByFloor.TryGetValue(node.Floor, out List<FloorNode> floorNodes)) //Wave0write
                { //Wave0write
                    floorNodes = new List<FloorNode>(); //Wave0write
                    map.NodesByFloor[node.Floor] = floorNodes; //Wave0write
                } //Wave0write

                floorNodes.Add(node); //Wave0write
            } //Wave0write

            return map; //Wave0write
        } //Wave0write

        private static PlayerSnapshot FromPlayer(PlayerState player) //Wave0write
        { //Wave0write
            if (player == null) //Wave0write
            { //Wave0write
                return null; //Wave0write
            } //Wave0write

            return new PlayerSnapshot //Wave0write
            { //Wave0write
                Name = player.Name, //Wave0write
                Level = player.Level, //Wave0write
                Exp = player.Exp, //Wave0write
                Stats = FromStats(player.Stats), //Wave0write
                Rune = new PlayerRuneSnapshot //Wave0write
                { //Wave0write
                    StartingClassRuneId = (int)player.StartingClass, //Wave0write
                    UnlockedNodeIds = player.Rune?.UnlockedIds != null ? new List<int>(player.Rune.UnlockedIds) : new List<int>(), //Wave0write
                    RunePoints = player.Rune != null ? player.Rune.RunePoints : 0, //Wave0write
                }, //Wave0write
                Inventory = FromInventory(player.Inventory), //Wave0write
                Equipment = FromEquipment(player.Equipment), //Wave0write
                ConsumableSlots = player.Consumables != null ? new List<int>(player.Consumables.SlotItemIds) : new List<int> { 0, 0, 0, 0 }, //Wave0write
                Locker = FromLocker(player.Locker), //Wave0write
            }; //Wave0write
        } //Wave0write

        private static PlayerState ToPlayer(PlayerSnapshot snapshot, DataManager data) //Wave0write
        { //Wave0write
            if (snapshot == null) //Wave0write
            { //Wave0write
                return null; //Wave0write
            } //Wave0write

            var player = new PlayerState //Wave0write
            { //Wave0write
                Name = snapshot.Name, //Wave0write
                Level = snapshot.Level <= 0 ? 1 : snapshot.Level, //Wave0write
                Exp = snapshot.Exp, //Wave0write
                Stats = ToStats(snapshot.Stats), //Wave0write
                StartingClass = snapshot.Rune != null ? (RuneClass)snapshot.Rune.StartingClassRuneId : RuneClass.Dealer, //Wave0write
                Inventory = ToInventory(snapshot.Inventory, data), //Wave0write
                Equipment = ToEquipment(snapshot.Equipment, data), //Wave0write
                Consumables = new ConsumableSlots(), //Wave0write
                Locker = ToLocker(snapshot.Locker, data), //Wave0write
            }; //Wave0write

            player.Rune = new PlayerRuneState //Wave0write
            { //Wave0write
                ClassId = player.StartingClass, //Wave0write
                RunePoints = snapshot.Rune != null ? snapshot.Rune.RunePoints : 0, //Wave0write
                UnlockedIds = snapshot.Rune?.UnlockedNodeIds != null ? new HashSet<int>(snapshot.Rune.UnlockedNodeIds) : new HashSet<int>(), //Wave0write
                Tree = data != null ? RuneTree.BuildFromData(player.StartingClass, data.Runes.Values) : null, //Wave0write
            }; //Wave0write

            if (snapshot.ConsumableSlots != null) //Wave0write
            { //Wave0write
                for (int i = 0; i < player.Consumables.SlotItemIds.Length && i < snapshot.ConsumableSlots.Count; i++) //Wave0write
                { //Wave0write
                    player.Consumables.SlotItemIds[i] = snapshot.ConsumableSlots[i]; //Wave0write
                } //Wave0write
            } //Wave0write

            return player; //Wave0write
        } //Wave0write

        private static StatBlockSnapshot FromStats(StatBlock stats) //Wave0write
        { //Wave0write
            return stats == null ? null : new StatBlockSnapshot { MaxHP = stats.MaxHP, CurrentHP = stats.CurrentHP, MaxMP = stats.MaxMP, CurrentMP = stats.CurrentMP, ATK = stats.ATK, DEF = stats.DEF, SPD = stats.SPD }; //Wave0write
        } //Wave0write

        private static StatBlock ToStats(StatBlockSnapshot snapshot) //Wave0write
        { //Wave0write
            var stats = new StatBlock(); //Wave0write
            if (snapshot == null) //Wave0write
            { //Wave0write
                stats.SetBaseStats(80, 20, 10, 2, 10); //Wave0write
                stats.RestoreToFull(); //Wave0write
                return stats; //Wave0write
            } //Wave0write

            stats.SetBaseStats(snapshot.MaxHP, snapshot.MaxMP, snapshot.ATK, snapshot.DEF, snapshot.SPD); //Wave0write
            stats.CurrentHP = snapshot.CurrentHP; //Wave0write
            stats.CurrentMP = snapshot.CurrentMP; //Wave0write
            stats.ClampToMax(); //Wave0write
            return stats; //Wave0write
        } //Wave0write

        private static InventorySnapshot FromInventory(InventoryState inv) //Wave0write
        { //Wave0write
            var snapshot = new InventorySnapshot { StackableItems = new List<InventoryEntry>(), EquipItems = new List<EquipItemEntry>() }; //Wave0write
            if (inv == null) //Wave0write
            { //Wave0write
                return snapshot; //Wave0write
            } //Wave0write

            foreach (KeyValuePair<int, int> entry in inv.StackableItems) //Wave0write
            { //Wave0write
                snapshot.StackableItems.Add(new InventoryEntry { ItemId = entry.Key, Count = entry.Value }); //Wave0write
            } //Wave0write

            foreach (Item item in inv.EquipItems) //Wave0write
            { //Wave0write
                if (item?.Data != null) //Wave0write
                { //Wave0write
                    snapshot.EquipItems.Add(new EquipItemEntry { ItemId = item.Data.Id, Enhancement = item.Enhancement }); //Wave0write
                } //Wave0write
            } //Wave0write

            return snapshot; //Wave0write
        } //Wave0write

        private static InventoryState ToInventory(InventorySnapshot snapshot, DataManager data) //Wave0write
        { //Wave0write
            var inv = new InventoryState(); //Wave0write
            if (snapshot == null) //Wave0write
            { //Wave0write
                return inv; //Wave0write
            } //Wave0write

            if (snapshot.StackableItems != null) //Wave0write
            { //Wave0write
                foreach (InventoryEntry entry in snapshot.StackableItems) //Wave0write
                { //Wave0write
                    inv.StackableItems[entry.ItemId] = entry.Count; //Wave0write
                } //Wave0write
            } //Wave0write

            if (snapshot.EquipItems != null && data != null) //Wave0write
            { //Wave0write
                foreach (EquipItemEntry entry in snapshot.EquipItems) //Wave0write
                { //Wave0write
                    if (data.Items.TryGetValue(entry.ItemId, out ItemData itemData)) //Wave0write
                    { //Wave0write
                        inv.EquipItems.Add(new Item { Data = itemData, Enhancement = entry.Enhancement }); //Wave0write
                    } //Wave0write
                } //Wave0write
            } //Wave0write

            return inv; //Wave0write
        } //Wave0write

        private static EquipmentSnapshot FromEquipment(EquipmentSlots equipment) //Wave0write
        { //Wave0write
            return new EquipmentSnapshot //Wave0write
            { //Wave0write
                WeaponId = equipment?.Weapon?.Data != null ? equipment.Weapon.Data.Id : 0, //Wave0write
                WeaponEnhancement = equipment?.Weapon != null ? equipment.Weapon.Enhancement : 0, //Wave0write
                ArmorBodyId = equipment?.ArmorBody?.Data != null ? equipment.ArmorBody.Data.Id : 0, //Wave0write
                ArmorBodyEnhancement = equipment?.ArmorBody != null ? equipment.ArmorBody.Enhancement : 0, //Wave0write
                ArmorArmsId = equipment?.ArmorArms?.Data != null ? equipment.ArmorArms.Data.Id : 0, //Wave0write
                ArmorArmsEnhancement = equipment?.ArmorArms != null ? equipment.ArmorArms.Enhancement : 0, //Wave0write
                ArmorLegsId = equipment?.ArmorLegs?.Data != null ? equipment.ArmorLegs.Data.Id : 0, //Wave0write
                ArmorLegsEnhancement = equipment?.ArmorLegs != null ? equipment.ArmorLegs.Enhancement : 0, //Wave0write
            }; //Wave0write
        } //Wave0write

        private static EquipmentSlots ToEquipment(EquipmentSnapshot snapshot, DataManager data) //Wave0write
        { //Wave0write
            var equipment = new EquipmentSlots(); //Wave0write
            if (snapshot == null || data == null) //Wave0write
            { //Wave0write
                return equipment; //Wave0write
            } //Wave0write

            equipment.Weapon = CreateItem(snapshot.WeaponId, snapshot.WeaponEnhancement, data); //Wave0write
            equipment.ArmorBody = CreateItem(snapshot.ArmorBodyId, snapshot.ArmorBodyEnhancement, data); //Wave0write
            equipment.ArmorArms = CreateItem(snapshot.ArmorArmsId, snapshot.ArmorArmsEnhancement, data); //Wave0write
            equipment.ArmorLegs = CreateItem(snapshot.ArmorLegsId, snapshot.ArmorLegsEnhancement, data); //Wave0write
            return equipment; //Wave0write
        } //Wave0write

        private static LockerSnapshot FromLocker(LockerState locker) //Wave0write
        { //Wave0write
            var snapshot = new LockerSnapshot { Unlocked = locker != null && locker.Unlocked, StackableItems = new List<InventoryEntry>(), EquipItems = new List<EquipItemEntry>() }; //Wave0write
            if (locker == null) //Wave0write
            { //Wave0write
                return snapshot; //Wave0write
            } //Wave0write

            foreach (KeyValuePair<int, int> entry in locker.StackableItems) //Wave0write
            { //Wave0write
                snapshot.StackableItems.Add(new InventoryEntry { ItemId = entry.Key, Count = entry.Value }); //Wave0write
            } //Wave0write

            foreach (Item item in locker.EquipItems) //Wave0write
            { //Wave0write
                if (item?.Data != null) //Wave0write
                { //Wave0write
                    snapshot.EquipItems.Add(new EquipItemEntry { ItemId = item.Data.Id, Enhancement = item.Enhancement }); //Wave0write
                } //Wave0write
            } //Wave0write

            return snapshot; //Wave0write
        } //Wave0write

        private static LockerState ToLocker(LockerSnapshot snapshot, DataManager data) //Wave0write
        { //Wave0write
            var locker = new LockerState(); //Wave0write
            if (snapshot == null) //Wave0write
            { //Wave0write
                return locker; //Wave0write
            } //Wave0write

            locker.Unlocked = snapshot.Unlocked; //Wave0write
            if (snapshot.StackableItems != null) //Wave0write
            { //Wave0write
                foreach (InventoryEntry entry in snapshot.StackableItems) //Wave0write
                { //Wave0write
                    locker.StackableItems[entry.ItemId] = entry.Count; //Wave0write
                } //Wave0write
            } //Wave0write

            if (snapshot.EquipItems != null && data != null) //Wave0write
            { //Wave0write
                foreach (EquipItemEntry entry in snapshot.EquipItems) //Wave0write
                { //Wave0write
                    Item item = CreateItem(entry.ItemId, entry.Enhancement, data); //Wave0write
                    if (item != null) //Wave0write
                    { //Wave0write
                        locker.EquipItems.Add(item); //Wave0write
                    } //Wave0write
                } //Wave0write
            } //Wave0write

            return locker; //Wave0write
        } //Wave0write

        private static Item CreateItem(int itemId, int enhancement, DataManager data) //Wave0write
        { //Wave0write
            if (itemId == 0 || data == null || !data.Items.TryGetValue(itemId, out ItemData itemData)) //Wave0write
            { //Wave0write
                return null; //Wave0write
            } //Wave0write

            return new Item { Data = itemData, Enhancement = enhancement }; //Wave0write
        } //Wave0write

        private static RosterSnapshot FromRoster(CompanionRosterState roster) //Wave0write
        { //Wave0write
            var snapshot = new RosterSnapshot { Active = new List<CompanionSnapshot>(), Bench = new List<CompanionSnapshot>() }; //Wave0write
            if (roster == null) //Wave0write
            { //Wave0write
                return snapshot; //Wave0write
            } //Wave0write

            foreach (CompanionInstance inst in roster.Active) snapshot.Active.Add(FromCompanion(inst)); //Wave0write
            foreach (CompanionInstance inst in roster.Bench) snapshot.Bench.Add(FromCompanion(inst)); //Wave0write
            return snapshot; //Wave0write
        } //Wave0write

        private static CompanionRosterState ToRoster(RosterSnapshot snapshot) //Wave0write
        { //Wave0write
            var roster = new CompanionRosterState(); //Wave0write
            if (snapshot == null) //Wave0write
            { //Wave0write
                return roster; //Wave0write
            } //Wave0write

            if (snapshot.Active != null) foreach (CompanionSnapshot src in snapshot.Active) roster.Active.Add(ToCompanion(src)); //Wave0write
            if (snapshot.Bench != null) foreach (CompanionSnapshot src in snapshot.Bench) roster.Bench.Add(ToCompanion(src)); //Wave0write
            return roster; //Wave0write
        } //Wave0write

        private static CompanionSnapshot FromCompanion(CompanionInstance inst) //Wave0write
        { //Wave0write
            return new CompanionSnapshot //Wave0write
            { //Wave0write
                CompanionId = inst.CompanionDataId, //Wave0write
                Level = inst.Level, //Wave0write
                Exp = inst.Exp, //Wave0write
                FixedRuneSequence = inst.Rune?.FixedSequence != null ? new List<int>(inst.Rune.FixedSequence) : new List<int>(), //Wave0write
                UnlockedCount = inst.Rune != null ? inst.Rune.UnlockedCount : 0, //Wave0write
                Stats = FromStats(inst.Stats), //Wave0write
                Equipment = FromEquipment(inst.Equipment), //Wave0write
            }; //Wave0write
        } //Wave0write

        private static CompanionInstance ToCompanion(CompanionSnapshot src) //Wave0write
        { //Wave0write
            return new CompanionInstance //Wave0write
            { //Wave0write
                CompanionDataId = src.CompanionId, //Wave0write
                Level = src.Level <= 0 ? 1 : src.Level, //Wave0write
                Exp = src.Exp, //Wave0write
                Stats = ToStats(src.Stats), //Wave0write
                Rune = new CompanionRuneState { FixedSequence = src.FixedRuneSequence != null ? new List<int>(src.FixedRuneSequence) : new List<int>(), UnlockedCount = src.UnlockedCount }, //Wave0write
                Equipment = new EquipmentSlots(), //Wave0write
            }; //Wave0write
        } //Wave0write

        private static ErosionSnapshot FromErosion(ErosionStateModel model) //Wave0write
        { //Wave0write
            var snapshot = new ErosionSnapshot { StageRates = new List<float>(), ErosionStarted = model != null && model.IsActivated, StageSafeLocked = new List<bool>() }; //Wave0write
            for (int i = 1; i <= 6; i++) //Wave0write
            { //Wave0write
                float rate = model != null ? model.GetRate(i) : 0f; //Wave0write
                snapshot.StageRates.Add(rate); //Wave0write
                snapshot.StageSafeLocked.Add(rate >= 100f); //Wave0write
            } //Wave0write

            return snapshot; //Wave0write
        } //Wave0write

        private static ErosionStateModel ToErosion(ErosionSnapshot snapshot) //Wave0write
        { //Wave0write
            var model = new ErosionStateModel { IsActivated = snapshot != null && snapshot.ErosionStarted, CurrentEroddingStage = 1 }; //Wave0write
            bool foundCurrentStage = false; //Wave0write
            for (int i = 1; i <= 6; i++) //Wave0write
            { //Wave0write
                int listIndex = i - 1; //Wave0write
                model.StageRates[i] = snapshot?.StageRates != null && listIndex < snapshot.StageRates.Count ? snapshot.StageRates[listIndex] : 0f; //Wave0write
                if (!foundCurrentStage && model.StageRates[i] < 100f) //Wave0write
                { //Wave0write
                    model.CurrentEroddingStage = i; //Wave0write
                    foundCurrentStage = true; //Wave0write
                } //Wave0write
            } //Wave0write

            return model; //Wave0write
        } //Wave0write

        private static List<bool> FromSafeUnlocks(SafeZoneUnlockState state) //Wave0write
        { //Wave0write
            var result = new List<bool>(); //Wave0write
            for (int i = 0; i < 6; i++) //Wave0write
            { //Wave0write
                result.Add(state != null && state.IsUnlocked(i)); //Wave0write
            } //Wave0write

            return result; //Wave0write
        } //Wave0write

        private static SafeZoneUnlockState ToSafeUnlocks(List<bool> values) //Wave0write
        { //Wave0write
            var state = new SafeZoneUnlockState(); //Wave0write
            for (int i = 0; i < state.Unlocked.Length; i++) //Wave0write
            { //Wave0write
                state.Unlocked[i] = values != null && i < values.Count && values[i]; //Wave0write
            } //Wave0write

            return state; //Wave0write
        } //Wave0write

        private static List<ShopStockEntrySnapshot> FromShopStock(ShopStockState state) //Wave0write
        { //Wave0write
            var result = new List<ShopStockEntrySnapshot>(); //Wave0write
            if (state?.Entries == null) //Wave0write
            { //Wave0write
                return result; //Wave0write
            } //Wave0write

            foreach (ShopStockEntry entry in state.Entries) //Wave0write
            { //Wave0write
                if (entry == null) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                result.Add(new ShopStockEntrySnapshot //Wave0write
                { //Wave0write
                    ItemId = entry.ItemId, //Wave0write
                    Available = entry.Available, //Wave0write
                    RemainingCount = entry.RemainingCount, //Wave0write
                    InitialCount = entry.InitialCount, //Wave0write
                    UnitPrice = entry.UnitPrice, //Wave0write
                    UnlockKey = entry.UnlockKey, //Wave0write
                }); //Wave0write
            } //Wave0write

            return result; //Wave0write
        } //Wave0write

        private static ShopStockState ToShopStock(List<ShopStockEntrySnapshot> snapshot) //Wave0write
        { //Wave0write
            var state = new ShopStockState { Entries = new List<ShopStockEntry>() }; //Wave0write
            if (snapshot != null) //Wave0write
            { //Wave0write
                foreach (ShopStockEntrySnapshot src in snapshot) //Wave0write
                { //Wave0write
                    if (src == null) //Wave0write
                    { //Wave0write
                        continue; //Wave0write
                    } //Wave0write

                    state.Entries.Add(new ShopStockEntry //Wave0write
                    { //Wave0write
                        ItemId = src.ItemId, //Wave0write
                        Available = src.Available, //Wave0write
                        RemainingCount = src.RemainingCount, //Wave0write
                        InitialCount = src.InitialCount, //Wave0write
                        UnitPrice = src.UnitPrice, //Wave0write
                        UnlockKey = src.UnlockKey, //Wave0write
                    }); //Wave0write
                } //Wave0write
            } //Wave0write

            state.EnsureDefaultSafe1Stock(); //Wave0write
            return state; //Wave0write
        } //Wave0write

        private static int ResolveCurrentFloor(FloorMapModel map) //Wave0write
        { //Wave0write
            return map != null ? System.Math.Max(0, map.NextSelectableFloor - 1) : 0; //Wave0write
        } //Wave0write

        private static int ResolveHighestClearedFloor(FloorMapModel map) //Wave0write
        { //Wave0write
            int highest = 0; //Wave0write
            if (map == null) return highest; //Wave0write
            foreach (List<FloorNode> nodes in map.NodesByFloor.Values) //Wave0write
            { //Wave0write
                foreach (FloorNode node in nodes) //Wave0write
                { //Wave0write
                    if (!node.IsSafeZone && node.IsCleared && node.Floor > highest) highest = node.Floor; //Wave0write
                } //Wave0write
            } //Wave0write

            return highest; //Wave0write
        } //Wave0write
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

        /// <summary>현재 보유 룬 포인트(레벨업으로 적립).</summary>
        public int RunePoints;
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


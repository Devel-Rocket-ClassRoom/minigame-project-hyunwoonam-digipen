using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 게임 전체 최상위 싱글톤. 하위 시스템 초기화, 런 데이터 보관, 씬 흐름의 게이트웨이 역할.
    /// 직접 로직은 하지 않고 하위 시스템 호출만 한다.
    /// </summary>
    public sealed class GameSystemManager : Singleton<GameSystemManager>
    {
        /// <summary>정적 데이터 로더.</summary>
        public DataManager Data { get; private set; }

        /// <summary>씬 전환 매니저.</summary>
        public GameSceneManager Scenes { get; private set; }

        /// <summary>세이브/이어하기/기록 저장 매니저.</summary>
        public SaveLoader Save { get; private set; }

        /// <summary>전체 글로벌 단축키 입력 라우터.</summary>
        public HotkeyManager Hotkey { get; private set; }

        /// <summary>침식 시스템.</summary>
        public ErosionSystem Erosion { get; private set; }

        /// <summary>이벤트 버스 (레벨업/침식/EXP 등 도메인 이벤트 발행).</summary>
        public EventBus Events { get; private set; }

        /// <summary>현재 진행 중인 런 상태 (없으면 null).</summary>
        public GameRunState CurrentRun { get; private set; }

        /// <summary>현재 전투 진입 컨텍스트 (전투 중 아니면 null).</summary>
        public CombatContext CombatContext { get; private set; }

        // Guid3 §9.E 2026-05-27: 길드/인벤토리/스탯 화면 등 전투 외 시점에서도
        // Player MonoBehaviour 의 권위 출처가 필요. CombatController.OnEnter 가 set, OnExit 가 null.
        // 전투 외에서 Player MonoBehaviour 가 없으면 null 일 수 있다(Guid3 §10 W-G3-1 보류).
        /// <summary>현재 활성 Player MonoBehaviour. 전투 외에서는 null 일 수 있다.</summary>
        public Player ActivePlayer { get; set; }

        private bool floorMapRechallengeRequested;
        private int floorMapRechallengeMaxFloor;
        private int floorMapRechallengeReturnSafeIndex;
        private bool floorMapSafeTravelRequested;
        private int floorMapSafeTravelSourceSafeIndex;
        private const int SafeIndexForErosionStart = 2;

        /// <summary>
        /// 게임 부팅 시 호출. 하위 시스템 초기화 순서를 강제한다.
        /// </summary>
        protected override void Awake()
        {
            // 동작 요약:
            // - 1) base.Awake() 호출(싱글톤 등록).
            // - 2) Events = new EventBus() — 이벤트 버스 최우선.
            // - 3) Data = new DataManager(); Data.LoadAll() — CSV/JSON 일괄 로드.
            // - 4) Save = new SaveLoader(); Save.LoadAll() — Continue/기록 로드.
            // - 5) Scenes = GetComponentInChildren<GameSceneManager>() 또는 신규 추가.
            // - 6) Hotkey = new HotkeyManager(); Hotkey.BindGlobalKeys().
            // - 7) Erosion 은 런 생성/로드 시 CurrentRun.Erosion 에 연결.
            // - 8) DontDestroyOnLoad(this.gameObject)는 base.Awake()에서 처리.
            // - 9) Scenes.LoadMainMenu().

            base.Awake();

            if (!IsSingletonInstance)
            {
                return;
            }

            //초기화

            Events = new EventBus();

            Data = new DataManager();
            Data.LoadAll();

            Save = new SaveLoader();
            Save.LoadAll();

            Scenes = GetComponentInChildren<GameSceneManager>();
            if (Scenes == null)
            {
                Scenes = gameObject.AddComponent<GameSceneManager>();
            }

            Hotkey = new HotkeyManager();
            Hotkey.BindGlobalKeys();
        }

        /// <summary>
        /// 싱글톤 인스턴스 파괴 시 전역 참조를 정리한다.
        /// 저장은 명시적 체크포인트(Save.SaveSnapshot, QuitGame, EndCombat 등)에서 처리하고 여기서는 수행하지 않는다.
        /// </summary>
        protected override void OnDestroy()
        {
            if (IsSingletonInstance == true)
            {
                // 1. 전역 입력/이벤트 구독 해제
                // 2. 진행 중인 임시 컨텍스트 정리
                // 3. 런 상태 참조 정리
                // 4. 시스템 참조 정리

                // 현재 HotkeyManagert에는 Unbind 메서드가 없으므로,
                // 나중에 생기면 여기서 호출.
                // Hotkey?.UnbindGlobalKeys();

                UnsubscribeErosionEvents();
                CombatContext = null;
                CurrentRun = null;
                Erosion = null;
                Hotkey = null;
                Scenes = null;
                Save = null;
                Data = null;
                Events = null;
            }

            base.OnDestroy();
        }

        private void Update()
        {
            if (!IsSingletonInstance)
            {
                return;
            }

            Hotkey?.PollInput();
        }


        /// <summary>
        /// 새 게임을 시작한다. 메인 메뉴의 New Game 버튼에서 호출.
        /// 새 게임 맵은 여기서 1회 생성하고, 이후 저장/로드는 seed 재생성이 아니라 전체 맵 스냅샷을 사용한다.
        /// </summary>
        public void StartNewGame()
        {
            // 동작 요약:
            // - Save.HasContinue()가 true면 호출 전 메인메뉴 측에서 확인 팝업을 띄웠어야 함.
            // - CurrentRun = new GameRunState(); 새 런 데이터 생성.
            // - FloorMapCreator.Generate(Data.World)로 전체 맵을 1회 생성 후 CurrentRun.FloorMap에 보관.
            // - 생성된 FloorMap은 저장 시 SaveSnapshot.FloorMap.Nodes로 전체 구조가 직렬화됨.
            // - 시작 룬 선택 페이지를 위해 Safe0 진입 요청.
            // - 침식 상태 초기화.

            if (Save != null && Save.HasContinue() == true)
            {
                Save.ClearContinue();
            }

            CurrentRun = new GameRunState();

            CurrentRun.FloorMap = FloorMapCreator.Generate(Data.World);

            CurrentRun.CurrentDay = 0;
            Events?.RaiseDayChanged(CurrentRun.CurrentDay);
            CurrentRun.CurrentFloor = 0;
            CurrentRun.HighestFloor = 0;

            CurrentRun.Player = CreateInitialPlayerState();
            CurrentRun.Roster = new CompanionRosterState();

            CurrentRun.Erosion = new ErosionStateModel();
            CurrentRun.SafeUnlocks = new SafeZoneUnlockState();
            CurrentRun.SafeUnlocks.Unlock(0);
            CurrentRun.ShopStock = ShopStockState.CreateDefaultSafe1Stock();

            CurrentRun.Gold = 0;
            CurrentRun.ManaStone = 0;
            CurrentRun.Tutorial = new TutorialProgressState();

            AttachErosionToCurrentRun();

            CombatContext = null;
            Scenes.LoadSafeZone(0);
        }

        /// <summary>
        /// 새 런의 플레이어 초기 상태를 생성한다.
        /// 실제 시작 직업/룬 선택은 Safe0의 시작 룬 선택 흐름에서 확정한다.
        /// </summary>
        private PlayerState CreateInitialPlayerState()
        {
            // 동작 요약:
            // - PlayerState 생성.
            // - 기본 이름, 레벨, EXP, StatBlock 초기값 설정.
            // - PlayerRuneState, InventoryState, EquipmentSlots, ConsumableSlots, LockerState 초기화.
            // - 시작 직업 룬은 Safe0 선택 결과를 반영할 수 있도록 미정 상태로 둔다.
            // - 완성 후 CurrentRun.Player에 저장할 PlayerState 반환.
            var stats = new StatBlock(); //Wave0write
            stats.SetBaseStats(90, 20, 10, 2, 10); //Wave0write
            stats.RestoreToFull(); //Wave0write

            var player = new PlayerState
            { 
                Name = "Player", 
                Level = 1, //Wave0write
                Exp = 0, //Wave0write
                Stats = stats, //Wave0write
                StartingClass = RuneClass.None, //Wave0write
                Rune = new PlayerRuneState { ClassId = RuneClass.None, RunePoints = 0, UnlockedIds = new System.Collections.Generic.HashSet<int>() }, //Wave0write
                Inventory = new InventoryState(), //Wave0write
                Equipment = new EquipmentSlots(), //Wave0write
                Consumables = new ConsumableSlots(), //Wave0write
                Locker = new LockerState(), //Wave0write
            }; //Wave0write

            player.Inventory.Add(1, 2); //Wave0write
            player.Inventory.Add(3, 1); //Wave0write
            AddStartingEquipment(player.Inventory, 901); //Wave0write
            AddStartingEquipment(player.Inventory, 902); //Wave0write
            AddStartingEquipment(player.Inventory, 903); //Wave0write
            AddStartingEquipment(player.Inventory, 904); //Wave0write
            player.Consumables.SlotItemIds[0] = 1; //Wave0write
            player.Consumables.SlotItemIds[1] = 3; //Wave0write
            return player; //Wave0write
        }

        private void AddStartingEquipment(InventoryState inventory, int itemId) //Wave0write
        { //Wave0write
            if (inventory == null || Data?.Items == null || !Data.Items.TryGetValue(itemId, out ItemData itemData)) //Wave0write
            { //Wave0write
                Debug.LogError("[GameSystemManager] 시작 테스트 장비 ID 없음: " + itemId); //Wave0write
                return; //Wave0write
            } //Wave0write

            if (itemData.Category != ItemCategory.Equipment || itemData.Stackable) //Wave0write
            { //Wave0write
                Debug.LogError("[GameSystemManager] 시작 테스트 장비가 Equipment가 아닙니다: " + itemId); //Wave0write
                return; //Wave0write
            } //Wave0write

            inventory.AddEquip(new Item { Data = itemData, Enhancement = 0 }); //Wave0write
        } //Wave0write

        /// <summary>
        /// 이어하기. 메인 메뉴의 Continue 버튼에서 호출.
        /// 저장된 JSON 스냅샷을 런타임 상태로 복원하며, 플로어 맵은 저장된 전체 노드 목록으로 재구성한다.
        /// </summary>
        public void ContinueGame()
        {
            // 동작 요약:
            // - Save.HasContinue()가 false면 메인 메뉴에 머물거나 Continue 버튼 측에서 차단.
            // - Save.Continue에서 SaveSnapshott를 가져온다.
            // - snapshot.FloorMap.Nodes를 순회해 FloorMapModel.NodesById와 NodesByFloor를 직접 복원.
            // - FloorMapCreator.Generate()는 호출하지 않음(seed 재생성 금지).
            // - Player, Roster, Erosion, SafeUnlocks, Gold, ManaStone, Tutorial을 snapshot에서 복원.
            // - Erosion = new ErosionSystem(CurrentRun.Erosion, Events)로 런 상태 모델에 다시 연결.
            // - CombatContext = null로 초기화.
            // - snapshot.Location.SceneId로 Scenes.RequestScene() 호출.
            if (Save == null || !Save.HasContinue()) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            CurrentRun = Save.Continue.ToGameRunStatet(Data); //Wave0write
            AttachErosionToCurrentRun(); //Wave0write
            CombatContext = null; //Wave0write
            SceneId sceneId = Save.Continue.Location != null ? Save.Continue.Location.SceneId : SceneId.Safe0; //Wave0write
            Scenes.RequestScene(sceneId); //Wave0write

        }

        /// <summary>
        /// 전투 노드 진입. FloorMapControllert가 호출.
        /// </summary>
        /// <param name="node">선택된 노드.</param>
        /// <param name="isRechallenget">재도전 모드 여부.</param>
        public void StartCombatNode(FloorNode node, bool isRechallenget)
        {
            // 동작 요약:
            // - node null, CurrentRun null, 이미 클리어된 노드 등 진입 불가 조건 검사.
            // - CombatContext = new CombatContext { Node = node, IsBossNode = node.IsBoss, IsRechallenge = isRechallenget }.
            // - currentDay += 1 (Erosion 진행 트리거).
            // - CurrentFloor, HighestFloor를 node.Floor 기준으로 갱신.
            // - Erosion.AdvanceDay(currentDay).
            // - Scenes.LoadCombat().
            if (CurrentRun == null || node == null || CurrentRun.FloorMap == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (node.IsSafeZone) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            NormalizeCombatNodeMonsterCount(node); //Wave0write

            int rechallengeMaxFloor = floorMapRechallengeMaxFloor; //Wave0write
            int rechallengeReturnSafeIndex = floorMapRechallengeReturnSafeIndex; //Wave0write
            bool rechallengeSelectable = isRechallenget && node.Floor > 0 && node.Floor <= rechallengeMaxFloor; //Wave0write
            bool selectable = isRechallenget ? rechallengeSelectable : node.Floor == CurrentRun.FloorMap.NextSelectableFloor; //Wave0write
            if (!selectable || (!isRechallenget && node.IsCleared)) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            AdvanceRunDay(); //Wave0write
            CurrentRun.CurrentFloor = node.Floor; //Wave0write
            CurrentRun.HighestFloor = System.Math.Max(CurrentRun.HighestFloor, node.Floor); //Wave0write

            CombatContext = new CombatContext //Wave0write
            { //Wave0write
                Node = node, //Wave0write
                IsBossNode = node.IsBoss, //Wave0write
                IsRechallenge = isRechallenget, //Wave0write
                RechallengeReturnSafeIndex = isRechallenget ? rechallengeReturnSafeIndex : 0, //Wave0write
            }; //Wave0write

            ClearFloorMapRechallengeState(); //Wave0write

            Save?.SaveSnapshot(); //Wave0write
            Scenes.LoadCombat(); //Wave0write

        }

        /// <summary>안전지대에서 안전지대 이동 가능 상태로 플로어맵을 연다.</summary>
        public void LoadFloorMapFromSafe(int safeIndex)
        {
            ArmFloorMapSafeTravel(safeIndex);
            if (CanOpenRechallengeFromSafe(safeIndex, out int maxFloor))
            {
                floorMapRechallengeRequested = true;
                floorMapRechallengeMaxFloor = maxFloor;
                floorMapRechallengeReturnSafeIndex = System.Math.Max(0, System.Math.Min(5, safeIndex));
            }
            else
            {
                floorMapRechallengeRequested = false;
                floorMapRechallengeMaxFloor = 0;
                floorMapRechallengeReturnSafeIndex = 0;
            }

            Scenes.LoadFloorMap();
        }

        /// <summary>안전지대에서 아래 단계 재도전용 플로어맵을 연다.</summary>
        public void LoadFloorMapForRechallengeFromSafe(int safeIndex)
        {
            ArmFloorMapSafeTravel(safeIndex);
            floorMapRechallengeRequested = true;
            floorMapRechallengeMaxFloor = ResolveRechallengeMaxFloor(safeIndex);
            floorMapRechallengeReturnSafeIndex = System.Math.Max(0, System.Math.Min(5, safeIndex));
            Scenes.LoadFloorMap();
        }

        /// <summary>플로어맵 컨트롤러가 안전지대 이동 가능 요청을 1회 소비한다.</summary>
        public bool TryConsumeFloorMapSafeTravel(out int sourceSafeIndex)
        {
            sourceSafeIndex = floorMapSafeTravelRequested ? floorMapSafeTravelSourceSafeIndex : 0;
            bool requested = floorMapSafeTravelRequested;
            floorMapSafeTravelRequested = false;
            return requested;
        }

        /// <summary>플로어맵 컨트롤러가 재도전 표시 요청을 1회 소비한다.</summary>
        public bool TryConsumeFloorMapRechallenge(out int maxFloor, out int returnSafeIndex)
        {
            maxFloor = floorMapRechallengeRequested ? floorMapRechallengeMaxFloor : 0;
            returnSafeIndex = floorMapRechallengeRequested ? floorMapRechallengeReturnSafeIndex : 0;
            bool requested = floorMapRechallengeRequested && maxFloor > 0;
            floorMapRechallengeRequested = false;
            return requested;
        }

        public bool TryConsumeFloorMapRechallenge(out int maxFloor)
        {
            return TryConsumeFloorMapRechallenge(out maxFloor, out _);
        }

        /// <summary>플로어맵의 안전지대 노드에서 안전지대로 이동한다. 안전지대 간 이동은 날짜를 증가시키지 않는다.</summary>
        public void EnterSafeZoneFromFloorMap(int safeIndex) //Wave0write
        { //Wave0write
            if (CurrentRun?.SafeUnlocks == null || Scenes == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (safeIndex < 0 || safeIndex > 5 || !CurrentRun.SafeUnlocks.IsUnlocked(safeIndex)) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            CurrentRun.CurrentFloor = ResolveSafeZoneFloor(safeIndex); //Wave0write
            Save?.SaveSnapshot(); //Wave0write
            ClearFloorMapRechallengeState(); //Wave0write
            Scenes.LoadSafeZone(safeIndex); //Wave0write
        } //Wave0write

        private void ClearFloorMapRechallengeState()
        {
            floorMapRechallengeRequested = false;
            floorMapRechallengeMaxFloor = 0;
            floorMapRechallengeReturnSafeIndex = 0;
            floorMapSafeTravelRequested = false;
            floorMapSafeTravelSourceSafeIndex = 0;
        }

        private void ArmFloorMapSafeTravel(int safeIndex)
        {
            floorMapSafeTravelRequested = true;
            floorMapSafeTravelSourceSafeIndex = System.Math.Max(0, System.Math.Min(5, safeIndex));
        }

        private int ResolveRechallengeMaxFloor(int safeIndex)
        {
            if (Data?.World?.Stages != null)
            {
                for (int i = 0; i < Data.World.Stages.Count; i++)
                {
                    StageDef stage = Data.World.Stages[i];
                    if (stage != null && stage.UnlocksSafeZoneIndex == safeIndex)
                    {
                        return stage.BossFloor;
                    }
                }
            }

            return safeIndex <= 1 ? 3 : 0;
        }

        private bool CanOpenRechallengeFromSafe(int safeIndex, out int maxFloor)
        {
            maxFloor = 0;
            if (CurrentRun?.FloorMap == null || CurrentRun.SafeUnlocks == null || safeIndex <= 0)
            {
                return false;
            }

            if (!CurrentRun.SafeUnlocks.IsUnlocked(safeIndex))
            {
                return false;
            }

            int safeFloor = ResolveSafeZoneFloor(safeIndex);
            if (CurrentRun.CurrentFloor != safeFloor)
            {
                return false;
            }

            maxFloor = ResolveRechallengeMaxFloor(safeIndex);
            if (maxFloor <= 0)
            {
                return false;
            }

            int clearedStage = StageIndexResolver.FromFloor(maxFloor, Data?.World);
            return CurrentRun.FloorMap.IsStageCleared(clearedStage);
        }

        private void AdvanceRunDay() //Wave0write
        { //Wave0write
            if (CurrentRun == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            CurrentRun.CurrentDay += 1; //Wave0write
            Events?.RaiseDayChanged(CurrentRun.CurrentDay); //Wave0write
            Erosion?.AdvanceDay(CurrentRun.CurrentDay); //Wave0write
        } //Wave0write

        private void AdvanceDayForBossSafeEntry() //Wave0write
        { //Wave0write
            AdvanceRunDay(); //Wave0write
        } //Wave0write

        private int ResolveSafeZoneFloor(int safeIndex) //Wave0write
        { //Wave0write
            if (Data?.World?.SafeZones != null) //Wave0write
            { //Wave0write
                for (int i = 0; i < Data.World.SafeZones.Count; i++) //Wave0write
                { //Wave0write
                    SafeZoneDef safeZone = Data.World.SafeZones[i]; //Wave0write
                    if (safeZone != null && safeZone.Index == safeIndex) //Wave0write
                    { //Wave0write
                        return safeZone.FloorNumber; //Wave0write
                    } //Wave0write
                } //Wave0write
            } //Wave0write

            return 0; //Wave0write
        } //Wave0write

        private static void NormalizeCombatNodeMonsterCount(FloorNode node) //Wave0write
        { //Wave0write
            if (node == null || node.IsSafeZone) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (node.IsBoss || node.Floor == 1) //Wave0write
            { //Wave0write
                node.MonsterCount = 1; //Wave0write
            } //Wave0write
            else if (node.Floor == 2) //Wave0write
            { //Wave0write
                node.MonsterCount = 2; //Wave0write
            } //Wave0write
        } //Wave0write

        /// <summary>
        /// 전투 종료 분기. CombatControllert가 결과를 가지고 호출.
        /// Victory 시 보상 집계 → 인벤토리 삽입 → CombatRewardPage 표시 순서로 처리.
        /// </summary>
        /// <param name="result">전투 결과.</param>
        /// <param name="controller">보상 집계용 컨트롤러 참조.</param>
        public void EndCombat(CombatResult result, CombatController controller)
        {
            // 동작 요약:
            //
            // [Defeat]
            // - Save.AppendGrave(CurrentRun.Player.Name, System.DateTime.Now) — 묘비 영구 등록.
            // - CurrentRun = null.
            // - CombatContext = null.
            // - Scenes.LoadMainMenu().
            // - return.
            //
            // [Victory]
            // 1) 보상 집계
            //    - NodeRewardSummary summary = controller.CollectNodeRewards().
            //
            // 2) EXP 처리
            //    - 플레이어와 동료는 전투 중 몬스터 처치 시 각자 자동으로 EXP 습득.
            //      (EntityBase.OnKill 훅 또는 DamageCalculator 처리 — 별도 분배 없음)
            //    - 여기서는 추가 EXP 지급 없음.
            //
            // 3) 골드 추가
            //    - CurrentRun.Gold += summary.TotalGold.
            //
            // 4) 아이템 인벤토리 삽입 (오버플로우 분리)
            //    - InventoryState inv = CurrentRun.Player.Inventory.
            //    - List<int> overflowIds = new List<int>().
            //    - summary.DroppedItemIds 순회:
            //        * ItemData data = Data.Items[itemId].
            //        * data.IsStackable → inv.TryAdd(itemId, 1) 실패 시 overflowIds.Add(itemId).
            //        * !data.IsStackable → new Item(data) 생성 후 inv.TryAddEquip() 실패 시 overflowIds.Add(itemId).
            //
            // 5) 보상 UI 표시 (항상 표시, 오버플로우 있을 때는 버리기 UI 함께)
            //    - controller.Hud.RewardPage.Show(summary, overflowIds, OnRewardClosed).
            //      (CombatHudt가 CombatRewardPaget를 자식으로 보유)
            //
            // 6) [OnRewardClosed 콜백 내부]
            //    - CurrentRun.FloorMap.MarkCleared(CombatContext.Node.NodeId).
            //    - isBossNode → SafeUnlocks.Unlock(단계), Scenes.LoadSafeZone(단계).
            //    - isRechallenge → Scenes.LoadSafeZone(현재 단계).
            //    - 일반 → Scenes.LoadFloorMap().
            //    - CombatContext = null.
            //    - Save.SaveSnapshot()으로 CurrentRun.FloorMap의 전체 노드 구조와 다른 런 상태를 JSON에 함께 저장.
            if (CurrentRun == null) //Wave0write
            { //Wave0write
                CombatContext = null; //Wave0write
                Scenes.LoadMainMenu(); //Wave0write
                return; //Wave0write
            } //Wave0write

            if (result == CombatResult.Defeat) //Wave0write
            { //Wave0write
                Save?.AppendGrave(CurrentRun.Player != null ? CurrentRun.Player.Name : "Player", System.DateTime.Now); //Wave0write
                Save?.ClearContinue(); //Wave0write
                CurrentRun = null; //Wave0write
                CombatContext = null; //Wave0write
                ShowGameOverOverlay(); //Wave0write
                Scenes.LoadMainMenu(); //Wave0write
                return; //Wave0write
            } //Wave0write

            if (result == CombatResult.Retreat) //Wave0write
            { //Wave0write
                int safeIndex = CombatContext != null ? System.Math.Max(0, CombatContext.Node.StageIndex - 1) : 0; //Wave0write
                CombatContext = null; //Wave0write
                Save?.SaveSnapshot(); //Wave0write
                Scenes.LoadSafeZone(safeIndex); //Wave0write
                return; //Wave0write
            } //Wave0write

            NodeRewardSummary summary = controller != null ? controller.CollectNodeRewards() : new NodeRewardSummary(); //Wave0write
            if (summary == null) //Wave0write
            { //Wave0write
                summary = new NodeRewardSummary(); //Wave0write
            } //Wave0write

            AddExpToPlayerState(summary.TotalExp); //Wave0write
            CurrentRun.Gold += summary.TotalGold; //Wave0write
            Events?.RaiseGoldChanged(CurrentRun.Gold); //Wave0write

            var overflowIds = new System.Collections.Generic.List<int>(); //Wave0write
            GrantDroppedItems(summary, overflowIds); //Wave0write

            System.Action closeAction = () => FinishVictoryAfterReward(); //Wave0write
            if (controller?.Hud?.RewardPage != null) //Wave0write
            { //Wave0write
                controller.Hud.RewardPage.Show(summary, overflowIds, closeAction); //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                closeAction(); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 클리어 처리. 49층 최종 보스 처치 시 호출.
        /// </summary>
        public void CompleteRun()
        {
            // 동작 요약:
            // - 비석에 캐릭터 이름 + 일시 영구 기록(Save.AppendClearRecord(CurrentRun.Player.Name, System.DateTime.Now)).
            // - 필요하면 현재 SaveSnapshott를 IsCompleted=true로 저장하거나 Continue를 삭제하는 정책을 SaveLoadert에서 처리.
            // - CurrentRun 정리.
            // - CombatContext 정리.
            // - Scenes.LoadMainMenu().
            if (CurrentRun != null) //Wave0write
            { //Wave0write
                Save?.AppendClearRecord(CurrentRun.Player != null ? CurrentRun.Player.Name : "Player", System.DateTime.Now); //Wave0write
            } //Wave0write

            Save?.ClearContinue(); //Wave0write
            CurrentRun = null; //Wave0write
            CombatContext = null; //Wave0write
            Scenes.LoadMainMenu(); //Wave0write

        }

        /// <summary>
        /// 게임 종료. ESC 확인 팝업의 Yes에서 호출.
        /// </summary>
        public void QuitGame()
        {
            // 동작 요약:
            // - CurrentRun이 있으면 Save.SaveSnapshot()으로 자동 저장.
            // - Save.SaveSnapshot()은 FloorMap 전체 구조, 위치, 플레이어, 동료, 침식, 자원 상태를 하나의 JSON으로 저장.
            // - Application.Quit().
            if (CurrentRun != null) //Wave0write
            { //Wave0write
                Save?.SaveSnapshot(); //Wave0write
            } //Wave0write

            Application.Quit(); //Wave0write

        }

        private void AddExpToPlayerState(int amount) //Wave0write
        { //Wave0write
            if (CurrentRun?.Player == null || amount <= 0) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            CurrentRun.Player.Exp += amount; //Wave0write
            int required = RequiredExpForLevel(CurrentRun.Player.Level); //Wave0write
            while (required > 0 && CurrentRun.Player.Exp >= required) //Wave0write
            { //Wave0write
                CurrentRun.Player.Exp -= required; //Wave0write
                CurrentRun.Player.Level += 1; //Wave0write
                CurrentRun.Player.Rune?.AddRunePoint(Data?.Balance?.RunePointPerLevel ?? 1); //Wave0write
                GrowPlayerStats(CurrentRun.Player); //Wave0write
                Events?.RaisePlayerLevelUp(CurrentRun.Player.Level); //Wave0write
                required = RequiredExpForLevel(CurrentRun.Player.Level); //Wave0write
            } //Wave0write

            Events?.RaisePlayerExpChanged(CurrentRun.Player.Exp, required); //Wave0write
        } //Wave0write

        private int RequiredExpForLevel(int level) //Wave0write
        { //Wave0write
            if (Data?.Balance?.ExpToNextLevel != null && level >= 0 && level < Data.Balance.ExpToNextLevel.Count) //Wave0write
            { //Wave0write
                return Data.Balance.ExpToNextLevel[level]; //Wave0write
            } //Wave0write

            return 999999; //Wave0write
        } //Wave0write

        private static void GrowPlayerStats(PlayerState player) //Wave0write
        { //Wave0write
            if (player?.Stats == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            player.Stats.BaseMaxHP += 8; //Wave0write
            player.Stats.BaseMaxMP += 2; //Wave0write
            player.Stats.BaseATK += 2; //Wave0write
            player.Stats.BaseDEF += 1; //Wave0write
            player.Stats.BaseSPD += 1; //Wave0write
            player.Stats.RecalculateFinalStats(); //Wave0write
            player.Stats.RestoreToFull(); //Wave0write
        } //Wave0write

        private void GrantDroppedItems(NodeRewardSummary summary, System.Collections.Generic.List<int> overflowIds) //Wave0write
        { //Wave0write
            if (summary?.DroppedItemIds == null || CurrentRun?.Player?.Inventory == null || Data?.Items == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            foreach (int itemId in summary.DroppedItemIds) //Wave0write
            { //Wave0write
                if (!Data.Items.TryGetValue(itemId, out ItemData itemData)) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                bool added = itemData.Stackable //Wave0write
                    ? CurrentRun.Player.Inventory.TryAdd(itemId, 1) //Wave0write
                    : CurrentRun.Player.Inventory.TryAddEquip(new Item { Data = itemData, Enhancement = 0 }); //Wave0write
                if (!added) //Wave0write
                { //Wave0write
                    overflowIds.Add(itemId); //Wave0write
                } //Wave0write
            } //Wave0write
        } //Wave0write

        private void FinishVictoryAfterReward() //Wave0write
        { //Wave0write
            FloorNode node = CombatContext?.Node; //Wave0write
            bool isBoss = CombatContext != null && CombatContext.IsBossNode; //Wave0write
            bool isRechallenge = CombatContext != null && CombatContext.IsRechallenge; //Wave0write
            int rechallengeReturnSafeIndex = CombatContext != null ? CombatContext.RechallengeReturnSafeIndex : 0; //Wave0write
            if (node != null) //Wave0write
            { //Wave0write
                int nextSelectableBeforeRechallenge = isRechallenge ? CurrentRun.FloorMap.NextSelectableFloor : 0; //Wave0write
                CurrentRun.FloorMap.MarkCleared(node.NodeId); //Wave0write
                if (isRechallenge && CurrentRun.FloorMap.NextSelectableFloor < nextSelectableBeforeRechallenge) //Wave0write
                { //Wave0write
                    CurrentRun.FloorMap.NextSelectableFloor = nextSelectableBeforeRechallenge; //Wave0write
                } //Wave0write
            } //Wave0write

            if (node != null && node.Floor >= 49 && isBoss) //Wave0write
            { //Wave0write
                CompleteRun(); //Wave0write
                return; //Wave0write
            } //Wave0write

            CombatContext = null; //Wave0write

            if (isBoss && node != null) //Wave0write
            { //Wave0write
                int safeIndex = StageIndexResolver.SafeIndexForStage(node.StageIndex); //Wave0write
                CurrentRun.SafeUnlocks.Unlock(safeIndex); //Wave0write
                Events?.RaiseSafeZoneLockChanged(safeIndex, false); //Wave0write
                if (isRechallenge) //Wave0write
                { //Wave0write
                    Erosion?.Reset(node.StageIndex); //Wave0write
                } //Wave0write
                else if (safeIndex >= SafeIndexForErosionStart) //Wave0write
                { //Wave0write
                    Erosion?.Activate(); //Wave0write
                } //Wave0write

                AdvanceDayForBossSafeEntry(); //Wave0write
                CurrentRun.CurrentFloor = ResolveSafeZoneFloor(safeIndex); //Wave0write
                Save?.SaveSnapshot(); //Wave0write
                Scenes.LoadSafeZone(safeIndex); //Wave0write
            } //Wave0write
            else if (isRechallenge && node != null) //Wave0write
            { //Wave0write
                CurrentRun.CurrentFloor = ResolveSafeZoneFloor(rechallengeReturnSafeIndex); //Wave0write
                Save?.SaveSnapshot(); //Wave0write
                Scenes.LoadSafeZone(System.Math.Max(0, System.Math.Min(5, rechallengeReturnSafeIndex))); //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                Save?.SaveSnapshot(); //Wave0write
                Scenes.LoadFloorMap(); //Wave0write
            } //Wave0write
        } //Wave0write

        private static void ShowGameOverOverlay() //Wave0write
        { //Wave0write
            if (GlobalOverlayController.TryGetInstance(out GlobalOverlayController overlay)) //Wave0write
            { //Wave0write
                overlay.ShowGameOver(); //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                Debug.LogError("[GameSystemManager] GlobalOverlayController 를 찾을 수 없어 GameOver 패널을 표시할 수 없습니다."); //Wave0write
            } //Wave0write
        } //Wave0write

        private void AttachErosionToCurrentRun()
        {
            if (CurrentRun?.Erosion == null)
            {
                Erosion = null;
                return;
            }

            Erosion = new ErosionSystem(CurrentRun.Erosion, Events, Data?.Balance);
            SubscribeErosionEvents();
        }

        private void SubscribeErosionEvents()
        {
            if (Events == null)
            {
                return;
            }

            Events.OnStageFullyEroded -= HandleStageFullyEroded;
            Events.OnAllStagesEroded -= HandleAllStagesEroded;
            Events.OnStageFullyEroded += HandleStageFullyEroded;
            Events.OnAllStagesEroded += HandleAllStagesEroded;
        }

        private void UnsubscribeErosionEvents()
        {
            if (Events == null)
            {
                return;
            }

            Events.OnStageFullyEroded -= HandleStageFullyEroded;
            Events.OnAllStagesEroded -= HandleAllStagesEroded;
        }

        private void HandleStageFullyEroded(int stage)
        {
            if (CurrentRun?.SafeUnlocks == null)
            {
                return;
            }

            int safeIndex = StageIndexResolver.SafeIndexForStage(stage);
            CurrentRun.SafeUnlocks.Lock(safeIndex);
            Events?.RaiseSafeZoneLockChanged(safeIndex, true);
            Save?.SaveSnapshot();
        }

        private void HandleAllStagesEroded()
        {
            if (CurrentRun != null)
            {
                Save?.AppendGrave(CurrentRun.Player != null ? CurrentRun.Player.Name : "Player", System.DateTime.Now);
            }

            Save?.ClearContinue();
            CurrentRun = null;
            CombatContext = null;
            // TEMP: H13-W1 보류 임시 처리. 정식 게임오버 패널 컨트롤러로 교체 필요.
            BootSceneBootstrap.ShowTempGameOverPanel();
            Scenes.LoadMainMenu();
        }
    }

    /// <summary>
    /// 한 런(한 도전) 동안 유지되는 동적 상태. 세이브/로드 단위.
    /// </summary>
    public sealed class GameRunState
    {
        /// <summary>현재 일자(노드 진입마다 +1).</summary>
        public int CurrentDay;

        /// <summary>현재 위치한 층(0 = Safe0).</summary>
        public int CurrentFloor;

        /// <summary>최대 도달 층(재도전/안전지대 잠금 판정용).</summary>
        public int HighestFloor;

        /// <summary>전체 플로어 맵. 저장 시 SaveSnapshot.FloorMap.Nodes로 전체 노드 구조가 직접 직렬화된다.</summary>
        public FloorMapModel FloorMap;

        /// <summary>플레이어 상태(레벨/EXP/스탯/룬/인벤토리/장비/소모/보관함).</summary>
        public PlayerState Player;

        /// <summary>현재 파티(동료 최대 3명).</summary>
        public CompanionRosterState Roster;

        /// <summary>단계별 침식 상태.</summary>
        public ErosionStateModel Erosion;

        /// <summary>각 안전지대 해금 상태(보스 클리어로 해금, 침식으로 잠김).</summary>
        public SafeZoneUnlockState SafeUnlocks;

        /// <summary>상점 재고/구매 이력 상태.</summary>
        public ShopStockState ShopStock;

        /// <summary>골드 잔액.</summary>
        public int Gold;

        /// <summary>마석 잔액.</summary>
        public int ManaStone;

        /// <summary>튜토리얼 진행 플래그.</summary>
        public TutorialProgressState Tutorial;
    }

    /// <summary>
    /// 전투 진입 컨텍스트. 노드 ID, 보스 여부, 재도전 여부 보관.
    /// </summary>
    public sealed class CombatContext
    {
        /// <summary>대상 노드.</summary>
        public FloorNode Node;

        /// <summary>보스 노드 여부.</summary>
        public bool IsBossNode;

        /// <summary>재도전 모드 여부.</summary>
        public bool IsRechallenge;

        /// <summary>재도전 전투 종료 후 복귀할 안전지대 인덱스.</summary>
        public int RechallengeReturnSafeIndex;

    }

    /// <summary>전투 결과.</summary>
    public enum CombatResult
    {
        /// <summary>승리.</summary>
        Victory,

        /// <summary>패배.</summary>
        Defeat,

        /// <summary>후퇴(아이템).</summary>
        Retreat,
    }
}

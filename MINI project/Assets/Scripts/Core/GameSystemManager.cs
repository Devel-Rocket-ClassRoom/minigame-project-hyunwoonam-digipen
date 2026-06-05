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

        /// <summary>화면/볼륨/언어 옵션 서비스.</summary>
        public OptionsService Options { get; private set; }

        /// <summary>현재 진행 중인 런 상태 (없으면 null).</summary>
        public GameRunState CurrentRun { get; private set; }

        /// <summary>현재 전투 진입 컨텍스트 (전투 중 아니면 null).</summary>
        public CombatContext CombatContext { get; private set; }

        // Guid3 §9.E 2026-05-27: 길드/인벤토리/스탯 화면 등 전투 외 시점에서도
        // Player MonoBehaviour 의 권위 출처가 필요. CombatController.OnEnter 가 set, OnExit 가 null.
        // 전투 외에서 Player MonoBehaviour 가 없으면 null 일 수 있다(Guid3 §10 W-G3-1 보류).
        /// <summary>현재 활성 Player MonoBehaviour. 전투 외에서는 null 일 수 있다.</summary>
        public Player ActivePlayer { get; set; }

        private RuntimeTextLocalizer runtimeTextLocalizer;

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
            // - 5) Scenes = 같은 GameObject의 GameSceneManager 참조.
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

            Options = new OptionsService();
            Options.Load();
            Options.Apply(Options.Current);

            runtimeTextLocalizer = GetComponent<RuntimeTextLocalizer>();
            if (runtimeTextLocalizer == null)
            {
                runtimeTextLocalizer = gameObject.AddComponent<RuntimeTextLocalizer>();
            }

            Scenes = GetComponent<GameSceneManager>();
            if (Scenes == null)
            {
                Debug.LogError(
                    "[GameSystemManager] GameSceneManager 컴포넌트가 같은 GameObject에 필요합니다."
                );
                enabled = false;
                return;
            }

            Hotkey = new HotkeyManager();
            Hotkey.BindGlobalKeys();
            Hotkey.OnTogglePage += RequestTogglePage;
            Hotkey.OnRequestQuit += RequestQuitConfirm;
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

                if (Hotkey != null)
                {
                    Hotkey.OnTogglePage -= RequestTogglePage;
                    Hotkey.OnRequestQuit -= RequestQuitConfirm;
                }

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
            CurrentRun.Erosion.EnsureStageCount(ErosionSystem.GetMaxStage(Data?.World));
            CurrentRun.SafeUnlocks = new SafeZoneUnlockState(GetSafeZoneCount());
            CurrentRun.SafeUnlocks.Unlock(0);
            CurrentRun.ShopStock = ShopStockState.CreateDefaultSafe1Stock();

            CurrentRun.Gold = 1000;
            CurrentRun.ManaStone = 0;
            CurrentRun.InitializeMineState();
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
            var stats = new StatBlock();
            stats.SetBaseStats(90, 20, 10, 2, 10);
            stats.RestoreToFull();

            var player = new PlayerState
            {
                Name = "Player",
                Level = 1,
                Exp = 0,
                Stats = stats,
                StartingClass = RuneClass.None,
                Rune = new PlayerRuneState
                {
                    ClassId = RuneClass.None,
                    RunePoints = 0,
                    UnlockedIds = new System.Collections.Generic.HashSet<int>(),
                },
                Inventory = new InventoryState(),
                Equipment = new EquipmentSlots(),
                Consumables = new ConsumableSlots(),
                Locker = new LockerState(),
            };

            player.Inventory.Add(1, 2);
            player.Inventory.Add(3, 1);

            AddStartingEquipment(player.Inventory, 101);
            AddStartingEquipment(player.Inventory, 102);
            AddStartingEquipment(player.Inventory, 103);
            AddStartingEquipment(player.Inventory, 104);

            player.Consumables.SlotItemIds[0] = 1;
            player.Consumables.SlotItemIds[1] = 3;

            return player;
        }

        private void AddStartingEquipment(InventoryState inventory, int itemId)
        {
            if (
                inventory == null
                || Data?.Items == null
                || !Data.Items.TryGetValue(itemId, out ItemData itemData)
            )
            {
                Debug.LogError("[GameSystemManager] 시작 장비 ID 없음: " + itemId);
                return;
            }

            if (itemData.Category != ItemCategory.Equipment || itemData.Stackable)
            {
                Debug.LogError(
                    "[GameSystemManager] 시작 장비가 Equipment가 아닙니다: " + itemId
                );
                return;
            }

            inventory.AddEquip(new Item { Data = itemData, Enhancement = 0 });
        }

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
            if (Save == null || !Save.HasContinue())
            {
                return;
            }

            CurrentRun = Save.Continue.ToGameRunStatet(Data);

            AttachErosionToCurrentRun();

            CombatContext = null;
            SceneId sceneId =
                Save.Continue.Location != null ? Save.Continue.Location.SceneId : SceneId.Safe0;
            Scenes.RequestScene(sceneId);
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
            if (CurrentRun == null || node == null || CurrentRun.FloorMap == null)
            {
                return;
            }

            if (node.IsSafeZone)
            {
                return;
            }

            NormalizeCombatNodeMonsterCount(node);

            int rechallengeMaxFloor = floorMapRechallengeMaxFloor;
            int rechallengeReturnSafeIndex = floorMapRechallengeReturnSafeIndex;
            bool rechallengeSelectable =
                isRechallenget && node.Floor > 0 && node.Floor <= rechallengeMaxFloor;
            bool selectable = isRechallenget
                ? rechallengeSelectable
                : node.Floor == CurrentRun.FloorMap.NextSelectableFloor;

            if (!selectable || (!isRechallenget && node.IsCleared))
            {
                return;
            }

            AdvanceRunDay();
            CurrentRun.CurrentFloor = node.Floor;
            CurrentRun.HighestFloor = System.Math.Max(CurrentRun.HighestFloor, node.Floor);

            CombatContext = new CombatContext
            {
                Node = node,
                IsBossNode = node.IsBoss,
                IsRechallenge = isRechallenget,
                RechallengeReturnSafeIndex = isRechallenget ? rechallengeReturnSafeIndex : 0,
            };

            ClearFloorMapRechallengeState();

            Save?.SaveSnapshot();
            Scenes.LoadCombat();
        }

        /// <summary>안전지대에서 안전지대 이동 가능 상태로 플로어맵을 연다.</summary>
        public void LoadFloorMapFromSafe(int safeIndex)
        {
            ArmFloorMapSafeTravel(safeIndex);

            if (CanOpenRechallengeFromSafe(safeIndex, out int maxFloor))
            {
                floorMapRechallengeRequested = true;
                floorMapRechallengeMaxFloor = maxFloor;
                floorMapRechallengeReturnSafeIndex = System.Math.Max(
                    0,
                    System.Math.Min(5, safeIndex)
                );
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

        /// <summary>플로어맵의 안전지대 노드에서 안전지대로 이동한다. 안전지대 간 이동은 날짜를 증가시키지 않는다.</summary>
        public void EnterSafeZoneFromFloorMap(int safeIndex)
        {
            if (CurrentRun?.SafeUnlocks == null || Scenes == null)
            {
                return;
            }

            if (
                safeIndex < 0
                || safeIndex >= GetSafeZoneCount()
                || !CurrentRun.SafeUnlocks.IsUnlocked(safeIndex)
            )
            {
                return;
            }

            CurrentRun.CurrentFloor = ResolveSafeZoneFloor(safeIndex);
            Save?.SaveSnapshot();
            ClearFloorMapRechallengeState();
            Scenes.LoadSafeZone(safeIndex);
        }

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
            floorMapSafeTravelSourceSafeIndex = System.Math.Max(
                0,
                System.Math.Min(GetSafeZoneCount() - 1, safeIndex)
            );
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

        private void AdvanceRunDay()
        {
            if (CurrentRun == null)
            {
                return;
            }

            CurrentRun.CurrentDay += 1;
            Events?.RaiseDayChanged(CurrentRun.CurrentDay);
            Erosion?.AdvanceDay(CurrentRun.CurrentDay);
        }

        private int ResolveSafeZoneFloor(int safeIndex)
        {
            if (Data?.World?.SafeZones != null)
            {
                for (int i = 0; i < Data.World.SafeZones.Count; i++)
                {
                    SafeZoneDef safeZone = Data.World.SafeZones[i];

                    if (safeZone != null && safeZone.Index == safeIndex)
                    {
                        return safeZone.FloorNumber;
                    }
                }
            }

            return 0;
        }

        private static void NormalizeCombatNodeMonsterCount(FloorNode node)
        {
            if (node == null || node.IsSafeZone)
            {
                return;
            }

            if (node.IsBoss || node.Floor == 1)
            {
                node.MonsterCount = 1;
            }
            else if (node.Floor == 2)
            {
                node.MonsterCount = 2;
            }
        }

        /// <summary>
        /// 전투 종료 분기. CombatControllert가 결과를 가지고 호출.
        /// Victory 시 보상 집계 → EXP/골드 지급 → CombatRewardPage 표시 순서로 처리.
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
            // 2) EXP/골드 지급
            //    - RewardGrant.ApplyNonItemRewards로 한 번만 지급한다.
            //
            // 3) 아이템 보상 세션 생성
            //    - 아이템은 RewardPanel에서 Get Items 또는 Done을 누를 때만 지급을 시도한다.
            //
            // 4) 보상 UI 표시
            //    - RewardPage 참조가 없거나 초기화에 실패하면 가능한 아이템을 순차 획득 후 이동한다.
            //
            // 5) [OnRewardClosed 콜백 내부]
            //    - CurrentRun.FloorMap.MarkCleared(CombatContext.Node.NodeId).
            //    - isBossNode → SafeUnlocks.Unlock(단계), Scenes.LoadSafeZone(단계).
            //    - isRechallenge → Scenes.LoadSafeZone(현재 단계).
            //    - 일반 → Scenes.LoadFloorMap().
            //    - CombatContext = null.
            //    - Save.SaveSnapshot()으로 CurrentRun.FloorMap의 전체 노드 구조와 다른 런 상태를 JSON에 함께 저장.
            if (CurrentRun == null)
            {
                CombatContext = null;
                Scenes.LoadMainMenu();
                return;
            }

            if (result == CombatResult.Defeat)
            {
                Save?.AppendGrave(
                    CurrentRun.Player != null ? CurrentRun.Player.Name : "Player",
                    System.DateTime.Now
                );
                Save?.ClearContinue();
                CurrentRun = null;
                CombatContext = null;
                ShowGameOverOverlay();
                Scenes.LoadMainMenu();

                return;
            }

            if (result == CombatResult.Retreat)
            {
                int safeIndex =
                    CombatContext != null
                        ? System.Math.Max(0, CombatContext.Node.StageIndex - 1)
                        : 0;
                CombatContext = null;
                Save?.SaveSnapshot();
                Scenes.LoadSafeZone(safeIndex);

                return;
            }

            NodeRewardSummary summary =
                controller != null ? controller.CollectNodeRewards() : new NodeRewardSummary();

            if (summary == null)
            {
                summary = new NodeRewardSummary();
            }

            RewardGrant.ApplyNonItemRewards(CurrentRun, Data, Events, summary);
            var claimSession = new CombatRewardClaimSession(
                summary.DroppedItemIds,
                itemId => RewardGrant.TryGrantItem(CurrentRun, Data, itemId)
            );

            System.Action closeAction = () => FinishVictoryAfterReward();

            if (
                controller?.Hud?.RewardPage != null
                && controller.Hud.RewardPage.Show(summary, claimSession, Data, closeAction)
            )
            {
                return;
            }

            claimSession.ClaimRemainingSequentially();
            closeAction();
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
            if (CurrentRun != null)
            {
                Save?.AppendClearRecord(
                    CurrentRun.Player != null ? CurrentRun.Player.Name : "Player",
                    System.DateTime.Now
                );
            }

            Save?.ClearContinue();
            CurrentRun = null;
            CombatContext = null;
            Scenes.LoadMainMenu();
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
            if (CurrentRun != null)
            {
                Save?.SaveSnapshot();
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void FinishVictoryAfterReward()
        {
            CombatContext context = CombatContext;
            VictoryFlowDecision decision = VictoryFlowResolver.Resolve(
                context,
                CurrentRun,
                Data?.World,
                GetSafeZoneCount() - 1
            );

            if (decision.CompleteRun)
            {
                CompleteRun();
                return;
            }

            CombatContext = null;

            if (decision.LoadSafeZone)
            {
                int safeIndex = decision.SafeIndex;

                CurrentRun.SafeUnlocks.Unlock(safeIndex);
                Events?.RaiseSafeZoneLockChanged(safeIndex, false);

                bool shouldActivateErosionAfterSafeEntry =
                    decision.ShouldActivateErosion && safeIndex >= SafeIndexForErosionStart;

                if (
                    decision.ShouldResetErosion
                    || Erosion?.IsStageFullyEroded(decision.StageIndex) == true
                )
                {
                    Erosion?.Reset(decision.StageIndex);
                }

                if (context != null && context.IsBossNode)
                {
                    AdvanceRunDay();
                }
                if (shouldActivateErosionAfterSafeEntry)
                {
                    Erosion?.Activate();
                }

                CurrentRun.CurrentFloor = ResolveSafeZoneFloor(safeIndex);
                Save?.SaveSnapshot();
                Scenes.LoadSafeZone(safeIndex);
            }
            else
            {
                Save?.SaveSnapshot();
                Scenes.LoadFloorMap();
            }
        }

        private static void ShowGameOverOverlay()
        {
            if (GlobalOverlayController.TryGetInstance(out GlobalOverlayController overlay))
            {
                overlay.ShowGameOver();
            }
            else
            {
                Debug.LogError(
                    "[GameSystemManager] GlobalOverlayController 를 찾을 수 없어 GameOver 패널을 표시할 수 없습니다."
                );
            }
        }

        private void AttachErosionToCurrentRun()
        {
            if (CurrentRun?.Erosion == null)
            {
                Erosion = null;
                return;
            }

            CurrentRun.Erosion.EnsureStageCount(ErosionSystem.GetMaxStage(Data?.World));
            CurrentRun.SafeUnlocks?.EnsureCapacity(GetSafeZoneCount());
            Erosion = new ErosionSystem(CurrentRun.Erosion, Events, Data?.Balance, Data?.World);

            SubscribeErosionEvents();
        }

        public void RequestQuitConfirm()
        {
            if (GlobalOverlayController.TryGetInstance(out GlobalOverlayController overlay))
            {
                overlay.ShowQuitConfirm(QuitGame);
                return;
            }

            Debug.LogError(
                "[GameSystemManager] GlobalOverlayController 를 찾을 수 없어 종료 확인 팝업을 표시할 수 없습니다."
            );
        }

        public void RequestTogglePage(HotkeyPageId pageId)
        {
            if (GlobalOverlayController.TryGetInstance(out GlobalOverlayController overlay))
            {
                overlay.HandleTogglePage(pageId);
                return;
            }

            Debug.LogError(
                "[GameSystemManager] GlobalOverlayController 를 찾을 수 없어 단축키 페이지를 열 수 없습니다."
            );
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

            int safeIndex = StageIndexResolver.SafeIndexForStage(stage, Data?.World);
            CurrentRun.FloorMap?.ResetStageProgression(stage);
            CurrentRun.SafeUnlocks.Lock(safeIndex);
            Events?.RaiseSafeZoneLockChanged(safeIndex, true);
            Save?.SaveSnapshot();
        }

        private int GetSafeZoneCount()
        {
            return Data?.World?.SafeZones != null && Data.World.SafeZones.Count > 0
                ? Data.World.SafeZones.Count
                : 6;
        }

        private void HandleAllStagesEroded()
        {
            if (CurrentRun != null)
            {
                Save?.AppendGrave(
                    CurrentRun.Player != null ? CurrentRun.Player.Name : "Player",
                    System.DateTime.Now
                );
            }

            Save?.ClearContinue();
            CurrentRun = null;
            CombatContext = null;
            ShowAllStagesErodedOverlay();
            Scenes.LoadMainMenu();
        }

        private static void ShowAllStagesErodedOverlay()
        {
            if (GlobalOverlayController.TryGetInstance(out GlobalOverlayController overlay))
            {
                overlay.ShowAllStagesEroded();
            }
            else
            {
                Debug.LogError(
                    "[GameSystemManager] GlobalOverlayController 를 찾을 수 없어 전체 침식 게임오버 패널을 표시할 수 없습니다."
                );
            }
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

        /// <summary>광산 활성 상태(Safe3~5, 인덱스 = SafeIndex - 3).</summary>
        public bool[] MineActivated;

        /// <summary>광산별 미수령 누적 골드(Safe3~5, 인덱스 = SafeIndex - 3).</summary>
        public int[] MineStored;

        /// <summary>광산별 마지막 일일 적립 일자(Safe3~5, 인덱스 = SafeIndex - 3).</summary>
        public int[] LastMineGainDay;

        /// <summary>튜토리얼 진행 플래그.</summary>
        public TutorialProgressState Tutorial;

        public void InitializeMineState()
        {
            EnsureMineState();
            for (int i = 0; i < 3; i++)
            {
                MineActivated[i] = false;
                MineStored[i] = 0;
                LastMineGainDay[i] = -1;
            }
        }

        public void EnsureMineState()
        {
            if (MineActivated == null || MineActivated.Length != 3)
            {
                MineActivated = new bool[3];
            }

            if (MineStored == null || MineStored.Length != 3)
            {
                MineStored = new int[3];
            }

            if (LastMineGainDay == null || LastMineGainDay.Length != 3)
            {
                LastMineGainDay = new int[3];
                for (int i = 0; i < LastMineGainDay.Length; i++)
                {
                    LastMineGainDay[i] = -1;
                }
            }
        }
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

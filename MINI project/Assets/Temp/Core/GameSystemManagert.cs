using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 게임 전체 최상위 싱글톤. 하위 시스템 초기화, 런 데이터 보관, 씬 흐름의 게이트웨이 역할.
    /// 직접 로직은 하지 않고 하위 시스템 호출만 한다.
    /// </summary>
    public sealed class GameSystemManagert : Singleton<GameSystemManagert>
    {
        /// <summary>정적 데이터 로더.</summary>
        public DataManagert Data { get; private set; }

        /// <summary>씬 전환 매니저.</summary>
        public GameSceneManagert Scenes { get; private set; }

        /// <summary>세이브/이어하기/기록 저장 매니저.</summary>
        public SaveLoadert Save { get; private set; }

        /// <summary>전체 글로벌 단축키 입력 라우터.</summary>
        public HotkeyManagert Hotkey { get; private set; }

        /// <summary>침식 시스템.</summary>
        public ErosionSystemt Erosion { get; private set; }

        /// <summary>이벤트 버스 (레벨업/침식/EXP 등 도메인 이벤트 발행).</summary>
        public EventBust Events { get; private set; }

        /// <summary>현재 진행 중인 런 상태 (없으면 null).</summary>
        public GameRunStatet CurrentRun { get; private set; }

        /// <summary>현재 전투 진입 컨텍스트 (전투 중 아니면 null).</summary>
        public CombatContextt CombatContext { get; private set; }

        /// <summary>
        /// 게임 부팅 시 호출. 하위 시스템 초기화 순서를 강제한다.
        /// </summary>
        protected override void Awake()
        {
            // 동작 요약:
            // - 1) base.Awake() 호출(싱글톤 등록).
            // - 2) Events = new EventBust() — 이벤트 버스 최우선.
            // - 3) Data = new DataManagert(); Data.LoadAll() — CSV/JSON 일괄 로드.
            // - 4) Save = new SaveLoadert(); Save.LoadAll() — Continue/기록 로드.
            // - 5) Scenes = GetComponentInChildren<GameSceneManagert>() 또는 신규 추가.
            // - 6) Hotkey = new HotkeyManagert(); Hotkey.BindGlobalKeys().
            // - 7) Erosion = new ErosionSystemt(new ErosionStateModelt(), Events).
            // - 8) DontDestroyOnLoad(this.gameObject)는 base.Awake()에서 처리.
            // - 9) Scenes.LoadMainMenu().

            base.Awake();

            if (!IsSingletonInstance)
            {
                return;
            }


            //초기화

            Events = new EventBust();

            Data = new DataManagert();
            Data.LoadAll();

            Save = new SaveLoadert();
            Save.LoadAll();

            Scenes = GetComponentInChildren<GameSceneManagert>();
            if (Scenes == null)
            {
                Scenes = gameObject.AddComponent<GameSceneManagert>();
            }

            Hotkey = new HotkeyManagert();
            Hotkey.BindGlobalKeys();

            Erosion = new ErosionSystemt(new ErosionStateModelt(), Events);

            Scenes.LoadMainMenu();
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
        

        /// <summary>
        /// 새 게임을 시작한다. 메인 메뉴의 New Game 버튼에서 호출.
        /// 새 게임 맵은 여기서 1회 생성하고, 이후 저장/로드는 seed 재생성이 아니라 전체 맵 스냅샷을 사용한다.
        /// </summary>
        public void StartNewGame()
        {
            // 동작 요약:
            // - Save.HasContinue()가 true면 호출 전 메인메뉴 측에서 확인 팝업을 띄웠어야 함.
            // - CurrentRun = new GameRunStatet(); 새 런 데이터 생성.
            // - FloorMapCreatort.Generate(Data.World)로 전체 맵을 1회 생성 후 CurrentRun.FloorMap에 보관.
            // - 생성된 FloorMap은 저장 시 SaveSnapshott.FloorMap.Nodes로 전체 구조가 직렬화됨.
            // - 시작 룬 선택 페이지를 위해 Safe0 진입 요청.
            // - 침식 상태 초기화.

            if(Save.HasContinue() == true)
            {
                return;
            }

            CurrentRun = new GameRunStatet();

            CurrentRun.FloorMap = FloorMapCreatort.Generate(Data.World);

            CurrentRun.CurrentDay = 0;
            CurrentRun.CurrentFloor = 0;
            CurrentRun.HighestFloor = 0;

            CurrentRun.Player = CreateInitialPlayerState();
            CurrentRun.Roster = new CompanionRosterStatet();

            CurrentRun.Erosion = new ErosionStateModelt();
            CurrentRun.SafeUnlocks = new SafeZoneUnlockStatet();
            CurrentRun.SafeUnlocks.Unlock(0);

            CurrentRun.Gold = 0;
            CurrentRun.ManaStone = 0;
            CurrentRun.Tutorial = new TutorialProgressStatet();

            Erosion = new ErosionSystemt(CurrentRun.Erosion, Events);

            CombatContext = null;
            Scenes.LoadSafeZone(0);
        }

        /// <summary>
        /// 새 런의 플레이어 초기 상태를 생성한다.
        /// 실제 시작 직업/룬 선택은 Safe0의 시작 룬 선택 흐름에서 확정한다.
        /// </summary>
        private PlayerStatet CreateInitialPlayerState()
        {
            // 동작 요약:
            // - PlayerStatet 생성.
            // - 기본 이름, 레벨, EXP, StatBlockt 초기값 설정.
            // - PlayerRuneStatet, InventoryStatet, EquipmentSlotst, ConsumableSlotst, LockerStatet 초기화.
            // - 시작 직업 룬은 Safe0 선택 결과를 반영할 수 있도록 미정 상태로 둔다.
            // - 완성 후 CurrentRun.Player에 저장할 PlayerStatet 반환.
            var stats = new StatBlockt(); //Wave0write
            stats.SetBaseStats(90, 20, 10, 2, 10); //Wave0write
            stats.RestoreToFull(); //Wave0write

            var player = new PlayerStatet //Wave0write
            { //Wave0write
                Name = "Player", //Wave0write
                Level = 1, //Wave0write
                Exp = 0, //Wave0write
                Stats = stats, //Wave0write
                StartingClass = RuneClasst.None, //Wave0write
                Rune = new PlayerRuneStatet { ClassId = RuneClasst.None, RunePoints = 0, UnlockedIds = new System.Collections.Generic.HashSet<int>() }, //Wave0write
                Inventory = new InventoryStatet(), //Wave0write
                Equipment = new EquipmentSlotst(), //Wave0write
                Consumables = new ConsumableSlotst(), //Wave0write
                Locker = new LockerStatet(), //Wave0write
            }; //Wave0write

            player.Inventory.Add(1, 2); //Wave0write
            player.Inventory.Add(3, 1); //Wave0write
            player.Consumables.SlotItemIds[0] = 1; //Wave0write
            player.Consumables.SlotItemIds[1] = 3; //Wave0write
            return player; //Wave0write
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
            // - snapshot.FloorMap.Nodes를 순회해 FloorMapModelt.NodesById와 NodesByFloor를 직접 복원.
            // - FloorMapCreatort.Generate()는 호출하지 않음(seed 재생성 금지).
            // - Player, Roster, Erosion, SafeUnlocks, Gold, ManaStone, Tutorial을 snapshot에서 복원.
            // - Erosion = new ErosionSystemt(CurrentRun.Erosion, Events)로 런 상태 모델에 다시 연결.
            // - CombatContext = null로 초기화.
            // - snapshot.Location.SceneId로 Scenes.RequestScene() 호출.
            if (Save == null || !Save.HasContinue()) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            CurrentRun = Save.Continue.ToGameRunStatet(Data); //Wave0write
            Erosion = new ErosionSystemt(CurrentRun.Erosion, Events); //Wave0write
            CombatContext = null; //Wave0write
            SceneIdt sceneId = Save.Continue.Location != null ? Save.Continue.Location.SceneId : SceneIdt.Safe0; //Wave0write
            Scenes.RequestScene(sceneId); //Wave0write

        }

        /// <summary>
        /// 전투 노드 진입. FloorMapControllert가 호출.
        /// </summary>
        /// <param name="node">선택된 노드.</param>
        /// <param name="isRechallenget">재도전 모드 여부.</param>
        public void StartCombatNode(FloorNodet node, bool isRechallenget)
        {
            // 동작 요약:
            // - node null, CurrentRun null, 이미 클리어된 노드 등 진입 불가 조건 검사.
            // - CombatContext = new CombatContextt { Node = node, IsBossNode = node.IsBoss, IsRechallenge = isRechallenget }.
            // - ErosionMultiplier는 Erosion.ComputeMonsterMultiplier(node.StageIndex)로 계산.
            // - currentDay += 1 (Erosion 진행 트리거).
            // - CurrentFloor, HighestFloor를 node.Floor 기준으로 갱신.
            // - Erosion.AdvanceDay(currentDay).
            // - Scenes.LoadCombat().
            if (CurrentRun == null || node == null || CurrentRun.FloorMap == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            bool selectable = node.Floor == CurrentRun.FloorMap.NextSelectableFloor || (isRechallenget && node.Floor < CurrentRun.FloorMap.NextSelectableFloor); //Wave0write
            if (!selectable || (!isRechallenget && node.IsCleared)) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            CurrentRun.CurrentDay += 1; //Wave0write
            CurrentRun.CurrentFloor = node.Floor; //Wave0write
            CurrentRun.HighestFloor = System.Math.Max(CurrentRun.HighestFloor, node.Floor); //Wave0write
            Erosion?.AdvanceDay(CurrentRun.CurrentDay); //Wave0write

            CombatContext = new CombatContextt //Wave0write
            { //Wave0write
                Node = node, //Wave0write
                IsBossNode = node.IsBoss, //Wave0write
                IsRechallenge = isRechallenget, //Wave0write
                ErosionMultiplier = Erosion != null ? Erosion.ComputeMonsterMultiplier(node.StageIndex) : 1f, //Wave0write
            }; //Wave0write

            Save?.SaveSnapshot(); //Wave0write
            Scenes.LoadCombat(); //Wave0write

        }

        /// <summary>
        /// 전투 종료 분기. CombatControllert가 결과를 가지고 호출.
        /// Victory 시 보상 집계 → 인벤토리 삽입 → CombatRewardPaget 표시 순서로 처리.
        /// </summary>
        /// <param name="result">전투 결과.</param>
        /// <param name="controller">보상 집계용 컨트롤러 참조.</param>
        public void EndCombat(CombatResultt result, CombatControllert controller)
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
            //    - NodeRewardSummaryt summary = controller.CollectNodeRewards().
            //
            // 2) EXP 처리
            //    - 플레이어와 동료는 전투 중 몬스터 처치 시 각자 자동으로 EXP 습득.
            //      (EntityBaset.OnKill 훅 또는 DamageCalculatort 처리 — 별도 분배 없음)
            //    - 여기서는 추가 EXP 지급 없음.
            //
            // 3) 골드 추가
            //    - CurrentRun.Gold += summary.TotalGold.
            //
            // 4) 아이템 인벤토리 삽입 (오버플로우 분리)
            //    - InventoryStatet inv = CurrentRun.Player.Inventory.
            //    - List<int> overflowIds = new List<int>().
            //    - summary.DroppedItemIds 순회:
            //        * ItemDatat data = Data.Items[itemId].
            //        * data.IsStackable → inv.TryAdd(itemId, 1) 실패 시 overflowIds.Add(itemId).
            //        * !data.IsStackable → new Itemt(data) 생성 후 inv.TryAddEquip() 실패 시 overflowIds.Add(itemId).
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

            if (result == CombatResultt.Defeat) //Wave0write
            { //Wave0write
                Save?.AppendGrave(CurrentRun.Player != null ? CurrentRun.Player.Name : "Player", System.DateTime.Now); //Wave0write
                Save?.ClearContinue(); //Wave0write
                CurrentRun = null; //Wave0write
                CombatContext = null; //Wave0write
                Scenes.LoadMainMenu(); //Wave0write
                return; //Wave0write
            } //Wave0write

            if (result == CombatResultt.Retreat) //Wave0write
            { //Wave0write
                int safeIndex = CombatContext != null ? System.Math.Max(0, CombatContext.Node.StageIndex - 1) : 0; //Wave0write
                CombatContext = null; //Wave0write
                Save?.SaveSnapshot(); //Wave0write
                Scenes.LoadSafeZone(safeIndex); //Wave0write
                return; //Wave0write
            } //Wave0write

            NodeRewardSummaryt summary = controller != null ? controller.CollectNodeRewards() : new NodeRewardSummaryt(); //Wave0write
            if (summary == null) //Wave0write
            { //Wave0write
                summary = new NodeRewardSummaryt(); //Wave0write
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

        private static void GrowPlayerStats(PlayerStatet player) //Wave0write
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

        private void GrantDroppedItems(NodeRewardSummaryt summary, System.Collections.Generic.List<int> overflowIds) //Wave0write
        { //Wave0write
            if (summary?.DroppedItemIds == null || CurrentRun?.Player?.Inventory == null || Data?.Items == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            foreach (int itemId in summary.DroppedItemIds) //Wave0write
            { //Wave0write
                if (!Data.Items.TryGetValue(itemId, out ItemDatat itemData)) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                bool added = itemData.Stackable //Wave0write
                    ? CurrentRun.Player.Inventory.TryAdd(itemId, 1) //Wave0write
                    : CurrentRun.Player.Inventory.TryAddEquip(new Itemt { Data = itemData, Enhancement = 0 }); //Wave0write
                if (!added) //Wave0write
                { //Wave0write
                    overflowIds.Add(itemId); //Wave0write
                } //Wave0write
            } //Wave0write
        } //Wave0write

        private void FinishVictoryAfterReward() //Wave0write
        { //Wave0write
            FloorNodet node = CombatContext?.Node; //Wave0write
            bool isBoss = CombatContext != null && CombatContext.IsBossNode; //Wave0write
            bool isRechallenge = CombatContext != null && CombatContext.IsRechallenge; //Wave0write
            if (node != null && !isRechallenge) //Wave0write
            { //Wave0write
                CurrentRun.FloorMap.MarkCleared(node.NodeId); //Wave0write
            } //Wave0write

            if (node != null && node.Floor >= 49 && isBoss) //Wave0write
            { //Wave0write
                CompleteRun(); //Wave0write
                return; //Wave0write
            } //Wave0write

            CombatContext = null; //Wave0write
            Save?.SaveSnapshot(); //Wave0write

            if (isBoss && node != null) //Wave0write
            { //Wave0write
                int safeIndex = System.Math.Min(5, node.StageIndex); //Wave0write
                CurrentRun.SafeUnlocks.Unlock(safeIndex); //Wave0write
                if (safeIndex >= 2) //Wave0write
                { //Wave0write
                    Erosion?.Activate(); //Wave0write
                } //Wave0write

                Scenes.LoadSafeZone(safeIndex); //Wave0write
            } //Wave0write
            else if (isRechallenge && node != null) //Wave0write
            { //Wave0write
                Scenes.LoadSafeZone(System.Math.Max(0, node.StageIndex - 1)); //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                Scenes.LoadFloorMap(); //Wave0write
            } //Wave0write
        } //Wave0write
    }

    /// <summary>
    /// 한 런(한 도전) 동안 유지되는 동적 상태. 세이브/로드 단위.
    /// </summary>
    public sealed class GameRunStatet
    {
        /// <summary>현재 일자(노드 진입마다 +1).</summary>
        public int CurrentDay;

        /// <summary>현재 위치한 층(0 = Safe0).</summary>
        public int CurrentFloor;

        /// <summary>최대 도달 층(재도전/안전지대 잠금 판정용).</summary>
        public int HighestFloor;

        /// <summary>전체 플로어 맵. 저장 시 SaveSnapshott.FloorMap.Nodes로 전체 노드 구조가 직접 직렬화된다.</summary>
        public FloorMapModelt FloorMap;

        /// <summary>플레이어 상태(레벨/EXP/스탯/룬/인벤토리/장비/소모/보관함).</summary>
        public PlayerStatet Player;

        /// <summary>현재 파티(동료 최대 3명).</summary>
        public CompanionRosterStatet Roster;

        /// <summary>단계별 침식 상태.</summary>
        public ErosionStateModelt Erosion;

        /// <summary>각 안전지대 해금 상태(보스 클리어로 해금, 침식으로 잠김).</summary>
        public SafeZoneUnlockStatet SafeUnlocks;

        /// <summary>골드 잔액.</summary>
        public int Gold;

        /// <summary>마석 잔액.</summary>
        public int ManaStone;

        /// <summary>튜토리얼 진행 플래그.</summary>
        public TutorialProgressStatet Tutorial;
    }

    /// <summary>
    /// 전투 진입 컨텍스트. 노드 ID, 보스 여부, 재도전 여부 보관.
    /// </summary>
    public sealed class CombatContextt
    {
        /// <summary>대상 노드.</summary>
        public FloorNodet Node;

        /// <summary>보스 노드 여부.</summary>
        public bool IsBossNode;

        /// <summary>재도전 모드 여부.</summary>
        public bool IsRechallenge;

        /// <summary>침식 보정 배수(현재 단계 침식률 기준).</summary>
        public float ErosionMultiplier;
    }

    /// <summary>전투 결과.</summary>
    public enum CombatResultt
    {
        /// <summary>승리.</summary>
        Victory,

        /// <summary>패배.</summary>
        Defeat,

        /// <summary>후퇴(아이템).</summary>
        Retreat,
    }
}

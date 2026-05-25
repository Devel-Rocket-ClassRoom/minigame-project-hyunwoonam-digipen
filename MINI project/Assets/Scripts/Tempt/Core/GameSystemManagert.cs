using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 게임 전체 최상위 싱글톤. 하위 시스템 초기화, 런 데이터 보관, 씬 흐름의 게이트웨이 역할.
    /// 직접 로직은 하지 않고 하위 시스템 호출만 한다.
    /// </summary>
    public sealed class GameSystemManagert : Singletont<GameSystemManagert>
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
            // - 7) Erosion = new ErosionSystemt(Events).
            // - 8) DontDestroyOnLoad(this.gameObject).
            // - 9) Scenes.LoadMainMenu().
        }

        /// <summary>
        /// 새 게임을 시작한다. 메인 메뉴의 New Game 버튼에서 호출.
        /// </summary>
        public void StartNewGame()
        {
            // 동작 요약:
            // - Save.HasContinue()가 true면 호출 전 메인메뉴 측에서 확인 팝업을 띄웠어야 함.
            // - CurrentRun = new GameRunStatet(); 새 런 데이터 생성.
            // - FloorMapCreatort로 전체 맵 생성 후 CurrentRun.FloorMap에 보관.
            // - 시작 룬 선택 페이지를 위해 Safe0 진입 요청.
            // - 침식 상태 초기화.
        }

        /// <summary>
        /// 이어하기. 메인 메뉴의 Continue 버튼에서 호출.
        /// </summary>
        public void ContinueGame()
        {
            // 동작 요약:
            // - SaveSnapshott 로드 후 CurrentRun, 침식, 위치 등 복원.
            // - 저장 시점의 씬으로 GameSceneManagert에 진입 요청.
        }

        /// <summary>
        /// 전투 노드 진입. FloorMapControllert가 호출.
        /// </summary>
        /// <param name="node">선택된 노드.</param>
        /// <param name="isRechallenget">재도전 모드 여부.</param>
        public void StartCombatNode(FloorNodet node, bool isRechallenget)
        {
            // 동작 요약:
            // - CombatContext = new CombatContextt(node, isRechallenget).
            // - currentDay += 1 (Erosion 진행 트리거).
            // - Erosion.AdvanceDay(currentDay).
            // - Scenes.LoadCombat().
        }

        /// <summary>
        /// 전투 종료 분기. CombatControllert가 결과를 가지고 호출.
        /// </summary>
        /// <param name="result">전투 결과.</param>
        public void EndCombat(CombatResultt result)
        {
            // 동작 요약:
            // - Defeat → 묘비 등록, Save.AppendGrave(), Scenes.LoadMainMenu().
            // - Victory + isBossNode → 해당 단계의 안전지대 해금/진입, 단계 침식률 초기화 옵션.
            // - Victory + isRechallenge → 현재 안전지대로 복귀.
            // - Victory + 일반 → FloorMap 복귀, 다음 층 활성화, EXP 합산 지급.
            // - CombatContext = null.
            // - 자동 저장 (Save.SaveSnapshot()).
        }

        /// <summary>
        /// 클리어 처리. 18층(설계변경 후 49층) 보스 처치 시 호출.
        /// </summary>
        public void CompleteRun()
        {
            // 동작 요약:
            // - 비석에 캐릭터 이름 + 일시 영구 기록(Save.AppendClearRecord).
            // - CurrentRun 정리.
            // - Scenes.LoadMainMenu().
        }

        /// <summary>
        /// 게임 종료. ESC 확인 팝업의 Yes에서 호출.
        /// </summary>
        public void QuitGame()
        {
            // 동작 요약:
            // - 자동 저장 (가능한 경우).
            // - Application.Quit().
        }
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

        /// <summary>전체 플로어 맵.</summary>
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

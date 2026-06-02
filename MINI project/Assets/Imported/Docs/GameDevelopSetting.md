# GameDevelopSetting.md — MiniProject (49층 탑 로그라이크)

## ⚠️ 작업 전 필수 확인

이 프로젝트의 모든 설계 결정과 인수인계 정보는 `MINI project/Assets/Imported/Docs` 아래 문서에 있습니다. **새 세션을 시작하거나 새 AI가 작업을 이어받을 때는 현재 권위 문서만 먼저 읽고 시작합니다.**

```text
MINI project/Assets/Imported/Docs/HANDOFF17.md                    현재 상태 단일 스냅샷
MINI project/Assets/Imported/Docs/TechnicalCheck_Report.md        최신 코드 평가 / 개선 우선순위
MINI project/Assets/Imported/Docs/SystemManagers_Architecture_Design.md  시스템 설계
MINI project/Assets/Imported/Docs/GameDevelopSetting.md           개발 규칙
MINI project/Assets/Imported/Docs/Archive/README.md               과거 HANDOFF/Guid 보관 규칙
```

- 이 위치는 한 번 바뀐 적이 있습니다(`Assets/Docs` → `Assets/Imported/Docs`). 오래된 문서/대화에서 옛 경로가 나오면 무시하고 위 경로를 사용합니다.
- 문서나 이전 대화 내용이 서로 충돌하면 **항상 최신 기준을 우선**합니다. 우선순위는 `사용자의 최신 직접 지시` → `현재 저장소 코드` → `HANDOFF17.md` → `TechnicalCheck_Report.md` → `SystemManagers_Architecture_Design.md` → `GameDevelopSetting.md` → `Docs/Archive/` 의 과거 HANDOFF/Guid → `최종기획안.md` 및 기타 참고 문서 순서입니다.
- `Docs/Archive/` 의 과거 `HANDOFF*.md` / `Guid*.md` 는 역사 기록입니다. 새 세션 시작 시 전체를 번호 순서대로 읽지 않습니다.
- 세션 진행 중 사용자의 의도, 변경사항, 결정사항, 보류/폐기된 판단, 구현 내용, 검증 결과가 바뀌면 `HANDOFF17.md` 를 업데이트합니다. 새 HANDOFF 번호는 사용자가 명시적으로 요청하거나 문서 체계를 다시 분기해야 할 때만 생성합니다.

## 프로젝트 개요

- **장르**: 솔로 다크 판타지 턴제 로그라이크 RPG
- **엔진**: Unity 6.3 LTS, URP, .NET 10
- **개발**: 1인 학생, 3주 절대 데드라인 (138시간)
- **GDD 전문**: `MINI project/Assets/Imported/Docs/최종기획안.md`

## 디렉터리 구조

```text
MIniProject/
├── CLAUDE.md
├── .codex/
│   └── AGENTS.md
├── .claude/settings.json
├── .mcp.json
└── MINI project/          ← Unity 프로젝트 루트
    ├── Assets/
    │   ├── Imported/
    │   │   └── Docs/          ← 기획/설계/인수인계 문서
    │   │       ├── GameDevelopSetting.md
    │   │       ├── HANDOFF17.md
    │   │       ├── TechnicalCheck_Report.md
    │   │       ├── SystemManagers_Architecture_Design.md
    │   │       ├── 최종기획안.md
    │   │       └── Archive/
    │   │           ├── README.md
    │   │           ├── HANDOFF*.md
    │   │           └── Guid*.md
    │   ├── Scenes/            ← MainMenu / Safe0 / Combat (FloorNode 추가 예정)
    │   └── Scripts/
    │       ├── Core/          ← Singleton, GameSystemManager, GameSceneManager,
    │       │                    DataManager, SaveLoader, FloorNodeCreator,
    │       │                    EntityBase, Skill/
    │       ├── Combat/        ← CombatFlow
    │       ├── Player/        ← Player
    │       ├── Monster/       ← MonsterBase, Monster1
    │       └── NPC/
    ├── Packages/manifest.json
    └── MINI project.slnx
```

## 핵심 아키텍처

### 씬 흐름 (확정)

```text
MainMenu  → Safe0 → FloorNode → Combat
                         ↑          │
                         └──────────┘  (진행 모드 / 재도전 모드 분기는 EndCombat에서 처리)
```

- `Safe0` 만 우선 구현. `Safe6`, `Safe12`는 추후 데이터 차별화로 처리.
- `FloorNode`는 진행 모드(전진 고정)와 재도전 모드(클리어 층 재방문) 공용.
- `DungeonScene` 이라는 옛 명칭은 모두 `Combat`으로 통일됨.

### EndCombat 분기 규칙

```text
Defeat                        → MainMenu (모드 무관, 영구사망)
Victory + isBossNode          → 다음 Safe 씬
Victory + !isRechallenge      → FloorNode 씬
Victory + isRechallenge       → 현재 Safe 씬
```

### 시스템 계층

```text
GameSystemManager (Singleton)
├─ DataManager        CSV / JSON
├─ SaveLoader         Continue + 비석/묘비 (Week 1엔 메모리 기반 뼈대만)
├─ FloorNodeCreator   던전 노드 생성
└─ GameSceneManager   씬 전환
```

- `GameSystemManager`만 Singleton. 하위 시스템은 모두 GSM이 소유.
- 사용자 정의 씬 매니저는 Unity 기본 `SceneManager`와 충돌 방지를 위해 반드시 `GameSceneManager` 이름 사용.
- 설계 문서의 `GameStateManager`, `DungeonCreator` 표기는 모두 위 이름으로 통일.

### CombatContext (전투 진입 컨텍스트)

`GameSystemManager`가 보관하는 전투 시작 정보. `StartCombatNode(nodeIndex)`에서 설정, `EndCombat(result)`에서 분기 기준.

```text
isRechallenge        : bool   재도전 모드 여부
isBossNode           : bool   보스 노드 여부
currentSafeZoneFloor : int    현재 기준 안전지대 층 (0 / 6 / 12)
```

### 전투 FSM (CombatFlow)

```text
CombatFlow.Start
  → 선공 결정 (Player ATK vs 몬스터 그룹 최대 ATK)
  → PlayerTurn 또는 MonsterTurn

PlayerTurn 행동:
  Attack / Skill1 / Skill2 / Defend → 메인 행동, 턴 소비, MonsterTurn으로
  Item                              → 보조 행동, 턴 미소비, PlayerTurn 유지

MonsterTurn:
  살아있는 몬스터들이 가중치(Attack 70 / Skill 20 / Defend 10)로 행동
  → PlayerTurn

종료:
  몬스터 전멸  → Victory → GSM.EndCombat(Victory)
  Player HP 0 → Defeat  → GSM.EndCombat(Defeat)
```

- **Player와 Monster에 별도 FSM을 만들지 않는다**. CombatFlow가 모든 턴 흐름을 관리하고, Player/Monster는 행동 데이터(스탯, 스킬)만 제공.
- 모든 주요 지점(선공 결정, 턴 전환, 행동 실행, HP 변화, 사망, 종료)에 `Debug.Log` 출력.

### Week 1 전투 수치 (`EntityBase`)

현재 코드 기준으로 8스탯은 폐기된 참고안이다. 런타임 전투는 Player / Monster 공통 `StatBlock` 의 5스탯(HP, MP, ATK, DEF, SPD)을 사용한다.

```text
HP / MP / ATK / DEF / SPD
```

초기 데모 기준값은 DataManager fallback 또는 향후 CSV/JSON 테이블이 권위다.
```text
Player / Monster : DataManager.LoadAll() 로 로드된 정적 데이터 참조
```

8스탯을 되살리는 작업은 현재 스코프가 아니며, 필요하면 별도 설계 변경으로 다룬다.

### 주요 시스템 및 책임

| 시스템 | 클래스 | 상태 |
|---|---|---|
| 전체 진입점 | `GameSystemManager` | 뼈대 완료, CombatContext 추가 예정 |
| 씬 전환 | `GameSceneManager` | 뼈대 완료, FloorNode 추가 예정 |
| 데이터 로딩 | `DataManager` | 껍데기 (CSV 로드 미구현) |
| 저장 | `SaveLoader` | 뼈대 (Week 1엔 메모리 기반만) |
| 던전 생성 | `FloorNodeCreator` | 껍데기 (3노드 더미 데이터 미구현) |
| 전투 흐름 | `CombatFlow` | 구현 완료 (UI 연결 잔여, EndCombat 분기 잔여) |
| 전투 수치 | `EntityBase` | 완료 (Week 1 직접 수치 방식) |
| 몬스터 공통 계층 | `MonsterBase` | 완료 (가중치 행동 선택) |
| 스킬 | `Skill` / `ActiveSkill` / `PassiveSkill` | 완료 (Week 1 임시 데이터 구조) |
| 침식 | `ErosionSystem` | Week 2 |
| 상태이상 | `StatusEffect` (컴포넌트) | Week 1 후반 / Week 2 |
| 룬 | `RuneManager` | Week 2 |
| 확장 동료 | `IPartyMember` | 인터페이스만 정의 예정 |

### 데이터 주도 원칙

- 몬스터·스킬·아이템·스탯 수치 → **CSV**
- 룬 트리·안전지대 정의·던전 노드·세이브 → **JSON**
- 수치 변경 시 코드 수정 없이 CSV/JSON만 편집

## 코드 컨벤션

- **포매터**: CSharpier (`csharpier` 전역 설치, PostToolUse 훅 자동 실행)
- 신규 코드에 `//Wave0write` 줄 끝 마커를 추가하지 않는다. 변경 이유가 필요하면 메서드/섹션 단위의 짧은 일반 주석만 사용한다.
- 네임스페이스: `MiniProject.<Layer>` (예: `MiniProject.Combat`, `MiniProject.Data`) — *현재 미적용, 신규 코드에서 검토*
- Unity MonoBehaviour는 `Assets/Scripts/` 하위, Pure C# 로직은 별도 어셈블리 고려
- 데이터 정의 파일: `Assets/Data/` 하위 CSV(테이블) + JSON(구조)

## 스코프 제약 (수정 금지)

다음 기능은 의도적으로 **제거된** 항목으로, 코드에 추가하지 않는다.

- 동료 영입/관리/사망 시스템 (인터페이스만 남김)
- 스트레스/광기 시스템
- 식량·횃불 자원
- 위치(포지션) 기반 전투
- 복잡한 후퇴 조건
- Player / Monster 개별 FSM (`ActorStateMachine` 등)

## GitHub 이슈 / 프로젝트 관리 원칙

- Repo: `Devel-Rocket-ClassRoom/minigame-project-hyunwoonam-digipen`
- Week 1 Project: `@hyunwoonam-digipen's Week1` (#59)

```text
이슈 = 이번 주 안에 완료할 단위만
완료 기준 = 이번 주 범위만 기술, - [ ] 체크박스 형식
Close = 실제 완료된 작업만
범위 외 / 다음 주 이슈 = 제목 [제거됨] 표시 + 프로젝트 보드에서 제거 (Close는 하지 않음)
In Progress / Done 상태는 실제 작업 상태와 정확히 일치
```

## 개발 도구

### CSharpier (자동 포매팅)
- `.cs` 파일 저장 시 PostToolUse 훅이 자동 실행
- 수동 실행: `csharpier <파일경로>` 또는 `csharpier .`

### 코나미 커맨드 에디터 (런타임 치트)
- 진입: ↑↑↓↓←→←→BA (어디서나)
- 기능: 층 점프, 침식 강제 적용, 몬스터 배치, HP/스탯 조작
- 세이브에 영향 없음 (재시작 시 초기화)
- *Week 2~3 구현 예정*

## AI 도구별 설정

AI 관련 MCP, 훅, 로컬 설정은 아래처럼 도구별 섹션에 분리해서 기록합니다.

- Claude 관련 설정은 `## Claude` 아래에만 기록합니다.
- Codex 관련 설정은 `## Codex` 아래에만 기록합니다.
- Claude가 자기 섹션을 읽다가 오류를 발견하면 `## Claude` 섹션만 수정합니다.
- Codex가 자기 섹션을 읽다가 오류를 발견하면 `## Codex` 섹션만 수정합니다.
- 한 도구가 다른 도구의 설정을 수정해야 할 필요가 있으면 직접 수정하지 않고 사용자에게 확인을 요청합니다.

## Claude

### Unity MCP (설정 완료)
`com.unity.ai.assistant` 패키지가 relay 바이너리를 제공하며, `.mcp.json`으로 Claude Code와 연결됩니다.

**연결 구조**
```text
Claude Code  →  relay_win.exe --mcp  →  Unity Editor (MCP Bridge)
```

**활성화 절차 (Unity Editor에서)**
1. Unity에서 프로젝트 열기 (Bridge가 자동 시작됨)
2. **Edit > Project Settings > AI > Unity MCP Server** 에서 Bridge 상태가 **Running** (녹색)인지 확인
3. Claude Code를 재시작하면 `.mcp.json`을 읽어 자동 연결
4. Unity에서 **Pending Connections** 알림이 뜨면 **Accept**

**사용 가능한 도구 예시**: `Unity_ManageScene`, `Unity_ManageGameObject`, `Unity_ReadConsole`

relay 바이너리: `C:/Users/ok623/.unity/relay/relay_win.exe` (v1.0.12)

### Claude Hooks
- `.cs` 파일 저장 시 Claude Code의 PostToolUse 훅이 CSharpier를 실행할 수 있습니다.
- 훅 경로와 세부 설정은 `.claude/settings.json`에서 관리합니다.

## Codex

### Codex Instructions
- Codex의 워크스페이스 지침은 `.codex/AGENTS.md`에서 관리합니다.
- Codex는 작업 전 `MINI project/Assets/Imported/Docs/GameDevelopSetting.md`와 `HANDOFF17.md`를 확인합니다. 과거 `HANDOFF*.md` / `Guid*.md` 는 `Docs/Archive/` 의 역사 기록으로, 필요한 경우에만 특정 문서를 열람합니다.
- Codex 관련 훅, MCP, 플러그인 설정이 추가되면 이 섹션에만 기록합니다.

## 개발 일정 체크포인트

| 일자 | 마일스톤 | 비상 플랜 |
|---|---|---|
| Day 7 | 전체 사이클 축소판 데모 | — |
| Day 14 | 12층 완성 | Plan B: 12층 완성판 |
| Day 18 | 3단계 콘텐츠 완성 | Plan C: 6층 완성판 |
| Day 21 | 빌드 + 포트폴리오 자료 | — |

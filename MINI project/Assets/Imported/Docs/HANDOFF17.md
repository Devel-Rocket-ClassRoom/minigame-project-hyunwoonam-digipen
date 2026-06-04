# HANDOFF17.md — Safe2 성소 + Safe3~5 광산 구현 스냅샷

날짜: 2026-06-03
브랜치: `TravenandGuild`

## 1. 이번 작업 기준

기준 문서:
- `.codex/AGENTS.md`
- `MINI project/Assets/Imported/Docs/GameDevelopSetting.md`
- `MINI project/Assets/Imported/Docs/HANDOFF16.md`
- `MINI project/Assets/Imported/Docs/Sanctuary_Mine_UI_Design.md`
- `MINI project/Assets/Imported/Docs/Prompts/Sanctuary_Mine_Implementation_Prompt.md`

사용자 직접 지시:
- Safe2에는 성소를 구현하고 UI에 연결한다.
- Safe3~5에는 광산을 구현하고 UI에 연결한다.
- 현재 브랜치를 유지한다.
- 문서에 커밋 요구가 있으면 현재 브랜치에서 커밋한다.
- 기존 몬스터 HP 임시 변경은 성소/광산 커밋 범위에서 제외한다.
- 빌드와 Unity 저장/닫기 검증이 성공한 경우에만 PC 종료를 진행한다.

## 2. 구현 내용

커밋:
- `838ae55` Add mine activation balance
- `9084756` Persist mine run state
- `a689a43` Implement sanctuary and mine safe zones
- 문서 갱신 커밋은 이 HANDOFF17 자체와 `GameDevelopSetting.md` 현재 문서 참조 갱신을 포함한다.

Balance:
- `BalanceData`에 `MineActivationCost`를 추가했다.
- `Balance.json`에 `"MineActivationCost": 1`을 추가했다.
- `DataManager.BuildFallbackBalance()`에 `MineActivationCost = 1` fallback을 추가했다.
- 광산은 마나석이 아니라 골드 기반으로 동작한다.

런 상태 / 저장:
- `GameRunState`에 `MineActivated[3]`, `MineStored[3]`, `LastMineGainDay[3]`를 추가했다.
- `InitializeMineState()`와 `EnsureMineState()`를 추가했다.
- New Game 시작 시 광산 상태를 초기화한다.
- `SaveSnapshot`에 광산 배열 3종 저장/복원을 추가했다.

성소:
- `SanctuaryController`를 신규 작성했다.
  - Safe2 전용 컨트롤러로 `SafeIndex = 2`를 사용한다.
  - 침식 시스템 활성화, 정화 가능 여부, 비용 산정, 골드 차감, `Erosion.Reduce`, 저장을 담당한다.
- `SanctuaryUI`를 신규 작성했다.
  - `Facility_SANCTUARY` 하위 `StageGrid` 6개 카드와 `RightColumn`, `PurifyButton`을 문서 경로 기준으로 바인딩한다.
  - 카드 선택, 침식 퍼센트/위험도/비용/상태/요약 갱신을 처리한다.
  - 런타임 `AddComponent` fallback은 넣지 않았다.
- `Safe2.unity`의 기존 `SafeZoneSimpleController`를 `SanctuaryController`로 교체하고 `Facility_SANCTUARY`에 `SanctuaryUI`를 연결했다.
- 침식 100% 안전지대 잠금은 기존 `GameSystemManager.HandleStageFullyEroded(int stage)`가 `SafeUnlocks.Lock(StageIndexResolver.SafeIndexForStage(stage, Data?.World))`로 처리하므로 중복 구현하지 않았다.

광산:
- `MineController`를 골드 기반 활성화/일일 적립/수령 모델로 교체했다.
  - Safe3~5는 각각 `MineIndex = SafeIndex - 3`으로 상태 배열을 사용한다.
  - 비활성 상태에서는 활성화 비용을 골드로 지불한다.
  - 활성화 후 하루에 한 번 `MineDailyGain`만큼 저장 골드를 적립한다.
  - 수령 시 저장 골드를 플레이어 골드에 더하고 저장 골드를 0으로 만든다.
- `MineUI`를 신규 작성했다.
  - `MainPanel/MainMinePanel` 하위 `MineInfoPanel`, `ActivationInfoPanel`, `CollectionPanel`, `BottomGuideBox`와 버튼, TopHUD를 문서 경로 기준으로 바인딩한다.
  - 비활성/활성 패널 전환, 활성화 비용, 일일 생산량, 저장 골드, 수령 가능 여부를 갱신한다.
- `Safe3.unity`, `Safe4.unity`, `Safe5.unity`의 기존 `SafeZoneSimpleController`를 `MineController`로 교체하고 각 `MainPanel`에 `MineUI`를 연결했다.
- `Safe3.unity`에는 기존 day label / floor map button 참조 대상 이름이 없어 해당 필드는 기존처럼 null로 유지했다.

프로젝트 파일:
- `Assembly-CSharp.csproj`에 신규 스크립트 Include를 추가했다.

2026-06-04 추가 UI 연결:
- Safe2 `Canvas_SafeZone2` 중앙에 실제 씬 오브젝트 `OpenSanctuaryButton`을 추가했다.
  - 클릭 시 `Facility_SANCTUARY.SetActive(true)`가 실행되도록 영구 이벤트를 연결했다.
- Safe3~5 각 `Canvas_SafeZone3/4/5` 중앙에 실제 씬 오브젝트 `OpenMineButton`을 추가했다.
  - 클릭 시 각 씬의 `MainPanel.SetActive(true)`가 실행되도록 영구 이벤트를 연결했다.
- `SanctuaryUI.ClosePanel()`과 `MineUI.ClosePanel()`을 추가해 내부 닫기/취소 버튼이 전체 패널을 닫도록 정리했다.
- 버튼 배치용 임시 Editor 스크립트 `Assets/Editor/PlaceSafeFacilityButtonsOnce.cs`는 최종 산출물에 남기지 않고 삭제했다.

2026-06-04 추가 연결 보강:
- `.claude/CLAUDE.md` 지침과 성소/광산 설계 문서를 재확인했다.
- `SanctuaryUI`의 `SummaryValues` 바인딩을 단순 순서 의존에서 `ValueRow`의 `Label`/행 이름 기반 매칭 + 순서 fallback 방식으로 보강했다.
- `MineUI`의 `StatGrid`와 `CollectionSummaryValues` 바인딩을 `Label`/행 이름 기반 매칭 + 순서 fallback 방식으로 보강했다.
- Safe2~5 중앙 패널 열기 버튼의 Canvas sibling 순서를 대상 패널보다 앞쪽으로 이동해, 패널이 열릴 때 버튼이 패널 앞을 가리지 않게 했다.

2026-06-04 비용 임시 조정:
- 사용자 지시에 따라 성소 정화 비용과 광산 활성화 비용을 모두 임시 `1`로 고정했다.
- `Balance.json`과 `DataManager.BuildFallbackBalance()`의 `ErosionAltarCost`를 `1`로 변경했다.
- `SanctuaryController.GetPurifyCost()`와 `MineController.GetActivationCost()`는 해당 비용에 인플레이션을 적용하지 않고 기본 비용 `1`을 그대로 사용한다.

2026-06-04 UI 클릭/바인딩 차단 원인 수정:
- Safe2 `STAGE 1~6_Card` 루트에 `Button` 컴포넌트가 없어 `SanctuaryUI`가 초기화 중 실패하고 카드 클릭/값 갱신/RightColumn 갱신이 모두 중단되는 문제를 확인했다.
- Safe2 `STAGE 1~6_Card`에 실제 Unity UI `Button` 컴포넌트를 추가하고 카드 루트 `Image`를 `targetGraphic`으로 연결했다.
- Safe3~5 `Btn_CollectGold`, `Btn_Close`에 `Button` 컴포넌트가 없어 `MineUI`의 수령/닫기 와이어가 불완전한 문제를 확인했다.
- Safe3~5 `Btn_CollectGold`, `Btn_Close`에 실제 Unity UI `Button` 컴포넌트를 추가하고 각 루트 `Image`를 `targetGraphic`으로 연결했다.
- 정적 검사로 Safe2 카드 6개, Safe3~5 Activate/Collect/Close/Cancel 버튼 모두 `Button` 컴포넌트를 보유함을 확인했다.

## 3. 검증 결과

완료:
- `dotnet build "MINI project\Assembly-CSharp.csproj"` 통과.
  - 경고 0 / 오류 0.
- 2026-06-04 추가 버튼 작업 후 `dotnet build "MINI project\Assembly-CSharp.csproj"` 재실행 통과.
  - 경고 0 / 오류 0.
- 2026-06-04 추가 버튼 작업 대상 `Safe2~5.unity`, `MineUI.cs`, `SanctuaryUI.cs`에 대해 `git diff --check` 통과.
- Safe2 `OpenSanctuaryButton`, Safe3~5 `OpenMineButton`이 Canvas 자식으로 존재하고 각 대상 패널을 `SetActive(true)`로 여는 YAML 참조를 정적으로 확인했다.
- 2026-06-04 추가 연결 보강 후 `dotnet build "MINI project\Assembly-CSharp.csproj"` 재실행 통과.
  - 경고 0 / 오류 0.
- 2026-06-04 추가 연결 보강 대상 `Safe2~5.unity`, `MineUI.cs`, `SanctuaryUI.cs`에 대해 `git diff --check` 통과.
- Safe2~5 중앙 열기 버튼이 대상 패널 RectTransform보다 Canvas sibling 순서상 앞에 있어, 패널 활성화 시 패널이 버튼 위에 렌더링됨을 정적으로 확인했다.
- 성소/광산 관련 C# 파일, Balance 파일, csproj 대상 `git diff --check` 통과.
- `SaveSnapshot`에서 `MineActivated`, `MineStored`, `LastMineGainDay`가 저장/복원되는 코드 경로를 확인했다.
- 신규/변경 성소/광산 코드에서 `ManaStone +=`, `RaiseManaStoneChanged`, `OnManaStoneChanged`, 런타임 `AddComponent<...>` fallback 잔존 없음 확인.

미완료 / 제한:
- Unity MCP HTTP 서버는 응답했으나 Unity 인스턴스 등록이 `0`으로 떨어져 직접 Unity Editor 메뉴 실행 검증은 중단했다. 사용자는 이후 2번 방식, 즉 씬 파일 직접 반영 방식을 지시했다.
- `csharpier check`는 현재 PATH에서 `csharpier` 명령을 찾지 못해 미수행.
- `dotnet csharpier check`는 `dotnet-csharpier` 도구가 설치되어 있지 않아 미수행.
- Safe2~5 씬을 포함한 전체 `git diff --check`는 기존 씬 YAML에 남아 있던 trailing whitespace 때문에 실패했다. 코드/리소스 대상 검사는 통과했다.
- Unity Editor PlayMode 수동 검증은 아직 완료되지 않았다.
- Unity Console Error 0 / Missing Reference 0은 아직 확인되지 않았다.
- Unity 프로젝트 저장/닫기 검증은 아직 완료되지 않았다.
- 위 Unity 검증 조건이 완료되지 않았으므로 PC 종료는 진행하지 않는다.

## 4. 기존 dirty 상태 중 제외한 변경

이번 성소/광산 커밋 범위에서 제외:
- `MINI project/Assets/Resources/Tables/MonsterStatusTable.csv`
- `MINI project/Assets/Scripts/Core/DataManager.cs` 안의 몬스터 HP 임시 변경 hunk

위 변경은 이전 사용자 지시인 "몬스터들 체력 전부 1로 임시로 변경"에 해당하며, 이번 성소/광산 구현 커밋에는 포함하지 않는다.

## 5. 다음 작업

- Unity Editor에서 Safe2 성소 UI를 열어 6개 카드 표시, 선택, RightColumn 갱신, 정화 버튼 골드 차감/침식 감소/비활성 조건을 확인한다.
- Unity Editor에서 Safe3~5 광산 UI를 열어 활성화 패널, 활성화 비용 차감, 당일 적립, 수령 패널 전환, 수령 골드 반영, Safe별 일일 보상 차등을 확인한다.
- Unity Console Error 0과 Missing Reference 0을 확인한다.
- Unity 저장/닫기 검증이 성공한 경우에만 PC 종료를 수행한다.

## 6. 2026-06-04 성소/광산 UI 재연결 및 PlayMode 검증

원인:
- `SanctuaryUI`는 `Facility_SANCTUARY/ContentArea`를 찾았지만 실제 계층은 `Facility_SANCTUARY/PaddingContainer/ContentArea`여서 `Awake()` 초기화가 중단됐다.
- `MineUI`는 `MainPanel/MainMinePanel`, `MainPanel/ActivationInfoPanel`, `MainPanel/CollectionPanel`을 찾았지만 실제 패널은 `MainPanel/ContentArea` 아래에 있어 `Awake()` 초기화가 중단됐다.
- 초기화 중단 때문에 Value/슬라이더 갱신과 런타임 버튼 와이어가 전부 실행되지 않았다.
- `MineController.ResolveSafeIndexFromScene()`가 `GameSceneManager.CurrentSceneId`만 사용해 Safe4/5 직접 Play 시 Safe3 데이터로 떨어질 수 있었다.

수행:
- `SanctuaryUI`가 실제 `PaddingContainer/ContentArea` 계층을 캐시하도록 수정했다.
- 성소 SummaryValues를 실제 라벨에 맞춰 `Current Erosion`, `After Purify`, `Required Gold`, `Owned Gold`, `Linked Safe Zone` 값으로 연결했다.
- `MineUI`가 실제 `ContentArea/MainMinePanel`, `ContentArea/ActivationInfoPanel`, `ContentArea/CollectionPanel` 계층을 캐시하도록 수정했다.
- 광산 StatGrid, ActivationInfoPanel, CollectionSummaryValues의 모든 Value를 런 상태 값으로 연결했다.
- 광산 `ActivateButton`, `CancelButton`, `Btn_CollectGold`, `Btn_Close`, Header `CloseButton`을 모두 런타임 와이어했다.
- Safe3의 신규 `EnterFloorMapButton`을 `MineController.enterFloorMapButton`에 연결했다.
- Safe4의 신규 `EnterFloorMapButton`을 공식 컨트롤러 참조로 연결하고, 기존 중복 버튼도 `DepartToFloorMap()` 영구 이벤트로 동작하도록 유지했다.
- `MineController`가 자기 GameObject의 실제 씬 이름을 우선 사용해 Safe3/4/5 인덱스를 결정하도록 수정했다.

검증:
- Unity 정적 연결 감사: Safe2 카드 6개/슬라이더/버튼/SummaryValues, Safe3~5 광산 Value/버튼/EnterFloorMap 연결 전체 통과.
- Safe2 PlayMode:
  - 성소 열기 후 `SanctuaryUI.enabled=true`.
  - Stage1 침식 `42%`가 슬라이더 `0.42`와 Percent `42%`로 표시됨.
  - Stage2 선택 후 Summary가 `18%`, 정화 후 `8%`, 보유 골드, 연결 Safe 값으로 갱신됨.
  - Purify 클릭 시 침식 `18→8`, 골드 `25→23`.
  - Header Close 클릭 시 패널 닫힘.
- Safe3 PlayMode:
  - Stored/Daily/Status/Total Gold 및 Activation/Collection Value 갱신 확인.
  - Activate 클릭 시 활성화, 골드 차감, 당일 적립, 패널 전환 확인.
  - Collect 클릭 시 골드 수령 및 Stored Gold 0 확인.
  - Cancel, Btn_Close, Header Close 모두 패널 닫힘 확인.
- Safe4/5 PlayMode:
  - Safe4 `SafeIndex=4`, 일일 생산량 `2 G`.
  - Safe5 `SafeIndex=5`, 일일 생산량 `3 G`.
  - 각 씬 Value 표시와 닫기 버튼 동작 확인.
- Unity Console 최종 조회: Error 0 / Warning 0.
- `dotnet build "MINI project/Assembly-CSharp.csproj"`: 경고 0 / 오류 0.

## 7. 2026-06-04 Safe2~5 ResourceGroup / DayText 연결

원인:
- Safe2와 Safe3의 `SafeStatusHud`에 Gold/HP/MP TMP 참조가 모두 비어 있어 상단 자원 값이 런타임 상태와 연결되지 않았다.
- Safe3의 `MineController.dayLabel` 참조가 비어 있어 `DayText`가 현재 일자로 갱신되지 않았다.
- Safe4와 Safe5의 ResourceGroup 및 DayText 참조는 이미 정상 연결 상태였다.

수행:
- Safe2와 Safe3의 기존 `ResourceRow`를 ResourceGroup 역할로 사용해 Gold/HP/MP TMP를 `SafeStatusHud`에 연결했다.
- Safe3의 `DayText` TMP를 `MineController.dayLabel`에 연결했다.
- Safe2, Safe4, Safe5의 기존 DayText 연결과 Safe4~5 ResourceGroup 연결은 유지했다.

검증:
- Safe2~5 정적 참조 감사에서 DayText와 Gold/HP/MP 참조가 모두 non-null임을 확인했다.
- Safe2~5 PlayMode에서 `SafeStatusHud.enabled=true`와 `Day 1`, `150 G`, `HP 100 / 100`, `MP 25 / 25` 실제 갱신을 확인했다.
- `dotnet build "MINI project/Assembly-CSharp.csproj"`: 경고 0 / 오류 0.
- Unity Console 최종 조회: Error 0 / Warning 0.

## 8. 2026-06-04 UIsample Combat Reward Panel 구성

수행:
- 제공된 Combat Reward Panel TXT 명세를 기준으로 `UIsample.unity`의 기존 `Canvas_GlobalOverlay` 아래에 `RewardPanel`을 구성했다.
- Canvas / Canvas Scaler 설정은 변경하지 않았다.
- RewardPanel은 중앙 고정 `760x560` 크기이며 직속 자식은 `HeaderArea`, `RewardSummaryArea`, `ItemScrollArea`, `ButtonArea` 네 영역만 사용한다.
- EXP / GOLD 요약 카드, 6개 예시 아이템의 세로 스크롤 목록, `CLAIM` / `FLOOR MAP` 두 버튼을 구성했다.
- UIsample 실행 시 RewardPanel만 표시되도록 `Canvas_GlobalOverlay`를 활성화하고 기존 샘플 자식 UI는 비활성화했다.
- 프로젝트 스크립트와 버튼 동작 로직은 추가하지 않았다.

검증:
- 정적 감사에서 Canvas 설정, RewardPanel 크기/중앙 앵커, 네 영역 위치와 크기, 버튼 2개, 세로 ScrollRect, `668x46` 아이템 행 6개를 확인했다.
- PlayMode에서 RewardPanel 표시, `REWARD` 제목 렌더링, 세로 스크롤 이동을 확인했다.
- `1366x768`, `1600x900`, `1920x1080`, `2560x1440`에서 중앙 배치 및 화면 범위 내 유지 계산 검증을 통과했다.
- 영역 간 간격은 Header→Summary `24px`, Summary→Items `16px`, Items→Buttons `18px`이며 겹침이 없다.
- 완전히 잘린 텍스트 `0`, 활성 Canvas 자식은 RewardPanel 하나임을 확인했다.

## 9. 2026-06-04 Boot 침식 Game Over UI 구성

수행:
- 제공된 의도 대화와 UI 제작 프롬프트를 기준으로 `Boot.unity`의 기존 `Canvas` 아래에 전체 화면 `ErosionGameOverRoot`를 구성했다.
- 루트 직속 레이어는 `BlackBackground`, `DeepRedVignette`, `PurpleCorruptionFog`, `ErosionCrackOverlay`, `RedPulseRing`, `NoiseScanlineOverlay`, `CenterTextGroup`, `FinalDarkVignette` 순서다.
- 화면에는 3중 침식 글리치 표현의 `GAME OVER` 문구만 사용하고, 버튼이나 추가 설명 텍스트는 생성하지 않았다.
- 붉은/보라 오염 띠, 균열선, 끊어진 침식 파동, 스캔라인, 밑줄과 추상적 번짐을 UI 레이어로 구성했다.
- `ErosionGameOverRoot`는 평상시 비활성 상태로 저장했다.
- Boot의 기존 Canvas Scaler는 명세 예상과 달리 `Constant Pixel Size`였으며, Canvas 변경 금지 요구에 따라 설정을 변경하지 않았다.
- 프로젝트 스크립트와 동작 로직은 추가하지 않았다.

검증:
- PlayMode 캡처로 전체 화면 침식 연출과 중앙 `GAME OVER` 가독성을 확인했다.
- 루트 전체 화면 Stretch, 요구된 8개 레이어 순서, 버튼 0개, 표시 문구가 `GAME OVER`뿐임을 정적 감사했다.
- `1366x768`, `1600x900`, `1920x1080`, `2560x1440`에서 중앙 `1120x240` 콘텐츠가 화면 범위 안에 유지됨을 확인했다.
- 최종 저장 상태에서 `ErosionGameOverRoot.activeSelf=false`임을 확인했다.

## 10. 2026-06-04 침식 Game Over 연결 및 Any Key 복귀

원인:
- 침식 전체 100% 게임오버 경로는 `GlobalOverlayController.ShowAllStagesEroded()`로 분리되어 있었지만, 내부에서 일반 전투 패배용 `ShowGameOver()`를 그대로 호출해 기존 `GameOver` 패널이 표시됐다.

수행:
- `GlobalOverlayController`에 Boot의 `ErosionGameOverRoot` 직렬화 참조를 추가했다.
- 일반 전투 패배는 기존 `GameOver`, 전체 침식 게임오버는 `ErosionGameOverRoot`를 표시하도록 분리했다.
- 게임오버 표시 전 기존 게임오버 패널들을 모두 닫아 두 패널이 겹치지 않도록 했다.
- 기존 `Input.anyKeyDown` 입력 대기와 `HideGameOver()` 경로를 두 게임오버 패널이 공유하도록 확장했다.
- Boot 씬의 `GlobalOverlayController.gameOverPanel`과 `erosionGameOverPanel`을 각각 기존 `GameOver`, 수정된 `ErosionGameOverRoot`에 직접 연결했다.

검증:
- 수정 전 PlayMode 재현: `ShowAllStagesEroded()` 호출 시 기존 `GameOver=true`, `ErosionGameOverRoot=false`.
- 수정 후 PlayMode: `ShowAllStagesEroded()` 호출 시 기존 `GameOver=false`, `ErosionGameOverRoot=true`.
- 침식 루트 내부의 `PRESS ANY KEY TO RETURN` 텍스트 존재를 확인했다.
- 공용 복귀 경로가 일반/침식 게임오버 패널을 모두 닫는 것을 확인했다.
- 일반 전투 패배용 `ShowGameOver()`가 기존 `GameOver`를 계속 표시하는 것을 확인했다.

## 11. 2026-06-04 Safe2 전체 침식 임시 테스트 트리거

수행:
- Unity Editor 테스트에서 Safe2에 도달하면 모든 단계 침식률을 100%로 만드는 임시 트리거를 추가했다.
- `SanctuaryController.SetupZoneFeatures()`에서 성소 UI 초기 갱신 후 `ErosionSystem.DebugForceAllStagesFullyEroded()`를 호출한다.
- 임시 트리거는 `UNITY_EDITOR` 조건부 코드라 에디터 테스트에서만 동작한다.
- 모든 단계에 대해 침식 변경/완전 침식 이벤트를 정상 순서로 발행한 뒤 전체 침식 이벤트를 발행해 실제 게임오버 흐름을 통과한다.

검증:
- Safe2 `OnEnter()` 호출 후 `ErosionGameOverRoot=true`, 기존 `GameOver=false`를 확인했다.
- `dotnet build "MINI project/Assembly-CSharp.csproj"`: 경고 0 / 오류 0.

제거 위치:
- 테스트 완료 후 `SanctuaryController.SetupZoneFeatures()`의 `DebugForceAllStagesFullyEroded()` 호출과 `ErosionSystem.DebugForceAllStagesFullyEroded()` 메서드를 함께 제거한다.

제거 완료:
- 2026-06-04 사용자 요청으로 Safe2 전체 침식 임시 테스트 트리거를 제거했다.
- Safe2는 다시 정상적으로 침식 시스템을 활성화하고 성소 UI만 갱신한다.
- 침식 전체 100% 발생 시 `ErosionGameOverRoot` 표시 및 Any Key 복귀 기능은 유지한다.

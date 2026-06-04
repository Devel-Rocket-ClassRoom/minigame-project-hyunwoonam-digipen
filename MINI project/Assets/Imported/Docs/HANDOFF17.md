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

## 12. 2026-06-04 Combat 승리 보상 패널 연결

수행:
- 전투 승리 시 아이템까지 즉시 지급하던 흐름을 EXP/골드 즉시 지급과 아이템 선택 지급으로 분리했다.
- `CombatRewardClaimSession`을 추가해 중복 아이템 수량, 선택 상태, 남은 수량, `Get Items` 사용 여부를 관리한다.
- `Get Items`는 선택된 아이템 중 인벤토리에 들어가는 수량만 획득하고, 실패한 수량은 목록에 남긴다.
- `Done`은 `Get Items`를 누르지 않았을 때만 남은 아이템을 순차적으로 가능한 만큼 획득한다.
- `Get Items`를 한 번이라도 누른 뒤 `Done`을 누르면 남은 아이템을 추가 획득하지 않고 기존 승리 후 안전지대/FloorMap 전환을 실행한다.
- `CombatRewardPage`를 실제 동작 코드로 교체해 EXP/Gold Value, ItemRow 선택/수량/숨김, Get Items, Done을 연결했다.
- Combat 씬 `RewardPanel`에 `CombatRewardPage`를 부착하고 `CombatHud.RewardPage`에 연결했다.
- `ItemRow_01~06`에 실제 `Button`을 추가하고, RewardPanel은 전투 시작 시 비활성 상태로 저장했다.
- 6종을 넘는 고유 드랍은 첫 ItemRow를 복제해 ScrollArea에 표시한다.
- RewardPanel 참조가 없거나 초기화에 실패하면 아이템을 가능한 만큼 순차 획득한 뒤 기존 승리 후 전환을 실행한다.

검증:
- `CombatRewardClaimSession` RED→GREEN 검증에서 중복 집계, 선택 부분 획득, 실패 수량 유지, Get Items 이후 Done 무획득, Get Items 미사용 Done 순차 획득을 확인했다.
- Unity Editor 상호작용 검증에서 EXP/Gold Value 갱신, ItemRow 선택, 성공 행 숨김, 실패 행 유지, Get Items/Done 콜백 동작을 확인했다.
- `RewardGrant.TryGrantItem` 검증에서 스택 최대치 및 장비 슬롯 최대치 도달 시 인벤토리를 변경하지 않고 실패하는 것을 확인했다.
- `RewardGrant.ApplyNonItemRewards`가 EXP/골드만 지급하고 드랍 아이템은 지급하지 않는 것을 확인했다.
- Combat 씬 정적 감사에서 RewardPanel 시작 비활성, ItemRow 버튼 6개, `CombatHud.RewardPage` 및 모든 직렬화 참조 연결을 확인했다.
- 8개 고유 아이템 보상으로 실행해 ItemRow 동적 복제와 ScrollArea 표시 경로를 확인했다.
- `dotnet build "MINI project/Assembly-CSharp.csproj"`: 경고 0 / 오류 0.
- Unity Console 최종 조회: Error 0 / Warning 0.

## 13. 2026-06-04 Safe1 Tavern Storage 실제 데이터 연결

수행:
- `LockerState`에 활성화 상태와 별도로 저장되는 용량을 추가했다.
  - 비활성 `0`, 활성화 `16`, 업그레이드마다 `+4`, 최대 `24`.
  - 스택 아이템 종류당 1칸, 장비 인스턴스당 1칸으로 사용량을 계산한다.
  - 가득 찬 보관함은 새 아이템 종류와 장비를 거부하며, 이미 존재하는 스택 수량 증가는 허용한다.
- `TavernStorage` 도메인 서비스를 추가했다.
  - 임시 비용은 활성화 `1 G`, 이후 업그레이드 `2 G`, `3 G`.
  - 활성화/업그레이드, 인벤토리↔보관함 이동, 폐기, 저장 체크포인트를 담당한다.
- `LockerSnapshot`에 용량을 저장/복원하고, 용량 필드가 없는 기존 저장은 활성 보관함을 `16`칸으로 보정한다.
- `TavernStorageUI`, `TavernStorageSlotView`를 추가했다.
  - 상태, 사용량/용량, 다음 비용/MAX, 실제 아이템 상세, 선택 수량, 이동/회수/폐기/활성화/업그레이드를 실제 런 데이터에 연결한다.
  - 아이템 선택 전 `AMOUNT 0`, 선택 후 `1`부터 선택한 실제 수량까지 조절한다.
  - 최대 용량에서는 `MAX`를 노란색으로 표시하고 업그레이드 버튼을 비활성화한다.
- Safe1 `Content_STORAGE`의 예시 아이템 텍스트를 전부 제거했다.
- 인벤토리 30칸과 보관함 최대 24칸을 4열 스크롤 슬롯으로 배치했다.
- 유지보수를 위해 신규 UI 코드에는 런타임 이름 검색, `AddComponent`, 버튼 `AddListener`를 사용하지 않았다.
- UI 참조 배열과 슬롯/동작 버튼의 `OnClick`은 Safe1 씬 Inspector 영구 참조로 직접 연결했다.

검증:
- 보관함 도메인 감사에서 활성화/업그레이드 비용 `1/2/3`, 용량 `16/20/24`, 최대 용량 차단, 기존 스택 추가 허용, 저장 복원을 확인했다.
- Safe1 정적 감사에서 직렬화 참조 누락 `0`, 슬롯 뷰 `54`, 슬롯 영구 이벤트 `54`, 동작 버튼 영구 이벤트 `6`을 확인했다.
- 모든 슬롯의 예시 아이템 텍스트가 비어 있고 기본 흰색 테두리이며, 슬롯 위치가 서로 겹치지 않음을 확인했다.
- `dotnet build "MINI project\Assembly-CSharp.csproj"`: 경고 0 / 오류 0.
- Unity Console 최종 조회: Error 0 / Warning 0.

## 14. 2026-06-04 Safe1 Tavern Lodging 실제 데이터 연결

수행:
- `TavernLodging` 도메인 서비스를 추가했다.
  - 숙박 인원은 플레이어 본인과 현재 활성 파티 동료를 합산한다.
  - 임시 비용은 1인당 `1 G`이며, 골드가 부족하면 휴식을 거부한다.
  - 성공 시 비용을 차감하고 플레이어와 활성 동료의 HP/MP를 완전 회복한 뒤 날짜를 하루 진행한다.
  - 날짜 변경 이벤트, 침식 일일 진행, 골드 변경 이벤트, 저장 체크포인트를 연결했다.
- `TavernLodgingUI`를 추가해 `Line_Lodging_Cost_Use_1Day`에 1인당 비용, 파티 인원, 총비용을 영어로 표시한다.
- Safe1 `Content_LODGING`에 `TavernLodgingUI`를 부착하고 비용 TMP, REST 버튼, `Rest()` OnClick을 Inspector 영구 참조로 직접 연결했다.
- REST 버튼은 대상 Image를 흰색으로 두고 일반/선택 상태는 회색, 눌림 상태는 빨간색으로 설정했다.
- 기존 눌림 색상이 보이지 않던 직접 원인은 REST 버튼이 `interactable=false`였고, 대상 Image 기본 회색과 ColorTint가 곱해지고 있었기 때문이다.

검증:
- 3인 파티에서 총비용 `3 G`, 골드 차감, 전원 HP/MP 완전 회복, 날짜 `+1`을 확인했다.
- 골드 부족 시 골드, HP/MP, 날짜가 변경되지 않음을 확인했다.
- Safe1 정적 감사에서 비용 TMP와 REST 버튼 직렬화 참조, `TavernLodgingUI.Rest` 영구 이벤트, 빨간 Pressed Color, 일반색과 같은 Selected Color를 확인했다.
- `dotnet build "MINI project/Assembly-CSharp.csproj"`: 경고 0 / 오류 0.

## 15. 2026-06-05 Safe1 Guild Content_COMPANIONS 연결

수행:
- `GuildCompanionsController`를 추가해 `Content_COMPANIONS`를 현재 파티 권위 데이터에 연결했다.
  - Slot 1은 Player, Slot 2~4는 `CurrentRun.Roster.Active` 최대 3명을 표시한다.
  - 빈 행 기본값은 `SLOT 1 EMPTY`~`SLOT 4 EMPTY`로 교체했다.
  - 선택한 파티원의 Level, EXP, Rune, Description을 `COMPANION_STATUS_Section`에 표시한다.
- `PrimaryButton_OPEN_RUNE_TREE`를 Safe1 기존 `RuneTreePanel`에 연결했다.
  - Header Name에 선택한 파티원 이름을 표시한다.
  - Player와 Companion 모두 읽기 전용으로 표시하며 직접 룬 투자는 차단한다.
  - 잠긴 룬도 선택해 DetailPanel의 Title, Type, Desc, 현재 투자 수치 / 최대 수치를 확인할 수 있다.
- 동료 룬은 표시용 `PlayerRuneState` 복사본으로 변환해 원본 `CompanionRuneState`를 변경하지 않는다.
- Safe1 시설 ESC 처리에서 열린 Guild RuneTreePanel을 먼저 닫도록 연결했다.
- `OWNED_COMPANIONS_Section` 행의 기존 Image가 `raycastTarget=false`여서 포인터 클릭이 차단되던 문제를 수정했다.
  - Safe1 씬의 4개 행 Image에 Raycast Target을 활성화했다.
  - `GuildCompanionsController`가 초기화 시 행 Button의 Target Graphic Raycast Target을 보장한다.

검증:
- Safe1 정적 감사에서 `GuildCompanionsController` 활성, 필수 직렬화 참조 누락 0, 4개 행 Button 연결, RuneTreePanel 기본 비활성을 확인했다.
- PlayMode 테스트 파티에서 Player + Active Companion + 빈 슬롯 순서와 상태 4종 표시를 확인했다.
- Companion RuneTreePanel Header Name, 전체 12개 룬 노드 선택 가능, 해금 룬 `1 / 1`, 잠긴 룬 `0 / 3`, DetailPanel 표시를 확인했다.
- Player와 Companion 양쪽에서 `OnUnlockClicked()` 호출 후 룬 진행 상태가 변경되지 않는 것을 확인했다.
- 실제 PointerClick 이벤트 경로로 Slot 2를 선택했을 때 Description이 Player에서 선택 동료 값으로 변경되는 것을 확인했다.
- `dotnet build "MINI project/Assembly-CSharp.csproj"`: 경고 0 / 오류 0.

## 16. 2026-06-05 전투 보상 동료 EXP 및 자동 룬 투자

수행:
- 전투 승리 EXP를 플레이어뿐 아니라 전투에 참가한 `CurrentRun.Roster.Active` 동료 각각에게도 동일하게 지급하도록 연결했다.
- 벤치 동료는 전투 미참가자이므로 EXP 지급 대상에서 제외했다.
- 동료가 레벨업할 때마다 현재 룬 트리에서 선행 조건을 만족하고 최대 투자치에 도달하지 않은 노드 중 하나에 자동으로 1포인트 투자한다.
- 자동 선택은 동료 Seed, 현재 Level, 투자 이력을 이용해 결정하며, 동료 Seed를 `CompanionSnapshot`에 저장/복원한다.
- 동료 룬 투자 이력은 같은 노드 중복을 허용하며, Guild 룬 트리에서 현재 투자치 / 최대 투자치로 표시한다.

검증:
- 실제 데이터로 활성 동료 2명과 벤치 동료 1명에게 `85 EXP` 보상을 지급했다.
  - 활성 동료 각각 `Lv.1 → Lv.4`, 잔여 EXP `5`, 자동 룬 투자 3회를 확인했다.
  - 벤치 동료는 `Lv.1`, EXP `0`을 유지하는 것을 확인했다.
- 모든 자동 선택이 선택 당시 선행 조건을 만족하고 최대 투자치에 도달하지 않은 노드만 대상으로 하는 것을 확인했다.
- 실제 룬 트리의 총 투자 가능량 `21`포인트까지 자동 투자 후 초과 투자 없이 후보가 소진되는 것을 확인했다.
- SaveSnapshot 왕복 후 동료 Seed, Level, EXP, 룬 투자 이력이 유지되고 다음 레벨업 자동 투자가 기존 진행을 이어가는 것을 확인했다.

## 17. 2026-06-05 전투 씬 아군 가로 배치

수행:
- 전투 런타임 아군 배치를 세로 배치에서 가로 배치로 변경했다.
- 플레이어와 활성 동료는 동일한 `y=1.35`에 플레이어부터 오른쪽 순서로 배치된다.
- 몬스터는 `y=2.35`에 배치해 아군보다 위쪽에 보이도록 진영 대비를 추가했다.
- 파티 인원에 따라 `x=-3.35`를 중심으로 `1.1` 간격을 유지해 1~4인 파티 모두 화면 안에서 중앙 정렬된다.

검증:
- 4인 파티 좌표가 `(-5.00, 1.35)`, `(-3.90, 1.35)`, `(-2.80, 1.35)`, `(-1.70, 1.35)` 순서로 계산되는 것을 확인했다.
- 몬스터 좌표가 인원수와 관계없이 `y=2.35`를 유지하는 것을 확인했다.
- Combat 카메라의 Orthographic Size `5` 및 화면 비율 기준으로 1~4인 아군 배치가 화면 범위 안에 유지되는 것을 확인했다.

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

## 3. 검증 결과

완료:
- `dotnet build "MINI project\Assembly-CSharp.csproj"` 통과.
  - 경고 0 / 오류 0.
- 성소/광산 관련 C# 파일, Balance 파일, csproj 대상 `git diff --check` 통과.
- `SaveSnapshot`에서 `MineActivated`, `MineStored`, `LastMineGainDay`가 저장/복원되는 코드 경로를 확인했다.
- 신규/변경 성소/광산 코드에서 `ManaStone +=`, `RaiseManaStoneChanged`, `OnManaStoneChanged`, 런타임 `AddComponent<...>` fallback 잔존 없음 확인.

미완료 / 제한:
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

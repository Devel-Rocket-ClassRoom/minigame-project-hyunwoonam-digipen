# Tempt — 전체 코드 재설계 (설계변경.txt 기반)

> 본 폴더는 **기존 코드와 분리된 신규 설계 골격**입니다.
> 모든 클래스는 끝에 `t` 접미사를 붙여 기존 코드와 이름이 충돌하지 않도록 합니다.
> 메소드 내부는 동작 주석만 작성되어 있고 실제 구현은 비어 있습니다.

## 폴더/레이어 구조

```text
Tempt/
├── Core/        싱글톤, GameSystemManagert, GameSceneManagert, DataManagert, HotkeyManagert, EventBust
├── Save/        SaveLoadert, SaveSnapshott
├── Data/        CSV/JSON 정적 데이터 모델
├── Entity/      EntityBaset → CharacterBaset → Playert/TeamBaset, MonsterBaset
├── Skill/       Skillt, SkillTargetTypet, SkillEffectt
├── Rune/        RuneNodet, RuneTreet, PlayerRuneStatet, CompanionRuneStatet, RuneTreeGeneratort
├── Item/        Itemt, InventoryStatet, EquipmentSlotst, ConsumableSlotst, LockerStatet
├── Combat/      CombatControllert, CombatFlowt, ActionQueuet, MonsterSpawnert, ActionSelector, TargetSelectort, DamageCalculatort
├── Floor/       FloorMapControllert, FloorMapCreatort, FloorMapModelt, FloorNodet
├── Safe/        SafeZoneControllerBaset, Safe0~5Controllert, 기능 모듈(Innt/Shopt/Guildt/Forget/Templet/ErosionAltart/Minet/Lockert)
├── MainMenu/    MainMenuControllert, NewGameConfirmPopupt
├── Erosion/     ErosionSystemt, ErosionStateModelt
├── UI/          UIManagert, UIPageControllerBaset, ResponsiveCanvasScalert, LanguageServicet, 단축키 페이지, 전투 HUD
├── Tutorial/    TutorialControllert, TutorialStept
└── Util/        WeightedRandomt, DebugLoggert
```

## 의존 방향

```text
Core  ← Data, Save, UI, Util
Data  ← Entity, Skill, Item, Rune
Entity← Skill, Item, Rune
Combat← Entity, Skill, Item, Floor 진입정보
Floor ← Combat
Safe  ← Item, Rune, Entity, Save, UI
MainMenu← Save, UI
Erosion← Floor, Safe
Save  ← (모든 도메인 상태 스냅샷)
UI    ← (도메인 직접 의존 금지, 이벤트/콜백으로만)
```

## 핵심 규약

```text
- 모든 클래스명 끝 t 접미사 (기존 이름 충돌 방지)
- 모든 public 멤버 위 /// <summary> doxygen XML 주석
- 메소드 내부는 동작/의도 주석만 작성 (실제 구현은 추후)
- 전투 FSM은 CombatFlowt 하나만 사용 (Player/Monster/Companion 별도 FSM 금지)
- 씬 전환은 GameSystemManagert → GameSceneManagert 경로
- 단축키는 어디서나 동작 (단, 소비 아이템 4칸 변경은 CombatControllert에서 차단)
- 스킬 패시브 제거 (룬으로 대체)
- 스탯: HP / MP / ATK / SPD / DEF / EXP 6종만
```

## 설계 결정 요약 (사용자 확정)

```text
1. 안전지대 = 6개 씬 분리 (Safe0t~Safe5t)
2. 4/12/20/30/40층 = 안전지대 층
3. UI = 글로벌 UIManagert + 씬별 PageController
4. 전투 라운드 = SPD + 가중치 보정 라운드 큐
5. 스킬 타겟 = Skillt 데이터의 SkillTargetTypet enum 일괄 구분
6. 동료 AI = 직업별 고정 우선순위 규칙
7. EXP = 노드 클리어 시 몬스터 EXP 합산 지급, 레벨업 시 UI 표시, 필요량 레벨업마다 증가
8. 인벤토리 = 보유 아이템 + 골드 / 장비(무기, 방어구 몸/팔/다리) / 소모 4칸 / 보관함 별개(주점 구매로 활성)
```

## 보류/추가 확인 필요 (HANDOFFtemp에 기록)

```text
- EXP/레벨/룬 진행의 세이브 포함 범위
- 보관함 데이터 수명 (Player당? 영구? 런 단위?)
- 동료 EXP 분배 규칙(개별 vs 파티 공유)
- 마석(자원) vs 골드(자원) 분리 사용처
- 튜토리얼 1회성 vs 세이브 단위 반복
- 시작 룬 풀(딜러/탱커/마법딜러/지원가) 외 추가 직업 여부
```

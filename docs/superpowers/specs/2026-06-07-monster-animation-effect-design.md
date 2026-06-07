# 몬스터 구조 — 애니메이션 · 이펙트 차별화 설계

> 작성일: 2026-06-07
> 원칙: 동작 보존(기존 미지정 시 현 동작 유지), 데이터 구동, YAGNI.
> 검증: Unity MCP(컴파일 0/0 + Play Mode 스폰/애니/이펙트 확인) + EditMode 테스트.

## 1. 배경 / 문제

- 런타임 몬스터 = `MonsterDefault.prefab`(`Monster : MonsterBase`) 호스트 + 자식 비주얼 프리팹(`SPUM_Prefabs` 보유).
- `CombatMonsterSpawner.CreateFallbackHost`가 `AddComponent<Monster>()` 하드코딩 → 모든 몬스터가 단일 `Monster` 행동.
- `Monster`는 빈 서브클래스(`base.PrepareForCombat()`만 호출) — 다형성 없는 보일러플레이트.
- `SkillData.AnimationKey` 필드 존재하나 **어디서도 미사용**(사장). SPUM `PlayAnimation`/`OverrideControllerInit`이 게임 코드에서 **한 번도 호출 안 됨** → 전투 애니메이션 미작동.
- 기본 공격 이펙트는 `CombatEffectPresenter`에 `"splash"` 하드코딩(전 몬스터 동일).
- 이펙트 스폰은 타겟 앵커 + `Quaternion.identity` + 프리팹 자체 스케일 + 수명 2초 고정 → 에셋별 크기/위치 보정 불가.

## 2. 목표 (확정 스코프)

1. 단일 데이터 구동 몬스터 클래스 — 상속 트리(Monster1/2/3) 도입 안 함. 특수 행동용 `virtual` 훅 추가 안 함(YAGNI).
2. 전투 애니메이션 배선: **IDLE**(평상시+방어) / **ATTACK**(공격·스킬) / **DAMAGED**(피격) / **DEATH**(사망). SPUM `PlayAnimation` 경유.
3. 기본 공격 이펙트 몬스터별 차별화(`MonsterData.AttackEffectKey`). 스킬 이펙트는 기존 `SkillData.EffectKey` 유지.
4. 공격 애니 클립 선택 몬스터별(`MonsterData.AttackAnimIndex`).
5. 이펙트 에셋별 크기/위치/회전/수명 보정(`CombatEffectConfig` 프리팹 컴포넌트).
6. **공격/스킬 효과음(SFX)**: 기본공격 몬스터별(`MonsterData.AttackSfxKey`), 스킬별(`SkillData.SfxKey`). 신규 `CombatSfxPresenter`.
7. 스폰 하드코딩(`AddComponent<Monster>`) 정리.

### 비목표 (스코프 컷)
- 몬스터별 특수 C# 행동 / 보스 기믹 (현재 불필요 — 필요 시점에 그때 한 마리만 subclass).
- 상태이상·약점상성(설계 확정 컷).
- 데이터 CSV→ScriptableObject 이관(과투자, save 호환 위험).

## 3. 설계

### 3.1 몬스터 클래스 통합
- `MonsterBase`(abstract) + 빈 `Monster` → **단일 concrete `Monster` 클래스로 통합**.
  - `MonsterBase.cs`의 데이터/런타임 로직 전부를 `Monster.cs`(`public sealed class Monster : EntityBase`)로 이전, `MonsterBase.cs` **삭제**.
  - `NodeRewardContribution`(현재 MonsterBase.cs에 동거) 도 `Monster.cs` 또는 별도 파일로 이동.
  - 외부 참조 `MonsterBase` → `Monster` 일괄 치환: `CombatMonsterSpawner`(`List<Monster> Spawned`, `GetComponent<Monster>()`, `AddComponent<Monster>()`), 기타 `MonsterBase` 사용처 전부.
  - `EntityBase` 차원 `override`(`PrepareForCombat` 등 entity-type 다형성)는 유지.
  - 결과: 몬스터 런타임 타입은 `Monster` 단일. 상속 트리·`virtual` 훅 없음.

### 3.2 SPUM 애니메이션 어댑터
> 구현 주의(검증 중 확정): SPUM은 asmdef 없이 `Assembly-CSharp`에 컴파일되며 `Tempt` asmdef가 컴파일 참조할 수 없다(Scripts↔Imported 의존 0 원칙). 따라서 `CombatUnitAnimator`는 plain 클래스로서 `Type.GetType("SPUM_Prefabs, Assembly-CSharp")` 런타임 리플렉션으로 `OverrideControllerInit`/`PopulateAnimationLists`/`PlayAnimation`를 바인딩한다. 전투 이벤트 시점만 호출되어 핫패스가 아니며, 부재·실패 시 no-op.

신규 `CombatUnitAnimator`(`Combat/`):
- 책임: 자식 `SPUM_Prefabs` 1개를 찾아 캐싱, 초기화, 상태 재생 래핑.
- API:
  ```
  void Initialize();                       // OverrideControllerInit + (필요 시) PopulateAnimationLists, 이후 IDLE
  void Play(PlayerState state, int index); // 가드/널/범위 체크 후 SPUM_Prefabs.PlayAnimation
  void PlayIdle(); PlayAttack(int index); PlayDamaged(); PlayDeath();
  ```
- 초기화 가드: `SPUM_Prefabs._anim == null`이거나 클립 리스트 비면 no-op(로그 1회). `allListsHaveItemsExist()` false면 `PopulateAnimationLists()` 시도.
- 인덱스 범위 초과 시 0으로 클램프(빈 리스트면 skip).
- **무손실**: 자식에 `SPUM_Prefabs` 없으면 전부 no-op → 기존(애니 없음) 동작 유지.

`EntityBase`에 `CombatUnitAnimator Animator` 참조 추가(전투 진입 시 `GetComponentInChildren` 1회 캐싱). 플레이어/동료 비주얼도 SPUM이면 자동 적용(공통). 단 데이터 키는 몬스터 우선.

### 3.3 애니메이션 훅 지점
| 시점 | 위치 | 호출 |
|---|---|---|
| 전투 시작 | `EntityBase.PrepareForCombat` | `Animator.Initialize()` → IDLE |
| 공격/스킬 실행 | `CombatFlow.ApplyImpact`(Attack/Skill 분기) | actor `PlayAttack(animIndex)` + `CombatEffectPresenter.Play` + `CombatSfxPresenter.Play`(기존 위치) |
| 피격 | `EntityBase.ApplyDamage`(기존 `PlayHitFx` 옆) | `IsDead ? PlayDeath() : PlayDamaged()` |
| 방어/행동 종료 | `EntityBase.OnRoundEnd` 또는 액션 종료 | `PlayIdle()` (사망 상태면 skip) |

- 공격 애니 인덱스: actor가 `MonsterBase`면 스킬이면 `SkillData`(향후 확장), 기본공격이면 `MonsterData.AttackAnimIndex`. 비-몬스터/미지정은 ATTACK index 0.
- DEATH 1회 재생 후 IDLE 복귀 금지(사망 플래그 가드).

### 3.4 이펙트 차별화
**기본 공격 이펙트 (몬스터별)**:
- `MonsterData`에 `AttackEffectKey`(string) 컬럼 추가(+ `MonsterStatusTable.csv` 컬럼, `FromRow`).
- `MonsterBase`에 `AttackEffectKey` 런타임 필드(`InitializeFromData`에서 복사).
- `CombatEffectPresenter.EffectKeyOf(Attack)`: `action.Actor`가 `MonsterBase`이고 `AttackEffectKey` 비어있지 않으면 그 키, 아니면 `"splash"`(fallback). → 무손실.

**스킬 이펙트**: 기존 `SkillData.EffectKey` 유지(변경 없음).

**공격 애니 인덱스**:
- `MonsterData.AttackAnimIndex`(int, 기본 0) 컬럼 추가(+ CSV/FromRow + 런타임 복사).

### 3.5 이펙트 에셋별 보정
신규 `CombatEffectConfig`(MonoBehaviour, 이펙트 프리팹에 부착):
```
Vector3 localScale = Vector3.one;
Vector3 offset     = Vector3.zero;   // 타겟 앵커 기준
Vector3 euler      = Vector3.zero;
float   lifetimeSec = 0f;            // 0 → 기본 2초
bool    attachToTarget = false;      // true → 타겟 자식으로 부착(추적)
```
- `CombatEffectPresenter.Play`: Instantiate 후 `GetComponent<CombatEffectConfig>()`:
  - 위치 = 앵커 + `offset`, 회전 = `euler`, 스케일 = `localScale`.
  - `attachToTarget`면 `SetParent(target.transform, true)`.
  - 수명 = `lifetimeSec > 0 ? lifetimeSec : 2f`.
  - 컴포넌트 없으면 기존 동작(앵커, identity, 프리팹 스케일, 2초) → **무손실**.

### 3.6 효과음(SFX)
- 오디오 인프라 전무(greenfield). `OptionsService`가 `AudioListener.volume = MasterVolume` 적용 → 모든 재생이 마스터 볼륨에 자동 스케일(별도 볼륨 연동 불필요, SFX 전용 볼륨 없음).
- 신규 `CombatSfxPresenter`(static, `CombatEffectPresenter`와 동형):
  ```
  void Play(CombatAction action);
  ```
  - 키 해석: Skill → `action.Skill.Data.SfxKey`; Attack → `action.Actor`가 `Monster`면 `AttackSfxKey`, 아니면 빈값.
  - 빈 키 → **재생 안 함(no-op, 무손실)**. 키 있으면 `Resources/Sfx/{key}` `AudioClip` 로드(캐시) → `AudioSource.PlayClipAtPoint(clip, pos, 1f)`(AudioListener.volume 자동 적용).
  - pos = 타겟 앵커(`CombatEffectPresenter`와 동일) 또는 actor 위치.
  - 클립 없으면 로그 1회 후 skip.
- `CombatFlow.ApplyImpact`에서 `CombatEffectPresenter.Play` 직후 `CombatSfxPresenter.Play(action)` 호출.

## 4. 데이터 스키마 변경
`MonsterStatusTable.csv` 신규 컬럼(맨 끝, 기존 행은 빈값 → fallback):
- `AttackEffectKey` (string, 빈값 → "splash")
- `AttackAnimIndex` (int, 빈값 → 0)
- `AttackSfxKey` (string, 빈값 → 무음)

`SkillTable.csv` 신규 컬럼:
- `SfxKey` (string, 빈값 → 무음)

`MonsterData`/`Monster`/`SkillData`에 대응 필드 + `FromRow`/`InitializeFromData` 매핑. 빈값 안전 파싱(`CsvParser.GetString/GetInt` 기본값). 기존 행/스킬은 빈값 → 무음(무손실).

## 5. 무손실 / 동작 보존 보장
- 신규 데이터 컬럼 미지정 → 기존 값("splash", index 0) → 현 동작 동일.
- `CombatEffectConfig`/`SPUM_Prefabs` 부재 → 어댑터·프리젠터 no-op → 현 동작 동일.
- 클래스 통합은 타입 식별자만 바뀌고 인스턴스 1개 부착·데이터 주입 경로 불변.
- 세이브 영향 0(몬스터는 런타임 전용, save 미포함).

## 6. 검증 계획
1. 컴파일 0/0 (MCP refresh + console).
2. EditMode: `MonsterData.FromRow` 신규 컬럼 파싱(빈값 기본값) 단위 테스트 추가. `CombatEffectConfig` 기본값 테스트.
3. Play Mode(MCP): Safe→Combat 진입 → 스폰된 몬스터가 IDLE 재생, 공격 시 ATTACK, 피격 DAMAGED, 사망 DEATH 전이 확인(예외 0). `SPUM_Prefabs` 초기화 NRE 없음.
4. 이펙트: 몬스터별 `AttackEffectKey` 다른 두 몬스터 스폰 → 다른 이펙트 확인. `CombatEffectConfig` offset/scale 적용 확인.
5. SFX: `AttackSfxKey`/`SfxKey` 지정 시 `Resources/Sfx/{key}` 재생(클립 로드 성공), 빈값 무음, AudioListener.volume 반영.
6. 회귀: 신규 컬럼 미지정 몬스터/스킬이 기존과 동일하게 splash + index0 + 무음 동작.

## 7. 영향 파일 (예상)
- 통합: `Entity/Monster/Monster.cs`(= 기존 MonsterBase 로직 전부 이전, `class Monster : EntityBase`), `Entity/Monster/MonsterBase.cs` **삭제**.
- 수정: `Combat/CombatMonsterSpawner.cs`(`MonsterBase`→`Monster` 치환), `Combat/CombatEffectPresenter.cs`(키 분기+config), `Combat/CombatFlow.cs`(ATTACK 훅 + SFX 호출), `Entity/EntityBase.cs`(animator 참조+DAMAGED/DEATH 훅), `Data/MonsterData.cs`(컬럼), `Data/SkillData.cs`(SfxKey), `Resources/Tables/MonsterStatusTable.csv`(컬럼), `Resources/Tables/SkillTable.csv`(SfxKey), 기타 `MonsterBase` 참조처 전부.
- 신규: `Combat/CombatUnitAnimator.cs`, `Combat/CombatEffectConfig.cs`, `Combat/CombatSfxPresenter.cs`.
- 프리팹: 이펙트 프리팹에 `CombatEffectConfig` 부착(디자이너, 선택).

## 8. 미해결 / 가정
- SPUM `PlayAnimation`의 ATTACK 클립 인덱스 체계는 각 몬스터 SPUM 패키지 구성에 의존 — 기본 index 0이 유효 ATTACK 클립이라 가정. 구현 중 실 프리팹로 확인.
- 플레이어/동료 SPUM 애니 적용은 부수효과(공통 어댑터). 스코프는 몬스터지만 무손실이라 허용. 원치 않으면 몬스터 한정으로 좁힘.

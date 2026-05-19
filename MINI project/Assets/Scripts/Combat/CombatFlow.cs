using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0414

/// <summary>
/// 전투의 턴 상태입니다.
/// </summary>
public enum CombatTurnState
{
    PreCombat,
    PlayerTurn,
    MonsterTurn,
    Victory,
    Defeat,
}

/// <summary>
/// Combat 씬의 턴 흐름을 단일 FSM으로 관리합니다.
/// </summary>
/// <remarks>
/// Week 1 범위:
/// - 선공은 ATK 비교 (몬스터 그룹 중 하나라도 더 높으면 몬스터 선공)
/// - 플레이어 행동 4종: Attack / Skill1 / Skill2 / Defend
/// - 아이템: 턴 미소비, Week 1엔 로그만 출력
/// - 몬스터 행동: Attack 70 / Skill 20 / Defend 10 가중치
/// - 데미지 = max(1, ATK - DEF), 방어 시 절반
/// - 모든 주요 지점에 Debug.Log 출력
/// Player / Monster는 별도 FSM을 갖지 않고 행동 데이터만 제공합니다.
/// </remarks>
public class CombatFlow : MonoBehaviour
{
    [Header("참조 (비워두면 씬에서 자동 탐색)")]
    [SerializeField]
    private Player player;

    [SerializeField]
    private List<MonsterBase> monsters = new List<MonsterBase>();

    [Header("전투 규칙")]
    [Tooltip("방어 중 받는 데미지 배율")]
    [SerializeField]
    private float defendDamageMultiplier = 0.5f;

    private CombatTurnState turnState = CombatTurnState.PreCombat;

    /// <summary>
    /// 현재 턴 상태입니다.
    /// </summary>
    public CombatTurnState TurnState => turnState;

    private void Start()
    {
        // 기존 구현:
        // ResolveSceneReferences();
        // if (!HasRequiredReferences())
        // {
        //     return;
        // }
        //
        // InitializeRuntimeState();
        //
        // bool playerFirst = DecideFirstAttacker();
        // Debug.Log(
        //     $"[CombatFlow] 선공 결정: {(playerFirst ? "Player" : "Monster")} "
        //         + $"(Player.ATK={player.ATK}, MonsterMaxATK={GetMaxMonsterATK()})"
        // );
        //
        // if (playerFirst)
        // {
        //     EnterPlayerTurn();
        // }
        // else
        // {
        //     EnterMonsterTurn();
        // }

        // TODO:
        // - 목표: Combat 씬 시작 시 참조, 런타임 상태, 선공자를 준비하고 첫 턴으로 진입한다.
        // - 의도: CombatFlow가 전투 전체의 단일 FSM 시작점이 되게 한다.
        // - 구현해야 할 것: 씬 참조 자동 탐색, 필수 참조 검증, HP/MP 리셋, 선공 결정, PlayerTurn/MonsterTurn 진입을 수행한다.
    }

    private void ResolveSceneReferences()
    {
        // 기존 구현:
        // if (player == null)
        // {
        //     player = FindAnyObjectByType<Player>();
        // }
        //
        // if (monsters == null || monsters.Count == 0)
        // {
        //     monsters = new List<MonsterBase>(
        //         FindObjectsByType<MonsterBase>(FindObjectsSortMode.None)
        //     );
        // }

        // TODO:
        // - 목표: Inspector에 연결되지 않은 Player와 MonsterBase 참조를 씬에서 자동 탐색한다.
        // - 의도: Combat 씬의 수동 연결 누락을 줄이고 데모 실행 가능성을 높인다.
        // - 구현해야 할 것: player가 null이면 Player를 찾고, monsters가 비어 있으면 MonsterBase 목록을 수집한다.
    }

    private bool HasRequiredReferences()
    {
        // 기존 구현:
        // if (player == null)
        // {
        //     Debug.LogError("[CombatFlow] Player reference is missing.");
        //     enabled = false;
        //     return false;
        // }
        //
        // if (monsters == null || monsters.Count == 0)
        // {
        //     Debug.LogError("[CombatFlow] Monster reference is missing.");
        //     enabled = false;
        //     return false;
        // }
        //
        // return true;

        // TODO:
        // - 목표: 전투 시작에 필요한 Player와 Monster 참조가 모두 존재하는지 검증한다.
        // - 의도: 참조 누락 상태에서 전투 로직이 NullReferenceException으로 진행되지 않게 한다.
        // - 구현해야 할 것: player와 monsters를 검사하고 누락 시 에러 로그와 enabled=false 처리 후 false를 반환한다.
        return default;
    }

    private void InitializeRuntimeState()
    {
        // 기존 구현:
        // player.ResetForNewCombat();
        //
        // foreach (var monster in monsters)
        // {
        //     monster.ResetForNewCombat();
        // }

        // TODO:
        // - 목표: Player와 모든 Monster의 전투 런타임 상태를 새 전투 상태로 초기화한다.
        // - 의도: 이전 전투의 HP/MP/방어 상태가 다음 전투에 남지 않게 한다.
        // - 구현해야 할 것: player.ResetForNewCombat()과 각 monster.ResetForNewCombat()을 호출한다.
    }

    /// <summary>
    /// 선공을 판별합니다. 현재는 ATK 비교 기반이며, 규칙 변경 시 이 함수만 수정하면 됩니다.
    /// 몬스터 그룹 중 한 마리라도 ATK가 플레이어보다 크면 몬스터 그룹이 선공합니다.
    /// </summary>
    private bool DecideFirstAttacker()
    {
        // 기존 구현:
        // return player.ATK >= GetMaxMonsterATK();

        // TODO:
        // - 목표: Player와 몬스터 그룹의 ATK를 비교해 선공자를 결정한다.
        // - 의도: 선공 규칙을 한 함수에 격리해 추후 Strategy 패턴이나 데이터 규칙으로 교체하기 쉽게 한다.
        // - 구현해야 할 것: player.ATK와 GetMaxMonsterATK()를 비교하고 동률은 Player 선공으로 처리한다.
        return default;
    }

    private int GetMaxMonsterATK()
    {
        // 기존 구현:
        // int max = 0;
        // foreach (var m in monsters)
        // {
        //     if (m.ATK > max)
        //     {
        //         max = m.ATK;
        //     }
        // }
        // return max;

        // TODO:
        // - 목표: 현재 몬스터 그룹 중 가장 높은 ATK를 계산한다.
        // - 의도: 선공 결정에서 몬스터 그룹 전체의 위협도를 단일 값으로 비교한다.
        // - 구현해야 할 것: monsters를 순회하며 최대 ATK를 찾아 반환한다.
        return default;
    }

    private void EnterPlayerTurn()
    {
        // 기존 구현:
        // turnState = CombatTurnState.PlayerTurn;
        // player.SetDefending(false);
        // Debug.Log("[CombatFlow] === Player Turn ===");

        // TODO:
        // - 목표: 전투 FSM을 PlayerTurn 상태로 전환한다.
        // - 의도: 플레이어 입력을 받을 수 있는 상태를 명확히 표시하고 방어 상태를 턴 시작에 해제한다.
        // - 구현해야 할 것: turnState 설정, player 방어 상태 초기화, 턴 진입 로그 출력을 수행한다.
    }

    private void EnterMonsterTurn()
    {
        // 기존 구현:
        // turnState = CombatTurnState.MonsterTurn;
        // Debug.Log("[CombatFlow] === Monster Turn ===");
        // ResolveAllMonsterActions();
        //
        // if (IsPlayerDefeated())
        // {
        //     EndCombatWithResult(DemoCombatResult.Defeat);
        //     return;
        // }
        //
        // EnterPlayerTurn();

        // TODO:
        // - 목표: 전투 FSM을 MonsterTurn 상태로 전환하고 모든 살아있는 몬스터 행동을 처리한다.
        // - 의도: 플레이어 메인 행동 이후 몬스터 턴을 자동으로 진행한다.
        // - 구현해야 할 것: turnState 설정, 몬스터 행동 처리, Player 사망 판정, 생존 시 PlayerTurn 복귀를 수행한다.
    }

    /// <summary>
    /// Attack 버튼: 기본 공격.
    /// </summary>
    public void OnAttackButton()
    {
        // 기존 구현:
        // if (turnState != CombatTurnState.PlayerTurn)
        // {
        //     return;
        // }
        //
        // int target = GetFirstAliveMonster();
        // if (target < 0)
        // {
        //     return;
        // }
        //
        // int dmg = CalculateDamage(player.ATK, monsters[target].DEF, monsters[target].IsDefending);
        // Debug.Log($"[CombatFlow] Player Attack → Monster[{target}], damage={dmg}");
        // ApplyDamageToMonster(target, dmg);
        //
        // AfterPlayerMainAction();

        // TODO:
        // - 목표: 플레이어 기본 공격 버튼 입력을 처리한다.
        // - 의도: UI 버튼은 CombatFlow 메서드만 호출하고 실제 턴 소비/피해 처리는 CombatFlow가 담당하게 한다.
        // - 구현해야 할 것: PlayerTurn 검증, 첫 생존 몬스터 선택, 데미지 계산/적용, 플레이어 메인 행동 후처리를 수행한다.
    }

    /// <summary>
    /// Skill 1 버튼: 안전지대에서 설정한 슬롯 1 스킬.
    /// </summary>
    public void OnSkill1Button()
    {
        // 기존 구현:
        // if (turnState != CombatTurnState.PlayerTurn)
        // {
        //     return;
        // }
        // UsePlayerSkill(player.Skill1);

        // TODO:
        // - 목표: 플레이어 Skill1 버튼 입력을 처리한다.
        // - 의도: 슬롯 1 액티브 스킬 사용을 공통 스킬 처리 함수로 위임한다.
        // - 구현해야 할 것: PlayerTurn 검증 후 player.Skill1을 UsePlayerSkill에 전달한다.
    }

    /// <summary>
    /// Skill 2 버튼: 안전지대에서 설정한 슬롯 2 스킬.
    /// </summary>
    public void OnSkill2Button()
    {
        // 기존 구현:
        // if (turnState != CombatTurnState.PlayerTurn)
        // {
        //     return;
        // }
        // UsePlayerSkill(player.Skill2);

        // TODO:
        // - 목표: 플레이어 Skill2 버튼 입력을 처리한다.
        // - 의도: 슬롯 2 액티브 스킬 사용을 공통 스킬 처리 함수로 위임한다.
        // - 구현해야 할 것: PlayerTurn 검증 후 player.Skill2를 UsePlayerSkill에 전달한다.
    }

    private void UsePlayerSkill(ActiveSkill skill)
    {
        // 기존 구현:
        // if (skill == null)
        // {
        //     Debug.Log("[CombatFlow] 빈 스킬 슬롯입니다.");
        //     return;
        // }
        //
        // if (player.CurrentMP < skill.mpCost)
        // {
        //     Debug.Log(
        //         $"[CombatFlow] MP 부족: [{skill.skillName}] (필요 {skill.mpCost} / 보유 {player.CurrentMP})"
        //     );
        //     return;
        // }
        //
        // int target = GetFirstAliveMonster();
        // if (target < 0)
        // {
        //     return;
        // }
        //
        // player.TrySpendMP(skill.mpCost);
        // int skillATK = Mathf.RoundToInt(player.ATK * skill.atkMultiplier);
        // int dmg = CalculateDamage(skillATK, monsters[target].DEF, monsters[target].IsDefending);
        // Debug.Log(
        //     $"[CombatFlow] Player Skill [{skill.skillName}] → Monster[{target}], "
        //         + $"damage={dmg}, MP-{skill.mpCost} ({player.CurrentMP}/{player.MaxMP})"
        // );
        // ApplyDamageToMonster(target, dmg);
        //
        // AfterPlayerMainAction();

        // TODO:
        // - 목표: 플레이어 액티브 스킬 사용을 처리한다.
        // - 의도: Skill1/Skill2 버튼이 같은 MP 검증, 대상 선택, 데미지 계산 흐름을 공유하게 한다.
        // - 구현해야 할 것: 빈 슬롯/MP 부족/대상 없음 검증, MP 소비, 배율 기반 데미지 계산, 피해 적용, 메인 행동 후처리를 수행한다.
    }

    /// <summary>
    /// Defend 버튼: 이번 턴 동안 받는 데미지 감소.
    /// </summary>
    public void OnDefendButton()
    {
        // 기존 구현:
        // if (turnState != CombatTurnState.PlayerTurn)
        // {
        //     return;
        // }
        //
        // player.SetDefending(true);
        // Debug.Log("[CombatFlow] Player Defend");
        //
        // AfterPlayerMainAction();

        // TODO:
        // - 목표: 플레이어 방어 버튼 입력을 처리한다.
        // - 의도: 방어를 메인 행동으로 취급하고 다음 몬스터 턴 피해를 감소시킨다.
        // - 구현해야 할 것: PlayerTurn 검증, player.SetDefending(true), 로그 출력, 메인 행동 후처리를 수행한다.
    }

    /// <summary>
    /// Item 버튼: 턴을 소비하지 않으며 메인 행동 확정 전까지 여러 번 사용 가능.
    /// Week 1에서는 로그만 출력합니다.
    /// </summary>
    public void OnItemButton()
    {
        // 기존 구현:
        // if (turnState != CombatTurnState.PlayerTurn)
        // {
        //     return;
        // }
        //
        // Debug.Log("[CombatFlow] Player Item Use (Week 1 stub, 턴 미소비)");

        // TODO:
        // - 목표: 플레이어 아이템 버튼 입력을 턴 미소비 보조 행동으로 처리한다.
        // - 의도: 메인 행동 전까지 아이템을 여러 번 사용할 수 있는 전투 규칙을 유지한다.
        // - 구현해야 할 것: PlayerTurn 검증, 아이템 보유/효과 처리, 턴 상태 유지, Week 1에서는 최소 로그 출력을 수행한다.
    }

    private void AfterPlayerMainAction()
    {
        // 기존 구현:
        // if (AreAllMonstersDead())
        // {
        //     EndCombatWithResult(DemoCombatResult.Victory);
        //     return;
        // }
        //
        // EnterMonsterTurn();

        // TODO:
        // - 목표: 플레이어 메인 행동 후 승리 여부를 판단하고 다음 턴으로 넘긴다.
        // - 의도: Attack/Skill/Defend가 동일한 턴 소비 후처리를 공유하게 한다.
        // - 구현해야 할 것: 모든 몬스터 사망 시 Victory 종료, 아니면 MonsterTurn 진입을 수행한다.
    }

    private void ResolveAllMonsterActions()
    {
        // 기존 구현:
        // foreach (var monster in monsters)
        // {
        //     monster.SetDefending(false);
        // }
        //
        // for (int i = 0; i < monsters.Count; i++)
        // {
        //     if (monsters[i].IsDead)
        //     {
        //         continue;
        //     }
        //
        //     ResolveSingleMonsterAction(i);
        //
        //     if (IsPlayerDefeated())
        //     {
        //         return;
        //     }
        // }

        // TODO:
        // - 목표: 살아있는 모든 몬스터의 행동을 순서대로 처리한다.
        // - 의도: MonsterTurn에서 그룹 전체 행동을 CombatFlow가 일괄 관리한다.
        // - 구현해야 할 것: 몬스터 방어 상태 초기화, 사망 몬스터 스킵, 단일 몬스터 행동 실행, Player 사망 시 조기 종료를 수행한다.
    }

    private void ResolveSingleMonsterAction(int index)
    {
        // 기존 구현:
        // MonsterBase m = monsters[index];
        // CombatActionType action = m.DecideAction();
        //
        // switch (action)
        // {
        //     case CombatActionType.Attack:
        //     {
        //         int dmg = CalculateDamage(m.ATK, player.DEF, player.IsDefending);
        //         Debug.Log($"[CombatFlow] Monster[{index}] Attack → Player, damage={dmg}");
        //         ApplyDamageToPlayer(dmg);
        //         break;
        //     }
        //     case CombatActionType.Skill:
        //     {
        //         ActiveSkill skill = m.GetActiveSkill(0);
        //         if (skill == null)
        //         {
        //             int fallbackDamage = CalculateDamage(m.ATK, player.DEF, player.IsDefending);
        //             Debug.Log(
        //                 $"[CombatFlow] Monster[{index}] Skill slot empty. Attack fallback → Player, damage={fallbackDamage}"
        //             );
        //             ApplyDamageToPlayer(fallbackDamage);
        //             break;
        //         }
        //
        //         int skillATK = Mathf.RoundToInt(m.ATK * skill.atkMultiplier);
        //         int dmg = CalculateDamage(skillATK, player.DEF, player.IsDefending);
        //         Debug.Log(
        //             $"[CombatFlow] Monster[{index}] Skill [{skill.skillName}] → Player, damage={dmg}"
        //         );
        //         ApplyDamageToPlayer(dmg);
        //         break;
        //     }
        //     case CombatActionType.Defend:
        //     {
        //         m.SetDefending(true);
        //         Debug.Log($"[CombatFlow] Monster[{index}] Defend");
        //         break;
        //     }
        // }

        // TODO:
        // - 목표: 특정 몬스터 한 마리의 결정된 행동을 실행한다.
        // - 의도: MonsterBase.DecideAction 결과를 CombatFlow의 피해/방어 처리와 연결한다.
        // - 구현해야 할 것: Attack/Skill/Defend 분기, 빈 스킬 슬롯 fallback, 데미지 계산/적용, 방어 상태 설정, 로그 출력을 수행한다.
    }

    private int CalculateDamage(int atk, int def, bool defending)
    {
        // 기존 구현:
        // int raw = Mathf.Max(1, atk - def);
        // if (defending)
        // {
        //     raw = Mathf.Max(1, Mathf.RoundToInt(raw * defendDamageMultiplier));
        // }
        // return raw;

        // TODO:
        // - 목표: ATK, DEF, 방어 상태를 바탕으로 최종 데미지를 계산한다.
        // - 의도: Week 1 데모의 단순 데미지 규칙을 한 함수에 격리한다.
        // - 구현해야 할 것: 최소 1 피해 보장, 방어 시 defendDamageMultiplier 적용, 정수 반올림 보정을 수행한다.
        return default;
    }

    private void ApplyDamageToPlayer(int dmg)
    {
        // 기존 구현:
        // int appliedDamage = player.TakeDamage(dmg);
        // Debug.Log($"[CombatFlow] Player HP: {player.CurrentHP}/{player.MaxHP} (-{appliedDamage})");
        // if (player.IsDead)
        // {
        //     Debug.Log("[CombatFlow] Player 사망");
        // }

        // TODO:
        // - 목표: 계산된 피해를 Player에게 적용하고 HP 변화와 사망을 기록한다.
        // - 의도: CombatFlow가 전투 로그와 사망 판정을 일관되게 관리한다.
        // - 구현해야 할 것: player.TakeDamage 호출, HP 로그 출력, player.IsDead일 때 사망 로그 출력을 수행한다.
    }

    private void ApplyDamageToMonster(int index, int dmg)
    {
        // 기존 구현:
        // int appliedDamage = monsters[index].TakeDamage(dmg);
        // Debug.Log(
        //     $"[CombatFlow] Monster[{index}] HP: {monsters[index].CurrentHP}/{monsters[index].MaxHP} (-{appliedDamage})"
        // );
        // if (monsters[index].IsDead)
        // {
        //     Debug.Log($"[CombatFlow] Monster[{index}] 사망");
        // }

        // TODO:
        // - 목표: 계산된 피해를 지정 몬스터에게 적용하고 HP 변화와 사망을 기록한다.
        // - 의도: 플레이어 행동 처리에서 몬스터 피해 적용을 공통화한다.
        // - 구현해야 할 것: monsters[index].TakeDamage 호출, HP 로그 출력, 사망 시 사망 로그 출력을 수행한다.
    }

    private int GetFirstAliveMonster()
    {
        // 기존 구현:
        // for (int i = 0; i < monsters.Count; i++)
        // {
        //     if (!monsters[i].IsDead)
        //     {
        //         return i;
        //     }
        // }
        // return -1;

        // TODO:
        // - 목표: 플레이어 행동의 기본 대상을 찾는다.
        // - 의도: 타겟 선택 UI가 없는 Week 1 상태에서 첫 생존 몬스터를 자동 타겟으로 사용한다.
        // - 구현해야 할 것: monsters를 앞에서부터 순회하고 첫 생존 몬스터 인덱스를 반환하며 없으면 -1을 반환한다.
        return default;
    }

    private bool AreAllMonstersDead()
    {
        // 기존 구현:
        // for (int i = 0; i < monsters.Count; i++)
        // {
        //     if (!monsters[i].IsDead)
        //     {
        //         return false;
        //     }
        // }
        // return true;

        // TODO:
        // - 목표: 전투 승리 조건인 몬스터 전멸 여부를 판단한다.
        // - 의도: 플레이어 메인 행동 후 Victory 전환 여부를 결정한다.
        // - 구현해야 할 것: 살아있는 몬스터가 하나라도 있으면 false, 모두 죽었으면 true를 반환한다.
        return default;
    }

    private bool IsPlayerDefeated()
    {
        // 기존 구현:
        // return player.IsDead;

        // TODO:
        // - 목표: 플레이어 패배 조건을 판단한다.
        // - 의도: 몬스터 행동 후 Defeat 전환 여부를 결정한다.
        // - 구현해야 할 것: player.IsDead 값을 반환한다.
        return default;
    }

    private void EndCombatWithResult(DemoCombatResult result)
    {
        // 기존 구현:
        // turnState =
        //     result == DemoCombatResult.Victory ? CombatTurnState.Victory : CombatTurnState.Defeat;
        // Debug.Log($"[CombatFlow] 전투 종료: {result}");
        // GameSystemManager.Instance.EndCombat(result);

        // TODO:
        // - 목표: 전투 FSM을 종료 상태로 바꾸고 결과를 GameSystemManager에 보고한다.
        // - 의도: CombatFlow가 직접 씬을 로드하지 않고 전투 결과만 상위 시스템에 전달한다.
        // - 구현해야 할 것: Victory/Defeat turnState 설정, 종료 로그 출력, GameSystemManager.Instance.EndCombat(result) 호출을 수행한다.
    }
}

#pragma warning restore CS0414

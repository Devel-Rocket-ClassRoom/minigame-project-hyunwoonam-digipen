using System;
using UnityEngine;

/// <summary>
/// 전투 중 특정 시점에 자동으로 발동하는 패시브 스킬입니다.
/// </summary>
/// <remarks>
/// Week 1 범위:
/// - 슬롯 구조와 트리거 / 효과 enum 만 정의합니다.
/// - 실제 효과 적용은 Week 2에서 EntityBase.ApplyPassives() 형태로 연결합니다.
/// </remarks>
[Serializable]
public class PassiveSkill : Skill
{
    [Tooltip("패시브 발동 트리거")]
    public PassiveTriggerType triggerType = PassiveTriggerType.OnCombatStart;

    [Tooltip("패시브 효과 종류")]
    public PassiveEffectType effectType = PassiveEffectType.BoostATK;

    [Tooltip("효과 수치 (트리거 / 효과별 의미 다름)")]
    public float value = 0f;
}

/// <summary>
/// 패시브 스킬 발동 트리거 종류입니다.
/// </summary>
public enum PassiveTriggerType
{
    OnCombatStart,
    OnTurnStart,
    OnDamaged,
    OnKill,
    Always,
}

/// <summary>
/// 패시브 스킬 효과 종류입니다.
/// </summary>
public enum PassiveEffectType
{
    BoostATK,
    BoostDEF,
    BoostMaxHP,
    HealOnKill,
    ReflectDamage,
}

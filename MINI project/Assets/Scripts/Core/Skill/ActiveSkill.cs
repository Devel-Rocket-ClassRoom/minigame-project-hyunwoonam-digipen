using System;
using UnityEngine;

/// <summary>
/// 턴 중 사용자가 발동시키는 액티브 스킬입니다.
/// </summary>
/// <remarks>
/// Week 1 범위:
/// - MP 소모와 ATK 배율 데미지만 처리합니다.
/// - 추가 효과(상태이상, 다중 타겟 등)는 Week 2에서 확장합니다.
/// </remarks>
[Serializable]
public class ActiveSkill : Skill
{
    [Tooltip("MP 소모량")]
    public int mpCost = 5;

    [Tooltip("ATK 대비 데미지 배율")]
    public float atkMultiplier = 1.5f;
}

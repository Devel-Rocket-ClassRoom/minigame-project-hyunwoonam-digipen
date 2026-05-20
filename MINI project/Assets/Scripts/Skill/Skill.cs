using System;
using UnityEngine;

/// <summary>
/// 모든 스킬의 공통 base 입니다.
/// </summary>
/// <remarks>
/// Active / Passive 두 종류로 갈라집니다.
/// 인스펙터에서 다형성으로 직렬화하기 위해 EntityBase는 Skill[]을
/// [SerializeReference]로 보유합니다.
/// HANDOFF3 §2-5 참고.
/// </remarks>
[Serializable]
public abstract class Skill
{
    [Tooltip("스킬 이름 (로그/UI 표시용)")]
    public string skillName = "기본 스킬";

    [Tooltip("스킬 설명 (UI 툴팁용)")]
    [TextArea]
    public string description = "";
}

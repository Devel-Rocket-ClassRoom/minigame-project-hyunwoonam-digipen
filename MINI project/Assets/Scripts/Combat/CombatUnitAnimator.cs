using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 전투 엔티티의 SPUM 애니메이션 구동 어댑터.
///
/// SPUM(`SPUM_Prefabs`)은 asmdef가 없어 predefined `Assembly-CSharp`에 컴파일되며,
/// `Tempt` asmdef는 predefined 어셈블리를 컴파일 참조할 수 없다(계획된 Scripts↔Imported 의존 0).
/// 따라서 타입/메서드를 런타임 리플렉션으로 바인딩한다. 전투 이벤트(공격/피격/사망/라운드)
/// 시점에만 호출되므로 핫패스가 아니다. SPUM 부재·초기화 실패 시 전부 no-op(무손실).
/// </summary>
public sealed class CombatUnitAnimator
{
    private const string Assembly = "Assembly-CSharp";

    private static bool resolved;
    private static Type spumType;
    private static MethodInfo initMethod;
    private static MethodInfo populateMethod;
    private static MethodInfo allListsMethod;
    private static MethodInfo playMethod;
    private static object stateIdle;
    private static object stateAttack;
    private static object stateDamaged;
    private static object stateDeath;

    private readonly Component spum;
    private bool initialized;
    private bool dead;

    private CombatUnitAnimator(Component spum)
    {
        this.spum = spum;
    }

    /// <summary>
    /// root(또는 자식)에 SPUM_Prefabs가 있으면 어댑터 생성, 없으면 null.
    /// </summary>
    public static CombatUnitAnimator TryCreate(GameObject root)
    {
        EnsureReflection();
        if (spumType == null || root == null)
        {
            return null;
        }

        Component c = root.GetComponentInChildren(spumType, true);
        return c != null ? new CombatUnitAnimator(c) : null;
    }

    private static void EnsureReflection()
    {
        if (resolved)
        {
            return;
        }

        resolved = true;
        spumType = Type.GetType("SPUM_Prefabs, " + Assembly);
        Type stateType = Type.GetType("PlayerState, " + Assembly);
        if (spumType != null)
        {
            initMethod = spumType.GetMethod("OverrideControllerInit");
            populateMethod = spumType.GetMethod("PopulateAnimationLists");
            allListsMethod = spumType.GetMethod("allListsHaveItemsExist");
            playMethod = spumType.GetMethod("PlayAnimation");
        }

        if (stateType != null)
        {
            stateIdle = Enum.Parse(stateType, "IDLE");
            stateAttack = Enum.Parse(stateType, "ATTACK");
            stateDamaged = Enum.Parse(stateType, "DAMAGED");
            stateDeath = Enum.Parse(stateType, "DEATH");
        }
    }

    /// <summary>SPUM 오버라이드 컨트롤러 초기화 후 IDLE 재생. 1회만.</summary>
    public void Initialize()
    {
        if (initialized || spum == null)
        {
            return;
        }

        try
        {
            bool hasLists = allListsMethod != null && (bool)allListsMethod.Invoke(spum, null);
            if (!hasLists)
            {
                populateMethod?.Invoke(spum, null);
            }

            initMethod?.Invoke(spum, null);
            initialized = true;
            PlayIdle();
        }
        catch (Exception e)
        {
            GameLog.LogWarning("[CombatUnitAnimator] Initialize 실패(무시): " + e.Message);
        }
    }

    public void PlayIdle()
    {
        if (!dead)
        {
            Play(stateIdle, 0);
        }
    }

    public void PlayAttack(int index)
    {
        if (!dead)
        {
            Play(stateAttack, Mathf.Max(0, index));
        }
    }

    public void PlayDamaged()
    {
        if (!dead)
        {
            Play(stateDamaged, 0);
        }
    }

    public void PlayDeath()
    {
        if (dead)
        {
            return;
        }

        dead = true;
        Play(stateDeath, 0);
    }

    private void Play(object state, int index)
    {
        if (spum == null || playMethod == null || state == null || !initialized)
        {
            return;
        }

        try
        {
            playMethod.Invoke(spum, new object[] { state, index });
        }
        catch (Exception)
        {
            // 클립 리스트 비었거나 인덱스 범위 밖 → 애니 생략(크래시 방지, 무손실).
        }
    }
}

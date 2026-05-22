using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 전투 피격 이펙트를 대상 엔티티의 EffectAnchor 위에 출력합니다.
/// </summary>
public static class CombatHitEffectPresenter
{
    public const string SplashPrefabAssetPath = "Assets/Imported/Use/splash.prefab";
    public const string SplashPrefabResourcePath = "Effects/splash";
    public const float DefaultEffectLifetimeSeconds = 1f;

    public static GameObject LoadSplashPrefab()
    {
#if UNITY_EDITOR
        GameObject editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SplashPrefabAssetPath);
        if (editorPrefab != null)
        {
            return editorPrefab;
        }
#endif

        return Resources.Load<GameObject>(SplashPrefabResourcePath);
    }

    public static GameObject SpawnSplashHitEffect(EntityBase target)
    {
        return SpawnHitEffect(target, LoadSplashPrefab(), DefaultEffectLifetimeSeconds);
    }

    public static GameObject SpawnHitEffect(
        EntityBase target,
        GameObject effectPrefab,
        float destroyAfterSeconds
    )
    {
        if (target == null || effectPrefab == null)
        {
            return null;
        }

        EntityWorldUI worldUI = EntityWorldUI.EnsureFor(target, target is MonsterBase);
        if (worldUI == null || worldUI.EffectAnchor == null)
        {
            return null;
        }

        Transform effectAnchor = worldUI.EffectAnchor;
        GameObject instance = Object.Instantiate(
            effectPrefab,
            effectAnchor.position,
            effectPrefab.transform.rotation
        );
        instance.transform.localScale = effectPrefab.transform.localScale;

        ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Play(true);
        }

        if (destroyAfterSeconds > 0f)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(instance, destroyAfterSeconds);
            }
            else
            {
                Object.DestroyImmediate(instance);
            }
        }

        return instance;
    }
}

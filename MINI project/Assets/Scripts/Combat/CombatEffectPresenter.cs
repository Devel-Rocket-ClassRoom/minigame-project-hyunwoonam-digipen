using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 액션에 대응하는 Resources/Effects 프리팹 재생기.
    /// </summary>
    public static class CombatEffectPresenter
    {
        private const string BasicAttackEffectKey = "basicattack";
        private const float EffectLifetimeSec = 2f;

        private static readonly Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

        public static void Play(CombatAction action)
        {
            string effectKey = EffectKeyOf(action);
            if (string.IsNullOrEmpty(effectKey) || action.Targets == null)
            {
                return;
            }

            GameObject prefab = LoadEffectPrefab(effectKey);
            if (prefab == null)
            {
                GameLog.LogError("[CombatEffectPresenter] Effect prefab missing: Resources/Effects/" + effectKey);
                return;
            }

            for (int i = 0; i < action.Targets.Count; i++)
            {
                EntityBase target = action.Targets[i];
                if (target == null)
                {
                    continue;
                }

                Vector3 anchor = target.WorldUI != null
                    ? target.WorldUI.EffectAnchorPosition
                    : target.transform.position + new Vector3(0f, 0.7f, 0f);

                SpawnEffect(prefab, anchor, target);
            }
        }

        private static void SpawnEffect(GameObject prefab, Vector3 anchor, EntityBase target)
        {
            // 이펙트 프리팹에 CombatEffectConfig가 있으면 에셋별 보정(스케일/오프셋/회전/수명/추적) 적용.
            // 없으면 기존 동작(앵커, identity, 프리팹 스케일, 기본 수명) → 무손실.
            CombatEffectConfig config = prefab.GetComponent<CombatEffectConfig>();
            Vector3 offset = config != null ? config.Offset : Vector3.zero;
            Quaternion rotation = config != null ? Quaternion.Euler(config.Euler) : Quaternion.identity;

            GameObject instance = Object.Instantiate(prefab, anchor + offset, rotation);

            if (config != null)
            {
                instance.transform.localScale = config.LocalScale;
                if (config.AttachToTarget && target != null)
                {
                    instance.transform.SetParent(target.transform, true);
                }
            }

            float lifetime = config != null && config.LifetimeSec > 0f ? config.LifetimeSec : EffectLifetimeSec;
            Object.Destroy(instance, lifetime);
        }

        private static string EffectKeyOf(CombatAction action)
        {
            if (action == null)
            {
                return string.Empty;
            }

            switch (action.Type)
            {
                case CombatActionType.Attack:
                    // 몬스터별 기본 공격 이펙트(빈값이면 공용 basicattack) — 무손실.
                    if (action.Actor is Monster monster && !string.IsNullOrEmpty(monster.AttackEffectKey))
                    {
                        return monster.AttackEffectKey;
                    }

                    return BasicAttackEffectKey;
                case CombatActionType.Skill:
                    return action.ResolvedSkillData?.EffectKey ?? string.Empty;
                default:
                    return string.Empty;
            }
        }

        private static GameObject LoadEffectPrefab(string effectKey)
        {
            if (prefabCache.TryGetValue(effectKey, out GameObject cached))
            {
                return cached;
            }

            GameObject prefab = Resources.Load<GameObject>("Effects/" + effectKey);
            prefabCache[effectKey] = prefab;
            return prefab;
        }
    }
}

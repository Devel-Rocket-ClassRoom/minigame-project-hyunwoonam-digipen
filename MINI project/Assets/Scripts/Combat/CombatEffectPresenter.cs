using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 액션에 대응하는 Resources/Effects 프리팹 재생기.
    /// </summary>
    public static class CombatEffectPresenter
    {
        private const string BasicAttackEffectKey = "splash";
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
                Debug.LogError("[CombatEffectPresenter] Effect prefab missing: Resources/Effects/" + effectKey);
                return;
            }

            for (int i = 0; i < action.Targets.Count; i++)
            {
                EntityBase target = action.Targets[i];
                if (target == null)
                {
                    continue;
                }

                Vector3 position = target.WorldUI != null
                    ? target.WorldUI.EffectAnchorPosition
                    : target.transform.position + new Vector3(0f, 0.7f, 0f);

                GameObject instance = Object.Instantiate(prefab, position, Quaternion.identity);
                Object.Destroy(instance, EffectLifetimeSec);
            }
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
                    return BasicAttackEffectKey;
                case CombatActionType.Skill:
                    return action.Skill?.Data?.EffectKey ?? string.Empty;
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

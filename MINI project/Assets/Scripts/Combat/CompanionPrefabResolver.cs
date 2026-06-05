using UnityEngine;

namespace Tempt
{
    public static class CompanionPrefabResolver
    {
        private const string ResourceRoot = "Companions";
        private const string PrefabExtension = ".prefab";

        public static string ToResourcePath(string prefabKey)
        {
            if (string.IsNullOrWhiteSpace(prefabKey))
            {
                return string.Empty;
            }

            string normalized = prefabKey.Trim().Replace('\\', '/');
            if (normalized.EndsWith(PrefabExtension, System.StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(0, normalized.Length - PrefabExtension.Length);
            }

            normalized = normalized.Trim('/');
            if (
                normalized.StartsWith(ResourceRoot + "/", System.StringComparison.OrdinalIgnoreCase)
            )
            {
                return normalized;
            }

            return ResourceRoot + "/" + normalized;
        }

        public static GameObject CreateRuntimeObject(
            CompanionInstance state,
            DataManager data,
            Transform parent,
            Vector3 position
        )
        {
            GameObject prefab = ResolvePrefab(state, data);
            GameObject go =
                prefab != null
                    ? Object.Instantiate(prefab, position, Quaternion.identity, parent)
                    : new GameObject(
                        "Companion.Runtime." + (state != null ? state.CompanionDataId : 0)
                    );

            if (prefab == null)
            {
                go.transform.SetParent(parent, false);
                go.transform.position = position;
            }
            else
            {
                go.name = "Companion.Runtime." + (state != null ? state.CompanionDataId : 0);
            }

            return go;
        }

        private static GameObject ResolvePrefab(CompanionInstance state, DataManager data)
        {
            if (
                state == null
                || data?.Companions == null
                || !data.Companions.TryGetValue(state.CompanionDataId, out CompanionData companion)
                || companion == null
                || string.IsNullOrWhiteSpace(companion.PrefabKey)
            )
            {
                return null;
            }

            string resourcePath = ToResourcePath(companion.PrefabKey);
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogError(
                    "[CompanionPrefabResolver] Companion prefab missing: Resources/" + resourcePath
                );
            }

            return prefab;
        }
    }
}

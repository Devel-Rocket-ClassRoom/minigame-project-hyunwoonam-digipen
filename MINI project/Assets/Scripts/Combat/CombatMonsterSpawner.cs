using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 선택한 FloorNode의 임시 몬스터 수에 맞춰 Combat 씬의 Monster1 개체를 준비합니다.
/// </summary>
public class CombatMonsterSpawner : MonoBehaviour
{
    private const string CombatSceneName = "Combat";
    private const int MaxTemporaryMonsterCount = 3;
    private const float HorizontalSpacing = 2f;
    private static readonly Vector3 DefaultMonsterSpawnOrigin = new Vector3(3.87f, 2.29f, 0f);

    public const string MonsterPrefabAssetPath = "Assets/prefabs/Monster1.prefab";
    public const string MonsterPrefabResourcePath = "Prefabs/Monster1";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoaded()
    {
        UnitySceneManager.sceneLoaded -= OnSceneLoaded;
        UnitySceneManager.sceneLoaded += OnSceneLoaded;
        EnsureController(UnitySceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureController(scene);
    }

    private static void EnsureController(Scene scene)
    {
        if (scene.name != CombatSceneName)
        {
            return;
        }

        if (FindAnyObjectByType<CombatMonsterSpawner>() != null)
        {
            return;
        }

        GameObject controller = new GameObject("CombatMonsterSpawner");
        controller.AddComponent<CombatMonsterSpawner>();
    }

    private void Awake()
    {
        SpawnForSelectedNode();
    }

    public static Vector3 GetCenteredSpawnPosition(Vector3 origin, int index, int count)
    {
        int safeCount = Mathf.Max(1, count);
        float startX = -(safeCount - 1) * HorizontalSpacing * 0.5f;
        return origin + new Vector3(startX + index * HorizontalSpacing, 0f, 0f);
    }

    public static Monster1 LoadMonsterPrefab()
    {
#if UNITY_EDITOR
        GameObject editorPrefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(
            MonsterPrefabAssetPath
        );
        if (editorPrefabObject != null)
        {
            return editorPrefabObject.GetComponent<Monster1>();
        }
#endif

        GameObject prefabObject = Resources.Load<GameObject>(MonsterPrefabResourcePath);
        if (prefabObject == null)
        {
            return null;
        }

        return prefabObject.GetComponent<Monster1>();
    }

    private void SpawnForSelectedNode()
    {
        if (!GameSystemManager.TryGetInstance(out GameSystemManager gameSystemManager))
        {
            Debug.LogWarning(
                "[CombatMonsterSpawner] GameSystemManager is missing. Keeping scene monsters."
            );
            return;
        }

        FloorNodeData selectedNode = gameSystemManager.SelectedCombatNode;
        if (selectedNode == null)
        {
            Debug.Log("[CombatMonsterSpawner] No selected FloorNode. Keeping scene monsters.");
            return;
        }

        int desiredCount = Mathf.Clamp(selectedNode.MonsterCount, 1, MaxTemporaryMonsterCount);
        List<Monster1> monsters = new List<Monster1>(
            FindObjectsByType<Monster1>(FindObjectsSortMode.None)
        );
        Vector3 origin = GetSpawnOrigin(monsters);
        Monster1 prefab = LoadMonsterPrefab();

        if (prefab != null)
        {
            DisableSceneMonsters(monsters);
            SpawnFromPrefab(prefab, origin, desiredCount);
            Debug.Log(
                $"[CombatMonsterSpawner] Spawned {desiredCount} prefab Monster1 for node={selectedNode.NodeIndex}, difficulty={selectedNode.Difficulty}."
            );
            return;
        }

        if (monsters.Count == 0)
        {
            Debug.LogWarning(
                "[CombatMonsterSpawner] No Monster1 prefab or scene template found in Combat scene."
            );
            return;
        }

        while (monsters.Count < desiredCount)
        {
            GameObject clone = Instantiate(monsters[0].gameObject, monsters[0].transform.parent);
            clone.name = $"Monster_{monsters.Count + 1}";
            Monster1 monster = clone.GetComponent<Monster1>();
            monsters.Add(monster);
        }

        for (int i = monsters.Count - 1; i >= desiredCount; i--)
        {
            Destroy(monsters[i].gameObject);
            monsters.RemoveAt(i);
        }

        for (int i = 0; i < monsters.Count; i++)
        {
            monsters[i].transform.position = GetCenteredSpawnPosition(origin, i, monsters.Count);
            EntityWorldUI.EnsureFor(monsters[i], true);
        }

        Debug.Log(
            $"[CombatMonsterSpawner] Spawned {monsters.Count} Monster1 for node={selectedNode.NodeIndex}, difficulty={selectedNode.Difficulty}."
        );
    }

    private static Vector3 GetSpawnOrigin(List<Monster1> sceneMonsters)
    {
        for (int i = 0; i < sceneMonsters.Count; i++)
        {
            if (sceneMonsters[i] != null)
            {
                return sceneMonsters[i].transform.position;
            }
        }

        return DefaultMonsterSpawnOrigin;
    }

    private static void DisableSceneMonsters(List<Monster1> sceneMonsters)
    {
        for (int i = 0; i < sceneMonsters.Count; i++)
        {
            if (sceneMonsters[i] == null)
            {
                continue;
            }

            sceneMonsters[i].gameObject.SetActive(false);
            Destroy(sceneMonsters[i].gameObject);
        }
    }

    private static void SpawnFromPrefab(Monster1 prefab, Vector3 origin, int desiredCount)
    {
        for (int i = 0; i < desiredCount; i++)
        {
            Vector3 position = GetCenteredSpawnPosition(origin, i, desiredCount);
            Monster1 monster = Instantiate(prefab, position, Quaternion.identity);
            monster.name = $"Monster_{i + 1}";
            EntityWorldUI.EnsureFor(monster, true);
        }
    }
}

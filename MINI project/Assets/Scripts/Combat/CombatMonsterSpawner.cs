using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

/// <summary>
/// 선택한 FloorNode의 임시 몬스터 수에 맞춰 Combat 씬의 Monster1 개체를 준비합니다.
/// </summary>
public class CombatMonsterSpawner : MonoBehaviour
{
    private const string CombatSceneName = "Combat";
    private const int MaxTemporaryMonsterCount = 3;
    private const float HorizontalSpacing = 2f;

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

        if (monsters.Count == 0)
        {
            Debug.LogWarning("[CombatMonsterSpawner] No Monster1 template found in Combat scene.");
            return;
        }

        Monster1 template = monsters[0];
        Vector3 origin = template.transform.position;

        while (monsters.Count < desiredCount)
        {
            GameObject clone = Instantiate(template.gameObject, template.transform.parent);
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
        }

        Debug.Log(
            $"[CombatMonsterSpawner] Spawned {monsters.Count} Monster1 for node={selectedNode.NodeIndex}, difficulty={selectedNode.Difficulty}."
        );
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 시작 시 노드 정보에 맞춰 몬스터를 생성/배치한다.
    /// </summary>
    public sealed class CombatMonsterSpawner : MonoBehaviour
    {
        private const string MonsterResourceRoot = "Monsters/";
        private const string RuntimeHostPrefabPath = "Monsters/MonsterDefault";

        /// <summary>생성된 몬스터들.</summary>
        public List<Monster> SpawnedT = new List<Monster>();

        /// <summary>
        /// 노드에서 몬스터를 무작위 선택 후 생성.
        /// </summary>
        public void SpawnFromNode(FloorNode node)
        {
            SpawnFromNode(node, null, null);
        }

        /// <summary>
        /// 노드에서 몬스터를 무작위 선택 후 지정된 런타임 위치에 생성한다.
        /// </summary>
        public void SpawnFromNode(FloorNode node, Transform parent, IReadOnlyList<Vector3> spawnPositions)
        {
            // 동작 요약:
            // - DataManager.PickMonsterGroup(node.Difficulty, node.MonsterCount) 호출.
            // - 기존 씬 몬스터 비활성/제거.
            // - 각 ID에 대해 Resources/Addressables에서 프리팹 로드.
            // - Instantiate, Monster.InitializeFromData(data, ErosionSystem 조회 배수).
            // - 배치 간격 일정하게 정렬.
            // - SpawnedT에 누적.
            Cleanup();
            if (node == null || !GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return;
            }

            float erosionMultiplier = gsm.Erosion != null ? gsm.Erosion.ComputeMonsterMultiplier(node.StageIndex) : 1f;
            IList<int> monsterIds = node.IsBoss
                ? new List<int> { PickBossMonsterId(gsm.Data, node.StageIndex) }
                : gsm.Data.PickMonsterGroup(node.Difficulty, node.MonsterCount);
            float spacing = 1.45f;
            for (int i = 0; i < monsterIds.Count; i++)
            {
                if (!gsm.Data.Monsters.TryGetValue(monsterIds[i], out MonsterData data))
                {
                    continue;
                }

                Vector3 pos = spawnPositions != null && i < spawnPositions.Count
                    ? spawnPositions[i]
                    : transform.position + new Vector3(i * spacing, 0f, 0f);
                GameObject prefab = !string.IsNullOrEmpty(data.PrefabKey)
                    ? Resources.Load<GameObject>(MonsterResourceRoot + data.PrefabKey)
                    : null;
                if (prefab == null)
                {
                    GameLog.LogError("[CombatMonsterSpawner] Monster prefab missing: Resources/Monsters/" + data.PrefabKey);
                    continue;
                }

                GameObject go = InstantiateMonster(prefab, pos, parent);
                go.transform.position = pos;
                Monster monster = go.GetComponent<Monster>();
                if (monster == null)
                {
                    GameLog.LogError("[CombatMonsterSpawner] Monster missing on prefab: " + data.PrefabKey);
                    Destroy(go);
                    continue;
                }

                monster.InitializeFromData(data, erosionMultiplier);
                SpawnedT.Add(monster);
            }
        }

        /// <summary>정리.</summary>
        public void Cleanup()
        {
            // 동작 요약: SpawnedT 순회 Destroy + 리스트 비움.
            foreach (Monster monster in SpawnedT)
            {
                if (monster != null)
                {
                    Destroy(monster.gameObject);
                }
            }

            SpawnedT.Clear();
        }

        private static int PickBossMonsterId(DataManager data, int stageIndex)
        {
            int fallback = 0;
            foreach (MonsterData monster in data.Monsters.Values)
            {
                if (!monster.IsBoss)
                {
                    continue;
                }

                if (fallback == 0)
                {
                    fallback = monster.Id;
                }

                if (monster.StageIndex == stageIndex || monster.Id == 1900 + stageIndex)
                {
                    return monster.Id;
                }
            }

            return fallback;
        }

        private static GameObject InstantiateMonster(GameObject prefab, Vector3 pos, Transform parent)
        {
            if (prefab.GetComponent<Monster>() != null)
            {
                return Instantiate(prefab, pos, Quaternion.identity, parent);
            }

            GameObject hostPrefab = Resources.Load<GameObject>(RuntimeHostPrefabPath);
            GameObject host = hostPrefab != null
                ? Instantiate(hostPrefab, pos, Quaternion.identity, parent)
                : CreateFallbackHost(pos, parent);
            host.name = prefab.name;

            GameObject visual = Instantiate(prefab, host.transform);
            visual.name = prefab.name + "_Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = prefab.transform.localScale;
            DisableCircleArtifacts(visual);

            return host;
        }

        private static GameObject CreateFallbackHost(Vector3 pos, Transform parent)
        {
            GameObject host = new GameObject("Monster");
            host.transform.SetParent(parent, false);
            host.transform.position = pos;
            host.AddComponent<Monster>();
            return host;
        }

        private static void DisableCircleArtifacts(GameObject visualRoot)
        {
            if (visualRoot == null)
            {
                return;
            }

            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.gameObject.name.Contains("Circle"))
                {
                    renderer.enabled = false;
                }
            }
        }
    }
}


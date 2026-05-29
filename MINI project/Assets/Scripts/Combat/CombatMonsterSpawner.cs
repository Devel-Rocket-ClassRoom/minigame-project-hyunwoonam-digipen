using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 시작 시 노드 정보에 맞춰 몬스터를 생성/배치한다.
    /// </summary>
    public sealed class CombatMonsterSpawner : MonoBehaviour
    {
        /// <summary>생성된 몬스터들.</summary>
        public List<MonsterBase> SpawnedT = new List<MonsterBase>();

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
            // - Instantiate, MonsterBase.InitializeFromData(data, ErosionSystem 조회 배수).
            // - 배치 간격 일정하게 정렬.
            // - SpawnedT에 누적.
            //TODO: Cleanup(); // 이전 몬스터 제거
            //TODO: List<int> monsterIds = GameSystemManager.Instance.Data.PickMonsterGroup(node.Difficulty, node.MonsterCount);
            //TODO: float spacing = 2.0f; // 배치 간격(단위: Unity 미터)
            //TODO: for (int i = 0; i < monsterIds.Count; i++)
            //TODO: {
            //TODO:     MonsterData data = GameSystemManager.Instance.Data.Monsters[monsterIds[i]];
            //TODO:     GameObject prefab = Resources.Load<GameObject>("Monsters/" + data.PrefabKey);
            //TODO:     Vector3 pos = new Vector3(i * spacing, 0, 0);
            //TODO:     GameObject go = Instantiate(prefab, pos, Quaternion.identity);
            //TODO:     MonsterBase monster = go.GetComponent<MonsterBase>();
            //TODO:     monster.InitializeFromData(data, erosionMultiplier);
            //TODO:     SpawnedT.Add(monster);
            //TODO: }
            Cleanup(); //Wave0write
            if (node == null || !GameSystemManager.TryGetInstance(out GameSystemManager gsm)) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            float erosionMultiplier = gsm.Erosion != null ? gsm.Erosion.ComputeMonsterMultiplier(node.StageIndex) : 1f; //Wave0write
            IList<int> monsterIds = node.IsBoss //Wave0write
                ? new List<int> { PickBossMonsterId(gsm.Data, node.StageIndex) } //Wave0write
                : gsm.Data.PickMonsterGroup(node.Difficulty, node.MonsterCount); //Wave0write
            float spacing = 1.45f; //Wave0write
            for (int i = 0; i < monsterIds.Count; i++) //Wave0write
            { //Wave0write
                if (!gsm.Data.Monsters.TryGetValue(monsterIds[i], out MonsterData data)) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                Vector3 pos = spawnPositions != null && i < spawnPositions.Count
                    ? spawnPositions[i]
                    : transform.position + new Vector3(i * spacing, 0f, 0f); //Wave0write
                GameObject prefab = !string.IsNullOrEmpty(data.PrefabKey) ? Resources.Load<GameObject>("Monsters/" + data.PrefabKey) : null; //Wave0write
                GameObject go = prefab != null ? Instantiate(prefab, pos, Quaternion.identity, parent) : new GameObject(data.NameKey); //Wave0write
                if (prefab == null && parent != null) //Wave0write
                { //Wave0write
                    go.transform.SetParent(parent, false); //Wave0write
                } //Wave0write
                go.transform.position = pos; //Wave0write
                MonsterBase monster = go.GetComponent<MonsterBase>(); //Wave0write
                if (monster == null) //Wave0write
                { //Wave0write
                    monster = go.AddComponent<Monster1>(); //Wave0write
                } //Wave0write

                monster.InitializeFromData(data, erosionMultiplier); //Wave0write
                SpawnedT.Add(monster); //Wave0write
            } //Wave0write
        }

        /// <summary>정리.</summary>
        public void Cleanup()
        {
            // 동작 요약: SpawnedT 순회 Destroy + 리스트 비움.
            //TODO: foreach (var m in SpawnedT) if (m != null) Destroy(m.gameObject);
            //TODO: SpawnedT.Clear();
            foreach (MonsterBase monster in SpawnedT) //Wave0write
            { //Wave0write
                if (monster != null) //Wave0write
                { //Wave0write
                    Destroy(monster.gameObject); //Wave0write
                } //Wave0write
            } //Wave0write

            SpawnedT.Clear(); //Wave0write
        }

        private static int PickBossMonsterId(DataManager data, int stageIndex) //Wave0write
        { //Wave0write
            int fallback = 0; //Wave0write
            foreach (MonsterData monster in data.Monsters.Values) //Wave0write
            { //Wave0write
                if (!monster.IsBoss) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                if (fallback == 0) //Wave0write
                { //Wave0write
                    fallback = monster.Id; //Wave0write
                } //Wave0write

                if (monster.Id == 1900 + stageIndex) //Wave0write
                { //Wave0write
                    return monster.Id; //Wave0write
                } //Wave0write
            } //Wave0write

            return fallback; //Wave0write
        } //Wave0write
    }
}


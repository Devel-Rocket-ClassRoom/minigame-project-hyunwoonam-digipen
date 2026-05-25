using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 전투 시작 시 노드 정보에 맞춰 몬스터를 생성/배치한다.
    /// </summary>
    public sealed class CombatMonsterSpawnert : MonoBehaviour
    {
        /// <summary>생성된 몬스터들.</summary>
        public List<MonsterBaset> SpawnedT = new List<MonsterBaset>();

        /// <summary>
        /// 노드에서 몬스터를 무작위 선택 후 생성.
        /// </summary>
        public void SpawnFromNode(FloorNodet node, float erosionMultiplier)
        {
            // 동작 요약:
            // - DataManagert.PickMonsterGroup(node.Difficulty, node.MonsterCount) 호출.
            // - 기존 씬 몬스터 비활성/제거.
            // - 각 ID에 대해 Resources/Addressables에서 프리팹 로드.
            // - Instantiate, MonsterBaset.InitializeFromData(data, erosionMultiplier).
            // - 배치 간격 일정하게 정렬.
            // - SpawnedT에 누적.
        }

        /// <summary>정리.</summary>
        public void Cleanup()
        {
            // 동작 요약: SpawnedT 순회 Destroy + 리스트 비움.
        }
    }
}

using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 몬스터 베이스. CharacterBaset를 사용하지 않으며 룬/EXP 미보유.
    /// 행동은 MonsterActionSelectort가 가중치로 선택. 침식 변이 셰이더 적용 훅 보유.
    /// </summary>
    public abstract class MonsterBaset : EntityBaset
    {
        /// <summary>몬스터 데이터 ID.</summary>
        public int MonsterDataId;

        /// <summary>행동 가중치.</summary>
        public ActionWeightTablet ActionWeights;

        /// <summary>처치 시 지급 EXP.</summary>
        public int ExpReward;

        /// <summary>드랍 골드 평균.</summary>
        public int GoldDropAvg;

        /// <summary>침식 변이 적용 여부.</summary>
        public bool IsEroded { get; private set; }

        /// <summary>
        /// 데이터에서 초기화. CombatMonsterSpawnert가 호출.
        /// </summary>
        public void InitializeFromData(MonsterDatat data, float erosionMultiplier)
        {
            // 동작 요약:
            // - Stats 셋업 (data + erosion 배수 적용).
            // - ActiveSkills 슬롯에 data.SkillIds 매핑.
            // - ActionWeights, ExpReward, GoldDropAvg 복사.
            // - erosionMultiplier > 1 → ApplyErosionVisual().
        }

        /// <summary>
        /// 침식 시각화 적용(어두운 색조).
        /// </summary>
        public void ApplyErosionVisual()
        {
            // 동작 요약:
            // - IsEroded = true.
            // - SpriteRenderer/머티리얼에 침식 셰이더 키 적용.
        }

        /// <summary>
        /// 처치 시 보상 정보 반환. CombatControllert가 합산용으로 사용.
        /// </summary>
        public NodeRewardContributiont GetRewardContribution()
        {
            // 동작 요약: ExpReward + 정규분포 골드 표본 반환.
            return null;
        }
    }

    /// <summary>한 마리가 노드 보상에 기여하는 분.</summary>
    public sealed class NodeRewardContributiont
    {
        /// <summary>지급 EXP.</summary>
        public int Exp;

        /// <summary>드랍 골드.</summary>
        public int Gold;

        /// <summary>드랍 아이템(있다면).</summary>
        public List<int> DroppedItemIds;
    }
}

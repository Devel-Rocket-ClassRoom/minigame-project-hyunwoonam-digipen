using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 데이터 테이블로 초기화되는 단일 몬스터 런타임.
    /// CharacterBase를 사용하지 않으며 룬/EXP 미보유.
    /// 행동은 MonsterActionSelector가 가중치로 선택. 침식 변이 셰이더 적용 훅 보유.
    /// 드랍은 DropTableId를 통해 DataManager.ResolveDrops()로 결정.
    /// 스탯/스킬/드랍/이펙트/애니/SFX 차별화는 모두 데이터(MonsterData)로 주입한다.
    /// 상속 트리·virtual 훅 없음(특수 행동이 실제로 필요한 시점에 그때 도입).
    /// </summary>
    public sealed class Monster : EntityBase
    {
        /// <summary>몬스터 데이터 ID.</summary>
        public int MonsterDataId;

        /// <summary>행동 가중치.</summary>
        public ActionWeightTable ActionWeights;

        /// <summary>처치 시 지급 EXP.</summary>
        public int RewardExp;

        /// <summary>처치 시 지급 골드.</summary>
        public int RewardGold;

        /// <summary>
        /// 드랍 테이블 ID. DataManager.ResolveDrops(DropTableId)로 실제 드랍 결정.
        /// 0이면 드랍 없음.
        /// InitializeFromData에서 데이터 테이블 값을 복사한다.
        /// </summary>
        public int DropTableId;

        /// <summary>침식 변이 적용 여부.</summary>
        public bool IsEroded { get; private set; }

        /// <summary>기본 공격 이펙트 키(빈값이면 공용 "basicattack").</summary>
        public string AttackEffectKey;

        /// <summary>기본 공격 SPUM ATTACK 클립 인덱스.</summary>
        public int AttackAnimIndex;

        /// <summary>기본 공격 효과음 키(빈값이면 무음).</summary>
        public string AttackSfxKey;

        /// <summary>
        /// 데이터에서 초기화. CombatMonsterSpawner가 호출.
        /// </summary>
        public void InitializeFromData(MonsterData data, float erosionMultiplier)
        {
            // 동작 요약:
            // - MonsterDataId = data.Id.
            // - Stats 셋업 (data 수치 + erosionMultiplier 배수 적용, 1.5배 기본).
            // - ActiveSkills 슬롯에 data.SkillIds 매핑.
            // - ActionWeights, RewardExp, RewardGold 복사.
            // - DropTableId = data.DropTableId.
            // - erosionMultiplier > 1 → ApplyErosionVisual().
            if (data == null)
            {
                return;
            }

            MonsterDataId = data.Id;
            DisplayName = data.NameKey;
            Stats = new StatBlock();
            float mult = Mathf.Max(1f, erosionMultiplier);
            Stats.SetBaseStats(
                Mathf.RoundToInt(data.MaxHP * mult),
                data.MaxMP,
                Mathf.RoundToInt(data.ATK * mult),
                Mathf.RoundToInt(data.DEF * mult),
                data.SPD);
            Stats.RestoreToFull();

            ActiveSkills = new Skill[2];
            if (data.SkillIds != null && GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                for (int i = 0; i < data.SkillIds.Count && i < ActiveSkills.Length; i++)
                {
                    if (gsm.Data.Skills.TryGetValue(data.SkillIds[i], out SkillData skillData))
                    {
                        ActiveSkills[i] = new Skill(skillData);
                    }
                }
            }

            ActionWeights = data.ActionWeights ?? ActionWeightTable.Default;
            RewardExp = data.RewardExp;
            RewardGold = data.RewardGold;
            DropTableId = data.DropTableId;
            AttackEffectKey = data.AttackEffectKey;
            AttackAnimIndex = data.AttackAnimIndex;
            AttackSfxKey = data.AttackSfxKey;
            if (erosionMultiplier > 1f)
            {
                ApplyErosionVisual();
            }
        }

        /// <summary>
        /// 침식 시각화 적용(어두운 색조).
        /// </summary>
        public void ApplyErosionVisual()
        {
            IsEroded = true;
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in renderers)
            {
                sr.color = Color.Lerp(sr.color, new Color(0.30f, 0.18f, 0.35f, 1f), 0.55f);
            }
        }

        /// <summary>
        /// 처치 시 보상 정보 반환. CombatController가 노드 클리어 시 합산용으로 사용.
        /// 드랍 아이템은 DataManager.ResolveDrops(DropTableId)로 확률 계산.
        /// </summary>
        public NodeRewardContribution GetRewardContribution()
        {
            // 동작 요약:
            // - DataManager data = GameSystemManager.Instance.Data.
            // - List<DroppedItemStack> drops = data.ResolveDrops(DropTableId).
            // - DroppedItemIds 목록 = drops.SelectMany(d => Enumerable.Repeat(d.ItemId, d.Count)).
            // - NodeRewardContribution { Exp = RewardExp, Gold = RewardGold, DroppedItemIds } 반환.
            var itemIds = new List<int>();
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                List<DroppedItemStack> drops = gsm.Data.ResolveDrops(DropTableId);
                foreach (DroppedItemStack stack in drops)
                {
                    for (int i = 0; i < stack.Count; i++)
                    {
                        itemIds.Add(stack.ItemId);
                    }
                }
            }

            return new NodeRewardContribution { Exp = RewardExp, Gold = RewardGold, DroppedItemIds = itemIds };
        }
    }

    /// <summary>한 마리가 노드 보상에 기여하는 분.</summary>
    public sealed class NodeRewardContribution
    {
        /// <summary>지급 EXP.</summary>
        public int Exp;

        /// <summary>드랍 골드.</summary>
        public int Gold;

        /// <summary>드랍 아이템 ID 목록(중복 허용, 수량 반영).</summary>
        public List<int> DroppedItemIds;
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 몬스터 베이스. CharacterBaset를 사용하지 않으며 룬/EXP 미보유.
    /// 행동은 MonsterActionSelectort가 가중치로 선택. 침식 변이 셰이더 적용 훅 보유.
    /// 드랍은 DropTableId를 통해 DataManager.ResolveDrops()로 결정.
    /// 각각의 몬스터 코드(Monster1 등)에서 DropTableId를 포함한 스탯/스킬/애니메이션을 재정의한다.
    /// </summary>
    public abstract class MonsterBase : EntityBase
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
        /// 각 몬스터 코드(Monster1 등)에서 재정의(override 또는 InitializeFromData 복사).
        /// </summary>
        public int DropTableId;

        /// <summary>침식 변이 적용 여부.</summary>
        public bool IsEroded { get; private set; }

        /// <summary>
        /// 데이터에서 초기화. CombatMonsterSpawnert가 호출.
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
            //TODO: MonsterDataId = data.Id;
            //TODO: Stats.BaseMaxHP = Mathf.RoundToInt(data.BaseMaxHP * erosionMultiplier);
            //TODO: Stats.BaseMaxMP = data.BaseMaxMP;
            //TODO: Stats.BaseATK   = Mathf.RoundToInt(data.BaseATK * erosionMultiplier);
            //TODO: Stats.BaseDEF   = Mathf.RoundToInt(data.BaseDEF * erosionMultiplier);
            //TODO: Stats.BaseSPD   = data.BaseSPD;
            //TODO: Stats.RestoreToFull();
            //TODO: // 스킬 매핑
            //TODO: ActiveSkills = new System.Collections.Generic.List<Skill>();
            //TODO: foreach (int sid in data.SkillIds)
            //TODO:     ActiveSkills.Add(new Skill(GameSystemManager.Instance.Data.Skills[sid]));
            //TODO: ActionWeights = data.ActionWeights;
            //TODO: RewardExp  = data.RewardExp;
            //TODO: RewardGold = data.RewardGold;
            //TODO: DropTableId = data.DropTableId;
            //TODO: if (erosionMultiplier > 1f) ApplyErosionVisual();
            if (data == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            MonsterDataId = data.Id; //Wave0write
            DisplayName = data.NameKey; //Wave0write
            Stats = new StatBlock(); //Wave0write
            float mult = Mathf.Max(1f, erosionMultiplier); //Wave0write
            Stats.SetBaseStats( //Wave0write
                Mathf.RoundToInt(data.MaxHP * mult), //Wave0write
                data.MaxMP, //Wave0write
                Mathf.RoundToInt(data.ATK * mult), //Wave0write
                Mathf.RoundToInt(data.DEF * mult), //Wave0write
                data.SPD); //Wave0write
            Stats.RestoreToFull(); //Wave0write

            ActiveSkills = new Skill[2]; //Wave0write
            if (data.SkillIds != null && GameSystemManager.TryGetInstance(out GameSystemManager gsm)) //Wave0write
            { //Wave0write
                for (int i = 0; i < data.SkillIds.Count && i < ActiveSkills.Length; i++) //Wave0write
                { //Wave0write
                    if (gsm.Data.Skills.TryGetValue(data.SkillIds[i], out SkillData skillData)) //Wave0write
                    { //Wave0write
                        ActiveSkills[i] = new Skill(skillData); //Wave0write
                    } //Wave0write
                } //Wave0write
            } //Wave0write

            ActionWeights = data.ActionWeights ?? new ActionWeightTable { Attack = 80, Skill = 0, Defend = 20 }; //Wave0write
            RewardExp = data.RewardExp; //Wave0write
            RewardGold = data.RewardGold; //Wave0write
            DropTableId = data.DropTableId; //Wave0write
            if (erosionMultiplier > 1f) //Wave0write
            { //Wave0write
                ApplyErosionVisual(); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 침식 시각화 적용(어두운 색조).
        /// </summary>
        public void ApplyErosionVisual()
        {
            IsEroded = true; //Wave0write
            SpriteRenderer sr = GetComponent<SpriteRenderer>(); //Wave0write
            if (sr != null) //Wave0write
            { //Wave0write
                // TEMP: 셰이더 에셋 부재로 SpriteRenderer.color lerp 만 적용. ErosionShaderKey 도입 시 교체.
                sr.color = Color.Lerp(sr.color, new Color(0.30f, 0.18f, 0.35f, 1f), 0.55f); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 처치 시 보상 정보 반환. CombatControllert가 노드 클리어 시 합산용으로 사용.
        /// 드랍 아이템은 DataManager.ResolveDrops(DropTableId)로 확률 계산.
        /// </summary>
        public NodeRewardContribution GetRewardContribution()
        {
            // 동작 요약:
            // - DataManager data = GameSystemManager.Instance.Data.
            // - List<DroppedItemStack> drops = data.ResolveDrops(DropTableId).
            // - DroppedItemIds 목록 = drops.SelectMany(d => Enumerable.Repeat(d.ItemId, d.Count)).
            // - NodeRewardContribution { Exp = RewardExp, Gold = RewardGold, DroppedItemIds } 반환.
            //TODO: DataManager dataMgr = GameSystemManager.Instance.Data;
            //TODO: List<DroppedItemStack> drops = dataMgr.ResolveDrops(DropTableId);
            //TODO: var itemIds = new List<int>();
            //TODO: foreach (var stack in drops)
            //TODO:     for (int i = 0; i < stack.Count; i++) itemIds.Add(stack.ItemId);
            //TODO: return new NodeRewardContribution { Exp = RewardExp, Gold = RewardGold, DroppedItemIds = itemIds };
            var itemIds = new List<int>(); //Wave0write
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm)) //Wave0write
            { //Wave0write
                List<DroppedItemStack> drops = gsm.Data.ResolveDrops(DropTableId); //Wave0write
                foreach (DroppedItemStack stack in drops) //Wave0write
                { //Wave0write
                    for (int i = 0; i < stack.Count; i++) //Wave0write
                    { //Wave0write
                        itemIds.Add(stack.ItemId); //Wave0write
                    } //Wave0write
                } //Wave0write
            } //Wave0write

            return new NodeRewardContribution { Exp = RewardExp, Gold = RewardGold, DroppedItemIds = itemIds }; //Wave0write
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


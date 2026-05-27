using System.Collections.Generic;
using UnityEngine;

namespace Tempt
{
    /// <summary>
    /// 모든 전투 엔티티(Player/Companion/Monster)의 공통 베이스.
    /// 보유: 스탯, 액티브 스킬 2칸, 패시브 스킬 목록(자동 적용), 공격/방어/피해 처리, 머리 위 UI 훅.
    /// 패시브 스킬은 CharacterBase.SyncPassivesFromRunes()가 룬 해금 시 자동으로 채우며
    /// 플레이어/동료가 직접 선택하거나 제거할 수 없다.
    /// </summary>
    public abstract class EntityBase : MonoBehaviour
    {
        /// <summary>스탯 블록.</summary>
        public StatBlock Stats { get; protected set; }

        /// <summary>액티브 스킬 2칸.</summary>
        public Skill[] ActiveSkills { get; protected set; } = new Skill[2];

        /// <summary>
        /// 패시브 스킬 목록.
        /// 룬 해금으로만 추가되며, 전투 시작/레벨업 시 SyncPassivesFromRunes()가 갱신한다.
        /// Monster는 이 목록을 사용하지 않는다(몬스터 스킬은 모두 ActionWeights 기반 액티브).
        /// </summary>
        public IReadOnlyList<Skill> PassiveSkills => passiveSkills;

        private List<Skill> passiveSkills = new List<Skill>();

        /// <summary>이번 라운드 동안 방어 중인가.</summary>
        public bool IsDefending { get; private set; }

        /// <summary>사망 여부.</summary>
        public bool IsDead => Stats != null && Stats.CurrentHP <= 0;

        /// <summary>표시 이름(언어 키 또는 직접 문자열).</summary>
        public string DisplayName;

        /// <summary>머리 위 UI 핸들.</summary>
        public EntityWorldUI WorldUI;

        /// <summary>
        /// 전투 진입 시 슬롯/상태를 정리한다.
        /// HP/MP의 완전 회복 여부는 파생 클래스 결정.
        /// </summary>
        public virtual void PrepareForCombat()
        {
            // 동작 요약:
            // - Stats.ClampToMax().
            // - IsDefending = false.
            // - 액티브 스킬 슬롯 길이 2 보정.
            // - WorldUI.HideActionIcon().
            // - 패시브 효과는 전투 시작 전 SyncPassivesFromRunes()가 이미 갱신했으므로
            //   여기서 ApplyAllPassiveEffects()를 호출해 스탯에 반영한다.
            //TODO: Stats.ClampToMax();
            //TODO: IsDefending = false;
            //TODO: if (ActiveSkills == null || ActiveSkills.Length != 2) ActiveSkills = new Skill[2];
            //TODO: WorldUI?.HideActionIcon();
            //TODO: ApplyAllPassiveEffects();
            if (Stats == null) //Wave0write
            { //Wave0write
                Stats = new StatBlock(); //Wave0write
                Stats.SetBaseStats(1, 0, 0, 0, 0); //Wave0write
            } //Wave0write

            Stats.ClampToMax(); //Wave0write
            IsDefending = false; //Wave0write
            if (ActiveSkills == null || ActiveSkills.Length != 2) //Wave0write
            { //Wave0write
                ActiveSkills = new Skill[2]; //Wave0write
            } //Wave0write

            WorldUI?.HideActionIcon(); //Wave0write
            ApplyAllPassiveEffects(); //Wave0write
        }

        /// <summary>
        /// 라운드 종료 시 호출. 방어 해제 등 정리.
        /// </summary>
        public virtual void OnRoundEnd()
        {
            // 동작 요약: IsDefending = false; WorldUI.HideActionIcon().
            //TODO: IsDefending = false;
            //TODO: WorldUI?.HideActionIcon();
            IsDefending = false; //Wave0write
            WorldUI?.HideActionIcon(); //Wave0write
        }

        /// <summary>
        /// 데미지 적용. DamageCalculatort에서 호출.
        /// </summary>
        public int ApplyDamage(int rawDamage)
        {
            // 동작 요약:
            // - Stats.TakeDamage(rawDamage, IsDefending) 호출(IsDefending이 true면 추가 경감).
            // - WorldUI.PlayHitFx().
            // - 실제 차감 피해 반환.
            //TODO: int actualDamage = Stats.TakeDamage(rawDamage, IsDefending);
            //TODO: WorldUI?.PlayHitFx();
            //TODO: return actualDamage;
            if (Stats == null) //Wave0write
            { //Wave0write
                return 0; //Wave0write
            } //Wave0write

            int actualDamage = Stats.TakeDamage(rawDamage, IsDefending); //Wave0write
            WorldUI?.PlayHitFx(); //Wave0write
            return actualDamage; //Wave0write
        }

        /// <summary>
        /// 회복 적용.
        /// </summary>
        public int ApplyHeal(int rawHeal)
        {
            // 동작 요약:
            // - Stats.CurrentHP에 더하고 MaxHP로 클램프.
            // - WorldUI.PlayHealFx().
            // - 실제 회복량 반환.
            //TODO: int before = Stats.CurrentHP;
            //TODO: Stats.CurrentHP = Mathf.Min(Stats.CurrentHP + rawHeal, Stats.MaxHP);
            //TODO: int actualHeal = Stats.CurrentHP - before;
            //TODO: WorldUI?.PlayHealFx();
            //TODO: return actualHeal;
            if (Stats == null || rawHeal <= 0) //Wave0write
            { //Wave0write
                return 0; //Wave0write
            } //Wave0write

            int before = Stats.CurrentHP; //Wave0write
            Stats.CurrentHP = Mathf.Min(Stats.CurrentHP + rawHeal, Stats.MaxHP); //Wave0write
            int actualHeal = Stats.CurrentHP - before; //Wave0write
            WorldUI?.PlayHealFx(); //Wave0write
            return actualHeal; //Wave0write
        }

        /// <summary>
        /// 보호막 적용(이번 라운드 한정 또는 일정 시간).
        /// </summary>
        public void ApplyShield(int amount, int durationRounds)
        {
            // 동작 요약: 보호막 상태 컴포넌트(미정의 시 단순 누적값) 추가.
            //TODO: // ShieldStatet가 정의된 경우: var shield = GetComponent<ShieldStatet>() ?? gameObject.AddComponent<ShieldStatet>();
            //TODO: // shield.Add(amount, durationRounds);
            //TODO: // 미정의 시 임시 누적: Stats.CurrentHP += amount; Stats.ClampToMax(); // 보호막 = 임시 HP 보정
            //TODO: WorldUI?.ShowShieldFx(amount);
            if (Stats != null && amount > 0) //Wave0write
            { //Wave0write
                Stats.CurrentHP = Mathf.Min(Stats.MaxHP, Stats.CurrentHP + amount); //Wave0write
            } //Wave0write

        }

        /// <summary>
        /// 이번 턴 방어 상태 설정.
        /// </summary>
        public void SetDefending(bool value)
        {
            // 동작 요약: IsDefending = value; 머리 위 파랑 아이콘 표시/해제.
            //TODO: IsDefending = value;
            //TODO: if (value) WorldUI?.ShowActionIcon(ActionIconTypet.Defend);
            //TODO: else WorldUI?.HideActionIcon();
            IsDefending = value; //Wave0write
            if (value) //Wave0write
            { //Wave0write
                WorldUI?.ShowDefendIcon(); //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                WorldUI?.HideActionIcon(); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 액티브 스킬 슬롯 조회.
        /// </summary>
        public Skill GetActiveSkill(int slotIndex)
        {
            // 동작 요약: 범위 검사 후 ActiveSkills[slotIndex] 반환.
            //TODO: if (slotIndex < 0 || slotIndex >= ActiveSkills.Length) return null;
            //TODO: return ActiveSkills[slotIndex];
            return ActiveSkills != null && slotIndex >= 0 && slotIndex < ActiveSkills.Length ? ActiveSkills[slotIndex] : null; //Wave0write
        }

        /// <summary>
        /// 액티브 스킬 슬롯 설정. 길드/룬 변경 시 호출.
        /// </summary>
        public void SetActiveSkill(int slotIndex, Skill skill)
        {
            // 동작 요약: 범위 검사 후 ActiveSkills[slotIndex] = skill.
            //TODO: if (slotIndex < 0 || slotIndex >= ActiveSkills.Length) return;
            //TODO: ActiveSkills[slotIndex] = skill;
            if (ActiveSkills == null || ActiveSkills.Length != 2) //Wave0write
            { //Wave0write
                ActiveSkills = new Skill[2]; //Wave0write
            } //Wave0write

            if (slotIndex >= 0 && slotIndex < ActiveSkills.Length) //Wave0write
            { //Wave0write
                ActiveSkills[slotIndex] = skill; //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 패시브 스킬 추가. CharacterBase.SyncPassivesFromRunes()만 호출해야 한다.
        /// 외부에서 직접 호출 금지.
        /// </summary>
        internal void AddPassiveSkill(Skill skill)
        {
            // 동작 요약:
            // - skill.Data.SkillType == Passive 검증.
            // - passiveSkills에 중복 없이 추가.
            //TODO: if (skill == null || skill.Data == null) return;
            //TODO: if (skill.Data.SkillType != SkillType.Passive) return;
            //TODO: if (!passiveSkills.Contains(skill)) passiveSkills.Add(skill);
            if (skill?.Data == null || skill.Data.SkillType != SkillType.Passive) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            if (!passiveSkills.Exists(s => s.Data != null && s.Data.Id == skill.Data.Id)) //Wave0write
            { //Wave0write
                passiveSkills.Add(skill); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 패시브 스킬 목록 초기화. SyncPassivesFromRunes() 호출 전 선행 처리.
        /// </summary>
        internal void ClearPassiveSkills()
        {
            // 동작 요약: passiveSkills.Clear().
            //TODO: passiveSkills.Clear();
            passiveSkills.Clear(); //Wave0write
        }

        /// <summary>
        /// 현재 passiveSkills 목록에 등록된 모든 패시브 효과를 Stats.PassiveBonus에 적용.
        /// PrepareForCombat 시 1회 호출. 중복 적용 방지를 위해 ResetPassiveBonuses() 후 재적용.
        /// </summary>
        protected void ApplyAllPassiveEffects()
        {
            // 동작 요약:
            // - Stats.ResetPassiveBonuses().
            // - passiveSkills 순회: 각 Skillt의 Data.EffectFormula / EffectValue 기준으로 스탯 보정 적용.
            // - StatBlockt는 기본 스탯 + 장비 보정 + 룬 보정 + 패시브 보정을 합산해 최종 스탯을 갱신한다.
            //TODO: Stats.ResetPassiveBonuses();
            //TODO: foreach (Skill passive in passiveSkills)
            //TODO: {
            //TODO:     if (passive?.Data == null) continue;
            //TODO:     SkillData d = passive.Data;
            //TODO:     // DamageScale → ATK 보정, HealScale → HP 보정, ShieldScale → DEF 보정 등
            //TODO:     // 실제 매핑은 SkillData.EffectFormula 필드 확정 후 분기 처리
            //TODO:     // 예: if (d.DamageScale > 0f) Stats.ApplyPassiveBonus(StatType.ATK, (int)(Stats.ATK * d.DamageScale));
            //TODO: }
            if (Stats == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            Stats.ResetPassiveBonuses(); //Wave0write
            foreach (Skill passive in passiveSkills) //Wave0write
            { //Wave0write
                SkillData data = passive?.Data; //Wave0write
                if (data == null) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                if (data.DamageScale > 0f) //Wave0write
                { //Wave0write
                    Stats.ApplyPassiveBonus(StatType.ATK, Mathf.RoundToInt(Stats.BaseATK * data.DamageScale)); //Wave0write
                } //Wave0write
                if (data.HealScale > 0f) //Wave0write
                { //Wave0write
                    Stats.ApplyPassiveBonus(StatType.HP, Mathf.RoundToInt(Stats.BaseMaxHP * data.HealScale)); //Wave0write
                } //Wave0write
                if (data.ShieldScale > 0f) //Wave0write
                { //Wave0write
                    Stats.ApplyPassiveBonus(StatType.DEF, Mathf.RoundToInt(Stats.BaseDEF * data.ShieldScale)); //Wave0write
                } //Wave0write
            } //Wave0write
        }
    }
}


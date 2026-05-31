namespace Tempt
{
    /// <summary>
    /// 캐릭터(플레이어 + 동료)의 공통 베이스. 룬/EXP/레벨을 가짐.
    /// Monster는 이 클래스를 사용하지 않는다.
    /// </summary>
    public abstract class CharacterBase : EntityBase
    {
        /// <summary>현재 레벨(1부터 시작).</summary>
        public int Level { get; protected set; } = 1;

        /// <summary>현재 EXP.</summary>
        public int CurrentExp { get; protected set; }

        /// <summary>현 레벨의 필요 EXP(BalanceData 참조).</summary>
        public int RequiredExp { get; protected set; }

        /// <summary>EXP를 지급한다. 도달 시 레벨업.</summary>
        public void GainExp(int amount)
        {
            // 동작 요약:
            // - CurrentExp += amount.
            // - while (CurrentExp >= RequiredExp) → LevelUp().
            // - EventBus.RaisePlayerExpChanged(CurrentExp, RequiredExp).
            // - 동료는 별도 이벤트(이 클래스는 일단 같은 흐름, 발행은 파생에서).
            if (amount <= 0)
            {
                return;
            }

            EnsureRequiredExp();
            CurrentExp += amount;
            while (RequiredExp > 0 && CurrentExp >= RequiredExp)
            {
                LevelUp();
            }

            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaisePlayerExpChanged(CurrentExp, RequiredExp);
            }
        }

        /// <summary>
        /// 레벨업 처리. 룬 포인트 적립, 스탯 갱신, 패시브 스킬 동기화.
        /// </summary>
        protected virtual void LevelUp()
        {
            // 동작 요약:
            // - CurrentExp -= RequiredExp.
            // - Level += 1.
            // - RequiredExp = BalanceData.ExpToNextLevel[Level].
            // - 스탯 성장(파생 클래스가 OnLeveledUp로 처리).
            // - 룬 포인트 적립(Player는 PlayerRuneState.AddPoint; Companion은 CompanionRuneState 자동 해금).
            // - SyncPassivesFromRunes() 호출 → 패시브 목록 갱신.
            // - 이벤트 발행.
            EnsureRequiredExp();
            CurrentExp -= RequiredExp;
            Level += 1;
            RequiredExp = ResolveRequiredExp(Level);
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                SyncPassivesFromRunes(gsm.Data);
                gsm.Events?.RaisePlayerLevelUp(Level);
            }
            OnLeveledUp();
        }

        /// <summary>레벨업 후 파생 클래스가 처리할 후훅.</summary>
        protected abstract void OnLeveledUp();

        /// <summary>
        /// 노드 클리어 시 합산 EXP를 받는다. CombatControllert가 호출.
        /// </summary>
        public void GainNodeReward(int totalExp)
        {
            // 동작 요약: GainExp(totalExp).
            GainExp(totalExp);
        }

        /// <summary>
        /// 현재 해금된 룬 노드를 기준으로 PassiveSkills 목록을 재구성한다.
        /// 레벨업, 룬 해금/초기화, 전투 진입 전에 호출.
        /// 플레이어(PlayerRuneState 사용)와 동료(CompanionRuneState 사용) 모두 이 흐름을 따른다.
        /// </summary>
        /// <param name="data">DataManager 참조. 스킬 데이터 조회용.</param>
        public void SyncPassivesFromRunes(DataManager data)
        {
            // 동작 요약:
            // - ClearPassiveSkills().
            // - 해금된 룬 노드 목록 조회(파생 클래스가 GetUnlockedRuneNodeIds()로 제공).
            // - 각 RuneData 조회: EffectType == UnlockSkill → EffectValue를 skillId로 변환.
            // - data.Skills[skillId] 조회: SkillType == Passive 인 경우만 AddPassiveSkill().
            // - Active 스킬은 이 메서드에서 건드리지 않는다(슬롯 관리는 SetActiveSkill로 별도).
            ClearPassiveSkills();
            System.Collections.Generic.IReadOnlyList<int> unlockedIds = GetUnlockedRuneNodeIds();
            if (data == null || unlockedIds == null)
            {
                return;
            }

            foreach (int nodeId in unlockedIds)
            {
                if (!data.Runes.TryGetValue(nodeId, out RuneData runeData) || runeData.EffectType != RuneEffectType.UnlockSkill)
                {
                    continue;
                }

                int skillId = (int)runeData.EffectValue;
                if (data.Skills.TryGetValue(skillId, out SkillData skillData) && skillData.SkillType == SkillType.Passive)
                {
                    AddPassiveSkill(new Skill(skillData));
                }
            }
        }

        /// <summary>
        /// 파생 클래스가 현재 해금된 룬 노드 ID 목록을 반환한다.
        /// Player는 PlayerRuneState.UnlockedNodeIds, Companion은 CompanionRuneState 기반.
        /// </summary>
        protected abstract System.Collections.Generic.IReadOnlyList<int> GetUnlockedRuneNodeIds();

        private void EnsureRequiredExp()
        {
            if (RequiredExp <= 0)
            {
                RequiredExp = ResolveRequiredExp(Level);
            }
        }

        private static int ResolveRequiredExp(int level)
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                return RunProgression.RequiredExpForLevel(gsm.Data, level);
            }

            return 999999;
        }
    }
}


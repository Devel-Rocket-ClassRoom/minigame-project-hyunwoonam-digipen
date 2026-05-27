namespace Tempt
{
    /// <summary>
    /// 동료 베이스. 게임 시작 시 룬 트리가 무작위로 고정되고, 레벨업 시 정해진 트리 순서로 자동 해금.
    /// 행동 결정은 직업별 고정 우선순위 규칙(CompanionActionSelector)을 사용.
    /// </summary>
    public abstract class TeamBase : CharacterBase
    {
        /// <summary>동료 데이터 ID(CompanionData 참조).</summary>
        public int CompanionDataId;

        /// <summary>고정 룬 트리 상태.</summary>
        public CompanionRuneState Rune;

        /// <summary>장비 슬롯(동료도 장비 가능).</summary>
        public EquipmentSlots Equipment;

        /// <summary>행동 규칙 키(직업별 우선순위 식별자).</summary>
        public string ActionRuleKey;

        /// <summary>
        /// 모집 직후 1회 호출. 트리 무작위 생성 + 시작 룬 해금.
        /// </summary>
        public void Initialize(int companionDataId, int seed)
        {
            // 동작 요약:
            // - CompanionDataId = companionDataId.
            // - Rune = RuneTreeGenerator.GenerateFixedTree(data, seed).
            // - Rune.UnlockStarter().
            // - 직업별 ActionRuleKey 설정.
            // - 기본 스탯 설정.
            //TODO: CompanionDataId = companionDataId;
            //TODO: CompanionData data = GameSystemManager.Instance.Data.Companions[companionDataId];
            //TODO: ActionRuleKey = data.ActionRuleKey;
            //TODO: Rune = RuneTreeGenerator.GenerateFixedTree(data, seed);
            //TODO: Rune.UnlockStarter();
            //TODO: // 기본 스탯(직업 베이스)
            //TODO: Stats.BaseMaxHP = data.BaseMaxHP;
            //TODO: Stats.BaseMaxMP = data.BaseMaxMP;
            //TODO: Stats.BaseATK   = data.BaseATK;
            //TODO: Stats.BaseDEF   = data.BaseDEF;
            //TODO: Stats.BaseSPD   = data.BaseSPD;
            //TODO: Stats.RestoreToFull();
            //TODO: SyncPassivesFromRunes();
            CompanionDataId = companionDataId; //Wave0write
            Equipment = new EquipmentSlots(); //Wave0write
            Stats = new StatBlock(); //Wave0write
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm) && gsm.Data.Companions.TryGetValue(companionDataId, out CompanionData data)) //Wave0write
            { //Wave0write
                ActionRuleKey = data.ActionRuleKey; //Wave0write
                EquipmentStatMod baseStats = data.BaseStats ?? new EquipmentStatMod { HP = 70, MP = 10, ATK = 8, DEF = 2, SPD = 9 }; //Wave0write
                Stats.SetBaseStats(baseStats.HP, baseStats.MP, baseStats.ATK, baseStats.DEF, baseStats.SPD); //Wave0write
                Rune = new CompanionRuneState { ClassId = data.ClassId, Tree = RuneTree.BuildFromData(data.ClassId, gsm.Data.Runes.Values), FixedSequence = new System.Collections.Generic.List<int>(), UnlockedCount = 0 }; //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                ActionRuleKey = "Dealer"; //Wave0write
                Stats.SetBaseStats(70, 10, 8, 2, 9); //Wave0write
                Rune = new CompanionRuneState { ClassId = RuneClass.Dealer, FixedSequence = new System.Collections.Generic.List<int>() }; //Wave0write
            } //Wave0write

            Stats.RestoreToFull(); //Wave0write
            Rune.UnlockStarter(); //Wave0write
        }

        // Wave0refactor 2026-05-27: CharacterBase.LevelUp 이 이미 SyncPassivesFromRunes 를
        // 호출하므로 여기서는 다시 호출하지 않는다. 동료 룬 자동 해금만 처리한다.
        /// <inheritdoc/>
        protected override void OnLeveledUp()
        {
            // 동작 요약:
            // - 직업별 성장 적용.
            // - Rune.UnlockNextNodeIfPossible() — 미해금 노드 1개 자동 진행.
            // - SyncPassivesFromRunes 는 CharacterBase.LevelUp 에서 이미 호출됨.
            //   (동료 룬이 자동 해금되어 패시브가 변하더라도 다음 라운드의 PrepareForCombat 에서
            //    ApplyAllPassiveEffects 가 다시 합산하므로 즉시 재호출은 불필요)
            if (Stats != null)
            {
                Stats.BaseMaxHP += 6;
                Stats.BaseATK += 1;
                Stats.BaseDEF += 1;
                Stats.BaseSPD += 1;
                Stats.RecalculateFinalStats();
            }

            Rune?.UnlockNextNodeIfPossible();
        }

        /// <inheritdoc/>
        protected override System.Collections.Generic.IReadOnlyList<int> GetUnlockedRuneNodeIds()
        {
            // 동작 요약:
            // - Rune?.FixedSequence의 앞 UnlockedCount개(해금된 노드)만 잘라 반환.
            // - Rune이 null이면 빈 목록 반환.
            //TODO: if (Rune == null) return new System.Collections.Generic.List<int>();
            //TODO: return Rune.FixedSequence.GetRange(0, Rune.UnlockedCount);
            if (Rune?.FixedSequence == null) //Wave0write
            { //Wave0write
                return new System.Collections.Generic.List<int>(); //Wave0write
            } //Wave0write

            int count = System.Math.Min(Rune.UnlockedCount, Rune.FixedSequence.Count); //Wave0write
            return Rune.FixedSequence.GetRange(0, count); //Wave0write
        }
    }
}


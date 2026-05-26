namespace Tempt
{
    /// <summary>
    /// 플레이어 캐릭터. 사용자가 직접 룬 트리를 해금하고 장비/소모를 관리한다.
    /// </summary>
    public sealed class Playert : CharacterBaset
    {
        /// <summary>선택한 시작 직업.</summary>
        public RuneClasst StartingClass;

        /// <summary>플레이어 룬 상태.</summary>
        public PlayerRuneStatet Rune;

        /// <summary>인벤토리.</summary>
        public InventoryStatet Inventory;

        /// <summary>장비 슬롯.</summary>
        public EquipmentSlotst Equipment;

        /// <summary>전투 소모 4칸.</summary>
        public ConsumableSlotst Consumables;

        /// <summary>보관함(주점 구매 후 활성).</summary>
        public LockerStatet Locker;

        /// <summary>
        /// Safe0에서 시작 룬을 선택한 시점에 1회 호출.
        /// </summary>
        public void ApplyStartingClass(RuneClasst pickedClass)
        {
            // 동작 요약:
            // - StartingClass = pickedClass.
            // - Rune = new PlayerRuneStatet(pickedClass).
            // - Rune.UnlockStarter().
            //TODO: StartingClass = pickedClass;
            //TODO: Rune = new PlayerRuneStatet(pickedClass);
            //TODO: Rune.UnlockStarter();
            //TODO: SyncPassivesFromRunes(); // 패시브 스킬 동기화
            StartingClass = pickedClass; //Wave0write
            Rune = new PlayerRuneStatet //Wave0write
            { //Wave0write
                ClassId = pickedClass, //Wave0write
                RunePoints = 0, //Wave0write
                UnlockedIds = new System.Collections.Generic.HashSet<int>(), //Wave0write
                Tree = GameSystemManagert.TryGetInstance(out GameSystemManagert gsm) ? RuneTreet.BuildFromData(pickedClass, gsm.Data.Runes.Values) : null, //Wave0write
            }; //Wave0write
            Rune.UnlockStarter(); //Wave0write
            if (gsm != null) //Wave0write
            { //Wave0write
                SyncPassivesFromRunes(gsm.Data); //Wave0write
            } //Wave0write
            RecalcBonusStats(); //Wave0write
        }

        /// <summary>
        /// 장비 변경 시 호출. 스탯 보정 재계산.
        /// </summary>
        public void RecalcBonusStats()
        {
            // 동작 요약:
            // - Equipment.AggregateStatMod() + Rune.AggregateStatMod() → Stats.Bonus.
            // - Stats.ClampToMax().
            //TODO: StatModt equipMod = Equipment != null ? Equipment.AggregateStatMod() : new StatModt();
            //TODO: StatModt runeMod  = Rune    != null ? Rune.AggregateStatMod()      : new StatModt();
            //TODO: Stats.BonusMaxHP  = equipMod.MaxHP  + runeMod.MaxHP;
            //TODO: Stats.BonusMaxMP  = equipMod.MaxMP  + runeMod.MaxMP;
            //TODO: Stats.BonusATK    = equipMod.ATK    + runeMod.ATK;
            //TODO: Stats.BonusDEF    = equipMod.DEF    + runeMod.DEF;
            //TODO: Stats.BonusSPD    = equipMod.SPD    + runeMod.SPD;
            //TODO: Stats.ClampToMax();
            if (Stats == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            Stats.ResetEquipmentBonuses(); //Wave0write
            Stats.ApplyEquipmentBonus(Equipment != null ? Equipment.AggregateStatMod() : new EquipmentStatModt()); //Wave0write
            Stats.ResetRuneBonuses(); //Wave0write
            EquipmentStatModt runeMod = Rune != null ? Rune.AggregateStatMod() : new EquipmentStatModt(); //Wave0write
            Stats.ApplyRuneBonus(StatTypet.HP, runeMod.HP); //Wave0write
            Stats.ApplyRuneBonus(StatTypet.MP, runeMod.MP); //Wave0write
            Stats.ApplyRuneBonus(StatTypet.ATK, runeMod.ATK); //Wave0write
            Stats.ApplyRuneBonus(StatTypet.DEF, runeMod.DEF); //Wave0write
            Stats.ApplyRuneBonus(StatTypet.SPD, runeMod.SPD); //Wave0write
        }

        /// <inheritdoc/>
        protected override void OnLeveledUp()
        {
            // 동작 요약:
            // - 직업별 기본 성장량 적용.
            // - Rune.AddRunePoint(BalanceDatat.RunePointPerLevel).
            // - EventBust.RaisePlayerLevelUp(Level).
            //TODO: Stats.BaseMaxHP += BalanceDatat.GetHPGrowth(StartingClass);
            //TODO: Stats.BaseATK   += BalanceDatat.GetATKGrowth(StartingClass);
            //TODO: Stats.BaseDEF   += BalanceDatat.GetDEFGrowth(StartingClass);
            //TODO: Stats.BaseSPD   += BalanceDatat.GetSPDGrowth(StartingClass);
            //TODO: Rune.AddRunePoint(BalanceDatat.RunePointPerLevel);
            //TODO: RecalcBonusStats();
            //TODO: SyncPassivesFromRunes();
            //TODO: GameSystemManagert.Instance.Events.RaisePlayerLevelUp(Level);
            if (Stats != null) //Wave0write
            { //Wave0write
                Stats.BaseMaxHP += 8; //Wave0write
                Stats.BaseMaxMP += 2; //Wave0write
                Stats.BaseATK += 2; //Wave0write
                Stats.BaseDEF += 1; //Wave0write
                Stats.BaseSPD += 1; //Wave0write
                Stats.RecalculateFinalStats(); //Wave0write
                Stats.RestoreToFull(); //Wave0write
            } //Wave0write

            if (GameSystemManagert.TryGetInstance(out GameSystemManagert gsm)) //Wave0write
            { //Wave0write
                Rune?.AddRunePoint(gsm.Data?.Balance?.RunePointPerLevel ?? 1); //Wave0write
                SyncPassivesFromRunes(gsm.Data); //Wave0write
                gsm.Events?.RaisePlayerLevelUp(Level); //Wave0write
            } //Wave0write
        }

        /// <inheritdoc/>
        protected override System.Collections.Generic.IReadOnlyList<int> GetUnlockedRuneNodeIds()
        {
            // 동작 요약:
            // - Rune?.UnlockedIds(HashSet<int>)를 List<int>로 변환하여 반환.
            // - Rune이 null이면 빈 목록 반환.
            //TODO: if (Rune == null) return new System.Collections.Generic.List<int>();
            //TODO: return new System.Collections.Generic.List<int>(Rune.UnlockedIds);
            return Rune?.UnlockedIds != null ? new System.Collections.Generic.List<int>(Rune.UnlockedIds) : new System.Collections.Generic.List<int>(); //Wave0write
        }

        /// <summary>
        /// 후퇴 아이템 사용 시 분기. 전투 중에는 CombatControllert가 위임.
        /// </summary>
        public void UseRetreatItem()
        {
            // 동작 요약:
            // - Inventory에서 후퇴 아이템 1개 차감.
            // - GameSystemManagert.Instance.EndCombat(Retreat) 호출.
            //TODO: // 후퇴 아이템 ID는 BalanceDatat.RetreatItemId 상수로 관리
            //TODO: if (!Inventory.Remove(BalanceDatat.RetreatItemId, 1)) return;
            //TODO: GameSystemManagert.Instance.EndCombat(CombatResultt.Retreat, null);
            if (GameSystemManagert.TryGetInstance(out GameSystemManagert gsm)) //Wave0write
            { //Wave0write
                gsm.EndCombat(CombatResultt.Retreat, null); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 저장/런 상태 데이터를 전투용 MonoBehaviour에 바인딩한다.
        /// </summary>
        public void BindState(PlayerStatet state) //Wave0write
        { //Wave0write
            if (state == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            DisplayName = state.Name; //Wave0write
            Level = state.Level <= 0 ? 1 : state.Level; //Wave0write
            CurrentExp = state.Exp; //Wave0write
            Stats = state.Stats ?? new StatBlockt(); //Wave0write
            StartingClass = state.StartingClass; //Wave0write
            Rune = state.Rune; //Wave0write
            Inventory = state.Inventory ?? new InventoryStatet(); //Wave0write
            Equipment = state.Equipment ?? new EquipmentSlotst(); //Wave0write
            Consumables = state.Consumables ?? new ConsumableSlotst(); //Wave0write
            Locker = state.Locker ?? new LockerStatet(); //Wave0write

            if (ActiveSkills == null || ActiveSkills.Length != 2) //Wave0write
            { //Wave0write
                ActiveSkills = new Skillt[2]; //Wave0write
            } //Wave0write

            if (GameSystemManagert.TryGetInstance(out GameSystemManagert gsm)) //Wave0write
            { //Wave0write
                if (gsm.Data.Skills.TryGetValue(1, out SkillDatat attackSkill)) //Wave0write
                { //Wave0write
                    ActiveSkills[0] = new Skillt(attackSkill); //Wave0write
                } //Wave0write
                if (gsm.Data.Skills.TryGetValue(2, out SkillDatat areaSkill)) //Wave0write
                { //Wave0write
                    ActiveSkills[1] = new Skillt(areaSkill); //Wave0write
                } //Wave0write
                SyncPassivesFromRunes(gsm.Data); //Wave0write
            } //Wave0write

            RecalcBonusStats(); //Wave0write
            Stats.RestoreToFull(); //Wave0write
        } //Wave0write

        /// <summary>
        /// 전투 후 MonoBehaviour의 핵심 상태를 저장/런 상태에 반영한다.
        /// </summary>
        public void CopyToState(PlayerStatet state) //Wave0write
        { //Wave0write
            if (state == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            state.Level = Level; //Wave0write
            state.Exp = CurrentExp; //Wave0write
            state.Stats = Stats; //Wave0write
            state.StartingClass = StartingClass; //Wave0write
            state.Rune = Rune; //Wave0write
            state.Inventory = Inventory; //Wave0write
            state.Equipment = Equipment; //Wave0write
            state.Consumables = Consumables; //Wave0write
            state.Locker = Locker; //Wave0write
        } //Wave0write
    }
}

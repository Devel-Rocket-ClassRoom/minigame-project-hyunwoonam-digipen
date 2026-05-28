namespace Tempt
{
    /// <summary>
    /// 플레이어 캐릭터. 사용자가 직접 룬 트리를 해금하고 장비/소모를 관리한다.
    /// </summary>
    public sealed class Player : CharacterBase
    {
        /// <summary>선택한 시작 직업.</summary>
        public RuneClass StartingClass;

        /// <summary>플레이어 룬 상태.</summary>
        public PlayerRuneState Rune;

        /// <summary>인벤토리.</summary>
        public InventoryState Inventory;

        /// <summary>장비 슬롯.</summary>
        public EquipmentSlots Equipment;

        /// <summary>전투 소모 4칸.</summary>
        public ConsumableSlots Consumables;

        /// <summary>보관함(주점 구매 후 활성).</summary>
        public LockerState Locker;

        /// <summary>
        /// Safe0에서 시작 룬을 선택한 시점에 1회 호출.
        /// </summary>
        public void ApplyStartingClass(RuneClass pickedClass)
        {
            // 동작 요약:
            // - StartingClass = pickedClass.
            // - Rune = new PlayerRuneState(pickedClass).
            // - Rune.UnlockStarter().
            //TODO: StartingClass = pickedClass;
            //TODO: Rune = new PlayerRuneState(pickedClass);
            //TODO: Rune.UnlockStarter();
            //TODO: SyncPassivesFromRunes(); // 패시브 스킬 동기화
            StartingClass = pickedClass; //Wave0write
            Rune = new PlayerRuneState //Wave0write
            { //Wave0write
                ClassId = pickedClass, //Wave0write
                RunePoints = 0, //Wave0write
                UnlockedIds = new System.Collections.Generic.HashSet<int>(), //Wave0write
                Tree = GameSystemManager.TryGetInstance(out GameSystemManager gsm) ? RuneTree.BuildFromData(pickedClass, gsm.Data.Runes.Values) : null, //Wave0write
            }; //Wave0write
            Rune.UnlockStarter(); //Wave0write
            if (gsm != null) //Wave0write
            { //Wave0write
                SyncPassivesFromRunes(gsm.Data); //Wave0write
                if (gsm.CurrentRun?.Player != null) //Wave0write
                { //Wave0write
                    PlayerState state = gsm.CurrentRun.Player; //Wave0write
                    state.StartingClass = pickedClass; //Wave0write
                    state.Rune = Rune; //Wave0write
                    if (state.OwnedSkillIds == null) //Wave0write
                    { //Wave0write
                        state.OwnedSkillIds = new System.Collections.Generic.HashSet<int>(); //Wave0write
                    } //Wave0write
                    if (state.ActiveSlotSkillIds == null || state.ActiveSlotSkillIds.Length != 2) //Wave0write
                    { //Wave0write
                        state.ActiveSlotSkillIds = new int[2]; //Wave0write
                    } //Wave0write

                    state.ActiveSlotSkillIds[0] = 0; //Wave0write
                    state.ActiveSlotSkillIds[1] = 0; //Wave0write
                    int[] starters = gsm.Data.GetStartingSkillIds(pickedClass); //Wave0write
                    for (int i = 0; i < starters.Length && i < state.ActiveSlotSkillIds.Length; i++) //Wave0write
                    { //Wave0write
                        int skillId = starters[i]; //Wave0write
                        state.ActiveSlotSkillIds[i] = skillId; //Wave0write
                        if (skillId != 0) //Wave0write
                        { //Wave0write
                            state.OwnedSkillIds.Add(skillId); //Wave0write
                        } //Wave0write
                    } //Wave0write

                    gsm.Events?.RaiseSkillsChanged(); //Wave0write
                } //Wave0write
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
            Stats.ApplyEquipmentBonus(Equipment != null ? Equipment.AggregateStatMod() : new EquipmentStatMod()); //Wave0write
            Stats.ResetRuneBonuses(); //Wave0write
            EquipmentStatMod runeMod = Rune != null ? Rune.AggregateStatMod() : new EquipmentStatMod(); //Wave0write
            Stats.ApplyRuneBonus(StatType.HP, runeMod.HP); //Wave0write
            Stats.ApplyRuneBonus(StatType.MP, runeMod.MP); //Wave0write
            Stats.ApplyRuneBonus(StatType.ATK, runeMod.ATK); //Wave0write
            Stats.ApplyRuneBonus(StatType.DEF, runeMod.DEF); //Wave0write
            Stats.ApplyRuneBonus(StatType.SPD, runeMod.SPD); //Wave0write
        }

        // Wave0refactor 2026-05-27: CharacterBase.LevelUp 이 이미 SyncPassivesFromRunes 와
        // Events.RaisePlayerLevelUp 을 호출한다. 여기서는 직업 성장/룬 포인트만 처리한다.
        /// <inheritdoc/>
        protected override void OnLeveledUp()
        {
            // 동작 요약:
            // - 직업별 기본 성장량 적용.
            // - Rune.AddRunePoint(BalanceData.RunePointPerLevel).
            // - SyncPassivesFromRunes / RaisePlayerLevelUp 은 CharacterBase.LevelUp 에서 이미 호출되므로
            //   이 함수에서는 절대 다시 호출하지 않는다(이벤트 이중 발사 방지).
            if (Stats != null)
            {
                Stats.BaseMaxHP += 8;
                Stats.BaseMaxMP += 2;
                Stats.BaseATK += 2;
                Stats.BaseDEF += 1;
                Stats.BaseSPD += 1;
                Stats.RecalculateFinalStats();
            }

            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                Rune?.AddRunePoint(gsm.Data?.Balance?.RunePointPerLevel ?? 1);
            }
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
            // - GameSystemManager.Instance.EndCombat(Retreat) 호출.
            //TODO: // 후퇴 아이템 ID는 BalanceData.RetreatItemId 상수로 관리
            //TODO: if (!Inventory.Remove(BalanceData.RetreatItemId, 1)) return;
            //TODO: GameSystemManager.Instance.EndCombat(CombatResult.Retreat, null);
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm)) //Wave0write
            { //Wave0write
                gsm.EndCombat(CombatResult.Retreat, null); //Wave0write
            } //Wave0write
        }

        /// <summary>
        /// 저장/런 상태 데이터를 전투용 MonoBehaviour에 바인딩한다.
        /// </summary>
        public void BindState(PlayerState state) //Wave0write
        { //Wave0write
            if (state == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            DisplayName = state.Name; //Wave0write
            Level = state.Level <= 0 ? 1 : state.Level; //Wave0write
            CurrentExp = state.Exp; //Wave0write
            Stats = state.Stats ?? new StatBlock(); //Wave0write
            StartingClass = state.StartingClass; //Wave0write
            Rune = state.Rune; //Wave0write
            Inventory = state.Inventory ?? new InventoryState(); //Wave0write
            Equipment = state.Equipment ?? new EquipmentSlots(); //Wave0write
            Consumables = state.Consumables ?? new ConsumableSlots(); //Wave0write
            Locker = state.Locker ?? new LockerState(); //Wave0write

            if (ActiveSkills == null || ActiveSkills.Length != 2) //Wave0write
            { //Wave0write
                ActiveSkills = new Skill[2]; //Wave0write
            } //Wave0write

            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm)) //Wave0write
            { //Wave0write
                BindActiveSlotsFromState(state, gsm.Data); //Wave0write
                SyncPassivesFromRunes(gsm.Data); //Wave0write
            } //Wave0write
            else //Wave0write
            { //Wave0write
                UnityEngine.Debug.LogError("[Player.BindState] GameSystemManager 참조가 없어 ActiveSkills 를 바인딩할 수 없습니다."); //Wave0write
            } //Wave0write

            RecalcBonusStats(); //Wave0write
        } //Wave0write

        private void BindActiveSlotsFromState(PlayerState state, DataManager data) //Wave0write
        { //Wave0write
            if (state?.ActiveSlotSkillIds == null || state.ActiveSlotSkillIds.Length != 2) //Wave0write
            { //Wave0write
                UnityEngine.Debug.LogError("[Player.BindState] PlayerState.ActiveSlotSkillIds 가 없거나 길이가 2가 아닙니다."); //Wave0write
                ActiveSkills[0] = null; //Wave0write
                ActiveSkills[1] = null; //Wave0write
                return; //Wave0write
            } //Wave0write

            if (data?.Skills == null) //Wave0write
            { //Wave0write
                UnityEngine.Debug.LogError("[Player.BindState] DataManager.Skills 참조가 없습니다."); //Wave0write
                return; //Wave0write
            } //Wave0write

            for (int slot = 0; slot < ActiveSkills.Length; slot++) //Wave0write
            { //Wave0write
                int skillId = state.ActiveSlotSkillIds[slot]; //Wave0write
                if (skillId == 0) //Wave0write
                { //Wave0write
                    ActiveSkills[slot] = null; //Wave0write
                    continue; //Wave0write
                } //Wave0write

                if (data.Skills.TryGetValue(skillId, out SkillData skillData)) //Wave0write
                { //Wave0write
                    ActiveSkills[slot] = new Skill(skillData); //Wave0write
                } //Wave0write
                else //Wave0write
                { //Wave0write
                    UnityEngine.Debug.LogError("[Player.BindState] ActiveSlotSkillIds[" + slot + "] 데이터 없음: " + skillId); //Wave0write
                    ActiveSkills[slot] = null; //Wave0write
                } //Wave0write
            } //Wave0write
        } //Wave0write

        // Wave0refactor 2026-05-27: BUG-2 수정.
        // GameSystemManager.EndCombat 이 PlayerState.Level / Exp 를 먼저 갱신하고 직후
        // CombatController.OnExit 가 이 메서드를 부르면, 과거 구현은 Player.Level / CurrentExp
        // (BindState 시점의 stale 값) 로 다시 덮어써서 레벨업이 사라지는 경합이 있었다.
        //
        // Wave0 범위에서 전투 중에 Player MonoBehaviour 가 자체적으로 Level / Exp 를 올리는
        // 코드 경로는 없다(EXP 는 EndCombat 에서만 누적). 그러므로 Level / Exp 는 PlayerState
        // 한쪽이 단일 권위(source of truth)이고, 여기서는 두 필드를 더 이상 덮어쓰지 않는다.
        //
        // 나머지 참조(Stats / Rune / Inventory / Equipment / Consumables / Locker / StartingClass)
        // 는 BindState 시점에 PlayerState 와 같은 인스턴스를 공유하도록 바인딩되어 있어
        // 별도의 writeback 없이도 PlayerState 는 이미 최신이다. 안전을 위해 참조 정합성만
        // 보정한다(예: 외부에서 새 Inventory 인스턴스를 할당한 경우).
        /// <summary>
        /// 전투 후 MonoBehaviour의 핵심 상태를 저장/런 상태에 반영한다.
        /// Level / Exp 는 PlayerState 가 단일 권위이므로 여기서 덮어쓰지 않는다.
        /// </summary>
        public void CopyToState(PlayerState state)
        {
            if (state == null)
            {
                return;
            }

            // Level / Exp 는 PlayerState 가 권위. 절대 덮어쓰지 않는다.
            // Stats 등은 BindState 에서 공유 참조라서 이미 동기화되어 있지만,
            // 호출자가 새 인스턴스를 만든 경우를 위해 참조만 맞춘다(값 복사 아님).
            state.Stats = Stats ?? state.Stats;
            state.StartingClass = StartingClass;
            state.Rune = Rune ?? state.Rune;
            state.Inventory = Inventory ?? state.Inventory;
            state.Equipment = Equipment ?? state.Equipment;
            state.Consumables = Consumables ?? state.Consumables;
            state.Locker = Locker ?? state.Locker;
        }
    }
}


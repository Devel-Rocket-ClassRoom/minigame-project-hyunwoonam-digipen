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
            StartingClass = pickedClass;
            Rune = new PlayerRuneState
            {
                ClassId = pickedClass,
                RunePoints = 0,
                UnlockedIds = new System.Collections.Generic.HashSet<int>(),
                Tree = GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                    ? RuneTree.BuildFromData(pickedClass, gsm.Data.Runes.Values)
                    : null,
            };
            Rune.UnlockStarter();
            if (gsm != null)
            {
                SyncPassivesFromRunes(gsm.Data);
                if (gsm.CurrentRun?.Player != null)
                {
                    PlayerState state = gsm.CurrentRun.Player;
                    state.StartingClass = pickedClass;
                    state.Rune = Rune;
                    if (state.OwnedSkillIds == null)
                    {
                        state.OwnedSkillIds = new System.Collections.Generic.HashSet<int>();
                    }
                    if (state.ActiveSlotSkillIds == null || state.ActiveSlotSkillIds.Length != 2)
                    {
                        state.ActiveSlotSkillIds = new int[2];
                    }

                    state.ActiveSlotSkillIds[0] = 0;
                    state.ActiveSlotSkillIds[1] = 0;
                    int[] starters = gsm.Data.GetStartingSkillIds(pickedClass);
                    for (int i = 0; i < starters.Length && i < state.ActiveSlotSkillIds.Length; i++)
                    {
                        int skillId = starters[i];
                        state.ActiveSlotSkillIds[i] = skillId;
                        if (skillId != 0)
                        {
                            state.OwnedSkillIds.Add(skillId);
                        }
                    }

                    gsm.Events?.RaiseSkillsChanged();
                }
            }
            RecalcBonusStats();
        }

        /// <summary>
        /// 장비 변경 시 호출. 스탯 보정 재계산.
        /// </summary>
        public void RecalcBonusStats()
        {
            // 동작 요약:
            // - Equipment.AggregateStatMod() + Rune.AggregateStatMod() → Stats.Bonus.
            // - Stats.ClampToMax().
            if (Stats == null)
            {
                return;
            }

            Stats.ResetEquipmentBonuses();
            Stats.ApplyEquipmentBonus(
                Equipment != null ? Equipment.AggregateStatMod() : new EquipmentStatMod()
            );
            Stats.ResetRuneBonuses();
            EquipmentStatMod runeMod =
                Rune != null ? Rune.AggregateStatMod() : new EquipmentStatMod();
            Stats.ApplyRuneBonus(StatType.HP, runeMod.HP);
            Stats.ApplyRuneBonus(StatType.MP, runeMod.MP);
            Stats.ApplyRuneBonus(StatType.ATK, runeMod.ATK);
            Stats.ApplyRuneBonus(StatType.DEF, runeMod.DEF);
            Stats.ApplyRuneBonus(StatType.SPD, runeMod.SPD);
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
                if (Rune != null)
                {
                    Rune.AddRunePoint(gsm.Data?.Balance?.RunePointPerLevel ?? 1);
                    gsm.Events?.RaiseRunePointsChanged(Rune.RunePoints);
                }
            }
        }

        /// <inheritdoc/>
        protected override System.Collections.Generic.IReadOnlyList<int> GetUnlockedRuneNodeIds()
        {
            // 동작 요약:
            // - 마스터 완료된 룬 노드 ID를 반환한다. 부분 투자 노드는 패시브 해금 대상이 아니다.
            // - Rune이 null이면 빈 목록 반환.
            return Rune != null
                ? Rune.GetMasteredNodeIds()
                : new System.Collections.Generic.List<int>();
        }

        /// <summary>
        /// 후퇴 아이템 사용 시 분기. 전투 중에는 CombatControllert가 위임.
        /// </summary>
        public void UseRetreatItem()
        {
            // 동작 요약:
            // - Inventory에서 후퇴 아이템 1개 차감.
            // - GameSystemManager.Instance.EndCombat(Retreat) 호출.
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.EndCombat(CombatResult.Retreat, null);
            }
        }

        /// <summary>
        /// 저장/런 상태 데이터를 전투용 MonoBehaviour에 바인딩한다.
        /// </summary>
        public void BindState(PlayerState state)
        {
            if (state == null)
            {
                return;
            }

            DisplayName = state.Name;
            Level = state.Level <= 0 ? 1 : state.Level;
            CurrentExp = state.Exp;
            Stats = state.Stats ?? new StatBlock();
            StartingClass = state.StartingClass;
            Rune = state.Rune;
            Inventory = state.Inventory ?? new InventoryState();
            Equipment = state.Equipment ?? new EquipmentSlots();
            Consumables = state.Consumables ?? new ConsumableSlots();
            Locker = state.Locker ?? new LockerState();

            if (ActiveSkills == null || ActiveSkills.Length != 2)
            {
                ActiveSkills = new Skill[2];
            }

            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                BindActiveSlotsFromState(state, gsm.Data);
                SyncPassivesFromRunes(gsm.Data);
            }
            else
            {
                GameLog.LogError(
                    "[Player.BindState] GameSystemManager 참조가 없어 ActiveSkills 를 바인딩할 수 없습니다."
                );
            }

            RecalcBonusStats();
        }

        private void BindActiveSlotsFromState(PlayerState state, DataManager data)
        {
            if (state?.ActiveSlotSkillIds == null || state.ActiveSlotSkillIds.Length != 2)
            {
                GameLog.LogError(
                    "[Player.BindState] PlayerState.ActiveSlotSkillIds 가 없거나 길이가 2가 아닙니다."
                );
                ActiveSkills[0] = null;
                ActiveSkills[1] = null;
                return;
            }

            if (data?.Skills == null)
            {
                GameLog.LogError(
                    "[Player.BindState] DataManager.Skills 참조가 없습니다."
                );
                return;
            }

            for (int slot = 0; slot < ActiveSkills.Length; slot++)
            {
                int skillId = state.ActiveSlotSkillIds[slot];
                if (skillId == 0)
                {
                    ActiveSkills[slot] = null;
                    continue;
                }

                if (data.Skills.TryGetValue(skillId, out SkillData skillData))
                {
                    ActiveSkills[slot] = new Skill(skillData);
                }
                else
                {
                    GameLog.LogError(
                        "[Player.BindState] ActiveSlotSkillIds["
                            + slot
                            + "] 데이터 없음: "
                            + skillId
                    );
                    ActiveSkills[slot] = null;
                }
            }
        }

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

namespace Tempt
{
    // Guid2 §7 2026-05-27: 장비 슬롯 ↔ 인벤토리의 두 단계 동작을 한 호출로 묶는 정적 헬퍼.
    // 실패 시 인벤토리/장비 상태 변동 없이 false 반환(롤백 보장).
    // fallback 금지: player / item / Data 누락은 LogError + false.
    /// <summary>
    /// 인벤토리와 EquipmentSlots 사이의 장착/해제를 묶는 단일 진입점.
    /// 호출 후에는 항상 Stats / EventBus 가 갱신된 상태가 보장된다.
    /// </summary>
    public static class EquipFlow
    {
        /// <summary>
        /// 인벤토리 의 장비 인스턴스를 4슬롯 중 하나에 장착한다.
        /// 기존 장착되어 있던 장비는 인벤토리로 복귀한다.
        /// 인벤토리 가득 등 모든 실패 시 false 반환 + 상태 롤백.
        /// </summary>
        public static bool Equip(Player player, Item item)
        {
            if (player == null)
            {
                GameLog.LogError("[EquipFlow.Equip] Player 참조가 없습니다.");
                return false;
            }

            return EquipCore(player.Inventory, player.Equipment, item, player.RecalcBonusStats);
        }

        /// <summary>
        /// 4슬롯 중 한 곳을 해제하여 인벤토리로 옮긴다.
        /// 인벤토리 가득 시 슬롯에 다시 끼우고 false.
        /// </summary>
        public static bool Unequip(Player player, EquipmentSlotId slot)
        {
            if (player == null)
            {
                GameLog.LogError("[EquipFlow.Unequip] Player 참조가 없습니다.");
                return false;
            }

            return UnequipCore(player.Inventory, player.Equipment, slot, player.RecalcBonusStats);
        }

        /// <summary>런 상태(PlayerState)의 장비 인스턴스를 장착한다. Safe/인벤토리 UI에서 사용한다.</summary>
        public static bool Equip(PlayerState player, Item item)
        {
            if (player == null)
            {
                GameLog.LogError("[EquipFlow.Equip] PlayerState 참조가 없습니다.");
                return false;
            }

            return EquipCore(player.Inventory, player.Equipment, item, () => RecalcStateStats(player));
        }

        /// <summary>런 상태(PlayerState)의 장비 슬롯을 해제한다. Safe/인벤토리 UI에서 사용한다.</summary>
        public static bool Unequip(PlayerState player, EquipmentSlotId slot)
        {
            if (player == null)
            {
                GameLog.LogError("[EquipFlow.Unequip] PlayerState 참조가 없습니다.");
                return false;
            }

            return UnequipCore(player.Inventory, player.Equipment, slot, () => RecalcStateStats(player));
        }

        /// <summary>런 상태(PlayerState)의 장비/룬 스탯 보정을 다시 계산한다.</summary>
        public static void RecalculateStats(PlayerState player)
        {
            if (player == null)
            {
                GameLog.LogError("[EquipFlow.RecalculateStats] PlayerState 참조가 없습니다.");
                return;
            }

            RecalcStateStats(player);
        }

        private static bool EquipCore(InventoryState inventory, EquipmentSlots equipment, Item item, System.Action recalc)
        {
            if (IsCombatScene())
            {
                return false;
            }

            if (inventory == null || equipment == null || item?.Data == null)
            {
                GameLog.LogError("[EquipFlow.Equip] Inventory / Equipment / Item.Data 누락.");
                return false;
            }

            EquipmentSlotId slot = item.Data.EquipSlot;
            if (item.Data.Category != ItemCategory.Equipment || item.Data.Stackable || slot == EquipmentSlotId.None)
            {
                GameLog.LogError("[EquipFlow.Equip] 장착 가능한 장비가 아닙니다: " + item.Data.NameKey);
                return false;
            }

            if (!inventory.RemoveEquip(item))
            {
                GameLog.LogError("[EquipFlow.Equip] 인벤토리에 해당 장비가 없습니다: " + item.Data.NameKey);
                return false;
            }

            Item previous = equipment.Equip(slot, item);
            if (previous != null && !inventory.TryAddEquip(previous))
            {
                equipment.Unequip(slot);
                equipment.Equip(slot, previous);
                inventory.AddEquip(item);
                GameLog.LogError("[EquipFlow.Equip] 이전 장비를 인벤토리에 되돌릴 수 없어 롤백했습니다.");
                return false;
            }

            recalc?.Invoke();
            RaiseChanged();
            return true;
        }

        private static bool UnequipCore(InventoryState inventory, EquipmentSlots equipment, EquipmentSlotId slot, System.Action recalc)
        {
            if (IsCombatScene())
            {
                return false;
            }

            if (inventory == null || equipment == null)
            {
                GameLog.LogError("[EquipFlow.Unequip] Inventory / Equipment 누락.");
                return false;
            }

            if (slot == EquipmentSlotId.None)
            {
                GameLog.LogError("[EquipFlow.Unequip] slot 이 None 입니다.");
                return false;
            }

            Item old = equipment.Unequip(slot);
            if (old == null)
            {
                return false;
            }

            if (!inventory.TryAddEquip(old))
            {
                equipment.Equip(slot, old);
                GameLog.LogError("[EquipFlow.Unequip] 인벤토리 장비 공간 부족으로 해제를 롤백했습니다.");
                return false;
            }

            recalc?.Invoke();
            RaiseChanged();
            return true;
        }

        private static void RecalcStateStats(PlayerState player)
        {
            if (player?.Stats == null)
            {
                return;
            }

            player.Stats.ResetEquipmentBonuses();
            player.Stats.ApplyEquipmentBonus(player.Equipment != null ? player.Equipment.AggregateStatMod() : new EquipmentStatMod());
            player.Stats.ResetRuneBonuses();
            EquipmentStatMod runeMod = player.Rune != null ? player.Rune.AggregateStatMod() : new EquipmentStatMod();
            player.Stats.ApplyRuneBonus(StatType.HP, runeMod.HP);
            player.Stats.ApplyRuneBonus(StatType.MP, runeMod.MP);
            player.Stats.ApplyRuneBonus(StatType.ATK, runeMod.ATK);
            player.Stats.ApplyRuneBonus(StatType.DEF, runeMod.DEF);
            player.Stats.ApplyRuneBonus(StatType.SPD, runeMod.SPD);
        }

        private static void RaiseChanged()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseInventoryChanged();
                gsm.Events?.RaiseEquipmentChanged();
            }
        }

        private static bool IsCombatScene()
        {
            return GameSystemManager.TryGetInstance(out GameSystemManager gsm)
                && (gsm.CombatContext != null || (gsm.Scenes != null && gsm.Scenes.CurrentSceneId == SceneId.Combat));
        }
    }
}

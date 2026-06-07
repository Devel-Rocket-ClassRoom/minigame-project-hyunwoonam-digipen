namespace Tempt
{
    /// <summary>
    /// 장비 슬롯. 무기 + 방어구(몸/팔/다리). 한 슬롯에 하나만 장착.
    /// </summary>
    public sealed class EquipmentSlots
    {
        private static readonly SlotBinding[] SlotBindings =
        {
            new SlotBinding(EquipmentSlotId.Weapon, slots => slots.Weapon, (slots, item) => slots.Weapon = item),
            new SlotBinding(EquipmentSlotId.ArmorBody, slots => slots.ArmorBody, (slots, item) => slots.ArmorBody = item),
            new SlotBinding(EquipmentSlotId.ArmorArms, slots => slots.ArmorArms, (slots, item) => slots.ArmorArms = item),
            new SlotBinding(EquipmentSlotId.ArmorLegs, slots => slots.ArmorLegs, (slots, item) => slots.ArmorLegs = item),
        };

        /// <summary>무기.</summary>
        public Item Weapon;

        /// <summary>방어구(몸통).</summary>
        public Item ArmorBody;

        /// <summary>방어구(팔).</summary>
        public Item ArmorArms;

        /// <summary>방어구(다리).</summary>
        public Item ArmorLegs;

        /// <summary>
        /// 슬롯에 장착. 기존 장비는 인벤토리로 반환.
        /// </summary>
        public Item Equip(EquipmentSlotId slot, Item item)
        {
            if (slot == EquipmentSlotId.None || item == null || item.Data == null || item.Data.EquipSlot != slot)
            {
                return null;
            }

            Item old = GetSlot(slot);
            SetSlot(slot, item);
            RaiseEquipmentChanged();
            return old;
        }

        /// <summary>
        /// 슬롯에서 해제. 인벤토리로 반환.
        /// </summary>
        public Item Unequip(EquipmentSlotId slot)
        {
            Item old = GetSlot(slot);
            if (old == null)
            {
                return null;
            }

            SetSlot(slot, null);
            RaiseEquipmentChanged();
            return old;
        }

        /// <summary>
        /// 4개 슬롯의 스탯 보정 합산.
        /// </summary>
        public EquipmentStatMod AggregateStatMod()
        {
            var result = new EquipmentStatMod();
            for (int i = 0; i < SlotBindings.Length; i++)
            {
                AddMod(result, SlotBindings[i].Get(this));
            }
            return result;
        }

        /// <summary>해당 아이템이 어느 슬롯에든 장착돼 있는지. (하드코딩 슬롯 비교 중복 제거용)</summary>
        public bool Contains(Item item)
        {
            if (item == null)
            {
                return false;
            }

            for (int i = 0; i < SlotBindings.Length; i++)
            {
                if (SlotBindings[i].Get(this) == item)
                {
                    return true;
                }
            }

            return false;
        }

        private Item GetSlot(EquipmentSlotId slot)
        {
            SlotBinding binding = FindBinding(slot);
            return binding != null ? binding.Get(this) : null;
        }

        private void SetSlot(EquipmentSlotId slot, Item item)
        {
            SlotBinding binding = FindBinding(slot);
            binding?.Set(this, item);
        }

        private static SlotBinding FindBinding(EquipmentSlotId slot)
        {
            for (int i = 0; i < SlotBindings.Length; i++)
            {
                if (SlotBindings[i].Slot == slot)
                {
                    return SlotBindings[i];
                }
            }

            return null;
        }

        private static void AddMod(EquipmentStatMod target, Item item)
        {
            if (target == null || item == null)
            {
                return;
            }

            EquipmentStatMod mod = item.GetFinalMod();
            target.HP += mod.HP;
            target.MP += mod.MP;
            target.ATK += mod.ATK;
            target.DEF += mod.DEF;
            target.SPD += mod.SPD;
        }

        private static void RaiseEquipmentChanged()
        {
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm))
            {
                gsm.Events?.RaiseEquipmentChanged();
            }
        }

        private sealed class SlotBinding
        {
            public readonly EquipmentSlotId Slot;
            private readonly System.Func<EquipmentSlots, Item> getter;
            private readonly System.Action<EquipmentSlots, Item> setter;

            public SlotBinding(EquipmentSlotId slot, System.Func<EquipmentSlots, Item> getter, System.Action<EquipmentSlots, Item> setter)
            {
                Slot = slot;
                this.getter = getter;
                this.setter = setter;
            }

            public Item Get(EquipmentSlots slots)
            {
                return getter(slots);
            }

            public void Set(EquipmentSlots slots, Item item)
            {
                setter(slots, item);
            }
        }
    }
}


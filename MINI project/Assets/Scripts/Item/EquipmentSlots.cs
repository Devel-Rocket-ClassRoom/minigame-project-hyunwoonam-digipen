namespace Tempt
{
    /// <summary>
    /// 장비 슬롯. 무기 + 방어구(몸/팔/다리). 한 슬롯에 하나만 장착.
    /// </summary>
    public sealed class EquipmentSlots
    {
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
            if (slot == EquipmentSlotId.None || item == null || item.Data == null || item.Data.EquipSlot != slot) //Wave0write
            { //Wave0write
                return null; //Wave0write
            } //Wave0write

            Item old = GetSlot(slot); //Wave0write
            SetSlot(slot, item); //Wave0write
            RaiseEquipmentChanged(); //Wave0write
            return old; //Wave0write
        }

        /// <summary>
        /// 슬롯에서 해제. 인벤토리로 반환.
        /// </summary>
        public Item Unequip(EquipmentSlotId slot)
        {
            Item old = GetSlot(slot); //Wave0write
            if (old == null) //Wave0write
            { //Wave0write
                return null; //Wave0write
            } //Wave0write

            SetSlot(slot, null); //Wave0write
            RaiseEquipmentChanged(); //Wave0write
            return old; //Wave0write
        }

        /// <summary>
        /// 4개 슬롯의 스탯 보정 합산.
        /// </summary>
        public EquipmentStatMod AggregateStatMod()
        {
            var result = new EquipmentStatMod(); //Wave0write
            AddMod(result, Weapon); //Wave0write
            AddMod(result, ArmorBody); //Wave0write
            AddMod(result, ArmorArms); //Wave0write
            AddMod(result, ArmorLegs); //Wave0write
            return result; //Wave0write
        }

        private Item GetSlot(EquipmentSlotId slot) //Wave0write
        { //Wave0write
            switch (slot) //Wave0write
            { //Wave0write
                case EquipmentSlotId.Weapon: return Weapon; //Wave0write
                case EquipmentSlotId.ArmorBody: return ArmorBody; //Wave0write
                case EquipmentSlotId.ArmorArms: return ArmorArms; //Wave0write
                case EquipmentSlotId.ArmorLegs: return ArmorLegs; //Wave0write
                default: return null; //Wave0write
            } //Wave0write
        } //Wave0write

        private void SetSlot(EquipmentSlotId slot, Item item) //Wave0write
        { //Wave0write
            switch (slot) //Wave0write
            { //Wave0write
                case EquipmentSlotId.Weapon: Weapon = item; break; //Wave0write
                case EquipmentSlotId.ArmorBody: ArmorBody = item; break; //Wave0write
                case EquipmentSlotId.ArmorArms: ArmorArms = item; break; //Wave0write
                case EquipmentSlotId.ArmorLegs: ArmorLegs = item; break; //Wave0write
            } //Wave0write
        } //Wave0write

        private static void AddMod(EquipmentStatMod target, Item item) //Wave0write
        { //Wave0write
            if (target == null || item == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            EquipmentStatMod mod = item.GetFinalMod(); //Wave0write
            target.HP += mod.HP; //Wave0write
            target.MP += mod.MP; //Wave0write
            target.ATK += mod.ATK; //Wave0write
            target.DEF += mod.DEF; //Wave0write
            target.SPD += mod.SPD; //Wave0write
        } //Wave0write

        private static void RaiseEquipmentChanged() //Wave0write
        { //Wave0write
            if (GameSystemManager.TryGetInstance(out GameSystemManager gsm)) //Wave0write
            { //Wave0write
                gsm.Events?.RaiseEquipmentChanged(); //Wave0write
            } //Wave0write
        } //Wave0write
    }
}


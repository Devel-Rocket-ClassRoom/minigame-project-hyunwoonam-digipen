namespace Tempt
{
    /// <summary>
    /// 장비 슬롯. 무기 + 방어구(몸/팔/다리). 한 슬롯에 하나만 장착.
    /// </summary>
    public sealed class EquipmentSlotst
    {
        /// <summary>무기.</summary>
        public Itemt Weapon;

        /// <summary>방어구(몸통).</summary>
        public Itemt ArmorBody;

        /// <summary>방어구(팔).</summary>
        public Itemt ArmorArms;

        /// <summary>방어구(다리).</summary>
        public Itemt ArmorLegs;

        /// <summary>
        /// 슬롯에 장착. 기존 장비는 인벤토리로 반환.
        /// </summary>
        public Itemt Equip(EquipmentSlotIdt slot, Itemt item)
        {
            // 동작 요약:
            // - slot에 맞는 필드 교체.
            // - 슬롯이 None이거나 item.Data.EquipSlot이 다르면 거절.
            // - 기존 장비 반환 → 호출자가 인벤토리에 추가.
            // - EventBust.RaiseEquipmentChanged().
            //TODO: if (slot == EquipmentSlotIdt.None || item == null) return null;
            //TODO: if (item.Data.EquipSlot != slot) return null;
            //TODO: Itemt old = GetSlot(slot);
            //TODO: SetSlot(slot, item);
            //TODO: GameSystemManagert.Instance.Events.RaiseEquipmentChanged();
            //TODO: return old; // 호출자가 인벤토리에 추가
            if (slot == EquipmentSlotIdt.None || item == null || item.Data == null || item.Data.EquipSlot != slot) //Wave0write
            { //Wave0write
                return null; //Wave0write
            } //Wave0write

            Itemt old = GetSlot(slot); //Wave0write
            SetSlot(slot, item); //Wave0write
            RaiseEquipmentChanged(); //Wave0write
            return old; //Wave0write
        }

        /// <summary>
        /// 슬롯에서 해제. 인벤토리로 반환.
        /// </summary>
        public Itemt Unequip(EquipmentSlotIdt slot)
        {
            // 동작 요약: 해당 슬롯 null로 설정 후 기존 아이템 반환.
            //TODO: Itemt old = GetSlot(slot);
            //TODO: SetSlot(slot, null);
            //TODO: if (old != null) GameSystemManagert.Instance.Events.RaiseEquipmentChanged();
            //TODO: return old;
            Itemt old = GetSlot(slot); //Wave0write
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
        public EquipmentStatModt AggregateStatMod()
        {
            // 동작 요약: 각 슬롯의 Itemt.GetFinalMod() 합산.
            //TODO: var result = new EquipmentStatModt();
            //TODO: foreach (var item in new[] { Weapon, ArmorBody, ArmorArms, ArmorLegs })
            //TODO: {
            //TODO:     if (item == null) continue;
            //TODO:     var mod = item.GetFinalMod();
            //TODO:     result.HP  += mod.HP;  result.MP  += mod.MP;
            //TODO:     result.ATK += mod.ATK; result.DEF += mod.DEF; result.SPD += mod.SPD;
            //TODO: }
            //TODO: return result;
            var result = new EquipmentStatModt(); //Wave0write
            AddMod(result, Weapon); //Wave0write
            AddMod(result, ArmorBody); //Wave0write
            AddMod(result, ArmorArms); //Wave0write
            AddMod(result, ArmorLegs); //Wave0write
            return result; //Wave0write
        }

        private Itemt GetSlot(EquipmentSlotIdt slot) //Wave0write
        { //Wave0write
            switch (slot) //Wave0write
            { //Wave0write
                case EquipmentSlotIdt.Weapon: return Weapon; //Wave0write
                case EquipmentSlotIdt.ArmorBody: return ArmorBody; //Wave0write
                case EquipmentSlotIdt.ArmorArms: return ArmorArms; //Wave0write
                case EquipmentSlotIdt.ArmorLegs: return ArmorLegs; //Wave0write
                default: return null; //Wave0write
            } //Wave0write
        } //Wave0write

        private void SetSlot(EquipmentSlotIdt slot, Itemt item) //Wave0write
        { //Wave0write
            switch (slot) //Wave0write
            { //Wave0write
                case EquipmentSlotIdt.Weapon: Weapon = item; break; //Wave0write
                case EquipmentSlotIdt.ArmorBody: ArmorBody = item; break; //Wave0write
                case EquipmentSlotIdt.ArmorArms: ArmorArms = item; break; //Wave0write
                case EquipmentSlotIdt.ArmorLegs: ArmorLegs = item; break; //Wave0write
            } //Wave0write
        } //Wave0write

        private static void AddMod(EquipmentStatModt target, Itemt item) //Wave0write
        { //Wave0write
            if (target == null || item == null) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            EquipmentStatModt mod = item.GetFinalMod(); //Wave0write
            target.HP += mod.HP; //Wave0write
            target.MP += mod.MP; //Wave0write
            target.ATK += mod.ATK; //Wave0write
            target.DEF += mod.DEF; //Wave0write
            target.SPD += mod.SPD; //Wave0write
        } //Wave0write

        private static void RaiseEquipmentChanged() //Wave0write
        { //Wave0write
            if (GameSystemManagert.TryGetInstance(out GameSystemManagert gsm)) //Wave0write
            { //Wave0write
                gsm.Events?.RaiseEquipmentChanged(); //Wave0write
            } //Wave0write
        } //Wave0write
    }
}

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
            return null;
        }

        /// <summary>
        /// 슬롯에서 해제. 인벤토리로 반환.
        /// </summary>
        public Itemt Unequip(EquipmentSlotIdt slot)
        {
            // 동작 요약: 해당 슬롯 null로 설정 후 기존 아이템 반환.
            return null;
        }

        /// <summary>
        /// 4개 슬롯의 스탯 보정 합산.
        /// </summary>
        public EquipmentStatModt AggregateStatMod()
        {
            // 동작 요약: 각 슬롯의 Itemt.GetFinalMod() 합산.
            return null;
        }
    }
}

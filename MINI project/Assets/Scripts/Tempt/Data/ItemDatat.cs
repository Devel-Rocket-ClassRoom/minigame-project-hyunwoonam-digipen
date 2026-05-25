namespace Tempt
{
    /// <summary>
    /// 아이템 정적 데이터. CSV 1행.
    /// </summary>
    public sealed class ItemDatat : DataTablet
    {
        /// <summary>아이템 카테고리.</summary>
        public ItemCategoryt Category;

        /// <summary>장비 슬롯(Equipment일 때만 의미).</summary>
        public EquipmentSlotIdt EquipSlot;

        /// <summary>장비 부여 스탯.</summary>
        public EquipmentStatModt EquipMod;

        /// <summary>소모 효과 키(Consumable일 때만).</summary>
        public string ConsumeEffectKey;

        /// <summary>구매 가격(기본).</summary>
        public int BaseBuyPrice;

        /// <summary>판매 가격(기본).</summary>
        public int BaseSellPrice;

        /// <summary>중첩 가능 여부.</summary>
        public bool Stackable;

        /// <summary>최대 스택.</summary>
        public int MaxStack;

        /// <summary>후퇴 아이템 여부(전투에서 안전지대로 즉시 복귀).</summary>
        public bool IsRetreat;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            // 동작 요약: 필드 순서대로 파싱.
        }
    }

    /// <summary>아이템 카테고리.</summary>
    public enum ItemCategoryt
    {
        /// <summary>소모.</summary>
        Consumable,

        /// <summary>장비.</summary>
        Equipment,

        /// <summary>재료/기타.</summary>
        Material,
    }

    /// <summary>장비 슬롯.</summary>
    public enum EquipmentSlotIdt
    {
        /// <summary>없음(소모/재료용).</summary>
        None,

        /// <summary>무기.</summary>
        Weapon,

        /// <summary>방어구(몸통).</summary>
        ArmorBody,

        /// <summary>방어구(팔).</summary>
        ArmorArms,

        /// <summary>방어구(다리).</summary>
        ArmorLegs,
    }

    /// <summary>장비가 부여하는 스탯 보정.</summary>
    public sealed class EquipmentStatModt
    {
        /// <summary>HP 보정.</summary>
        public int HP;

        /// <summary>MP 보정.</summary>
        public int MP;

        /// <summary>ATK 보정.</summary>
        public int ATK;

        /// <summary>DEF 보정.</summary>
        public int DEF;

        /// <summary>SPD 보정.</summary>
        public int SPD;
    }
}

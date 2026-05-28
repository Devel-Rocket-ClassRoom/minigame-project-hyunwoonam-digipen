namespace Tempt
{
    /// <summary>
    /// 아이템 정적 데이터. ItemTable.csv 1행.
    /// </summary>
    public sealed class ItemData : DataTable
    {
        /// <summary>아이템 카테고리.</summary>
        public ItemCategory Category;

        /// <summary>
        /// 세부 분류 문자열.
        /// 예: HP_Potion, MP_Potion, Offensive, Escape, Weapon, ArmorBody, ArmorArms, ArmorLegs.
        /// enum이 아니라 CSV 문자열로 유지하되 DataManager 검증 단계에서 허용값을 검사한다.
        /// </summary>
        public string SubCategory;

        /// <summary>장비 슬롯(Equipment일 때만 의미).</summary>
        public EquipmentSlotId EquipSlot;

        /// <summary>장비가 부여하는 스탯 보정(Equipment일 때만).</summary>
        public EquipmentStatMod EquipMod;

        /// <summary>소모 효과 키(Consumable일 때만). 회복/후퇴/공격력 버프 등 분기 기준.</summary>
        public string ConsumeEffectKey;

        /// <summary>
        /// 효과 수치. 소모 아이템: 회복량/계수. 장비: 주 스탯 계수.
        /// EquipMod의 세부 필드와 중복될 수 있으나 단일 대표 수치로 UI 표시에 사용.
        /// </summary>
        public float ParamValue;

        /// <summary>침식률 적용 전 기본 가격(골드). 구매/판매가는 Shop 이 산식으로 계산한다.</summary>
        public int BasePrice;

        /// <summary>중첩 가능 여부(소모품/재료 true, 장비 false).</summary>
        public bool Stackable;

        /// <summary>최대 스택.</summary>
        public int MaxStack;

        /// <summary>후퇴 아이템 여부(전투에서 안전지대로 즉시 복귀).</summary>
        public bool IsRetreat;

        /// <inheritdoc/>
        public override void Parse(string[] cells)
        {
            // 동작 요약: ItemTable.csv 열 순서대로 파싱.
            // - cells[0] = ItemID → Id
            // - cells[1] = NameKey
            // - cells[2] = Category (enum 문자열 → ItemCategory)
            // - cells[3] = SubCategory (string: HP_Potion/MP_Potion/Offensive/Escape/Weapon/ArmorBody/ArmorArms/ArmorLegs)
            // - cells[4] = BasePrice (int)
            // - cells[5] = ParamValue (float)
            // - cells[6] = DescriptionKey → DescKey
            // - EquipSlot, EquipMod, ConsumeEffectKey, Stackable, MaxStack, IsRetreat는
            //   추가 컬럼 또는 SubCategory 기반 기본값으로 처리
            //TODO: Id           = int.Parse(cells[0]);
            //TODO: NameKey      = cells[1];
            //TODO: Category     = (ItemCategory)System.Enum.Parse(typeof(ItemCategory), cells[2]);
            //TODO: SubCategory  = cells[3];
            //TODO: BasePrice    = int.Parse(cells[4]);
            //TODO: ParamValue   = float.Parse(cells[5]);
            //TODO: DescKey      = cells[6];
            //TODO: // Stackable/IsRetreat: Category 기반 기본값
            //TODO: Stackable = (Category == ItemCategory.Consumable || Category == ItemCategory.Material);
            //TODO: IsRetreat = SubCategory == "Escape";
            //TODO: MaxStack      = cells.Length > 7 ? int.Parse(cells[7]) : (Stackable ? 99 : 1);
        }
    }

    /// <summary>아이템 카테고리.</summary>
    public enum ItemCategory
    {
        /// <summary>소모.</summary>
        Consumable,

        /// <summary>장비.</summary>
        Equipment,

        /// <summary>재료/기타.</summary>
        Material,
    }

    /// <summary>장비 슬롯.</summary>
    public enum EquipmentSlotId
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
    public sealed class EquipmentStatMod
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


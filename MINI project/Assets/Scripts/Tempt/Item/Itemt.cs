namespace Tempt
{
    /// <summary>
    /// 아이템 런타임 인스턴스. 정적 데이터 참조 + 보유 컨텍스트(인벤토리 위치 등).
    /// 데이터가 Stackable=true일 경우 InventoryEntryt의 Count로 표현하고 인스턴스는 1개만 사용.
    /// </summary>
    public sealed class Itemt
    {
        /// <summary>참조 데이터.</summary>
        public ItemDatat Data;

        /// <summary>강화 단계(장비 한정, 0=기본).</summary>
        public int Enhancement;

        /// <summary>
        /// 강화 적용 후 최종 보정 스탯 반환(장비 한정).
        /// </summary>
        public EquipmentStatModt GetFinalMod()
        {
            // 동작 요약:
            // - Data.EquipMod에 Enhancement 곱셈 보정 적용.
            // - 대장간 강화 수식은 BalanceDatat.
            return null;
        }
    }
}

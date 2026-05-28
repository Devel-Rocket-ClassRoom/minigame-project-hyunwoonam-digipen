namespace Tempt
{
    /// <summary>
    /// 아이템 런타임 인스턴스. 정적 데이터 참조 + 보유 컨텍스트(인벤토리 위치 등).
    /// 데이터가 Stackable=true일 경우 InventoryEntryt의 Count로 표현하고 인스턴스는 1개만 사용.
    /// </summary>
    public sealed class Item
    {
        /// <summary>참조 데이터.</summary>
        public ItemData Data;

        /// <summary>강화 단계(장비 한정, 0=기본).</summary>
        public int Enhancement;

        /// <summary>
        /// 강화 적용 후 최종 보정 스탯 반환(장비 한정).
        /// </summary>
        public EquipmentStatMod GetFinalMod()
        {
            // 동작 요약:
            // - Data.EquipMod에 Enhancement 곱셈 보정 적용.
            // - 대장간 강화 수식은 BalanceData.
            //TODO: if (Data == null || Data.EquipMod == null) return new EquipmentStatMod();
            //TODO: // 강화 보정 배수: 1 + Enhancement * BalanceData.EnhanceMultiplier
            //TODO: float multiplier = 1f + Enhancement * BalanceData.EnhanceMultiplier;
            //TODO: return new EquipmentStatMod
            //TODO: {
            //TODO:     HP  = UnityEngine.Mathf.RoundToInt(Data.EquipMod.HP  * multiplier),
            //TODO:     MP  = UnityEngine.Mathf.RoundToInt(Data.EquipMod.MP  * multiplier),
            //TODO:     ATK = UnityEngine.Mathf.RoundToInt(Data.EquipMod.ATK * multiplier),
            //TODO:     DEF = UnityEngine.Mathf.RoundToInt(Data.EquipMod.DEF * multiplier),
            //TODO:     SPD = UnityEngine.Mathf.RoundToInt(Data.EquipMod.SPD * multiplier),
            //TODO: };
            if (Data == null || Data.EquipMod == null) //Wave0write
            { //Wave0write
                return new EquipmentStatMod(); //Wave0write
            } //Wave0write

            float enhanceMultiplier = 0f; //Wave0write
            if (Enhancement > 0) //Wave0write
            { //Wave0write
                if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.Data?.Balance == null) //Wave0write
                { //Wave0write
                    UnityEngine.Debug.LogError("[Item.GetFinalMod] BalanceData.EnhanceMultiplier 참조가 없습니다."); //Wave0write
                    return new EquipmentStatMod(); //Wave0write
                } //Wave0write

                enhanceMultiplier = gsm.Data.Balance.EnhanceMultiplier; //Wave0write
            } //Wave0write

            float multiplier = 1f + System.Math.Max(0, Enhancement) * enhanceMultiplier; //Wave0write
            return new EquipmentStatMod //Wave0write
            { //Wave0write
                HP = UnityEngine.Mathf.RoundToInt(Data.EquipMod.HP * multiplier), //Wave0write
                MP = UnityEngine.Mathf.RoundToInt(Data.EquipMod.MP * multiplier), //Wave0write
                ATK = UnityEngine.Mathf.RoundToInt(Data.EquipMod.ATK * multiplier), //Wave0write
                DEF = UnityEngine.Mathf.RoundToInt(Data.EquipMod.DEF * multiplier), //Wave0write
                SPD = UnityEngine.Mathf.RoundToInt(Data.EquipMod.SPD * multiplier), //Wave0write
            }; //Wave0write
        }
    }
}


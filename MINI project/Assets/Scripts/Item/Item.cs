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

    /// <summary>연속 강화 실패 횟수. 천장 성공 계산에 사용하고 성공 시 0으로 초기화된다.</summary>
    public int EnhanceFailStreak;

    /// <summary>
    /// 강화 적용 후 최종 보정 스탯 반환(장비 한정).
    /// </summary>
    public EquipmentStatMod GetFinalMod()
    {
        // 동작 요약:
        // - Data.EquipMod에 Enhancement 곱셈 보정 적용.
        // - 대장간 강화 수식은 BalanceData.
        if (Data == null || Data.EquipMod == null)
        {
            return new EquipmentStatMod();
        }

        float enhanceMultiplier = 0f;
        if (Enhancement > 0)
        {
            if (!GameSystemManager.TryGetInstance(out GameSystemManager gsm) || gsm.Data?.Balance == null)
            {
                GameLog.LogError("[Item.GetFinalMod] BalanceData.EnhanceMultiplier 참조가 없습니다.");
                return new EquipmentStatMod();
            }

            enhanceMultiplier = gsm.Data.Balance.EnhanceMultiplier;
        }

        float multiplier = 1f + System.Math.Max(0, Enhancement) * enhanceMultiplier;
        return new EquipmentStatMod
        {
            HP = UnityEngine.Mathf.RoundToInt(Data.EquipMod.HP * multiplier),
            MP = UnityEngine.Mathf.RoundToInt(Data.EquipMod.MP * multiplier),
            ATK = UnityEngine.Mathf.RoundToInt(Data.EquipMod.ATK * multiplier),
            DEF = UnityEngine.Mathf.RoundToInt(Data.EquipMod.DEF * multiplier),
            SPD = UnityEngine.Mathf.RoundToInt(Data.EquipMod.SPD * multiplier),
        };
    }
}


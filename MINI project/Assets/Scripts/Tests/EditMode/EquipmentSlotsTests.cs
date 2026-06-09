using NUnit.Framework;

/// <summary>
/// 장비 슬롯 장착/해제/스왑/합산 순수 로직 회귀 테스트.
/// 씬·싱글턴 의존 없음(Enhancement=0 → GetFinalMod 순수). Unity Test Runner(EditMode).
/// </summary>
public sealed class EquipmentSlotsTests
{
    private static Item MakeEquip(EquipmentSlotId slot, int atk = 0, int def = 0, int hp = 0)
    {
        return new Item
        {
            Enhancement = 0,
            Data = new ItemData
            {
                EquipSlot = slot,
                EquipMod = new EquipmentStatMod { ATK = atk, DEF = def, HP = hp },
            },
        };
    }

    [Test]
    public void Equip_EmptySlot_ReturnsNull_AndHolds()
    {
        var slots = new EquipmentSlots();
        Item weapon = MakeEquip(EquipmentSlotId.Weapon, atk: 5);

        Item previous = slots.Equip(EquipmentSlotId.Weapon, weapon);

        Assert.IsNull(previous, "빈 슬롯 장착 시 이전 아이템은 없어야 한다.");
        Assert.AreSame(weapon, slots.Weapon);
        Assert.IsTrue(slots.Contains(weapon));
    }

    [Test]
    public void Equip_WrongSlot_Rejected()
    {
        var slots = new EquipmentSlots();
        Item weapon = MakeEquip(EquipmentSlotId.Weapon, atk: 5);

        // 무기 아이템을 방어구 슬롯에 장착 시도 → 거부.
        Item result = slots.Equip(EquipmentSlotId.ArmorBody, weapon);

        Assert.IsNull(result);
        Assert.IsNull(slots.ArmorBody);
        Assert.IsFalse(slots.Contains(weapon));
    }

    [Test]
    public void Equip_Swap_ReturnsPrevious()
    {
        var slots = new EquipmentSlots();
        Item first = MakeEquip(EquipmentSlotId.Weapon, atk: 5);
        Item second = MakeEquip(EquipmentSlotId.Weapon, atk: 9);

        slots.Equip(EquipmentSlotId.Weapon, first);
        Item swappedOut = slots.Equip(EquipmentSlotId.Weapon, second);

        Assert.AreSame(first, swappedOut, "스왑 시 이전 장비를 반환해야 한다.");
        Assert.AreSame(second, slots.Weapon);
        Assert.IsFalse(slots.Contains(first));
        Assert.IsTrue(slots.Contains(second));
    }

    [Test]
    public void Unequip_RemovesAndReturns()
    {
        var slots = new EquipmentSlots();
        Item body = MakeEquip(EquipmentSlotId.ArmorBody, def: 3);
        slots.Equip(EquipmentSlotId.ArmorBody, body);

        Item removed = slots.Unequip(EquipmentSlotId.ArmorBody);

        Assert.AreSame(body, removed);
        Assert.IsNull(slots.ArmorBody);
        Assert.IsNull(slots.Unequip(EquipmentSlotId.ArmorBody), "빈 슬롯 해제는 null.");
    }

    [Test]
    public void AggregateStatMod_SumsAcrossSlots()
    {
        var slots = new EquipmentSlots();
        slots.Equip(EquipmentSlotId.Weapon, MakeEquip(EquipmentSlotId.Weapon, atk: 5));
        slots.Equip(EquipmentSlotId.ArmorBody, MakeEquip(EquipmentSlotId.ArmorBody, def: 3, hp: 10));
        slots.Equip(EquipmentSlotId.ArmorLegs, MakeEquip(EquipmentSlotId.ArmorLegs, def: 2));

        EquipmentStatMod total = slots.AggregateStatMod();

        Assert.AreEqual(5, total.ATK);
        Assert.AreEqual(5, total.DEF);
        Assert.AreEqual(10, total.HP);
        Assert.AreEqual(0, total.SPD);
    }

    [Test]
    public void AggregateStatMod_Empty_IsZero()
    {
        var slots = new EquipmentSlots();
        EquipmentStatMod total = slots.AggregateStatMod();
        Assert.AreEqual(0, total.ATK + total.DEF + total.HP + total.MP + total.SPD);
    }
}

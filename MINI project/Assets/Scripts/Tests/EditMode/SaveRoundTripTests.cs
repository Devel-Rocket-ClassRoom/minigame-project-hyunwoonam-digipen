using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// SaveSnapshot 라운드트립(런타임 → 스냅샷 → 런타임) 회귀 테스트.
/// 구조적 부채 방어: 런타임 모델에 필드를 추가하고 DTO/From/To 매핑을 갱신하지 않으면
/// (조용한 저장 손실) 이 테스트가 실패해 즉시 드러난다.
/// 새 직렬화 필드를 추가하면 여기에도 비-기본값 단언을 추가할 것.
/// </summary>
public sealed class SaveRoundTripTests
{
    private static GameRunState BuildPopulatedRun()
    {
        var owned = new HashSet<int>();
        owned.Add(10);
        owned.Add(20);

        var stats = new StatBlock
        {
            MaxHP = 120,
            CurrentHP = 110,
            MaxMP = 40,
            CurrentMP = 30,
            ATK = 25,
            DEF = 12,
            SPD = 18,
        };

        var rune = new PlayerRuneState
        {
            RunePoints = 3,
            UnlockedIds = new HashSet<int>(),
            InvestedPointsByNode = new Dictionary<int, int>(),
        };

        var player = new PlayerState
        {
            Level = 5,
            Exp = 99,
            StartingClass = RuneClass.MagicDealer,
            Stats = stats,
            Rune = rune,
            Inventory = new InventoryState(),
            Equipment = new EquipmentSlots(),
            Consumables = new ConsumableSlots(),
            Locker = new LockerState(),
            OwnedSkillIds = owned,
            ActiveSlotSkillIds = new int[] { 10, 20 },
        };
        player.Inventory.StackableItems[101] = 3;
        player.Inventory.StackableItems[102] = 1;
        player.Consumables.SlotItemIds[0] = 101;
        player.Consumables.SlotItemIds[2] = 102;

        var run = new GameRunState
        {
            CurrentDay = 7,
            Gold = 1234,
            ManaStone = 56,
            Player = player,
            Roster = new CompanionRosterState(),
            Erosion = new ErosionStateModel(),
            SafeUnlocks = new SafeZoneUnlockState(6),
        };
        run.Erosion.EnsureStageCount(6);
        run.Erosion.StageRates[2] = 35f;
        run.SafeUnlocks.Unlock(1);
        run.EnsureMineState();
        run.MineActivated[0] = true;
        run.MineStored[0] = 50;
        run.LastMineGainDay[0] = 3;
        run.Roster.Recruit(new CompanionInstance { CompanionDataId = 2001, Level = 2, Exp = 5 });
        return run;
    }

    [Test]
    public void FullRunState_RoundTrips_AllDomains()
    {
        var data = new DataManager();
        data.LoadAll();

        GameRunState src = BuildPopulatedRun();
        SaveSnapshot snap = SaveSnapshot.FromGameRunStatet(src, SceneId.Safe1);
        GameRunState r = snap.ToGameRunStatet(data);

        // 스칼라 자원/진행
        Assert.AreEqual(7, r.CurrentDay, "CurrentDay");
        Assert.AreEqual(1234, r.Gold, "Gold");
        Assert.AreEqual(56, r.ManaStone, "ManaStone");

        // 플레이어 핵심
        Assert.AreEqual(5, r.Player.Level, "Level");
        Assert.AreEqual(99, r.Player.Exp, "Exp");
        Assert.AreEqual(RuneClass.MagicDealer, r.Player.StartingClass, "StartingClass");

        // 스탯
        Assert.AreEqual(120, r.Player.Stats.MaxHP, "MaxHP");
        Assert.AreEqual(110, r.Player.Stats.CurrentHP, "CurrentHP");
        Assert.AreEqual(25, r.Player.Stats.ATK, "ATK");
        Assert.AreEqual(18, r.Player.Stats.SPD, "SPD");

        // 스킬 풀/슬롯 (HashSet/배열 → List 변환 경로)
        Assert.AreEqual(2, r.Player.OwnedSkillIds.Count, "OwnedSkillIds count");
        Assert.IsTrue(r.Player.OwnedSkillIds.Contains(10) && r.Player.OwnedSkillIds.Contains(20), "OwnedSkillIds values");
        Assert.AreEqual(10, r.Player.ActiveSlotSkillIds[0], "ActiveSlot0");
        Assert.AreEqual(20, r.Player.ActiveSlotSkillIds[1], "ActiveSlot1");

        // 룬 포인트 (투자/해금은 룬트리 유효노드 의존이라 별도 — RunePoints 로 도메인 가드)
        Assert.AreEqual(3, r.Player.Rune.RunePoints, "RunePoints");

        // 인벤토리(Dictionary 변환)
        Assert.AreEqual(3, r.Player.Inventory.StackableItems[101], "Inventory 101");
        Assert.AreEqual(1, r.Player.Inventory.StackableItems[102], "Inventory 102");

        // 소모품 슬롯
        Assert.AreEqual(101, r.Player.Consumables.SlotItemIds[0], "Consumable slot0");
        Assert.AreEqual(102, r.Player.Consumables.SlotItemIds[2], "Consumable slot2");

        // 침식률
        Assert.That(r.Erosion.GetRate(2), Is.EqualTo(35f).Within(0.0001f), "Erosion stage2 rate");

        // 안전지대 해금
        Assert.IsTrue(r.SafeUnlocks.IsUnlocked(1), "SafeUnlocks");

        // 광산 3개 배열
        Assert.IsTrue(r.MineActivated[0], "MineActivated[0]");
        Assert.AreEqual(50, r.MineStored[0], "MineStored[0]");
        Assert.AreEqual(3, r.LastMineGainDay[0], "LastMineGainDay[0]");

        // 동료 명부
        Assert.AreEqual(1, r.Roster.Bench.Count, "Roster bench count");
        Assert.AreEqual(2001, r.Roster.Bench[0].CompanionDataId, "Roster companion id");
        Assert.AreEqual(2, r.Roster.Bench[0].Level, "Roster companion level");
    }
}

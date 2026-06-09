using System.Collections.Generic;

/// <summary>
/// 한 런의 전체 상태를 직렬화하기 위한 스냅샷.
/// "종료 시점 전체 복원" 정책에 따라 모든 동적 상태를 포함.
/// 플로어 맵은 seed 재생성이 아니라 저장 시점의 전체 노드 구조를 직접 포함한다.
/// 저장 형식: JSON (PersistentDataPath/save.json).
/// </summary>
[System.Serializable]
public sealed partial class SaveSnapshot
{
    /// <summary>저장 일자(ISO).</summary>
    public string SavedAtIso;

    /// <summary>클리어 완료된 런 여부(true면 Continue 불가).</summary>
    public bool IsCompleted;

    /// <summary>
    /// 클리어 기록에서 불러온 런(침식 영구 정지). IsCompleted 와 독립.
    /// 기록 플레이 중 자동 저장돼도 유지된다. 구버전 save 는 false.
    /// </summary>
    public bool ErosionFrozen;

    /// <summary>현재 게임 내 일자.</summary>
    public int CurrentDay;

    /// <summary>현재 위치한 층/씬.</summary>
    public SaveLocation Location;

    /// <summary>플로어 맵 전체 구조와 진행 상태 직렬화.</summary>
    public FloorMapSnapshot FloorMap;

    /// <summary>플레이어 상태.</summary>
    public PlayerSnapshot Player;

    /// <summary>동료 명부.</summary>
    public RosterSnapshot Roster;

    /// <summary>침식 상태(단계별 침식률 + 잠금 상태).</summary>
    public ErosionSnapshot Erosion;

    /// <summary>
    /// 안전지대 해금 상태(Safe0~5). 인덱스 = 안전지대 번호.
    /// false = 잠금, true = 해금.
    /// </summary>
    public List<bool> SafeUnlocks;

    /// <summary>상점 재고와 구매 이력.</summary>
    public List<ShopStockEntrySnapshot> ShopStock;

    /// <summary>골드.</summary>
    public int Gold;

    /// <summary>마석.</summary>
    public int ManaStone;

    /// <summary>광산 활성 상태(Safe3~5).</summary>
    public List<bool> MineActivated;

    /// <summary>광산별 미수령 누적 골드.</summary>
    public List<int> MineStored;

    /// <summary>광산별 마지막 일일 적립 일자.</summary>
    public List<int> LastMineGainDay;

    /// <summary>튜토리얼 진행.</summary>
    public TutorialSnapshot Tutorial;

    // 옵션(언어/볼륨/화면)은 OptionsService 가 options.json 에 독립 저장한다(런 save.json 과 무관).
    // 과거 여기 있던 Options 필드는 하드코딩 기본값을 쓰기만 하고 한 번도 읽히지 않는 사장 필드라 제거함.

    /// <summary>
    /// 런타임 런 상태를 저장용 스냅샷으로 변환한다.
    /// </summary>
    public static SaveSnapshot FromGameRunStatet(GameRunState run, SceneId sceneId)
    {
        if (run == null)
        {
            return null;
        }

        return new SaveSnapshot
        {
            SavedAtIso = System.DateTime.Now.ToString("o"),
            IsCompleted = false,
            ErosionFrozen = run.IsClearedRun,
            CurrentDay = run.CurrentDay,
            Location = new SaveLocation { SceneId = sceneId, SubLocationKey = string.Empty },
            FloorMap = FromFloorMap(run.FloorMap),
            Player = FromPlayer(run.Player),
            Roster = FromRoster(run.Roster),
            Erosion = FromErosion(run.Erosion),
            SafeUnlocks = FromSafeUnlocks(run.SafeUnlocks),
            ShopStock = FromShopStock(run.ShopStock),
            Gold = run.Gold,
            ManaStone = run.ManaStone,
            MineActivated = FromBoolArray(run.MineActivated, 3),
            MineStored = FromIntArray(run.MineStored, 3, 0),
            LastMineGainDay = FromIntArray(run.LastMineGainDay, 3, -1),
            Tutorial = new TutorialSnapshot
            {
                CompletedSteps =
                    run.Tutorial != null
                        ? new List<string>(run.Tutorial.CompletedSteps)
                        : new List<string>(),
            },
        };
    }

    /// <summary>
    /// 저장 스냅샷을 런타임 런 상태로 복원한다.
    /// </summary>
    public GameRunState ToGameRunStatet(DataManager data)
    {
        var run = new GameRunState
        {
            CurrentDay = CurrentDay,
            IsClearedRun = ErosionFrozen,
            FloorMap = ToFloorMap(FloorMap),
            Player = ToPlayer(Player, data),
            Roster = ToRoster(Roster),
            Erosion = ToErosion(Erosion, data?.World),
            SafeUnlocks = ToSafeUnlocks(SafeUnlocks),
            ShopStock = ToShopStock(ShopStock),
            Gold = Gold,
            ManaStone = ManaStone,
            MineActivated = ToBoolArray(MineActivated, 3),
            MineStored = ToIntArray(MineStored, 3, 0),
            LastMineGainDay = ToIntArray(LastMineGainDay, 3, -1),
            Tutorial = new TutorialProgressState
            {
                CompletedSteps =
                    Tutorial?.CompletedSteps != null
                        ? new List<string>(Tutorial.CompletedSteps)
                        : new List<string>(),
            },
        };

        run.CurrentFloor = ResolveCurrentFloor(run.FloorMap);
        run.HighestFloor = ResolveHighestClearedFloor(run.FloorMap);
        run.EnsureMineState();
        return run;
    }

    private static List<bool> FromBoolArray(bool[] values, int count)
    {
        var result = new List<bool>();
        for (int i = 0; i < count; i++)
        {
            result.Add(values != null && i < values.Length && values[i]);
        }

        return result;
    }

    private static bool[] ToBoolArray(List<bool> values, int count)
    {
        var result = new bool[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = values != null && i < values.Count && values[i];
        }

        return result;
    }

    private static List<int> FromIntArray(int[] values, int count, int fallback)
    {
        var result = new List<int>();
        for (int i = 0; i < count; i++)
        {
            result.Add(values != null && i < values.Length ? values[i] : fallback);
        }

        return result;
    }

    private static int[] ToIntArray(List<int> values, int count, int fallback)
    {
        var result = new int[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = values != null && i < values.Count ? values[i] : fallback;
        }

        return result;
    }








    private static List<int> CreateEmptyConsumableSlots()
    {
        var result = new List<int>();
        for (int i = 0; i < ConsumableSlots.SlotCount; i++)
        {
            result.Add(0);
        }

        return result;
    }









    private static Item CreateItem(
        int itemId,
        int enhancement,
        int enhanceFailStreak,
        DataManager data
    )
    {
        if (
            itemId == 0
            || data == null
            || !data.Items.TryGetValue(itemId, out ItemData itemData)
        )
        {
            return null;
        }

        return new Item
        {
            Data = itemData,
            Enhancement = enhancement,
            EnhanceFailStreak = enhanceFailStreak,
        };
    }











    private static int ResolveCurrentFloor(FloorMapModel map)
    {
        return map != null ? System.Math.Max(0, map.NextSelectableFloor - 1) : 0;
    }

    private static int ResolveHighestClearedFloor(FloorMapModel map)
    {
        int highest = 0;
        if (map == null)
            return highest;
        foreach (List<FloorNode> nodes in map.NodesByFloor.Values)
        {
            foreach (FloorNode node in nodes)
            {
                if (!node.IsSafeZone && node.IsCleared && node.Floor > highest)
                    highest = node.Floor;
            }
        }

        return highest;
    }
}

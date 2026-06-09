using UnityEngine;

/// <summary>
/// floor ↔ stage / Safe 인덱스 매핑의 단일 권위 함수 집합.
/// WorldData.Stages 에 정의된 범위만 신뢰한다.
/// </summary>
public static class StageIndexResolver
{
    // Guid4 §5.A 2026-05-29
    public static int FromFloor(int floor, WorldData world)
    {
        if (world?.Stages == null)
        {
            GameLog.LogError("[StageIndexResolver] WorldData.Stages 참조가 없습니다.");
            return 1;
        }

        for (int i = 0; i < world.Stages.Count; i++)
        {
            StageDef stage = world.Stages[i];
            if (stage == null)
            {
                continue;
            }

            if (floor >= stage.FloorStart && floor <= stage.FloorEnd)
            {
                return stage.StageIndex;
            }

            if (floor == stage.BossFloor)
            {
                return stage.StageIndex;
            }
        }

        if (world.SafeZones != null)
        {
            for (int i = 0; i < world.SafeZones.Count; i++)
            {
                SafeZoneDef safeZone = world.SafeZones[i];

                if (safeZone != null && floor == safeZone.FloorNumber)
                {
                    return SafeStageForIndex(safeZone.Index, world);
                }
            }
        }

        GameLog.LogError("[StageIndexResolver] floor 가 어떤 Stage 에도 속하지 않습니다: " + floor);
        return 1;
    }

    // Guid4 §5.B 2026-05-29
    public static int FromNode(FloorNode node)
    {
        if (node == null)
        {
            GameLog.LogError("[StageIndexResolver] FloorNode 참조가 없습니다.");
            return 1;
        }

        return Mathf.Max(1, node.StageIndex);
    }

    // Guid4 §5.C 2026-05-29
    public static bool TryGetFloorRange(int stage, WorldData world, out int floorStart, out int floorEnd)
    {
        floorStart = 0;
        floorEnd = 0;

        if (world?.Stages == null)
        {
            GameLog.LogError("[StageIndexResolver] WorldData.Stages 참조가 없습니다.");
            return false;
        }

        for (int i = 0; i < world.Stages.Count; i++)
        {
            StageDef stageDef = world.Stages[i];

            if (stageDef != null && stageDef.StageIndex == stage)
            {
                floorStart = stageDef.FloorStart;
                floorEnd = stageDef.BossFloor > 0 ? stageDef.BossFloor : stageDef.FloorEnd;
                return true;
            }
        }

        GameLog.LogError("[StageIndexResolver] stage 범위를 찾지 못했습니다: " + stage);
        return false;
    }

    // Guid4 §5.D 2026-05-29
    public static int SafeIndexForStage(int stage)
    {
        if (stage < 1)
        {
            return 0;
        }

        if (stage > 5)
        {
            return 5;
        }

        return stage;
    }

    public static int SafeIndexForStage(int stage, WorldData world)
    {
        if (world?.Stages != null)
        {
            for (int i = 0; i < world.Stages.Count; i++)
            {
                StageDef stageDef = world.Stages[i];

                if (stageDef != null && stageDef.StageIndex == stage)
                {
                    return stageDef.UnlocksSafeZoneIndex;
                }
            }
        }

        return SafeIndexForStage(stage);
    }

    private static int SafeStageForIndex(int safeIndex, WorldData world)
    {
        if (safeIndex <= 0)
        {
            return 1;
        }

        int maxStage = ErosionSystem.GetMaxStage(world);

        return Mathf.Clamp(safeIndex, 1, maxStage);
    }
}

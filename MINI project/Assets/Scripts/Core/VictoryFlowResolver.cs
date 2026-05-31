namespace Tempt
{
    public static class VictoryFlowResolver
    {
        public static VictoryFlowDecision Resolve(CombatContext context, GameRunState run, WorldData world, int maxSafeIndex)
        {
            var decision = new VictoryFlowDecision();
            FloorNode node = context?.Node;
            if (node == null || run?.FloorMap == null)
            {
                decision.LoadFloorMap = true;
                return decision;
            }

            int nextSelectableBeforeRechallenge = context.IsRechallenge ? run.FloorMap.NextSelectableFloor : 0;
            run.FloorMap.MarkCleared(node.NodeId);
            if (context.IsRechallenge && run.FloorMap.NextSelectableFloor < nextSelectableBeforeRechallenge)
            {
                run.FloorMap.NextSelectableFloor = nextSelectableBeforeRechallenge;
            }

            if (context.IsBossNode && node.Floor >= 49)
            {
                decision.CompleteRun = true;
                return decision;
            }

            if (context.IsBossNode)
            {
                decision.LoadSafeZone = true;
                decision.SafeIndex = StageIndexResolver.SafeIndexForStage(node.StageIndex, world);
                decision.ShouldActivateErosion = !context.IsRechallenge;
                decision.ShouldResetErosion = context.IsRechallenge;
                decision.StageIndex = node.StageIndex;
                return decision;
            }

            if (context.IsRechallenge)
            {
                decision.LoadSafeZone = true;
                decision.SafeIndex = System.Math.Max(0, System.Math.Min(maxSafeIndex, context.RechallengeReturnSafeIndex));
                return decision;
            }

            decision.LoadFloorMap = true;
            return decision;
        }
    }

    public sealed class VictoryFlowDecision
    {
        public bool CompleteRun;
        public bool LoadSafeZone;
        public bool LoadFloorMap;
        public int SafeIndex;
        public int StageIndex;
        public bool ShouldActivateErosion;
        public bool ShouldResetErosion;
    }
}

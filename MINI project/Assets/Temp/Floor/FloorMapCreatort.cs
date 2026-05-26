namespace Tempt
{
    /// <summary>
    /// 새 게임 시 전체 플로어 맵 1회 생성.
    /// 생성 결과는 GameRunStatet.FloorMap에 보관되며, 저장 시 전체 노드 구조가 JSON에 직접 기록된다.
    /// 이어하기에서는 이 생성기를 다시 호출하지 않는다.
    /// 튜토리얼 단계는 노드 수 고정(1층=1, 2층=2, 3층=1보스).
    /// 4/12/20/30/40층은 안전지대 진입 층(설계 확정).
    /// </summary>
    public static class FloorMapCreatort
    {
        /// <summary>
        /// 맵 생성.
        /// </summary>
        public static FloorMapModelt Generate(WorldDatat world)
        {
            // 동작 요약:
            // - 새 게임 시작 시점의 랜덤 값으로 전체 맵 구조를 1회 생성한다.
            // - 생성에 사용한 랜덤 값은 저장/로드 복원 단위로 사용하지 않는다.
            // - 모든 일반 층(world.Stages 순회):
            //   * 튜토리얼 단계는 TutorialNodeCounts에 따라 노드 수 고정.
            //   * 일반은 [MinNodesPerFloor, MaxNodesPerFloor] 무작위.
            //   * 각 노드의 난이도는 stage 범위 내 무작위.
            //   * 보스 층은 노드 1개, IsBoss=true.
            // - 층 간 연결(NextNodeIds): 다음 층의 노드 인덱스를 적절히 분산.
            // - 4/12/20/30/40층은 노드 없이 안전지대 진입 층으로 표시(맵 모델에 placeholder 노드 또는 빈 floor).
            // - SaveSnapshott 저장 시 이 결과의 모든 FloorNodet 필드를 FloorNodeSnapshott로 직렬화한다.
            // - 결과 FloorMapModelt 반환.
            //TODO: var rng  = new System.Random();
            //TODO: var model = new FloorMapModelt();
            //TODO: int nodeIdCounter = 0;
            //TODO: int[] safeFloors = { 4, 12, 20, 30, 40 };
            //TODO: for (int stageIdx = 0; stageIdx < world.Stages.Count; stageIdx++)
            //TODO: {
            //TODO:     var stage = world.Stages[stageIdx];
            //TODO:     int bossFloor = (stageIdx + 1) * 8; // 8, 16, 24, 32, 40
            //TODO:     for (int floor = stage.StartFloor; floor <= bossFloor; floor++)
            //TODO:     {
            //TODO:         if (System.Array.IndexOf(safeFloors, floor) >= 0) continue; // 안전지대 층 skip
            //TODO:         bool isBoss = floor == bossFloor;
            //TODO:         int nodeCount = isBoss ? 1
            //TODO:             : (stageIdx == 0 ? world.TutorialNodeCounts[floor - 1]
            //TODO:             : rng.Next(world.MinNodesPerFloor, world.MaxNodesPerFloor + 1));
            //TODO:         var floorNodes = new System.Collections.Generic.List<FloorNodet>();
            //TODO:         for (int i = 0; i < nodeCount; i++)
            //TODO:         {
            //TODO:             var node = new FloorNodet
            //TODO:             {
            //TODO:                 NodeId = nodeIdCounter++,
            //TODO:                 Floor = floor,
            //TODO:                 StageIndex = stageIdx + 1,
            //TODO:                 IsBoss = isBoss,
            //TODO:                 MonsterCount = isBoss ? 1 : rng.Next(1, 4),
            //TODO:                 NextNodeIds = new System.Collections.Generic.List<int>(),
            //TODO:                 Difficulty = isBoss ? stage.BossDifficulty
            //TODO:                     : rng.Next(stage.MinDifficulty, stage.MaxDifficulty + 1),
            //TODO:             };
            //TODO:             floorNodes.Add(node);
            //TODO:             model.NodesById[node.NodeId] = node;
            //TODO:         }
            //TODO:         model.NodesByFloor[floor] = floorNodes;
            //TODO:         // 이전 층 노드와 연결
            //TODO:         if (floor > stage.StartFloor && model.NodesByFloor.TryGetValue(floor - 1, out var prevFloor))
            //TODO:         {
            //TODO:             foreach (var prev in prevFloor)
            //TODO:             {
            //TODO:                 int targetIdx = rng.Next(0, floorNodes.Count);
            //TODO:                 if (!prev.NextNodeIds.Contains(floorNodes[targetIdx].NodeId))
            //TODO:                     prev.NextNodeIds.Add(floorNodes[targetIdx].NodeId);
            //TODO:             }
            //TODO:         }
            //TODO:     }
            //TODO: }
            //TODO: model.NextSelectableFloor = world.Stages[0].StartFloor;
            //TODO: return model;
            var model = new FloorMapModelt(); //Wave0write
            if (world == null || world.Stages == null || world.Stages.Count == 0) //Wave0write
            { //Wave0write
                return model; //Wave0write
            } //Wave0write

            int nodeId = 1; //Wave0write
            System.Random rng = new System.Random(); //Wave0write
            System.Collections.Generic.List<FloorNodet> previousFloorNodes = null; //Wave0write

            foreach (StageDeft stage in world.Stages) //Wave0write
            { //Wave0write
                for (int floor = stage.FloorStart; floor <= stage.BossFloor; floor++) //Wave0write
                { //Wave0write
                    bool isBossFloor = floor == stage.BossFloor; //Wave0write
                    int nodeCount = ResolveNodeCount(world, stage, floor, isBossFloor, rng); //Wave0write
                    var floorNodes = new System.Collections.Generic.List<FloorNodet>(); //Wave0write

                    for (int i = 0; i < nodeCount; i++) //Wave0write
                    { //Wave0write
                        var node = new FloorNodet //Wave0write
                        { //Wave0write
                            NodeId = nodeId++, //Wave0write
                            Floor = floor, //Wave0write
                            StageIndex = stage.StageIndex, //Wave0write
                            Difficulty = isBossFloor ? stage.DifficultyMax + 1 : rng.Next(stage.DifficultyMin, stage.DifficultyMax + 1), //Wave0write
                            MonsterCount = isBossFloor ? 1 : rng.Next(world.FloorGen.MonstersMin, world.FloorGen.MonstersMax + 1), //Wave0write
                            IsBoss = isBossFloor, //Wave0write
                            IsCleared = false, //Wave0write
                            NextNodeIds = new System.Collections.Generic.List<int>(), //Wave0write
                        }; //Wave0write
                        floorNodes.Add(node); //Wave0write
                        model.NodesById[node.NodeId] = node; //Wave0write
                    } //Wave0write

                    model.NodesByFloor[floor] = floorNodes; //Wave0write
                    LinkPreviousFloor(previousFloorNodes, floorNodes, rng); //Wave0write
                    previousFloorNodes = floorNodes; //Wave0write
                } //Wave0write
            } //Wave0write

            model.NextSelectableFloor = FindFirstFloor(model); //Wave0write
            return model; //Wave0write
        }

        private static int ResolveNodeCount(WorldDatat world, StageDeft stage, int floor, bool isBossFloor, System.Random rng) //Wave0write
        { //Wave0write
            if (isBossFloor) //Wave0write
            { //Wave0write
                return 1; //Wave0write
            } //Wave0write

            if (stage.StageIndex == 1 && world.FloorGen.TutorialNodeCounts != null) //Wave0write
            { //Wave0write
                int index = floor - stage.FloorStart; //Wave0write
                if (index >= 0 && index < world.FloorGen.TutorialNodeCounts.Count) //Wave0write
                { //Wave0write
                    return System.Math.Max(1, world.FloorGen.TutorialNodeCounts[index]); //Wave0write
                } //Wave0write
            } //Wave0write

            return rng.Next(world.FloorGen.MinNodesPerFloor, world.FloorGen.MaxNodesPerFloor + 1); //Wave0write
        } //Wave0write

        private static void LinkPreviousFloor(System.Collections.Generic.List<FloorNodet> previousFloorNodes, System.Collections.Generic.List<FloorNodet> floorNodes, System.Random rng) //Wave0write
        { //Wave0write
            if (previousFloorNodes == null || floorNodes == null || floorNodes.Count == 0) //Wave0write
            { //Wave0write
                return; //Wave0write
            } //Wave0write

            foreach (FloorNodet previous in previousFloorNodes) //Wave0write
            { //Wave0write
                int targetIndex = rng.Next(0, floorNodes.Count); //Wave0write
                int targetId = floorNodes[targetIndex].NodeId; //Wave0write
                if (!previous.NextNodeIds.Contains(targetId)) //Wave0write
                { //Wave0write
                    previous.NextNodeIds.Add(targetId); //Wave0write
                } //Wave0write
            } //Wave0write
        } //Wave0write

        private static int FindFirstFloor(FloorMapModelt model) //Wave0write
        { //Wave0write
            int first = int.MaxValue; //Wave0write
            foreach (int floor in model.NodesByFloor.Keys) //Wave0write
            { //Wave0write
                if (floor < first) //Wave0write
                { //Wave0write
                    first = floor; //Wave0write
                } //Wave0write
            } //Wave0write

            return first == int.MaxValue ? 0 : first; //Wave0write
        } //Wave0write
    }
}

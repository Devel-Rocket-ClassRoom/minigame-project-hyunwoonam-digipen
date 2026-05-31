namespace Tempt
{
    /// <summary>
    /// 새 게임 시 전체 플로어 맵 1회 생성.
    /// 생성 결과는 GameRunState.FloorMap에 보관되며, 저장 시 전체 노드 구조가 JSON에 직접 기록된다.
    /// 이어하기에서는 이 생성기를 다시 호출하지 않는다.
    /// 튜토리얼 단계는 노드 수 고정(1층=1, 2층=2, 3층=1보스).
    /// 4/12/20/30/40층은 안전지대 진입 층(설계 확정).
    /// </summary>
    public static class FloorMapCreator
    {
        /// <summary>
        /// 맵 생성.
        /// </summary>
        public static FloorMapModel Generate(WorldData world)
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
            // - SaveSnapshot 저장 시 이 결과의 모든 FloorNode 필드를 FloorNodeSnapshott로 직렬화한다.
            // - 결과 FloorMapModel 반환.
            var model = new FloorMapModel();
            if (world == null || world.Stages == null || world.Stages.Count == 0)
            {
                return model;
            }

            int nodeId = 1;
            System.Random rng = new System.Random();
            System.Collections.Generic.List<FloorNode> previousFloorNodes = null;

            foreach (StageDef stage in world.Stages)
            {
                for (int floor = stage.FloorStart; floor <= stage.BossFloor; floor++)
                {
                    bool isBossFloor = floor == stage.BossFloor;
                    int nodeCount = ResolveNodeCount(world, stage, floor, isBossFloor, rng);
                    var floorNodes = new System.Collections.Generic.List<FloorNode>();

                    for (int i = 0; i < nodeCount; i++)
                    {
                        var node = new FloorNode
                        {
                            NodeId = nodeId++,
                            Floor = floor,
                            StageIndex = stage.StageIndex,
                            Difficulty = isBossFloor ? stage.DifficultyMax + 1 : rng.Next(stage.DifficultyMin, stage.DifficultyMax + 1),
                            MonsterCount = ResolveMonsterCount(world, floor, isBossFloor, rng),
                            IsBoss = isBossFloor,
                            IsCleared = false,
                            NextNodeIds = new System.Collections.Generic.List<int>(),
                        };
                        floorNodes.Add(node);
                        model.NodesById[node.NodeId] = node;
                    }

                    model.NodesByFloor[floor] = floorNodes;
                    LinkPreviousFloor(previousFloorNodes, floorNodes, rng);
                    previousFloorNodes = floorNodes;
                }
            }

            EnsureSafeZoneNodes(model, world);
            model.NextSelectableFloor = FindFirstFloor(model);
            return model;
        }

        public static void EnsureSafeZoneNodes(FloorMapModel model, WorldData world)
        {
            if (model == null || world?.Stages == null || world.SafeZones == null)
            {
                return;
            }

            int nextNodeId = FindNextNodeId(model);
            nextNodeId = EnsureStartSafeZoneNode(model, world, nextNodeId);
            for (int stageIndex = 0; stageIndex < world.Stages.Count; stageIndex++)
            {
                StageDef stage = world.Stages[stageIndex];
                SafeZoneDef safeZone = FindSafeZone(world, stage.UnlocksSafeZoneIndex);
                if (safeZone == null || safeZone.FloorNumber <= stage.BossFloor)
                {
                    continue;
                }

                FloorNode safeNode = FindSafeNode(model, safeZone.Index, safeZone.FloorNumber);
                if (safeNode == null)
                {
                    safeNode = new FloorNode
                    {
                        NodeId = nextNodeId++,
                        Floor = safeZone.FloorNumber,
                        StageIndex = safeZone.Index,
                        Difficulty = 0,
                        MonsterCount = 0,
                        IsBoss = false,
                        IsSafeZone = true,
                        IsCleared = false,
                        NextNodeIds = new System.Collections.Generic.List<int>(),
                    };
                    AddNode(model, safeNode);
                }

                LinkBossToSafe(model, stage.BossFloor, safeNode);
                LinkSafeToNextStage(model, world, stageIndex, safeNode);
            }
        }

        private static int EnsureStartSafeZoneNode(FloorMapModel model, WorldData world, int nextNodeId)
        {
            SafeZoneDef safeZone = FindSafeZone(world, 0);
            int floor = safeZone != null ? safeZone.FloorNumber : 0;
            FloorNode safeNode = FindSafeNode(model, 0, floor);
            if (safeNode == null)
            {
                safeNode = new FloorNode
                {
                    NodeId = nextNodeId++,
                    Floor = floor,
                    StageIndex = 0,
                    Difficulty = 0,
                    MonsterCount = 0,
                    IsBoss = false,
                    IsSafeZone = true,
                    IsCleared = false,
                    NextNodeIds = new System.Collections.Generic.List<int>(),
                };
                AddNode(model, safeNode);
            }

            if (world.Stages.Count > 0)
            {
                LinkSafeToFloor(model, safeNode, world.Stages[0].FloorStart);
            }

            return nextNodeId;
        }

        private static int ResolveNodeCount(WorldData world, StageDef stage, int floor, bool isBossFloor, System.Random rng)
        {
            if (isBossFloor)
            {
                return 1;
            }

            if (stage.StageIndex == 1 && world.FloorGen.TutorialNodeCounts != null)
            {
                int index = floor - stage.FloorStart;
                if (index >= 0 && index < world.FloorGen.TutorialNodeCounts.Count)
                {
                    return System.Math.Max(1, world.FloorGen.TutorialNodeCounts[index]);
                }
            }

            return rng.Next(world.FloorGen.MinNodesPerFloor, world.FloorGen.MaxNodesPerFloor + 1);
        }

        private static int ResolveMonsterCount(WorldData world, int floor, bool isBossFloor, System.Random rng)
        {
            if (isBossFloor || floor == 1)
            {
                return 1;
            }

            if (floor == 2)
            {
                return 2;
            }

            return rng.Next(world.FloorGen.MonstersMin, world.FloorGen.MonstersMax + 1);
        }

        private static void LinkPreviousFloor(System.Collections.Generic.List<FloorNode> previousFloorNodes, System.Collections.Generic.List<FloorNode> floorNodes, System.Random rng)
        {
            if (previousFloorNodes == null || floorNodes == null || floorNodes.Count == 0)
            {
                return;
            }

            foreach (FloorNode previous in previousFloorNodes)
            {
                int targetIndex = rng.Next(0, floorNodes.Count);
                int targetId = floorNodes[targetIndex].NodeId;
                if (!previous.NextNodeIds.Contains(targetId))
                {
                    previous.NextNodeIds.Add(targetId);
                }
            }
        }

        private static int FindNextNodeId(FloorMapModel model)
        {
            int next = 1;
            foreach (int nodeId in model.NodesById.Keys)
            {
                if (nodeId >= next)
                {
                    next = nodeId + 1;
                }
            }

            return next;
        }

        private static SafeZoneDef FindSafeZone(WorldData world, int safeZoneIndex)
        {
            foreach (SafeZoneDef safeZone in world.SafeZones)
            {
                if (safeZone != null && safeZone.Index == safeZoneIndex)
                {
                    return safeZone;
                }
            }

            return null;
        }

        private static FloorNode FindSafeNode(FloorMapModel model, int safeZoneIndex, int floor)
        {
            if (!model.NodesByFloor.TryGetValue(floor, out System.Collections.Generic.List<FloorNode> nodes))
            {
                return null;
            }

            foreach (FloorNode node in nodes)
            {
                if (node != null && node.IsSafeZone && node.StageIndex == safeZoneIndex)
                {
                    return node;
                }
            }

            return null;
        }

        private static void AddNode(FloorMapModel model, FloorNode node)
        {
            model.NodesById[node.NodeId] = node;
            if (!model.NodesByFloor.TryGetValue(node.Floor, out System.Collections.Generic.List<FloorNode> nodes))
            {
                nodes = new System.Collections.Generic.List<FloorNode>();
                model.NodesByFloor[node.Floor] = nodes;
            }

            nodes.Add(node);
        }

        private static void LinkBossToSafe(FloorMapModel model, int bossFloor, FloorNode safeNode)
        {
            if (!model.NodesByFloor.TryGetValue(bossFloor, out System.Collections.Generic.List<FloorNode> bossNodes))
            {
                return;
            }

            foreach (FloorNode bossNode in bossNodes)
            {
                if (bossNode == null || !bossNode.IsBoss || bossNode.NextNodeIds == null)
                {
                    continue;
                }

                bossNode.NextNodeIds.RemoveAll(id => model.NodesById.TryGetValue(id, out FloorNode next) && next != null && !next.IsSafeZone && next.Floor > bossFloor);
                if (!bossNode.NextNodeIds.Contains(safeNode.NodeId))
                {
                    bossNode.NextNodeIds.Add(safeNode.NodeId);
                }
            }
        }

        private static void LinkSafeToNextStage(FloorMapModel model, WorldData world, int stageIndex, FloorNode safeNode)
        {
            if (safeNode.NextNodeIds == null)
            {
                safeNode.NextNodeIds = new System.Collections.Generic.List<int>();
            }

            if (stageIndex + 1 >= world.Stages.Count)
            {
                return;
            }

            StageDef nextStage = world.Stages[stageIndex + 1];
            if (!model.NodesByFloor.TryGetValue(nextStage.FloorStart, out System.Collections.Generic.List<FloorNode> nextNodes))
            {
                return;
            }

            foreach (FloorNode nextNode in nextNodes)
            {
                if (nextNode != null && !safeNode.NextNodeIds.Contains(nextNode.NodeId))
                {
                    safeNode.NextNodeIds.Add(nextNode.NodeId);
                }
            }
        }

        private static void LinkSafeToFloor(FloorMapModel model, FloorNode safeNode, int floor)
        {
            if (safeNode.NextNodeIds == null)
            {
                safeNode.NextNodeIds = new System.Collections.Generic.List<int>();
            }

            if (!model.NodesByFloor.TryGetValue(floor, out System.Collections.Generic.List<FloorNode> nextNodes))
            {
                return;
            }

            foreach (FloorNode nextNode in nextNodes)
            {
                if (nextNode != null && !nextNode.IsSafeZone && !safeNode.NextNodeIds.Contains(nextNode.NodeId))
                {
                    safeNode.NextNodeIds.Add(nextNode.NodeId);
                }
            }
        }

        private static int FindFirstFloor(FloorMapModel model)
        {
            int first = int.MaxValue;
            foreach (System.Collections.Generic.KeyValuePair<int, System.Collections.Generic.List<FloorNode>> entry in model.NodesByFloor)
            {
                if (entry.Value == null)
                {
                    continue;
                }

                bool hasCombatNode = entry.Value.Exists(node => node != null && !node.IsSafeZone);
                if (hasCombatNode && entry.Key < first)
                {
                    first = entry.Key;
                }
            }

            return first == int.MaxValue ? 0 : first;
        }
    }
}


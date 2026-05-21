# FloorMap Progression Design

Date: 2026-05-21

## Purpose

Implement the Week 1 FloorMap progression loop so that the player starts in Safe0, enters FloorMap through the Safe0 Map button, sees the full generated map, advances floor by floor, and unlocks previous-floor rechallenge only after clearing the temporary boss and reaching Safe0.

## Confirmed Play Mode Flow

```text
1. New Game
2. Safe0
3. Click Safe0 Map button to enter FloorMap
4. FloorMap shows the full map
5. Only floor 1 nodes are clickable
6. After floor 1 Victory, return to FloorMap; only floor 2 nodes are clickable
7. After floor 2 Victory, return to FloorMap; only floor 3 boss is clickable
8. After floor 3 boss Victory, move to Safe0
9. After Safe0 arrival, previous floors are unlocked for rechallenge
```

## Scope

This design covers the Week 1 temporary 1F-3F flow.

In scope:

- Full demo map generation at run start.
- FloorMap rendering of all generated floors and nodes.
- Scrollable FloorMap UI when content exceeds panel bounds.
- Normal floors with 1-3 random nodes.
- Fixed single boss node on floor 3.
- Runtime click-lock rules.
- Safe0 Map button entry into FloorMap.
- Temporary Monster1 multi-spawn based on node difficulty.

Out of scope:

- CSV/JSON monster pool loading.
- Real Safe6/Safe12 scenes.
- Full 18-floor content generation.
- Permanent save serialization of generated map state.
- Final boss or stage-specific monster pools.

## Data Model

`FloorNodeData` should represent both static node data and mutable run state needed for the Week 1 loop.

Fields:

```text
NodeIndex       unique node id across the generated map
Floor           floor number
NodeSlot        index within the floor row
DisplayName     label for UI/logs
IsBossNode      true for fixed boss node
IsSafeNode      true for future safe nodes; false in current 1F-3F demo
Difficulty      temporary int difficulty, 1-3
MonsterCount    temporary monster count derived from difficulty
IsCleared       mutable clear state
```

`IsUnlocked` should not be stored as fixed node data. Node clickability depends on current run state and should be computed by `GameSystemManager.CanSelectFloorNode(node)`.

## FloorNodeCreator

`FloorNodeCreator` becomes the owner of generated map data.

Responsibilities:

- Generate the full demo map once for a new run.
- Keep the generated node list stable during the run.
- Provide `Nodes` and `TryGetNode(nodeIndex, out node)`.
- Provide a clear-state update API, such as `MarkNodeCleared(nodeIndex)`.

Week 1 generation rules:

```text
Floor 1: normal floor, random 1-3 nodes
Floor 2: normal floor, random 1-3 nodes
Floor 3: boss floor, fixed 1 node
```

Temporary difficulty and monster count:

```text
Difficulty 1 -> MonsterCount 1
Difficulty 2 -> MonsterCount 2
Difficulty 3 -> MonsterCount 3
Boss node    -> Difficulty 3, MonsterCount 1
```

## GameSystemManager

`GameSystemManager` owns run progression and clickability policy.

New or clarified state:

```text
currentFloor
currentDungeonNodeIndex
nextSelectableFloor
maxRechallengeFloor
currentSafeZoneFloor
selectedCombatNode
pendingSafeArrivalRechallengeUnlock
```

StartNewGame:

```text
Initialize systems
Set run active
Clear player combat state
Generate full demo map once
currentFloor = 0
currentDungeonNodeIndex = -1
nextSelectableFloor = 1
maxRechallengeFloor = 0
currentSafeZoneFloor = 0
pendingSafeArrivalRechallengeUnlock = false
Load Safe0
```

EnterFloorMap:

```text
Initialize systems
Ensure run is active
Generate map only if no nodes exist
Load FloorMap
```

CanSelectFloorNode:

```text
Return false if node is null.
Return false if node.Floor == currentFloor.
Return true if node.Floor == nextSelectableFloor and node is not cleared.
Return true if node.IsCleared and node.Floor <= maxRechallengeFloor.
Otherwise return false.
```

StartCombatNode:

```text
Find selected node.
Reject if CanSelectFloorNode(node) is false.
Store currentDungeonNodeIndex and selectedCombatNode.
Set currentFloor = node.Floor.
Load Combat.
```

EndCombat:

```text
Victory:
  Mark selected node cleared.
  Record current node/floor state.
  If selected node is boss:
    pendingSafeArrivalRechallengeUnlock = true
    Load Safe0
  Else:
    nextSelectableFloor = selected node floor + 1
    Load FloorMap

Defeat:
  End run
  Load MainMenu

Retreat:
  Load Safe0
```

Safe arrival handling:

```text
When Safe0 loads after boss Victory in Week 1:
  If pendingSafeArrivalRechallengeUnlock is true:
    maxRechallengeFloor = max(maxRechallengeFloor, selected boss floor)
    pendingSafeArrivalRechallengeUnlock = false
```

For Week 1, Safe0 is reused as the next Safe after the temporary floor 3 boss. When Safe6/Safe12 exist, this exception should be replaced with real safe-zone arrival.

## SafeZoneController

Create `Assets/Scripts/Safe/SafeZoneController.cs`.

Responsibilities:

- Auto-inject on Safe0 scene load.
- Find the existing Safe0 `Map` button.
- Attach `GameSystemManager.EnterFloorMap()` to its `onClick`.
- Notify `GameSystemManager` that Safe0 has been entered so pending rechallenge unlock can be applied.

The controller should be defensive:

- If `GameSystemManager` is missing, log a warning.
- If the `Map` button is missing, log a warning.
- Avoid registering duplicate listeners on repeated scene loads.

## FloorMapController

`FloorMapController` should render the full generated map.

UI structure:

```text
Canvas
  Header
  ScrollRect
    Viewport
      Content
        FloorRow 1
          Node buttons
        FloorRow 2
          Node buttons
        FloorRow 3
          Node buttons
```

Button behavior:

- Button size should be at least `160x72`.
- Normal, boss, safe, locked, and cleared states should be visually distinguishable.
- `button.interactable` should come from `GameSystemManager.CanSelectFloorNode(node)`.
- Labels should include floor, node slot, difficulty, and monster count.

The first implementation can keep runtime UI creation. Later scene-authored UI can replace it.

## CombatMonsterSpawner

Create `Assets/Scripts/Combat/CombatMonsterSpawner.cs`.

Responsibilities:

- Run in `Awake()` before `CombatFlow.Start()`.
- Read the selected `FloorNodeData` from `GameSystemManager`.
- Use the existing scene `Monster1` object as the first monster/template.
- Clone additional `Monster1` objects until the scene has `MonsterCount` monsters.
- Position spawned monsters with simple offsets so each is clickable.

Temporary spawn layout:

```text
1 monster : original position
2 monsters: original y +/- 0.8
3 monsters: original y + 0.9, original, original y - 0.9
```

If no selected node exists, leave the existing scene monster unchanged for standalone Combat testing.

`CombatFlow` should continue to discover monsters through `FindObjectsByType<MonsterBase>()`.

## Tests

Update `FloorNodeFlowTests.cs`.

Recommended tests:

```text
GenerateDemoFloorMapCreatesNormalFloorsWithOneToThreeNodesAndSingleBossNode
GeneratedNodesHaveDifficultyAndMonsterCount
CanSelectOnlyNextFloorDuringProgression
BossClearDoesNotUnlockRechallengeUntilSafeArrival
FloorNodeSceneIdLoadsFloorMapScene
```

Unity Play Mode manual validation should follow the confirmed flow exactly.

## Risks

- The current Safe0 button is scene-authored; runtime listener attachment must avoid duplicate listeners.
- The existing Combat scene has a manually placed Monster object, so dynamic spawns must avoid duplicate count errors.
- Random generation can make tests flaky unless tests validate ranges or use a deterministic seed.
- `Assets/Tests` may be ignored by git but still compiled by Unity, so test files must stay compilable.

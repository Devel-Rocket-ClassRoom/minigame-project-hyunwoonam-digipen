# FloorMap Progression Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the Week 1 Safe0 -> FloorMap -> Combat progression loop with full-map rendering, node locking, and temporary Monster1 multi-spawn.

**Architecture:** `GameSystemManager` owns run state and node clickability. `FloorNodeCreator` owns generated map data. `FloorMapController`, `SafeZoneController`, and `CombatMonsterSpawner` adapt scene UI/combat setup to that state without moving combat FSM logic out of `CombatFlow`.

**Tech Stack:** Unity C#, NUnit EditMode tests, runtime-generated Unity UI.

---

### Task 1: Floor Node Model And Generation

**Files:**
- Modify: `MINI project/Assets/Tests/EditMode/FloorNodeFlowTests.cs`
- Modify: `MINI project/Assets/Scripts/Core/FloorNodeCreator.cs`

- [ ] **Step 1: Write failing tests**

Add tests that require demo map generation to create 1F/2F random node counts, a single 3F boss node, difficulty, monster count, and mutable clear state.

- [ ] **Step 2: Run build and verify RED**

Run: `dotnet build "MINI project/MINI project.slnx"`
Expected: FAIL because `Difficulty`, `MonsterCount`, `NodeSlot`, and clear-state APIs do not exist yet.

- [ ] **Step 3: Implement FloorNodeData and FloorNodeCreator**

Add map generation, clear-state APIs, and a stable generated node list.

- [ ] **Step 4: Run build and verify GREEN**

Run: `dotnet build "MINI project/MINI project.slnx"`
Expected: PASS.

### Task 2: GameSystemManager Progression Policy

**Files:**
- Modify: `MINI project/Assets/Tests/EditMode/FloorNodeFlowTests.cs`
- Modify: `MINI project/Assets/Scripts/Core/GameSystemManager.cs`

- [ ] **Step 1: Write failing tests**

Add tests for `CanSelectFloorNode`, normal Victory progression, boss Victory pending Safe unlock, and Safe0 arrival rechallenge unlock.

- [ ] **Step 2: Run build and verify RED**

Run: `dotnet build "MINI project/MINI project.slnx"`
Expected: FAIL because `CanSelectFloorNode`, `NotifySafeZoneEntered`, and selected-node state APIs are missing.

- [ ] **Step 3: Implement progression state**

Add `nextSelectableFloor`, `maxRechallengeFloor`, selected node state, clickability logic, normal Victory -> FloorMap, boss Victory -> Safe0, and Safe0 arrival unlock.

- [ ] **Step 4: Run build and verify GREEN**

Run: `dotnet build "MINI project/MINI project.slnx"`
Expected: PASS.

### Task 3: Runtime Safe0 Entry And FloorMap UI

**Files:**
- Create: `MINI project/Assets/Scripts/Safe/SafeZoneController.cs`
- Create: `MINI project/Assets/Scripts/Safe/SafeZoneController.cs.meta`
- Modify: `MINI project/Assets/Scripts/Floor/FloorMapController.cs`

- [ ] **Step 1: Implement SafeZoneController**

Auto-inject on Safe0 load, notify `GameSystemManager` of Safe arrival, and bind the `Map` button to `EnterFloorMap`.

- [ ] **Step 2: Implement scrollable full-map FloorMap UI**

Render rows grouped by floor in a `ScrollRect`; set node button interactability via `GameSystemManager.CanSelectFloorNode`.

- [ ] **Step 3: Run build**

Run: `dotnet build "MINI project/MINI project.slnx"`
Expected: PASS.

### Task 4: Temporary Monster Spawning

**Files:**
- Create: `MINI project/Assets/Scripts/Combat/CombatMonsterSpawner.cs`
- Create: `MINI project/Assets/Scripts/Combat/CombatMonsterSpawner.cs.meta`

- [ ] **Step 1: Implement CombatMonsterSpawner**

In `Awake`, read the selected node from `GameSystemManager`, use the scene `Monster1` as the first monster, clone up to `MonsterCount`, and place monsters with simple offsets.

- [ ] **Step 2: Run build**

Run: `dotnet build "MINI project/MINI project.slnx"`
Expected: PASS.

### Task 5: Documentation And Final Verification

**Files:**
- Modify: `MINI project/Assets/Imported/Docs/HANDOFF6.md`

- [ ] **Step 1: Record implementation summary**

Append changed files, behavior, and verification results to `HANDOFF6.md`.

- [ ] **Step 2: Run final build**

Run: `dotnet build "MINI project/MINI project.slnx"`
Expected: PASS with 0 errors.

- [ ] **Step 3: Report manual Play Mode checklist**

Report that Unity Play Mode must verify:

```text
1. New Game
2. Safe0
3. Click Safe0 Map button to enter FloorMap
4. Full FloorMap visible
5. Only floor 1 nodes clickable
6. Floor 1 Victory -> FloorMap, only floor 2 clickable
7. Floor 2 Victory -> FloorMap, only floor 3 boss clickable
8. Floor 3 boss Victory -> Safe0
9. Safe0 arrival unlocks previous-floor rechallenge
```

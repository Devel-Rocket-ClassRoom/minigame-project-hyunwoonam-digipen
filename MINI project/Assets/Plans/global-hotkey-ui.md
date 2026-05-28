# Project Overview
- Game Title: Dark Fantasy Turn-Based Roguelike RPG
- High-Level Concept: A global overlay UI system for navigating character stats, inventory, and skills using hotkeys (I, S, R, K, ESC) during non-combat gameplay.
- Players: Single-player
- Inspiration / Reference Games: Darkest Dungeon, Diablo (dark fantasy UI)
- Tone / Art Direction: Dark fantasy, matte charcoal, sharp edges, gold highlights, dark red accents.
- Target Platform: PC (StandaloneWindows64)
- Screen Orientation / Resolution: Landscape 1920x1080
- Render Pipeline: URP

# Game Mechanics
## Core Gameplay Loop
Players engage in exploration and turn-based combat. Between encounters or in safe zones, they manage resources, upgrade skills, and customize their "Rune Tree" via the global UI overlay.
## Controls and Input Methods
- **I**: Open/Close Inventory
- **S**: Open/Close Status
- **R**: Open/Close Rune Tree
- **K**: Open/Close Skills
- **ESC**: Open Exit Confirmation (or close current panel)
- **Mouse**: Interaction with UI elements (buttons, slots, nodes)

# UI
## Hierarchy Structure
- **Canvas_GlobalOverlay** (Reference: 1920x1080, Scale With Screen Size)
    - **DimOverlay**: Full-screen black Image (45-60% alpha), blocks raycasts when panels are open.
    - **TopHUD**: Minimalist bar at the top (SAFE ZONE, G, HP/MP).
    - **GlobalPanelRoot**: Container for all panels.
        - **InventoryPanel**: 3-column layout (Quick Items, Grid, Detail).
        - **StatusPanel**: 2-column layout (Character Card, Stats/Resources).
        - **RuneTreePanel**: 2-column layout (Large node graph, Node Detail).
        - **SkillsPanel**: 2-column layout (Active Skill List, Passive Effects/Detail).
        - **ExitGamePanel**: Compact centered confirmation box.
    - **HotkeyPreviewBar**: Bottom-anchored bar showing keyboard shortcuts.

## Visual Language
- **Background**: Matte Charcoal (#1A1A1A) at ~88% opacity.
- **Borders**: 1px Solid Gray (#4A4A4A).
- **Headers**: White Uppercase, wide spacing.
- **Highlights**: Muted Gold (#C5A059).
- **Primary Action**: Dark Red (#7E1A1A).
- **Secondary Text**: Light Gray (#A0A0A0).
- **Shapes**: Sharp rectangles, no rounded corners.

# Key Asset & Context
- **GlobalHotkeyUIController.cs**: Manages panel visibility, input polling, and event blocking.
- **UIPanel.cs**: Base component for each panel to handle Open/Close animations (CanvasGroup fade).
- **RuneNode.cs**: Component for the Rune Tree nodes to handle states (Locked, Unlocked, Selected).

# Implementation Steps
## Phase 1: Infrastructure & Core Controller
1. **Create GlobalHotkeyUIController.cs**: Implement singleton-like access and input handling for I, S, R, K, ESC.
2. **Setup Canvas in Boot Scene**: Configure `Canvas_GlobalOverlay` with `CanvasScaler` (1920x1080, Match 0.5) and `GraphicRaycaster`.
3. **Implement DimOverlay**: Create a full-screen image with `CanvasGroup` to toggle visibility and block world interaction.

## Phase 2: Common UI Elements
1. **Create Panel Prefab Base**: A standard frame with Header (Title, Subtitle, Close Button) and 1px border.
2. **Setup TopHUD**: Implement the fixed top bar with non-clickable status text.
3. **Setup HotkeyPreviewBar**: Implement the bottom bar with five buttons (I, S, R, K, ESC) and highlight states.

## Phase 3: Specific Panels
1. **Inventory Panel (I)**:
    - Left: Quick Item slots (2x2 grid).
    - Center: Inventory grid (4x6).
    - Right: Item Detail card with USE/DISCARD buttons.
2. **Status Panel (S)**:
    - Left: Character card with portrait placeholder and EXP bar.
    - Right: 5 Stat cards and combat summary rows.
3. **Rune Tree Panel (R)**:
    - Left: Large Graph area with circular nodes and occult background diagram.
    - Right: Node detail panel with UNLOCK/RESET buttons.
4. **Skills Panel (K)**:
    - Left: Scrollable list of Active Skills.
    - Right: Passive effects list and Selected Skill detail.
5. **Exit Game Panel (ESC)**:
    - Compact modal with YES/NO confirmation buttons.

## Phase 4: Integration & Interaction
1. **Input System Binding**: Hook keyboard inputs to `GlobalHotkeyUIController` methods.
2. **World Interaction Blocking**: Ensure `DimOverlay` blocks raycasts to the underlying scene when any panel is active.
3. **Responsive Testing**: Verify anchors and percentages at 1366x768, 1600x900, 1920x1080, and 2560x1440.

# Verification & Testing
- **Input Test**: Press each hotkey to ensure the correct panel opens and others close.
- **Close Action**: Click the 'X' button or press ESC to ensure panels close.
- **Layout Test**: Change game window resolution to verify anchors and 4% safe margins.
- **Blocking Test**: Ensure clicking through the UI does not trigger any hypothetical background objects.
- **Visual Audit**: Confirm colors, borders, and font styles match the "Dark Fantasy" specification.

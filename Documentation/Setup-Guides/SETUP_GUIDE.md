# Squad Control System Setup Guide

## Prerequisites
- Unreal Engine 5.5
- Enhanced Input Plugin enabled
- C++ project setup

## Step 1: Compile the C++ Classes

### 1.1 Build the Project
```bash
# From your project root directory
cd "C:\Users\ricma\Documents\Unreal Projects\ProjectHive 5.5"

# Generate project files
"C:\Program Files\Epic Games\UE_5.5\Engine\Binaries\DotNET\UnrealBuildTool\UnrealBuildTool.exe" -projectfiles -project="ProjectHive.uproject" -game -rocket -progress

# Build the project
"C:\Program Files\Epic Games\UE_5.5\Engine\Build\BatchFiles\Build.bat" ProjectHiveEditor Win64 Development "ProjectHive.uproject" -waitmutex
```

### 1.2 Complete Missing Implementation Files
We need to create the missing `.cpp` implementation files:

1. **SquadManager.cpp** (already created)
2. **GameStateManager.cpp** (already created)  
3. **SquadPlayerController.cpp** (needs to be created)

## Step 2: Create Enhanced Input Assets

### 2.1 Create Input Actions
1. In Content Browser, create folder: `Content/Input/Actions/`
2. Right-click → Input → Input Action
3. Create these Input Actions:

**Tactical Controls:**
- `IA_TacticalPause` (Input Action)
- `IA_SlowMotion` (Input Action)
- `IA_FormationCycle` (Input Action)

**Squad Commands:**
- `IA_RightClick` (Input Action)
- `IA_LeftClick` (Input Action)
- `IA_HoldPosition` (Input Action)
- `IA_TakeCover` (Input Action)

**Camera Controls:**
- `IA_MoveForward` (Input Action, Value Type: Axis1D)
- `IA_MoveRight` (Input Action, Value Type: Axis1D)
- `IA_Zoom` (Input Action, Value Type: Axis1D)
- `IA_CameraRotate` (Input Action, Value Type: Axis1D)
- `IA_MiddleMouse` (Input Action)
- `IA_MouseMove` (Input Action, Value Type: Axis2D)

### 2.2 Create Input Mapping Context
1. Create folder: `Content/Input/`
2. Right-click → Input → Input Mapping Context
3. Name it: `IMC_SquadControl`
4. Add mappings:

| Action | Key/Input | Modifiers |
|--------|-----------|-----------|
| IA_TacticalPause | Space | None |
| IA_SlowMotion | Left Shift | None |
| IA_FormationCycle | F | None |
| IA_RightClick | Right Mouse Button | None |
| IA_LeftClick | Left Mouse Button | None |
| IA_HoldPosition | H | None |
| IA_TakeCover | C | None |
| IA_MoveForward | W (1.0), S (-1.0) | None |
| IA_MoveRight | D (1.0), A (-1.0) | None |
| IA_Zoom | Mouse Wheel Axis | None |
| IA_CameraRotate | Q (1.0), E (-1.0) | None |
| IA_MiddleMouse | Middle Mouse Button | None |
| IA_MouseMove | Mouse XY 2D-Axis | None |

## Step 3: Create Blueprint Classes

### 3.1 Create Squad Player Controller Blueprint
1. Content Browser → Create folder: `Content/Blueprints/Controllers/`
2. Right-click → Blueprint Class
3. Search for "Squad Player Controller" (our C++ class)
4. Name it: `BP_SquadPlayerController`
5. Open the Blueprint and set these properties:
   - **Input Mapping Context**: Set to `IMC_SquadControl`
   - **Tactical Pause Action**: Set to `IA_TacticalPause`
   - **Slow Motion Action**: Set to `IA_SlowMotion`
   - **Formation Cycle Action**: Set to `IA_FormationCycle`
   - **Right Click Action**: Set to `IA_RightClick`
   - (Continue for all input actions...)

### 3.2 Create Game Mode Blueprint
1. Content Browser → `Content/Blueprints/GameModes/`
2. Right-click → Blueprint Class → Game Mode Base
3. Name it: `BP_SquadGameMode`
4. Set **Default Player Controller Class** to `BP_SquadPlayerController`

## Step 4: Set Up the Level

### 4.1 Configure World Settings
1. In your level, go to **World Settings**
2. Set **Game Mode Override** to `BP_SquadGameMode`
3. This ensures your squad controller is used

### 4.2 Create Squad Units
1. Place 4-5 units in your level (using your existing SelectableUnit actors)
2. Make sure they have the **Player faction** set
3. Position them in a starting formation

### 4.3 Initialize Squad in Level Blueprint
1. Open **Level Blueprint** (or create a custom GameState)
2. Add this initialization logic:

```cpp
// In Level Blueprint or Game State
Event BeginPlay
  → Get Player Controller (cast to Squad Player Controller)
  → Get All Actors of Class (SelectableUnit)
  → Filter for Player Faction
  → Call "Initialize Squad Controller" with the units array
```

## Step 5: Testing Setup

### 5.1 Basic Functionality Test
1. **Play in Editor**
2. **Test Tactical Pause**: Press `SPACE` - time should freeze
3. **Test Formation Cycling**: Press `F` - units should change formation
4. **Test Movement**: Right-click on ground - squad should move in formation
5. **Test Slow Motion**: Press `SHIFT` - time should slow down

### 5.2 Debug Information
Add these to your HUD for debugging:
- Current Game State
- Current Tactical State
- Squad Size
- Current Formation
- Time Scale

## Step 6: Optional Enhancements

### 6.1 Visual Feedback
1. Create materials for formation indicators
2. Add Niagara effects for cursor feedback
3. Create UI widgets for squad status

### 6.2 Sound Integration
1. Add sound cues for tactical state changes
2. Squad callouts for commands
3. Formation change audio feedback

## Troubleshooting

### Common Issues:

**1. C++ Classes Not Showing Up**
- Make sure project compiles successfully
- Refresh the Content Browser
- Restart Unreal Editor

**2. Input Not Working**
- Verify Input Mapping Context is set correctly
- Check that Enhanced Input plugin is enabled
- Ensure Input Actions are properly configured

**3. Squad Not Moving**
- Verify UnitMovementComponent exists on units
- Check that units have Player faction
- Ensure GameStateManager is properly initialized

**4. Tactical Pause Not Working**
- Check GameStateManager singleton is created
- Verify time scale is being applied to world
- Make sure delegates are properly bound

### Debug Commands (Console):
```
showdebug ai          // Show AI debugging
stat fps              // Show FPS and performance
showdebug input       // Show input debugging
showdebug collision   // Show collision debugging
```

## Next Steps After Basic Setup

1. **Test all functionality** systematically
2. **Tune formation spacing** and behavior parameters
3. **Add visual polish** (effects, UI, indicators)
4. **Implement cover detection** improvements
5. **Add sound and animation integration**
6. **Create additional formations** if needed
7. **Performance optimization** and testing

This setup should give you a fully functional squad-based control system with tactical pause and formation management!

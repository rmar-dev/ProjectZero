# Cover System Setup Guide

## Prerequisites
Make sure your project has been built successfully with all the cover system files.

## Step 1: Create a CoverSystemManager in Your Level

1. **In the Unreal Editor:**
   - Open your level
   - In the Place Actors panel, search for "CoverSystemManager"
   - Drag one into your level (anywhere is fine, it doesn't need a specific location)
   - This manager will automatically register all cover points in the world

## Step 2: Place Cover Points in Your Level

1. **Add Cover Points:**
   - Search for "CoverPoint" in the Place Actors panel
   - Place several ACoverPoint actors around your level where you want units to take cover
   - These could be behind walls, crates, pillars, etc.
   
2. **Configure Cover Points (Optional):**
   - Select a cover point
   - In the Details panel, you can adjust:
     - `MaxOccupants`: How many units can use this cover point (default: 1)
     - `CoverType`: Full, Half, or Minimal cover
     - `CoverDirection`: Which direction this cover protects from

## Step 3: Add SelectableUnits to Your Level

1. **Create Units:**
   - Since SelectableUnit is abstract, you'll need a Blueprint child class
   - Create a Blueprint based on SelectableUnit
   - Add a visible mesh component so you can see the unit
   - Place several instances in your level

2. **Configure Unit Cover Settings:**
   - Select a unit
   - In the Details panel, under "Cover AI":
     - Ensure `bUseCoverAI = true`
     - Set `CoverSearchRadius = 300.0` (or larger for testing)
     - Set `CoverEvaluationThreshold = 0.5`

## Step 4: Test the System

### Method 1: Use the Test Function
1. Select a unit in the world
2. In the Details panel, find the "Cover AI Debug" section
3. Click "Test Seek Cover"
4. Check the Output Log for debug messages

### Method 2: Simulate Combat
1. Create another actor to act as an "enemy"
2. Call `TakeDamage()` on a unit with the enemy as the damage causer
3. The unit should automatically seek cover

## Expected Behavior

When working correctly, you should see:

1. **Console Output:**
   ```
   [Unit]: TestSeekCover called with threat direction: (0,0,0)
   [Unit]: Cover system service found, searching for cover...
   [Unit]: Found 3 available cover points within 300.0 units
   [Unit]: MovementComponent moving to (X=100, Y=200, Z=0)
   [Unit]: Successfully initiated cover seeking to (X=100, Y=200, Z=0)
   [Unit]: Distance to target: 150.2 units
   [Unit]: Reached cover with 75.0% damage reduction
   ```

2. **Visual Behavior:**
   - Unit changes state to "TakingCover"
   - Unit rotates toward the cover point
   - Unit moves smoothly to the cover location
   - Unit state changes to "InCover" when it arrives

## Troubleshooting

### "Cover system service not available!"
- **Problem**: No CoverSystemManager in the world
- **Solution**: Add a CoverSystemManager actor to your level

### "No cover points found!"
- **Problem**: No ACoverPoint actors in range
- **Solution**: Place ACoverPoint actors within the unit's CoverSearchRadius

### "Failed to reserve cover point!"
- **Problem**: Cover point is already occupied
- **Solution**: Add more cover points or increase MaxOccupants on existing ones

### Unit doesn't move
- **Problem**: MovementComponent not working
- **Solution**: Check that the unit has a UnitMovementComponent and it's properly configured

### Unit moves but doesn't reach "InCover" state
- **Problem**: Unit stops moving before reaching the exact cover location
- **Solution**: The tolerance is 100 units (1 meter). Make sure there are no obstacles blocking the path.

## Advanced Usage

### Triggering Cover from Blueprint
```cpp
// In Blueprint or C++
if (MyUnit && MyUnit->GetClass()->ImplementsInterface(USelectableInterface::StaticClass()))
{
    MyUnit->TestSeekCover(FVector(1, 0, 0)); // Seek cover from threat coming from X direction
}
```

### Checking Cover Status
```cpp
// Check if unit is in cover
bool bInCover = MyUnit->IsInCover();

// Get cover effectiveness (0.0 to 1.0)
float CoverEffectiveness = MyUnit->GetCurrentCoverEffectiveness();

// Get current unit state
EUnitState CurrentState = MyUnit->GetCurrentState();
```

## Debug Commands

While testing, you can enable debug drawing by calling on the CoverSystemManager:
- `DebugDrawCoverPoints()` - Shows cover points in the world
- `PrintCoverSystemStats()` - Prints statistics to console

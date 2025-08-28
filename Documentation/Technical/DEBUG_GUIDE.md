# Cover System Debug Guide

## Quick Debug Steps for Slot Counting Issue

### 1. Using the Debug Function
1. **Select a cover point** in the editor
2. **In Details panel** → Find "Cover Debug" section
3. **Click "Debug Occupancy State"** button
4. **Check console output** (press `~` to open console)

### 2. What to Look For

#### Expected Debug Output:
```
=== COVER POINT OCCUPANCY DEBUG ===
Cover Point: CoverPoint_C_2
OccupyingUnits Array Size: [NUMBER]
GetOccupiedSlots() returns: [NUMBER] 
GetMaxOccupants() returns: [NUMBER]
GetAvailableSlots() returns: [NUMBER]
  Unit 0: [UNIT_NAME] (Valid: YES/NO)
Cover State: [Available/Occupied/Reserved/Disabled]
Is Available: YES/NO
=== END OCCUPANCY DEBUG ===
```

#### Key Diagnostic Questions:
- **Is `AssignUnit()` being called?** Look for logs starting with "AssignUnit called for"
- **Does the array size match GetOccupiedSlots()?** They should be identical
- **Are units being added to the array?** Look for "Added unit [NAME] to OccupyingUnits. Count is now: [NUMBER]"

### 3. Possible Scenarios

#### Scenario A: AssignUnit Never Called
**Symptoms**: No "AssignUnit called" logs appear
**Cause**: Units aren't actually trying to assign to cover points
**Next Step**: Check unit AI/movement code

#### Scenario B: Array Gets Populated but GetOccupiedSlots() Returns 0
**Symptoms**: "Count is now: 1" but "GetOccupiedSlots() returns: 0"
**Cause**: Method implementation issue
**Next Step**: Check method implementation

#### Scenario C: Array Gets Cleared After Assignment
**Symptoms**: Array count increases then drops back to 0
**Cause**: Something is clearing the array (possibly Tick() method)
**Next Step**: Check for array modifications elsewhere

#### Scenario D: Units Are Added but Invalid
**Symptoms**: Array size > 0 but units show as "Valid: NO"
**Cause**: Units are being destroyed or becoming invalid
**Next Step**: Check unit lifecycle

### 4. Additional Debug Messages to Watch For

#### During Unit Assignment:
```
[CoverPoint_Name]: AssignUnit called for [Unit_Name]
[CoverPoint_Name]: Current OccupyingUnits count BEFORE: 0
[CoverPoint_Name]: Added unit [Unit_Name] to OccupyingUnits. Count is now: 1
[CoverPoint_Name]: GetOccupiedSlots() returns: 1, GetMaxOccupants() returns: 1
```

#### During Reservation Clearing:
```
[CoverPoint_Name]: Clearing reservation for [Unit_Name] as they are now occupying the cover point
```

#### On Screen Messages:
```
Unit assigned to cover point. Occupied slots: 1/1 (Array size: 1)
```

### 5. Console Commands for Testing

#### Open Console (press `~`) and try:
```
// No direct console commands available yet
// Use Blueprint debug functions in Details panel
```

### 6. If Issue Persists

Create an issue report with:
1. **Screenshots** of debug output
2. **Console logs** showing the problematic behavior  
3. **Steps to reproduce** the issue
4. **Which scenario** (A, B, C, or D) matches your observations

### 7. Temporary Workaround

If you need to test other systems while this is being debugged:
1. **Comment out** the slot display in UI
2. **Use cover point states** (Available/Occupied/Reserved) for logic instead
3. **Rely on IsAvailable()** method which works correctly

---

**Remember**: The reservation and availability systems are working correctly. Only the slot counting display has issues.

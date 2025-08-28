# Console Commands for Cover System Testing

## Quick Tests to Run in Editor/Game Console

### 1. Basic System Status
```
// Check if modules are loaded
ModuleStatus CoreMechanics
ModuleStatus Terrain

// List available console commands  
help cover
help debug

// Show current game stats
stat game
```

### 2. Debug Output Commands
```
// Enable debug categories in console
log LogTemp Verbose
log LogCover Verbose  
log LogTerrain Verbose

// Show debug information
showdebug game
showdebug collision
showdebug ai
```

### 3. Memory and Performance  
```
// Check memory usage
stat memory
memreport -full

// Performance stats
stat unit
stat fps
```

### 4. Blueprint-Specific Commands
```
// List all Blueprint classes
list blueprints

// Check for Blueprint compilation errors
bp.list.errors

// Refresh all Blueprints
bp.refresh.all
```

## Testing Steps in Order

### Step 1: Verify Editor Launch
1. Open Unreal Editor (should be starting now)
2. Wait for project to fully load
3. Check Output Log for any errors related to CoreMechanics or Terrain modules

**Expected Results:**
- No red error messages
- "CoreMechanics module loaded successfully" type messages
- "Terrain module loaded successfully" type messages

### Step 2: Check Component Availability  
1. Create a new Blueprint (Actor class)
2. Click "Add Component" 
3. Search for "Cover Detection"

**Expected Results:**  
- "Cover Detection Component" appears in the component list
- Can be added to the Blueprint
- Properties panel shows Cover Properties and Cover Destruction sections

### Step 3: Verify Enum Values
1. Select the Cover Detection Component
2. Look at the Cover Type dropdown in Details panel

**Expected Results:**
- Should show: None, Light, Heavy, Garrison, Negative
- Should NOT show old values like Half, Full, Concealment

### Step 4: Test Basic Functionality
1. Create a test level (File → New Level → Default)
2. Place your test actor with Cover Detection Component
3. Press Play
4. Open console (~ key) and run:
```
log LogTemp Verbose
```

**Expected Results:**
- Actor spawns successfully
- No error messages in console
- Component initializes with correct health values

### Step 5: Runtime Function Testing
If you created the test Blueprint with functions:
1. Trigger the test damage function (key binding or widget)
2. Check console output for delegate events

**Expected Results:**
- Health decreases when damage is applied
- Cover state changes appropriately
- Events fire and can be observed

## Immediate Red Flags to Watch For

### Critical Issues (Stop and Fix):
- **Module Load Errors**: "Failed to load CoreMechanics" 
- **Component Not Found**: Cover Detection Component doesn't appear
- **Enum Issues**: Old enum values still showing
- **Linker Errors**: Symbol redefinition errors
- **Crashes**: Editor or game crashes when using cover system

### Warning Issues (Note but Continue):
- **Performance Warnings**: High memory usage or slow compilation
- **Blueprint Warnings**: Non-critical Blueprint compilation warnings  
- **Debug Output**: Excessive debug spam (can be filtered)

## Quick Fixes for Common Problems

### If Cover Detection Component doesn't appear:
```console
// Refresh modules
ModuleReload CoreMechanics

// Clear Blueprint caches  
bp.refresh.all
```

### If enum values are wrong:
1. Close editor
2. Delete Intermediate folder
3. Regenerate project files: 
   ```cmd
   UnrealBuildTool.exe -projectfiles -project="ProjectHive.uproject" -game -rocket -progress
   ```
4. Rebuild and reopen

### If functions don't appear in Blueprint:
- Check UFUNCTION macros in header files
- Recompile the project
- Refresh the Blueprint editor (Compile button)

## Success Indicators

### Phase 1 Success (Basic Integration):
✅ Editor loads without errors
✅ Cover Detection Component is available  
✅ Properties can be edited
✅ New enum values are present

### Phase 2 Success (Runtime):
✅ Actors spawn with components
✅ Native functions can be called
✅ Delegates/events work  
✅ Debug info displays correctly

### Phase 3 Success (Full System):
✅ Cover damage system works
✅ Occupancy tracking works
✅ Integration with Terrain module works
✅ Performance is acceptable

Follow these steps in order, and stop at any critical issues to address them before continuing!

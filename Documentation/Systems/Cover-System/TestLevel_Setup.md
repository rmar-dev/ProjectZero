# Test Level Setup for Cover System

## Create Test Level
1. File → New Level → Empty Level
2. Save as "TestLevel_CoverSystem" in Content/Test folder

## Add Basic Environment
1. **Add a Floor**
   - Place a scaled cube (10,10,0.1) at (0,0,-50)
   - Or use BSP Box and convert to static mesh

2. **Add Lighting**
   - Add a Directional Light 
   - Add a Sky Light for ambient lighting

3. **Add Player Start**
   - Place a Player Start actor in the level

## Add Test Actors

### 1. Place Cover Points (Existing Terrain System)
If you have existing CoverPoint actors:
- Place several ACoverPoint actors around the level
- Verify they now use the new ECoverType enum values
- Check their properties show the new enum options

### 2. Place Test Cover Actor
- Place the BP_CoverTestActor you created
- Position it where you can easily reach it in-game

### 3. Add Interactive Elements
Create a simple Blueprint widget or use keyboard input to trigger cover system tests:

#### Blueprint Widget (Optional)
- Create a Widget Blueprint with buttons:
  - "Damage Cover (25)"
  - "Reserve Cover"  
  - "Release Cover"
  - "Show Debug Info"
  - "Toggle Wireframe Debug"

#### Keyboard Input (Simpler)
In your Player Controller Blueprint or Game Mode:
- Bind keys 1-5 to trigger different cover system functions
- Have them find the nearest cover component and call test functions

## Testing Checklist

### Phase 1: Basic Functionality
- [ ] Level loads without errors
- [ ] Cover Detection Components appear in Blueprint component list
- [ ] Properties can be set in editor (Cover Type, Material, etc.)
- [ ] Default values are applied correctly

### Phase 2: Runtime Testing  
- [ ] Components initialize properly (health = max health)
- [ ] Debug info displays correct initial state
- [ ] ApplyCoverDamage_Native reduces health
- [ ] Cover state changes when health drops (Intact → Damaged → Heavily Damaged → Destroyed)
- [ ] Events fire when cover state changes

### Phase 3: Occupancy System
- [ ] ReserveCover_Native works 
- [ ] ReleaseCover_Native works
- [ ] Max occupants limit is enforced
- [ ] Weak pointer cleanup works (no crashes with invalid references)

### Phase 4: Integration Testing
- [ ] Terrain CoverPoint actors work with new system
- [ ] CoverPointManager can find and manage cover points
- [ ] Enum values display correctly in editor dropdowns
- [ ] No linker errors or symbol conflicts

## Console Commands to Test

Once in the game level:
```
// Enable debug output
showdebug game

// Show collision shapes (if you want to see bounds)
showdebug collision

// Show AI debug info (if available)
showdebug ai

// Enable stat monitoring
stat game
stat memory
```

## Expected Results by Phase

### Initial Load
- No compilation errors
- No Blueprint compilation errors
- Level loads successfully
- All actors spawn without crashes

### Component Testing
- Cover Detection Component properties are editable
- _Native functions are callable from Blueprint
- Events can be bound and fire correctly
- Debug info displays properly

### Integration
- Old CoverPoint actors work with new system
- Enum values appear correctly
- No conflicts between Terrain and CoreMechanics modules
- Performance is acceptable (no major hitches)

## Troubleshooting Common Issues

### If Cover Detection Component doesn't appear:
1. Check if CoreMechanics module is loaded
2. Verify UCLASS has proper meta tags
3. Rebuild and refresh Blueprint editor

### If _Native functions don't appear:
1. Verify UFUNCTION macros are present
2. Check BlueprintCallable category
3. Recompile and refresh

### If events don't fire:
1. Check delegate declarations
2. Verify events are properly bound in BeginPlay
3. Check if component is valid when event fires

### If enum values are wrong:
1. Verify ECoverType enum has correct values
2. Check if old .generated.h files need cleanup
3. Rebuild entire project

This systematic testing approach will help identify any remaining issues with the cover system integration.

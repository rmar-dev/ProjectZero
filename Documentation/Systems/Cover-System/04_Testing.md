# Testing & Troubleshooting Guide

## 🎯 Overview
Comprehensive guide for testing the cover system and solving common issues.

## 🧪 Testing Framework

### Phase 1: Component Verification (5 minutes)

#### ✅ Test 1: Component Availability
**Goal**: Verify Cover Detection Component is accessible

**Steps**:
1. Create new Blueprint → Actor
2. Click "Add Component"  
3. Search for "Cover Detection"

**Expected Result**: Component appears in list and can be added
**If Failed**: [Jump to Component Missing](#component-missing)

#### ✅ Test 2: Property Access
**Goal**: Ensure all properties are editable

**Steps**:
1. Add Cover Detection Component
2. Select it in Components panel
3. Check Details panel for categories:
   - Cover Properties
   - Cover Destruction

**Expected Result**: All properties visible and editable
**If Failed**: [Jump to Properties Missing](#properties-missing)

#### ✅ Test 3: Enum Values
**Goal**: Verify enum integration worked

**Steps**:
1. Select Cover Detection Component
2. Check Cover Type dropdown

**Expected Result**: Shows Light, Heavy, Garrison, Negative (NOT Half, Full, Concealment)
**If Failed**: [Jump to Enum Issues](#enum-issues)

### Phase 2: Runtime Testing (10 minutes)

#### ✅ Test 4: Component Initialization
**Goal**: Component initializes without errors

**Test Blueprint**:
```blueprint
Event BeginPlay
→ Get Cover Detection Component
→ Branch (Is Valid?)
   ├── True: Print String ("Cover Component Valid")
   └── False: Print String ("ERROR: Cover Component Invalid")
```

**Expected Result**: "Cover Component Valid" message
**If Failed**: [Jump to Init Issues](#initialization-issues)

#### ✅ Test 5: Health System
**Goal**: Health and damage system works

**Test Blueprint**:
```blueprint
Custom Event: Test Health
→ Get Cover Detection Component
→ Get Cover Debug Info Native
→ Print String (Debug Info)
→ Apply Cover Damage Native (25, "Test", (1,0,0))  
→ Get Cover Debug Info Native
→ Print String (Updated Debug Info)
```

**Expected Result**: 
- Initial health shows max value (100)
- After damage, health decreases (75)
- Cover state may change if health drops enough

**If Failed**: [Jump to Health Issues](#health-system-issues)

#### ✅ Test 6: Occupancy System
**Goal**: Reservation and occupancy tracking works

**Test Blueprint**:
```blueprint
Custom Event: Test Occupancy
→ Get Player Pawn
→ Get Cover Detection Component
→ Reserve Cover Native (Player Pawn)
→ Get Cover Debug Info Native  
→ Print String (Should show reservation)
→ Release Cover Native (Player Pawn)
→ Get Cover Debug Info Native
→ Print String (Should show release)
```

**Expected Result**: Debug info reflects reservation changes
**If Failed**: [Jump to Occupancy Issues](#occupancy-issues)

#### ✅ Test 7: Configurable Properties
**Goal**: New Blueprint-configurable properties work correctly

**Test Blueprint**:
```blueprint
Custom Event: Test Configuration
→ Print String ("Testing Configurable Properties")
→ Get CoverPoint Actor
→ Branch (Use Modular Visuals?)
   ├── True:
   │   ├── Print String ("Modular System Active")
   │   ├── Set Status Indicator Visible (true)
   │   ├── Set Status Indicator Color (Blue)
   │   └── Print String ("Visual Configuration Test Complete")
   └── False:
       └── Print String ("Legacy System Active")
```

**Property Validation Test:**
```blueprint
Custom Event: Validate Properties
→ Get Cover Mesh Scale → Print String
→ Get Ground Marker Scale → Print String
→ Get Status Indicator Scale → Print String
→ Get Interaction Volume Extent → Print String
→ Get Emissive Color Multiplier → Print String
```

**Expected Result**: 
- All properties return expected values (not zeros or nulls)
- Modular visuals show proper scaling and positioning
- Visual effects apply correctly with configured multipliers

**If Failed**: [Jump to Configuration Issues](#configuration-issues)

### Phase 3: Integration Testing (15 minutes)

#### ✅ Test 7: Event System
**Goal**: Delegates fire correctly

**Test Blueprint**:
```blueprint
Event BeginPlay
→ Get Cover Detection Component
→ Bind Event to On Cover Damaged
→ Bind Event to On Cover State Changed

On Cover Damaged Event Handler:
→ Print String ("Cover Damaged Event Fired!")

On Cover State Changed Event Handler:
→ Print String ("Cover State Changed Event Fired!")
```

**Expected Result**: Events fire when damage is applied
**If Failed**: [Jump to Event Issues](#event-system-issues)

#### ✅ Test 8: Manager Integration
**Goal**: Cover Point Manager can find cover points

**Test in existing CoverPoint**:
```blueprint
Event BeginPlay
→ Get All Actors of Class (CoverPointManager)
→ For Each Manager:
   └── Register Cover Point (Self)
```

**Expected Result**: No errors, cover point registers
**If Failed**: [Jump to Manager Issues](#manager-issues)

---

## 🚨 Troubleshooting Guide

### Component Missing
**Symptoms**: 
- Cover Detection Component doesn't appear in Add Component list
- "Class not found" errors

**Solutions**:
1. **Refresh Blueprints**:
   ```console
   bp.refresh.all
   ```

2. **Check Module Loading**:
   ```console
   ModuleStatus CoreMechanics
   ```

3. **Regenerate Project**:
   - Close Unreal Editor
   - Delete `Intermediate` and `Binaries` folders
   - Right-click `.uproject` → Generate Visual Studio Files
   - Build and reopen

4. **Verify UCLASS Declaration**:
   ```cpp
   // In CoverDetectionComponent.h
   UCLASS(ClassGroup=(Cover), meta=(BlueprintSpawnableComponent))
   class COREMECHANICS_API UCoverDetectionComponent : public USceneComponent
   ```

### Properties Missing
**Symptoms**:
- Cover Detection Component appears but no properties
- Properties show as empty or default values

**Solutions**:
1. **Check UPROPERTY Macros**:
   ```cpp
   UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Cover Properties")
   ECoverType CoverType = ECoverType::Light;
   ```

2. **Rebuild and Refresh**:
   ```console
   // In editor console
   Compile
   bp.refresh.all
   ```

3. **Check Header Include**:
   ```cpp
   #include "CoverSystemTypes.h"
   #include "CoverSystemConstants.h"
   ```

### Enum Issues
**Symptoms**:
- Old enum values (Half, Full, Concealment) still showing
- Enum dropdown is empty or shows wrong values

**Solutions**:
1. **Clear Generated Files**:
   - Close editor
   - Delete `Intermediate` folder
   - Delete `Binaries` folder  
   - Regenerate project files

2. **Verify Enum Definition**:
   ```cpp
   UENUM(BlueprintType)
   enum class ECoverType : uint8
   {
       None      UMETA(DisplayName = "None"),
       Light     UMETA(DisplayName = "Light Cover"),
       Heavy     UMETA(DisplayName = "Heavy Cover"), 
       Garrison  UMETA(DisplayName = "Garrison"),
       Negative  UMETA(DisplayName = "Negative Cover")
   };
   ```

3. **Full Rebuild**:
   ```cmd
   UnrealBuildTool.exe -projectfiles -project="ProjectHive.uproject" -game -rocket -progress
   ```

### Initialization Issues
**Symptoms**:
- Component is null at runtime
- BeginPlay doesn't execute cover initialization

**Solutions**:
1. **Check Component Creation**:
   ```cpp
   // In constructor
   CoverDetectionComponent = CreateDefaultSubobject<UCoverDetectionComponent>(TEXT("CoverDetectionComponent"));
   ```

2. **Verify BeginPlay Call**:
   ```cpp
   void AYourActor::BeginPlay()
   {
       Super::BeginPlay();  // Important!
       // Your code here
   }
   ```

3. **Debug Component State**:
   ```blueprint
   Event BeginPlay
   → Get Components by Class (Cover Detection Component)
   → Branch (Array Length > 0?)
      ├── True: Print "Component Found"
      └── False: Print "No Components Found"
   ```

### Health System Issues
**Symptoms**:
- Health doesn't change when damage applied
- Cover state doesn't update correctly

**Solutions**:
1. **Check Damage Function**:
   ```cpp
   // Verify this exists and is callable
   bool UCoverDetectionComponent::ApplyCoverDamage_Native(float DamageAmount, const FString& WeaponType, const FVector& AttackDirection)
   ```

2. **Enable Destruction**:
   ```blueprint
   // In Cover Detection Component properties
   Can Be Destroyed: ✓ Checked
   Max Health: > 0 (e.g., 100)
   ```

3. **Debug Health Values**:
   ```blueprint
   Before Damage:
   → Get Cover Health Native → Print String
   
   Apply Damage:
   → Apply Cover Damage Native
   
   After Damage:
   → Get Cover Health Native → Print String
   ```

### Occupancy Issues
**Symptoms**:
- Reservation/release functions don't work
- Debug info shows wrong occupancy count

**Solutions**:
1. **Check Weak Pointer Cleanup**:
   ```cpp
   // Should be called automatically
   void UCoverDetectionComponent::CleanupInvalidReferences() const
   ```

2. **Verify Actor References**:
   ```blueprint
   Reserve Cover Test:
   → Get Player Pawn
   → Branch (Is Valid?)
      ├── True: Reserve Cover Native
      └── False: Print "Invalid Actor Reference"
   ```

3. **Check Max Occupants**:
   ```blueprint
   // In Cover Detection Component
   Max Occupants: > 0 (e.g., 2)
   ```

### Event System Issues
**Symptoms**:
- Delegates don't fire
- Event binding fails

**Solutions**:
1. **Proper Event Binding**:
   ```blueprint
   Event BeginPlay
   → Get Cover Detection Component
   → Assign On Cover Damaged (Bind to Custom Event)
   ```

2. **Check Delegate Declarations**:
   ```cpp
   DECLARE_DYNAMIC_MULTICAST_DELEGATE_FourParams(FOnCoverDamaged, 
       UCoverDetectionComponent*, CoverComponent, 
       int32, DamageAmount, 
       const FString&, WeaponType, 
       const FVector&, AttackDirection);
   ```

3. **Verify Event Broadcasting**:
   ```cpp
   // In damage function
   OnCoverDamaged.Broadcast(this, DamageToApply, WeaponType, AttackDirection);
   ```

### Manager Issues
**Symptoms**:
- CoverPointManager can't find cover points
- Registration fails

**Solutions**:
1. **Check Manager Exists**:
   ```blueprint
   Get All Actors of Class (Cover Point Manager)
   → Branch (Array Length > 0?)
      ├── True: Use First Manager
      └── False: Print "No Cover Point Manager Found"
   ```

2. **Manual Registration**:
   ```cpp
   // In CoverPoint BeginPlay
   if (ADebugController* DebugController = ADebugController::GetInstance(GetWorld()))
   {
       DebugController->RegisterCoverPoint(this);
   }
   ```

### Configuration Issues
**Symptoms**:
- Configurable properties show zero or default values
- Modular visuals don't scale or position correctly
- Visual effects don't apply with proper intensity

**Solutions**:
1. **Verify Property Initialization**:
   ```cpp
   // In CoverPoint constructor
   CoverMeshScale = FVector(1.0f, 1.0f, 1.0f);
   GroundMarkerScale = FVector(0.8f, 0.8f, 0.02f);
   StatusIndicatorScale = FVector(1.1f, 1.1f, 1.1f);
   EmissiveColorMultiplier = 0.5f;
   ```

2. **Check UPROPERTY Exposure**:
   ```cpp
   UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Cover Visuals")
   FVector CoverMeshScale = FVector(1.0f, 1.0f, 1.0f);
   ```

3. **Test Property Access**:
   ```blueprint
   Custom Event: Test Property Access
   → Get Cover Mesh Scale
   → Branch (Is Zero Vector?)
      ├── True: Print "ERROR: Property not initialized"
      └── False: Print "Property working correctly"
   ```

4. **Blueprint Refresh**:
   ```console
   // In editor console
   bp.refresh.all
   bp.compile.all
   ```

---

## 🔧 Debug Tools

### Console Commands
```console
// Enable verbose logging
log LogTemp Verbose
log LogCover Verbose
log LogTerrain Verbose

// Show component info
showdebug game
showdebug collision

// Performance monitoring  
stat game
stat memory
stat unit

// Blueprint debugging
bp.list.errors
bp.refresh.all
```

### Visual Debug Tools

#### Enable Wireframe Debug
```blueprint
Custom Event: Toggle Debug
→ Get All Actors of Class (CoverPoint)
→ For Each Actor:
   └── Show Wireframe (true)
```

#### Debug Drawing
```cpp
// In your cover point
void ACoverPoint::ShowDebugInfo()
{
    if (UWorld* World = GetWorld())
    {
        // Draw debug sphere at cover position
        DrawDebugSphere(World, GetActorLocation(), 50.0f, 12, FColor::Red, true, 5.0f);
        
        // Draw debug text
        DrawDebugString(World, GetActorLocation() + FVector(0,0,100), 
            GetCoverDebugInfo_Native(), nullptr, FColor::White, 5.0f);
    }
}
```

### Performance Profiling

#### Memory Usage
```console
memreport -full
stat memory
```

#### Performance Stats
```console
stat fps
stat unit
stat game
```

#### Blueprint Performance
```console
stat blueprintvm
showdebug blueprintvm
```

---

## 📊 Automated Testing

### Unit Test Framework
Create `BP_CoverSystemTests` Blueprint:

```blueprint
Event BeginPlay
→ Run All Tests

Custom Event: Run All Tests
→ Sequence:
   ├── Test Component Creation
   ├── Test Health System  
   ├── Test Occupancy System
   ├── Test Event System
   └── Report Results

Custom Event: Test Health System
→ Create Test Actor with Cover Component
→ Apply Damage (50)
→ Check Health Changed
→ Assert Health = 50
→ Report Test Result
```

### Regression Testing
```blueprint
Event: Daily Regression Test
→ Load Test Level
→ Run Component Tests
→ Run Integration Tests  
→ Check Performance Benchmarks
→ Generate Report
```

---

## ✅ Pre-Production Checklist

### Performance Requirements
- [ ] System handles 100+ cover points without frame drops
- [ ] Memory usage stays under acceptable limits
- [ ] No memory leaks over extended gameplay

### Functionality Requirements  
- [ ] All component properties work correctly
- [ ] Health/damage system functions properly
- [ ] Occupancy tracking accurate
- [ ] Events fire reliably
- [ ] Integration with existing systems works

### Stability Requirements
- [ ] No crashes during normal gameplay  
- [ ] Handles edge cases gracefully
- [ ] Save/load compatibility maintained
- [ ] Network synchronization (if multiplayer)

### User Experience Requirements
- [ ] Clear visual feedback for designers
- [ ] Intuitive property names and organization
- [ ] Helpful debug information
- [ ] Good performance in editor and game

---

## 🎯 Success Metrics

### Technical Metrics
- **Build Success**: 100% clean compile
- **Test Pass Rate**: 95%+ automated tests passing  
- **Performance**: <2ms per frame for cover system
- **Memory**: <10MB for 100 cover points

### Usability Metrics
- **Setup Time**: <5 minutes for basic cover point
- **Learning Curve**: Designer productive within 30 minutes
- **Debug Time**: <10 minutes to identify and fix issues

**Status Check**: Use this guide to verify your cover system meets all requirements before moving to production! 🎯

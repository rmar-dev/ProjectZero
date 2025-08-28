# BP_CoverTestActor Blueprint Setup Instructions

## Overview
This Blueprint will be a simple test actor that demonstrates the Cover Detection Component functionality.

## Blueprint Setup Instructions

### 1. Create the Blueprint
- In Content Browser, create a new folder called "Test"
- Right-click in the Test folder → Create → Blueprint Class → Actor
- Name it "BP_CoverTestActor"

### 2. Add Components
1. **Static Mesh Component** (for visualization)
   - Add a Static Mesh Component
   - Set the mesh to a basic cube or cylinder
   - Scale it appropriately (e.g., 2,1,2 for a wall-like object)

2. **Cover Detection Component**
   - Add a Cover Detection Component from the Components panel
   - This should appear as "Cover Detection Component" in the list

### 3. Configure Cover Detection Component Properties
Set these properties in the Details panel:

**Cover Properties:**
- Cover Type: Heavy (for testing strong cover)
- Cover Material: Stone 
- Cover Facing: North
- Protection Arc: 120.0
- Max Occupants: 2
- Cover Priority: 1.0
- Cover Position Offset: (0, 0, 0)
- Is Temporary Cover: false

**Cover Destruction:**
- Max Health: 100
- Can Be Destroyed: true

### 4. Add Visual Feedback
1. Add a **Text Render Component** above the mesh
2. Set the text to show cover status
3. In BeginPlay, bind to the cover component events:
   - On Cover Damaged
   - On Cover State Changed  
   - On Cover Destroyed
   - On Cover Occupied
   - On Cover Vacated

### 5. Add Debug Functionality
Create these custom events:
- **Test Damage Cover**: Calls ApplyCoverDamage_Native with test values
- **Test Reserve Cover**: Calls ReserveCover_Native 
- **Test Release Cover**: Calls ReleaseCover_Native
- **Show Debug Info**: Calls GetCoverDebugInfo_Native and prints to screen

## Testing Events to Add

### Event BeginPlay
```
// Update text display with current cover info
Set Text (Text Render) → Get Cover Debug Info Native
```

### Custom Event: Test Damage
```
Input: Damage Amount (float, default 25)
→ Apply Cover Damage Native (Damage Amount, "Rifle", (1,0,0))
→ Update Display Text
```

### Custom Event: Test Reserve  
```
Input: Reserving Actor (Actor Reference, default Self)
→ Reserve Cover Native (Reserving Actor)
→ Update Display Text  
```

### Custom Event: Update Display
```
→ Get Cover Debug Info Native
→ Set Text (Text Render Component)
```

## Key Testing Points
1. **Component Creation**: Verify the Cover Detection Component appears and can be added
2. **Property Access**: Verify all UPROPERTY values can be set in editor
3. **Native Functions**: Verify _Native functions can be called from Blueprint
4. **Events**: Verify delegates fire when cover state changes
5. **Interface Compliance**: Verify the component implements ICoverDetectable interface

## Console Commands for Testing
Once in-game, use these console commands:
- `showdebug coverpoints` - If available
- `stat game` - Show game stats
- `showdebug collision` - Show collision info

## Expected Behavior
- Component should initialize properly with default values
- Health should start at MaxHealth (100)
- Cover should be in "Intact" state initially  
- Damage should reduce health and potentially change state
- Reservations should track properly
- Debug info should display current status

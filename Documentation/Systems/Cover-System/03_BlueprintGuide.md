# Blueprint Implementation Guide

## 🎯 Overview
This guide shows you how to create production-ready cover points using Blueprints, with full integration into your gameplay systems.

## 📋 What You'll Build
- **BP_CoverPoint_Basic**: Simple cover points for level designers
- **BP_CoverPoint_Advanced**: Full-featured cover with visuals and effects
- **BP_CoverTestWidget**: UI for testing cover functionality
- **Integration patterns** for unit AI and combat systems

---

## 🏗️ Part 1: Basic Cover Point Blueprint

### Create the Blueprint
1. **Content Browser**: Right-click → Blueprint Class → Actor
2. **Name**: `BP_CoverPoint_Basic`
3. **Open** the Blueprint

### Add Core Components

#### 1. Root Setup
```
DefaultSceneRoot (rename to "CoverRoot")
├── CoverDetectionComponent
├── StaticMeshComponent (rename to "CoverVisualization") 
└── SceneComponent (rename to "UnitPosition")
```

#### 2. Component Configuration

**CoverDetectionComponent:**
- This is your core component (already added by default)

**CoverVisualization (Static Mesh):**
- Static Mesh: `/Engine/BasicShapes/Cylinder`
- Material: Create a material or use default
- Transform: Scale (1, 1, 0.1) for a flat marker
- Collision: No Collision (visual only)

**UnitPosition (Scene Component):**
- Transform: Location (0, 150, 0) - where units will stand
- This shows where units position themselves

### Configure Cover Properties

**Select CoverDetectionComponent** and set:

```ini
Cover Properties:
├── Cover Type: Heavy
├── Cover Material: Stone  
├── Cover Facing: North
├── Protection Arc: 120.0
├── Max Occupants: 2
├── Cover Priority: 1.0
├── Cover Position Offset: (0, 0, 0)
├── Is Temporary Cover: false
└── Cover Lifespan: -1

Cover Destruction:
├── Max Health: 100
├── Can Be Destroyed: true
```

#### Configure Visual Properties

**Select the CoverPoint Actor** and configure:

```ini
Cover Visuals (Modular System):
├── Use Modular Visuals: ✓ Checked
├── Cover Mesh Scale: (1.0, 1.0, 1.0)
├── Ground Marker Scale: (0.8, 0.8, 0.02)
├── Status Indicator Scale: (1.1, 1.1, 1.1)
├── Cover Mesh Location: (0, 0, 100)
├── Ground Marker Location: (0, 0, 1)
└── Status Indicator Location: (0, 0, 100)

Interaction Settings:
├── Interaction Volume Extent: (75, 75, 25)
├── Interaction Volume Location: (0, 0, 15)
└── Unit Position Offset: (0, 0, 0)

Visual Effects:
├── Emissive Color Multiplier: 0.5
├── Hover Scale Multiplier: 1.2
├── Available Color: Green
├── Occupied Color: Red
├── Hover Color: Yellow
└── Disabled Color: Gray

Debug Settings:
├── Debug Sphere Radius: 15.0
├── Debug Wireframe Thickness: 2.0
├── Debug Display Duration: 30.0
├── Unit Marker Sphere Radius: 25.0
└── Coordinate System Scale: 50.0
```

### Add Visual Feedback

#### 1. Add Text Component
- **Add Component**: Text Render
- **Name**: "StatusText"
- **Transform**: Location (0, 0, 100)
- **Text**: "Cover Point"
- **World Size**: 24
- **Text Color**: White

#### 2. Add Material Dynamic Instance
**In Construction Script:**
```blueprint
Construction Script
→ Create Dynamic Material Instance
   ├── Source Material: [Your Material]
   └── Outer: Self
→ Set Material (StaticMeshComponent, Index 0)
```

### Event Graph - Basic Functionality

#### Event BeginPlay
```blueprint
Event BeginPlay
→ Get Cover Detection Component
→ Branch (Is Valid?)
   ├── True: 
   │   ├── Bind Event to On Cover State Changed
   │   ├── Bind Event to On Cover Damaged  
   │   ├── Bind Event to On Cover Occupied
   │   ├── Bind Event to On Cover Vacated
   │   └── Update Visual Display
   └── False: Print String ("Cover Component Missing!")
```

#### Visual Update Function
**Create Custom Function: "UpdateVisualDisplay"**
```blueprint
UpdateVisualDisplay
→ Get Cover Detection Component
→ Get Cover Debug Info Native
→ Set Text (StatusText, Debug Info)
→ Branch (Current State)
   ├── Intact: Set Material Color (Green)
   ├── Damaged: Set Material Color (Yellow) 
   ├── Heavily Damaged: Set Material Color (Orange)
   └── Destroyed: Set Material Color (Red)
```

#### Event Handlers
**Create Custom Events:**

**On Cover State Changed Event:**
```blueprint
OnCoverStateChanged (CoverComponent, OldState, NewState)
→ Update Visual Display
→ Print String (State Change Message)
```

**On Cover Damaged Event:**
```blueprint
OnCoverDamaged (CoverComponent, Damage, WeaponType, Direction)
→ Update Visual Display
→ Print String (Damage Message)
→ [Optional] Play Damage Effect
```

**On Cover Occupied Event:**
```blueprint
OnCoverOccupied (CoverComponent, Unit)
→ Update Visual Display  
→ Print String (Unit Assigned Message)
```

**On Cover Vacated Event:**
```blueprint
OnCoverVacated (CoverComponent, Unit)
→ Update Visual Display
→ Print String (Unit Removed Message)
```

### Compile and Test
1. **Compile** the Blueprint
2. **Save** 
3. **Place in level** and test

---

## 🎨 Part 2: Advanced Cover Point Blueprint

### Create Advanced Version
1. **Duplicate** `BP_CoverPoint_Basic`
2. **Rename** to `BP_CoverPoint_Advanced`

### Enhanced Components

#### Add Visual Effects
```
CoverRoot
├── [Previous Components]
├── ParticleSystemComponent ("DamageEffects")
├── AudioComponent ("CoverSounds")
├── StaticMeshComponent ("CoverObject") - The actual cover  
├── StaticMeshComponent ("DestructionDebris")
└── WidgetComponent ("HoverWidget") - For UI
```

#### Configure New Components

**CoverObject (The actual wall/crate):**
- Static Mesh: Choose appropriate cover mesh
- Material: Your cover material
- Collision: Block All or Custom
- Transform: Position as needed

**DamageEffects (Particle System):**
- Template: Damage/spark effects
- Auto Activate: false
- Location: At cover object

**CoverSounds (Audio):**
- Sound: None (controlled by Blueprint)
- Auto Activate: false

**HoverWidget (Widget Component):**
- Widget Class: Create UI widget for hover info
- Draw Size: (200, 100)
- Visibility: Hidden initially

### Advanced Event Graph

#### Enhanced BeginPlay
```blueprint
Event BeginPlay
→ [Previous BeginPlay logic]
→ Setup Hover Detection
→ Initialize Effects Systems
→ Register with Cover Manager (if available)
```

#### Damage Effects System
**Enhanced Damage Event:**
```blueprint
OnCoverDamaged (CoverComponent, Damage, WeaponType, Direction)
→ [Previous damage logic]
→ Branch (Damage > Threshold?)
   ├── True:
   │   ├── Activate Particle System
   │   ├── Play Audio Cue
   │   └── Screen Shake (if nearby player)
   └── False: [Minor effects]
→ Delay (2 seconds)
→ Deactivate Effects
```

#### Destruction Effects
**On Cover Destroyed:**
```blueprint
OnCoverDestroyed (CoverComponent)
→ [Previous destruction logic]
→ Sequence:
   ├── 1. Hide Cover Object
   ├── 2. Show Destruction Debris  
   ├── 3. Play Destruction Sound
   ├── 4. Spawn Destruction Particles
   └── 5. Update All Visual Elements
```

#### Hover System
**Mouse Events:**
```blueprint
OnBeginMouseOver (Component)
→ Show Hover Widget
→ Update Hover Info
→ Highlight Cover Object

OnEndMouseOver (Component)  
→ Hide Hover Widget
→ Remove Highlight
```

#### Smart Position Calculation
**Create Function: "GetBestUnitPosition"**
```blueprint
GetBestUnitPosition (Threat Location)
→ Get Cover Detection Component
→ Get Cover Bounds Native
→ Calculate Safe Angles
→ Find Optimal Position
→ Return Best Position
```

---

## 🧪 Part 3: Testing Widget Blueprint

### Create Test Widget
1. **Create Widget Blueprint**: `BP_CoverTestWidget`
2. **Add to viewport** during testing

### Widget Design
```
Canvas Panel
├── Vertical Box
│   ├── Text Block ("Cover System Test")
│   ├── Button ("Test Damage 25")
│   ├── Button ("Test Damage 50") 
│   ├── Button ("Reserve Cover")
│   ├── Button ("Release Cover")
│   ├── Button ("Show Debug Info")
│   └── Button ("Toggle Wireframe")
└── Text Block ("Output Display")
```

### Widget Logic

#### Get Target Cover Point
**Create Function: "GetNearestCoverPoint"**
```blueprint
GetNearestCoverPoint
→ Get Player Pawn
→ Get Actor Location
→ Get All Actors of Class (CoverPoint)
→ Find Nearest
→ Return Cover Point
```

#### Button Functions
**Test Damage Button:**
```blueprint
OnTestDamageClicked
→ Get Nearest Cover Point
→ Get Cover Detection Component
→ Apply Cover Damage Native (25, "Test Rifle", (1,0,0))
→ Update Output Display
```

**Reserve Cover Button:**
```blueprint
OnReserveCoverClicked  
→ Get Nearest Cover Point
→ Get Player Pawn
→ Reserve Cover Native (Player Pawn)
→ Update Output Display
```

**Show Debug Info Button:**
```blueprint
OnShowDebugClicked
→ Get Nearest Cover Point  
→ Get Cover Debug Info Native
→ Set Text (Output Display)
```

---

## 🔗 Part 4: Integration Patterns

### Unit AI Integration

#### In Your Unit Blueprint
**Function: "FindBestCover"**
```blueprint
FindBestCover (Threat Position)
→ Get Cover Point Manager
→ Find Best Cover Point (Self, Threat Position)
→ Branch (Found Cover?)
   ├── True:
   │   ├── Reserve Cover Native
   │   ├── Set AI Target (Cover Position)  
   │   └── Start Move to Cover
   └── False: Use Alternative Behavior
```

#### Movement Component Integration
**When Unit Reaches Cover:**
```blueprint
OnMovementComplete
→ Branch (At Cover Position?)
   ├── True:
   │   ├── Add Occupant (Self)
   │   ├── Set Unit State (In Cover)
   │   └── Apply Cover Bonuses
   └── False: [Normal movement handling]
```

### Combat System Integration

#### Damage Calculation
**In Combat Manager:**
```blueprint
CalculateDamage (Attacker, Target)
→ Get Target Cover Status
→ Branch (Target In Cover?)
   ├── True:
   │   ├── Get Cover Protection Value
   │   ├── Calculate Line of Sight
   │   ├── Apply Cover Damage Reduction
   │   └── Return Modified Damage
   └── False: Return Base Damage
```

#### Cover Destruction in Combat
**When Cover Takes Damage:**
```blueprint
OnCoverDamaged Event
→ Branch (Cover Destroyed?)
   ├── True:
   │   ├── Find All Occupants
   │   ├── For Each Occupant:
   │   │   ├── Remove Cover Status
   │   │   ├── Trigger "Seek New Cover" AI
   │   │   └── Apply Exposure Penalties
   │   └── Broadcast Cover Lost Event
   └── False: Continue Normal Flow
```

---

## 🎮 Part 5: Player Interaction

### Cover Preview System
**In Player Controller:**
```blueprint
OnMoveCommandGiven (Target Location)
→ Trace for Nearby Cover Points
→ For Each Cover Point:
   ├── Check if Reachable
   ├── Calculate Cover Effectiveness
   ├── Show Preview UI
   └── Highlight Recommended Positions
```

### Visual Feedback
**Cover Status Indicators:**
```blueprint
UpdateCoverStatusUI
→ Get Selected Units
→ For Each Unit:
   ├── Get Current Cover Status
   ├── Show Cover Effectiveness
   ├── Display Health/State
   └── Update UI Icons
```

---

## 🔧 Part 6: Performance Optimization

### Efficient Component Usage
**In BeginPlay:**
```blueprint
BeginPlay
→ Branch (Is Editor World?)
   ├── True: Enable All Debug Features
   └── False: Disable Heavy Debug Systems

→ Register with Manager (Delayed)
→ Setup LOD System for Effects
```

### Smart Update Frequency
**Create Custom Event: "PeriodicUpdate"**
```blueprint
PeriodicUpdate (Every 2 seconds)
→ Cleanup Invalid References  
→ Update Priority Scores
→ Refresh Manager Registration
```

---

## ✅ Testing Checklist

### Basic Functionality
- [ ] Cover Detection Component appears
- [ ] Properties can be edited in editor
- [ ] Visual feedback works (colors, text)
- [ ] Events fire correctly (damage, occupancy)
- [ ] Debug info displays properly

### Advanced Features  
- [ ] Effects system works (particles, audio)
- [ ] Hover UI displays correctly
- [ ] Destruction visuals work
- [ ] Performance is acceptable

### Integration
- [ ] Unit AI can find and use cover
- [ ] Combat system applies cover bonuses
- [ ] Player gets visual feedback
- [ ] Multiple units can share cover

### Edge Cases
- [ ] Cover destruction handling
- [ ] Invalid unit references cleaned up
- [ ] Network synchronization (if multiplayer)
- [ ] Save/load compatibility

## 🎉 Completion

You now have a complete Blueprint-based cover system! The blueprints you created provide:

- **Designer-friendly** cover point placement
- **Visual feedback** for development and gameplay  
- **Extensible architecture** for game-specific needs
- **Performance optimization** for production use

### Next Steps:
- 📊 **Balance Testing**: Tune cover effectiveness values
- 🎨 **Art Integration**: Replace placeholder meshes with final art
- 🤖 **AI Enhancement**: Advanced AI cover tactics
- 🌐 **Multiplayer**: Network synchronization if needed

**Status: 🎯 Production Ready!**

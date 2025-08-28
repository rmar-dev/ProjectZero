# ProjectZero - Task Management
## **Tactical Squad-Based Extraction Shooter (Unity Port)**

**Current Phase:** Phase 1 - Foundation Systems (Unity Port)  
**Target Completion:** September 30, 2025  
**Status:** Fresh Unity Project - All Tasks Reset to Not Started  
**Last Updated:** August 28, 2025  
**Engine Migration:** Unreal Engine 5.5 → Unity 2022.3 LTS

---

## 🚀 **PHASE 1: CORE FOUNDATION** - Unity Implementation

**Current Sprint:** Core Systems Implementation  
**Target:** Playable squad movement with tactical pause  
**Timeline:** 2 weeks (Sep 1-14, 2025)

### 🔥 **WEEK 1: PROJECT & CORE SETUP** (Sep 1-7)

#### **Priority 1: Unity Project Foundation** ✅ CRITICAL
- [ ] **Create Unity Project**
  - [ ] Create new Unity 2022.3 LTS project named "ProjectZero"
  - [ ] Set up folder structure: `_Project/{Scripts,Prefabs,Scenes,Input,Materials}`
  - [ ] Install required packages: Input System, Cinemachine, NavMesh Components
  - [ ] Configure project settings for tactical game

#### **Priority 2: Core State Management** ✅ CRITICAL  
- [ ] **GameStateManager.cs**
  - [ ] Create singleton MonoBehaviour in `Scripts/Core/`
  - [ ] Implement GameState enum: MainMenu, Loading, InGame, Paused, GameOver
  - [ ] Implement TacticalState enum: RealTime, TacticalPause, SlowMotion
  - [ ] Add Time.timeScale control for pause/slowmo
  - [ ] Add Unity Events for state change notifications
  - [ ] Test state transitions work properly

#### **Priority 3: Input System Foundation** ✅ CRITICAL
- [ ] **Unity Input System Setup**
  - [ ] Create `SquadControls.inputactions` asset in `Input/` folder
  - [ ] Configure Action Maps: TacticalControls, SquadCommands, CameraControls
  - [ ] Generate C# class from Input Actions
  - [ ] Test input actions respond in scene

### 🔥 **WEEK 2: SQUAD & CAMERA SYSTEMS** (Sep 8-14)

#### **Priority 4: Camera System** ✅ CRITICAL
- [ ] **TacticalCameraController.cs**
  - [ ] Create script in `Scripts/Camera/`
  - [ ] Set up Cinemachine Virtual Camera with top-down view
  - [ ] Implement WASD movement with smooth interpolation
  - [ ] Add mouse wheel zoom with bounds
  - [ ] Add Q/E rotation controls
  - [ ] Test camera feels responsive and smooth

#### **Priority 5: Basic Squad System** ✅ CRITICAL
- [ ] **SquadManager.cs**
  - [ ] Create MonoBehaviour in `Scripts/Squad/`
  - [ ] Implement squad member List<SelectableUnit>
  - [ ] Add formation types: Tight, Spread, Wedge  
  - [ ] Create basic formation calculation algorithms
  - [ ] Add right-click contextual command processing
  - [ ] Test formation switching works

#### **Priority 6: Character Foundation** ✅ CRITICAL
- [ ] **SelectableUnit.cs**
  - [ ] Create MonoBehaviour in `Scripts/Characters/`
  - [ ] Add NavMesh Agent requirement and configuration
  - [ ] Implement MoveTo(Vector3 destination) method
  - [ ] Add basic unit properties (name, role, health)
  - [ ] Create unit selection/highlighting system
  - [ ] Test units move correctly with NavMesh

#### **Priority 7: Integration & Testing** ✅ CRITICAL
- [ ] **Scene Setup & Integration**
  - [ ] Create MainGame.unity scene with NavMesh baked
  - [ ] Place GameStateManager GameObject in scene  
  - [ ] Set up SquadManager with 3-4 test units (capsules)
  - [ ] Configure Cinemachine camera targeting squad center
  - [ ] Test all systems work together without errors

#### **Priority 8: Input Integration** ✅ CRITICAL  
- [ ] **SquadPlayerController.cs**
  - [ ] Create input handler in `Scripts/Input/`
  - [ ] Connect Input Actions to SquadManager commands
  - [ ] Implement mouse world position calculation
  - [ ] Add contextual command determination logic
  - [ ] Test: Space pauses, F cycles formations, right-click moves squad

---

## 🎯 **PHASE 1: UNITY IMPLEMENTATION TASKS**

### **Priority 1: Project Setup** (Critical - Week 1)
- [ ] 🔥 **Unity Project Configuration**
  - [ ] Set up Unity 2022.3 LTS project with proper folder structure
  - [ ] Configure Unity Input System package
  - [ ] Install Cinemachine package for camera control
  - [ ] Set up NavMesh components and baking
- [ ] 🔥 **Basic Scene Setup**
  - [ ] Create main game scene with lighting
  - [ ] Set up NavMesh surface for squad movement
  - [ ] Configure basic terrain/ground for testing
  - [ ] Add basic environment objects for cover testing

### **Priority 2: Core Systems Implementation** (Critical - Week 2-3)
- [ ] 🔥 **Game State Manager (Unity)**
  - [ ] Create GameStateManager.cs as singleton MonoBehaviour
  - [ ] Implement game states: MainMenu, Loading, InGame, Paused, GameOver
  - [ ] Implement tactical states: RealTime, TacticalPause, SlowMotion, CommandPlanning
  - [ ] Add Unity Events for state change notifications
  - [ ] Integrate with Time.timeScale for pause/slowmo
- [ ] 🔥 **Squad Management System (Unity)**
  - [ ] Create SquadManager.cs MonoBehaviour component
  - [ ] Implement squad member management with Unity transforms
  - [ ] Add formation types: Tight, Spread, Wedge
  - [ ] Create command processing: Move, Attack, TakeCover, HoldPosition
  - [ ] Implement command queuing for tactical pause scenarios
- [ ] 🔥 **Camera Controller (Unity)**
  - [ ] Create TacticalCameraController.cs with Cinemachine
  - [ ] Implement WASD movement with smooth camera follow
  - [ ] Add mouse wheel zoom with bounds checking
  - [ ] Add Q/E rotation controls
  - [ ] Configure edge scrolling support

### **Priority 3: Input System Implementation** (High - Week 3-4)
- [ ] ⚡ **Unity Input System Setup**
  - [ ] Create Input Actions asset with all tactical controls
  - [ ] Configure Input Action Map for squad control
  - [ ] Implement SquadPlayerController.cs for input handling
  - [ ] Add contextual command determination (ground, enemy, cover)
- [ ] ⚡ **Character Movement Integration**
  - [ ] Create SelectableUnit.cs with NavMesh Agent
  - [ ] Implement formation movement with Unity pathfinding
  - [ ] Add unit selection and highlighting system
  - [ ] Configure smooth movement transitions

### **Priority 4: Basic Combat Foundation** (Medium - Week 4)
- [ ] 📋 **Health and Damage System**
  - [ ] Create Health.cs component for all units
  - [ ] Implement basic damage calculation
  - [ ] Add visual health indicators
  - [ ] Configure death/unconscious states
- [ ] 📋 **Basic AI Behavior**
  - [ ] Create enemy AI with basic behaviors
  - [ ] Implement simple attack patterns
  - [ ] Add threat detection and response
  - [ ] Configure basic pathfinding for enemies

---

## 🎯 **PHASE 1 SUCCESS CRITERIA** ❌ **UNITY IMPLEMENTATION REQUIRED**
- [ ] Player can move camera with WASD smoothly using Cinemachine
- [ ] Right-click moves squad in formation to target location using NavMesh
- [ ] Space bar pauses game using Time.timeScale and allows command queuing
- [ ] F key cycles between Tight, Spread, and Wedge formations
- [ ] Basic tactical pause and slow motion functional with Unity time control
- [ ] No critical bugs in core Unity systems

---

## 🚧 **PHASE 2: SQUAD MOVEMENT & COVER INTEGRATION** (October 2025)

**Phase 2 Status: Pending Phase 1 completion**

### **Unity-Specific Implementation Goals**
- Convert UE5 cover system to Unity GameObject-based system
- Implement Unity NavMesh-based pathfinding for cover seeking
- Use Unity's layer system for cover detection
- Integrate with Unity's built-in collision detection

### **📋 Future Tasks for Phase 2**
- [ ] **Unit Cover AI Implementation (Unity)**
  - [ ] Create CoverPoint.cs MonoBehaviour for cover positions
  - [ ] Implement cover evaluation using Unity's spatial queries
  - [ ] Add automatic cover seeking behavior using NavMesh
- [ ] **Squad Cover Integration (Unity)**
  - [ ] Add V-key "take cover" squad command to Input Actions
  - [ ] Implement contextual cursor system with Unity UI
  - [ ] Create formation-cover integration logic

---

## 📈 **UPCOMING PHASES**

### **Phase 3: Character Art Pipeline (Unity)** (November 2025)
- Character creation workflow (Blender → Mixamo → Unity)
- Unity Animator Controller setup
- Production-ready marine models with Unity materials

### **Phase 4: ATB Combat System (Unity)** (December 2025)
- Unity-based ATB implementation using Coroutines
- Combat integration with Unity's built-in systems
- Weapon systems using Unity's physics

---

## 📝 **Unity-Specific Development Notes**

### **Key Unity Features to Leverage**
- **NavMesh System** - For squad movement and AI pathfinding
- **Input System** - Modern input handling with action maps
- **Cinemachine** - Professional camera control for tactical view
- **Unity Events** - For loose coupling between systems
- **ScriptableObjects** - For faction data and configuration
- **Coroutines** - For ATB timing and smooth transitions
- **Layer System** - For cover detection and line-of-sight
- **Unity UI** - For tactical interface and HUD

### **Migration Considerations**
- Convert UE5 delegates to Unity Events
- Replace UE5 components with Unity MonoBehaviour equivalents
- Use Unity's built-in physics instead of UE5 physics
- Leverage Unity's cross-platform deployment capabilities

---

## Task Commands (Copy these to use)

```bash
# View current tasks
type tasks_unity.md

# Add a new task (edit the file)  
notepad tasks_unity.md

# Mark task as done (change [ ] to [x])
# Example: - [x] Set up Unity project with Input System

# View completed tasks
findstr /C:"[x]" tasks_unity.md
```

## Legend
- 🔥 Critical/Urgent
- 📋 Important but not urgent  
- 🔧 Technical improvements
- ✅ Completed
- 🚧 In Progress
- ⏸️ Blocked/Waiting

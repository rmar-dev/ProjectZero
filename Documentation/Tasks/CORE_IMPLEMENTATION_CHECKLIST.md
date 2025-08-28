# ProjectZero - Core Implementation Checklist
## **Immediate Action Plan for Unity Foundation**

**Sprint Goal:** Get playable squad movement with tactical pause working in Unity  
**Target Date:** September 14, 2025  
**Status:** Ready to Begin Implementation  

---

## 🎯 **CRITICAL PATH TASKS** (Must be done in order)

### **STEP 1: Unity Project Setup** ⏱️ 2-3 hours

#### **Create Unity Project**
- [ ] **Open Unity Hub**
- [ ] **Create New Project**: 3D (Built-in Render Pipeline)
- [ ] **Name**: ProjectZero
- [ ] **Location**: `C:\Users\ricma\ProjectZero\UnityProject\`

#### **Install Required Packages**
```
Window → Package Manager → Install:
- [ ] Input System (com.unity.inputsystem)
- [ ] Cinemachine (com.unity.cinemachine) 
- [ ] AI Navigation (com.unity.ai.navigation)
```

#### **Create Folder Structure**
```
Assets/_Project/
- [ ] Scripts/Core/
- [ ] Scripts/Squad/  
- [ ] Scripts/Camera/
- [ ] Scripts/Characters/
- [ ] Scripts/Input/
- [ ] Prefabs/
- [ ] Scenes/
- [ ] Input/
- [ ] Materials/
```

---

### **STEP 2: Core State Management** ⏱️ 3-4 hours

#### **GameStateManager.cs** 
**Location**: `Assets/_Project/Scripts/Core/GameStateManager.cs`

**Implementation Checklist:**
- [ ] **Create singleton pattern** with `Instance` property
- [ ] **Add GameState enum**: MainMenu, Loading, InGame, Paused, GameOver
- [ ] **Add TacticalState enum**: RealTime, TacticalPause, SlowMotion, CommandPlanning
- [ ] **Implement SetTacticalState()** method with Time.timeScale control
- [ ] **Add Unity Events**: OnGameStateChanged, OnTacticalStateChanged, OnTimeScaleChanged
- [ ] **Add DontDestroyOnLoad** for scene persistence
- [ ] **Test state transitions** work correctly

**Validation Test:**
```csharp
// In test scene, add this to a test GameObject:
private void Update()
{
    if (Input.GetKeyDown(KeyCode.T))
    {
        var state = GameStateManager.Instance.currentTacticalState;
        var newState = state == TacticalState.RealTime ? 
                      TacticalState.TacticalPause : TacticalState.RealTime;
        GameStateManager.Instance.SetTacticalState(newState);
    }
}
```

---

### **STEP 3: Input System Foundation** ⏱️ 2-3 hours

#### **Input Actions Asset**
**Location**: `Assets/_Project/Input/SquadControls.inputactions`

**Configuration Checklist:**
- [ ] **Create Input Actions asset**
- [ ] **Add Action Map: "TacticalControls"**
  - [ ] TacticalPause (Button): Space
  - [ ] SlowMotion (Button): Left Shift  
  - [ ] FormationCycle (Button): F
- [ ] **Add Action Map: "SquadCommands"**
  - [ ] RightClick (Button): Mouse Right Button
  - [ ] LeftClick (Button): Mouse Left Button  
  - [ ] MousePosition (Value): Mouse Position
- [ ] **Add Action Map: "CameraControls"**
  - [ ] MoveForward (Value): W/S Axis
  - [ ] MoveRight (Value): A/D Axis
  - [ ] Zoom (Value): Mouse Scroll Wheel
- [ ] **Generate C# Class** → SquadControls.cs

---

### **STEP 4: Camera System** ⏱️ 3-4 hours

#### **TacticalCameraController.cs**
**Location**: `Assets/_Project/Scripts/Camera/TacticalCameraController.cs`

**Implementation Checklist:**
- [ ] **Create MonoBehaviour class**
- [ ] **Add Cinemachine Virtual Camera reference**
- [ ] **Add Transform cameraTarget reference**
- [ ] **Initialize SquadControls input**
- [ ] **Implement WASD movement**:
  ```csharp
  private void HandleMovement()
  {
      Vector2 input = controls.CameraControls.Movement.ReadValue<Vector2>();
      Vector3 movement = new Vector3(input.x, 0, input.y) * moveSpeed * Time.unscaledDeltaTime;
      cameraTarget.position += movement;
  }
  ```
- [ ] **Implement zoom control** with mouse wheel
- [ ] **Add camera bounds** to prevent going outside map
- [ ] **Test smooth camera movement**

#### **Scene Camera Setup**
- [ ] **Create Empty GameObject**: "Camera Target"
- [ ] **Add Cinemachine Virtual Camera**
- [ ] **Configure Virtual Camera**:
  - Body: Framing Transposer
  - Follow: Camera Target
  - Position: Above and behind target
- [ ] **Attach TacticalCameraController** to Camera Target

---

### **STEP 5: Squad Management Core** ⏱️ 4-5 hours

#### **SquadManager.cs**
**Location**: `Assets/_Project/Scripts/Squad/SquadManager.cs`

**Implementation Checklist:**
- [ ] **Create MonoBehaviour class**
- [ ] **Add squad member List<SelectableUnit>**
- [ ] **Add FormationType enum**: Tight, Spread, Wedge
- [ ] **Implement formation calculation**:
  ```csharp
  private Vector3 CalculateFormationOffset(int memberIndex)
  {
      float angle = (360f / squadMembers.Count) * memberIndex * Mathf.Deg2Rad;
      float radius = currentFormation == FormationType.Tight ? 2f : 4f;
      return new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
  }
  ```
- [ ] **Implement SetFormation()** method
- [ ] **Add ProcessContextualCommand()** for right-click handling
- [ ] **Add command queuing** for tactical pause
- [ ] **Test formation switching** works visually

---

### **STEP 6: Character Foundation** ⏱️ 3-4 hours

#### **SelectableUnit.cs** 
**Location**: `Assets/_Project/Scripts/Characters/SelectableUnit.cs`

**Implementation Checklist:**
- [ ] **Create MonoBehaviour with [RequireComponent(typeof(NavMeshAgent))]**
- [ ] **Add unit properties**: unitName, role, moveSpeed
- [ ] **Cache NavMesh Agent** in Awake()
- [ ] **Implement MoveTo()** method:
  ```csharp
  public void MoveTo(Vector3 destination)
  {
      if (navAgent != null && navAgent.isActiveAndEnabled)
      {
          navAgent.SetDestination(destination);
      }
  }
  ```
- [ ] **Add selection highlighting** using material emission
- [ ] **Test unit movement** with NavMesh

#### **Create Unit Prefab**
- [ ] **Create Capsule GameObject**: "SelectableUnit_Basic"
- [ ] **Add SelectableUnit script**
- [ ] **Add and configure NavMesh Agent**: Speed 3.5, Angular Speed 180
- [ ] **Create prefab**: `Assets/_Project/Prefabs/SelectableUnit_Basic.prefab`

---

### **STEP 7: Input Integration** ⏱️ 4-5 hours

#### **SquadPlayerController.cs**
**Location**: `Assets/_Project/Scripts/Input/SquadPlayerController.cs`

**Implementation Checklist:**
- [ ] **Create MonoBehaviour class**
- [ ] **Initialize SquadControls input**
- [ ] **Add references to GameStateManager and SquadManager**
- [ ] **Implement OnEnable/OnDisable** for input binding
- [ ] **Add input callback methods**:
  ```csharp
  private void OnTacticalPause(InputAction.CallbackContext context)
  {
      if (context.performed)
      {
          var currentState = GameStateManager.Instance.currentTacticalState;
          var newState = currentState == TacticalState.RealTime ? 
                        TacticalState.TacticalPause : TacticalState.RealTime;
          GameStateManager.Instance.SetTacticalState(newState);
      }
  }
  ```
- [ ] **Implement right-click handling** with world position calculation
- [ ] **Add formation cycling** on F key press
- [ ] **Test all input commands** work correctly

---

### **STEP 8: Scene Integration & Testing** ⏱️ 2-3 hours

#### **MainGame Scene Setup**
**Location**: `Assets/_Project/Scenes/MainGame.unity`

**Scene Configuration Checklist:**
- [ ] **Create MainGame scene**
- [ ] **Add GameStateManager GameObject** (mark DontDestroyOnLoad)
- [ ] **Create ground plane** and mark Navigation Static
- [ ] **Bake NavMesh**: Window → AI → Navigation → Bake
- [ ] **Add SquadManager GameObject**
- [ ] **Place 3-4 SelectableUnit prefabs** in scene
- [ ] **Add units to SquadManager** squadMembers list in Inspector
- [ ] **Set up Camera Target** and Cinemachine Virtual Camera
- [ ] **Add SquadPlayerController** to a GameObject

#### **Final Integration Test**
- [ ] **Press Play**
- [ ] **Test WASD camera movement**
- [ ] **Test Space bar tactical pause** (check time scale changes)
- [ ] **Test F key formation cycling**
- [ ] **Test right-click squad movement**
- [ ] **Verify no errors in Console**

---

## 🏆 **DEFINITION OF DONE**

### **Core Foundation Complete When:**
1. ✅ **Unity project opens** without errors
2. ✅ **All core scripts compile** successfully  
3. ✅ **Camera moves smoothly** with WASD input
4. ✅ **Space bar pauses/unpauses** game (Time.timeScale = 0/1)
5. ✅ **F key cycles formations** (visible unit repositioning)
6. ✅ **Right-click moves squad** in formation to target location
7. ✅ **Units pathfind correctly** using NavMesh
8. ✅ **No critical bugs or errors** in Console

---

## 📋 **ESTIMATED TIME INVESTMENT**

| Task | Time Estimate | Dependencies |
|------|---------------|--------------|
| Unity Project Setup | 2-3 hours | None |
| GameStateManager | 3-4 hours | Project Setup |
| Input System | 2-3 hours | Project Setup |
| Camera System | 3-4 hours | Input System, GameStateManager |
| Squad Management | 4-5 hours | GameStateManager |
| Character Foundation | 3-4 hours | Squad Management |
| Input Integration | 4-5 hours | All previous systems |
| Scene Integration | 2-3 hours | All systems complete |

**Total Estimated Time: 23-31 hours (3-4 full development days)**

---

## 🚀 **IMMEDIATE NEXT ACTIONS**

### **Start Today:**
1. 🏃‍♂️ **Create Unity Project** (30 minutes)
2. 🏃‍♂️ **Set up folder structure** (15 minutes)
3. 🏃‍♂️ **Install packages** (15 minutes)
4. 🏃‍♂️ **Create GameStateManager.cs** (2 hours)

### **This Week Priority Order:**
1. **Day 1**: Project setup + GameStateManager
2. **Day 2**: Input System + basic scene setup  
3. **Day 3**: Camera system + basic movement
4. **Day 4**: Squad management + character foundation
5. **Day 5**: Integration testing + polish

---

**Ready to build the core? Start with Step 1! 🚀**

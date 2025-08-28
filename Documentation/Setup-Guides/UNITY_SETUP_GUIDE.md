# ProjectZero - Unity Setup Guide

## Prerequisites
- Unity 2022.3 LTS or newer
- Unity Input System package
- Cinemachine package  
- Unity NavMesh Components
- Visual Studio 2022 or JetBrains Rider

## Step 1: Create Unity Project

### 1.1 New Unity Project Setup
```bash
# Create new Unity project (use Unity Hub)
# Template: 3D (Built-in Render Pipeline)
# Project Name: ProjectZero
# Location: C:\Users\ricma\ProjectZero\UnityProject
```

### 1.2 Install Required Packages
1. Open **Package Manager** (Window → Package Manager)
2. Install these packages:
   - **Input System** (com.unity.inputsystem)
   - **Cinemachine** (com.unity.cinemachine) 
   - **NavMesh Components** (com.unity.ai.navigation)
   - **ProBuilder** (optional, for level prototyping)

### 1.3 Configure Project Settings
```csharp
// In Project Settings
1. Input System Package → Both (Old and New Input System)
2. Player Settings → Configuration → Scripting Backend → Mono
3. XR Settings → Initialize XR on Startup → Unchecked
```

## Step 2: Project Folder Structure

### 2.1 Create Asset Organization
```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/
│   │   ├── Squad/
│   │   ├── Combat/
│   │   ├── Camera/
│   │   ├── UI/
│   │   └── Utilities/
│   ├── Prefabs/
│   │   ├── Characters/
│   │   ├── Squad/
│   │   ├── Cover/
│   │   └── UI/
│   ├── Scenes/
│   ├── Materials/
│   ├── Input/
│   └── Art/
├── ThirdParty/
└── Testing/
```

### 2.2 Create Assembly Definitions (Optional but Recommended)
Create `.asmdef` files for faster compilation:
- `_Project/Scripts/ProjectZero.Core.asmdef`
- `_Project/Scripts/ProjectZero.Squad.asmdef`
- `_Project/Scripts/ProjectZero.Combat.asmdef`

## Step 3: Input System Setup

### 3.1 Create Input Actions Asset
1. **Create Input Actions**: `Assets/_Project/Input/SquadControls.inputactions`
2. **Add Action Maps**: 
   - TacticalControls
   - SquadCommands  
   - CameraControls

### 3.2 Configure Input Actions
```csharp
// TacticalControls Action Map
- TacticalPause (Button): Space
- SlowMotion (Button): Left Shift  
- FormationCycle (Button): F
- TakeCover (Button): V

// SquadCommands Action Map
- RightClick (Button): Mouse Right Button
- LeftClick (Button): Mouse Left Button
- HoldPosition (Button): H
- MousePosition (Value): Mouse Position

// CameraControls Action Map  
- MoveForward (Value): W/S Axis
- MoveRight (Value): A/D Axis
- Zoom (Value): Mouse Scroll Wheel
- Rotate (Value): Q/E Axis
- MiddleMouseDrag (Button): Mouse Middle Button
```

### 3.3 Generate Input Action Class
1. Select `SquadControls.inputactions`
2. Inspector → **Generate C# Class** ✓
3. Class Name: `SquadControls`
4. This creates `SquadControls.cs` for easy code access

## Step 4: Core System Implementation

### 4.1 GameStateManager Singleton
Create `Assets/_Project/Scripts/Core/GameStateManager.cs`:

```csharp
using UnityEngine;
using UnityEngine.Events;

public class GameStateManager : MonoBehaviour
{
    private static GameStateManager _instance;
    public static GameStateManager Instance => _instance;

    [Header("Game States")]
    public GameState currentGameState = GameState.MainMenu;
    public TacticalState currentTacticalState = TacticalState.RealTime;

    [Header("Time Control")]
    public float slowMotionScale = 0.3f;
    public float pauseScale = 0f;
    public float normalScale = 1f;

    [Header("Events")]
    public UnityEvent<GameState> OnGameStateChanged;
    public UnityEvent<TacticalState> OnTacticalStateChanged;
    public UnityEvent<float> OnTimeScaleChanged;

    private void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetTacticalState(TacticalState newState)
    {
        currentTacticalState = newState;
        UpdateTimeScale(newState);
        OnTacticalStateChanged?.Invoke(newState);
    }

    private void UpdateTimeScale(TacticalState state)
    {
        float targetScale = state switch
        {
            TacticalState.Paused => pauseScale,
            TacticalState.SlowMotion => slowMotionScale,
            _ => normalScale
        };
        
        Time.timeScale = targetScale;
        OnTimeScaleChanged?.Invoke(targetScale);
    }
}

public enum GameState
{
    MainMenu,
    Loading,
    InGame,
    Paused,
    GameOver
}

public enum TacticalState  
{
    RealTime,
    TacticalPause,
    SlowMotion,
    CommandPlanning
}
```

### 4.2 Squad Manager Component
Create `Assets/_Project/Scripts/Squad/SquadManager.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SquadManager : MonoBehaviour
{
    [Header("Squad Configuration")]
    public List<SelectableUnit> squadMembers = new List<SelectableUnit>();
    public FormationType currentFormation = FormationType.Tight;
    
    [Header("Formation Settings")]
    public float tightRadius = 2f;
    public float spreadRadius = 4f;
    public float wedgeSpacing = 3f;

    private Queue<SquadCommand> commandQueue = new Queue<SquadCommand>();
    
    public void ProcessContextualCommand(Vector3 worldPosition, GameObject targetObject = null)
    {
        SquadCommand command = DetermineCommand(worldPosition, targetObject);
        
        if (GameStateManager.Instance.currentTacticalState == TacticalState.Paused)
        {
            QueueCommand(command);
        }
        else
        {
            ExecuteCommand(command);
        }
    }
    
    public void SetFormation(FormationType newFormation)
    {
        currentFormation = newFormation;
        UpdateFormationPositions();
    }
    
    private void UpdateFormationPositions()
    {
        for (int i = 0; i < squadMembers.Count; i++)
        {
            Vector3 offset = CalculateFormationOffset(i, currentFormation);
            Vector3 targetPosition = transform.position + offset;
            squadMembers[i].MoveTo(targetPosition);
        }
    }
}

public enum FormationType
{
    Tight,
    Spread,  
    Wedge
}
```

### 4.3 Selectable Unit Character
Create `Assets/_Project/Scripts/Characters/SelectableUnit.cs`:

```csharp
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SelectableUnit : MonoBehaviour
{
    [Header("Unit Configuration")]
    public string unitName;
    public UnitRole role;
    
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 180f;
    
    private NavMeshAgent navAgent;
    private Health healthComponent;
    
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        healthComponent = GetComponent<Health>();
        
        // Configure NavMesh Agent
        navAgent.speed = moveSpeed;
        navAgent.angularSpeed = rotationSpeed;
    }
    
    public void MoveTo(Vector3 destination)
    {
        if (navAgent != null)
        {
            navAgent.SetDestination(destination);
        }
    }
    
    public void Stop()
    {
        if (navAgent != null)
        {
            navAgent.ResetPath();
        }
    }
}

public enum UnitRole
{
    Rifleman,
    Sniper,
    Support,
    Heavy,
    Medic
}
```

## Step 5: Camera System Setup

### 5.1 Cinemachine Virtual Camera
1. **GameObject → Cinemachine → Virtual Camera**
2. **Configure Camera**:
   - Body: Framing Transposer
   - Aim: Do Nothing
   - Position: Above squad at tactical angle

### 5.2 Tactical Camera Controller
Create `Assets/_Project/Scripts/Camera/TacticalCameraController.cs`:

```csharp
using UnityEngine;
using Cinemachine;

public class TacticalCameraController : MonoBehaviour
{
    [Header("Camera References")]
    public CinemachineVirtualCamera virtualCamera;
    public Transform cameraTarget;
    
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    
    [Header("Bounds")]
    public Vector2 mapBounds = new Vector2(50f, 50f);
    
    private SquadControls controls;
    private CinemachineFramingTransposer framingTransposer;
    
    private void Awake()
    {
        controls = new SquadControls();
        framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }
    
    private void OnEnable()
    {
        controls.Enable();
        controls.CameraControls.MoveForward.performed += OnMoveForward;
        controls.CameraControls.MoveRight.performed += OnMoveRight;
        controls.CameraControls.Zoom.performed += OnZoom;
    }
    
    private void OnDisable()
    {
        controls.Disable();
    }
    
    // Implement movement methods here...
}
```

## Step 6: Scene Setup

### 6.1 Create Main Game Scene
1. **Create Scene**: `Assets/_Project/Scenes/MainGame.unity`
2. **Add GameStateManager**: Create empty GameObject, add GameStateManager script
3. **Add Squad Manager**: Create empty GameObject, add SquadManager script
4. **Configure Lighting**: Use URP if needed, or optimize built-in lighting

### 6.2 NavMesh Baking
1. **Window → AI → Navigation**
2. **Select ground objects** → Navigation Static ✓
3. **Bake tab → Bake Navigation Mesh**
4. **Verify blue NavMesh overlay** appears on walkable surfaces

### 6.3 Test Character Setup  
1. **Create Capsule GameObject** (temporary character)
2. **Add Components**:
   - SelectableUnit script
   - NavMeshAgent component
   - Health script (create basic version)
3. **Configure NavMeshAgent**: Speed 3.5, Angular Speed 180
4. **Add to SquadManager** squadMembers list

## Step 7: Testing & Validation

### 7.1 Basic Functionality Test
1. **Play Mode Testing**:
   - WASD camera movement with Cinemachine
   - Right-click squad movement with NavMesh
   - Space bar tactical pause with Time.timeScale
   - F key formation cycling
   - Mouse wheel zoom

### 7.2 Debug Information Setup
Create simple UI Canvas with Text elements showing:
- Current Game State
- Current Tactical State  
- Current Time Scale
- Squad Size
- Current Formation

## Step 8: Performance Optimization

### 8.1 Initial Optimization Settings
```csharp
// Performance recommendations
1. NavMesh Agent → Obstacle Avoidance → Quality: Medium
2. Quality Settings → V Sync Count → Every Second V Blank
3. Player Settings → Color Space → Linear (for better lighting)
4. Camera → Rendering → HDR → Off (unless needed)
```

## Troubleshooting

### Common Unity Issues:

**1. Input System Not Working**
- Verify Input System package installed
- Check Project Settings → Input System Package active
- Ensure Input Actions asset properly configured

**2. NavMesh Agent Not Moving**
- Verify NavMesh is baked in scene
- Check Agent → Area Mask includes walkable areas
- Ensure destination is on NavMesh (blue areas)

**3. Cinemachine Camera Issues**  
- Verify CinemachineVirtualCamera has target
- Check Framing Transposer settings
- Ensure camera target GameObject exists

**4. Scripts Not Compiling**
- Check Console for compilation errors
- Verify all using statements are correct
- Ensure proper namespace and class naming

### Debug Commands (Console):
```csharp
// In Unity Console, you can use:
Debug.Log("Current State: " + GameStateManager.Instance.currentGameState);
// Add debug logging throughout your scripts
```

## Next Steps After Basic Setup

1. **Test all input controls** systematically
2. **Tune movement and camera** settings for feel
3. **Add visual feedback** (selection highlights, formation indicators)
4. **Implement cover point system** with Unity GameObjects
5. **Add health bars and UI** elements  
6. **Create enemy AI** with basic behavior
7. **Performance testing** with multiple units

## Unity-Specific Best Practices

### **Performance**
- Use object pooling for frequently spawned objects
- Implement LOD (Level of Detail) for characters at distance
- Cache component references in Awake()
- Use Unity's Job System for expensive calculations

### **Code Organization**  
- One MonoBehaviour class per file
- Use SerializeField for inspector exposure
- Group related functionality in components
- Use ScriptableObjects for configuration data

### **Input Handling**
- Use Input Actions instead of old Input.GetKey()
- Implement proper input buffering for tactical pause
- Consider accessibility in input design

This setup should give you a solid Unity foundation for implementing your tactical squad-based extraction shooter!

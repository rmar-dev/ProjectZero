# ProjectZero - Quick Implementation Reference
## **Copy-Paste Ready Code for Core Systems**

---

## 🚀 **1. GameStateManager.cs** (Singleton)

```csharp
using UnityEngine;
using UnityEngine.Events;

public class GameStateManager : MonoBehaviour
{
    private static GameStateManager _instance;
    public static GameStateManager Instance => _instance;

    [Header("Current States")]
    public GameState currentGameState = GameState.MainMenu;
    public TacticalState currentTacticalState = TacticalState.RealTime;

    [Header("Time Control")]
    public float slowMotionScale = 0.3f;
    public float pauseScale = 0f;
    public float normalScale = 1f;

    [Header("Unity Events")]
    public UnityEvent<GameState> OnGameStateChanged;
    public UnityEvent<TacticalState> OnTacticalStateChanged;
    public UnityEvent<float> OnTimeScaleChanged;

    private void Awake()
    {
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
        if (currentTacticalState == newState) return;
        
        currentTacticalState = newState;
        UpdateTimeScale(newState);
        OnTacticalStateChanged?.Invoke(newState);
        
        Debug.Log($"Tactical State changed to: {newState}, Time Scale: {Time.timeScale}");
    }

    private void UpdateTimeScale(TacticalState state)
    {
        float targetScale = state switch
        {
            TacticalState.TacticalPause => pauseScale,
            TacticalState.SlowMotion => slowMotionScale,
            _ => normalScale
        };
        
        Time.timeScale = targetScale;
        OnTimeScaleChanged?.Invoke(targetScale);
    }

    public void SetGameState(GameState newState)
    {
        currentGameState = newState;
        OnGameStateChanged?.Invoke(newState);
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

---

## 🎮 **2. SelectableUnit.cs** (Character)

```csharp
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SelectableUnit : MonoBehaviour
{
    [Header("Unit Configuration")]
    public string unitName = "Marine";
    public UnitRole role = UnitRole.Rifleman;
    public float moveSpeed = 3.5f;
    
    [Header("Status")]
    public bool isSelected = false;
    
    private NavMeshAgent navAgent;
    private Renderer unitRenderer;
    private Material originalMaterial;
    private Material highlightMaterial;
    
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        unitRenderer = GetComponent<Renderer>();
        
        // Configure NavMesh Agent
        navAgent.speed = moveSpeed;
        navAgent.angularSpeed = 180f;
        
        // Set up materials for highlighting
        originalMaterial = unitRenderer.material;
        highlightMaterial = new Material(originalMaterial);
        highlightMaterial.SetColor("_EmissionColor", Color.yellow * 0.5f);
    }
    
    public void MoveTo(Vector3 destination)
    {
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.SetDestination(destination);
            Debug.Log($"{unitName} moving to {destination}");
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        unitRenderer.material = selected ? highlightMaterial : originalMaterial;
    }
    
    public void Stop()
    {
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.ResetPath();
        }
    }
    
    public bool IsMoving()
    {
        return navAgent != null && navAgent.hasPath && navAgent.remainingDistance > 0.1f;
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

---

## 👥 **3. SquadManager.cs** (Squad Control)

```csharp
using System.Collections.Generic;
using UnityEngine;

public class SquadManager : MonoBehaviour
{
    [Header("Squad Configuration")]
    public List<SelectableUnit> squadMembers = new List<SelectableUnit>();
    public FormationType currentFormation = FormationType.Tight;
    
    [Header("Formation Settings")]
    public float tightRadius = 2f;
    public float spreadRadius = 4f;
    public float wedgeSpacing = 3f;
    
    [Header("Movement")]
    public Transform squadCenter;
    
    private void Start()
    {
        if (squadCenter == null)
            squadCenter = transform;
            
        // Initialize squad
        UpdateFormationPositions();
        
        Debug.Log($"Squad initialized with {squadMembers.Count} members");
    }
    
    public void ProcessContextualCommand(Vector3 worldPosition)
    {
        Debug.Log($"Squad moving to: {worldPosition}");
        
        // Update squad center position
        squadCenter.position = worldPosition;
        
        // Move all units in formation
        UpdateFormationPositions();
    }
    
    public void SetFormation(FormationType newFormation)
    {
        currentFormation = newFormation;
        UpdateFormationPositions();
        Debug.Log($"Formation changed to: {newFormation}");
    }
    
    public void CycleFormation()
    {
        FormationType newFormation = currentFormation switch
        {
            FormationType.Tight => FormationType.Spread,
            FormationType.Spread => FormationType.Wedge,
            FormationType.Wedge => FormationType.Tight,
            _ => FormationType.Tight
        };
        SetFormation(newFormation);
    }
    
    private void UpdateFormationPositions()
    {
        for (int i = 0; i < squadMembers.Count; i++)
        {
            Vector3 offset = CalculateFormationOffset(i);
            Vector3 targetPosition = squadCenter.position + offset;
            squadMembers[i].MoveTo(targetPosition);
        }
    }
    
    private Vector3 CalculateFormationOffset(int memberIndex)
    {
        switch (currentFormation)
        {
            case FormationType.Tight:
                float tightAngle = (360f / squadMembers.Count) * memberIndex * Mathf.Deg2Rad;
                return new Vector3(
                    Mathf.Cos(tightAngle) * tightRadius,
                    0,
                    Mathf.Sin(tightAngle) * tightRadius
                );
                
            case FormationType.Spread:
                float spreadAngle = (360f / squadMembers.Count) * memberIndex * Mathf.Deg2Rad;
                return new Vector3(
                    Mathf.Cos(spreadAngle) * spreadRadius,
                    0,
                    Mathf.Sin(spreadAngle) * spreadRadius
                );
                
            case FormationType.Wedge:
                if (memberIndex == 0)
                    return Vector3.zero; // Leader at center
                
                int side = (memberIndex % 2 == 1) ? -1 : 1;
                int rank = (memberIndex + 1) / 2;
                return new Vector3(side * wedgeSpacing, 0, -rank * wedgeSpacing);
                
            default:
                return Vector3.zero;
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

---

## 🎥 **4. TacticalCameraController.cs** (Cinemachine)

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
        
        if (virtualCamera != null)
        {
            framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        
        if (cameraTarget == null)
        {
            cameraTarget = transform;
        }
    }
    
    private void OnEnable()
    {
        controls.Enable();
    }
    
    private void OnDisable()
    {
        controls.Disable();
    }
    
    private void Update()
    {
        HandleCameraMovement();
        HandleCameraZoom();
        EnforceBounds();
    }
    
    private void HandleCameraMovement()
    {
        Vector2 input = controls.CameraControls.MoveForward.ReadValue<float>() * Vector2.up + 
                       controls.CameraControls.MoveRight.ReadValue<float>() * Vector2.right;
        
        Vector3 movement = new Vector3(input.x, 0, input.y) * moveSpeed * Time.unscaledDeltaTime;
        cameraTarget.position += movement;
    }
    
    private void HandleCameraZoom()
    {
        float zoomInput = controls.CameraControls.Zoom.ReadValue<float>();
        
        if (Mathf.Abs(zoomInput) > 0.1f && framingTransposer != null)
        {
            float currentDistance = framingTransposer.m_CameraDistance;
            float newDistance = Mathf.Clamp(
                currentDistance - zoomInput * zoomSpeed * Time.unscaledDeltaTime,
                minZoom, maxZoom
            );
            framingTransposer.m_CameraDistance = newDistance;
        }
    }
    
    private void EnforceBounds()
    {
        Vector3 pos = cameraTarget.position;
        pos.x = Mathf.Clamp(pos.x, -mapBounds.x, mapBounds.x);
        pos.z = Mathf.Clamp(pos.z, -mapBounds.y, mapBounds.y);
        cameraTarget.position = pos;
    }
}
```

---

## 🕹️ **5. SquadPlayerController.cs** (Input Handler)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class SquadPlayerController : MonoBehaviour
{
    [Header("System References")]
    public SquadManager squadManager;
    public TacticalCameraController cameraController;
    public Camera mainCamera;
    
    private SquadControls controls;
    
    private void Awake()
    {
        controls = new SquadControls();
        
        // Auto-find references if not assigned
        if (squadManager == null)
            squadManager = FindObjectOfType<SquadManager>();
        if (cameraController == null)
            cameraController = FindObjectOfType<TacticalCameraController>();
        if (mainCamera == null)
            mainCamera = Camera.main;
    }
    
    private void OnEnable()
    {
        controls.Enable();
        
        // Bind input actions
        controls.TacticalControls.TacticalPause.performed += OnTacticalPause;
        controls.TacticalControls.FormationCycle.performed += OnFormationCycle;
        controls.SquadCommands.RightClick.performed += OnRightClick;
    }
    
    private void OnDisable()
    {
        controls.Disable();
        
        // Unbind input actions
        controls.TacticalControls.TacticalPause.performed -= OnTacticalPause;
        controls.TacticalControls.FormationCycle.performed -= OnFormationCycle;
        controls.SquadCommands.RightClick.performed -= OnRightClick;
    }
    
    private void OnTacticalPause(InputAction.CallbackContext context)
    {
        if (GameStateManager.Instance == null) return;
        
        var currentState = GameStateManager.Instance.currentTacticalState;
        var newState = currentState == TacticalState.RealTime ? 
                      TacticalState.TacticalPause : TacticalState.RealTime;
        
        GameStateManager.Instance.SetTacticalState(newState);
    }
    
    private void OnFormationCycle(InputAction.CallbackContext context)
    {
        if (squadManager != null)
        {
            squadManager.CycleFormation();
        }
    }
    
    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (squadManager == null || mainCamera == null) return;
        
        // Get mouse position in world space
        Vector2 mousePos = controls.SquadCommands.MousePosition.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            squadManager.ProcessContextualCommand(hit.point);
        }
    }
}
```

---

## 📋 **6. Input Actions Configuration**

**File**: `Assets/_Project/Input/SquadControls.inputactions`

### **Action Maps to Create:**

#### **TacticalControls**
```
TacticalPause (Button): Space
SlowMotion (Button): Left Shift
FormationCycle (Button): F
```

#### **SquadCommands**  
```
RightClick (Button): Mouse Right Button
LeftClick (Button): Mouse Left Button
MousePosition (Value): Mouse Position
```

#### **CameraControls**
```
MoveForward (Value): W/S Composite Axis
MoveRight (Value): A/D Composite Axis  
Zoom (Value): Mouse Scroll Wheel
```

**After creating, check "Generate C# Class" ✓**

---

## 🏗️ **7. Scene Setup Instructions**

### **MainGame Scene Checklist:**

1. **Create Scene**: `Assets/_Project/Scenes/MainGame.unity`

2. **GameStateManager Setup**:
   - Create Empty GameObject: "GameStateManager"
   - Add GameStateManager script
   - Mark "Don't Destroy On Load"

3. **Ground Setup**:
   - Create Plane GameObject (scale 10,1,10)
   - Mark "Navigation Static"
   - Window → AI → Navigation → Bake

4. **Squad Setup**:
   - Create Empty GameObject: "SquadController"  
   - Add SquadManager script
   - Create 3-4 Capsule GameObjects as test units
   - Add SelectableUnit script to each capsule
   - Add NavMeshAgent to each capsule
   - Drag capsules into SquadManager squadMembers list

5. **Camera Setup**:
   - Create Empty GameObject: "CameraTarget"
   - Position at (0, 0, 0)
   - GameObject → Cinemachine → Virtual Camera
   - Set Virtual Camera Follow → Camera Target
   - Configure: Body = Framing Transposer, distance 15
   - Add TacticalCameraController to Camera Target

6. **Input Setup**:
   - Create Empty GameObject: "InputController"
   - Add SquadPlayerController script  
   - Drag references: SquadManager, TacticalCameraController, Main Camera

---

## ✅ **8. Testing Commands**

### **Validation Tests** (Run these after implementation):

```csharp
// Add to any test GameObject for quick testing:
public class QuickTester : MonoBehaviour
{
    private void Update()
    {
        // Test state management
        if (Input.GetKeyDown(KeyCode.T))
        {
            GameStateManager.Instance.SetTacticalState(
                GameStateManager.Instance.currentTacticalState == TacticalState.RealTime ? 
                TacticalState.TacticalPause : TacticalState.RealTime);
        }
        
        // Test formation cycling
        if (Input.GetKeyDown(KeyCode.Y))
        {
            FindObjectOfType<SquadManager>()?.CycleFormation();
        }
    }
}
```

### **Console Verification**:
Expected Console output when working:
```
GameStateManager singleton initialized
Squad initialized with 4 members
Formation changed to: Spread
Tactical State changed to: TacticalPause, Time Scale: 0
Marine moving to (5.2, 0, 3.1)
```

---

## 🎯 **Implementation Priority Order**

1. **GameStateManager** → Test pause/unpause works
2. **SelectableUnit** → Test single unit moves with NavMesh  
3. **SquadManager** → Test formation calculation
4. **Input Actions** → Test input system responds
5. **TacticalCameraController** → Test camera movement
6. **SquadPlayerController** → Test right-click movement
7. **Scene Integration** → Test everything together

**Target:** 2-3 hours per system, 20-25 hours total for playable core

# ProjectZero - Tactical Camera System

## 🎯 Overview

The Tactical Camera System is a modular, compositional architecture designed for ProjectZero's top-down tactical gameplay. It provides smooth camera movement, zoom control, edge scrolling, and target following capabilities while maintaining high performance and easy maintenance.

## 🏗️ Architecture

### Compositional Design
The system uses a **composition-based architecture** where the main `TacticalCameraController` manages multiple specialized components:

- **TacticalCameraController** - Main controller and composition root
- **CameraKeyboardMovement** - WASD keyboard movement
- **CameraZoomControl** - Mouse wheel zoom functionality  
- **CameraEdgeScrolling** - Edge-of-screen scrolling
- **CameraFollowTarget** - Automatic target following
- **CameraConstraints** - Bounds enforcement and smoothing
- **CameraInputHandler** - Unity Input System integration

### Key Benefits
- ✅ **Modular** - Each component handles one responsibility
- ✅ **Testable** - Components can be unit tested independently
- ✅ **Maintainable** - Easy to modify or replace individual behaviors
- ✅ **Configurable** - Data-driven via ScriptableObject settings
- ✅ **Extensible** - New components can be added without changing existing code

## 🔧 Setup

### 1. Create Camera Settings Asset
```csharp
// Right-click in Project -> Create -> ProjectZero -> Camera -> Camera Settings
var settings = ScriptableObject.CreateInstance<CameraSettings>();
// Configure movement, zoom, edge scrolling, and bounds settings
```

### 2. Setup Scene Hierarchy
```
CameraSystem (GameObject)
├── TacticalCameraController (Component)
├── CameraKeyboardMovement (Component)  
├── CameraZoomControl (Component)
├── CameraEdgeScrolling (Component)
├── CameraFollowTarget (Component)
├── CameraConstraints (Component)
└── CameraInputHandler (Component)

CameraTarget (Empty GameObject)
└── Position for camera to control

CM VirtualCamera (Cinemachine Virtual Camera)
├── Follow: CameraTarget
└── Body: Framing Transposer
```

### 3. Configure Components
1. **TacticalCameraController**:
   - Assign CameraSettings asset
   - Assign CameraTarget transform
   - Assign Cinemachine VirtualCamera
   - Add all camera components to the components list

2. **Camera Components**:
   - Will be automatically initialized by TacticalCameraController
   - Configure individual settings as needed

### 4. Setup Input Actions
1. Assign the `CameraControls.inputactions` asset to your Input Action Asset references
2. Configure input bindings in the Input Actions editor

## 🎮 Input Controls

### Default Bindings
- **WASD / Arrow Keys** - Camera movement
- **Mouse Wheel** - Zoom in/out
- **Screen Edges** - Edge scrolling (when enabled)
- **Home Key** - Reset camera to center
- **Space** - Focus on squad

### Customization
All input can be remapped through Unity's Input System:
```csharp
// Access via CameraInputHandler component
cameraInputHandler.SetInputActions(movementAction, zoomAction, resetAction, focusAction);
```

## 🔨 Usage Examples

### Basic Setup
```csharp
// Get the camera controller
var cameraController = FindObjectOfType<TacticalCameraController>();

// Set camera position
cameraController.SetPosition(new Vector3(10, 0, 10));

// Set zoom level (0.0 = max out, 1.0 = max in)
cameraController.SetZoom(0.7f);

// Follow a target
cameraController.SetFollowTarget(squadCenterTransform);
```

### Dynamic Component Control
```csharp
// Disable edge scrolling during combat
cameraController.SetComponentEnabled<CameraEdgeScrolling>(false);

// Add a new camera component at runtime
cameraController.AddCameraComponent<CustomCameraComponent>();

// Remove a component
cameraController.RemoveCameraComponent<CameraEdgeScrolling>();
```

### Event Handling
```csharp
// Subscribe to camera events
cameraController.OnCameraPositionChanged.AddListener(OnCameraPositionChanged);
cameraController.OnZoomChanged.AddListener(OnZoomChanged);

void OnCameraPositionChanged(Vector3 newPosition)
{
    Debug.Log($"Camera moved to: {newPosition}");
}
```

## ⚙️ Configuration

### CameraSettings ScriptableObject
The camera behavior is controlled through a `CameraSettings` ScriptableObject:

```csharp
[Header("Movement Settings")]
public float MoveSpeed = 10f;           // Base movement speed
public float MovementSmoothTime = 0.3f; // Smooth movement interpolation
public bool UseAcceleration = true;     // Enable movement acceleration

[Header("Zoom Settings")]  
public float MinZoomDistance = 10f;     // Closest zoom level
public float MaxZoomDistance = 50f;     // Furthest zoom level
public float ZoomSpeed = 2f;            // Zoom sensitivity

[Header("Edge Scrolling")]
public bool EnableEdgeScrolling = true; // Enable/disable edge scrolling
public float EdgeScrollBorder = 20f;    // Edge detection zone in pixels

[Header("Bounds")]
public Vector2 MapBounds = Vector2(100, 100); // Map boundaries
public bool EnforceBounds = true;       // Enable bounds checking
```

### Component-Specific Settings
Each component can be configured independently:

```csharp
// Configure keyboard movement
var keyboardMovement = cameraController.GetComponent<CameraKeyboardMovement>();
keyboardMovement.IsEnabled = true;

// Configure zoom control
var zoomControl = cameraController.GetComponent<CameraZoomControl>();
zoomControl.SetZoom(0.5f, smooth: true);

// Configure follow behavior
var followTarget = cameraController.GetComponent<CameraFollowTarget>();
followTarget.SetFollowTarget(squadTransform, smoothTransition: true);
```

## 🧪 Testing

### Debug Features
- **Visual Gizmos** - Enable debug mode on components to see movement vectors, bounds, and targets
- **On-Screen Debug Info** - Real-time display of camera state and input values
- **Component Status** - Monitor which components are active and their priorities

### Test Scene Setup
1. Create an empty scene
2. Add the camera system as described above
3. Create some test objects to move around
4. Test all input combinations and edge cases

## 📈 Performance

### Optimization Features
- **Component Priority System** - Updates in optimal order
- **Conditional Updates** - Components skip processing when not needed
- **Efficient Input Processing** - Only broadcasts when values change
- **Smooth Interpolation** - Uses Unity's optimized SmoothDamp functions

### Best Practices
- Use `Time.unscaledDeltaTime` for pause-independent movement
- Cache component references in `Awake()`
- Enable/disable components based on game state
- Use object pooling for frequently created camera effects

## 🔮 Extension

### Creating Custom Components
```csharp
public class CustomCameraComponent : BaseCameraComponent
{
    public override int UpdatePriority => 50; // Set appropriate priority
    
    protected override void OnInitialize()
    {
        // Custom initialization logic
    }
    
    protected override void OnUpdateComponent()  
    {
        // Custom update logic - called every frame when enabled
        if (!ShouldProcessInput()) return;
        
        // Use CameraContext to access shared data
        Vector3 newPosition = CameraContext.CameraTarget.position + someMovement;
        ApplyMovementWithBounds(someMovement);
    }
}
```

### Integration with Game Systems
The camera system integrates seamlessly with ProjectZero's other systems:

```csharp
// Listen to game state changes
GameStateManager.Instance.OnTacticalStateChanged.AddListener(OnTacticalStateChanged);

void OnTacticalStateChanged(TacticalState newState)
{
    switch (newState)
    {
        case TacticalState.Paused:
            // Maybe disable certain camera components during pause
            cameraController.SetComponentEnabled<CameraEdgeScrolling>(false);
            break;
            
        case TacticalState.RealTime:
            // Re-enable all components
            cameraController.SetComponentEnabled<CameraEdgeScrolling>(true);
            break;
    }
}
```

## 🐛 Troubleshooting

### Common Issues

**Camera not moving**
- Check that Input Actions are properly assigned and enabled
- Verify CameraTarget is assigned to TacticalCameraController
- Ensure camera components are added to the components list

**Jittery movement**
- Adjust MovementSmoothTime in CameraSettings
- Check that Time.timeScale is not affecting movement
- Verify bounds constraints aren't causing conflicts

**Edge scrolling not working**
- Confirm EdgeScrolling component is enabled
- Check EdgeScrollBorder setting in CameraSettings  
- Verify mouse is actually at screen edges

**Zoom not responding**
- Confirm Cinemachine VirtualCamera is properly assigned
- Check that FramingTransposer component is attached to virtual camera
- Verify zoom Input Action is bound to mouse scroll wheel

### Debug Tools
Enable debug mode on any component to see:
- Movement vectors and velocities
- Bounds visualization
- Input state information
- Component update priorities
- Target positions and follow states

---

This camera system provides a solid foundation for ProjectZero's tactical gameplay while remaining flexible for future enhancements and modifications.

# ProjectZero - Time Dilation Controller System

## Overview

The Time Dilation Controller system provides a single source of truth for managing time scale transitions between real-time and tactical time states in ProjectZero. This system enables smooth transitions between different time scales while maintaining consistent game state management.

## System Architecture

### Core Components

1. **TimeDilationController** - The main singleton controller managing all time state transitions
2. **TimeStateManager** - Integration layer providing convenience methods and system coordination
3. **TimeDilationSettings** - ScriptableObject for designer-configurable time settings
4. **TimeDilationUI** - UI integration component for visual feedback
5. **TacticalTimeStates** - Enums and data structures defining time states

### Time States

- **RealTime** - Normal speed gameplay (1.0x time scale)
- **TacticalPause** - Complete pause for command queuing (0.0x time scale)
- **SlowMotion** - Tactical decision making mode (0.3x time scale, configurable)
- **CommandPlanning** - Ultra-slow for complex ability coordination (0.1x time scale)
- **AbilityExecution** - Variable scale for special ability executions

## Quick Setup

### 1. Create Time Dilation Settings Asset

1. Right-click in Project window
2. Create → ProjectZero → Core → Time Dilation Settings
3. Name it "DefaultTimeDilationSettings"
4. Configure the time scales and transition settings as needed

### 2. Setup Scene

Add these components to your scene:

```csharp
// Create a GameObject with TimeDilationController (will auto-create as singleton)
var controller = TimeDilationController.Instance;

// Add TimeStateManager to scene for integration
GameObject stateManager = new GameObject("TimeStateManager");
stateManager.AddComponent<TimeStateManager>();

// Optional: Add TimeDilationUI for visual feedback
GameObject ui = new GameObject("TimeDilationUI");
ui.AddComponent<TimeDilationUI>();
```

### 3. Basic Usage

```csharp
// Get the controller instance
var timeController = TimeDilationController.Instance;

// Change time states
timeController.SetTimeState(TacticalTimeState.SlowMotion, TimeStatePriority.Normal, reason: "Player Request");

// Quick methods
timeController.ToggleTacticalMode();
timeController.EnterTacticalPause("Planning Phase");
timeController.ForceRealTime("Emergency Exit");

// Check current state
if (timeController.IsInTacticalMode)
{
    // Handle tactical mode logic
}
```

## Integration with Game Systems

### ATB System Integration

```csharp
// In your ATB manager
public class ATBManager : MonoBehaviour
{
    private void Update()
    {
        float timeMultiplier = TimeDilationController.Instance.GetATBTimeMultiplier();
        UpdateATBGauges(Time.deltaTime * timeMultiplier);
    }
    
    private void OnMultipleUnitsReady(int readyCount)
    {
        var timeManager = FindObjectOfType<TimeStateManager>();
        timeManager?.OnATBUnitsReady(readyCount);
    }
}
```

### Combat System Integration

```csharp
// In your combat manager
public class CombatManager : MonoBehaviour
{
    private TimeStateManager timeManager;
    
    private void Start()
    {
        timeManager = FindObjectOfType<TimeStateManager>();
    }
    
    public void OnCombatStarted(int enemyCount)
    {
        timeManager?.OnCombatStarted(enemyCount);
    }
    
    public void OnCombatEnded()
    {
        timeManager?.OnCombatEnded();
    }
}
```

### Squad Controller Integration

```csharp
// In your squad manager
public class SquadManager : MonoBehaviour
{
    private TimeDilationController timeController;
    private Queue<SquadCommand> commandQueue = new Queue<SquadCommand>();
    
    private void Start()
    {
        timeController = TimeDilationController.Instance;
        timeController.OnStateChanged.AddListener(OnTimeStateChanged);
    }
    
    public void ProcessCommand(SquadCommand command)
    {
        if (timeController.IsPaused)
        {
            commandQueue.Enqueue(command);
        }
        else
        {
            ExecuteCommand(command);
        }
    }
    
    private void OnTimeStateChanged(TacticalTimeState newState)
    {
        if (newState == TacticalTimeState.RealTime && commandQueue.Count > 0)
        {
            ExecuteQueuedCommands();
        }
    }
}
```

## Default Input Controls

- **Space** - Toggle tactical mode
- **Tab** - Toggle tactical mode (via TimeStateManager)
- **P** - Pause/Resume
- **Left Shift** - Hold for slow motion
- **Left Ctrl** - Hold for command planning (when in tactical mode)

## Events System

### TimeDilationController Events

```csharp
controller.OnStateChanged.AddListener((TacticalTimeState newState) => {
    Debug.Log($"Time state changed to: {newState}");
});

controller.OnTimeScaleChanged.AddListener((float newScale) => {
    Debug.Log($"Time scale changed to: {newScale}");
});

controller.OnTacticalModeEntered.AddListener((TacticalTimeState state) => {
    // Handle entering tactical mode
});

controller.OnTacticalModeExited.AddListener((TacticalTimeState previousState) => {
    // Handle exiting tactical mode
});
```

### TimeStateManager Events

```csharp
manager.OnCombatStateChanged.AddListener((bool isInCombat) => {
    // Handle combat state changes
});

manager.OnATBSuggestionTriggered.AddListener(() => {
    // Handle ATB suggestions
});
```

## Configuration

### TimeDilationSettings Properties

- **Time Scales** - Configure the time scale values for each state
- **Transition Settings** - Control how transitions between states behave
- **Auto-Exit Settings** - Configure automatic timeout behavior
- **ATB Integration** - Settings for ATB system interaction
- **Debug Settings** - Logging and UI display options

### Recommended Settings

For tactical shooter gameplay:
- Real Time: 1.0x
- Slow Motion: 0.3x
- Command Planning: 0.1x
- Tactical Pause: 0.0x
- Auto-exit timeout: 8 seconds for slow motion, 15 seconds for command planning

## Performance Considerations

- Time scale transitions use coroutines for smooth interpolation
- Update frequency is configurable (default 30Hz for transitions)
- ATB suggestions have cooldowns to prevent spam
- State history is limited to prevent memory growth

## Debugging

### Debug Methods

```csharp
// Get debug information
string debugInfo = timeController.GetDebugInfo();
Debug.Log(debugInfo);

// Access state history
var history = timeController.StateHistory;
foreach (var change in history)
{
    Debug.Log($"State change: {change.fromState} -> {change.toState} | Reason: {change.reason}");
}
```

### Context Menu Commands

Right-click on TimeDilationController in inspector:
- "Reset to Real Time" - Force return to real time
- "Clear State History" - Clear debug history

## Best Practices

1. **Single Source of Truth** - Always use TimeDilationController.Instance for time state checks
2. **Use Reasons** - Provide descriptive reasons for state changes for debugging
3. **Priority System** - Use appropriate priorities to prevent conflicting state changes
4. **Event Driven** - Subscribe to events rather than polling for state changes
5. **Settings Asset** - Use the ScriptableObject settings for all configuration

## Troubleshooting

### Common Issues

1. **Time not changing** - Check if there are higher priority state changes blocking the request
2. **Input not working** - Ensure TimeStateManager is in the scene and has input handling enabled
3. **UI not updating** - Verify TimeDilationUI is properly connected to the systems
4. **Performance issues** - Reduce transition update frequency or disable debug logging

### Error Messages

- "Cannot change to [state] - insufficient priority" - Another system has set a higher priority state
- "Could not find TimeDilationController" - The singleton wasn't properly initialized
- "No settings assigned" - Need to assign a TimeDilationSettings asset

## Example Scene Setup

```csharp
// Complete scene setup example
public class GameSceneInitializer : MonoBehaviour
{
    [SerializeField] private TimeDilationSettings timeDilationSettings;
    
    private void Start()
    {
        SetupTimeDilationSystem();
    }
    
    private void SetupTimeDilationSystem()
    {
        // Get or create the controller
        var controller = TimeDilationController.Instance;
        
        // Create and setup state manager
        GameObject stateManagerGO = new GameObject("TimeStateManager");
        var stateManager = stateManagerGO.AddComponent<TimeStateManager>();
        
        // Create UI if needed
        if (FindObjectOfType<TimeDilationUI>() == null)
        {
            GameObject uiGO = new GameObject("TimeDilationUI");
            uiGO.AddComponent<TimeDilationUI>();
        }
        
        Debug.Log("Time dilation system setup complete");
    }
}
```

This system provides a robust, flexible, and performant solution for managing time dilation in your tactical extraction shooter while serving as the single source of truth for all time-related state management.

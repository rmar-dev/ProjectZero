# Tactical Time Dilation System API Documentation

## Overview
The Tactical Time Dilation System provides a simplified two-state time control mechanism for tactical gameplay. The system allows toggling between normal game speed and a slower tactical mode to enhance strategic decision-making.

## System Architecture

### Core Components
- **GameStateManager**: Central authority for time dilation state management
- **ProjectHiveConstants**: Centralized configuration constants
- **IGameStateManagerInterface**: Blueprint/interface compatibility layer
- **Player Controllers**: Input handling for tactical mode activation

---

## Constants Reference

All tactical time dilation constants are defined in `ProjectHiveConstants.h` under the `Gameplay` namespace:

### Primary Constants
```cpp
namespace ProjectHiveConstants::Gameplay 
{
    // === TIME DILATION CONSTANTS ===
    constexpr float NORMAL_TIME_SCALE = 1.0f;               // Normal game speed (100%)
    constexpr float TACTICAL_TIME_SCALE = 0.05f;            // Tactical mode speed (5%)
    constexpr float TACTICAL_MODE_DURATION = 8.0f;          // Auto-return duration in seconds
    
    // Time scale constraints for UI validation
    constexpr float MIN_TACTICAL_TIME_SCALE = 0.005f;       // Minimum allowed tactical speed (0.5%)
    constexpr float MAX_TACTICAL_TIME_SCALE = 0.2f;         // Maximum allowed tactical speed (20%)
}
```

### Legacy Constants (Deprecated)
```cpp
// These constants are provided for backward compatibility
constexpr float SLOW_MOTION_SCALE = TACTICAL_TIME_SCALE;         // Use TACTICAL_TIME_SCALE instead
constexpr float SLOW_MOTION_TIMEOUT = TACTICAL_MODE_DURATION;    // Use TACTICAL_MODE_DURATION instead
constexpr float TACTICAL_PAUSE_SCALE = 0.0f;                    // No longer used
```

---

## Enumerations

### ETacticalState
```cpp
enum class ETacticalState : uint8
{
    Normal      UMETA(DisplayName = "Normal"),      // Standard game speed (100%)
    Tactical    UMETA(DisplayName = "Tactical")     // Tactical mode speed (~5%)
};
```

### EGameState
```cpp
enum class EGameState : uint8
{
    MainMenu    UMETA(DisplayName = "Main Menu"),
    Loading     UMETA(DisplayName = "Loading"),
    InGame      UMETA(DisplayName = "In Game"),
    Paused      UMETA(DisplayName = "Paused"),
    GameOver    UMETA(DisplayName = "Game Over")
};
```

---

## Core API Reference

### UGameStateManager

#### Primary Functions

**ToggleTacticalMode()**
```cpp
UFUNCTION(BlueprintCallable, Category = "Tactical State")
void ToggleTacticalMode();
```
- Toggles between Normal and Tactical time states
- Primary function for Space key binding
- Handles all state transitions automatically

**EnterTacticalMode(float Duration = -1.0f)**
```cpp
UFUNCTION(BlueprintCallable, Category = "Tactical State")
void EnterTacticalMode(float Duration = -1.0f);
```
- Forces entry into Tactical mode
- `Duration`: Custom timeout in seconds (-1.0f uses default from settings)
- Respects `bAllowTacticalMode` setting

**ReturnToNormal()**
```cpp
UFUNCTION(BlueprintCallable, Category = "Tactical State")
void ReturnToNormal();
```
- Forces return to Normal time state
- Clears any active timeout timers
- Can be called from anywhere in code

#### State Query Functions

**IsInTacticalMode()**
```cpp
UFUNCTION(BlueprintPure, Category = "Tactical State")
bool IsInTacticalMode() const;
```
- Returns `true` if currently in Tactical state
- Primary state check function
- Use this instead of legacy functions

**GetTacticalState()**
```cpp
UFUNCTION(BlueprintPure, Category = "Tactical State")
ETacticalState GetTacticalState() const;
```
- Returns current tactical state enum value
- Useful for switch statements or detailed state logic

**GetCurrentTimeScale()**
```cpp
UFUNCTION(BlueprintPure, Category = "Time Control")
float GetCurrentTimeScale() const;
```
- Returns the current active time scale multiplier
- Will be 1.0f in Normal mode, ~0.05f in Tactical mode

#### Time Dilation Functions

**GetGameWorldDeltaTime(float RealDeltaTime)**
```cpp
UFUNCTION(BlueprintPure, Category = "Time Control")
float GetGameWorldDeltaTime(float RealDeltaTime) const;
```
- Returns scaled delta time for game world objects
- Use this for unit movement, AI, gameplay timers
- Automatically applies current time scale

**GetUnscaledDeltaTime(float RealDeltaTime)**
```cpp
UFUNCTION(BlueprintPure, Category = "Time Control")
float GetUnscaledDeltaTime(float RealDeltaTime) const;
```
- Returns unscaled real-time delta
- Use this for UI animations, camera movement, system timers
- Always returns the input parameter unchanged

#### Settings Management

**UpdateGameSettings(const FGameSettings& NewSettings)**
```cpp
UFUNCTION(BlueprintCallable, Category = "Settings")
void UpdateGameSettings(const FGameSettings& NewSettings);
```
- Updates all game settings including time dilation parameters
- Triggers appropriate system updates

---

## Settings Configuration

### FGameSettings Structure

```cpp
USTRUCT(BlueprintType)
struct FGameSettings
{
    // === TIME CONTROL SETTINGS ===
    
    // Normal game speed (should always be 1.0)
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Time Control")
    float RealTimeScale = ProjectHiveConstants::Gameplay::NORMAL_TIME_SCALE;
    
    // Tactical mode speed (0.5% to 20% range)
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Time Control", 
              meta = (ClampMin = "0.005", ClampMax = "0.2"))
    float TacticalTimeScale = ProjectHiveConstants::Gameplay::TACTICAL_TIME_SCALE;
    
    // How long tactical mode stays active before auto-returning
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Time Control")
    float TacticalModeDuration = ProjectHiveConstants::Gameplay::TACTICAL_MODE_DURATION;
    
    // Whether to automatically return to normal time
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Time Control")
    bool bAutoReturnToNormal = true;
    
    // Whether tactical mode is allowed
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Time Control")
    bool bAllowTacticalMode = true;
};
```

---

## Input Integration

### Recommended Input Setup

**Space Key (Primary)**
```cpp
// In PlayerController input binding
void OnTacticalModePressed()
{
    if (UGameStateManager* GSM = UGameStateManager::GetInstance(this))
    {
        GSM->ToggleTacticalMode();
    }
}
```

**Alternative Keys (Secondary)**
```cpp
// Shift key or other alternative inputs
void OnAlternateTacticalPressed()
{
    if (UGameStateManager* GSM = UGameStateManager::GetInstance(this))
    {
        GSM->ToggleTacticalMode();
    }
}
```

---

## Legacy Compatibility

### Deprecated Functions
These functions are maintained for backward compatibility but should not be used in new code:

```cpp
// DEPRECATED - Use ToggleTacticalMode() instead
void ToggleTacticalPause();

// DEPRECATED - Use EnterTacticalMode() instead  
void EnterSlowMotion(float Duration = -1.0f);

// DEPRECATED - Use ReturnToNormal() instead
void ReturnToRealTime();

// DEPRECATED - Use IsInTacticalMode() instead
bool IsPaused() const;
bool IsInSlowMotion() const;
```

---

## Event System

### Available Delegates

**FOnTacticalStateChanged**
```cpp
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnTacticalStateChanged, ETacticalState, NewTacticalState);
```
- Broadcasts when tactical state changes
- Useful for UI updates, audio cues, visual effects

**FOnTimeScaleChanged**
```cpp
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnTimeScaleChanged, float, NewTimeScale);
```
- Broadcasts when time scale value changes
- Useful for systems that need precise time scale information

### Usage Example
```cpp
// In BeginPlay or initialization
if (UGameStateManager* GSM = UGameStateManager::GetInstance(this))
{
    GSM->OnTacticalStateChanged.AddDynamic(this, &AMyActor::OnTacticalStateChanged);
}

// Handler function
UFUNCTION()
void AMyActor::OnTacticalStateChanged(ETacticalState NewState)
{
    if (NewState == ETacticalState::Tactical)
    {
        // Tactical mode entered - update UI, effects, etc.
    }
    else
    {
        // Normal mode restored
    }
}
```

---

## Best Practices

### Do's ✅
- **Use `ToggleTacticalMode()`** for space key and primary tactical controls
- **Use `GetGameWorldDeltaTime()`** for gameplay-affecting timers and movement
- **Use `GetUnscaledDeltaTime()`** for UI animations and camera movement
- **Check `IsInTacticalMode()`** for state-dependent logic
- **Use constants** from ProjectHiveConstants instead of magic numbers
- **Subscribe to events** for UI and visual feedback

### Don'ts ❌
- **Don't use deprecated functions** like `EnterSlowMotion()` or `ToggleTacticalPause()`
- **Don't hardcode time scale values** - use constants from ProjectHiveConstants
- **Don't apply time dilation to camera movement** - it should remain responsive
- **Don't assume tactical mode will stay active** - respect auto-timeout settings
- **Don't call `SetTimeScale()` directly** - use tactical state functions instead

### Performance Considerations
- The system uses selective time dilation by default for better performance
- Global time dilation is available as fallback but less recommended
- Event broadcasts are lightweight but avoid excessive subscriptions
- Constants are compile-time and have zero runtime cost

---

## Integration Examples

### Unit Movement Integration
```cpp
// In UnitMovementComponent::Tick()
void UUnitMovementComponent::TickComponent(float DeltaTime, ...)
{
    if (UGameStateManager* GSM = UGameStateManager::GetInstance(this))
    {
        // Use scaled delta time for movement
        float GameDeltaTime = GSM->GetGameWorldDeltaTime(DeltaTime);
        
        // Apply movement using scaled time
        UpdateMovement(GameDeltaTime);
    }
}
```

### Camera Controller Integration
```cpp
// In CameraController::Tick()
void ATacticalCameraController::Tick(float DeltaTime)
{
    if (UGameStateManager* GSM = UGameStateManager::GetInstance(this))
    {
        // Use unscaled delta time for responsive camera movement
        float CameraDeltaTime = GSM->GetUnscaledDeltaTime(DeltaTime);
        
        // Update camera with real-time responsiveness
        UpdateCameraMovement(CameraDeltaTime);
    }
}
```

### UI Integration
```cpp
// In UI Widget
void UMyTacticalWidget::NativeConstruct()
{
    if (UGameStateManager* GSM = UGameStateManager::GetInstance(this))
    {
        GSM->OnTacticalStateChanged.AddDynamic(this, &UMyTacticalWidget::OnTacticalStateChanged);
    }
}

UFUNCTION()
void UMyTacticalWidget::OnTacticalStateChanged(ETacticalState NewState)
{
    // Update UI to show tactical mode status
    TacticalModeIndicator->SetVisibility(
        NewState == ETacticalState::Tactical ? 
        ESlateVisibility::Visible : 
        ESlateVisibility::Hidden
    );
}
```

---

## Troubleshooting

### Common Issues

**Space key not working**
- Verify input action is bound to `ToggleTacticalMode()`
- Check that `bAllowTacticalMode` is true in settings
- Ensure GameStateManager singleton is properly initialized

**Movement feels unresponsive in tactical mode**
- Confirm you're using `GetGameWorldDeltaTime()` for movement
- Check that tactical time scale is not too low (< 0.005)

**Camera movement affected by time dilation**
- Use `GetUnscaledDeltaTime()` for camera movement code
- Ensure selective time dilation is enabled (default)

**Tactical mode doesn't auto-return**
- Check that `bAutoReturnToNormal` is true
- Verify `TacticalModeDuration` is > 0
- Confirm `TickGameState()` is being called regularly

---

## Migration Guide

### From Legacy Multi-Speed System

**Old Code:**
```cpp
// Old complex system
GameStateManager->CycleTacticalModeIntensity();
if (GameStateManager->GetTacticalModeType() == ETacticalModeType::SlowMotion)
{
    // Handle slow motion
}
```

**New Code:**
```cpp
// New simplified system
GameStateManager->ToggleTacticalMode();
if (GameStateManager->IsInTacticalMode())
{
    // Handle tactical mode (single state)
}
```

### Constants Migration

**Old Code:**
```cpp
float TacticalSpeed = 0.05f;  // Magic number
```

**New Code:**
```cpp
float TacticalSpeed = ProjectHiveConstants::Gameplay::TACTICAL_TIME_SCALE;
```

---

This documentation reflects the current simplified tactical time dilation system implementation and should be the primary reference for developers working with time dilation features.

# ACoverPoint Class Reference

## 📋 Overview

The `ACoverPoint` class provides tactical position markers where units can take cover and gain protection from enemy fire. This class has been enhanced to follow best practices by eliminating magic numbers and providing Blueprint-configurable properties for visual scaling, positioning, timing, and effects.

## 🏗️ Class Architecture

### Components
- **CoverDetectionComponent**: Handles cover mechanics (health, occupancy, protection calculations)
- **CoverMesh**: Main cover geometry (wall, crate, etc.) - static visual element
- **GroundMarker**: Ground positioning indicator - helps with level design
- **StatusIndicator**: Dynamic visual feedback element (color changes, hover effects)
- **InteractionVolume**: Collision box for mouse interactions
- **UnitPositionMarker**: Scene component marking where units should stand
- **AudioFeedbackComponent**: Handles cover-related sound effects

### Visual Systems
The class supports two visual approaches:
1. **Modular System** (recommended): Separate components for different visual roles
2. **Legacy System**: Single mesh component for backwards compatibility

## 🎛️ Configurable Properties

### Cover Visuals (Modular System)

#### Component Scales
All scales are configurable via Blueprint properties to eliminate magic numbers:

```cpp
// Component scaling - Blueprint configurable
UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Cover Visuals")
FVector CoverMeshScale = FVector(1.0f, 1.0f, 1.0f);

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Cover Visuals")
FVector GroundMarkerScale = FVector(0.8f, 0.8f, 0.02f);

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Cover Visuals")
FVector StatusIndicatorScale = FVector(1.1f, 1.1f, 1.1f);
```

**Purpose**: 
- `CoverMeshScale`: Controls the size of the main cover object (walls, crates, barriers)
- `GroundMarkerScale`: Controls the flat ground indicator (helps with positioning)
- `StatusIndicatorScale`: Controls the visual feedback element (slightly larger than cover for visibility)

#### Component Positioning
```cpp
// Component locations - Blueprint configurable
UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Cover Visuals")
FVector CoverMeshLocation = FVector(0.0f, 0.0f, 100.0f);

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Cover Visuals")
FVector GroundMarkerLocation = FVector(0.0f, 0.0f, 1.0f);

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Cover Visuals")
FVector StatusIndicatorLocation = FVector(0.0f, 0.0f, 100.0f);
```

**Purpose**:
- Allows precise positioning of visual elements relative to cover point origin
- Enables different cover layouts without code changes
- Supports varied art assets and visual designs

### Interaction Configuration

```cpp
// Interaction volume setup - Blueprint configurable
UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Interaction")
FVector InteractionVolumeExtent = FVector(75.0f, 75.0f, 25.0f);

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Interaction")
FVector InteractionVolumeLocation = FVector(0.0f, 0.0f, 15.0f);

// Unit positioning
UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Unit Positioning")
FVector UnitPositionOffset = FVector(0.0f, 0.0f, 0.0f);
```

**Purpose**:
- `InteractionVolumeExtent`: Defines clickable area size for mouse interactions
- `InteractionVolumeLocation`: Positions the interaction area relative to cover
- `UnitPositionOffset`: Fine-tunes where units stand relative to cover

### Visual Effects

```cpp
// Material and visual effects - Blueprint configurable
UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Visual Effects")
float EmissiveColorMultiplier = 0.5f;

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Visual")
float HoverScaleMultiplier = 1.2f;

// Color configuration
UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Status Indicator")
FLinearColor AvailableColor = FLinearColor::Green;

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Status Indicator")
FLinearColor OccupiedColor = FLinearColor::Red;

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Status Indicator")
FLinearColor HoverColor = FLinearColor::Yellow;

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Status Indicator")
FLinearColor DisabledColor = FLinearColor::Gray;
```

**Purpose**:
- `EmissiveColorMultiplier`: Controls glow intensity for visual feedback
- `HoverScaleMultiplier`: Controls how much cover grows when hovered
- Color properties: Provide clear visual state indicators

### Debug Configuration

```cpp
// Debug visualization parameters - Blueprint configurable
UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Debug")
float DebugSphereRadius = 15.0f;

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Debug")
float DebugWireframeThickness = 2.0f;

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Debug")
float DebugDisplayDuration = 30.0f;

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Debug")
float UnitMarkerSphereRadius = 25.0f;

UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Debug")
float CoordinateSystemScale = 50.0f;
```

**Purpose**:
- Provides fine control over debug visualization elements
- Allows adjustment of wireframe thickness and display durations
- Enables customization of coordinate system and marker sizes

### Internal Constants (Private)

```cpp
// Private properties to eliminate magic numbers
/** Default scale for legacy mesh component in backwards compatibility mode */
FVector DefaultLegacyMeshScale;

/** Default actor scale for consistent sizing across instances */
FVector DefaultActorScale;

/** Duration for unit assignment/removal debug messages (default: 8.0s) */
float MessageDisplayDuration;

/** Duration for general debug messages and hover feedback (default: 5.0s) */
float DebugMessageDuration;

/** Delay for test timer in hover interaction testing (default: 2.0s) */
float TestTimerDelay;
```

**Purpose**:
- Centralizes timing constants for consistency
- Provides fallback scale values for legacy support
- Eliminates hardcoded values throughout the implementation

## 🎯 Usage Examples

### Basic Setup in Blueprint

1. **Add CoverPoint to Level**:
   - Drag `ACoverPoint` or Blueprint derived from it into level
   - Position where you want tactical cover

2. **Configure Core Properties**:
   ```
   Cover Visuals:
   ├── Use Modular Visuals: ✓ Checked
   ├── Cover Mesh Scale: (2.0, 0.5, 2.0) [for wall-like cover]
   ├── Ground Marker Scale: (1.0, 1.0, 0.02) [flat ground indicator]
   └── Status Indicator Scale: (1.2, 1.2, 1.2) [slightly larger for visibility]
   ```

3. **Set Interaction Parameters**:
   ```
   Interaction:
   ├── Interaction Volume Extent: (100, 100, 50) [larger clickable area]
   └── Unit Position Offset: (0, 150, 0) [units stand 150cm in front]
   ```

### Advanced Configuration

#### Enhanced Visual Effects
```
Visual Effects:
├── Emissive Color Multiplier: 0.8 [brighter glow]
├── Hover Scale Multiplier: 1.3 [more dramatic hover effect]
└── Available Color: Bright Green [high visibility]
```

#### Debug Configuration
```
Debug:
├── Debug Sphere Radius: 20.0 [larger debug markers]
├── Debug Wireframe Thickness: 3.0 [thicker wireframes]
└── Debug Display Duration: 60.0 [longer debug text display]
```

## 🔧 Best Practices

### Property Configuration Guidelines

1. **Component Scales**:
   - Keep CoverMeshScale proportional to your art assets
   - Use small Z-scale for GroundMarkerScale (0.01-0.05) for flat appearance
   - Make StatusIndicatorScale slightly larger than CoverMeshScale for visibility

2. **Visual Effects**:
   - EmissiveColorMultiplier: 0.3-0.8 for subtle to moderate glow
   - HoverScaleMultiplier: 1.1-1.5 for noticeable but not excessive scaling
   - Use high-contrast colors for clear state indication

3. **Debug Settings**:
   - Adjust DebugSphereRadius based on cover size
   - Use thicker wireframes (2.0-4.0) for better visibility in complex levels
   - Set DebugDisplayDuration to 30-60 seconds for thorough debugging

### Performance Considerations

- **Modular System**: More components but better organization and performance
- **Legacy System**: Fewer components but less flexible
- **Debug Features**: Disable in shipping builds for optimal performance
- **Update Frequency**: Visual updates only occur on state changes

## 🎮 Integration Patterns

### With Squad System
```cpp
// In unit AI logic
ACoverPoint* BestCover = CoverManager->FindBestCoverPoint(Unit, ThreatLocation);
if (BestCover && BestCover->CanOccupy(Unit))
{
    BestCover->AssignUnit(Unit);
    Unit->MoveTo(BestCover->GetUnitPosition());
}
```

### With Combat System
```cpp
// When calculating damage
float CoverProtection = Unit->GetCoverPoint() ? 
    Unit->GetCoverPoint()->GetProtectionValue(AttackDirection) : 0.0f;
float FinalDamage = BaseDamage * (1.0f - CoverProtection);
```

### Blueprint Events
```blueprint
Event OnCoverPointClicked
→ Branch (Is Available?)
   ├── True: Show Unit Assignment UI
   └── False: Show "Cover Unavailable" Message

Event OnCoverPointHovered  
→ Show Cover Information Widget
→ Highlight Cover Protection Arc
```

## 🚀 Benefits of Enhanced Configuration

### For Developers
- **No Magic Numbers**: All values are named constants or configurable properties
- **Centralized Configuration**: Related values grouped in logical categories
- **Easy Debugging**: Consistent timing values across all debug systems
- **Maintainable Code**: Clear separation between logic and configuration

### For Designers
- **Visual Flexibility**: Adjust component scales and positions without code changes
- **Rapid Iteration**: Tweak visual effects and timing from Blueprint properties
- **Consistent Behavior**: Unified timing and scaling across all cover points
- **Debug Control**: Fine-tune debug visualization for level design workflow

### For Artists
- **Asset Independence**: Visual properties adjust to accommodate different art assets
- **Effect Control**: Material parameters and visual effects easily adjustable
- **Scale Consistency**: Proportional scaling maintains art asset integrity
- **Color Customization**: Full control over visual state indicators

## 📈 Migration Benefits

The refactoring from magic numbers to configurable properties provides:

1. **Improved Maintainability**: ~15 hardcoded values replaced with named properties
2. **Enhanced Flexibility**: Blueprint-configurable without C++ recompilation
3. **Better Documentation**: Self-documenting property names and categories
4. **Consistent Timing**: Unified debug message and animation durations
5. **Future-Proofing**: Easy to extend with additional configurable parameters

This enhanced `ACoverPoint` class now follows industry best practices while maintaining full backwards compatibility with existing systems.

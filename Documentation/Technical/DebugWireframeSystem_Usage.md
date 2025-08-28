# Debug Wireframe System Usage Guide

## 🎯 Overview

The new modular debug wireframe system allows you to add debug visualization to ANY actor by simply adding a component. The system is centrally controlled and can be toggled on/off for all objects at once.

## 🧩 Components

### 1. **UDebugWireframeComponent** - The Main Component
- Add this to any actor to enable debug visualization
- Auto-detects and visualizes collision components
- Supports custom shapes and text
- Integrates with the centralized debug controller

### 2. **ADebugController** - Central Management
- Singleton that manages all debug features
- Console commands for easy control
- Feature flags for different debug types

### 3. **IDebugWireframeProvider** - Custom Interface
- Implement this on actors for custom debug data
- Provides shapes and text dynamically

## 🚀 Quick Start

### Add Debug Wireframe to Any Actor

**In C++:**
```cpp
// In your actor's constructor
DebugWireframeComponent = CreateDefaultSubobject<UDebugWireframeComponent>(TEXT("DebugWireframe"));
```

**In Blueprint:**
1. Open your actor Blueprint
2. Add Component → Debug Wireframe Component
3. Configure settings in the Details panel

### Settings You Can Configure:
- `bAutoDetectOnBeginPlay` - Automatically show collision volumes
- `bShowActorBounds` - Show actor bounding box
- `bShowActorCenter` - Show center point marker
- `bShowComponentBounds` - Show all component bounds
- `DefaultWireframeColor` - Default color for shapes
- `DebugDuration` - How long debug shapes stay visible

## 🎮 Console Commands

**In-Game Console (press ~ key):**

```
ToggleCoverPointWireframes     # Toggle wireframes on/off
ShowDebugStatus               # Show what's enabled
EnableDebugFeature CoverPointWireframes 1   # Enable wireframes
EnableDebugFeature CoverPointWireframes 0   # Disable wireframes
```

## 📝 Adding Custom Shapes

**In C++:**
```cpp
// Get the wireframe component
UDebugWireframeComponent* WireframeComp = GetComponentByClass<UDebugWireframeComponent>();

// Add a sphere
WireframeComp->AddSphere(GetActorLocation(), 100.0f, FColor::Red, 2.0f, TEXT("Detection Range"));

// Add a box around a component
WireframeComp->AddComponentBox(MeshComponent, FColor::Green, 1.0f, TEXT("Collision"));

// Add an arrow showing direction
WireframeComp->AddArrow(Start, End, FColor::Blue, 3.0f, TEXT("Movement Direction"));

// Add coordinate system
WireframeComp->AddCoordinateSystem(GetActorLocation(), GetActorRotation(), 50.0f, TEXT("Local Axes"));
```

**In Blueprint:**
1. Get Debug Wireframe Component reference
2. Call Add Sphere/Add Component Box/etc.
3. Specify parameters in the node

## 🎨 Implementing Custom Debug Data

**Option 1: Implement the Interface in C++**

```cpp
// In YourActor.h
#include "IDebugWireframeProvider.h"

UCLASS()
class YOURGAME_API AYourActor : public AActor, public IDebugWireframeProvider
{
    GENERATED_BODY()

public:
    // Implement the interface
    virtual TArray<FWireframeShape> GetDebugWireframeShapes_Implementation() const override;
    virtual TArray<FWireframeDebugText> GetDebugTextInfo_Implementation() const override;
};

// In YourActor.cpp
TArray<FWireframeShape> AYourActor::GetDebugWireframeShapes_Implementation() const
{
    TArray<FWireframeShape> Shapes;
    
    // Create a custom shape
    FWireframeShape Shape;
    Shape.ShapeType = EWireframeShapeType::Sphere;
    Shape.Transform = FTransform(GetActorLocation());
    Shape.Size = FVector(DetectionRadius, 0, 0);
    Shape.Color = FColor::Orange;
    Shape.Label = TEXT("AI Detection");
    
    Shapes.Add(Shape);
    return Shapes;
}

TArray<FWireframeDebugText> AYourActor::GetDebugTextInfo_Implementation() const
{
    TArray<FWireframeDebugText> Texts;
    
    FWireframeDebugText DebugText;
    DebugText.Text = FString::Printf(TEXT("Health: %d\nState: %s"), Health, *CurrentState.ToString());
    DebugText.WorldPosition = GetActorLocation() + FVector(0, 0, 150);
    DebugText.Color = FColor::White;
    
    Texts.Add(DebugText);
    return Texts;
}
```

**Option 2: Blueprint Interface**

1. Open your actor Blueprint
2. Go to Class Settings → Interfaces
3. Add "Debug Wireframe Provider"
4. Implement the events:
   - Get Debug Wireframe Shapes
   - Get Debug Text Info
   - On Wireframe Debug Toggled

## 🎯 Use Cases

### For Cover Points:
- Shows collision volumes for mouse interaction debugging
- Displays cover type and occupancy status
- Visualizes unit positioning markers

### For AI Units:
- Detection/attack range visualization  
- Path planning debug lines
- Behavior state information
- Target acquisition indicators

### For Interactive Objects:
- Interaction trigger volumes
- Use prompts and status text
- Animation debug information

### For Weapons/Items:
- Damage area visualization
- Projectile trajectories
- Equipment stats display

## 🎨 Available Shape Types

- `Box` - Rectangular wireframe boxes
- `Sphere` - Spherical wireframes  
- `Capsule` - Capsule shapes (character collision)
- `Line` - Simple lines between points
- `Arrow` - Directional arrows
- `Cone` - Cone shapes (vision/detection areas)
- `CoordinateSystem` - XYZ axis display

## 📊 Debug Controller Features

**Central Control:**
- All wireframe components register automatically
- Global on/off switches
- Per-feature type control (future expansion)
- Console command integration
- Blueprint accessible functions

**Extensible:**
- Easy to add new debug feature types
- Modular component architecture
- Interface-based custom data
- Runtime configuration

## 🔧 Benefits

✅ **Modular** - Add to any actor with one component
✅ **Centralized** - Control all debug features from one place  
✅ **Extensible** - Easy to add new debug types
✅ **Efficient** - Only active when debug mode is on
✅ **Flexible** - Custom shapes, colors, durations
✅ **Team-Friendly** - Console commands for designers/QA
✅ **Clean** - Debug code separated from game logic

## 🎮 Perfect for Mouse Interaction Debug!

The system is ideal for debugging your mouse hover issues:

1. Add `UDebugWireframeComponent` to your cover points
2. Use `ToggleCoverPointWireframes` console command  
3. See exact collision volumes and verify they match your expectations
4. Check if interaction volumes are properly positioned
5. Verify that the debug controller shows the right number of registered objects

This modular system replaces the old inline wireframe code and makes debug visualization available to ALL your actors! 🚀

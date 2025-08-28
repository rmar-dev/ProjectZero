# 3D Grid Pathfinding System - Implementation Guide

## 📋 Overview

This document covers the initial implementation of the 3D Grid Pathfinding System for ProjectHive 5.5, corresponding to **Task 1.1: Grid Data Structure Implementation** from our task breakdown.

## 🏗️ Module Structure

The pathfinding system is implemented as a separate C++ module located at:
```
Source/Pathfinding/
├── Pathfinding.Build.cs          # Module build configuration
├── Public/
│   ├── Pathfinding.h             # Module interface
│   ├── Grid3DNode.h              # Grid node data structures
│   └── Grid3DManager.h           # Main grid management class
└── Private/
    ├── Pathfinding.cpp           # Module implementation
    ├── Grid3DNode.cpp            # Grid node implementations
    └── Grid3DManager.cpp         # Grid manager implementation
```

## 📊 Core Data Structures

### FGrid3DNode

The fundamental building block of the grid system:

```cpp
USTRUCT(BlueprintType)
struct PATHFINDING_API FGrid3DNode
{
    // Grid coordinates
    int32 X, Y, Z;
    
    // Pathfinding properties
    bool bIsWalkable;
    float MovementCost;
    float TerrainModifier;
    
    // A* algorithm properties
    float GCost, HCost, FCost;
    FGrid3DNode* ParentNode;
    
    // Game state
    bool bIsOccupied;
    TWeakObjectPtr<AActor> OccupyingActor;
    FVector WorldPosition;
};
```

**Key Features:**
- Complete A* pathfinding support
- Game state tracking (occupation, obstacles)
- World-to-grid coordinate conversion
- Blueprint accessibility

### FGrid3DCoordinate

Lightweight coordinate structure for grid operations:

```cpp
USTRUCT(BlueprintType)
struct PATHFINDING_API FGrid3DCoordinate
{
    int32 X, Y, Z;
    // Operators and utility functions
};
```

### FGrid3DConfiguration

Configuration structure for grid setup:

```cpp
USTRUCT(BlueprintType)
struct PATHFINDING_API FGrid3DConfiguration
{
    // Grid dimensions
    int32 GridWidth = 50;
    int32 GridLength = 50;
    int32 GridHeight = 10;
    
    // Spacing and positioning
    float NodeSpacing = 100.0f;
    FVector GridOrigin = FVector::ZeroVector;
    
    // Movement costs
    float HorizontalMovementCost = 1.0f;
    float VerticalMovementCost = 1.5f;
    float DiagonalMovementCost = 1.414f;
};
```

## 🎮 AGrid3DManager Class

The main actor class that manages the entire grid system.

### Key Features

1. **Grid Initialization**
   - Automatic grid creation on BeginPlay
   - Configurable dimensions and spacing
   - Obstacle detection integration

2. **Coordinate Conversion**
   - `WorldToGrid()` - Convert world position to grid coordinates
   - `GridToWorld()` - Convert grid coordinates to world position
   - Automatic position caching

3. **Node Access**
   - `GetNodeAt()` - Direct node access by coordinates
   - `GetNodeAtWorldPosition()` - Node access by world position
   - Validation functions

4. **Grid Modification**
   - `SetNodeWalkable()` - Mark nodes as walkable/unwalkable
   - `SetNodeMovementCost()` - Adjust movement costs
   - `SetNodeOccupied()` - Track unit occupation

5. **Pathfinding Support**
   - `GetNeighbors()` - 6-directional or 26-directional movement
   - `GetDistance()` - Euclidean distance calculation
   - `GetManhattanDistance()` - Manhattan distance calculation

6. **Obstacle Detection**
   - `RebuildGridObstacles()` - Full grid obstacle update
   - `UpdateObstaclesInRadius()` - Localized updates
   - Collision system integration

7. **Debug Visualization**
   - Real-time grid visualization
   - Color-coded node states
   - Coordinate display
   - Performance-optimized rendering

## 🚀 Usage Instructions

### 1. Basic Setup

1. Add a `Grid3DManager` actor to your level
2. Configure the grid settings in the Details panel:
   - Grid dimensions (Width, Length, Height)
   - Node spacing (world units between nodes)
   - Grid origin offset

### 2. Blueprint Usage

The system is fully Blueprint-accessible:

```cpp
// Get the grid manager (in Blueprint)
AGrid3DManager* GridManager = GetWorld()->SpawnActor<AGrid3DManager>();

// Convert world position to grid
FGrid3DCoordinate GridPos = GridManager->WorldToGrid(ActorLocation);

// Get node at position
FGrid3DNode* Node = GridManager->GetNodeAt(GridPos);

// Check if position is walkable
bool bCanWalk = Node && Node->IsValid();
```

### 3. C++ Usage

```cpp
// Example: Mark area around actor as occupied
void MarkAreaOccupied(AActor* Actor, AGrid3DManager* GridManager)
{
    if (!Actor || !GridManager)
        return;
        
    FVector ActorLocation = Actor->GetActorLocation();
    FGrid3DCoordinate GridPos = GridManager->WorldToGrid(ActorLocation);
    
    // Mark node as occupied
    GridManager->SetNodeOccupied(GridPos, true, Actor);
}
```

### 4. Debug Visualization

Enable visualization through:
- Set `bShowGridDebugVisualization = true` in editor
- Call `ToggleGridVisualization()` at runtime
- Use console command: `showdebug pathfinding` (when implemented)

## 🔧 Configuration Options

### Grid Dimensions
- **GridWidth/GridLength**: Horizontal grid size (recommended: 50-100)
- **GridHeight**: Vertical layers (recommended: 5-20)
- **NodeSpacing**: Distance between nodes in world units (recommended: 100.0f)

### Movement Costs
- **HorizontalMovementCost**: Cost for X/Y movement (default: 1.0f)
- **VerticalMovementCost**: Cost for Z movement (default: 1.5f)
- **DiagonalMovementCost**: Cost for diagonal movement (default: 1.414f)

### Performance Tuning
- Smaller grids = better performance
- Larger node spacing = fewer nodes = better performance
- Disable visualization in shipping builds

## ⚡ Performance Considerations

### Memory Usage
- Each node uses ~100 bytes
- 50x50x10 grid = ~12.5MB memory
- Uses efficient 1D array storage with hash map lookups

### CPU Performance
- Grid initialization: O(n³) where n is average dimension
- Node access: O(1) via hash map
- Neighbor queries: O(26) worst case
- Obstacle updates: O(nodes in region)

### Optimization Tips
1. Use appropriate grid dimensions for your level size
2. Update obstacles incrementally rather than full rebuilds
3. Disable debug visualization in shipping builds
4. Consider LOD system for very large grids

## 🐛 Debug Features

### Console Commands (Future)
```
pathfinding.toggle_visualization    # Toggle grid visualization
pathfinding.print_info             # Print grid statistics
pathfinding.rebuild_obstacles      # Force obstacle rebuild
```

### Debug Colors
- **Green**: Walkable nodes
- **Red**: Unwalkable/blocked nodes  
- **Orange**: Occupied nodes

### Logging
The system provides detailed logging:
- Grid initialization status
- Node counts and statistics
- Performance warnings

## 🔄 Integration Points

### With Squad System
```cpp
// Example: Check if formation position is valid
bool IsFormationPositionValid(const FVector& WorldPos, AGrid3DManager* GridManager)
{
    FGrid3DNode* Node = GridManager->GetNodeAtWorldPosition(WorldPos);
    return Node && Node->IsValid();
}
```

### With Combat System
```cpp
// Example: Get valid cover positions around target
TArray<FVector> GetCoverPositions(const FVector& TargetPos, AGrid3DManager* GridManager)
{
    TArray<FVector> CoverPositions;
    FGrid3DCoordinate TargetGrid = GridManager->WorldToGrid(TargetPos);
    
    TArray<FGrid3DNode*> Neighbors = GridManager->GetNeighbors(TargetGrid, true);
    for (FGrid3DNode* Neighbor : Neighbors)
    {
        if (Neighbor && Neighbor->IsValid())
        {
            CoverPositions.Add(Neighbor->WorldPosition);
        }
    }
    
    return CoverPositions;
}
```

## 📈 Next Steps

This implementation completes **Task 1.1** from our breakdown. Next tasks include:

1. **Task 1.2**: Grid Visualization System (partially complete)
2. **Task 1.3**: Basic Obstacle Detection (partially complete)  
3. **Task 2.1**: A* Algorithm Implementation
4. **Task 3.1**: Basic Unit Movement Controller

## 🔗 Related Files

- [Task Breakdown](../../Tasks/3D_Grid_Pathfinding_Task_Breakdown.md) - Complete project plan
- [System Overview](README.md) - High-level system information

---

**Implementation Status**: ✅ Task 1.1 Complete  
**Next Priority**: A* Pathfinding Algorithm (Task 2.1)  
**Estimated Time to Next Milestone**: 4-5 days for core pathfinding

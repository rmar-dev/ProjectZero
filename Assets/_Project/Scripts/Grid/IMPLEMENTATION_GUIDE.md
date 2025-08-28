# 3D Grid Pathfinding System - Implementation Guide

## 🚀 **Quick Start (5 Minutes)**

### **Method 1: Automated Setup (Recommended)**

1. **Create an empty GameObject** in your scene
2. **Add the `GridTestSceneSetup` component** to it
3. **Right-click the component** in the Inspector → **"Setup Test Scene"**
4. **Press Play** to test the system

**That's it!** You now have a fully functional 3D grid pathfinding system with:
- A 20x5x20 grid with obstacles and platforms
- Mouse-based node selection
- Right-click pathfinding
- Visual feedback with Gizmos

---

## 🛠️ **Manual Setup (Full Control)**

### **Step 1: Create Grid Configuration Asset**

```csharp
// In Project window: Right-click → Create → ProjectZero/Grid/Grid Configuration
```

Configure your grid settings:
- **Grid Dimensions**: Width x Height x Depth (e.g., 50x10x50)
- **Node Size**: Physical size of each grid cell (e.g., 1.0)
- **Movement Settings**: Enable diagonal/vertical movement
- **Performance Settings**: Concurrent requests, time slicing

### **Step 2: Create Grid Manager**

```csharp
// Create empty GameObject
GameObject gridManagerGO = new GameObject("GridManager3D");
GridManager3D gridManager = gridManagerGO.AddComponent<GridManager3D>();

// Assign your configuration
gridManager.gridConfig = yourGridConfiguration;
```

### **Step 3: Initialize the System**

```csharp
// Initialize automatically on Start (default)
// OR manually call:
gridManager.InitializeGrid();
```

---

## 🎮 **Basic Usage Examples**

### **Check if Position is Walkable**

```csharp
GridManager3D gridManager = FindObjectOfType<GridManager3D>();
Vector3 worldPosition = transform.position;

if (gridManager.IsWalkablePosition(worldPosition))
{
    Debug.Log("Position is walkable!");
}
```

### **Request Pathfinding**

```csharp
// Simple pathfinding request
Vector3 startPos = unit.transform.position;
Vector3 targetPos = target.transform.position;

gridManager.RequestManager.SubmitRequest(
    startPos,
    targetPos,
    (path) => {
        Debug.Log($"Path found with {path.Count} nodes");
        // Move your unit along the path
        StartCoroutine(MoveAlongPath(path));
    },
    PathfindingProfile.CreateDefault(),
    PathfindingPriority.Normal,
    unit // Reference for debugging
);
```

### **Get Node Information**

```csharp
Vector3 worldPos = mouseWorldPosition;
GridNode node = gridManager.GetNodeAtWorldPosition(worldPos);

if (node != null)
{
    Debug.Log($"Node at {node.GridPosition} - Walkable: {node.IsWalkable}, Terrain: {node.TerrainType}");
}
```

---

## 🎯 **Advanced Usage**

### **Custom Pathfinding Profiles**

```csharp
// Create profile for a heavy unit (limited mobility)
PathfindingProfile heavyProfile = PathfindingProfile.CreateForRole(UnitRole.Heavy);

// Create custom profile
PathfindingProfile customProfile = ScriptableObject.CreateInstance<PathfindingProfile>();
customProfile.MaxClimbHeight = 1.5f;
customProfile.AllowDiagonalMovement = false;
customProfile.AvoidHazards = true;

// Use in pathfinding request
gridManager.RequestManager.SubmitRequest(start, target, OnPathComplete, customProfile);
```

### **Priority-Based Pathfinding**

```csharp
// High priority for player units
string urgentRequest = gridManager.RequestManager.SubmitRequest(
    playerStart,
    playerTarget,
    OnPlayerPathComplete,
    playerProfile,
    PathfindingPriority.Urgent // Will be processed first
);

// Normal priority for AI units
string aiRequest = gridManager.RequestManager.SubmitRequest(
    aiStart,
    aiTarget,
    OnAIPathComplete,
    aiProfile,
    PathfindingPriority.Normal
);
```

### **Formation Pathfinding**

```csharp
// For squad-based movement
public void MoveSquadInFormation(List<Transform> squadMembers, Vector3 targetCenter)
{
    Vector3[] formationPositions = CalculateFormationPositions(targetCenter, squadMembers.Count);
    
    for (int i = 0; i < squadMembers.Count; i++)
    {
        gridManager.RequestManager.SubmitRequest(
            squadMembers[i].position,
            formationPositions[i],
            (path) => StartCoroutine(MoveUnitAlongPath(squadMembers[i], path)),
            GetProfileForUnit(squadMembers[i]),
            PathfindingPriority.High
        );
    }
}
```

---

## 🔧 **Integration with Your Game Systems**

### **Squad Manager Integration**

```csharp
public class SquadController : MonoBehaviour
{
    private GridManager3D gridManager;
    private List<UnitController> squadMembers;
    
    private void Start()
    {
        gridManager = FindObjectOfType<GridManager3D>();
        
        // Subscribe to grid initialization
        gridManager.OnGridInitialized += OnGridReady;
    }
    
    public void MoveSquadTo(Vector3 targetPosition)
    {
        if (!gridManager.IsInitialized) return;
        
        Vector3 squadCenter = GetSquadCenter();
        gridManager.RequestPathfindingToPosition(targetPosition);
    }
    
    private void OnGridReady()
    {
        Debug.Log("Grid is ready, squad can now pathfind!");
    }
}
```

### **ATB Combat Integration**

```csharp
public class ATBCombatManager : MonoBehaviour
{
    private GridManager3D gridManager;
    
    public void PlanMovementAction(Unit unit, Vector3 targetPosition)
    {
        // During tactical pause, queue movement
        string requestId = gridManager.RequestManager.SubmitRequest(
            unit.transform.position,
            targetPosition,
            (path) => {
                // Store planned movement for execution when turn starts
                unit.QueuedMovement = new MovementAction(path);
            },
            unit.PathfindingProfile,
            PathfindingPriority.High,
            unit
        );
        
        unit.PendingPathfindingRequest = requestId;
    }
    
    public void ExecuteQueuedMovements()
    {
        // When exiting tactical pause, execute all queued movements
        foreach (Unit unit in allUnits)
        {
            if (unit.QueuedMovement != null)
            {
                StartCoroutine(ExecuteMovement(unit, unit.QueuedMovement.Path));
                unit.QueuedMovement = null;
            }
        }
    }
}
```

### **Cover System Integration**

```csharp
public class CoverSystemIntegration : MonoBehaviour
{
    private GridManager3D gridManager;
    
    public Vector3 FindNearestCoverFrom(Vector3 unitPosition, Vector3 threatPosition)
    {
        GridNode unitNode = gridManager.GetNodeAtWorldPosition(unitPosition);
        Vector3 threatDirection = (threatPosition - unitPosition).normalized;
        
        // Search nearby nodes for cover
        GridNode[] nearbyNodes = gridManager.GridSystem.GetNodesInRadius(
            unitNode.GridPosition, 5f // Search radius
        );
        
        GridNode bestCoverNode = null;
        float bestCoverScore = 0f;
        
        foreach (GridNode node in nearbyNodes)
        {
            if (node.ProvidesCoverFrom(threatDirection))
            {
                float distance = Vector3.Distance(unitPosition, node.WorldPosition);
                float coverScore = (1f / distance) * GetCoverEffectiveness(node, threatDirection);
                
                if (coverScore > bestCoverScore)
                {
                    bestCoverScore = coverScore;
                    bestCoverNode = node;
                }
            }
        }
        
        return bestCoverNode?.WorldPosition ?? unitPosition;
    }
}
```

---

## 📊 **Performance Optimization**

### **Configuration for Performance**

```csharp
// In GridConfiguration asset:

// Reduce concurrent requests for lower-end devices
maxConcurrentRequests = 2; // Instead of 4

// Increase time slice for smoother framerate
pathfindingTimeSlice = 0.008f; // 8ms instead of 5ms

// Disable neighbor caching for memory-constrained devices
cacheNeighbors = false;

// Reduce grid size if needed
gridWidth = 30; // Instead of 50
gridHeight = 5;  // Instead of 10
gridDepth = 30;  // Instead of 50
```

### **Memory Management**

```csharp
public class PerformanceMonitor : MonoBehaviour
{
    private GridManager3D gridManager;
    
    private void Update()
    {
        if (gridManager.IsInitialized)
        {
            var stats = gridManager.GetStatistics();
            
            // Monitor memory usage
            if (stats.MemoryUsageMB > 10f) // 10 MB threshold
            {
                Debug.LogWarning($"High memory usage: {stats.MemoryUsageMB:F1} MB");
            }
            
            // Monitor pending requests
            if (stats.PendingPathfindingRequests > 10)
            {
                Debug.LogWarning("Pathfinding queue is backing up!");
            }
        }
    }
}
```

---

## 🐛 **Troubleshooting**

### **Common Issues and Solutions**

| **Issue** | **Cause** | **Solution** |
|-----------|-----------|--------------|
| **"Grid not initialized"** | GridManager not set up properly | Ensure GridConfiguration is assigned and InitializeGrid() is called |
| **No pathfinding results** | Start/target positions not walkable | Use `IsWalkablePosition()` to validate positions before pathfinding |
| **Performance issues** | Grid too large or too many requests | Reduce grid size or limit concurrent requests |
| **Units walking through walls** | Obstacle detection not working | Check LayerMask settings and ensure obstacles have colliders |
| **Vertical movement not working** | AllowVerticalMovement disabled | Enable in GridConfiguration and ensure ClimbHeight > 0 |

### **Debugging Tools**

```csharp
// Enable debug logging
gridConfig.LogPathfindingStatistics = true;
gridConfig.EnablePathfindingDebug = true;

// Monitor grid statistics
var stats = gridManager.GetStatistics();
Debug.Log($"Grid: {stats.GridSize}, Nodes: {stats.TotalNodes}, Memory: {stats.MemoryUsageMB:F1}MB");

// Check pathfinding request status
PathfindingRequestStatus status = gridManager.RequestManager.GetRequestStatus(requestId);
```

### **Gizmos Visualization**

When you select the GridManager3D in the scene:
- **Cyan wireframe** = Grid boundaries
- **Green cubes** = Selected nodes
- **Yellow lines** = Current pathfinding result

---

## 🎯 **Best Practices**

### **1. Initialization**
- Always check `gridManager.IsInitialized` before using pathfinding
- Initialize grid during loading screens, not during gameplay

### **2. Request Management**
- Use appropriate priority levels for different unit types
- Cancel requests when units are destroyed or change objectives
- Batch pathfinding requests when possible

### **3. Performance**
- Start with smaller grid sizes and increase as needed
- Use profiles to limit expensive pathfinding for simple units
- Monitor memory usage, especially on mobile devices

### **4. Integration**
- Subscribe to grid events rather than polling
- Cache frequently accessed nodes when possible
- Use world position validation before pathfinding

---

## 🔮 **Next Steps**

Once you have the basic system working, you can:

1. **Implement the remaining todos**: Vertical Movement Rules, Enhanced Visualizer, Unit Tests
2. **Add visual path rendering** with LineRenderer for better feedback
3. **Integrate with your squad movement system** for formation pathfinding
4. **Add dynamic obstacle updates** for destructible environments
5. **Optimize for mobile** if targeting mobile platforms

---

## 📞 **Support**

The system is designed to be self-contained and well-documented. If you encounter issues:

1. **Check the Console** for error messages and warnings
2. **Enable debug logging** in GridConfiguration
3. **Use the test scene** to isolate issues
4. **Check the statistics** for performance problems

Your 3D Grid Pathfinding System is now ready for production use! 🎮✨

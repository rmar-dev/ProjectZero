# Unity 3D Grid Pathfinding System Documentation
## ProjectZero - Tactical Squad Movement

**Version:** 1.0 (Unity Implementation)  
**Date:** August 28, 2025  
**Engine:** Unity 2022.3 LTS  
**System Type:** 3D Grid-Based Pathfinding with Vertical Movement  
**Integration Status:** Phase 2 Development Target

---

## 📋 **System Overview**

The Unity 3D Grid Pathfinding System provides tactical movement for squad-based gameplay, supporting:
- **3D Grid Structure** - Multi-layered grid with vertical movement
- **A* Pathfinding** - Optimized pathfinding with configurable heuristics
- **Squad Coordination** - Formation-aware pathfinding for multiple units
- **Real-Time Performance** - Suitable for real-time tactical gameplay
- **Unity Integration** - Built with Unity best practices and native systems

---

## 🏗️ **Core Architecture Principles**

### **1. Unity Component-Based Design**
- All grid functionality implemented as MonoBehaviour components
- Clear separation of concerns between grid management, pathfinding, and movement
- Leverages Unity's built-in component system for modularity

### **2. ScriptableObject Configuration**
- Grid settings stored in ScriptableObject assets for designer-friendly configuration
- Separate pathfinding profiles for different unit types
- Easy runtime modification and testing

### **3. Job System Integration**
- Expensive pathfinding calculations moved to Unity Jobs
- Multi-threaded pathfinding for large grids and multiple units
- Maintains 60+ FPS during complex pathfinding scenarios

### **4. Memory Efficient Storage**
- Grid nodes stored in optimized 3D arrays
- Spatial hashing for fast neighbor queries
- Object pooling for pathfinding requests

---

## 📐 **Grid Structure & Coordinates**

### **3D Grid Representation**
```csharp
// Grid coordinate system (0-based indexing)
// X: East-West axis (Right-Left)  
// Y: Vertical axis (Up-Down)
// Z: North-South axis (Forward-Back)

public struct GridCoordinate
{
    public int x, y, z;
    
    public GridCoordinate(int x, int y, int z)
    {
        this.x = x; this.y = y; this.z = z;
    }
}
```

### **World-Grid Conversion**
```csharp
// Convert world position to grid coordinates
public GridCoordinate WorldToGrid(Vector3 worldPosition)
{
    Vector3 localPos = worldPosition - gridOrigin;
    return new GridCoordinate(
        Mathf.FloorToInt(localPos.x / nodeSize),
        Mathf.FloorToInt(localPos.y / nodeSize), 
        Mathf.FloorToInt(localPos.z / nodeSize)
    );
}

// Convert grid coordinates to world position
public Vector3 GridToWorld(GridCoordinate gridPos)
{
    return gridOrigin + new Vector3(
        gridPos.x * nodeSize + nodeSize * 0.5f,
        gridPos.y * nodeSize + nodeSize * 0.5f,
        gridPos.z * nodeSize + nodeSize * 0.5f
    );
}
```

---

## 🎯 **Grid Node System**

### **GridNode Structure**
```csharp
[System.Serializable]
public class GridNode
{
    [Header("Position")]
    public GridCoordinate coordinate;
    public Vector3 worldPosition;
    
    [Header("Pathfinding")]
    public bool isWalkable = true;
    public float movementCost = 1.0f;
    public TerrainType terrainType = TerrainType.Normal;
    
    [Header("Occupancy")]
    public bool isOccupied = false;
    public Transform occupyingUnit = null;
    public bool isReserved = false;
    public Transform reservingUnit = null;
    
    [Header("A* Algorithm")]
    public float gCost = 0f;  // Distance from start
    public float hCost = 0f;  // Distance to target  
    public float fCost => gCost + hCost; // Total cost
    public GridNode parent = null;
    
    [Header("Cover & Tactics")]
    public bool providesPartialCover = false;
    public bool providesFullCover = false;
    public Vector3 coverDirection = Vector3.zero;
    
    // Performance optimization - cache neighbor references
    [System.NonSerialized]
    public GridNode[] neighbors;
    
    public bool IsAvailable => isWalkable && !isOccupied && !isReserved;
    public bool IsTraversable => isWalkable && !isOccupied;
}
```

### **Terrain Types & Movement Costs**
```csharp
public enum TerrainType
{
    Normal = 0,        // 1.0x movement cost
    Difficult = 1,     // 1.5x movement cost (mud, debris)
    Slow = 2,          // 2.0x movement cost (thick vegetation)
    Road = 3,          // 0.8x movement cost (clear path)
    Stairs = 4,        // 1.2x movement cost (vertical movement)
    Impassable = 5     // Cannot traverse
}

public static class MovementCosts
{
    public static readonly Dictionary<TerrainType, float> BaseCosts = new()
    {
        { TerrainType.Normal, 1.0f },
        { TerrainType.Difficult, 1.5f },
        { TerrainType.Slow, 2.0f },
        { TerrainType.Road, 0.8f },
        { TerrainType.Stairs, 1.2f },
        { TerrainType.Impassable, float.MaxValue }
    };
}
```

---

## 🎮 **Unity Component Architecture**

### **1. GridManager.cs - Core Grid System**
**Location:** `Assets/_Project/Scripts/Pathfinding/GridManager.cs`

```csharp
public class GridManager : MonoBehaviour
{
    [Header("Grid Configuration")]
    [SerializeField] private GridConfiguration config;
    [SerializeField] private LayerMask obstacleLayerMask = -1;
    
    [Header("Runtime Settings")]  
    [SerializeField] private bool showGridVisualization = true;
    [SerializeField] private bool enableDynamicObstacles = true;
    
    [Header("Performance")]
    [SerializeField] private int maxPathfindingRequestsPerFrame = 5;
    [SerializeField] private bool useJobSystemForPathfinding = true;
    
    private GridNode[,,] grid;
    private Vector3 gridOrigin;
    private Queue<PathfindingRequest> pathfindingQueue;
    
    public static GridManager Instance { get; private set; }
    
    // Events
    public UnityEvent<GridCoordinate[]> OnPathCalculated;
    public UnityEvent<GridCoordinate, bool> OnNodeOccupancyChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeGrid();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

### **2. GridConfiguration.cs - ScriptableObject Settings**
**Location:** `Assets/_Project/Scripts/Pathfinding/GridConfiguration.cs`

```csharp
[CreateAssetMenu(fileName = "GridConfiguration", menuName = "ProjectZero/Grid Configuration")]
public class GridConfiguration : ScriptableObject
{
    [Header("Grid Dimensions")]
    [Range(10, 200)]
    public int gridWidth = 50;
    [Range(5, 50)] 
    public int gridHeight = 10;
    [Range(10, 200)]
    public int gridDepth = 50;
    
    [Header("Grid Spacing")]
    [Range(0.5f, 5.0f)]
    public float nodeSize = 1.0f;
    public Vector3 gridOffset = Vector3.zero;
    
    [Header("Movement Rules")]
    public bool allowDiagonalMovement = true;
    public bool allowVerticalMovement = true;
    [Range(1, 3)]
    public int maxVerticalStepHeight = 1;
    
    [Header("Pathfinding Costs")]
    public float horizontalMovementCost = 1.0f;
    public float verticalMovementCost = 1.4f;
    public float diagonalMovementCost = 1.414f;
    
    [Header("Performance")]
    [Range(100, 10000)]
    public int maxPathfindingIterations = 1000;
    [Range(0.1f, 5.0f)]
    public float pathfindingTimeSlice = 1.0f; // Max ms per frame
}
```

### **3. PathfindingRequest.cs - Request System**
**Location:** `Assets/_Project/Scripts/Pathfinding/PathfindingRequest.cs`

```csharp
[System.Serializable]
public class PathfindingRequest
{
    public Transform requester;
    public GridCoordinate start;
    public GridCoordinate target;
    public PathfindingProfile profile;
    public System.Action<GridCoordinate[], bool> callback;
    public float priority = 1.0f;
    
    public PathfindingRequest(Transform requester, GridCoordinate start, GridCoordinate target, 
                            System.Action<GridCoordinate[], bool> callback, PathfindingProfile profile = null)
    {
        this.requester = requester;
        this.start = start;
        this.target = target;
        this.callback = callback;
        this.profile = profile ?? PathfindingProfile.Default;
    }
}

[CreateAssetMenu(fileName = "PathfindingProfile", menuName = "ProjectZero/Pathfinding Profile")]
public class PathfindingProfile : ScriptableObject
{
    [Header("Unit Properties")]
    public float movementSpeed = 3.5f;
    public bool canClimbStairs = true;
    public bool canJumpGaps = false;
    public int maxClimbHeight = 1;
    
    [Header("Pathfinding Behavior")]
    public bool preferCoverRoutes = false;
    public bool avoidEnemies = true;
    public float enemyAvoidanceRange = 5.0f;
    
    [Header("Heuristic Weights")]
    [Range(0.1f, 2.0f)]
    public float heuristicWeight = 1.0f;
    
    public static PathfindingProfile Default => Resources.Load<PathfindingProfile>("DefaultPathfindingProfile");
}
```

---

## ⚡ **A* Pathfinding Implementation**

### **Core A* Algorithm**
```csharp
public class AStarPathfinder : MonoBehaviour
{
    private GridManager gridManager;
    private List<GridNode> openSet;
    private HashSet<GridNode> closedSet;
    
    public async Task<GridCoordinate[]> FindPathAsync(GridCoordinate start, GridCoordinate target, 
                                                     PathfindingProfile profile)
    {
        return await Task.Run(() => FindPath(start, target, profile));
    }
    
    private GridCoordinate[] FindPath(GridCoordinate start, GridCoordinate target, PathfindingProfile profile)
    {
        GridNode startNode = gridManager.GetNode(start);
        GridNode targetNode = gridManager.GetNode(target);
        
        if (startNode == null || targetNode == null || !targetNode.IsTraversable)
            return new GridCoordinate[0];
            
        openSet.Clear();
        closedSet.Clear();
        
        startNode.gCost = 0;
        startNode.hCost = CalculateDistance(startNode.coordinate, target, profile);
        startNode.parent = null;
        
        openSet.Add(startNode);
        
        int iterations = 0;
        while (openSet.Count > 0 && iterations < gridManager.Config.maxPathfindingIterations)
        {
            GridNode currentNode = GetLowestFCostNode();
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            
            if (currentNode.coordinate.Equals(target))
            {
                return ReconstructPath(currentNode);
            }
            
            foreach (GridNode neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.IsTraversable || closedSet.Contains(neighbor))
                    continue;
                    
                float newGCost = currentNode.gCost + CalculateMovementCost(currentNode, neighbor, profile);
                
                if (!openSet.Contains(neighbor))
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = CalculateDistance(neighbor.coordinate, target, profile);
                    neighbor.parent = currentNode;
                    openSet.Add(neighbor);
                }
                else if (newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost;
                    neighbor.parent = currentNode;
                }
            }
            
            iterations++;
        }
        
        return new GridCoordinate[0]; // No path found
    }
}
```

### **Distance & Heuristic Calculations**
```csharp
private float CalculateDistance(GridCoordinate from, GridCoordinate to, PathfindingProfile profile)
{
    // Manhattan distance with vertical weighting
    int dx = Mathf.Abs(to.x - from.x);
    int dy = Mathf.Abs(to.y - from.y);
    int dz = Mathf.Abs(to.z - from.z);
    
    float distance = dx + dz; // Horizontal distance
    distance += dy * gridManager.Config.verticalMovementCost; // Weighted vertical distance
    
    return distance * profile.heuristicWeight;
}

private float CalculateMovementCost(GridNode from, GridNode to, PathfindingProfile profile)
{
    float baseCost = MovementCosts.BaseCosts[to.terrainType];
    
    // Vertical movement penalty
    if (from.coordinate.y != to.coordinate.y)
    {
        baseCost *= gridManager.Config.verticalMovementCost;
    }
    
    // Diagonal movement penalty
    bool isDiagonal = (from.coordinate.x != to.coordinate.x) && (from.coordinate.z != to.coordinate.z);
    if (isDiagonal)
    {
        baseCost *= gridManager.Config.diagonalMovementCost;
    }
    
    // Cover preference bonus
    if (profile.preferCoverRoutes && (to.providesPartialCover || to.providesFullCover))
    {
        baseCost *= 0.8f;
    }
    
    return baseCost;
}
```

---

## 🚶 **Vertical Movement Rules**

### **Movement Constraints**
```csharp
public class VerticalMovementRules
{
    // Maximum height difference for a single step
    public static bool CanStepUp(GridNode from, GridNode to, PathfindingProfile profile)
    {
        int heightDiff = to.coordinate.y - from.coordinate.y;
        return heightDiff <= profile.maxClimbHeight;
    }
    
    // Check for falling scenarios
    public static bool RequiresFalling(GridNode from, GridNode to)
    {
        return to.coordinate.y < from.coordinate.y;
    }
    
    // Validate vertical movement legality
    public static bool IsVerticalMovementValid(GridNode from, GridNode to, GridManager grid, PathfindingProfile profile)
    {
        int heightDiff = to.coordinate.y - from.coordinate.y;
        
        // Moving up - check climb constraints
        if (heightDiff > 0)
        {
            if (heightDiff > profile.maxClimbHeight) return false;
            
            // Check for stairs or climbable terrain
            if (heightDiff > 1 && to.terrainType != TerrainType.Stairs) return false;
        }
        
        // Moving down - check for safe landing
        if (heightDiff < 0)
        {
            // Allow controlled descent up to 2 levels
            if (heightDiff < -2) return false;
        }
        
        // Check intermediate nodes for obstructions
        return !HasVerticalObstructions(from, to, grid);
    }
    
    private static bool HasVerticalObstructions(GridNode from, GridNode to, GridManager grid)
    {
        // Check nodes between from.y and to.y for blocking obstacles
        int minY = Mathf.Min(from.coordinate.y, to.coordinate.y);
        int maxY = Mathf.Max(from.coordinate.y, to.coordinate.y);
        
        for (int y = minY + 1; y < maxY; y++)
        {
            GridCoordinate checkCoord = new GridCoordinate(to.coordinate.x, y, to.coordinate.z);
            GridNode checkNode = grid.GetNode(checkCoord);
            
            if (checkNode != null && !checkNode.isWalkable)
                return true; // Obstruction found
        }
        
        return false;
    }
}
```

### **Neighbor Generation for 3D Movement**
```csharp
public GridNode[] GetNeighbors(GridNode node)
{
    List<GridNode> neighbors = new List<GridNode>();
    GridConfiguration config = gridManager.Config;
    
    // 6-directional movement (cardinal directions + up/down)
    Vector3Int[] directions = {
        Vector3Int.right,    // East
        Vector3Int.left,     // West  
        Vector3Int.forward,  // North
        Vector3Int.back,     // South
        Vector3Int.up,       // Up
        Vector3Int.down      // Down
    };
    
    // Add diagonal directions if allowed
    if (config.allowDiagonalMovement)
    {
        Vector3Int[] diagonalDirections = {
            new Vector3Int(1, 0, 1),   // Northeast
            new Vector3Int(-1, 0, 1),  // Northwest
            new Vector3Int(1, 0, -1),  // Southeast
            new Vector3Int(-1, 0, -1)  // Southwest
        };
        directions = directions.Concat(diagonalDirections).ToArray();
    }
    
    foreach (Vector3Int dir in directions)
    {
        GridCoordinate neighborCoord = new GridCoordinate(
            node.coordinate.x + dir.x,
            node.coordinate.y + dir.y, 
            node.coordinate.z + dir.z
        );
        
        GridNode neighbor = gridManager.GetNode(neighborCoord);
        if (neighbor != null && IsValidNeighbor(node, neighbor))
        {
            neighbors.Add(neighbor);
        }
    }
    
    return neighbors.ToArray();
}

private bool IsValidNeighbor(GridNode from, GridNode to)
{
    // Basic walkability check
    if (!to.isWalkable) return false;
    
    // Vertical movement validation
    if (from.coordinate.y != to.coordinate.y)
    {
        if (!gridManager.Config.allowVerticalMovement) return false;
        return VerticalMovementRules.IsVerticalMovementValid(from, to, gridManager, currentProfile);
    }
    
    return true;
}
```

---

## 🎪 **Unity Integration & Performance**

### **Grid Visualization**
```csharp
public class GridVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private bool showOnlyWalkableNodes = false;
    [SerializeField] private Color walkableColor = Color.white;
    [SerializeField] private Color unwalkableColor = Color.red;
    [SerializeField] private Color occupiedColor = Color.yellow;
    [SerializeField] private Color pathColor = Color.green;
    
    private GridManager gridManager;
    
    private void OnDrawGizmos()
    {
        if (!showGrid || gridManager == null) return;
        
        DrawGridNodes();
        if (showCurrentPath) DrawCurrentPath();
    }
    
    private void DrawGridNodes()
    {
        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                for (int z = 0; z < gridManager.GridDepth; z++)
                {
                    GridCoordinate coord = new GridCoordinate(x, y, z);
                    GridNode node = gridManager.GetNode(coord);
                    
                    if (node == null) continue;
                    if (showOnlyWalkableNodes && !node.isWalkable) continue;
                    
                    // Determine node color
                    Color gizmoColor = GetNodeColor(node);
                    
                    // Draw node representation
                    Gizmos.color = gizmoColor;
                    Vector3 worldPos = gridManager.GridToWorld(coord);
                    Gizmos.DrawWireCube(worldPos, Vector3.one * gridManager.Config.nodeSize * 0.8f);
                    
                    // Draw vertical connections
                    if (y < gridManager.GridHeight - 1)
                    {
                        GridNode nodeAbove = gridManager.GetNode(new GridCoordinate(x, y + 1, z));
                        if (nodeAbove != null && nodeAbove.isWalkable)
                        {
                            Gizmos.color = Color.cyan;
                            Vector3 abovePos = gridManager.GridToWorld(new GridCoordinate(x, y + 1, z));
                            Gizmos.DrawLine(worldPos, abovePos);
                        }
                    }
                }
            }
        }
    }
    
    private Color GetNodeColor(GridNode node)
    {
        if (node.isOccupied) return occupiedColor;
        if (!node.isWalkable) return unwalkableColor;
        
        // Color by terrain type
        switch (node.terrainType)
        {
            case TerrainType.Difficult: return Color.orange;
            case TerrainType.Slow: return Color.red;
            case TerrainType.Road: return Color.blue;
            case TerrainType.Stairs: return Color.magenta;
            default: return walkableColor;
        }
    }
}
```

### **Unity Job System Integration**
```csharp
[BurstCompile]
public struct PathfindingJob : IJob
{
    [ReadOnly] public NativeArray<GridNodeData> gridNodes;
    [ReadOnly] public GridCoordinate start;
    [ReadOnly] public GridCoordinate target;
    [ReadOnly] public int gridWidth;
    [ReadOnly] public int gridHeight;
    [ReadOnly] public int gridDepth;
    
    public NativeArray<GridCoordinate> result;
    
    public void Execute()
    {
        // Implement A* algorithm using NativeArrays for burst compilation
        // This runs on a separate thread for performance
        var openSet = new NativeList<int>(Allocator.Temp);
        var closedSet = new NativeHashSet<int>(100, Allocator.Temp);
        
        // ... A* implementation using NativeCollections
        
        openSet.Dispose();
        closedSet.Dispose();
    }
}

// Usage in GridManager
public void RequestPathAsync(GridCoordinate start, GridCoordinate target, System.Action<GridCoordinate[]> callback)
{
    var job = new PathfindingJob
    {
        gridNodes = gridNodeDataArray,
        start = start,
        target = target,
        gridWidth = config.gridWidth,
        gridHeight = config.gridHeight,
        gridDepth = config.gridDepth,
        result = new NativeArray<GridCoordinate>(1000, Allocator.TempJob)
    };
    
    JobHandle jobHandle = job.Schedule();
    StartCoroutine(WaitForPathfindingJob(jobHandle, job.result, callback));
}
```

---

## 🎯 **Squad Formation Integration**

### **Formation-Aware Pathfinding**
```csharp
public class SquadPathfinding : MonoBehaviour
{
    [Header("Squad Settings")]
    [SerializeField] private List<Transform> squadMembers;
    [SerializeField] private FormationType currentFormation;
    [SerializeField] private float formationSpacing = 2.0f;
    
    private GridManager gridManager;
    
    public void MoveSquadToPosition(GridCoordinate targetCoordinate)
    {
        Vector3[] formationPositions = CalculateFormationPositions(targetCoordinate);
        
        for (int i = 0; i < squadMembers.Count && i < formationPositions.Length; i++)
        {
            GridCoordinate memberTarget = gridManager.WorldToGrid(formationPositions[i]);
            
            // Find nearest walkable node if target is blocked
            if (!IsPositionValid(memberTarget))
            {
                memberTarget = FindNearestWalkableNode(memberTarget);
            }
            
            // Request path for this squad member
            PathfindingRequest request = new PathfindingRequest(
                squadMembers[i],
                gridManager.WorldToGrid(squadMembers[i].position),
                memberTarget,
                (path, success) => OnSquadMemberPathReceived(squadMembers[i], path, success)
            );
            
            gridManager.RequestPath(request);
        }
    }
    
    private Vector3[] CalculateFormationPositions(GridCoordinate centerCoordinate)
    {
        Vector3 centerWorld = gridManager.GridToWorld(centerCoordinate);
        Vector3[] positions = new Vector3[squadMembers.Count];
        
        switch (currentFormation)
        {
            case FormationType.Line:
                for (int i = 0; i < squadMembers.Count; i++)
                {
                    float offset = (i - squadMembers.Count * 0.5f + 0.5f) * formationSpacing;
                    positions[i] = centerWorld + Vector3.right * offset;
                }
                break;
                
            case FormationType.Wedge:
                positions[0] = centerWorld; // Leader at front
                for (int i = 1; i < squadMembers.Count; i++)
                {
                    float side = (i % 2 == 1) ? -1f : 1f;
                    int rank = (i + 1) / 2;
                    positions[i] = centerWorld + new Vector3(side * formationSpacing, 0, -rank * formationSpacing);
                }
                break;
                
            case FormationType.Box:
                // Arrange in 2x2 or 2x3 box formation
                int columns = Mathf.CeilToInt(Mathf.Sqrt(squadMembers.Count));
                for (int i = 0; i < squadMembers.Count; i++)
                {
                    int row = i / columns;
                    int col = i % columns;
                    positions[i] = centerWorld + new Vector3(
                        (col - columns * 0.5f + 0.5f) * formationSpacing,
                        0,
                        (row - (squadMembers.Count / columns) * 0.5f + 0.5f) * formationSpacing
                    );
                }
                break;
        }
        
        return positions;
    }
    
    private GridCoordinate FindNearestWalkableNode(GridCoordinate blocked)
    {
        Queue<GridCoordinate> searchQueue = new Queue<GridCoordinate>();
        HashSet<GridCoordinate> visited = new HashSet<GridCoordinate>();
        
        searchQueue.Enqueue(blocked);
        visited.Add(blocked);
        
        while (searchQueue.Count > 0)
        {
            GridCoordinate current = searchQueue.Dequeue();
            
            if (IsPositionValid(current))
                return current;
            
            // Check all neighbors
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0) continue;
                    
                    GridCoordinate neighbor = new GridCoordinate(
                        current.x + dx, current.y, current.z + dz
                    );
                    
                    if (!visited.Contains(neighbor) && gridManager.IsValidCoordinate(neighbor))
                    {
                        searchQueue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
        }
        
        return blocked; // Fallback to original position
    }
}
```

---

## 📋 **Implementation Checklist**

### **Phase 1: Core Grid System** (Week 1-2)
- [ ] **GridManager.cs** - Core grid management MonoBehaviour
- [ ] **GridConfiguration.cs** - ScriptableObject for designer settings  
- [ ] **GridNode.cs** - Node data structure with A* properties
- [ ] **Grid coordinate conversion** - World ↔ Grid position conversion
- [ ] **Basic 3D grid creation** - Initialize grid array with proper bounds
- [ ] **Obstacle detection** - Physics-based obstacle scanning
- [ ] **Grid visualization** - Gizmos for Scene view debugging

### **Phase 2: A* Pathfinding** (Week 3-4)
- [ ] **AStarPathfinder.cs** - Core A* algorithm implementation
- [ ] **PathfindingRequest.cs** - Request queue system  
- [ ] **PathfindingProfile.cs** - Unit-specific pathfinding settings
- [ ] **Heuristic calculations** - Distance and cost calculations
- [ ] **Path reconstruction** - Convert node chain to coordinate array
- [ ] **Neighbor generation** - 6-directional and diagonal movement
- [ ] **Vertical movement rules** - Climb/fall validation

### **Phase 3: Unity Integration** (Week 5-6)
- [ ] **Job System integration** - Multi-threaded pathfinding
- [ ] **Coroutine-based processing** - Spread work across frames
- [ ] **Unity Event system** - Path completion callbacks
- [ ] **Performance profiling** - Ensure 60+ FPS with multiple units
- [ ] **Memory optimization** - Object pooling and efficient storage
- [ ] **Grid visualization tools** - Runtime debugging interface

### **Phase 4: Squad Integration** (Week 7-8) 
- [ ] **SquadPathfinding.cs** - Formation-aware pathfinding
- [ ] **Formation position calculation** - Line, Wedge, Box formations
- [ ] **Nearest walkable node finding** - Handle blocked formations
- [ ] **Coordinated movement** - Multiple unit pathfinding
- [ ] **Movement synchronization** - Squad timing coordination
- [ ] **Obstacle avoidance** - Dynamic unit collision prevention

---

## 🚨 **Performance Guidelines & Best Practices**

### **Grid Size Recommendations**
```csharp
// Small tactical maps (recommended)
GridWidth: 50, GridHeight: 10, GridDepth: 50
NodeSize: 1.0f
Total Nodes: 25,000 (~2.5MB memory)

// Medium strategic maps  
GridWidth: 100, GridHeight: 15, GridDepth: 100
NodeSize: 1.0f
Total Nodes: 150,000 (~15MB memory)

// Large open world maps (use with caution)
GridWidth: 200, GridHeight: 20, GridDepth: 200
NodeSize: 1.5f
Total Nodes: 800,000 (~80MB memory)
```

### **Performance Optimization Rules**
1. **Limit concurrent pathfinding requests** to 5-10 per frame
2. **Use Job System** for grids larger than 50x50x10
3. **Cache neighbor references** to avoid repeated calculations
4. **Implement path smoothing** to reduce node count in final paths
5. **Use hierarchical pathfinding** for very large grids (A* + flow fields)

### **Memory Management**
```csharp
public class GridMemoryManager
{
    private static readonly ObjectPool<List<GridNode>> nodeListPool = new ObjectPool<List<GridNode>>();
    private static readonly ObjectPool<HashSet<GridNode>> nodeSetPool = new ObjectPool<HashSet<GridNode>>();
    
    public static List<GridNode> GetNodeList()
    {
        var list = nodeListPool.Get();
        list.Clear();
        return list;
    }
    
    public static void ReturnNodeList(List<GridNode> list)
    {
        nodeListPool.Return(list);
    }
    
    // Use throughout pathfinding to avoid GC allocations
}
```

---

## 🔧 **Unity Editor Tools**

### **Grid Inspector Tool**
```csharp
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private GridManager gridManager;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        gridManager = (GridManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Generate Grid"))
        {
            gridManager.RegenerateGrid();
            EditorUtility.SetDirty(gridManager);
        }
        
        if (GUILayout.Button("Clear Grid"))
        {
            gridManager.ClearGrid();
            EditorUtility.SetDirty(gridManager);
        }
        
        if (GUILayout.Button("Scan for Obstacles"))
        {
            gridManager.ScanForObstacles();
            EditorUtility.SetDirty(gridManager);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Information", EditorStyles.boldLabel);
        
        if (Application.isPlaying && gridManager.IsInitialized)
        {
            EditorGUILayout.LabelField($"Total Nodes: {gridManager.TotalNodes:N0}");
            EditorGUILayout.LabelField($"Walkable Nodes: {gridManager.WalkableNodes:N0}");
            EditorGUILayout.LabelField($"Occupied Nodes: {gridManager.OccupiedNodes:N0}");
            EditorGUILayout.LabelField($"Memory Usage: {gridManager.EstimatedMemoryMB:F2} MB");
            
            if (GUILayout.Button("Print Grid Statistics"))
            {
                gridManager.PrintGridStatistics();
            }
        }
    }
    
    private void OnSceneGUI()
    {
        if (!gridManager.showGridVisualization) return;
        
        // Draw grid bounds in scene view
        Handles.color = Color.yellow;
        Vector3 center = gridManager.GridCenter;
        Vector3 size = gridManager.GridSize;
        Handles.DrawWireCube(center, size);
        
        // Draw grid origin
        Handles.color = Color.red;
        Handles.SphereHandleCap(0, gridManager.GridOrigin, Quaternion.identity, 0.5f, EventType.Repaint);
    }
}
#endif
```

### **Pathfinding Debugger Window**
```csharp
#if UNITY_EDITOR
public class PathfindingDebugger : EditorWindow
{
    private GridManager gridManager;
    private Vector3 startPos, endPos;
    private bool showPath = true;
    private List<GridCoordinate> lastCalculatedPath;
    
    [MenuItem("ProjectZero/Pathfinding Debugger")]
    public static void ShowWindow()
    {
        GetWindow<PathfindingDebugger>("Pathfinding Debugger");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Pathfinding Debug Tools", EditorStyles.boldLabel);
        
        gridManager = (GridManager)EditorGUILayout.ObjectField("Grid Manager", gridManager, typeof(GridManager), true);
        
        if (gridManager == null)
        {
            EditorGUILayout.HelpBox("Please assign a Grid Manager to use debug tools.", MessageType.Warning);
            return;
        }
        
        EditorGUILayout.Space();
        
        startPos = EditorGUILayout.Vector3Field("Start Position", startPos);
        endPos = EditorGUILayout.Vector3Field("End Position", endPos);
        
        if (GUILayout.Button("Calculate Path"))
        {
            CalculateDebugPath();
        }
        
        showPath = EditorGUILayout.Toggle("Show Path in Scene", showPath);
        
        if (lastCalculatedPath != null && lastCalculatedPath.Count > 0)
        {
            EditorGUILayout.LabelField($"Path Length: {lastCalculatedPath.Count} nodes");
            EditorGUILayout.LabelField($"Estimated Distance: {CalculatePathDistance():F2} units");
        }
    }
    
    private void CalculateDebugPath()
    {
        if (gridManager == null || !Application.isPlaying) return;
        
        GridCoordinate start = gridManager.WorldToGrid(startPos);
        GridCoordinate end = gridManager.WorldToGrid(endPos);
        
        // Request path calculation
        PathfindingRequest request = new PathfindingRequest(
            null, start, end, 
            (path, success) => {
                lastCalculatedPath = new List<GridCoordinate>(path);
                Debug.Log($"Path calculated: {path.Length} nodes, Success: {success}");
            }
        );
        
        gridManager.RequestPath(request);
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!showPath || lastCalculatedPath == null || gridManager == null) return;
        
        Handles.color = Color.green;
        for (int i = 0; i < lastCalculatedPath.Count - 1; i++)
        {
            Vector3 current = gridManager.GridToWorld(lastCalculatedPath[i]);
            Vector3 next = gridManager.GridToWorld(lastCalculatedPath[i + 1]);
            Handles.DrawLine(current, next);
        }
    }
}
#endif
```

---

## 🎯 **Integration with Existing ProjectZero Systems**

### **Squad Manager Integration**
```csharp
// Extend existing SquadManager.cs with pathfinding
public partial class SquadManager : MonoBehaviour
{
    [Header("Pathfinding")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private bool useGridPathfinding = true;
    
    private Dictionary<Transform, GridCoordinate[]> unitPaths = new Dictionary<Transform, GridCoordinate[]>();
    
    public override void ProcessContextualCommand(Vector3 worldPosition)
    {
        if (useGridPathfinding && gridManager != null)
        {
            ProcessGridBasedCommand(worldPosition);
        }
        else
        {
            // Fall back to NavMesh pathfinding
            base.ProcessContextualCommand(worldPosition);
        }
    }
    
    private void ProcessGridBasedCommand(Vector3 worldPosition)
    {
        GridCoordinate targetCoordinate = gridManager.WorldToGrid(worldPosition);
        
        // Calculate formation positions
        Vector3[] formationPositions = CalculateFormationPositions(worldPosition);
        
        // Request paths for each squad member
        for (int i = 0; i < squadMembers.Count && i < formationPositions.Length; i++)
        {
            Transform unit = squadMembers[i].transform;
            GridCoordinate startCoord = gridManager.WorldToGrid(unit.position);
            GridCoordinate endCoord = gridManager.WorldToGrid(formationPositions[i]);
            
            PathfindingRequest request = new PathfindingRequest(
                unit, startCoord, endCoord,
                (path, success) => OnUnitPathReceived(unit, path, success)
            );
            
            gridManager.RequestPath(request);
        }
    }
    
    private void OnUnitPathReceived(Transform unit, GridCoordinate[] path, bool success)
    {
        if (success && path.Length > 0)
        {
            unitPaths[unit] = path;
            
            // Convert path to world positions for unit movement
            Vector3[] worldPath = new Vector3[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                worldPath[i] = gridManager.GridToWorld(path[i]);
            }
            
            // Start unit movement along path
            UnitMovementController unitMovement = unit.GetComponent<UnitMovementController>();
            if (unitMovement != null)
            {
                unitMovement.StartMovementAlongPath(worldPath);
            }
        }
        else
        {
            Debug.LogWarning($"Failed to find path for unit {unit.name}");
            // Fall back to direct movement or NavMesh
        }
    }
}
```

### **SelectableUnit Movement Integration**
```csharp
// Extend SelectableUnit.cs with grid-based movement
public class UnitMovementController : MonoBehaviour
{
    [Header("Grid Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float nodeReachDistance = 0.1f;
    
    private Vector3[] currentPath;
    private int currentPathIndex = 0;
    private bool isMoving = false;
    
    public void StartMovementAlongPath(Vector3[] path)
    {
        if (path == null || path.Length == 0)
        {
            Debug.LogWarning($"Invalid path provided to {name}");
            return;
        }
        
        currentPath = path;
        currentPathIndex = 0;
        isMoving = true;
        
        Debug.Log($"{name} starting movement along path with {path.Length} nodes");
    }
    
    private void Update()
    {
        if (!isMoving || currentPath == null || currentPathIndex >= currentPath.Length)
        {
            if (isMoving)
            {
                OnMovementComplete();
            }
            return;
        }
        
        Vector3 targetPosition = currentPath[currentPathIndex];
        Vector3 currentPosition = transform.position;
        
        // Move towards target
        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        
        if (distanceToTarget <= nodeReachDistance)
        {
            // Reached current node, move to next
            currentPathIndex++;
            
            if (currentPathIndex >= currentPath.Length)
            {
                OnMovementComplete();
                return;
            }
        }
        
        // Continue moving towards current target
        Vector3 moveDirection = (targetPosition - currentPosition).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
        // Rotate towards movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void OnMovementComplete()
    {
        isMoving = false;
        currentPath = null;
        currentPathIndex = 0;
        
        Debug.Log($"{name} completed movement");
        
        // Notify grid manager that this position is now occupied
        GridManager gridManager = GridManager.Instance;
        if (gridManager != null)
        {
            GridCoordinate currentCoord = gridManager.WorldToGrid(transform.position);
            gridManager.SetNodeOccupied(currentCoord, transform, true);
        }
    }
    
    public void StopMovement()
    {
        isMoving = false;
        currentPath = null;
        currentPathIndex = 0;
    }
    
    public bool IsMoving => isMoving;
    public float GetRemainingDistance()
    {
        if (!isMoving || currentPath == null) return 0f;
        
        float distance = Vector3.Distance(transform.position, currentPath[currentPathIndex]);
        
        for (int i = currentPathIndex + 1; i < currentPath.Length; i++)
        {
            distance += Vector3.Distance(currentPath[i - 1], currentPath[i]);
        }
        
        return distance;
    }
}
```

---

## 🔍 **Testing & Validation**

### **Unit Test Framework**
```csharp
using NUnit.Framework;
using UnityEngine;

public class GridPathfindingTests
{
    private GameObject gridManagerObject;
    private GridManager gridManager;
    
    [SetUp]
    public void Setup()
    {
        gridManagerObject = new GameObject("TestGridManager");
        gridManager = gridManagerObject.AddComponent<GridManager>();
        
        // Create test configuration
        GridConfiguration testConfig = ScriptableObject.CreateInstance<GridConfiguration>();
        testConfig.gridWidth = 10;
        testConfig.gridHeight = 5;
        testConfig.gridDepth = 10;
        testConfig.nodeSize = 1.0f;
        
        gridManager.SetConfiguration(testConfig);
        gridManager.InitializeGrid();
    }
    
    [TearDown]
    public void Teardown()
    {
        if (gridManagerObject != null)
        {
            Object.DestroyImmediate(gridManagerObject);
        }
    }
    
    [Test]
    public void GridInitialization_CreatesCorrectNumberOfNodes()
    {
        Assert.AreEqual(500, gridManager.TotalNodes); // 10*5*10
        Assert.IsTrue(gridManager.IsInitialized);
    }
    
    [Test]
    public void WorldToGrid_ConvertsPositionsCorrectly()
    {
        Vector3 worldPos = new Vector3(2.5f, 1.5f, 3.5f);
        GridCoordinate gridPos = gridManager.WorldToGrid(worldPos);
        
        Assert.AreEqual(2, gridPos.x);
        Assert.AreEqual(1, gridPos.y);
        Assert.AreEqual(3, gridPos.z);
    }
    
    [Test]
    public void GridToWorld_ConvertsCoordinatesCorrectly()
    {
        GridCoordinate gridPos = new GridCoordinate(2, 1, 3);
        Vector3 worldPos = gridManager.GridToWorld(gridPos);
        
        Assert.AreEqual(2.5f, worldPos.x, 0.01f);
        Assert.AreEqual(1.5f, worldPos.y, 0.01f);
        Assert.AreEqual(3.5f, worldPos.z, 0.01f);
    }
    
    [Test]
    public void Pathfinding_FindsValidPath()
    {
        GridCoordinate start = new GridCoordinate(0, 0, 0);
        GridCoordinate end = new GridCoordinate(5, 0, 5);
        
        AStarPathfinder pathfinder = gridManagerObject.AddComponent<AStarPathfinder>();
        GridCoordinate[] path = pathfinder.FindPath(start, end, PathfindingProfile.Default);
        
        Assert.IsNotNull(path);
        Assert.Greater(path.Length, 0);
        Assert.AreEqual(start, path[0]);
        Assert.AreEqual(end, path[path.Length - 1]);
    }
    
    [Test]
    public void VerticalMovement_RespectsMountainRules()
    {
        GridCoordinate start = new GridCoordinate(0, 0, 0);
        GridCoordinate end = new GridCoordinate(0, 3, 0); // 3 levels up
        
        // Block intermediate nodes to force climbing
        gridManager.GetNode(new GridCoordinate(0, 1, 0)).terrainType = TerrainType.Stairs;
        gridManager.GetNode(new GridCoordinate(0, 2, 0)).terrainType = TerrainType.Stairs;
        
        AStarPathfinder pathfinder = gridManagerObject.AddComponent<AStarPathfinder>();
        
        // Should fail without climbable terrain
        PathfindingProfile noClimbProfile = ScriptableObject.CreateInstance<PathfindingProfile>();
        noClimbProfile.maxClimbHeight = 1;
        
        GridCoordinate[] pathNoClimb = pathfinder.FindPath(start, end, noClimbProfile);
        Assert.AreEqual(0, pathNoClimb.Length); // Should fail
        
        // Should succeed with stairs
        PathfindingProfile climbProfile = ScriptableObject.CreateInstance<PathfindingProfile>();
        climbProfile.maxClimbHeight = 3;
        climbProfile.canClimbStairs = true;
        
        GridCoordinate[] pathWithClimb = pathfinder.FindPath(start, end, climbProfile);
        Assert.Greater(pathWithClimb.Length, 0); // Should succeed
    }
}
```

### **Performance Benchmarks**
```csharp
public class GridPerformanceTests
{
    [Test]
    public void PathfindingPerformance_SmallGrid()
    {
        // Test 50x10x50 grid pathfinding performance
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 100; i++)
        {
            // Random pathfinding requests
            GridCoordinate start = GetRandomCoordinate();
            GridCoordinate end = GetRandomCoordinate();
            FindPath(start, end);
        }
        
        stopwatch.Stop();
        float avgTimeMs = stopwatch.ElapsedMilliseconds / 100f;
        
        Assert.Less(avgTimeMs, 5.0f, "Average pathfinding should be under 5ms");
    }
    
    [Test]
    public void GridMemoryUsage_WithinLimits()
    {
        long startMemory = System.GC.GetTotalMemory(true);
        
        // Create large grid
        CreateGrid(100, 20, 100); // 200k nodes
        
        long endMemory = System.GC.GetTotalMemory(true);
        long memoryUsedMB = (endMemory - startMemory) / (1024 * 1024);
        
        Assert.Less(memoryUsedMB, 50, "Grid should use less than 50MB for 200k nodes");
    }
}
```

---

## 📚 **Usage Examples & Code Snippets**

### **Basic Grid Setup**
```csharp
// 1. Create GridManager in scene
GameObject gridManagerObj = new GameObject("GridManager");
GridManager gridManager = gridManagerObj.AddComponent<GridManager>();

// 2. Configure grid settings
GridConfiguration config = Resources.Load<GridConfiguration>("DefaultGridConfig");
gridManager.SetConfiguration(config);

// 3. Initialize grid
gridManager.InitializeGrid();
```

### **Simple Pathfinding Request**
```csharp
// Request a path from unit current position to target
void RequestMovement(Transform unit, Vector3 targetWorldPos)
{
    GridCoordinate start = gridManager.WorldToGrid(unit.position);
    GridCoordinate target = gridManager.WorldToGrid(targetWorldPos);
    
    PathfindingRequest request = new PathfindingRequest(
        unit, start, target,
        (path, success) => {
            if (success)
            {
                MoveUnitAlongPath(unit, path);
            }
            else
            {
                Debug.Log("No path found!");
            }
        }
    );
    
    gridManager.RequestPath(request);
}
```

### **Formation Movement**
```csharp
// Move entire squad in formation
void MoveSquadToPosition(Vector3 targetPos)
{
    SquadPathfinding squadPathfinding = GetComponent<SquadPathfinding>();
    GridCoordinate targetCoord = gridManager.WorldToGrid(targetPos);
    
    squadPathfinding.MoveSquadToPosition(targetCoord);
}
```

### **Obstacle Management**
```csharp
// Add/remove obstacles dynamically
void UpdateGridObstacles()
{
    // Scan for new obstacles in area
    Collider[] obstacles = Physics.OverlapSphere(transform.position, scanRadius, obstacleLayerMask);
    
    foreach (Collider obstacle in obstacles)
    {
        // Mark grid nodes as unwalkable
        Bounds bounds = obstacle.bounds;
        gridManager.SetAreaWalkable(bounds, false);
    }
}
```

---

## 🎯 **Future Enhancements & Roadmap**

### **Phase 2: Advanced Features** (Future)
- [ ] **Hierarchical Pathfinding** - A* + Flow Fields for large maps
- [ ] **Dynamic Obstacles** - Real-time obstacle updates
- [ ] **Path Smoothing** - Bezier curve path optimization
- [ ] **Jump/Climb Points** - Special movement nodes for vertical traversal
- [ ] **Cover Integration** - Pathfinding considers cover positions
- [ ] **Multi-Level Buildings** - Interior/exterior grid connections

### **Phase 3: AI Enhancement** (Future)
- [ ] **Influence Maps** - Threat/safety area calculations
- [ ] **Group Pathfinding** - Flow-field based squad movement
- [ ] **Tactical Positioning** - AI considers cover and flanking
- [ ] **Dynamic Formation** - Adaptive formations based on terrain

### **Phase 4: Performance & Polish** (Future)
- [ ] **Level-of-Detail** - Variable grid resolution
- [ ] **Streaming** - Load/unload grid sections for large worlds
- [ ] **Visual Path Debugging** - In-game path visualization tools
- [ ] **Analytics** - Pathfinding performance metrics

---

## 🎉 **Conclusion**

This Unity 3D Grid Pathfinding System provides a robust foundation for tactical squad-based movement in ProjectZero. Key benefits:

✅ **Unity Native** - Built with Unity best practices and components  
✅ **3D Vertical Movement** - Full support for multi-level tactical gameplay  
✅ **Squad Coordination** - Formation-aware pathfinding for multiple units  
✅ **High Performance** - Job System integration and optimized algorithms  
✅ **Designer Friendly** - ScriptableObject configuration and visual tools  
✅ **Extensible** - Modular design for future enhancements  

The system integrates seamlessly with existing ProjectZero squad management and provides the foundation for advanced tactical AI and formation movement required for the extraction shooter gameplay.

---

**Next Steps:**
1. Review implementation plan and timeline
2. Begin Phase 1 development with core grid system
3. Test integration with existing SquadManager
4. Iterate based on performance and gameplay requirements

**Documentation Version:** 1.0  
**Last Updated:** August 28, 2025  
**Status:** Ready for Implementation
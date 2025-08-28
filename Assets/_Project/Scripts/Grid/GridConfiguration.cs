using UnityEngine;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// ScriptableObject configuration for 3D grid pathfinding system
    /// Allows designers to easily adjust grid parameters without code changes
    /// </summary>
    [CreateAssetMenu(fileName = "GridConfiguration", menuName = "ProjectZero/Grid/Grid Configuration", order = 1)]
    public class GridConfiguration : ScriptableObject
    {
        [Header("Grid Dimensions")]
        [SerializeField] private int _gridWidth = 50;
        [SerializeField] private int _gridHeight = 10;  // Vertical layers
        [SerializeField] private int _gridDepth = 50;
        [SerializeField] private float _nodeSize = 1.0f;
        [SerializeField] private Vector3 _gridOrigin = Vector3.zero;

        [Header("Pathfinding Settings")]
        [SerializeField] private bool _allowDiagonalMovement = true;
        [SerializeField] private bool _allowVerticalMovement = true;
        [SerializeField] private float _maxClimbHeight = 2.0f;
        [SerializeField] private int _maxPathfindingIterations = 10000;
        
        [Header("Movement Costs")]
        [SerializeField] private float _baseCost = 1.0f;
        [SerializeField] private float _diagonalCost = 1.414f;  // √2
        [SerializeField] private float _verticalCost = 2.0f;
        [SerializeField] private float _roughTerrainMultiplier = 1.5f;
        [SerializeField] private float _hazardousTerrainMultiplier = 3.0f;
        [SerializeField] private float _climbableTerrainMultiplier = 2.5f;

        [Header("Performance Settings")]
        [SerializeField] private bool _cacheNeighbors = true;
        [SerializeField] private int _maxConcurrentPathfindingRequests = 4;
        [SerializeField] private float _pathfindingTimeSlice = 0.005f; // 5ms per frame
        [SerializeField] private bool _useUnityJobSystem = false; // For future implementation
        
        [Header("Obstacle Detection")]
        [SerializeField] private LayerMask _obstacleLayerMask = -1;
        [SerializeField] private float _obstacleDetectionRadius = 0.4f;
        [SerializeField] private float _groundDetectionDistance = 2.0f;
        [SerializeField] private LayerMask _groundLayerMask = 1; // Default layer
        
        [Header("Visualization")]
        [SerializeField] private bool _showGridInEditor = true;
        [SerializeField] private bool _showWalkableNodes = true;
        [SerializeField] private bool _showUnwalkableNodes = false;
        [SerializeField] private bool _showVerticalConnections = true;
        [SerializeField] private Color _walkableNodeColor = Color.green;
        [SerializeField] private Color _unwalkableNodeColor = Color.red;
        [SerializeField] private Color _occupiedNodeColor = Color.yellow;
        [SerializeField] private Color _selectedNodeColor = Color.blue;
        [SerializeField] private Color _pathNodeColor = Color.cyan;

        [Header("Debug Settings")]
        [SerializeField] private bool _enablePathfindingDebug = false;
        [SerializeField] private bool _logPathfindingStatistics = false;
        [SerializeField] private float _debugVisualizationDuration = 2.0f;

        // Properties for easy access
        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;
        public int GridDepth => _gridDepth;
        public float NodeSize => _nodeSize;
        public Vector3 GridOrigin => _gridOrigin;
        
        public bool AllowDiagonalMovement => _allowDiagonalMovement;
        public bool AllowVerticalMovement => _allowVerticalMovement;
        public float MaxClimbHeight => _maxClimbHeight;
        public int MaxPathfindingIterations => _maxPathfindingIterations;
        
        public float BaseCost => _baseCost;
        public float DiagonalCost => _diagonalCost;
        public float VerticalCost => _verticalCost;
        public float RoughTerrainMultiplier => _roughTerrainMultiplier;
        public float HazardousTerrainMultiplier => _hazardousTerrainMultiplier;
        public float ClimbableTerrainMultiplier => _climbableTerrainMultiplier;
        
        public bool CacheNeighbors => _cacheNeighbors;
        public int MaxConcurrentPathfindingRequests => _maxConcurrentPathfindingRequests;
        public float PathfindingTimeSlice => _pathfindingTimeSlice;
        public bool UseUnityJobSystem => _useUnityJobSystem;
        
        public LayerMask ObstacleLayerMask => _obstacleLayerMask;
        public float ObstacleDetectionRadius => _obstacleDetectionRadius;
        public float GroundDetectionDistance => _groundDetectionDistance;
        public LayerMask GroundLayerMask => _groundLayerMask;
        
        public bool ShowGridInEditor => _showGridInEditor;
        public bool ShowWalkableNodes => _showWalkableNodes;
        public bool ShowUnwalkableNodes => _showUnwalkableNodes;
        public bool ShowVerticalConnections => _showVerticalConnections;
        public Color WalkableNodeColor => _walkableNodeColor;
        public Color UnwalkableNodeColor => _unwalkableNodeColor;
        public Color OccupiedNodeColor => _occupiedNodeColor;
        public Color SelectedNodeColor => _selectedNodeColor;
        public Color PathNodeColor => _pathNodeColor;
        
        public bool EnablePathfindingDebug => _enablePathfindingDebug;
        public bool LogPathfindingStatistics => _logPathfindingStatistics;
        public float DebugVisualizationDuration => _debugVisualizationDuration;

        /// <summary>
        /// Get the total number of nodes in the grid
        /// </summary>
        public int TotalNodes => GridWidth * GridHeight * GridDepth;

        /// <summary>
        /// Get estimated memory usage in MB
        /// </summary>
        public float EstimatedMemoryUsageMB
        {
            get
            {
                // Rough estimate: each GridNode is approximately 200 bytes
                const int bytesPerNode = 200;
                return (TotalNodes * bytesPerNode) / (1024f * 1024f);
            }
        }

        /// <summary>
        /// Get the world bounds of the grid
        /// </summary>
        public Bounds GetGridBounds()
        {
            Vector3 center = GridOrigin + new Vector3(
                GridWidth * NodeSize * 0.5f,
                GridHeight * NodeSize * 0.5f,
                GridDepth * NodeSize * 0.5f
            );
            
            Vector3 size = new Vector3(
                GridWidth * NodeSize,
                GridHeight * NodeSize,
                GridDepth * NodeSize
            );
            
            return new Bounds(center, size);
        }

        /// <summary>
        /// Check if the given coordinates are within grid bounds
        /// </summary>
        public bool IsWithinBounds(int x, int y, int z)
        {
            return x >= 0 && x < GridWidth && 
                   y >= 0 && y < GridHeight && 
                   z >= 0 && z < GridDepth;
        }

        /// <summary>
        /// Check if the given coordinates are within grid bounds
        /// </summary>
        public bool IsWithinBounds(Vector3Int gridPos)
        {
            return IsWithinBounds(gridPos.x, gridPos.y, gridPos.z);
        }

        /// <summary>
        /// Get movement cost multiplier for a terrain type
        /// </summary>
        public float GetTerrainCostMultiplier(TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.Normal:
                    return 1.0f;
                case TerrainType.Rough:
                    return _roughTerrainMultiplier;
                case TerrainType.Hazardous:
                    return _hazardousTerrainMultiplier;
                case TerrainType.Climbable:
                    return _climbableTerrainMultiplier;
                case TerrainType.Water:
                    return 2.0f; // Slower movement through water
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Get color for a node based on its state
        /// </summary>
        public Color GetNodeColor(GridNode node)
        {
            if (node == null) return Color.white;
            
            if (node.IsSelected) return _selectedNodeColor;
            if (node.IsOccupied) return _occupiedNodeColor;
            if (!node.IsWalkable) return _unwalkableNodeColor;
            return _walkableNodeColor;
        }

        /// <summary>
        /// Validate configuration values
        /// </summary>
        private void OnValidate()
        {
            // Ensure positive values
            _gridWidth = Mathf.Max(1, _gridWidth);
            _gridHeight = Mathf.Max(1, _gridHeight);
            _gridDepth = Mathf.Max(1, _gridDepth);
            _nodeSize = Mathf.Max(0.1f, _nodeSize);
            
            _maxClimbHeight = Mathf.Max(0f, _maxClimbHeight);
            _maxPathfindingIterations = Mathf.Max(100, _maxPathfindingIterations);
            
            _baseCost = Mathf.Max(0.1f, _baseCost);
            _diagonalCost = Mathf.Max(_baseCost, _diagonalCost);
            _verticalCost = Mathf.Max(_baseCost, _verticalCost);
            
            _roughTerrainMultiplier = Mathf.Max(1.0f, _roughTerrainMultiplier);
            _hazardousTerrainMultiplier = Mathf.Max(1.0f, _hazardousTerrainMultiplier);
            _climbableTerrainMultiplier = Mathf.Max(1.0f, _climbableTerrainMultiplier);
            
            _maxConcurrentPathfindingRequests = Mathf.Max(1, _maxConcurrentPathfindingRequests);
            _pathfindingTimeSlice = Mathf.Max(0.001f, _pathfindingTimeSlice);
            
            _obstacleDetectionRadius = Mathf.Max(0.1f, _obstacleDetectionRadius);
            _groundDetectionDistance = Mathf.Max(0.1f, _groundDetectionDistance);
            
            _debugVisualizationDuration = Mathf.Max(0.1f, _debugVisualizationDuration);
            
            // Warn about memory usage
            if (EstimatedMemoryUsageMB > 50f)
            {
                Debug.LogWarning($"GridConfiguration: Large grid detected! Estimated memory usage: {EstimatedMemoryUsageMB:F1} MB. Consider reducing grid size for better performance.");
            }
        }

        /// <summary>
        /// Create a default configuration
        /// </summary>
        [ContextMenu("Reset to Default Values")]
        public void ResetToDefaults()
        {
            _gridWidth = 50;
            _gridHeight = 10;
            _gridDepth = 50;
            _nodeSize = 1.0f;
            _gridOrigin = Vector3.zero;
            
            _allowDiagonalMovement = true;
            _allowVerticalMovement = true;
            _maxClimbHeight = 2.0f;
            _maxPathfindingIterations = 10000;
            
            _baseCost = 1.0f;
            _diagonalCost = 1.414f;
            _verticalCost = 2.0f;
            _roughTerrainMultiplier = 1.5f;
            _hazardousTerrainMultiplier = 3.0f;
            _climbableTerrainMultiplier = 2.5f;
            
            _cacheNeighbors = true;
            _maxConcurrentPathfindingRequests = 4;
            _pathfindingTimeSlice = 0.005f;
            _useUnityJobSystem = false;
            
            _obstacleLayerMask = -1;
            _obstacleDetectionRadius = 0.4f;
            _groundDetectionDistance = 2.0f;
            _groundLayerMask = 1;
            
            _showGridInEditor = true;
            _showWalkableNodes = true;
            _showUnwalkableNodes = false;
            _showVerticalConnections = true;
            _walkableNodeColor = Color.green;
            _unwalkableNodeColor = Color.red;
            _occupiedNodeColor = Color.yellow;
            _selectedNodeColor = Color.blue;
            _pathNodeColor = Color.cyan;
            
            _enablePathfindingDebug = false;
            _logPathfindingStatistics = false;
            _debugVisualizationDuration = 2.0f;
        }
    }
}

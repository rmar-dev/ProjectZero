using UnityEngine;
using System.Collections.Generic;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// 3D grid system that manages the data structure and coordinate conversions for pathfinding
    /// Extends the 2D grid concept to support vertical movement and multi-level navigation
    /// </summary>
    public class GridSystem3D
    {
        private GridConfiguration config;
        private GridNode[,,] grid;
        private Vector3 origin;
        private bool isInitialized = false;

        // Properties
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }
        public float NodeSize { get; private set; }
        public Vector3 Origin => origin;
        public int TotalNodeCount => Width * Height * Depth;
        public bool IsInitialized => isInitialized;

        // Events
        public System.Action OnGridInitialized;
        public System.Action OnGridCleared;

        public GridSystem3D(GridConfiguration configuration)
        {
            config = configuration;
            Initialize();
        }

        /// <summary>
        /// Initialize the 3D grid system
        /// </summary>
        public void Initialize()
        {
            Width = config.GridWidth;
            Height = config.GridHeight;
            Depth = config.GridDepth;
            NodeSize = config.NodeSize;
            origin = config.GridOrigin;

            CreateGrid();
            ScanForObstacles();
            
            if (config.CacheNeighbors)
            {
                CacheAllNeighbors();
            }
            
            isInitialized = true;
            OnGridInitialized?.Invoke();

            Debug.Log($"GridSystem3D initialized: {Width}x{Height}x{Depth}, Total nodes: {TotalNodeCount:N0}, Memory: {GetEstimatedMemoryUsage():F1} MB");
        }

        /// <summary>
        /// Create the 3D grid array and populate with nodes
        /// </summary>
        private void CreateGrid()
        {
            grid = new GridNode[Width, Height, Depth];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        Vector3 worldPos = GridToWorld(x, y, z);
                        grid[x, y, z] = new GridNode(x, y, z, worldPos);
                    }
                }
            }
        }

        /// <summary>
        /// Convert grid coordinates to world position
        /// </summary>
        public Vector3 GridToWorld(int x, int y, int z)
        {
            return origin + new Vector3(x * NodeSize, y * NodeSize, z * NodeSize);
        }

        /// <summary>
        /// Convert grid coordinates to world position
        /// </summary>
        public Vector3 GridToWorld(Vector3Int gridPos)
        {
            return GridToWorld(gridPos.x, gridPos.y, gridPos.z);
        }

        /// <summary>
        /// Convert world position to grid coordinates
        /// </summary>
        public Vector3Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 relativePos = worldPos - origin;
            int x = Mathf.FloorToInt(relativePos.x / NodeSize);
            int y = Mathf.FloorToInt(relativePos.y / NodeSize);
            int z = Mathf.FloorToInt(relativePos.z / NodeSize);

            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Get node at grid coordinates
        /// </summary>
        public GridNode GetNode(int x, int y, int z)
        {
            if (IsValidGridPosition(x, y, z))
                return grid[x, y, z];
            return null;
        }

        /// <summary>
        /// Get node at grid coordinates
        /// </summary>
        public GridNode GetNode(Vector3Int gridPos)
        {
            return GetNode(gridPos.x, gridPos.y, gridPos.z);
        }

        /// <summary>
        /// Get node at world position
        /// </summary>
        public GridNode GetNodeAtWorldPosition(Vector3 worldPos)
        {
            Vector3Int gridPos = WorldToGrid(worldPos);
            return GetNode(gridPos);
        }

        /// <summary>
        /// Check if grid coordinates are valid
        /// </summary>
        public bool IsValidGridPosition(int x, int y, int z)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Depth;
        }

        /// <summary>
        /// Check if grid coordinates are valid
        /// </summary>
        public bool IsValidGridPosition(Vector3Int gridPos)
        {
            return IsValidGridPosition(gridPos.x, gridPos.y, gridPos.z);
        }

        /// <summary>
        /// Get all nodes in a rectangular area
        /// </summary>
        public GridNode[] GetNodesInArea(Vector3Int start, Vector3Int end)
        {
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);
            int minZ = Mathf.Min(start.z, end.z);
            int maxZ = Mathf.Max(start.z, end.z);

            List<GridNode> nodes = new List<GridNode>();

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        if (IsValidGridPosition(x, y, z))
                        {
                            nodes.Add(grid[x, y, z]);
                        }
                    }
                }
            }

            return nodes.ToArray();
        }

        /// <summary>
        /// Get all nodes in a spherical area around a center point
        /// </summary>
        public GridNode[] GetNodesInRadius(Vector3Int center, float radius)
        {
            List<GridNode> nodes = new List<GridNode>();
            int radiusInt = Mathf.CeilToInt(radius);

            for (int x = center.x - radiusInt; x <= center.x + radiusInt; x++)
            {
                for (int y = center.y - radiusInt; y <= center.y + radiusInt; y++)
                {
                    for (int z = center.z - radiusInt; z <= center.z + radiusInt; z++)
                    {
                        if (IsValidGridPosition(x, y, z))
                        {
                            GridNode node = grid[x, y, z];
                            float distance = Vector3.Distance(node.GridPosition, center);
                            
                            if (distance <= radius)
                            {
                                nodes.Add(node);
                            }
                        }
                    }
                }
            }

            return nodes.ToArray();
        }

        /// <summary>
        /// Scan for obstacles using Unity's physics system
        /// </summary>
        public void ScanForObstacles()
        {
            int obstacleCount = 0;
            int unwalkableCount = 0;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        GridNode node = grid[x, y, z];
                        
                        // Check for obstacles at node position
                        bool hasObstacle = Physics.CheckSphere(
                            node.WorldPosition + Vector3.up * (NodeSize * 0.5f),
                            config.ObstacleDetectionRadius,
                            config.ObstacleLayerMask
                        );

                        if (hasObstacle)
                        {
                            node.IsWalkable = false;
                            unwalkableCount++;
                            obstacleCount++;
                        }
                        else
                        {
                            // Check for ground support
                            bool hasGroundSupport = CheckGroundSupport(node);
                            
                            if (!hasGroundSupport && y > 0)
                            {
                                node.IsWalkable = false;
                                unwalkableCount++;
                            }
                            else
                            {
                                node.IsWalkable = true;
                                
                                // Set terrain type based on surroundings
                                node.TerrainType = DetermineTerrainType(node);
                                
                                // Calculate climb height
                                node.ClimbHeight = CalculateClimbHeight(node);
                            }
                        }
                    }
                }
            }

            Debug.Log($"Obstacle scan complete: {obstacleCount} obstacles found, {unwalkableCount} unwalkable nodes, {TotalNodeCount - unwalkableCount} walkable nodes");
        }

        /// <summary>
        /// Check if a node has ground support (for nodes above ground level)
        /// </summary>
        private bool CheckGroundSupport(GridNode node)
        {
            if (node.Y == 0) return true; // Ground level always has support

            // Check if there's a walkable node directly below
            GridNode nodeBelow = GetNode(node.X, node.Y - 1, node.Z);
            if (nodeBelow != null && nodeBelow.IsWalkable)
                return true;

            // Check for ground using raycast
            Vector3 rayStart = node.WorldPosition + Vector3.up * (NodeSize * 0.1f);
            Vector3 rayDirection = Vector3.down;
            float rayDistance = config.GroundDetectionDistance;

            return Physics.Raycast(rayStart, rayDirection, rayDistance, config.GroundLayerMask);
        }

        /// <summary>
        /// Determine terrain type for a node based on surroundings
        /// </summary>
        private TerrainType DetermineTerrainType(GridNode node)
        {
            // Simple terrain detection - can be expanded based on your needs
            TerrainType terrain = TerrainType.Normal;

            // Check for water
            if (Physics.CheckSphere(node.WorldPosition, NodeSize * 0.3f, LayerMask.GetMask("Water")))
            {
                terrain |= TerrainType.Water;
            }

            // Check for rough terrain
            if (Physics.CheckSphere(node.WorldPosition, NodeSize * 0.4f, LayerMask.GetMask("RoughTerrain")))
            {
                terrain |= TerrainType.Rough;
            }

            // Check for hazardous terrain
            if (Physics.CheckSphere(node.WorldPosition, NodeSize * 0.4f, LayerMask.GetMask("Hazards")))
            {
                terrain |= TerrainType.Hazardous;
            }

            // Check for climbable surfaces nearby
            if (HasClimbableSurfaceNearby(node))
            {
                terrain |= TerrainType.Climbable;
            }

            // Check if indoor or outdoor (simplified)
            if (Physics.Raycast(node.WorldPosition, Vector3.up, 10f, LayerMask.GetMask("Ceiling")))
            {
                terrain |= TerrainType.Indoor;
            }
            else
            {
                terrain |= TerrainType.Outdoor;
            }

            return terrain;
        }

        /// <summary>
        /// Check if there are climbable surfaces near a node
        /// </summary>
        private bool HasClimbableSurfaceNearby(GridNode node)
        {
            // Check adjacent positions for vertical surfaces that can be climbed
            Vector3[] directions = {
                Vector3.forward, Vector3.back, Vector3.left, Vector3.right
            };

            foreach (Vector3 direction in directions)
            {
                if (Physics.Raycast(node.WorldPosition, direction, NodeSize * 0.6f, LayerMask.GetMask("Walls", "Climbable")))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculate the height above ground for a node
        /// </summary>
        private float CalculateClimbHeight(GridNode node)
        {
            if (node.Y == 0) return 0f;

            // Simple height calculation - distance from ground level
            return node.Y * NodeSize;
        }

        /// <summary>
        /// Cache neighbors for all nodes to improve pathfinding performance
        /// </summary>
        private void CacheAllNeighbors()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        GridNode node = grid[x, y, z];
                        List<GridNode> neighbors = GetNeighborsForCaching(node);
                        node.SetNeighbors(neighbors);
                    }
                }
            }

            Debug.Log($"Neighbor caching complete for {TotalNodeCount:N0} nodes");
        }

        /// <summary>
        /// Get neighbors for a node (used during caching)
        /// </summary>
        private List<GridNode> GetNeighborsForCaching(GridNode node)
        {
            List<GridNode> neighbors = new List<GridNode>();
            Vector3Int nodePos = node.GridPosition;

            // 6-directional movement (cardinal directions + up/down)
            Vector3Int[] directions = {
                new Vector3Int(1, 0, 0),   // East
                new Vector3Int(-1, 0, 0),  // West  
                new Vector3Int(0, 0, 1),   // North
                new Vector3Int(0, 0, -1),  // South
                new Vector3Int(0, 1, 0),   // Up
                new Vector3Int(0, -1, 0)   // Down
            };

            // Add diagonal directions if enabled
            if (config.AllowDiagonalMovement)
            {
                Vector3Int[] diagonalDirections = {
                    new Vector3Int(1, 0, 1),   // Northeast
                    new Vector3Int(-1, 0, 1),  // Northwest
                    new Vector3Int(1, 0, -1),  // Southeast
                    new Vector3Int(-1, 0, -1)  // Southwest
                };

                // Combine arrays
                Vector3Int[] allDirections = new Vector3Int[directions.Length + diagonalDirections.Length];
                directions.CopyTo(allDirections, 0);
                diagonalDirections.CopyTo(allDirections, directions.Length);
                directions = allDirections;
            }

            foreach (Vector3Int direction in directions)
            {
                Vector3Int neighborPos = nodePos + direction;
                GridNode neighbor = GetNode(neighborPos);
                
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Reset pathfinding data for all nodes
        /// </summary>
        public void ResetAllNodesForPathfinding()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        grid[x, y, z].ResetPathfindingData();
                    }
                }
            }
        }

        /// <summary>
        /// Clear all node selections
        /// </summary>
        public void ClearAllSelections()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        grid[x, y, z].ClearStates();
                    }
                }
            }
        }

        /// <summary>
        /// Get estimated memory usage in MB
        /// </summary>
        public float GetEstimatedMemoryUsage()
        {
            // Rough estimate: each GridNode is approximately 200 bytes
            const int bytesPerNode = 200;
            return (TotalNodeCount * bytesPerNode) / (1024f * 1024f);
        }

        /// <summary>
        /// Get grid bounds in world space
        /// </summary>
        public Bounds GetWorldBounds()
        {
            Vector3 center = origin + new Vector3(
                Width * NodeSize * 0.5f,
                Height * NodeSize * 0.5f,
                Depth * NodeSize * 0.5f
            );
            
            Vector3 size = new Vector3(
                Width * NodeSize,
                Height * NodeSize,
                Depth * NodeSize
            );
            
            return new Bounds(center, size);
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            if (grid != null)
            {
                // Clear all node references
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        for (int z = 0; z < Depth; z++)
                        {
                            grid[x, y, z] = null;
                        }
                    }
                }
                
                grid = null;
            }
            
            isInitialized = false;
            OnGridCleared?.Invoke();
            
            Debug.Log("GridSystem3D disposed");
        }
    }
}

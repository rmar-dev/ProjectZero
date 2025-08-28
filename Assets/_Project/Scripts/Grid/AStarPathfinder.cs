using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// A* pathfinding algorithm implementation for 3D grid navigation
    /// Supports 6-directional movement, diagonal movement, and vertical movement
    /// </summary>
    public class AStarPathfinder
    {
        private GridSystem3D gridSystem;
        private GridConfiguration config;
        private PathfindingHeap<GridNode> openSet;
        private HashSet<GridNode> closedSet;
        private List<GridNode> path;
        
        // Statistics for debugging
        public struct PathfindingStats
        {
            public int NodesExplored;
            public int NodesInOpenSet;
            public int NodesInClosedSet;
            public float TimeElapsed;
            public bool PathFound;
            public float PathLength;
            public int PathNodeCount;
        }
        
        public PathfindingStats LastStats { get; private set; }

        public AStarPathfinder(GridSystem3D gridSystem, GridConfiguration config)
        {
            this.gridSystem = gridSystem;
            this.config = config;
            
            openSet = new PathfindingHeap<GridNode>(gridSystem.TotalNodeCount);
            closedSet = new HashSet<GridNode>();
            path = new List<GridNode>();
        }

        /// <summary>
        /// Find path from start position to target position
        /// </summary>
        public List<GridNode> FindPath(Vector3 startWorldPos, Vector3 targetWorldPos, PathfindingProfile profile = null)
        {
            Vector3Int startGrid = gridSystem.WorldToGrid(startWorldPos);
            Vector3Int targetGrid = gridSystem.WorldToGrid(targetWorldPos);
            
            return FindPath(startGrid, targetGrid, profile);
        }

        /// <summary>
        /// Find path from start grid position to target grid position
        /// </summary>
        public List<GridNode> FindPath(Vector3Int startGridPos, Vector3Int targetGridPos, PathfindingProfile profile = null)
        {
            GridNode startNode = gridSystem.GetNode(startGridPos);
            GridNode targetNode = gridSystem.GetNode(targetGridPos);
            
            return FindPath(startNode, targetNode, profile);
        }

        /// <summary>
        /// Core A* pathfinding algorithm
        /// </summary>
        public List<GridNode> FindPath(GridNode startNode, GridNode targetNode, PathfindingProfile profile = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Reset statistics
            var stats = new PathfindingStats
            {
                NodesExplored = 0,
                PathFound = false,
                TimeElapsed = 0f,
                PathLength = 0f,
                PathNodeCount = 0
            };

            // Validate inputs
            if (startNode == null || targetNode == null)
            {
                Debug.LogWarning("AStarPathfinder: Invalid start or target node");
                LastStats = stats;
                return new List<GridNode>();
            }

            if (!targetNode.IsWalkable)
            {
                Debug.LogWarning("AStarPathfinder: Target node is not walkable");
                LastStats = stats;
                return new List<GridNode>();
            }

            // Use default profile if none provided
            if (profile == null)
                profile = PathfindingProfile.CreateDefault();

            // Initialize pathfinding data structures
            openSet.Clear();
            closedSet.Clear();
            
            // Reset all nodes in the grid
            ResetAllNodesForPathfinding();

            // Initialize start node
            startNode.GCost = 0;
            startNode.HCost = GetHeuristic(startNode, targetNode);
            openSet.Add(startNode);

            int iterations = 0;
            const int maxIterations = 10000; // Safety limit

            while (openSet.Count > 0 && iterations < maxIterations)
            {
                iterations++;
                stats.NodesExplored++;

                GridNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                // Check if we reached the target
                if (currentNode == targetNode)
                {
                    path = ReconstructPath(startNode, targetNode);
                    stats.PathFound = true;
                    stats.PathNodeCount = path.Count;
                    stats.PathLength = CalculatePathLength(path);
                    break;
                }

                // Explore neighbors
                List<GridNode> neighbors = GetNeighbors(currentNode, profile);
                foreach (GridNode neighbor in neighbors)
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    if (!neighbor.CanEnter(profile.MaxClimbHeight, profile.AllowedTerrainTypes))
                        continue;

                    float tentativeGCost = currentNode.GCost + GetMovementCost(currentNode, neighbor, profile);

                    if (tentativeGCost < neighbor.GCost)
                    {
                        neighbor.Parent = currentNode;
                        neighbor.GCost = tentativeGCost;
                        neighbor.HCost = GetHeuristic(neighbor, targetNode);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbor);
                        }
                    }
                }
            }

            stopwatch.Stop();
            stats.TimeElapsed = stopwatch.ElapsedMilliseconds;
            stats.NodesInOpenSet = openSet.Count;
            stats.NodesInClosedSet = closedSet.Count;
            
            LastStats = stats;

            // Log statistics if enabled
            if (config.LogPathfindingStatistics)
            {
                LogPathfindingStatistics(stats, startNode, targetNode);
            }

            return path;
        }

        /// <summary>
        /// Get valid neighbors for a given node
        /// </summary>
        private List<GridNode> GetNeighbors(GridNode node, PathfindingProfile profile)
        {
            List<GridNode> neighbors = new List<GridNode>();
            Vector3Int nodePos = node.GridPosition;

            // 6-directional movement (cardinal directions + up/down)
            Vector3Int[] directions = {
                new Vector3Int(1, 0, 0),   // East
                new Vector3Int(-1, 0, 0),  // West
                new Vector3Int(0, 0, 1),   // North
                new Vector3Int(0, 0, -1),  // South
            };

            // Add vertical movement if allowed
            if (config.AllowVerticalMovement && profile.AllowVerticalMovement)
            {
                System.Array.Resize(ref directions, 6);
                directions[4] = new Vector3Int(0, 1, 0);   // Up
                directions[5] = new Vector3Int(0, -1, 0);  // Down
            }

            // Add diagonal movement if allowed
            List<Vector3Int> diagonalDirections = new List<Vector3Int>();
            if (config.AllowDiagonalMovement && profile.AllowDiagonalMovement)
            {
                diagonalDirections.AddRange(new Vector3Int[] {
                    new Vector3Int(1, 0, 1),   // Northeast
                    new Vector3Int(-1, 0, 1),  // Northwest
                    new Vector3Int(1, 0, -1),  // Southeast
                    new Vector3Int(-1, 0, -1), // Southwest
                });
            }

            // Process cardinal and vertical directions
            foreach (Vector3Int direction in directions)
            {
                Vector3Int neighborPos = nodePos + direction;
                GridNode neighbor = gridSystem.GetNode(neighborPos);
                
                if (neighbor != null && IsValidNeighbor(node, neighbor, profile))
                {
                    neighbors.Add(neighbor);
                }
            }

            // Process diagonal directions
            foreach (Vector3Int direction in diagonalDirections)
            {
                Vector3Int neighborPos = nodePos + direction;
                GridNode neighbor = gridSystem.GetNode(neighborPos);
                
                if (neighbor != null && IsValidDiagonalNeighbor(node, neighbor, profile))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Check if a neighbor is valid for pathfinding
        /// </summary>
        private bool IsValidNeighbor(GridNode current, GridNode neighbor, PathfindingProfile profile)
        {
            // Basic walkability check
            if (!neighbor.IsWalkable || neighbor.IsOccupied)
                return false;

            // Check vertical movement constraints
            if (neighbor.IsVerticalTo(current))
            {
                return IsValidVerticalMovement(current, neighbor, profile);
            }

            return true;
        }

        /// <summary>
        /// Check if a diagonal neighbor is valid (ensuring no corner cutting)
        /// </summary>
        private bool IsValidDiagonalNeighbor(GridNode current, GridNode neighbor, PathfindingProfile profile)
        {
            if (!IsValidNeighbor(current, neighbor, profile))
                return false;

            // Prevent corner cutting - check that both cardinal neighbors are walkable
            Vector3Int currentPos = current.GridPosition;
            Vector3Int neighborPos = neighbor.GridPosition;
            
            Vector3Int cardinalNeighbor1 = new Vector3Int(neighborPos.x, currentPos.y, currentPos.z);
            Vector3Int cardinalNeighbor2 = new Vector3Int(currentPos.x, currentPos.y, neighborPos.z);
            
            GridNode node1 = gridSystem.GetNode(cardinalNeighbor1);
            GridNode node2 = gridSystem.GetNode(cardinalNeighbor2);
            
            return (node1?.IsWalkable ?? false) && (node2?.IsWalkable ?? false);
        }

        /// <summary>
        /// Check if vertical movement between nodes is valid
        /// </summary>
        private bool IsValidVerticalMovement(GridNode current, GridNode neighbor, PathfindingProfile profile)
        {
            float heightDifference = Mathf.Abs(neighbor.ClimbHeight - current.ClimbHeight);
            
            // Check climb height capability
            if (heightDifference > profile.MaxClimbHeight)
                return false;

            // Check if terrain supports climbing (if going up)
            if (neighbor.Y > current.Y)
            {
                return current.HasTerrainType(TerrainType.Climbable) || 
                       neighbor.HasTerrainType(TerrainType.Climbable) ||
                       heightDifference <= 0.5f; // Small steps don't require climbable terrain
            }

            return true; // Falling/going down is generally allowed
        }

        /// <summary>
        /// Calculate movement cost between two adjacent nodes
        /// </summary>
        private float GetMovementCost(GridNode from, GridNode to, PathfindingProfile profile)
        {
            float baseCost = config.BaseCost;
            
            // Apply terrain cost
            float terrainCost = config.GetTerrainCostMultiplier(to.TerrainType);
            baseCost *= terrainCost;
            
            // Apply profile-specific multipliers
            baseCost *= profile.GetMovementMultiplier(to.TerrainType);
            
            // Apply diagonal movement cost
            if (from.IsDiagonalTo(to))
            {
                baseCost *= config.DiagonalCost;
            }
            
            // Apply vertical movement cost
            if (from.IsVerticalTo(to))
            {
                baseCost *= config.VerticalCost;
                
                // Additional cost for climbing up
                if (to.Y > from.Y)
                {
                    float heightDifference = to.ClimbHeight - from.ClimbHeight;
                    baseCost += heightDifference * 0.5f; // Additional climbing penalty
                }
            }
            
            // Apply custom movement cost from nodes
            baseCost += from.GetMovementCostTo(to);
            
            return baseCost;
        }

        /// <summary>
        /// Calculate heuristic distance to target (Manhattan distance with vertical bias)
        /// </summary>
        private float GetHeuristic(GridNode from, GridNode to)
        {
            return from.DistanceTo(to);
        }

        /// <summary>
        /// Reconstruct path from target back to start using parent nodes
        /// </summary>
        private List<GridNode> ReconstructPath(GridNode startNode, GridNode targetNode)
        {
            List<GridNode> path = new List<GridNode>();
            GridNode currentNode = targetNode;
            
            while (currentNode != startNode && currentNode != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            
            if (currentNode == startNode)
            {
                path.Add(startNode);
            }
            
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Calculate the total length of a path
        /// </summary>
        private float CalculatePathLength(List<GridNode> path)
        {
            if (path.Count < 2) return 0f;
            
            float totalLength = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                totalLength += path[i].EuclideanDistanceTo(path[i + 1]);
            }
            
            return totalLength;
        }

        /// <summary>
        /// Reset pathfinding data for all nodes in the grid
        /// </summary>
        private void ResetAllNodesForPathfinding()
        {
            // This would be called on the grid system to reset all nodes
            gridSystem.ResetAllNodesForPathfinding();
        }

        /// <summary>
        /// Log pathfinding statistics for debugging
        /// </summary>
        private void LogPathfindingStatistics(PathfindingStats stats, GridNode startNode, GridNode targetNode)
        {
            string message = $"Pathfinding Complete:\n" +
                           $"  Start: {startNode.GridPosition}\n" +
                           $"  Target: {targetNode.GridPosition}\n" +
                           $"  Path Found: {stats.PathFound}\n" +
                           $"  Nodes Explored: {stats.NodesExplored}\n" +
                           $"  Path Length: {stats.PathLength:F2}\n" +
                           $"  Path Node Count: {stats.PathNodeCount}\n" +
                           $"  Time Elapsed: {stats.TimeElapsed}ms\n" +
                           $"  Open Set Size: {stats.NodesInOpenSet}\n" +
                           $"  Closed Set Size: {stats.NodesInClosedSet}";
            
            if (stats.PathFound)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogWarning($"Pathfinding Failed:\n{message}");
            }
        }

        /// <summary>
        /// Smooth path by removing unnecessary waypoints (optional post-processing)
        /// </summary>
        public List<GridNode> SmoothPath(List<GridNode> originalPath)
        {
            if (originalPath.Count <= 2) return originalPath;

            List<GridNode> smoothedPath = new List<GridNode> { originalPath[0] };
            
            for (int i = 1; i < originalPath.Count - 1; i++)
            {
                GridNode prev = smoothedPath[smoothedPath.Count - 1];
                GridNode current = originalPath[i];
                GridNode next = originalPath[i + 1];
                
                // Check if we can skip the current waypoint by going directly to the next
                if (!HasLineOfSight(prev, next))
                {
                    smoothedPath.Add(current);
                }
            }
            
            smoothedPath.Add(originalPath[originalPath.Count - 1]);
            return smoothedPath;
        }

        /// <summary>
        /// Check if there's a clear line of sight between two nodes
        /// </summary>
        private bool HasLineOfSight(GridNode from, GridNode to)
        {
            // Simple raycasting between nodes to check for obstacles
            Vector3 direction = (to.WorldPosition - from.WorldPosition).normalized;
            float distance = Vector3.Distance(from.WorldPosition, to.WorldPosition);
            
            return !Physics.Raycast(from.WorldPosition + Vector3.up * 0.1f, direction, distance, config.ObstacleLayerMask);
        }
    }
}

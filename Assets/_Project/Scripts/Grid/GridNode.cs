using UnityEngine;
using System.Collections.Generic;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// Enhanced grid node for A* pathfinding with 3D coordinates and pathfinding properties
    /// Replaces the simpler GridCell for advanced pathfinding scenarios
    /// </summary>
    [System.Serializable]
    public class GridNode : System.IComparable<GridNode>
    {
        [Header("Grid Coordinates")]
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        
        [Header("World Position")]
        public Vector3 WorldPosition { get; private set; }
        
        [Header("Node States")]
        public bool IsWalkable { get; set; }
        public bool IsOccupied { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHighlighted { get; set; }
        
        [Header("Pathfinding Properties")]
        public float GCost { get; set; }  // Distance from starting node
        public float HCost { get; set; }  // Distance to target node (heuristic)
        public float FCost => GCost + HCost;  // Total cost
        
        [Header("Terrain & Movement")]
        public TerrainType TerrainType { get; set; }
        public float MovementCost { get; set; }
        public float ClimbHeight { get; set; }  // How high this node is above ground
        
        [Header("Tactical Properties")]
        public bool ProvidesCover { get; set; }
        public CoverType CoverType { get; set; }
        public Vector3 CoverDirection { get; set; }  // Direction the cover faces
        
        [Header("Pathfinding Data")]
        public GridNode Parent { get; set; }  // For path reconstruction
        public int HeapIndex { get; set; }    // For binary heap optimization
        
        // Cached neighbors for performance
        private List<GridNode> _neighbors;
        private bool _neighborsInitialized = false;
        
        // Optional gameplay data
        public object GameplayData { get; set; }

        public GridNode(int x, int y, int z, Vector3 worldPosition)
        {
            X = x;
            Y = y;
            Z = z;
            WorldPosition = worldPosition;
            
            // Default values
            IsWalkable = true;
            IsOccupied = false;
            IsSelected = false;
            IsHighlighted = false;
            
            TerrainType = TerrainType.Normal;
            MovementCost = 1.0f;
            ClimbHeight = 0f;
            
            ProvidesCover = false;
            CoverType = CoverType.None;
            CoverDirection = Vector3.zero;
            
            ResetPathfindingData();
        }

        /// <summary>
        /// Get grid coordinates as Vector3Int
        /// </summary>
        public Vector3Int GridPosition => new Vector3Int(X, Y, Z);
        
        /// <summary>
        /// Get 2D grid coordinates (X, Z) as Vector2Int for compatibility
        /// </summary>
        public Vector2Int GridPosition2D => new Vector2Int(X, Z);

        /// <summary>
        /// Reset pathfinding data for a new pathfinding operation
        /// </summary>
        public void ResetPathfindingData()
        {
            GCost = float.MaxValue;
            HCost = 0f;
            Parent = null;
            HeapIndex = 0;
        }

        /// <summary>
        /// Check if this node can be entered by a unit with given movement capabilities
        /// </summary>
        public bool CanEnter(float unitClimbHeight, TerrainType allowedTerrain = TerrainType.All)
        {
            if (!IsWalkable || IsOccupied)
                return false;
                
            // Check climb height capability
            if (ClimbHeight > unitClimbHeight)
                return false;
                
            // Check terrain compatibility
            if (allowedTerrain != TerrainType.All && !HasTerrainType(allowedTerrain))
                return false;
                
            return true;
        }

        /// <summary>
        /// Check if this node has the specified terrain type
        /// </summary>
        public bool HasTerrainType(TerrainType terrain)
        {
            return (TerrainType & terrain) != 0;
        }

        /// <summary>
        /// Get the movement cost from this node to a neighbor
        /// </summary>
        public float GetMovementCostTo(GridNode neighbor)
        {
            if (neighbor == null)
                return float.MaxValue;
                
            float baseCost = MovementCost + neighbor.MovementCost;
            
            // Add diagonal movement penalty
            if (IsDiagonalTo(neighbor))
                baseCost *= 1.414f; // √2
                
            // Add vertical movement penalty
            float heightDifference = Mathf.Abs(neighbor.ClimbHeight - ClimbHeight);
            if (heightDifference > 0.1f)
                baseCost += heightDifference * 2f; // Climbing is more expensive
                
            return baseCost;
        }

        /// <summary>
        /// Check if another node is diagonal to this one (in 2D plane)
        /// </summary>
        public bool IsDiagonalTo(GridNode other)
        {
            if (other == null) return false;
            
            int deltaX = Mathf.Abs(X - other.X);
            int deltaZ = Mathf.Abs(Z - other.Z);
            
            return deltaX == 1 && deltaZ == 1 && Y == other.Y;
        }

        /// <summary>
        /// Check if another node is vertically connected to this one
        /// </summary>
        public bool IsVerticalTo(GridNode other)
        {
            if (other == null) return false;
            
            return X == other.X && Z == other.Z && Mathf.Abs(Y - other.Y) == 1;
        }

        /// <summary>
        /// Get distance to another node (for heuristic calculations)
        /// </summary>
        public float DistanceTo(GridNode other)
        {
            if (other == null) return float.MaxValue;
            
            // 3D Manhattan distance with consideration for vertical movement cost
            float deltaX = Mathf.Abs(X - other.X);
            float deltaY = Mathf.Abs(Y - other.Y);
            float deltaZ = Mathf.Abs(Z - other.Z);
            
            // Vertical movement is more expensive
            return deltaX + deltaZ + (deltaY * 2f);
        }

        /// <summary>
        /// Get Euclidean distance to another node
        /// </summary>
        public float EuclideanDistanceTo(GridNode other)
        {
            if (other == null) return float.MaxValue;
            
            return Vector3.Distance(WorldPosition, other.WorldPosition);
        }

        /// <summary>
        /// Set neighbor cache (called by GridSystem during initialization)
        /// </summary>
        public void SetNeighbors(List<GridNode> neighbors)
        {
            _neighbors = new List<GridNode>(neighbors);
            _neighborsInitialized = true;
        }

        /// <summary>
        /// Get cached neighbors (if available)
        /// </summary>
        public List<GridNode> GetNeighbors()
        {
            return _neighborsInitialized ? new List<GridNode>(_neighbors) : new List<GridNode>();
        }

        /// <summary>
        /// Clear all states (selection, highlighting, etc.)
        /// </summary>
        public void ClearStates()
        {
            IsSelected = false;
            IsHighlighted = false;
        }

        /// <summary>
        /// Get center world position of the node
        /// </summary>
        public Vector3 GetCenterWorldPosition(float nodeSize)
        {
            return WorldPosition + new Vector3(nodeSize * 0.5f, 0, nodeSize * 0.5f);
        }

        /// <summary>
        /// Check if this node is suitable for cover from a given direction
        /// </summary>
        public bool ProvidesCoverFrom(Vector3 threatDirection)
        {
            if (!ProvidesCover) return false;
            
            // Simple dot product check - cover is effective if threat is from the covered direction
            float alignment = Vector3.Dot(CoverDirection.normalized, threatDirection.normalized);
            return alignment > 0.5f; // Cover is effective if directions are reasonably aligned
        }

        #region IComparable Implementation for Priority Queue
        
        public int CompareTo(GridNode other)
        {
            if (other == null) return -1;
            
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
            {
                compare = HCost.CompareTo(other.HCost);
            }
            return compare;
        }

        #endregion

        public override string ToString()
        {
            return $"GridNode({X}, {Y}, {Z}) - Walkable: {IsWalkable}, Occupied: {IsOccupied}, FCost: {FCost:F1}";
        }

        public override bool Equals(object obj)
        {
            if (obj is GridNode other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2;
        }
    }

    /// <summary>
    /// Terrain types that affect movement and pathfinding
    /// </summary>
    [System.Flags]
    public enum TerrainType
    {
        None = 0,
        Normal = 1 << 0,      // Standard walkable terrain
        Rough = 1 << 1,       // Difficult terrain (higher movement cost)
        Hazardous = 1 << 2,   // Dangerous terrain (damage or debuffs)
        Climbable = 1 << 3,   // Can be climbed (walls, ladders, etc.)
        Water = 1 << 4,       // Water terrain (may require special movement)
        Indoor = 1 << 5,      // Interior spaces
        Outdoor = 1 << 6,     // Exterior spaces
        All = ~0              // All terrain types
    }

    /// <summary>
    /// Types of cover a node can provide
    /// </summary>
    public enum CoverType
    {
        None,           // No cover
        Partial,        // Partial cover (+2 AC in D&D terms)
        Full,           // Full cover (line of sight blocked)
        Concealment     // Concealment only (harder to target but not physical protection)
    }
}

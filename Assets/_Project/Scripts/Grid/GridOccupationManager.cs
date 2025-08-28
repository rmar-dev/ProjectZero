using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// Manages grid occupation for both static and dynamic objects efficiently
    /// </summary>
    public class GridOccupationManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GridManager3D gridManager;
        [SerializeField] private LayerMask staticObjectLayers = -1;
        [SerializeField] private LayerMask dynamicObjectLayers = -1;
        [SerializeField] private float dynamicUpdateInterval = 0.1f;
        [SerializeField] private int maxDynamicUpdatesPerFrame = 50;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool showStaticOccupation = true;
        [SerializeField] private bool showDynamicOccupation = true;
        
        // Occupation tracking
        private HashSet<GridNode> staticOccupiedNodes = new HashSet<GridNode>();
        private HashSet<GridNode> dynamicOccupiedNodes = new HashSet<GridNode>();
        private Dictionary<GameObject, HashSet<GridNode>> dynamicObjectNodes = new Dictionary<GameObject, HashSet<GridNode>>();
        
        // Performance optimization
        private List<IDynamicGridObject> trackedDynamicObjects = new List<IDynamicGridObject>();
        private Queue<IDynamicGridObject> updateQueue = new Queue<IDynamicGridObject>();
        private Coroutine dynamicUpdateCoroutine;
        
        // Spatial hashing for performance
        private Dictionary<int, HashSet<GridNode>> spatialBuckets = new Dictionary<int, HashSet<GridNode>>();
        
        public System.Action<GridNode, bool> OnNodeOccupationChanged;
        
        private void Start()
        {
            if (!gridManager)
                gridManager = FindObjectOfType<GridManager3D>();
            
            if (gridManager)
            {
                gridManager.OnGridInitialized += OnGridInitialized;
                if (gridManager.IsInitialized)
                    OnGridInitialized();
            }
        }
        
        private void OnGridInitialized()
        {
            StartCoroutine(InitializeOccupationSystem());
        }
        
        /// <summary>
        /// Initialize the occupation system after grid is ready
        /// </summary>
        private IEnumerator InitializeOccupationSystem()
        {
            yield return null; // Wait one frame
            
            Debug.Log("GridOccupationManager: Initializing occupation system...");
            
            // Step 1: Initialize spatial buckets
            InitializeSpatialBuckets();
            yield return null;
            
            // Step 2: Scan for static objects
            yield return StartCoroutine(ScanStaticObjects());
            
            // Step 3: Start tracking dynamic objects
            StartDynamicTracking();
            
            // Step 4: Start dynamic update coroutine
            if (dynamicUpdateCoroutine != null)
                StopCoroutine(dynamicUpdateCoroutine);
            dynamicUpdateCoroutine = StartCoroutine(DynamicUpdateLoop());
            
            Debug.Log($"GridOccupationManager: Initialized. Static: {staticOccupiedNodes.Count}, Dynamic trackers: {trackedDynamicObjects.Count}");
        }
        
        /// <summary>
        /// Initialize spatial hashing buckets for performance
        /// </summary>
        private void InitializeSpatialBuckets()
        {
            GridSystem3D grid = gridManager.GridSystem;
            
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    for (int z = 0; z < grid.Depth; z++)
                    {
                        GridNode node = grid.GetNode(x, y, z);
                        if (node != null)
                        {
                            int hash = SpatialHash(x, y, z);
                            if (!spatialBuckets.ContainsKey(hash))
                                spatialBuckets[hash] = new HashSet<GridNode>();
                            spatialBuckets[hash].Add(node);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Scan and mark all static objects in the scene
        /// </summary>
        private IEnumerator ScanStaticObjects()
        {
            Debug.Log("GridOccupationManager: Scanning static objects...");
            
            // Find all static colliders in the scene
            Collider[] staticColliders = FindObjectsOfType<Collider>();
            int processed = 0;
            
            foreach (Collider col in staticColliders)
            {
                // Skip if not in static layers or is a trigger
                if (!IsInLayerMask(col.gameObject.layer, staticObjectLayers) || col.isTrigger)
                    continue;
                
                // Skip if it's a dynamic object
                if (col.GetComponent<IDynamicGridObject>() != null)
                    continue;
                
                // Mark nodes as statically occupied
                MarkStaticObjectOccupation(col);
                processed++;
                
                // Yield every 10 objects to prevent frame drops
                if (processed % 10 == 0)
                    yield return null;
            }
            
            Debug.Log($"GridOccupationManager: Processed {processed} static objects, {staticOccupiedNodes.Count} nodes occupied");
        }
        
        /// <summary>
        /// Mark nodes occupied by a static object
        /// </summary>
        private void MarkStaticObjectOccupation(Collider staticCollider)
        {
            Bounds bounds = staticCollider.bounds;
            GridSystem3D grid = gridManager.GridSystem;
            
            // Convert world bounds to grid coordinates
            Vector3Int minCoord = WorldToGridCoordinate(bounds.min);
            Vector3Int maxCoord = WorldToGridCoordinate(bounds.max);
            
            // Clamp to grid bounds
            minCoord.x = Mathf.Max(0, minCoord.x);
            minCoord.y = Mathf.Max(0, minCoord.y);
            minCoord.z = Mathf.Max(0, minCoord.z);
            maxCoord.x = Mathf.Min(grid.Width - 1, maxCoord.x);
            maxCoord.y = Mathf.Min(grid.Height - 1, maxCoord.y);
            maxCoord.z = Mathf.Min(grid.Depth - 1, maxCoord.z);
            
            // Mark all overlapping nodes
            for (int x = minCoord.x; x <= maxCoord.x; x++)
            {
                for (int y = minCoord.y; y <= maxCoord.y; y++)
                {
                    for (int z = minCoord.z; z <= maxCoord.z; z++)
                    {
                        GridNode node = grid.GetNode(x, y, z);
                        if (node != null)
                        {
                            // Double-check with precise collision detection
                            if (IsColliderOverlappingNode(staticCollider, node))
                            {
                                SetNodeStaticOccupation(node, true);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Start tracking all dynamic objects in the scene
        /// </summary>
        private void StartDynamicTracking()
        {
            // Find all objects implementing IDynamicGridObject
            IDynamicGridObject[] dynamicObjects = FindObjectsOfType<MonoBehaviour>()
                .OfType<IDynamicGridObject>()
                .ToArray();
            
            foreach (var dynObj in dynamicObjects)
            {
                RegisterDynamicObject(dynObj);
            }
            
            Debug.Log($"GridOccupationManager: Started tracking {trackedDynamicObjects.Count} dynamic objects");
        }
        
        /// <summary>
        /// Register a dynamic object for tracking
        /// </summary>
        public void RegisterDynamicObject(IDynamicGridObject dynamicObject)
        {
            if (!trackedDynamicObjects.Contains(dynamicObject))
            {
                trackedDynamicObjects.Add(dynamicObject);
                dynamicObjectNodes[dynamicObject.GameObject] = new HashSet<GridNode>();
                
                // Initial occupation check
                UpdateDynamicObjectOccupation(dynamicObject);
                
                if (debugMode)
                    Debug.Log($"GridOccupationManager: Registered dynamic object: {dynamicObject.GameObject.name}");
            }
        }
        
        /// <summary>
        /// Unregister a dynamic object from tracking
        /// </summary>
        public void UnregisterDynamicObject(IDynamicGridObject dynamicObject)
        {
            if (trackedDynamicObjects.Contains(dynamicObject))
            {
                // Clear its occupation
                ClearDynamicObjectOccupation(dynamicObject.GameObject);
                
                trackedDynamicObjects.Remove(dynamicObject);
                dynamicObjectNodes.Remove(dynamicObject.GameObject);
                
                if (debugMode)
                    Debug.Log($"GridOccupationManager: Unregistered dynamic object: {dynamicObject.GameObject.name}");
            }
        }
        
        /// <summary>
        /// Main dynamic update loop - runs continuously
        /// </summary>
        private IEnumerator DynamicUpdateLoop()
        {
            while (true)
            {
                // Update a batch of dynamic objects each frame
                int updatesThisFrame = 0;
                
                while (updateQueue.Count > 0 && updatesThisFrame < maxDynamicUpdatesPerFrame)
                {
                    IDynamicGridObject dynObj = updateQueue.Dequeue();
                    if (dynObj != null && dynObj.GameObject != null)
                    {
                        UpdateDynamicObjectOccupation(dynObj);
                        updatesThisFrame++;
                    }
                }
                
                // Re-queue all dynamic objects for next update cycle
                if (updateQueue.Count == 0)
                {
                    foreach (var dynObj in trackedDynamicObjects)
                    {
                        if (dynObj != null && dynObj.GameObject != null && dynObj.HasMoved())
                        {
                            updateQueue.Enqueue(dynObj);
                        }
                    }
                }
                
                yield return new WaitForSeconds(dynamicUpdateInterval);
            }
        }
        
        /// <summary>
        /// Update occupation for a specific dynamic object
        /// </summary>
        private void UpdateDynamicObjectOccupation(IDynamicGridObject dynamicObject)
        {
            GameObject obj = dynamicObject.GameObject;
            if (!obj) return;
            
            // Clear previous occupation
            ClearDynamicObjectOccupation(obj);
            
            // Get object's current bounds
            Collider collider = obj.GetComponent<Collider>();
            if (!collider) return;
            
            Bounds bounds = collider.bounds;
            GridSystem3D grid = gridManager.GridSystem;
            
            // Convert to grid coordinates
            Vector3Int minCoord = WorldToGridCoordinate(bounds.min);
            Vector3Int maxCoord = WorldToGridCoordinate(bounds.max);
            
            // Clamp to grid bounds
            minCoord.x = Mathf.Max(0, minCoord.x);
            minCoord.y = Mathf.Max(0, minCoord.y);
            minCoord.z = Mathf.Max(0, minCoord.z);
            maxCoord.x = Mathf.Min(grid.Width - 1, maxCoord.x);
            maxCoord.y = Mathf.Min(grid.Height - 1, maxCoord.y);
            maxCoord.z = Mathf.Min(grid.Depth - 1, maxCoord.z);
            
            // Mark new occupation
            for (int x = minCoord.x; x <= maxCoord.x; x++)
            {
                for (int y = minCoord.y; y <= maxCoord.y; y++)
                {
                    for (int z = minCoord.z; z <= maxCoord.z; z++)
                    {
                        GridNode node = grid.GetNode(x, y, z);
                        if (node != null && IsColliderOverlappingNode(collider, node))
                        {
                            SetNodeDynamicOccupation(node, obj, true);
                        }
                    }
                }
            }
            
            // Update the object's last position
            dynamicObject.UpdateLastPosition();
        }
        
        /// <summary>
        /// Clear dynamic occupation for a specific object
        /// </summary>
        private void ClearDynamicObjectOccupation(GameObject obj)
        {
            if (dynamicObjectNodes.ContainsKey(obj))
            {
                foreach (GridNode node in dynamicObjectNodes[obj])
                {
                    SetNodeDynamicOccupation(node, obj, false);
                }
                dynamicObjectNodes[obj].Clear();
            }
        }
        
        /// <summary>
        /// Set static occupation for a node
        /// </summary>
        private void SetNodeStaticOccupation(GridNode node, bool occupied)
        {
            bool wasOccupied = IsNodeOccupied(node);
            
            if (occupied)
                staticOccupiedNodes.Add(node);
            else
                staticOccupiedNodes.Remove(node);
            
            bool nowOccupied = IsNodeOccupied(node);
            
            if (wasOccupied != nowOccupied)
            {
                node.IsWalkable = !nowOccupied;
                OnNodeOccupationChanged?.Invoke(node, nowOccupied);
            }
        }
        
        /// <summary>
        /// Set dynamic occupation for a node
        /// </summary>
        private void SetNodeDynamicOccupation(GridNode node, GameObject obj, bool occupied)
        {
            bool wasOccupied = IsNodeOccupied(node);
            
            if (occupied)
            {
                dynamicOccupiedNodes.Add(node);
                dynamicObjectNodes[obj].Add(node);
            }
            else
            {
                // Only remove from dynamic set if no other dynamic objects occupy this node
                bool hasOtherDynamicOccupants = false;
                foreach (var kvp in dynamicObjectNodes)
                {
                    if (kvp.Key != obj && kvp.Value.Contains(node))
                    {
                        hasOtherDynamicOccupants = true;
                        break;
                    }
                }
                
                if (!hasOtherDynamicOccupants)
                    dynamicOccupiedNodes.Remove(node);
                
                dynamicObjectNodes[obj].Remove(node);
            }
            
            bool nowOccupied = IsNodeOccupied(node);
            
            if (wasOccupied != nowOccupied)
            {
                node.IsWalkable = !nowOccupied;
                OnNodeOccupationChanged?.Invoke(node, nowOccupied);
            }
        }
        
        /// <summary>
        /// Check if a node is occupied by either static or dynamic objects
        /// </summary>
        public bool IsNodeOccupied(GridNode node)
        {
            return staticOccupiedNodes.Contains(node) || dynamicOccupiedNodes.Contains(node);
        }
        
        /// <summary>
        /// Check if a node is statically occupied
        /// </summary>
        public bool IsNodeStaticallyOccupied(GridNode node)
        {
            return staticOccupiedNodes.Contains(node);
        }
        
        /// <summary>
        /// Check if a node is dynamically occupied
        /// </summary>
        public bool IsNodeDynamicallyOccupied(GridNode node)
        {
            return dynamicOccupiedNodes.Contains(node);
        }
        
        /// <summary>
        /// Precise collision detection between collider and grid node
        /// </summary>
        private bool IsColliderOverlappingNode(Collider collider, GridNode node)
        {
            Vector3 nodeCenter = node.GetCenterWorldPosition(gridManager.GridSystem.NodeSize);
            Vector3 nodeSize = Vector3.one * gridManager.GridSystem.NodeSize;
            
            // Use Physics.CheckBox for precise detection
            return Physics.CheckBox(
                nodeCenter,
                nodeSize * 0.5f,
                Quaternion.identity,
                1 << collider.gameObject.layer
            );
        }
        
        /// <summary>
        /// Spatial hashing function
        /// </summary>
        private int SpatialHash(int x, int y, int z)
        {
            return (x * 73856093) ^ (y * 19349663) ^ (z * 83492791);
        }
        
        /// <summary>
        /// Check if a layer is in the layer mask
        /// </summary>
        private bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
        
    /// <summary>
    /// Convert world position to grid coordinate
    /// </summary>
    private Vector3Int WorldToGridCoordinate(Vector3 worldPosition)
    {
        if (!gridManager || !gridManager.IsInitialized)
            return Vector3Int.zero;
        
        GridSystem3D grid = gridManager.GridSystem;
        Vector3 gridOrigin = gridManager.transform.position; // Assuming grid starts at GridManager position
        Vector3 localPos = worldPosition - gridOrigin;
        
        // Fix for X-coordinate offset issue: Add half node size to properly center the calculation
        // This adjusts the conversion to ensure objects are mapped to the correct grid cell
        float halfNodeSize = grid.NodeSize * 0.5f;
        
        return new Vector3Int(
            Mathf.FloorToInt((localPos.x + halfNodeSize) / grid.NodeSize),
            Mathf.FloorToInt((localPos.y + halfNodeSize) / grid.NodeSize),
            Mathf.FloorToInt((localPos.z + halfNodeSize) / grid.NodeSize)
        );
    }
        
        /// <summary>
        /// Force refresh of all occupation data
        /// </summary>
        [ContextMenu("Refresh All Occupation Data")]
        public void RefreshAllOccupationData()
        {
            if (gridManager && gridManager.IsInitialized)
            {
                StartCoroutine(InitializeOccupationSystem());
            }
        }
        
        private void OnDestroy()
        {
            if (dynamicUpdateCoroutine != null)
            {
                StopCoroutine(dynamicUpdateCoroutine);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!debugMode || !gridManager || !gridManager.IsInitialized) return;
            
            // Draw static occupation
            if (showStaticOccupation)
            {
                Gizmos.color = Color.red;
                foreach (GridNode node in staticOccupiedNodes)
                {
                    Vector3 pos = node.GetCenterWorldPosition(gridManager.GridSystem.NodeSize);
                    Gizmos.DrawWireCube(pos, Vector3.one * gridManager.GridSystem.NodeSize);
                }
            }
            
            // Draw dynamic occupation
            if (showDynamicOccupation)
            {
                Gizmos.color = Color.yellow;
                foreach (GridNode node in dynamicOccupiedNodes)
                {
                    Vector3 pos = node.GetCenterWorldPosition(gridManager.GridSystem.NodeSize);
                    Gizmos.DrawWireCube(pos, Vector3.one * gridManager.GridSystem.NodeSize * 0.9f);
                }
            }
        }
    }
    
    /// <summary>
    /// Interface for objects that need dynamic grid occupation tracking
    /// </summary>
    public interface IDynamicGridObject
    {
        GameObject GameObject { get; }
        bool HasMoved();
        void UpdateLastPosition();
    }
}

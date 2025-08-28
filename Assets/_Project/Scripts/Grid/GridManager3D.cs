using UnityEngine;
using System.Collections.Generic;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// Unified manager for 3D grid pathfinding system
    /// Integrates GridSystem3D, pathfinding, visualization, and request management
    /// </summary>
    public class GridManager3D : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private GridConfiguration gridConfig;
        [SerializeField] private bool autoInitializeOnStart = true;
        [SerializeField] private bool autoScanForObstacles = true;
        
        [Header("Input Settings")]
        [SerializeField] private UnityEngine.Camera playerCamera;
        [SerializeField] private LayerMask groundLayerMask = -1;
        [SerializeField] private KeyCode selectKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode multiSelectKey = KeyCode.LeftShift;
        
        [Header("Selection Settings")]
        [SerializeField] private bool allowAreaSelection = true;
        [SerializeField] private bool allowMultiSelection = true;
        [SerializeField] private bool enablePathfindingOnSelection = false;

        // Core Systems
        private GridSystem3D gridSystem;
        private PathfindingRequestManager requestManager;
        
        // Selection state
        private bool isDragging = false;
        private Vector3Int dragStartCell;
        private Vector3Int dragCurrentCell;
        private List<GridNode> selectedNodes = new List<GridNode>();
        
        // Pathfinding integration
        private List<GridNode> currentPath = new List<GridNode>();
        private string currentPathfindingRequest = null;

        // Properties
        public GridSystem3D GridSystem => gridSystem;
        public PathfindingRequestManager RequestManager => requestManager;
        public List<GridNode> SelectedNodes => new List<GridNode>(selectedNodes);
        public List<GridNode> CurrentPath => new List<GridNode>(currentPath);
        public bool IsInitialized => gridSystem?.IsInitialized ?? false;

        // Events
        public System.Action<List<GridNode>> OnSelectionChanged;
        public System.Action<GridNode> OnNodeHovered;
        public System.Action<List<GridNode>> OnPathFound;
        public System.Action OnGridInitialized;

        private void Awake()
        {
            // Create default configuration if none assigned
            if (gridConfig == null)
            {
                Debug.LogWarning("No GridConfiguration assigned to GridManager3D. Creating default configuration.");
                gridConfig = ScriptableObject.CreateInstance<GridConfiguration>();
            }
            
            // Get camera if not assigned
            if (playerCamera == null)
                playerCamera = UnityEngine.Camera.main;
        }

        private void Start()
        {
            if (autoInitializeOnStart)
            {
                InitializeGrid();
            }
        }

        private void Update()
        {
            if (gridSystem?.IsInitialized == true)
            {
                HandleInput();
                UpdateNodeHighlighting();
            }
        }

        /// <summary>
        /// Initialize the complete 3D grid system
        /// </summary>
        [ContextMenu("Initialize Grid")]
        public void InitializeGrid()
        {
            try
            {
                Debug.Log("Initializing 3D Grid System...");
                
                // Create and initialize grid system
                gridSystem = new GridSystem3D(gridConfig);
                
                // Setup pathfinding request manager
                SetupRequestManager();
                
                // Scan for obstacles if enabled
                if (autoScanForObstacles)
                {
                    gridSystem.ScanForObstacles();
                }
                
                Debug.Log($"3D Grid System initialized successfully! " +
                         $"Grid: {gridSystem.Width}x{gridSystem.Height}x{gridSystem.Depth}, " +
                         $"Memory: {gridSystem.GetEstimatedMemoryUsage():F1} MB");
                
                OnGridInitialized?.Invoke();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize 3D Grid System: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Setup pathfinding request manager
        /// </summary>
        private void SetupRequestManager()
        {
            // Create request manager component
            GameObject requestManagerGO = new GameObject("PathfindingRequestManager");
            requestManagerGO.transform.SetParent(transform);
            requestManager = requestManagerGO.AddComponent<PathfindingRequestManager>();
            
            // Initialize with our grid system
            requestManager.Initialize(gridSystem, gridConfig);
            
            // Subscribe to events
            requestManager.OnRequestCompleted += OnPathfindingRequestCompleted;
            requestManager.OnRequestFailed += OnPathfindingRequestFailed;
        }


        /// <summary>
        /// Handle input for grid interaction
        /// </summary>
        private void HandleInput()
        {
            if (playerCamera == null) return;

            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3Int hoveredCell = gridSystem.WorldToGrid(mouseWorldPos);

            // Handle cell hovering and selection
            if (gridSystem.IsValidGridPosition(hoveredCell))
            {
                GridNode node = gridSystem.GetNode(hoveredCell);
                OnNodeHovered?.Invoke(node);
                
                // Select node on hover (optional)
                if (!isDragging)
                {
                    SelectNodeOnHover(node);
                }
            }

            // Handle area selection with mouse drag
            if (Input.GetKeyDown(selectKey))
            {
                StartAreaSelection(hoveredCell);
            }
            else if (Input.GetKey(selectKey) && isDragging)
            {
                UpdateAreaSelection(hoveredCell);
            }
            else if (Input.GetKeyUp(selectKey) && isDragging)
            {
                EndAreaSelection();
            }

            // Handle pathfinding on right-click
            if (Input.GetMouseButtonDown(1) && selectedNodes.Count > 0)
            {
                RequestPathfindingToPosition(mouseWorldPos);
            }
        }

        /// <summary>
        /// Get mouse position in world space
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
            {
                return hit.point;
            }
            
            // Fallback: project onto XZ plane at Y=0
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Select a node when hovering over it
        /// </summary>
        private void SelectNodeOnHover(GridNode node)
        {
            if (isDragging) return;
            
            // Clear previous selection if not multi-selecting
            if (!Input.GetKey(multiSelectKey) || !allowMultiSelection)
            {
                ClearSelection();
            }
            
            // Select the hovered node
            if (!node.IsSelected)
            {
                node.IsSelected = true;
                if (!selectedNodes.Contains(node))
                {
                    selectedNodes.Add(node);
                }
                OnSelectionChanged?.Invoke(SelectedNodes);
            }
        }

        /// <summary>
        /// Start area selection mode
        /// </summary>
        private void StartAreaSelection(Vector3Int cellPos)
        {
            if (!gridSystem.IsValidGridPosition(cellPos)) return;

            dragStartCell = cellPos;
            dragCurrentCell = cellPos;
            isDragging = true;

            // Clear previous selection if not multi-selecting
            if (!Input.GetKey(multiSelectKey) || !allowMultiSelection)
            {
                ClearSelection();
            }

            UpdateAreaSelection(cellPos);
        }

        /// <summary>
        /// Update area selection while dragging
        /// </summary>
        private void UpdateAreaSelection(Vector3Int cellPos)
        {
            if (!isDragging || !gridSystem.IsValidGridPosition(cellPos)) return;

            dragCurrentCell = cellPos;

            // Clear current drag selection
            ClearDragSelection();

            if (allowAreaSelection && dragStartCell != dragCurrentCell)
            {
                // Area selection (3D)
                GridNode[] nodesInArea = gridSystem.GetNodesInArea(dragStartCell, dragCurrentCell);
                foreach (GridNode node in nodesInArea)
                {
                    if (node.IsWalkable) // Only select walkable nodes
                    {
                        node.IsSelected = true;
                        if (!selectedNodes.Contains(node))
                        {
                            selectedNodes.Add(node);
                        }
                    }
                }
            }
            else
            {
                // Single node selection
                GridNode node = gridSystem.GetNode(cellPos);
                if (node != null && node.IsWalkable)
                {
                    node.IsSelected = true;
                    if (!selectedNodes.Contains(node))
                    {
                        selectedNodes.Add(node);
                    }
                }
            }
        }

        /// <summary>
        /// End area selection mode
        /// </summary>
        private void EndAreaSelection()
        {
            isDragging = false;
            OnSelectionChanged?.Invoke(SelectedNodes);
            
            Debug.Log($"Area selected {selectedNodes.Count} nodes");
        }

        /// <summary>
        /// Clear drag selection (preserves previously selected nodes when multi-selecting)
        /// </summary>
        private void ClearDragSelection()
        {
            if (allowAreaSelection && dragStartCell != dragCurrentCell)
            {
                GridNode[] previousDragNodes = gridSystem.GetNodesInArea(dragStartCell, dragCurrentCell);
                foreach (GridNode node in previousDragNodes)
                {
                    node.IsSelected = false;
                    selectedNodes.Remove(node);
                }
            }
        }

        /// <summary>
        /// Update node highlighting based on mouse position
        /// </summary>
        private void UpdateNodeHighlighting()
        {
            if (isDragging) return;

            // Clear all highlighting
            ClearHighlighting();

            // Highlight hovered node
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3Int hoveredCell = gridSystem.WorldToGrid(mouseWorldPos);
            
            if (gridSystem.IsValidGridPosition(hoveredCell))
            {
                GridNode node = gridSystem.GetNode(hoveredCell);
                if (node != null)
                {
                    node.IsHighlighted = true;
                }
            }
        }

        /// <summary>
        /// Clear highlighting from all nodes
        /// </summary>
        private void ClearHighlighting()
        {
            // This would ideally be optimized to only clear previously highlighted nodes
            for (int x = 0; x < gridSystem.Width; x++)
            {
                for (int y = 0; y < gridSystem.Height; y++)
                {
                    for (int z = 0; z < gridSystem.Depth; z++)
                    {
                        GridNode node = gridSystem.GetNode(x, y, z);
                        if (node != null)
                        {
                            node.IsHighlighted = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Request pathfinding to a world position
        /// </summary>
        public void RequestPathfindingToPosition(Vector3 worldPosition)
        {
            if (selectedNodes.Count == 0 || requestManager == null)
            {
                Debug.LogWarning("Cannot request pathfinding: No nodes selected or request manager not initialized");
                return;
            }

            // Use first selected node as start position
            GridNode startNode = selectedNodes[0];
            Vector3 startPos = startNode.WorldPosition;

            // Cancel previous request if still pending
            if (currentPathfindingRequest != null)
            {
                requestManager.CancelRequest(currentPathfindingRequest);
            }

            // Submit new pathfinding request
            currentPathfindingRequest = requestManager.SubmitRequest(
                startPos,
                worldPosition,
                OnPathfindingCompleted,
                PathfindingProfile.CreateDefault(),
                PathfindingPriority.Normal,
                this
            );

            if (gridConfig.LogPathfindingStatistics)
            {
                Debug.Log($"Submitted pathfinding request from {startPos} to {worldPosition}");
            }
        }

        /// <summary>
        /// Callback for pathfinding completion
        /// </summary>
        private void OnPathfindingCompleted(List<GridNode> path)
        {
            currentPath = path ?? new List<GridNode>();
            currentPathfindingRequest = null;
            
            OnPathFound?.Invoke(currentPath);
            
            Debug.Log($"Pathfinding completed: {currentPath.Count} nodes in path");
        }

        /// <summary>
        /// Handle pathfinding request completion
        /// </summary>
        private void OnPathfindingRequestCompleted(PathfindingRequest request)
        {
            if (gridConfig.EnablePathfindingDebug)
            {
                Debug.Log($"Pathfinding request completed: {request.RequestId} - " +
                         $"Path length: {request.ResultPath.Count}, " +
                         $"Time: {request.Stats.TimeElapsed}ms");
            }
        }

        /// <summary>
        /// Handle pathfinding request failure
        /// </summary>
        private void OnPathfindingRequestFailed(PathfindingRequest request)
        {
            Debug.LogWarning($"Pathfinding request failed: {request.RequestId} - {request.ErrorMessage}");
            
            if (request.RequestId == currentPathfindingRequest)
            {
                currentPathfindingRequest = null;
            }
        }

        /// <summary>
        /// Clear all selected nodes
        /// </summary>
        public void ClearSelection()
        {
            foreach (GridNode node in selectedNodes)
            {
                node.IsSelected = false;
            }
            selectedNodes.Clear();
            OnSelectionChanged?.Invoke(SelectedNodes);
        }

        /// <summary>
        /// Select nodes in the specified area
        /// </summary>
        public void SelectArea(Vector3Int start, Vector3Int end)
        {
            GridNode[] nodesInArea = gridSystem.GetNodesInArea(start, end);
            foreach (GridNode node in nodesInArea)
            {
                if (node.IsWalkable)
                {
                    node.IsSelected = true;
                    if (!selectedNodes.Contains(node))
                    {
                        selectedNodes.Add(node);
                    }
                }
            }
            OnSelectionChanged?.Invoke(SelectedNodes);
        }

        /// <summary>
        /// Get node at world position
        /// </summary>
        public GridNode GetNodeAtWorldPosition(Vector3 worldPos)
        {
            return gridSystem?.GetNodeAtWorldPosition(worldPos);
        }

        /// <summary>
        /// Check if a world position is walkable
        /// </summary>
        public bool IsWalkablePosition(Vector3 worldPos)
        {
            GridNode node = GetNodeAtWorldPosition(worldPos);
            return node?.IsWalkable ?? false;
        }

        /// <summary>
        /// Get statistics about the grid system
        /// </summary>
        public GridSystemStatistics GetStatistics()
        {
            if (!IsInitialized)
                return new GridSystemStatistics();

            var pathfindingStats = requestManager?.GetStatistics() ?? new PathfindingManagerStatistics();
            
            return new GridSystemStatistics
            {
                GridSize = new Vector3Int(gridSystem.Width, gridSystem.Height, gridSystem.Depth),
                TotalNodes = gridSystem.TotalNodeCount,
                SelectedNodes = selectedNodes.Count,
                CurrentPathLength = currentPath.Count,
                MemoryUsageMB = gridSystem.GetEstimatedMemoryUsage(),
                PendingPathfindingRequests = pathfindingStats.PendingRequests,
                ProcessingPathfindingRequests = pathfindingStats.ProcessingRequests
            };
        }

        /// <summary>
        /// Rescan for obstacles
        /// </summary>
        [ContextMenu("Rescan Obstacles")]
        public void RescanObstacles()
        {
            if (IsInitialized)
            {
                gridSystem.ScanForObstacles();
                Debug.Log("Obstacle scan completed");
            }
        }

        /// <summary>
        /// Clear current path visualization
        /// </summary>
        [ContextMenu("Clear Path")]
        public void ClearPath()
        {
            currentPath.Clear();
            OnPathFound?.Invoke(currentPath);
        }

        /// <summary>
        /// Editor convenience methods
        /// </summary>
        [ContextMenu("Clear Selection")]
        private void EditorClearSelection()
        {
            if (Application.isPlaying)
                ClearSelection();
        }

        private void OnDrawGizmosSelected()
        {
            if (!IsInitialized) return;

            // Draw grid bounds
            Bounds bounds = gridSystem.GetWorldBounds();
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // Draw selected nodes
            Gizmos.color = Color.green;
            foreach (GridNode node in selectedNodes)
            {
                Vector3 center = node.GetCenterWorldPosition(gridSystem.NodeSize);
                Gizmos.DrawWireCube(center, Vector3.one * gridSystem.NodeSize * 0.8f);
            }

            // Draw current path
            if (currentPath.Count > 1)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < currentPath.Count - 1; i++)
                {
                    Vector3 from = currentPath[i].GetCenterWorldPosition(gridSystem.NodeSize);
                    Vector3 to = currentPath[i + 1].GetCenterWorldPosition(gridSystem.NodeSize);
                    Gizmos.DrawLine(from, to);
                }
            }
        }

        private void OnDestroy()
        {
            gridSystem?.Dispose();
        }
    }

    /// <summary>
    /// Statistics for the grid system
    /// </summary>
    public struct GridSystemStatistics
    {
        public Vector3Int GridSize;
        public int TotalNodes;
        public int SelectedNodes;
        public int CurrentPathLength;
        public float MemoryUsageMB;
        public int PendingPathfindingRequests;
        public int ProcessingPathfindingRequests;
    }
}

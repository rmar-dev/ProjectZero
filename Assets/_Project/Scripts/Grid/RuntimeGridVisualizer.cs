using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// Enhanced runtime visualization for the 3D grid system
    /// Shows individual grid squares with different colors for different states
    /// </summary>
    public class RuntimeGridVisualizer : MonoBehaviour
    {
        [Header("Grid Square Visualization")]
        [SerializeField] private bool showGridSquares = true;
        [SerializeField] private bool showOnlyGroundLevel = true;
        [SerializeField] private int maxVisibleHeight = 3;
        [SerializeField] private float squareHeightOffset = 0.02f;
        
        [Header("Visual States")]
        [SerializeField] private bool showGridBounds = true;
        [SerializeField] private bool showCurrentPath = true;
        
        [Header("Grid Cell Settings")]
        [SerializeField] private float cellSizeMultiplier = 0.95f; // Size relative to grid node size
        [SerializeField] private float cellHeight = 0.1f; // Height of cube cells
        
        [Header("Materials")]
        [SerializeField] private Material freeNodeMaterial;
        [SerializeField] private Material occupiedNodeMaterial;
        [SerializeField] private Material hoveredNodeMaterial;
        [SerializeField] private Material selectedNodeMaterial;
        [SerializeField] private Material pathNodeMaterial;
        [SerializeField] private Color gridBoundsColor = Color.cyan;
        
        [Header("Performance Settings")]
        [SerializeField] private bool useInstancing = true;
        [SerializeField] private int maxSquaresPerFrame = 100;
        [SerializeField] private float updateInterval = 0.1f;
        
        [Header("Debugging")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool useOpaqueColors = false;
        [SerializeField] private bool useCubesInsteadOfQuads = false;
        [Range(0.1f, 2f)]
        [SerializeField] private float debugSquareSize = 1f;
        
        private GridManager3D gridManager;
        private GridOccupationManager occupationManager;
        private List<GameObject> visualizationObjects = new List<GameObject>();
        private Dictionary<GridNode, GameObject> nodeSquares = new Dictionary<GridNode, GameObject>();
        private GameObject visualizationParent;
        private GameObject squaresParent;
        
        // State tracking
        private GridNode hoveredNode;
        private List<GridNode> currentPathNodes = new List<GridNode>();
        private List<GridNode> selectedNodes = new List<GridNode>();
        
        // Update timing
        private float lastUpdateTime;
        
        private void Start()
        {
            // Find the grid manager
            gridManager = FindObjectOfType<GridManager3D>();
            if (gridManager == null)
            {
                Debug.LogWarning("RuntimeGridVisualizer: No GridManager3D found in scene");
                return;
            }
            
            // Find the occupation manager
            occupationManager = FindObjectOfType<GridOccupationManager>();
            if (occupationManager == null)
            {
                Debug.LogWarning("RuntimeGridVisualizer: No GridOccupationManager found in scene. Occupation detection will be limited.");
            }
            else
            {
                // Subscribe to occupation changes
                occupationManager.OnNodeOccupationChanged += OnNodeOccupationChanged;
            }

            // Create parent for organization
            visualizationParent = new GameObject("Grid Visualization");
            visualizationParent.transform.SetParent(transform);

            // Subscribe to events
            gridManager.OnGridInitialized += OnGridInitialized;
            gridManager.OnSelectionChanged += OnSelectionChanged;
            gridManager.OnPathFound += OnPathFound;

            // Initial setup if grid is already initialized
            if (gridManager.IsInitialized)
            {
                OnGridInitialized();
            }
        }

        private void OnGridInitialized()
        {
            Debug.Log("RuntimeGridVisualizer: Grid initialized, setting up visualization");
            CreateGridBoundsVisualization();
            
            if (showGridSquares)
            {
                CreateGridSquareVisualization();
            }
        }
        
        private void Update()
        {
            if (!gridManager || !gridManager.IsInitialized) return;
            
            // Update visualizations more frequently for testing
            if (Time.time - lastUpdateTime > updateInterval)
            {
                UpdateHoveredNode();
                UpdateGridSquareColors();
                lastUpdateTime = Time.time;
                
                // Debug: show some basic info every few seconds
                if (debugMode && Time.time % 3f < 0.1f)
                {
                    Debug.Log($"Grid squares count: {nodeSquares.Count}, Hovered node: {(hoveredNode != null ? $"({hoveredNode.X},{hoveredNode.Y},{hoveredNode.Z})" : "none")}");
                }
            }
            
            // Test: Press T to manually change colors of first few squares
            if (debugMode && Input.GetKeyDown(KeyCode.T))
            {
                TestColorChange();
            }
        }

        private void OnSelectionChanged(List<GridNode> selectedNodes)
        {
            CreateSelectedNodesVisualization(selectedNodes);
        }

        private void OnPathFound(List<GridNode> path)
        {
            CreatePathVisualization(path);
        }
        
        /// <summary>
        /// Called when a node's occupation state changes
        /// </summary>
        private void OnNodeOccupationChanged(GridNode node, bool isOccupied)
        {
            if (debugMode)
            {
                Debug.Log($"RuntimeGridVisualizer: Node ({node.X},{node.Y},{node.Z}) occupation changed to {isOccupied}");
            }
            
            // Update the visual immediately for this node
            if (nodeSquares.ContainsKey(node))
            {
                GameObject square = nodeSquares[node];
                if (square != null)
                {
                    Material targetMaterial = GetNodeMaterial(node);
                    Renderer renderer = square.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = targetMaterial;
                    }
                }
            }
        }

        /// <summary>
        /// Create visualization for grid bounds
        /// </summary>
        private void CreateGridBoundsVisualization()
        {
            if (!showGridBounds || !gridManager.IsInitialized) return;

            // Remove existing bounds visualization
            DestroyVisualizationObjects("GridBounds");

            // Create bounds wireframe
            Bounds bounds = gridManager.GridSystem.GetWorldBounds();
            GameObject boundsObj = CreateWireframeCube("GridBounds", bounds.center, bounds.size, gridBoundsColor);
            visualizationObjects.Add(boundsObj);

            Debug.Log($"RuntimeGridVisualizer: Created grid bounds visualization - {bounds.size}");
        }

        /// <summary>
        /// Create visualization for selected nodes
        /// </summary>
        private void CreateSelectedNodesVisualization(List<GridNode> selectedNodes)
        {
            // Store selected nodes for color updates
            this.selectedNodes = selectedNodes ?? new List<GridNode>();
            // Grid squares will handle the visual representation
        }

        /// <summary>
        /// Create visualization for pathfinding result
        /// </summary>
        private void CreatePathVisualization(List<GridNode> path)
        {
            if (!showCurrentPath) return;

            // Remove existing path visualization
            DestroyVisualizationObjects("PathLine");

            if (path.Count < 2) return;

            // Create line renderer for path
            GameObject pathObj = new GameObject("PathLine");
            pathObj.transform.SetParent(visualizationParent.transform);

            LineRenderer lr = pathObj.AddComponent<LineRenderer>();
            lr.material = CreateLineMaterial(Color.yellow); // Use yellow for path line
            lr.startWidth = 0.2f;
            lr.endWidth = 0.2f;
            lr.positionCount = path.Count;
            lr.useWorldSpace = true;

            // Set path points
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 worldPos = path[i].GetCenterWorldPosition(gridManager.GridSystem.NodeSize);
                worldPos.y += squareHeightOffset * 10f; // Slightly above node level
                lr.SetPosition(i, worldPos);
            }
            
            // Store path nodes for color updates
            currentPathNodes = path;

            visualizationObjects.Add(pathObj);
            Debug.Log($"RuntimeGridVisualizer: Visualizing path with {path.Count} nodes");
        }

        /// <summary>
        /// Create visualization for all walkable nodes (performance intensive!)
        /// </summary>
        private void CreateWalkableNodesVisualization()
        {
            // This method is deprecated - using grid squares instead
            Debug.Log("RuntimeGridVisualizer: Walkable nodes visualization is handled by grid squares");
        }

        /// <summary>
        /// Create a wireframe cube GameObject
        /// </summary>
        private GameObject CreateWireframeCube(string wireframeName, Vector3 center, Vector3 size, Color color)
        {
            var obj = new GameObject(wireframeName);
            obj.transform.SetParent(visualizationParent.transform);
            obj.transform.position = center;

            // Create wireframe using LineRenderer components
            var corners = GetCubeCorners(Vector3.zero, size);
            int[,] edges = {
                {0,1}, {1,2}, {2,3}, {3,0}, // Bottom face
                {4,5}, {5,6}, {6,7}, {7,4}, // Top face
                {0,4}, {1,5}, {2,6}, {3,7}  // Vertical edges
            };

            for (var i = 0; i < edges.GetLength(0); i++)
            {
                var edge = new GameObject($"Edge_{i}");
                edge.transform.SetParent(obj.transform);

                var lr = edge.AddComponent<LineRenderer>();
                lr.material = CreateLineMaterial(color);
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lr.positionCount = 2;
                lr.useWorldSpace = false;

                lr.SetPosition(0, corners[edges[i, 0]]);
                lr.SetPosition(1, corners[edges[i, 1]]);
            }

            return obj;
        }

        /// <summary>
        /// Create a simple quad for ground-level visualization
        /// </summary>
        private GameObject CreateQuad(string name, Vector3 center, float size, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            obj.name = name;
            obj.transform.SetParent(visualizationParent.transform);
            obj.transform.position = center;
            
            // Rotate quad to face up (flat on ground) - this is key for visibility!
            obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            obj.transform.localScale = Vector3.one * size;

            // Remove collider and set material
            DestroyImmediate(obj.GetComponent<Collider>());
            
            Renderer renderer = obj.GetComponent<Renderer>();
            renderer.material = material;
            
            // Ensure it renders in game view
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            return obj;
        }
        
        /// <summary>
        /// Create a cube as alternative to quads for better visibility
        /// </summary>
        private GameObject CreateCube(string name, Vector3 center, float size, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(visualizationParent.transform);
            obj.transform.position = center;
            
            // Use configurable height for cubes
            obj.transform.localScale = new Vector3(size, cellHeight, size);

            // Remove collider and set material
            DestroyImmediate(obj.GetComponent<Collider>());
            
            Renderer renderer = obj.GetComponent<Renderer>();
            renderer.material = material;
            
            // Ensure it renders in game view
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            return obj;
        }

        /// <summary>
        /// Get the 8 corners of a cube
        /// </summary>
        private Vector3[] GetCubeCorners(Vector3 center, Vector3 size)
        {
            Vector3 half = size * 0.5f;
            return new Vector3[]
            {
                center + new Vector3(-half.x, -half.y, -half.z), // 0
                center + new Vector3(+half.x, -half.y, -half.z), // 1
                center + new Vector3(+half.x, -half.y, +half.z), // 2
                center + new Vector3(-half.x, -half.y, +half.z), // 3
                center + new Vector3(-half.x, +half.y, -half.z), // 4
                center + new Vector3(+half.x, +half.y, -half.z), // 5
                center + new Vector3(+half.x, +half.y, +half.z), // 6
                center + new Vector3(-half.x, +half.y, +half.z)  // 7
            };
        }

        /// <summary>
        /// Create a simple line material
        /// </summary>
        private Material CreateLineMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            return mat;
        }

        /// <summary>
        /// Create a transparent material with improved shader fallback system
        /// </summary>
        private Material CreateTransparentMaterial(Color color)
        {
            // Updated shader priority order with better fallbacks
            Shader shader = null;
            
            // Try to find available shaders in priority order
            string[] shaderNames = {
                "Universal Render Pipeline/Lit",
                "HDRP/Lit", 
                "Standard",
                "Mobile/Diffuse",
                "Unlit/Color",
                "Legacy Shaders/Diffuse",
                "Legacy Shaders/Transparent/Diffuse",
                "Sprites/Default",
                "Hidden/Internal-Colored"
            };
            
            foreach (string shaderName in shaderNames)
            {
                shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    Debug.Log($"RuntimeGridVisualizer: Using shader '{shaderName}' for materials");
                    break;
                }
            }
            
            if (shader == null)
            {
                Debug.LogError("RuntimeGridVisualizer: No suitable shader found! This will cause pink materials.");
                return new Material(Shader.Find("Hidden/Internal-Colored")) { color = color };
            }
            
            Material mat = new Material(shader);
            mat.color = color;
            
            // Apply transparency settings based on shader type
            if (shader.name.Contains("Standard"))
            {
                // Built-in Standard shader transparency
                mat.SetFloat("_Mode", 2); // Fade mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000; // Transparent queue
            }
            else if (shader.name.Contains("Universal Render Pipeline"))
            {
                // URP transparency settings
                try
                {
                    mat.SetFloat("_Surface", 1); // Transparent
                    mat.SetFloat("_Blend", 0); // Alpha
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.EnableKeyword("_BLENDMODE_ALPHA");
                    mat.renderQueue = 3000;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"RuntimeGridVisualizer: Could not set URP transparency properties: {e.Message}");
                }
            }
            else if (shader.name.Contains("Unlit") || shader.name.Contains("Mobile"))
            {
                // Simple unlit/mobile shaders - just set color
                mat.renderQueue = 2450; // Slightly transparent
            }
            else
            {
                // Fallback for other shaders - basic setup
                mat.renderQueue = 2450;
            }
            
            Debug.Log($"RuntimeGridVisualizer: Created material with shader '{shader.name}', color {color}, renderQueue {mat.renderQueue}");
            return mat;
        }

        /// <summary>
        /// Create grid square visualization for all nodes
        /// </summary>
        private void CreateGridSquareVisualization()
        {
            if (!showGridSquares || !gridManager.IsInitialized) return;
            
            Debug.Log("RuntimeGridVisualizer: Creating grid square visualization...");
            
            // Create squares parent
            squaresParent = new GameObject("Grid Squares");
            squaresParent.transform.SetParent(visualizationParent.transform);
            
            GridSystem3D gridSystem = gridManager.GridSystem;
            int createdSquares = 0;
            
            // Determine height range to visualize
            int minHeight = 0;
            int maxHeight = showOnlyGroundLevel ? 1 : Mathf.Min(gridSystem.Height, maxVisibleHeight);
            
            for (int y = minHeight; y < maxHeight; y++)
            {
                for (int x = 0; x < gridSystem.Width; x++)
                {
                    for (int z = 0; z < gridSystem.Depth; z++)
                    {
                        GridNode node = gridSystem.GetNode(x, y, z);
                        if (node != null)
                        {
                            CreateNodeSquare(node, createdSquares);
                            createdSquares++;
                        }
                    }
                }
            }
            
            Debug.Log($"RuntimeGridVisualizer: Created {createdSquares} grid squares");
        }
        
        /// <summary>
        /// Create a single node square visualization
        /// </summary>
        private void CreateNodeSquare(GridNode node, int index)
        {
            Vector3 worldPos = node.GetCenterWorldPosition(gridManager.GridSystem.NodeSize);
            worldPos.y += squareHeightOffset + (node.Y * 0.01f); // Slight height offset per level
            
            // Use configurable cell size multiplier
            float squareSize = gridManager.GridSystem.NodeSize * cellSizeMultiplier;
            if (debugMode) squareSize *= debugSquareSize;
            
            GameObject square;
            Material nodeMaterial = GetNodeMaterial(node);
            
            if (useCubesInsteadOfQuads)
            {
                square = CreateCube($"GridSquare_{index}", worldPos, squareSize, nodeMaterial);
            }
            else
            {
                square = CreateQuad($"GridSquare_{index}", worldPos, squareSize, nodeMaterial);
            }
            
            square.transform.SetParent(squaresParent.transform);
            
            // Store reference for updates
            nodeSquares[node] = square;
            visualizationObjects.Add(square);
            
            // Debug first few squares
            if (debugMode && index < 5)
            {
                Debug.Log($"Created grid square {index} at position {worldPos} with size {squareSize} and material {nodeMaterial?.name}");
            }
        }
        
        /// <summary>
        /// Update hovered node based on mouse position
        /// </summary>
        private void UpdateHoveredNode()
        {
            if (!Input.mousePresent) return;
            
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) return;
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            // Cast ray to find hovered grid node
            GridNode newHoveredNode = GetNodeFromWorldPosition(ray);
            
            if (newHoveredNode != hoveredNode)
            {
                if (debugMode)
                {
                    if (hoveredNode != null)
                        Debug.Log($"Unhovered node at ({hoveredNode.X}, {hoveredNode.Y}, {hoveredNode.Z})");
                    if (newHoveredNode != null)
                        Debug.Log($"Hovered node at ({newHoveredNode.X}, {newHoveredNode.Y}, {newHoveredNode.Z})");
                }
                
                hoveredNode = newHoveredNode;
                // Color update will happen in UpdateGridSquareColors
            }
        }
        
        /// <summary>
        /// Get grid node from world position raycast
        /// </summary>
        private GridNode GetNodeFromWorldPosition(Ray ray)
        {
            if (!gridManager.IsInitialized) return null;
            
            // Simple ground plane intersection for now
            Plane groundPlane = new Plane(Vector3.up, 0f);
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                
                // Use the existing method from GridSystem3D
                return gridManager.GridSystem.GetNodeAtWorldPosition(hitPoint);
            }
            
            return null;
        }
        
        /// <summary>
        /// Update colors of all grid squares based on current state
        /// </summary>
        private void UpdateGridSquareColors()
        {
            if (!showGridSquares || gridManager == null) return;
            
            // Update selected nodes list
            selectedNodes = gridManager.SelectedNodes ?? new List<GridNode>();
            currentPathNodes = gridManager.CurrentPath ?? new List<GridNode>();
            
            foreach (var kvp in nodeSquares)
            {
                GridNode node = kvp.Key;
                GameObject square = kvp.Value;
                
                if (square != null)
                {
                    Material targetMaterial = GetNodeMaterial(node);
                    Renderer renderer = square.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != targetMaterial)
                    {
                        // Update to use the correct material asset
                        renderer.material = targetMaterial;
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the appropriate material for a grid node based on its state
        /// </summary>
        private Material GetNodeMaterial(GridNode node)
        {
            // Priority order: Path > Selected > Hovered > Occupied > Free
            if (currentPathNodes.Contains(node))
                return pathNodeMaterial;
            else if (selectedNodes.Contains(node))
                return selectedNodeMaterial;
            else if (node == hoveredNode)
                return hoveredNodeMaterial;
            else if (!node.IsWalkable)
                return occupiedNodeMaterial;
            else
                return freeNodeMaterial;
        }
        
        
        /// <summary>
        /// Toggle grid squares visualization
        /// </summary>
        [ContextMenu("Toggle Grid Squares")]
        public void ToggleGridSquares()
        {
            showGridSquares = !showGridSquares;
            
            if (showGridSquares)
            {
                CreateGridSquareVisualization();
            }
            else
            {
                // Destroy all grid squares
                if (squaresParent != null)
                {
                    DestroyImmediate(squaresParent);
                }
                nodeSquares.Clear();
                DestroyVisualizationObjects("GridSquare");
            }
        }
        
        /// <summary>
        /// Destroy visualization objects with specific name pattern
        /// </summary>
        private void DestroyVisualizationObjects(string namePattern)
        {
            for (int i = visualizationObjects.Count - 1; i >= 0; i--)
            {
                if (visualizationObjects[i] != null && visualizationObjects[i].name.Contains(namePattern))
                {
                    DestroyImmediate(visualizationObjects[i]);
                    visualizationObjects.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Refresh all visualizations
        /// </summary>
        [ContextMenu("Refresh Visualization")]
        public void RefreshVisualization()
        {
            // Clear all existing visualizations
            foreach (GameObject obj in visualizationObjects)
            {
                if (obj != null)
                    DestroyImmediate(obj);
            }
            visualizationObjects.Clear();
            nodeSquares.Clear();

            // Recreate visualizations
            if (gridManager != null && gridManager.IsInitialized)
            {
                CreateGridBoundsVisualization();
                CreateSelectedNodesVisualization(gridManager.SelectedNodes);
                CreatePathVisualization(gridManager.CurrentPath);
                
                if (showGridSquares)
                {
                    CreateGridSquareVisualization();
                }
            }
        }

        /// <summary>
        /// Test method to manually change colors of first few squares
        /// </summary>
        private void TestColorChange()
        {
            Debug.Log("Testing manual color change...");
            
            int testCount = 0;
            foreach (var kvp in nodeSquares)
            {
                if (testCount >= 3) break;
                
                GameObject square = kvp.Value;
                if (square != null)
                {
                    Renderer renderer = square.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // Destroy old material
                        if (renderer.material != null)
                        {
                            DestroyImmediate(renderer.material);
                        }
                        
                        // Create new bright red material for testing
                        Color testColor = testCount == 0 ? Color.red : (testCount == 1 ? Color.green : Color.blue);
                        testColor.a = useOpaqueColors ? 1f : 0.8f;
                        
                        renderer.material = CreateTransparentMaterial(testColor);
                        Debug.Log($"Changed test square {testCount} to color {testColor}");
                    }
                }
                testCount++;
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (gridManager != null)
            {
                gridManager.OnGridInitialized -= OnGridInitialized;
                gridManager.OnSelectionChanged -= OnSelectionChanged;
                gridManager.OnPathFound -= OnPathFound;
            }
            
            if (occupationManager != null)
            {
                occupationManager.OnNodeOccupationChanged -= OnNodeOccupationChanged;
            }

            // Clean up visualization objects
            foreach (GameObject obj in visualizationObjects)
            {
                if (obj != null)
                    DestroyImmediate(obj);
            }
        }
    }
}

using UnityEngine;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// Manual setup helper for the 3D grid pathfinding system
    /// Provides utility methods for manual scene configuration
    /// </summary>
    public class GridTestSceneSetup : MonoBehaviour
    {
        [Header("Manual Setup Helpers")]
        [SerializeField] private bool autoSetupOnStart = false;
        
        void Start()
        {
            // Only auto-setup if explicitly enabled
            if (autoSetupOnStart)
            {
                CreateRuntimeVisualizerOnly();
            }
        }

        /// <summary>
        /// Create only the runtime visualizer for better visual feedback
        /// Use this when you've set up your scene manually
        /// </summary>
        [ContextMenu("Create Runtime Visualizer Only")]
        public void CreateRuntimeVisualizerOnly()
        {
            CreateRuntimeVisualizer();
        }


        /// <summary>
        /// Create runtime visualizer for better visual feedback
        /// </summary>
        private void CreateRuntimeVisualizer()
        {
            // Check if runtime visualizer already exists
            RuntimeGridVisualizer existingVisualizer = FindObjectOfType<RuntimeGridVisualizer>();
            if (existingVisualizer != null)
            {
                Debug.Log("RuntimeGridVisualizer already exists");
                return;
            }

            // Create new runtime visualizer
            GameObject visualizerGO = new GameObject("RuntimeGridVisualizer");
            RuntimeGridVisualizer visualizer = visualizerGO.AddComponent<RuntimeGridVisualizer>();
            
            Debug.Log("Created RuntimeGridVisualizer for better visual feedback");
        }

    }
}

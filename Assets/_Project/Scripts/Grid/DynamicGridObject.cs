using UnityEngine;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// Example implementation of a dynamic object that can occupy grid nodes
    /// Attach this to any GameObject that moves and should affect pathfinding
    /// </summary>
    public class DynamicGridObject : MonoBehaviour, IDynamicGridObject
    {
        [Header("Dynamic Grid Settings")]
        [SerializeField] private float movementThreshold = 0.1f; // Minimum movement to trigger update
        [SerializeField] private bool autoRegister = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        public GameObject GameObject => gameObject;
        
        private Vector3 lastPosition;
        private GridOccupationManager occupationManager;
        private bool isRegistered = false;
        
        private void Start()
        {
            lastPosition = transform.position;
            
            if (autoRegister)
            {
                RegisterWithOccupationManager();
            }
        }
        
        private void RegisterWithOccupationManager()
        {
            if (occupationManager == null)
                occupationManager = FindObjectOfType<GridOccupationManager>();
            
            if (occupationManager != null && !isRegistered)
            {
                occupationManager.RegisterDynamicObject(this);
                isRegistered = true;
                
                if (debugMode)
                    Debug.Log($"DynamicGridObject: {name} registered with GridOccupationManager");
            }
        }
        
        public bool HasMoved()
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            return distanceMoved > movementThreshold;
        }
        
        public void UpdateLastPosition()
        {
            if (debugMode && HasMoved())
            {
                Debug.Log($"DynamicGridObject: {name} moved from {lastPosition} to {transform.position}");
            }
            
            lastPosition = transform.position;
        }
        
        /// <summary>
        /// Manually register this object with the occupation manager
        /// </summary>
        public void Register()
        {
            RegisterWithOccupationManager();
        }
        
        /// <summary>
        /// Manually unregister this object from the occupation manager
        /// </summary>
        public void Unregister()
        {
            if (occupationManager != null && isRegistered)
            {
                occupationManager.UnregisterDynamicObject(this);
                isRegistered = false;
                
                if (debugMode)
                    Debug.Log($"DynamicGridObject: {name} unregistered from GridOccupationManager");
            }
        }
        
        private void OnDestroy()
        {
            Unregister();
        }
        
        private void OnValidate()
        {
            // Ensure movement threshold is positive
            if (movementThreshold < 0)
                movementThreshold = 0.1f;
        }
    }
}

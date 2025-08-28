using UnityEngine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Enforces camera movement boundaries to keep camera within defined limits.
    /// Single responsibility: Constrain camera position within map bounds.
    /// </summary>
    [System.Serializable]
    public class CameraBounds : ICameraComponent
    {
        [Header("Boundary Settings")]
        [SerializeField] private Vector2 mapSize = new Vector2(50f, 50f);
        [SerializeField] private Vector2 centerOffset = Vector2.zero;
        [SerializeField] private bool enableBounds = true;
        [SerializeField] private bool visualizeInEditor = true;
        
        private Transform cameraTransform;
        private bool isActive = true;
        
        // Calculated boundary limits
        private float minX, maxX, minZ, maxZ;
        
        public bool IsActive 
        { 
            get => isActive; 
            set => isActive = value; 
        }
        
        public void Initialize(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
            CalculateBounds();
        }
        
        public void UpdateComponent()
        {
            if (!IsActive || !enableBounds || cameraTransform == null) return;
            
            EnforceBounds();
        }
        
        public void Cleanup()
        {
            cameraTransform = null;
        }
        
        /// <summary>
        /// Enforce camera position within defined boundaries
        /// </summary>
        private void EnforceBounds()
        {
            Vector3 currentPos = cameraTransform.position;
            Vector3 clampedPos = new Vector3(
                Mathf.Clamp(currentPos.x, minX, maxX),
                currentPos.y, // Don't clamp Y axis
                Mathf.Clamp(currentPos.z, minZ, maxZ)
            );
            
            // Only update position if it changed (avoid unnecessary transforms)
            if (Vector3.Distance(currentPos, clampedPos) > 0.001f)
            {
                cameraTransform.position = clampedPos;
            }
        }
        
        /// <summary>
        /// Calculate boundary limits based on map size and center offset
        /// </summary>
        private void CalculateBounds()
        {
            float halfWidth = mapSize.x * 0.5f;
            float halfHeight = mapSize.y * 0.5f;
            
            minX = centerOffset.x - halfWidth;
            maxX = centerOffset.x + halfWidth;
            minZ = centerOffset.y - halfHeight;
            maxZ = centerOffset.y + halfHeight;
        }
        
        /// <summary>
        /// Configure map boundaries at runtime
        /// </summary>
        public void ConfigureBounds(Vector2 size, Vector2 center = default)
        {
            mapSize = size;
            centerOffset = center;
            CalculateBounds();
        }
        
        /// <summary>
        /// Check if a position is within bounds
        /// </summary>
        public bool IsWithinBounds(Vector3 position)
        {
            return position.x >= minX && position.x <= maxX && 
                   position.z >= minZ && position.z <= maxZ;
        }
        
        /// <summary>
        /// Clamp a position to within bounds
        /// </summary>
        public Vector3 ClampToBounds(Vector3 position)
        {
            return new Vector3(
                Mathf.Clamp(position.x, minX, maxX),
                position.y,
                Mathf.Clamp(position.z, minZ, maxZ)
            );
        }
        
        /// <summary>
        /// Get current boundary information
        /// </summary>
        public (Vector2 min, Vector2 max, Vector2 size) GetBoundaryInfo()
        {
            return (
                new Vector2(minX, minZ),
                new Vector2(maxX, maxZ),
                mapSize
            );
        }
        
        /// <summary>
        /// Enable or disable boundary enforcement
        /// </summary>
        public void SetBoundsEnabled(bool enabled)
        {
            enableBounds = enabled;
        }
        
        /// <summary>
        /// Draw boundary gizmos in Scene view for debugging
        /// </summary>
        public void DrawBoundaryGizmos()
        {
            if (!visualizeInEditor) return;
            
            Gizmos.color = Color.yellow;
            
            Vector3 center = new Vector3(centerOffset.x, 0, centerOffset.y);
            Vector3 size = new Vector3(mapSize.x, 0.1f, mapSize.y);
            
            Gizmos.DrawWireCube(center, size);
            
            // Draw corner markers
            Gizmos.color = Color.red;
            float markerSize = 1f;
            
            Vector3[] corners = {
                new Vector3(minX, 0, minZ),
                new Vector3(maxX, 0, minZ),
                new Vector3(maxX, 0, maxZ),
                new Vector3(minX, 0, maxZ)
            };
            
            foreach (var corner in corners)
            {
                Gizmos.DrawSphere(corner, markerSize);
            }
        }
    }
}

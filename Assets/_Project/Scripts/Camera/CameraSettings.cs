using UnityEngine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Data-driven camera settings using ScriptableObject pattern.
    /// Allows designers to configure camera behavior without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "ProjectZero/Camera/Camera Settings")]
    public class CameraSettings : ScriptableObject
    {
        [Header("Movement Settings")]
        [SerializeField] [Range(1f, 50f)] 
        private float moveSpeed = 10f;
        
        [SerializeField] [Range(0.1f, 2f)] 
        private float movementSmoothTime = 0.3f;
        
        [SerializeField] 
        private bool useAcceleration = true;
        
        [SerializeField] [Range(1f, 5f)] 
        private float accelerationMultiplier = 2f;

        [Header("Zoom Settings")]
        [SerializeField] [Range(5f, 100f)] 
        private float minZoomDistance = 10f;
        
        [SerializeField] [Range(5f, 100f)] 
        private float maxZoomDistance = 50f;
        
        [SerializeField] [Range(0.1f, 5f)] 
        private float zoomSpeed = 2f;
        
        [SerializeField] [Range(0.1f, 2f)] 
        private float zoomSmoothTime = 0.2f;

        [Header("Edge Scrolling")]
        [SerializeField] 
        private bool enableEdgeScrolling = true;
        
        [SerializeField] [Range(0f, 50f)] 
        private float edgeScrollBorder = 20f;
        
        [SerializeField] [Range(0.1f, 2f)] 
        private float edgeScrollSpeedMultiplier = 0.5f;

        [Header("Bounds")]
        [SerializeField] 
        private Vector2 mapBounds = new Vector2(100f, 100f);
        
        [SerializeField] 
        private bool enforceBounds = true;

        [Header("Follow Settings")]
        [SerializeField] [Range(0.1f, 5f)] 
        private float followSpeed = 1f;
        
        [SerializeField] [Range(0f, 20f)] 
        private float followDeadZone = 3f;
        
        [SerializeField] [Range(0.1f, 2f)] 
        private float followSmoothTime = 0.5f;

        [Header("Input Response")]
        [SerializeField] [Range(0.1f, 3f)] 
        private float inputResponseTime = 0.1f;
        
        [SerializeField] 
        private bool allowInputDuringPause = false;

        // Public properties with validation
        public float MoveSpeed => Mathf.Max(0.1f, moveSpeed);
        public float MovementSmoothTime => Mathf.Max(0.01f, movementSmoothTime);
        public bool UseAcceleration => useAcceleration;
        public float AccelerationMultiplier => Mathf.Max(1f, accelerationMultiplier);
        
        public float MinZoomDistance => Mathf.Max(1f, minZoomDistance);
        public float MaxZoomDistance => Mathf.Max(minZoomDistance + 1f, maxZoomDistance);
        public float ZoomSpeed => Mathf.Max(0.1f, zoomSpeed);
        public float ZoomSmoothTime => Mathf.Max(0.01f, zoomSmoothTime);
        
        public bool EnableEdgeScrolling => enableEdgeScrolling;
        public float EdgeScrollBorder => Mathf.Max(0f, edgeScrollBorder);
        public float EdgeScrollSpeedMultiplier => Mathf.Clamp01(edgeScrollSpeedMultiplier);
        
        public Vector2 MapBounds => new Vector2(Mathf.Max(1f, mapBounds.x), Mathf.Max(1f, mapBounds.y));
        public bool EnforceBounds => enforceBounds;
        
        public float FollowSpeed => Mathf.Max(0.1f, followSpeed);
        public float FollowDeadZone => Mathf.Max(0f, followDeadZone);
        public float FollowSmoothTime => Mathf.Max(0.01f, followSmoothTime);
        
        public float InputResponseTime => Mathf.Max(0.01f, inputResponseTime);
        public bool AllowInputDuringPause => allowInputDuringPause;

        /// <summary>
        /// Get zoom distance based on normalized zoom level (0.0 - 1.0)
        /// </summary>
        public float GetZoomDistance(float normalizedZoom)
        {
            return Mathf.Lerp(MaxZoomDistance, MinZoomDistance, Mathf.Clamp01(normalizedZoom));
        }

        /// <summary>
        /// Get normalized zoom level from distance
        /// </summary>
        public float GetNormalizedZoom(float distance)
        {
            return Mathf.InverseLerp(MaxZoomDistance, MinZoomDistance, distance);
        }

        private void OnValidate()
        {
            // Ensure proper value ranges when editing in inspector
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            minZoomDistance = Mathf.Max(1f, minZoomDistance);
            maxZoomDistance = Mathf.Max(minZoomDistance + 1f, maxZoomDistance);
            mapBounds = new Vector2(Mathf.Max(1f, mapBounds.x), Mathf.Max(1f, mapBounds.y));
        }
    }
}

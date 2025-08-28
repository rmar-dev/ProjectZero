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
        
        [SerializeField] [Tooltip("When enabled, WASD moves relative to camera rotation (W = forward in camera direction). When disabled, WASD moves in world directions (W = north).")]
        private bool useCameraRelativeMovement = true;

        [Header("Zoom Settings")]
        [SerializeField] [Range(5f, 100f)] 
        private float minZoomDistance = 10f;
        
        [SerializeField] [Range(5f, 100f)] 
        private float maxZoomDistance = 50f;
        
        [SerializeField] [Range(0.1f, 5f)] 
        private float zoomSpeed = 2f;
        
        [SerializeField] [Range(0.1f, 2f)] 
        private float zoomSmoothTime = 0.2f;
        
        [Header("Zoom-Coupled Rotation")]
        [SerializeField] [Tooltip("When enabled, zooming in rotates camera down (more angled view) and zooming out rotates up (more top-down view)")]
        private bool enableZoomRotation = true;
        
        [Header("Pitch Control (Up/Down Rotation)")]
        [SerializeField] [Range(10f, 90f)] [Tooltip("Maximum pitch angle when fully zoomed out (top-down strategic view)")]
        private float maxPitchAngle = 80f;
        
        [SerializeField] [Range(10f, 80f)] [Tooltip("Minimum pitch angle when fully zoomed in (closer tactical view)")]
        private float minPitchAngle = 45f;
        
        [Header("Yaw Control (Left/Right Rotation)")]
        [SerializeField] [Tooltip("Enable yaw rotation coupling with zoom")]
        private bool enableYawRotation = false;
        
        [SerializeField] [Range(-180f, 180f)] [Tooltip("Maximum yaw angle when fully zoomed out")]
        private float maxYawAngle = 0f;
        
        [SerializeField] [Range(-180f, 180f)] [Tooltip("Minimum yaw angle when fully zoomed in")]
        private float minYawAngle = 0f;
        
        [Header("Rotation Timing")]
        [SerializeField] [Range(0.1f, 2f)] [Tooltip("How quickly the camera pitch rotates when zooming")]
        private float pitchSmoothTime = 0.3f;
        
        [SerializeField] [Range(0.1f, 2f)] [Tooltip("How quickly the camera yaw rotates when zooming")]
        private float yawSmoothTime = 0.3f;

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
        public bool UseCameraRelativeMovement => useCameraRelativeMovement;
        
        public float MinZoomDistance => Mathf.Max(1f, minZoomDistance);
        public float MaxZoomDistance => Mathf.Max(minZoomDistance + 1f, maxZoomDistance);
        public float ZoomSpeed => Mathf.Max(0.1f, zoomSpeed);
        public float ZoomSmoothTime => Mathf.Max(0.01f, zoomSmoothTime);
        
        public bool EnableZoomRotation => enableZoomRotation;
        
        // Pitch properties
        public float MaxPitchAngle => Mathf.Clamp(maxPitchAngle, 10f, 90f);
        public float MinPitchAngle => Mathf.Clamp(minPitchAngle, 10f, Mathf.Min(80f, maxPitchAngle - 5f));
        public float PitchSmoothTime => Mathf.Max(0.01f, pitchSmoothTime);
        
        // Yaw properties
        public bool EnableYawRotation => enableYawRotation;
        public float MaxYawAngle => Mathf.Clamp(maxYawAngle, -180f, 180f);
        public float MinYawAngle => Mathf.Clamp(minYawAngle, -180f, 180f);
        public float YawSmoothTime => Mathf.Max(0.01f, yawSmoothTime);
        
        // Backward compatibility
        public float MaxRotationAngle => MaxPitchAngle;
        public float MinRotationAngle => MinPitchAngle;
        public float RotationSmoothTime => PitchSmoothTime;
        
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
        
        /// <summary>
        /// Get camera rotation angle based on normalized zoom level (0.0 - 1.0)
        /// Zoomed out (0.0) = MaxRotationAngle (more top-down)
        /// Zoomed in (1.0) = MinRotationAngle (more angled)
        /// </summary>
        public float GetRotationAngle(float normalizedZoom)
        {
            if (!EnableZoomRotation) return MaxRotationAngle; // Default to strategic view if disabled
            return Mathf.Lerp(MaxRotationAngle, MinRotationAngle, Mathf.Clamp01(normalizedZoom));
        }
        
        /// <summary>
        /// Get camera pitch angle based on normalized zoom level (0.0 - 1.0)
        /// Zoomed out (0.0) = MaxPitchAngle (more top-down)
        /// Zoomed in (1.0) = MinPitchAngle (more angled)
        /// </summary>
        public float GetPitchAngle(float normalizedZoom)
        {
            if (!EnableZoomRotation) return MaxPitchAngle; // Default to strategic view if disabled
            return Mathf.Lerp(MaxPitchAngle, MinPitchAngle, Mathf.Clamp01(normalizedZoom));
        }
        
        /// <summary>
        /// Get camera yaw angle based on normalized zoom level (0.0 - 1.0)
        /// Zoomed out (0.0) = MaxYawAngle
        /// Zoomed in (1.0) = MinYawAngle
        /// </summary>
        public float GetYawAngle(float normalizedZoom)
        {
            if (!EnableYawRotation) return 0f; // No yaw rotation if disabled
            return Mathf.Lerp(MaxYawAngle, MinYawAngle, Mathf.Clamp01(normalizedZoom));
        }

        private void OnValidate()
        {
            // Ensure proper value ranges when editing in inspector
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            minZoomDistance = Mathf.Max(1f, minZoomDistance);
            maxZoomDistance = Mathf.Max(minZoomDistance + 1f, maxZoomDistance);
            mapBounds = new Vector2(Mathf.Max(1f, mapBounds.x), Mathf.Max(1f, mapBounds.y));
            
            // Ensure pitch angles are valid
            if (maxPitchAngle <= minPitchAngle) maxPitchAngle = minPitchAngle + 5f;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Creates a default camera settings asset in the Resources folder
        /// </summary>
        [ContextMenu("Create Default Camera Settings")]
        public void CreateDefaultAsset()
        {
            var newSettings = CreateInstance<CameraSettings>();
            
            // Set up good default values for RTS tactical camera
            newSettings.moveSpeed = 10f;
            newSettings.zoomSpeed = 2f;
            newSettings.minZoomDistance = 10f;
            newSettings.maxZoomDistance = 50f;
            newSettings.enableZoomRotation = true;
            newSettings.maxPitchAngle = 80f;
            newSettings.minPitchAngle = 45f;
            newSettings.enableYawRotation = false;
            newSettings.maxYawAngle = 0f;
            newSettings.minYawAngle = 0f;
            newSettings.pitchSmoothTime = 0.3f;
            newSettings.yawSmoothTime = 0.3f;
            
            string path = "Assets/_Project/Resources/DefaultCameraSettings.asset";
            UnityEditor.AssetDatabase.CreateAsset(newSettings, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            Debug.Log($"Created DefaultCameraSettings at {path}");
        }
#endif
    }
}

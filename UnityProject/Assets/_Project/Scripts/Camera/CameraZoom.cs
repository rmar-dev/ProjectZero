using UnityEngine;
using Cinemachine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Handles camera zoom by controlling Cinemachine virtual camera distance.
    /// Single responsibility: Manage camera distance/zoom based on input.
    /// </summary>
    [System.Serializable]
    public class CameraZoom : ICameraComponent
    {
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 20f;
        [SerializeField] private float smoothTime = 0.2f;
        [SerializeField] private bool enableSmoothing = true;
        
        private Transform cameraTransform;
        private CinemachineVirtualCamera virtualCamera;
        private CinemachineFramingTransposer framingTransposer;
        
        private float targetDistance;
        private float currentDistance;
        private float zoomVelocity = 0f;
        private bool isActive = true;
        
        public bool IsActive 
        { 
            get => isActive; 
            set => isActive = value; 
        }
        
        public void Initialize(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
            FindCinemachineComponents();
            InitializeZoomDistance();
        }
        
        public void UpdateComponent()
        {
            if (!IsActive || framingTransposer == null) return;
            
            ProcessZoom();
        }
        
        public void Cleanup()
        {
            cameraTransform = null;
            virtualCamera = null;
            framingTransposer = null;
        }
        
        /// <summary>
        /// Set zoom input (positive = zoom out, negative = zoom in)
        /// </summary>
        public void SetZoomInput(float zoomInput)
        {
            if (!IsActive) return;
            
            float zoomDelta = zoomInput * zoomSpeed * Time.unscaledDeltaTime;
            targetDistance = Mathf.Clamp(targetDistance + zoomDelta, minZoom, maxZoom);
        }
        
        /// <summary>
        /// Set zoom to specific distance
        /// </summary>
        public void SetZoomDistance(float distance)
        {
            targetDistance = Mathf.Clamp(distance, minZoom, maxZoom);
        }
        
        /// <summary>
        /// Apply zoom changes to the virtual camera
        /// </summary>
        private void ProcessZoom()
        {
            // Smooth zoom interpolation if enabled
            if (enableSmoothing)
            {
                currentDistance = Mathf.SmoothDamp(
                    currentDistance,
                    targetDistance,
                    ref zoomVelocity,
                    smoothTime
                );
            }
            else
            {
                currentDistance = targetDistance;
            }
            
            // Apply distance to Cinemachine
            framingTransposer.m_CameraDistance = currentDistance;
        }
        
        /// <summary>
        /// Find and cache Cinemachine components
        /// </summary>
        private void FindCinemachineComponents()
        {
            // Try to find virtual camera in the same GameObject first
            if (cameraTransform != null)
            {
                virtualCamera = cameraTransform.GetComponent<CinemachineVirtualCamera>();
            }
            
            // If not found, search in scene
            if (virtualCamera == null)
            {
                virtualCamera = Object.FindObjectOfType<CinemachineVirtualCamera>();
            }
            
            // Get the framing transposer component
            if (virtualCamera != null)
            {
                framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
            
            if (framingTransposer == null)
            {
                Debug.LogWarning("CameraZoom: No CinemachineFramingTransposer found. Zoom will not work.");
            }
        }
        
        /// <summary>
        /// Initialize zoom distance from current Cinemachine settings
        /// </summary>
        private void InitializeZoomDistance()
        {
            if (framingTransposer != null)
            {
                currentDistance = framingTransposer.m_CameraDistance;
                targetDistance = currentDistance;
                
                // Ensure current distance is within bounds
                targetDistance = Mathf.Clamp(targetDistance, minZoom, maxZoom);
                currentDistance = targetDistance;
            }
        }
        
        /// <summary>
        /// Configure zoom settings at runtime
        /// </summary>
        public void ConfigureZoom(float speed, float min, float max, float smoothTime = 0.2f)
        {
            this.zoomSpeed = speed;
            this.minZoom = min;
            this.maxZoom = max;
            this.smoothTime = smoothTime;
            
            // Clamp current target to new bounds
            targetDistance = Mathf.Clamp(targetDistance, minZoom, maxZoom);
        }
        
        /// <summary>
        /// Get current zoom distance for debugging
        /// </summary>
        public float GetCurrentZoom() => currentDistance;
        
        /// <summary>
        /// Get zoom progress (0 = min zoom, 1 = max zoom)
        /// </summary>
        public float GetZoomProgress() => Mathf.InverseLerp(minZoom, maxZoom, currentDistance);
    }
}

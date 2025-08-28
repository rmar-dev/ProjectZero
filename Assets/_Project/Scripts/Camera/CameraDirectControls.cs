using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Direct camera controls that bypass Input Actions for immediate zoom and rotation.
    /// Provides mouse wheel zoom and right-click drag rotation functionality.
    /// </summary>
    public class CameraDirectControls : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TacticalCameraController cameraController;
        [SerializeField] private CinemachineCamera virtualCamera;
        
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 3f;
        [SerializeField] private float zoomSmoothTime = 0.2f;
        [SerializeField] private float minZoomDistance = 5f;
        [SerializeField] private float maxZoomDistance = 50f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        [SerializeField] private bool enableZoomCoupledRotation = true;
        [SerializeField] private float minPitch = 10f;
        [SerializeField] private float maxPitch = 80f;
        
        [Header("Sensitivity Settings")]
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private float scrollSensitivity = 1f;
        
        [Header("Rotation Tuning")]
        [SerializeField] [Range(0.1f, 3f)] private float pitchSensitivity = 1f;
        [SerializeField] [Range(0.1f, 3f)] private float yawSensitivity = 1f;
        [SerializeField] [Tooltip("How quickly zoom rotation responds (lower = more responsive)")]
        [Range(0.05f, 1f)] private float zoomRotationSmoothTime = 0.3f;
        [SerializeField] [Tooltip("Enforce strict limits even for zoom-coupled rotation")]
        private bool enforceStrictLimits = true;
        
        private CinemachinePositionComposer positionComposer;
        
        // Smooth zoom variables
        private float targetZoomDistance = 25f;
        private float currentZoomVelocity;
        
        // Smooth rotation variables
        private Vector3 targetRotation = new Vector3(60f, 0f, 0f);
        private Vector3 rotationVelocity;

        void Start()
        {
            // Auto-find components if not assigned
            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<TacticalCameraController>();
                Debug.Log($"[CameraDirectControls] Found TacticalCameraController: {cameraController != null}");
            }
            
            // Disable CameraZoomControl to prevent zoom conflicts
            var zoomControl = GetComponent<CameraZoomControl>();
            if (zoomControl != null)
            {
                zoomControl.IsEnabled = false;
                Debug.Log("[CameraDirectControls] Disabled CameraZoomControl to prevent zoom conflicts");
            }
            
            if (virtualCamera == null)
            {
                virtualCamera = FindFirstObjectByType<CinemachineCamera>();
                Debug.Log($"[CameraDirectControls] Found CinemachineCamera: {virtualCamera != null}");
                if (virtualCamera != null)
                {
                    Debug.Log($"[CameraDirectControls] CinemachineCamera name: {virtualCamera.name}");
                }
            }
                
            if (virtualCamera != null)
            {
                positionComposer = virtualCamera.GetComponent<CinemachinePositionComposer>();
                if (positionComposer != null)
                {
                    targetZoomDistance = positionComposer.CameraDistance;
                }
                else
                {
                    // Try to add one
                    positionComposer = virtualCamera.gameObject.AddComponent<CinemachinePositionComposer>();
                    positionComposer.CameraDistance = targetZoomDistance;
                }
                
                // Initialize target rotation from current camera rotation
                targetRotation = virtualCamera.transform.eulerAngles;
            }
            else
            {
                Debug.LogError("[CameraDirectControls] No CinemachineCamera found! Camera controls will not work.");
            }
            
            Debug.Log("[CameraDirectControls] Use mouse wheel to zoom, Right-click + drag to rotate camera");
        }

        void Update()
        {
            HandleSmoothZoom();
            HandleSmoothRotation();
            ApplySmoothTransforms();
        }

        private void HandleSmoothZoom()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            
            Vector2 scroll = mouse.scroll.ReadValue();
            
            // Use normalized scroll input for consistent feel across different mice
            if (Mathf.Abs(scroll.y) > 0.1f)
            {
                // Convert scroll to zoom delta with proper sensitivity
                float scrollDelta = scroll.y * scrollSensitivity;
                
                // Update target zoom distance (discrete steps for better control)
                float zoomDelta = scrollDelta * zoomSpeed;
                targetZoomDistance = Mathf.Clamp(
                    targetZoomDistance - zoomDelta,
                    minZoomDistance,
                    maxZoomDistance
                );
                
                // Update controller context
                if (cameraController != null)
                {
                    float normalizedZoom = Mathf.InverseLerp(maxZoomDistance, minZoomDistance, targetZoomDistance);
                    cameraController.ZoomLevel = normalizedZoom;
                    
                    // Apply zoom-coupled rotation if enabled
                    if (enableZoomCoupledRotation && cameraController.Settings.EnableZoomRotation)
                    {
                        float targetPitchFromZoom = cameraController.Settings.GetPitchAngle(normalizedZoom);
                        float targetYawFromZoom = cameraController.Settings.GetYawAngle(normalizedZoom);
                        
                        // Update target rotation based on zoom (only if not manually overridden)
                        // RTS-style: Only apply zoom rotation to pitch, not yaw
                        if (!Mouse.current.rightButton.isPressed)
                        {
                            targetRotation.x = targetPitchFromZoom;
                            // Note: We don't modify yaw for RTS-style zoom rotation
                            
                            Debug.Log($"[CameraDirectControls] Zoom-coupled pitch rotation - Zoom: {normalizedZoom:F2}, Target Pitch: {targetPitchFromZoom:F1}°");
                        }
                    }
                }
            }
        }

        private void HandleSmoothRotation()
        {
            var mouse = Mouse.current;
            if (mouse == null || virtualCamera == null) return;
            
            // Right-click + drag to rotate
            if (mouse.rightButton.isPressed)
            {
                Vector2 mouseDelta = mouse.delta.ReadValue();
                
                // Apply mouse sensitivity and rotation speed with separate pitch/yaw tuning
                float yawDelta = mouseDelta.x * mouseSensitivity * rotationSpeed * yawSensitivity;
                float pitchDelta = -mouseDelta.y * mouseSensitivity * rotationSpeed * pitchSensitivity;
                
                // Update target rotation
                targetRotation.y += yawDelta;
                targetRotation.x += pitchDelta;
                
                // Apply pitch limits from CameraSettings if available, otherwise use fallback values
                float minPitchLimit = minPitch;
                float maxPitchLimit = maxPitch;
                
                if (cameraController?.Settings != null)
                {
                    minPitchLimit = cameraController.Settings.MinPitchAngle;
                    maxPitchLimit = cameraController.Settings.MaxPitchAngle;
                }
                
                // Only clamp pitch - let yaw rotate freely for RTS-style camera
                targetRotation.x = Mathf.Clamp(targetRotation.x, minPitchLimit, maxPitchLimit);
                
                // Normalize yaw to 0-360 range (no limits, full rotation allowed)
                targetRotation.y = targetRotation.y % 360f;
                if (targetRotation.y < 0f) targetRotation.y += 360f;
                
                Debug.Log($"[CameraDirectControls] Manual rotation - Pitch: {targetRotation.x:F1}° (limits: {minPitchLimit:F1}°-{maxPitchLimit:F1}°), Yaw: {targetRotation.y:F1}° (unlimited)");
            }
        }

        private void ApplySmoothTransforms()
        {
            if (virtualCamera == null || positionComposer == null) return;
            
            // Smooth zoom with SmoothDamp
            float currentDistance = positionComposer.CameraDistance;
            float smoothDistance = Mathf.SmoothDamp(
                currentDistance,
                targetZoomDistance,
                ref currentZoomVelocity,
                zoomSmoothTime
            );
            positionComposer.CameraDistance = smoothDistance;
            
            // Smooth rotation with SmoothDampAngle for each axis
            Vector3 currentRotation = virtualCamera.transform.eulerAngles;
            
            // Handle angle wrapping for smooth interpolation
            float smoothPitch = Mathf.SmoothDampAngle(
                currentRotation.x,
                targetRotation.x,
                ref rotationVelocity.x,
                rotationSmoothTime
            );
            
            float smoothYaw = Mathf.SmoothDampAngle(
                currentRotation.y,
                targetRotation.y,
                ref rotationVelocity.y,
                rotationSmoothTime
            );
            
            // Apply smoothed rotation
            virtualCamera.transform.eulerAngles = new Vector3(smoothPitch, smoothYaw, 0f);
        }

        void OnGUI()
        {
            // Show instructions
            string zoomRotationStatus = "";
            if (enableZoomCoupledRotation && cameraController?.Settings.EnableZoomRotation == true)
            {
                zoomRotationStatus = "\n• Zoom-Coupled Rotation: ENABLED";
            }
            else if (enableZoomCoupledRotation)
            {
                zoomRotationStatus = "\n• Zoom-Coupled Rotation: DISABLED (check CameraSettings)";
            }
            
            GUI.Label(new Rect(10, 10, 400, 80), 
                "Camera Test Controls:\n" +
                "• Mouse Wheel: Zoom In/Out\n" +
                "• Right-click + Drag: Rotate Camera" +
                zoomRotationStatus);
        }

        // Public methods to test zoom and rotation programmatically
        [ContextMenu("Zoom In")]
        public void TestZoomIn()
        {
            targetZoomDistance = Mathf.Max(targetZoomDistance - 5f, minZoomDistance);
        }

        [ContextMenu("Zoom Out")] 
        public void TestZoomOut()
        {
            targetZoomDistance = Mathf.Min(targetZoomDistance + 5f, maxZoomDistance);
        }

        [ContextMenu("Reset Camera Angle")]
        public void ResetCameraAngle()
        {
            targetRotation = new Vector3(60f, 0f, 0f);
        }

        [ContextMenu("Reset Zoom")]
        public void ResetZoom()
        {
            targetZoomDistance = (minZoomDistance + maxZoomDistance) / 2f;
        }
    }
}

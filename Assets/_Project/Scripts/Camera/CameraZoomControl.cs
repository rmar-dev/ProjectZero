using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Handles mouse wheel zoom control for the tactical camera.
    /// Integrates with Cinemachine for smooth zoom transitions.
    /// </summary>
    public class CameraZoomControl : MonoBehaviour, ICameraComponent
    {
        [Header("Input Configuration")]
        [SerializeField] private InputActionReference zoomAction;

        // Component state
        public bool IsEnabled { get; set; } = true;
        public int UpdatePriority => 90; // High priority but after movement

        // Dependencies
        private ICameraContext cameraContext;
        private CinemachinePositionComposer positionComposer;
        private float targetZoomDistance;
        private float currentZoomVelocity;
        
        // Zoom-coupled rotation
        private Transform virtualCameraTransform;
        private float targetPitchAngle;
        private float targetYawAngle;
        private float currentPitchVelocity;
        private float currentYawVelocity;

        #region ICameraComponent Implementation

        public void Initialize(ICameraContext context)
        {
            cameraContext = context;
            
            // Get Cinemachine position composer and virtual camera transform
            if (cameraContext.VirtualCamera != null)
            {
                positionComposer = cameraContext.VirtualCamera.GetComponent<CinemachinePositionComposer>();
                virtualCameraTransform = cameraContext.VirtualCamera.transform;
                
                if (positionComposer != null)
                {
                    targetZoomDistance = positionComposer.CameraDistance;
                }
                
                // Initialize rotation angles based on current zoom
                if (virtualCameraTransform != null)
                {
                    float normalizedZoom = cameraContext.Settings.GetNormalizedZoom(targetZoomDistance);
                    targetPitchAngle = cameraContext.Settings.GetPitchAngle(normalizedZoom);
                    targetYawAngle = cameraContext.Settings.GetYawAngle(normalizedZoom);
                }
            }

            // Enable input action
            if (zoomAction != null)
            {
                zoomAction.action.Enable();
            }
            else
            {
                Debug.LogWarning($"[CameraZoomControl] No zoom input action assigned on {gameObject.name}");
            }
        }

        public void UpdateComponent()
        {
            if (!IsEnabled || cameraContext == null) return;

            HandleZoomInput();
            ApplyZoom();
            ApplyRotation();
        }

        #endregion

        #region Zoom Logic

        private void HandleZoomInput()
        {
            // Skip input during tactical pause unless allowed
            if (Time.timeScale == 0f && !cameraContext.Settings.AllowInputDuringPause)
            {
                return;
            }

            float zoomInput = GetZoomInput();
            
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                // Calculate zoom delta
                float zoomDelta = zoomInput * cameraContext.Settings.ZoomSpeed * Time.unscaledDeltaTime;
                
                // Update target zoom distance
                targetZoomDistance = Mathf.Clamp(
                    targetZoomDistance - zoomDelta,
                    cameraContext.Settings.MinZoomDistance,
                    cameraContext.Settings.MaxZoomDistance
                );

                // Update context zoom level
                cameraContext.ZoomLevel = cameraContext.Settings.GetNormalizedZoom(targetZoomDistance);
                
                // Update target rotation angles based on zoom if enabled
                if (cameraContext.Settings.EnableZoomRotation)
                {
                    targetPitchAngle = cameraContext.Settings.GetPitchAngle(cameraContext.ZoomLevel);
                    if (cameraContext.Settings.EnableYawRotation)
                    {
                        targetYawAngle = cameraContext.Settings.GetYawAngle(cameraContext.ZoomLevel);
                    }
                    Debug.Log($"[ZoomRotation] Zoom: {cameraContext.ZoomLevel:F2}, Target Pitch: {targetPitchAngle:F1}°, Target Yaw: {targetYawAngle:F1}°");
                }
                
                cameraContext.IsInputActive = true;
            }
        }

        private void ApplyZoom()
        {
            if (positionComposer == null) return;

            // Smooth zoom interpolation that feels natural between zoom levels
            float currentDistance = positionComposer.CameraDistance;
            float distanceToTarget = Mathf.Abs(targetZoomDistance - currentDistance);
            
            if (distanceToTarget < 0.1f)
            {
                // Snap when extremely close to prevent micro-movements
                positionComposer.CameraDistance = targetZoomDistance;
            }
            else
            {
                // Smooth interpolation for pleasant zoom transitions
                positionComposer.CameraDistance = Mathf.SmoothDamp(
                    currentDistance,
                    targetZoomDistance,
                    ref currentZoomVelocity,
                    cameraContext.Settings.ZoomSmoothTime * 0.8f, // Slightly faster than setting but still smooth
                    Mathf.Infinity,
                    Time.unscaledDeltaTime
                );
            }
        }
        
        private void ApplyRotation()
        {
            if (!cameraContext.Settings.EnableZoomRotation)
            {
                Debug.LogWarning("[ZoomRotation] Zoom rotation is disabled in settings");
                return;
            }
            
            if (virtualCameraTransform == null)
            {
                Debug.LogError("[ZoomRotation] Virtual camera transform is null!");
                return;
            }

            // Get current rotation
            Vector3 currentRotation = virtualCameraTransform.eulerAngles;
            Vector3 beforeRotation = currentRotation;
            
            Debug.Log($"[ZoomRotation] BEFORE - Current rotation: {currentRotation}, ZoomLevel: {cameraContext.ZoomLevel:F2}");
            
            // Smooth rotation interpolation for X-axis (pitch)
            float currentXRotation = currentRotation.x;
            float currentYRotation = currentRotation.y;
            
            // Handle angle wrapping (0-360 vs -180 to 180)
            if (currentXRotation > 180f) currentXRotation -= 360f;
            if (currentYRotation > 180f) currentYRotation -= 360f;
            
            Debug.Log($"[ZoomRotation] Normalized angles - Pitch: {currentXRotation:F1}°, Yaw: {currentYRotation:F1}°");
            Debug.Log($"[ZoomRotation] Target angles - Pitch: {targetPitchAngle:F1}°, Yaw: {targetYawAngle:F1}°");
            
            // Apply pitch rotation (up/down)
            float smoothedPitch = Mathf.SmoothDampAngle(
                currentXRotation,
                targetPitchAngle,
                ref currentPitchVelocity,
                cameraContext.Settings.PitchSmoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );
            
            // Apply yaw rotation (left/right) if enabled
            float smoothedYaw = currentYRotation;
            if (cameraContext.Settings.EnableYawRotation)
            {
                smoothedYaw = Mathf.SmoothDampAngle(
                    currentYRotation,
                    targetYawAngle,
                    ref currentYawVelocity,
                    cameraContext.Settings.YawSmoothTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime
                );
            }
            
            Debug.Log($"[ZoomRotation] Smoothed angles - Pitch: {smoothedPitch:F1}°, Yaw: {smoothedYaw:F1}°");
            
            // Apply the smoothed rotation
            Vector3 newRotation = new Vector3(smoothedPitch, smoothedYaw, currentRotation.z);
            virtualCameraTransform.eulerAngles = newRotation;
            
            // Immediately check if the rotation was actually applied
            Vector3 actualRotation = virtualCameraTransform.eulerAngles;
            Debug.Log($"[ZoomRotation] AFTER - Set rotation to: {newRotation}, Actual rotation: {actualRotation}");
            
            // Check if Cinemachine is overriding our rotation
            if (Vector3.Distance(newRotation, actualRotation) > 1f)
            {
                Debug.LogWarning($"[ZoomRotation] CINEMACHINE OVERRIDE DETECTED! Set: {newRotation}, Got: {actualRotation}");
            }
            
            // Also check one frame later to see if it gets reset
            StartCoroutine(CheckRotationStability(newRotation));
        }

        private float GetZoomInput()
        {
            if (zoomAction?.action != null && zoomAction.action.enabled)
            {
                return zoomAction.action.ReadValue<float>();
            }

            // No fallback - require Input Action to be configured
            if (zoomAction == null)
            {
                Debug.LogWarning($"[CameraZoomControl] Zoom Action not assigned on {gameObject.name}. Mouse wheel zoom will not work.");
            }
            
            return 0f;
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            zoomAction?.action?.Enable();
        }

        private void OnDisable()
        {
            zoomAction?.action?.Disable();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set zoom level directly (0.0 = max zoom out, 1.0 = max zoom in)
        /// </summary>
        public void SetZoom(float normalizedZoom, bool smooth = true)
        {
            if (cameraContext == null) return;

            float targetDistance = cameraContext.Settings.GetZoomDistance(normalizedZoom);
            
            if (smooth)
            {
                targetZoomDistance = targetDistance;
            }
            else
            {
                targetZoomDistance = targetDistance;
                if (positionComposer != null)
                {
                    positionComposer.CameraDistance = targetDistance;
                }
            }
            
            // Update rotation angles based on zoom if enabled
            if (cameraContext.Settings.EnableZoomRotation)
            {
                targetPitchAngle = cameraContext.Settings.GetPitchAngle(normalizedZoom);
                if (cameraContext.Settings.EnableYawRotation)
                {
                    targetYawAngle = cameraContext.Settings.GetYawAngle(normalizedZoom);
                }
                
                // Apply rotation immediately if not smooth
                if (!smooth && virtualCameraTransform != null)
                {
                    Vector3 currentRotation = virtualCameraTransform.eulerAngles;
                    float newYaw = cameraContext.Settings.EnableYawRotation ? targetYawAngle : currentRotation.y;
                    virtualCameraTransform.eulerAngles = new Vector3(
                        targetPitchAngle,
                        newYaw,
                        currentRotation.z
                    );
                }
            }

            cameraContext.ZoomLevel = normalizedZoom;
        }

        /// <summary>
        /// Set the input action reference for zoom
        /// </summary>
        public void SetZoomAction(InputActionReference actionReference)
        {
            if (zoomAction != null)
            {
                zoomAction.action.Disable();
            }

            zoomAction = actionReference;

            if (zoomAction != null && enabled)
            {
                zoomAction.action.Enable();
            }
        }

        /// <summary>
        /// Get current zoom distance for debugging
        /// </summary>
        public float GetCurrentZoomDistance() => positionComposer?.CameraDistance ?? 0f;

        #endregion
        
        #region Debug Helpers
        
        private System.Collections.IEnumerator CheckRotationStability(Vector3 expectedRotation)
        {
            yield return null; // Wait one frame
            
            if (virtualCameraTransform != null)
            {
                Vector3 actualRotation = virtualCameraTransform.eulerAngles;
                if (Vector3.Distance(expectedRotation, actualRotation) > 1f)
                {
                    Debug.LogWarning($"[ZoomRotation] ROTATION RESET DETECTED! Expected: {expectedRotation}, Got: {actualRotation} (after 1 frame)");
                    
                    // Check what components might be overriding rotation
                    var cinemachineComponents = cameraContext.VirtualCamera.GetComponents<MonoBehaviour>();
                    Debug.Log($"[ZoomRotation] Cinemachine components on virtual camera:");
                    foreach (var comp in cinemachineComponents)
                    {
                        Debug.Log($"  - {comp.GetType().Name}: {(comp.enabled ? "ENABLED" : "disabled")}");
                    }
                }
            }
        }
        
        #endregion
    }
}

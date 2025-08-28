using UnityEngine;
using Unity.Cinemachine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Zoom rotation implementation that works with Cinemachine by using CinemachinePanTilt
    /// This approach integrates with Cinemachine instead of fighting against it
    /// </summary>
    public class CinemachineZoomRotation : MonoBehaviour, ICameraComponent
    {
        // Component state
        public bool IsEnabled { get; set; } = true;
        public int UpdatePriority => 85; // Run after zoom but before other rotation

        // Dependencies
        private ICameraContext cameraContext;
        private CinemachinePanTilt panTiltComponent;
        
        // Rotation state
        private float targetTiltValue;
        private float targetPanValue;
        private float currentTiltVelocity;
        private float currentPanVelocity;

        #region ICameraComponent Implementation

        public void Initialize(ICameraContext context)
        {
            cameraContext = context;
            
            // Get or add the PanTilt component
            if (cameraContext.VirtualCamera != null)
            {
                panTiltComponent = cameraContext.VirtualCamera.GetComponent<CinemachinePanTilt>();
                if (panTiltComponent == null)
                {
                    panTiltComponent = cameraContext.VirtualCamera.gameObject.AddComponent<CinemachinePanTilt>();
                    Debug.Log("[CinemachineZoomRotation] Added CinemachinePanTilt component");
                }

                // Initialize rotation values based on current zoom
                UpdateTargetRotationFromZoom();
            }
            else
            {
                Debug.LogError("[CinemachineZoomRotation] No virtual camera found!");
            }
        }

        public void UpdateComponent()
        {
            if (!IsEnabled || cameraContext == null || panTiltComponent == null) return;

            // Update target rotation if settings have zoom rotation enabled
            if (cameraContext.Settings.EnableZoomRotation)
            {
                UpdateTargetRotationFromZoom();
                ApplyCinemachineRotation();
            }
        }

        #endregion

        #region Rotation Logic

        private void UpdateTargetRotationFromZoom()
        {
            if (cameraContext.Settings.EnableZoomRotation)
            {
                // Get target angles from settings based on zoom level
                float targetPitchAngle = cameraContext.Settings.GetPitchAngle(cameraContext.ZoomLevel);
                float targetYawAngle = cameraContext.Settings.GetYawAngle(cameraContext.ZoomLevel);

                // Convert to Cinemachine axis values
                // For tactical cameras, we typically want:
                // - Tilt (pitch): negative values look down, positive look up
                // - Pan (yaw): direct mapping
                targetTiltValue = -targetPitchAngle; // Negative because Cinemachine tilt is inverted
                targetPanValue = targetYawAngle;

                Debug.Log($"[CinemachineZoomRotation] Zoom: {cameraContext.ZoomLevel:F2}, Target Tilt: {targetTiltValue:F1}°, Target Pan: {targetPanValue:F1}°");
            }
        }

        private void ApplyCinemachineRotation()
        {
            if (panTiltComponent == null) return;

            // Get current axis values
            float currentTilt = panTiltComponent.TiltAxis.Value;
            float currentPan = panTiltComponent.PanAxis.Value;

            // Smooth interpolation to target values
            float smoothedTilt = Mathf.SmoothDampAngle(
                currentTilt,
                targetTiltValue,
                ref currentTiltVelocity,
                cameraContext.Settings.PitchSmoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );

            float smoothedPan = currentPan;
            if (cameraContext.Settings.EnableYawRotation)
            {
                smoothedPan = Mathf.SmoothDampAngle(
                    currentPan,
                    targetPanValue,
                    ref currentPanVelocity,
                    cameraContext.Settings.YawSmoothTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime
                );
            }

            // Apply through Cinemachine axis system
            panTiltComponent.TiltAxis.Value = smoothedTilt;
            if (cameraContext.Settings.EnableYawRotation)
            {
                panTiltComponent.PanAxis.Value = smoothedPan;
            }

            // Debug logging
            bool tiltChanged = Mathf.Abs(smoothedTilt - currentTilt) > 0.1f;
            bool panChanged = cameraContext.Settings.EnableYawRotation && Mathf.Abs(smoothedPan - currentPan) > 0.1f;

            if (tiltChanged || panChanged)
            {
                Debug.Log($"[CinemachineZoomRotation] Applied - Tilt: {smoothedTilt:F1}° (target: {targetTiltValue:F1}°), Pan: {smoothedPan:F1}° (target: {targetPanValue:F1}°)");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set rotation directly through Cinemachine axes
        /// </summary>
        public void SetRotation(float pitch, float yaw, bool smooth = true)
        {
            if (panTiltComponent == null) return;

            float tiltValue = -pitch; // Convert pitch to tilt (inverted)
            float panValue = yaw;

            if (smooth)
            {
                targetTiltValue = tiltValue;
                targetPanValue = panValue;
            }
            else
            {
                panTiltComponent.TiltAxis.Value = tiltValue;
                panTiltComponent.PanAxis.Value = panValue;
                targetTiltValue = tiltValue;
                targetPanValue = panValue;
            }
        }

        /// <summary>
        /// Get current rotation values
        /// </summary>
        public Vector2 GetCurrentRotation()
        {
            if (panTiltComponent == null) return Vector2.zero;
            
            return new Vector2(
                -panTiltComponent.TiltAxis.Value, // Convert tilt back to pitch
                panTiltComponent.PanAxis.Value
            );
        }

        /// <summary>
        /// Configure the PanTilt component for optimal tactical camera behavior
        /// </summary>
        [ContextMenu("Configure PanTilt for Tactical Camera")]
        public void ConfigurePanTiltForTacticalCamera()
        {
            if (panTiltComponent == null) return;

            // Configure tilt axis (pitch)
            panTiltComponent.TiltAxis.Range = new Vector2(-90f, 90f); // Full range
            panTiltComponent.TiltAxis.Wrap = false; // No wrapping for pitch
            panTiltComponent.TiltAxis.Value = -60f; // Start at reasonable tactical angle

            // Configure pan axis (yaw) 
            if (cameraContext.Settings.EnableYawRotation)
            {
                panTiltComponent.PanAxis.Range = new Vector2(-180f, 180f); // Full rotation
                panTiltComponent.PanAxis.Wrap = true; // Allow wrapping for yaw
                panTiltComponent.PanAxis.Value = 0f; // Start facing north
            }
            else
            {
                panTiltComponent.PanAxis.Range = new Vector2(0f, 0f); // Lock yaw
                panTiltComponent.PanAxis.Value = 0f;
            }

            Debug.Log("[CinemachineZoomRotation] Configured PanTilt for tactical camera use");
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Auto-configure PanTilt if it exists
            if (panTiltComponent != null)
            {
                ConfigurePanTiltForTacticalCamera();
            }
        }

        #endregion
    }
}

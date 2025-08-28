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

        #region ICameraComponent Implementation

        public void Initialize(ICameraContext context)
        {
            cameraContext = context;
            
            // Get Cinemachine position composer
            if (cameraContext.VirtualCamera != null)
            {
                positionComposer = cameraContext.VirtualCamera.GetComponent<CinemachinePositionComposer>();
                if (positionComposer != null)
                {
                    targetZoomDistance = positionComposer.CameraDistance;
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
                cameraContext.IsInputActive = true;
            }
        }

        private void ApplyZoom()
        {
            if (positionComposer == null) return;

            // Smooth zoom interpolation
            positionComposer.CameraDistance = Mathf.SmoothDamp(
                positionComposer.CameraDistance,
                targetZoomDistance,
                ref currentZoomVelocity,
                cameraContext.Settings.ZoomSmoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );
        }

        private float GetZoomInput()
        {
            if (zoomAction?.action != null && zoomAction.action.enabled)
            {
                return zoomAction.action.ReadValue<float>();
            }

            // Fallback to traditional input system
            return Input.GetAxis("Mouse ScrollWheel");
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
    }
}

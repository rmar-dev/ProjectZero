using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Handles edge scrolling movement when mouse cursor approaches screen edges.
    /// Part of the compositional camera system.
    /// </summary>
    public class CameraEdgeScrolling : MonoBehaviour, ICameraComponent
    {
        // Component state
        public bool IsEnabled { get; set; } = true;
        public int UpdatePriority => 80; // Lower priority than keyboard input

        // Dependencies
        private ICameraContext cameraContext;
        private UnityEngine.Camera mainCamera;
        private Vector3 edgeScrollVelocity;

        #region ICameraComponent Implementation

        public void Initialize(ICameraContext context)
        {
            cameraContext = context;
            
            // Get the main camera for screen space calculations
            mainCamera = FindMainCamera();
            
            if (mainCamera == null)
            {
                Debug.LogWarning($"[CameraEdgeScrolling] Could not find main camera for edge scrolling on {gameObject.name}");
            }
        }

        public void UpdateComponent()
        {
            if (!IsEnabled || cameraContext == null || !cameraContext.Settings.EnableEdgeScrolling) 
                return;

            HandleEdgeScrolling();
            ApplyEdgeScrolling();
        }

        #endregion

        #region Edge Scrolling Logic

        private void HandleEdgeScrolling()
        {
            // Skip during tactical pause unless allowed
            if (Time.timeScale == 0f && !cameraContext.Settings.AllowInputDuringPause)
            {
                edgeScrollVelocity = Vector3.zero;
                return;
            }

            // Skip if keyboard input is active (prioritize keyboard over edge scrolling)
            if (cameraContext.IsInputActive)
            {
                edgeScrollVelocity = Vector3.zero;
                return;
            }

            Vector2 mousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            float borderSize = cameraContext.Settings.EdgeScrollBorder;

            Vector2 edgeMovement = Vector2.zero;

            // Check each screen edge
            if (mousePosition.x <= borderSize)
            {
                float intensity = (borderSize - mousePosition.x) / borderSize;
                edgeMovement.x = -intensity;
            }
            else if (mousePosition.x >= screenSize.x - borderSize)
            {
                float intensity = (mousePosition.x - (screenSize.x - borderSize)) / borderSize;
                edgeMovement.x = intensity;
            }

            if (mousePosition.y <= borderSize)
            {
                float intensity = (borderSize - mousePosition.y) / borderSize;
                edgeMovement.y = -intensity;
            }
            else if (mousePosition.y >= screenSize.y - borderSize)
            {
                float intensity = (mousePosition.y - (screenSize.y - borderSize)) / borderSize;
                edgeMovement.y = intensity;
            }

            // Apply edge scroll multiplier and smoothing
            float effectiveSpeed = cameraContext.Settings.MoveSpeed * cameraContext.Settings.EdgeScrollSpeedMultiplier;
            Vector3 worldMovement = new Vector3(edgeMovement.x, 0f, edgeMovement.y) * effectiveSpeed;
            
            edgeScrollVelocity = Vector3.Lerp(
                edgeScrollVelocity,
                worldMovement,
                Time.unscaledDeltaTime / cameraContext.Settings.InputResponseTime
            );

            // Set input active flag if edge scrolling is happening
            if (edgeScrollVelocity.magnitude > 0.01f)
            {
                cameraContext.IsInputActive = true;
            }
        }

        private void ApplyEdgeScrolling()
        {
            if (edgeScrollVelocity.magnitude > 0.01f)
            {
                Vector3 newPosition = cameraContext.CameraTarget.position + edgeScrollVelocity * Time.unscaledDeltaTime;
                Vector3 clampedPosition = cameraContext.ClampToBounds(newPosition);
                cameraContext.CameraTarget.position = clampedPosition;
            }
        }

        private UnityEngine.Camera FindMainCamera()
        {
            // Fallback to main camera - Cinemachine 3.x handles this automatically
            return UnityEngine.Camera.main ?? FindFirstObjectByType<UnityEngine.Camera>();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Check if mouse is currently in edge scroll zone
        /// </summary>
        public bool IsMouseInEdgeZone()
        {
            Vector2 mousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            float borderSize = cameraContext?.Settings.EdgeScrollBorder ?? 20f;

            return mousePosition.x <= borderSize ||
                   mousePosition.x >= screenSize.x - borderSize ||
                   mousePosition.y <= borderSize ||
                   mousePosition.y >= screenSize.y - borderSize;
        }

        /// <summary>
        /// Get current edge scroll velocity for debugging
        /// </summary>
        public Vector3 GetEdgeScrollVelocity() => edgeScrollVelocity;

        #endregion

        #region Debug

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (cameraContext?.CameraTarget == null) return;

            // Draw edge scroll velocity
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(cameraContext.CameraTarget.position, edgeScrollVelocity.normalized * 3f);
        }

        private void OnGUI()
        {
            if (!IsEnabled || cameraContext == null || cameraContext.Settings == null || !cameraContext.Settings.EnableEdgeScrolling) return;

            // Draw edge scroll borders in scene view
            float borderSize = cameraContext.Settings.EdgeScrollBorder;
            
            // Draw border rectangles
            GUI.color = new Color(1f, 0f, 1f, 0.1f); // Semi-transparent magenta
            
            // Left border
            GUI.DrawTexture(new Rect(0, 0, borderSize, Screen.height), Texture2D.whiteTexture);
            
            // Right border  
            GUI.DrawTexture(new Rect(Screen.width - borderSize, 0, borderSize, Screen.height), Texture2D.whiteTexture);
            
            // Top border
            GUI.DrawTexture(new Rect(0, Screen.height - borderSize, Screen.width, borderSize), Texture2D.whiteTexture);
            
            // Bottom border
            GUI.DrawTexture(new Rect(0, 0, Screen.width, borderSize), Texture2D.whiteTexture);
            
            GUI.color = Color.white;
        }
#endif

        #endregion
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Handles WASD keyboard movement for the tactical camera.
    /// Part of the compositional camera system.
    /// </summary>
    public class CameraKeyboardMovement : MonoBehaviour, ICameraComponent
    {
        [Header("Input Configuration")]
        [SerializeField] private InputActionReference movementAction;

        // Component state
        public bool IsEnabled { get; set; } = true;
        public int UpdatePriority => 100; // High priority for responsive input

        // Dependencies
        private ICameraContext cameraContext;
        private Vector3 targetVelocity;
        private Vector3 currentVelocity;

        #region ICameraComponent Implementation

        public void Initialize(ICameraContext context)
        {
            cameraContext = context;
            
            // Enable input action
            if (movementAction != null)
            {
                movementAction.action.Enable();
            }
            else
            {
                Debug.LogWarning($"[CameraKeyboardMovement] No movement input action assigned on {gameObject.name}");
            }
        }

        public void UpdateComponent()
        {
            if (!IsEnabled || cameraContext == null) return;

            HandleKeyboardMovement();
            ApplyMovement();
        }

        #endregion

        #region Movement Logic

        private void HandleKeyboardMovement()
        {
            // Skip input during tactical pause unless allowed
            if (Time.timeScale == 0f && !cameraContext.Settings.AllowInputDuringPause)
            {
                targetVelocity = Vector3.zero;
                return;
            }

            Vector2 inputVector = GetMovementInput();
            
            // Convert 2D input to 3D movement (camera-relative or world-space)
            Vector3 worldMovement = cameraContext.Settings.UseCameraRelativeMovement 
                ? GetCameraRelativeMovement(inputVector)
                : new Vector3(inputVector.x, 0f, inputVector.y);
            
            // Apply speed and time scaling
            float effectiveSpeed = cameraContext.Settings.MoveSpeed;
            
            // Apply acceleration if enabled
            if (cameraContext.Settings.UseAcceleration && inputVector.magnitude > 0.1f)
            {
                effectiveSpeed *= cameraContext.Settings.AccelerationMultiplier;
            }

            targetVelocity = worldMovement * effectiveSpeed;
            cameraContext.IsInputActive = inputVector.magnitude > 0.1f;
        }

        private void ApplyMovement()
        {
            // Smooth movement interpolation
            if (cameraContext.Settings.UseAcceleration)
            {
                Vector3 tempVelocity = cameraContext.CurrentVelocity;
                currentVelocity = Vector3.SmoothDamp(
                    currentVelocity, 
                    targetVelocity, 
                    ref tempVelocity, 
                    cameraContext.Settings.MovementSmoothTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime
                );
                cameraContext.CurrentVelocity = tempVelocity;
            }
            else
            {
                currentVelocity = targetVelocity;
            }

            // Apply movement to camera target
            if (currentVelocity.magnitude > 0.01f)
            {
                Vector3 newPosition = cameraContext.CameraTarget.position + currentVelocity * Time.unscaledDeltaTime;
                Vector3 clampedPosition = cameraContext.ClampToBounds(newPosition);
                cameraContext.CameraTarget.position = clampedPosition;
            }
        }

        private Vector2 GetMovementInput()
        {
            if (movementAction?.action != null && movementAction.action.enabled)
            {
                return movementAction.action.ReadValue<Vector2>();
            }

            // No fallback - require Input Action to be configured
            if (movementAction == null)
            {
                Debug.LogWarning($"[CameraKeyboardMovement] Movement Action not assigned on {gameObject.name}. WASD will not work.");
            }
            
            return Vector2.zero;
        }

        /// <summary>
        /// Converts 2D input (WASD) to 3D movement relative to camera rotation
        /// </summary>
        private Vector3 GetCameraRelativeMovement(Vector2 input)
        {
            if (input.magnitude < 0.01f)
                return Vector3.zero;

            // Get camera's forward and right vectors (but keep movement on XZ plane)
            if (cameraContext?.VirtualCamera == null)
            {
                // Fallback to world space if no camera available
                return new Vector3(input.x, 0f, input.y);
            }

            // Get camera transform
            Transform cameraTransform = cameraContext.VirtualCamera.transform;
            
            // Get camera's forward and right directions, projected onto XZ plane
            Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            
            // Handle case where camera is looking straight down
            if (cameraForward.magnitude < 0.1f)
            {
                // Use camera's rotation around Y axis only
                float yRotation = cameraTransform.eulerAngles.y * Mathf.Deg2Rad;
                cameraForward = new Vector3(Mathf.Sin(yRotation), 0f, Mathf.Cos(yRotation));
                cameraRight = new Vector3(Mathf.Cos(yRotation), 0f, -Mathf.Sin(yRotation));
            }
            
            // Combine input with camera-relative directions
            Vector3 movement = (cameraRight * input.x) + (cameraForward * input.y);
            
            // Ensure movement is normalized to prevent diagonal movement being faster
            return movement.normalized * input.magnitude;
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            movementAction?.action?.Enable();
        }

        private void OnDisable()
        {
            movementAction?.action?.Disable();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set the input action reference for movement
        /// </summary>
        public void SetMovementAction(InputActionReference actionReference)
        {
            if (movementAction != null)
            {
                movementAction.action.Disable();
            }

            movementAction = actionReference;

            if (movementAction != null && enabled)
            {
                movementAction.action.Enable();
            }
        }

        /// <summary>
        /// Get current movement velocity for debugging
        /// </summary>
        public Vector3 GetCurrentVelocity() => currentVelocity;

        /// <summary>
        /// Get target velocity for debugging
        /// </summary>
        public Vector3 GetTargetVelocity() => targetVelocity;

        #endregion

        #region Debug

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (cameraContext?.CameraTarget == null) return;

            // Draw movement vector
            Gizmos.color = Color.green;
            Gizmos.DrawRay(cameraContext.CameraTarget.position, currentVelocity.normalized * 2f);
            
            // Draw target velocity
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(cameraContext.CameraTarget.position + Vector3.up * 0.5f, targetVelocity.normalized * 2f);
        }
#endif

        #endregion
    }
}

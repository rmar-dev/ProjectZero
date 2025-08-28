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
            
            // Convert 2D input to 3D world movement
            Vector3 worldMovement = new Vector3(inputVector.x, 0f, inputVector.y);
            
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

            // Fallback to traditional input system
            return new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
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

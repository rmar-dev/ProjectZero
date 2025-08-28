using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Centralized input handler for camera controls.
    /// Integrates with Unity Input System and provides events for camera components.
    /// </summary>
    public class CameraInputHandler : BaseCameraComponent
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference movementAction;
        [SerializeField] private InputActionReference zoomAction;
        [SerializeField] private InputActionReference resetCameraAction;
        [SerializeField] private InputActionReference focusOnSquadAction;

        [Header("Input Events")]
        public UnityEvent<Vector2> OnMovementInput;
        public UnityEvent<float> OnZoomInput;
        public UnityEvent OnResetCamera;
        public UnityEvent OnFocusOnSquad;

        // Component properties
        public override int UpdatePriority => 100; // Highest priority for input processing

        // Input state tracking
        private Vector2 currentMovementInput;
        private float currentZoomInput;
        private bool inputActionsEnabled;

        #region Initialization

        protected override void OnInitialize()
        {
            EnableInputActions();
            SetupInputCallbacks();
        }

        private void EnableInputActions()
        {
            inputActionsEnabled = false;

            // Enable movement action
            if (movementAction?.action != null)
            {
                movementAction.action.Enable();
                inputActionsEnabled = true;
            }
            else
            {
                LogWarning("Movement action not assigned");
            }

            // Enable zoom action
            if (zoomAction?.action != null)
            {
                zoomAction.action.Enable();
            }
            else
            {
                LogWarning("Zoom action not assigned");
            }

            // Enable reset camera action
            if (resetCameraAction?.action != null)
            {
                resetCameraAction.action.Enable();
                resetCameraAction.action.performed += OnResetCameraPerformed;
            }

            // Enable focus on squad action
            if (focusOnSquadAction?.action != null)
            {
                focusOnSquadAction.action.Enable();
                focusOnSquadAction.action.performed += OnFocusOnSquadPerformed;
            }

            LogDebug($"Input actions enabled: {inputActionsEnabled}");
        }

        private void SetupInputCallbacks()
        {
            // Set up input action callbacks
            if (resetCameraAction?.action != null)
            {
                resetCameraAction.action.performed += OnResetCameraPerformed;
            }

            if (focusOnSquadAction?.action != null)
            {
                focusOnSquadAction.action.performed += OnFocusOnSquadPerformed;
            }
        }

        #endregion

        #region Update Logic

        protected override void OnUpdateComponent()
        {
            if (!ShouldProcessInput()) return;

            ProcessMovementInput();
            ProcessZoomInput();
        }

        private void ProcessMovementInput()
        {
            Vector2 newMovementInput = GetMovementInput();
            
            // Only broadcast if input changed
            if (Vector2.Distance(newMovementInput, currentMovementInput) > 0.01f)
            {
                currentMovementInput = newMovementInput;
                OnMovementInput?.Invoke(currentMovementInput);
                
                // Set input active state
                CameraContext.IsInputActive = newMovementInput.magnitude > 0.1f;
            }
        }

        private void ProcessZoomInput()
        {
            float newZoomInput = GetZoomInput();
            
            // Only broadcast if input changed
            if (Mathf.Abs(newZoomInput - currentZoomInput) > 0.01f)
            {
                currentZoomInput = newZoomInput;
                OnZoomInput?.Invoke(currentZoomInput);
                
                // Set input active for zoom
                if (Mathf.Abs(newZoomInput) > 0.01f)
                {
                    CameraContext.IsInputActive = true;
                }
            }
        }

        #endregion

        #region Input Reading

        private Vector2 GetMovementInput()
        {
            if (movementAction?.action != null && movementAction.action.enabled)
            {
                return movementAction.action.ReadValue<Vector2>();
            }

            // No fallback - require Input Action to be configured
            return Vector2.zero;
        }

        private float GetZoomInput()
        {
            if (zoomAction?.action != null && zoomAction.action.enabled)
            {
                return zoomAction.action.ReadValue<float>();
            }

            // No fallback - require Input Action to be configured
            return 0f;
        }

        #endregion

        #region Input Action Callbacks

        private void OnResetCameraPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnResetCamera?.Invoke();
                LogDebug("Reset camera requested");
            }
        }

        private void OnFocusOnSquadPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnFocusOnSquad?.Invoke();
                LogDebug("Focus on squad requested");
            }
        }

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            // Input actions will be enabled in Initialize()
        }

        private void OnEnable()
        {
            if (inputActionsEnabled)
            {
                EnableInputActions();
            }
        }

        private void OnDisable()
        {
            DisableInputActions();
        }

        private void OnDestroy()
        {
            DisableInputActions();
        }

        private void DisableInputActions()
        {
            // Disable actions and remove callbacks
            if (movementAction?.action != null)
            {
                movementAction.action.Disable();
            }

            if (zoomAction?.action != null)
            {
                zoomAction.action.Disable();
            }

            if (resetCameraAction?.action != null)
            {
                resetCameraAction.action.performed -= OnResetCameraPerformed;
                resetCameraAction.action.Disable();
            }

            if (focusOnSquadAction?.action != null)
            {
                focusOnSquadAction.action.performed -= OnFocusOnSquadPerformed;
                focusOnSquadAction.action.Disable();
            }

            inputActionsEnabled = false;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set input action references programmatically
        /// </summary>
        public void SetInputActions(
            InputActionReference movement = null,
            InputActionReference zoom = null,
            InputActionReference reset = null,
            InputActionReference focus = null)
        {
            DisableInputActions();

            if (movement != null) movementAction = movement;
            if (zoom != null) zoomAction = zoom;
            if (reset != null) resetCameraAction = reset;
            if (focus != null) focusOnSquadAction = focus;

            if (IsInitialized)
            {
                EnableInputActions();
            }
        }

        /// <summary>
        /// Get current movement input value
        /// </summary>
        public Vector2 GetCurrentMovementInput() => currentMovementInput;

        /// <summary>
        /// Get current zoom input value
        /// </summary>
        public float GetCurrentZoomInput() => currentZoomInput;

        /// <summary>
        /// Check if any input is currently active
        /// </summary>
        public bool HasActiveInput() => 
            currentMovementInput.magnitude > 0.1f || Mathf.Abs(currentZoomInput) > 0.01f;

        #endregion

        #region Debug

        protected override void OnDrawDebugGizmos()
        {
            if (CameraContext?.CameraTarget == null) return;

            Vector3 basePos = CameraContext.CameraTarget.position;

            // Draw movement input vector
            if (currentMovementInput.magnitude > 0.1f)
            {
                Gizmos.color = Color.green;
                Vector3 inputVector = new Vector3(currentMovementInput.x, 0f, currentMovementInput.y);
                Gizmos.DrawRay(basePos, inputVector * 3f);
            }

            // Draw zoom input indicator
            if (Mathf.Abs(currentZoomInput) > 0.01f)
            {
                Gizmos.color = currentZoomInput > 0 ? Color.blue : Color.red;
                Gizmos.DrawWireCube(basePos + Vector3.up * 3f, Vector3.one * 0.5f);
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!debugMode || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 120));
            GUILayout.BeginVertical("box");
            GUILayout.Label($"Camera Input Debug", GUI.skin.label);
            GUILayout.Label($"Movement: {currentMovementInput}");
            GUILayout.Label($"Zoom: {currentZoomInput:F3}");
            GUILayout.Label($"Input Active: {CameraContext?.IsInputActive}");
            GUILayout.Label($"Actions Enabled: {inputActionsEnabled}");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
#endif

        #endregion
    }
}

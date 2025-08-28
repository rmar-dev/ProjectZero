using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Handles input processing and distribution to camera components.
    /// Single responsibility: Process input events and notify appropriate components.
    /// </summary>
    [System.Serializable]
    public class CameraInputHandler : ICameraComponent
    {
        [Header("Input Settings")]
        [SerializeField] private bool enableInput = true;
        [SerializeField] private float inputDeadzone = 0.1f;
        
        // Input action references (will be set by the main controller)
        private InputAction moveAction;
        private InputAction zoomAction;
        
        // Component references for input distribution
        private CameraMovement movementComponent;
        private CameraZoom zoomComponent;
        
        private Transform cameraTransform;
        private bool isActive = true;
        
        // Current input states
        private Vector2 currentMoveInput;
        private float currentZoomInput;
        
        public bool IsActive 
        { 
            get => isActive; 
            set => isActive = value; 
        }
        
        public void Initialize(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
        }
        
        public void UpdateComponent()
        {
            if (!IsActive || !enableInput) return;
            
            ProcessInputs();
            DistributeInputs();
        }
        
        public void Cleanup()
        {
            DisableInputActions();
            cameraTransform = null;
            movementComponent = null;
            zoomComponent = null;
        }
        
        /// <summary>
        /// Set up input actions from the Unity Input System
        /// </summary>
        public void SetupInputActions(InputAction moveAction, InputAction zoomAction)
        {
            this.moveAction = moveAction;
            this.zoomAction = zoomAction;
            
            EnableInputActions();
        }
        
        /// <summary>
        /// Connect camera components for input distribution
        /// </summary>
        public void ConnectComponents(CameraMovement movement, CameraZoom zoom)
        {
            movementComponent = movement;
            zoomComponent = zoom;
        }
        
        /// <summary>
        /// Process current input values from Input System
        /// </summary>
        private void ProcessInputs()
        {
            // Process movement input
            if (moveAction != null && moveAction.enabled)
            {
                Vector2 rawInput = moveAction.ReadValue<Vector2>();
                currentMoveInput = ApplyDeadzone(rawInput);
            }
            else
            {
                currentMoveInput = Vector2.zero;
            }
            
            // Process zoom input
            if (zoomAction != null && zoomAction.enabled)
            {
                currentZoomInput = zoomAction.ReadValue<float>();
            }
            else
            {
                currentZoomInput = 0f;
            }
        }
        
        /// <summary>
        /// Distribute processed input to appropriate components
        /// </summary>
        private void DistributeInputs()
        {
            // Send movement input to movement component
            if (movementComponent != null && movementComponent.IsActive)
            {
                movementComponent.SetMovementInput(currentMoveInput);
            }
            
            // Send zoom input to zoom component
            if (zoomComponent != null && zoomComponent.IsActive && Mathf.Abs(currentZoomInput) > 0.01f)
            {
                zoomComponent.SetZoomInput(currentZoomInput);
            }
        }
        
        /// <summary>
        /// Apply deadzone to input vector
        /// </summary>
        private Vector2 ApplyDeadzone(Vector2 input)
        {
            if (input.magnitude < inputDeadzone)
            {
                return Vector2.zero;
            }
            
            // Normalize and re-map to account for deadzone
            Vector2 normalizedInput = input.normalized;
            float remappedMagnitude = Mathf.InverseLerp(inputDeadzone, 1f, input.magnitude);
            
            return normalizedInput * remappedMagnitude;
        }
        
        /// <summary>
        /// Enable input actions
        /// </summary>
        private void EnableInputActions()
        {
            moveAction?.Enable();
            zoomAction?.Enable();
        }
        
        /// <summary>
        /// Disable input actions
        /// </summary>
        private void DisableInputActions()
        {
            moveAction?.Disable();
            zoomAction?.Disable();
        }
        
        /// <summary>
        /// Configure input settings at runtime
        /// </summary>
        public void ConfigureInput(bool enableInput, float deadzone = 0.1f)
        {
            this.enableInput = enableInput;
            this.inputDeadzone = Mathf.Clamp01(deadzone);
        }
        
        /// <summary>
        /// Get current input states for debugging
        /// </summary>
        public (Vector2 movement, float zoom) GetCurrentInputs()
        {
            return (currentMoveInput, currentZoomInput);
        }
        
        /// <summary>
        /// Check if any input is currently active
        /// </summary>
        public bool HasActiveInput()
        {
            return currentMoveInput.magnitude > 0.01f || Mathf.Abs(currentZoomInput) > 0.01f;
        }
        
        /// <summary>
        /// Enable or disable input processing
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            enableInput = enabled;
            
            if (!enabled)
            {
                // Clear current inputs when disabled
                currentMoveInput = Vector2.zero;
                currentZoomInput = 0f;
                DistributeInputs(); // Send zero inputs to components
            }
        }
    }
}

using UnityEngine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Handles camera movement in world space.
    /// Single responsibility: Transform position based on movement input.
    /// </summary>
    [System.Serializable]
    public class CameraMovement : ICameraComponent
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private bool enableSmoothing = true;
        
        private Transform cameraTransform;
        private Vector2 currentInput = Vector2.zero;
        private Vector2 targetInput = Vector2.zero;
        private Vector2 inputVelocity = Vector2.zero;
        private bool isActive = true;
        
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
            if (!IsActive || cameraTransform == null) return;
            
            ProcessMovement();
        }
        
        public void Cleanup()
        {
            cameraTransform = null;
        }
        
        /// <summary>
        /// Set movement input direction (normalized Vector2)
        /// </summary>
        public void SetMovementInput(Vector2 input)
        {
            targetInput = input.normalized;
        }
        
        /// <summary>
        /// Apply movement to camera based on current input
        /// </summary>
        private void ProcessMovement()
        {
            // Smooth input interpolation if enabled
            if (enableSmoothing)
            {
                currentInput = Vector2.SmoothDamp(
                    currentInput, 
                    targetInput, 
                    ref inputVelocity, 
                    smoothTime
                );
            }
            else
            {
                currentInput = targetInput;
            }
            
            // Skip if no significant input
            if (currentInput.magnitude < 0.01f) return;
            
            // Calculate movement delta
            Vector3 movement = new Vector3(
                currentInput.x,
                0f,
                currentInput.y
            ) * moveSpeed * Time.unscaledDeltaTime;
            
            // Apply movement to camera transform
            cameraTransform.position += movement;
        }
        
        /// <summary>
        /// Configure movement settings at runtime
        /// </summary>
        public void ConfigureMovement(float speed, float smoothTime = 0.1f, bool enableSmoothing = true)
        {
            this.moveSpeed = speed;
            this.smoothTime = smoothTime;
            this.enableSmoothing = enableSmoothing;
        }
        
        /// <summary>
        /// Get current movement speed for debugging
        /// </summary>
        public float GetCurrentSpeed() => currentInput.magnitude * moveSpeed;
    }
}

using UnityEngine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Base class for camera components providing common functionality.
    /// Reduces boilerplate and ensures consistent behavior across components.
    /// </summary>
    public abstract class BaseCameraComponent : MonoBehaviour, ICameraComponent
    {
        [Header("Component Settings")]
        [SerializeField] protected bool enabledByDefault = true;
        [SerializeField] protected bool debugMode = false;

        // Interface implementation
        public virtual bool IsEnabled { get; set; } = true;
        public abstract int UpdatePriority { get; }

        // Protected members for derived classes
        protected ICameraContext CameraContext { get; private set; }
        protected bool IsInitialized { get; private set; }

        #region ICameraComponent Implementation

        public virtual void Initialize(ICameraContext context)
        {
            CameraContext = context;
            IsEnabled = enabledByDefault;
            IsInitialized = true;

            // Call derived class initialization
            OnInitialize();

            LogDebug($"Initialized {GetType().Name}");
        }

        public void UpdateComponent()
        {
            if (!IsEnabled || !IsInitialized || CameraContext == null) return;

            // Validate context before updating
            if (!ValidateContext())
            {
                LogWarning("Invalid camera context, skipping update");
                return;
            }

            OnUpdateComponent();
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called when the component is initialized. Override in derived classes.
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// Called every frame when the component is enabled. Override in derived classes.
        /// </summary>
        protected abstract void OnUpdateComponent();

        #endregion

        #region Protected Utility Methods

        /// <summary>
        /// Check if camera context is valid
        /// </summary>
        protected virtual bool ValidateContext()
        {
            return CameraContext?.CameraTarget != null && 
                   CameraContext?.Settings != null;
        }

        /// <summary>
        /// Check if input should be processed based on pause state
        /// </summary>
        protected bool ShouldProcessInput()
        {
            return Time.timeScale > 0f || CameraContext.Settings.AllowInputDuringPause;
        }

        /// <summary>
        /// Apply smooth movement with bounds checking
        /// </summary>
        protected void ApplyMovementWithBounds(Vector3 deltaMovement)
        {
            Vector3 newPosition = CameraContext.CameraTarget.position + deltaMovement;
            Vector3 clampedPosition = CameraContext.ClampToBounds(newPosition);
            CameraContext.CameraTarget.position = clampedPosition;
        }

        /// <summary>
        /// Log debug message if debug mode is enabled
        /// </summary>
        protected void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[{GetType().Name}] {message}");
            }
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{GetType().Name}] {message}");
        }

        /// <summary>
        /// Log error message
        /// </summary>
        protected void LogError(string message)
        {
            Debug.LogError($"[{GetType().Name}] {message}");
        }

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            // Initialize with default state
            IsEnabled = enabledByDefault;
        }

        protected virtual void OnValidate()
        {
            // Validate settings in editor
            if (Application.isPlaying && IsInitialized && !ValidateContext())
            {
                LogWarning("Camera context validation failed during OnValidate");
            }
        }

        #endregion

        #region Editor Debug

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (!debugMode || CameraContext?.CameraTarget == null) return;

            // Draw component identifier
            Gizmos.color = GetDebugColor();
            Gizmos.DrawWireSphere(CameraContext.CameraTarget.position + Vector3.up * 2f, 0.3f);
            
            // Draw custom gizmos from derived classes
            OnDrawDebugGizmos();
        }

        /// <summary>
        /// Override to draw custom debug gizmos
        /// </summary>
        protected virtual void OnDrawDebugGizmos() { }

        /// <summary>
        /// Get unique debug color for this component type
        /// </summary>
        protected virtual Color GetDebugColor()
        {
            // Generate consistent color based on type name
            int hash = GetType().Name.GetHashCode();
            Random.State oldState = Random.state;
            Random.InitState(hash);
            Color color = new Color(Random.value, Random.value, Random.value, 1f);
            Random.state = oldState;
            return color;
        }
#endif

        #endregion
    }
}

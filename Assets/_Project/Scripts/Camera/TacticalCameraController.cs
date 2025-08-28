using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Main tactical camera controller using compositional architecture.
    /// Manages camera components and provides shared context.
    /// Integrates with Cinemachine for professional camera behaviors.
    /// </summary>
    public class TacticalCameraController : MonoBehaviour, ICameraContext
    {
        [Header("Camera Configuration")]
        [SerializeField] private CameraSettings settings;
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private CinemachineCamera virtualCamera;

        [Header("Camera Components")]
        [SerializeField] private List<MonoBehaviour> cameraComponents = new List<MonoBehaviour>();

        [Header("Events")]
        public UnityEvent<Vector3> OnCameraPositionChanged;
        public UnityEvent<float> OnZoomChanged;
        public UnityEvent<bool> OnFollowModeChanged;

        // ICameraContext implementation
        public Transform CameraTarget => cameraTarget;
        public CinemachineCamera VirtualCamera => virtualCamera;
        public CameraSettings Settings => settings;
        public Vector3 CurrentVelocity { get; set; }
        public bool IsInputActive { get; set; }
        public Transform FollowTarget { get; set; }
        public float ZoomLevel { get; set; } = 0.5f;

        // Private fields
        private readonly List<ICameraComponent> activeCameraComponents = new List<ICameraComponent>();
        private Vector3 previousPosition;
        private float previousZoom;
        private CinemachinePositionComposer positionComposer;

        #region Unity Lifecycle

        private void Awake()
        {
            ValidateComponents();
            InitializeCinemachine();
            InitializeCameraComponents();
        }

        private void Start()
        {
            // Set initial position and zoom
            SetZoom(ZoomLevel);
            previousPosition = cameraTarget.position;
            previousZoom = ZoomLevel;
        }

        private void Update()
        {
            UpdateCameraComponents();
            CheckForChanges();
        }

        private void OnValidate()
        {
            ValidateComponents();
        }

        #endregion

        #region Initialization

        private void ValidateComponents()
        {
            if (settings == null)
            {
                Debug.LogError($"[TacticalCameraController] CameraSettings is not assigned on {gameObject.name}");
                return;
            }

            if (cameraTarget == null)
            {
                Debug.LogError($"[TacticalCameraController] Camera Target is not assigned on {gameObject.name}");
                return;
            }

            if (virtualCamera == null)
            {
                Debug.LogError($"[TacticalCameraController] Cinemachine Camera is not assigned on {gameObject.name}");
                return;
            }
        }

        private void InitializeCinemachine()
        {
            if (virtualCamera == null) return;

            // Set up Cinemachine to follow our camera target
            virtualCamera.Target.TrackingTarget = cameraTarget;
            // Note: LookAt not needed for top-down tactical view

            // Get or add position composer
            positionComposer = virtualCamera.GetComponent<CinemachinePositionComposer>();
            if (positionComposer == null)
            {
                positionComposer = virtualCamera.gameObject.AddComponent<CinemachinePositionComposer>();
            }

            // Configure for top-down tactical view
            positionComposer.TargetOffset = Vector3.zero;
            positionComposer.CameraDistance = settings.GetZoomDistance(ZoomLevel);
            
            // Set camera angle for top-down view (can be adjusted as needed)
            virtualCamera.transform.rotation = Quaternion.Euler(60f, 0f, 0f); // Tactical angle
        }

        private void InitializeCameraComponents()
        {
            activeCameraComponents.Clear();

            // Auto-discover camera components if none are manually assigned
            if (cameraComponents == null || cameraComponents.Count == 0)
            {
                // Find all ICameraComponent implementations on this GameObject
                var autoComponents = GetComponents<MonoBehaviour>()
                    .Where(c => c is ICameraComponent && c != this)
                    .ToList();
                
                cameraComponents.Clear();
                cameraComponents.AddRange(autoComponents);
                
                Debug.Log($"[TacticalCameraController] Auto-discovered {autoComponents.Count} camera components");
            }

            // Initialize discovered components
            foreach (var component in cameraComponents)
            {
                if (component is ICameraComponent cameraComponent)
                {
                    cameraComponent.Initialize(this);
                    activeCameraComponents.Add(cameraComponent);
                }
                else
                {
                    Debug.LogWarning($"[TacticalCameraController] Component {component.GetType().Name} does not implement ICameraComponent");
                }
            }

            // Sort by update priority
            activeCameraComponents.Sort((a, b) => b.UpdatePriority.CompareTo(a.UpdatePriority));

            Debug.Log($"[TacticalCameraController] Initialized {activeCameraComponents.Count} camera components");
        }

        #endregion

        #region Component Management

        private void UpdateCameraComponents()
        {
            foreach (var component in activeCameraComponents.Where(c => c.IsEnabled))
            {
                component.UpdateComponent();
            }
        }

        /// <summary>
        /// Add a camera component at runtime
        /// </summary>
        public void AddCameraComponent<T>() where T : MonoBehaviour, ICameraComponent
        {
            var component = gameObject.AddComponent<T>();
            component.Initialize(this);
            activeCameraComponents.Add(component);
            
            // Re-sort by priority
            activeCameraComponents.Sort((a, b) => b.UpdatePriority.CompareTo(a.UpdatePriority));
        }

        /// <summary>
        /// Remove a camera component at runtime
        /// </summary>
        public void RemoveCameraComponent<T>() where T : MonoBehaviour, ICameraComponent
        {
            var component = GetComponent<T>();
            if (component != null)
            {
                activeCameraComponents.Remove(component);
                Destroy(component);
            }
        }

        #endregion

        #region ICameraContext Implementation

        public bool IsWithinBounds(Vector3 worldPosition)
        {
            if (!settings.EnforceBounds) return true;

            var bounds = settings.MapBounds;
            return Mathf.Abs(worldPosition.x) <= bounds.x && Mathf.Abs(worldPosition.z) <= bounds.y;
        }

        public Vector3 ClampToBounds(Vector3 position)
        {
            if (!settings.EnforceBounds) return position;

            var bounds = settings.MapBounds;
            return new Vector3(
                Mathf.Clamp(position.x, -bounds.x, bounds.x),
                position.y,
                Mathf.Clamp(position.z, -bounds.y, bounds.y)
            );
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set camera zoom level (0.0 = max zoom out, 1.0 = max zoom in)
        /// </summary>
        public void SetZoom(float normalizedZoom)
        {
            ZoomLevel = Mathf.Clamp01(normalizedZoom);
            if (positionComposer != null)
            {
                positionComposer.CameraDistance = settings.GetZoomDistance(ZoomLevel);
            }
        }

        /// <summary>
        /// Set camera position directly
        /// </summary>
        public void SetPosition(Vector3 worldPosition)
        {
            var clampedPosition = ClampToBounds(worldPosition);
            cameraTarget.position = clampedPosition;
        }

        /// <summary>
        /// Set target to follow (null to stop following)
        /// </summary>
        public void SetFollowTarget(Transform target)
        {
            FollowTarget = target;
        }

        /// <summary>
        /// Enable or disable a specific camera component type
        /// </summary>
        public void SetComponentEnabled<T>(bool enabled) where T : ICameraComponent
        {
            var component = activeCameraComponents.OfType<T>().FirstOrDefault();
            if (component != null)
            {
                component.IsEnabled = enabled;
            }
        }

        /// <summary>
        /// Focus camera on a specific world position with smooth transition
        /// </summary>
        public void FocusOnPosition(Vector3 worldPosition, float duration = 1f)
        {
            StartCoroutine(FocusCoroutine(worldPosition, duration));
        }

        #endregion

        #region Private Methods

        private void CheckForChanges()
        {
            // Check for position changes
            if (Vector3.Distance(cameraTarget.position, previousPosition) > 0.01f)
            {
                OnCameraPositionChanged?.Invoke(cameraTarget.position);
                previousPosition = cameraTarget.position;
            }

            // Check for zoom changes
            if (Mathf.Abs(ZoomLevel - previousZoom) > 0.01f)
            {
                OnZoomChanged?.Invoke(ZoomLevel);
                previousZoom = ZoomLevel;
            }
        }

        private System.Collections.IEnumerator FocusCoroutine(Vector3 targetPosition, float duration)
        {
            var startPosition = cameraTarget.position;
            var clampedTarget = ClampToBounds(targetPosition);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                
                // Use smooth step for natural camera movement
                t = Mathf.SmoothStep(0f, 1f, t);
                
                cameraTarget.position = Vector3.Lerp(startPosition, clampedTarget, t);
                yield return null;
            }

            cameraTarget.position = clampedTarget;
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (settings == null) return;

            // Draw camera bounds
            var bounds = settings.MapBounds;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(bounds.x * 2, 0.1f, bounds.y * 2));

            // Draw follow dead zone if following a target
            if (FollowTarget != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(FollowTarget.position, settings.FollowDeadZone);
            }

            // Draw camera target position
            if (cameraTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(cameraTarget.position, 0.5f);
            }
        }
#endif

        #endregion
    }
}

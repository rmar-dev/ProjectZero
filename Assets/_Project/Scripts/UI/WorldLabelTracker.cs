using UnityEngine;

namespace ProjectZero.UI
{
    /// <summary>
    /// Tracks a world-space target and positions a UI label accordingly
    /// Handles scaling based on distance and screen bounds checking
    /// </summary>
    public class WorldLabelTracker : MonoBehaviour
    {
        [Header("Tracking Settings")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 worldOffset = Vector3.zero;
        [SerializeField] private UnityEngine.Camera trackingCamera;
        
        [Header("Distance Settings")]
        [SerializeField] private float maxDistance = 50f;
        [SerializeField] private bool scaleWithDistance = true;
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 1.5f;
        
        [Header("Bounds Settings")]
        [SerializeField] private bool hideBehindCamera = true;
        [SerializeField] private bool hideWhenTooFar = true;
        [SerializeField] private bool clampToScreenBounds = false;
        
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas parentCanvas;
        private Vector3 baseScale;
        private bool isInitialized = false;

        #region Initialization
        /// <summary>
        /// Initialize the world label tracker
        /// </summary>
        public void Initialize(Transform target, Vector3 offset, UnityEngine.Camera camera)
        {
            followTarget = target;
            worldOffset = offset;
            trackingCamera = camera;
            
            Initialize();
        }

        private void Initialize()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            if (trackingCamera == null)
            {
                trackingCamera = UnityEngine.Camera.main ?? FindFirstObjectByType<UnityEngine.Camera>();
            }
            
            // Find the parent canvas
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogWarning($"[WorldLabelTracker] No parent Canvas found for {gameObject.name}. World space tracking may not work correctly.");
            }
            
            baseScale = transform.localScale;
            isInitialized = true;
        }
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        private void LateUpdate()
        {
            if (!isInitialized || followTarget == null || trackingCamera == null)
                return;

            UpdatePosition();
            UpdateVisibility();
            UpdateScale();
        }
        #endregion

        #region Position and Visibility Updates
        private void UpdatePosition()
        {
            Vector3 worldPosition = followTarget.position + worldOffset;
            Vector3 screenPosition = trackingCamera.WorldToScreenPoint(worldPosition);

            if (clampToScreenBounds)
            {
                screenPosition = ClampToScreenBounds(screenPosition);
            }

            // Convert screen position to canvas position
            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace)
            {
                // For world space canvas, use world position directly
                transform.position = worldPosition;
            }
            else
            {
                // For screen space canvas, use screen position
                rectTransform.position = screenPosition;
            }
        }

        private void UpdateVisibility()
        {
            if (followTarget == null || trackingCamera == null)
            {
                SetVisibility(false);
                return;
            }

            Vector3 worldPosition = followTarget.position + worldOffset;
            Vector3 directionToTarget = worldPosition - trackingCamera.transform.position;
            float distance = directionToTarget.magnitude;

            bool shouldShow = true;

            // Check if behind camera
            if (hideBehindCamera)
            {
                Vector3 forward = trackingCamera.transform.forward;
                if (Vector3.Dot(directionToTarget.normalized, forward) < 0)
                {
                    shouldShow = false;
                }
            }

            // Check distance
            if (hideWhenTooFar && distance > maxDistance)
            {
                shouldShow = false;
            }

            // Check if on screen (for screen space canvases)
            if (shouldShow && parentCanvas != null && parentCanvas.renderMode != RenderMode.WorldSpace)
            {
                Vector3 screenPos = trackingCamera.WorldToScreenPoint(worldPosition);
                if (screenPos.z < 0 || screenPos.x < 0 || screenPos.x > Screen.width || 
                    screenPos.y < 0 || screenPos.y > Screen.height)
                {
                    shouldShow = clampToScreenBounds; // Only show if we're clamping to bounds
                }
            }

            SetVisibility(shouldShow);
        }

        private void UpdateScale()
        {
            if (!scaleWithDistance || followTarget == null || trackingCamera == null)
                return;

            float distance = Vector3.Distance(trackingCamera.transform.position, followTarget.position);
            float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
            
            // Inverse scale - closer objects are larger
            float scaleMultiplier = Mathf.Lerp(maxScale, minScale, normalizedDistance);
            transform.localScale = baseScale * scaleMultiplier;
        }

        private void SetVisibility(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        private Vector3 ClampToScreenBounds(Vector3 screenPosition)
        {
            float margin = 50f; // Pixels from screen edge
            
            screenPosition.x = Mathf.Clamp(screenPosition.x, margin, Screen.width - margin);
            screenPosition.y = Mathf.Clamp(screenPosition.y, margin, Screen.height - margin);
            
            return screenPosition;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Set a new follow target
        /// </summary>
        public void SetFollowTarget(Transform newTarget)
        {
            followTarget = newTarget;
        }

        /// <summary>
        /// Set the world offset from the target
        /// </summary>
        public void SetWorldOffset(Vector3 newOffset)
        {
            worldOffset = newOffset;
        }

        /// <summary>
        /// Set the tracking camera
        /// </summary>
        public void SetTrackingCamera(UnityEngine.Camera camera)
        {
            trackingCamera = camera;
        }

        /// <summary>
        /// Get the current distance to target
        /// </summary>
        public float GetDistanceToTarget()
        {
            if (followTarget == null || trackingCamera == null)
                return float.MaxValue;

            return Vector3.Distance(trackingCamera.transform.position, followTarget.position);
        }

        /// <summary>
        /// Check if the label is currently visible
        /// </summary>
        public bool IsVisible()
        {
            if (canvasGroup != null)
                return canvasGroup.alpha > 0f;
            
            return gameObject.activeSelf;
        }
        #endregion

        #region Debug
        private void OnDrawGizmosSelected()
        {
            if (followTarget == null)
                return;

            // Draw connection line
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, followTarget.position + worldOffset);
            
            // Draw offset position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(followTarget.position + worldOffset, 0.1f);
            
            // Draw max distance sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(followTarget.position, maxDistance);
        }
        #endregion
    }
}

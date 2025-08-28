using UnityEngine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Handles camera constraints including bounds enforcement and movement smoothing.
    /// Part of the compositional camera system architecture.
    /// </summary>
    public class CameraConstraints : BaseCameraComponent
    {
        [Header("Constraint Settings")]
        [SerializeField] private bool enforceXBounds = true;
        [SerializeField] private bool enforceZBounds = true;
        [SerializeField] private bool maintainYPosition = true;
        [SerializeField] private float fixedYPosition = 0f;

        [Header("Smoothing")]
        [SerializeField] private bool enablePositionSmoothing = true;
        [SerializeField] private AnimationCurve smoothingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // Component properties
        public override int UpdatePriority => 10; // Low priority - apply constraints last

        // Private state
        private Vector3 targetPosition;
        private Vector3 smoothVelocity;
        private bool hasTargetPosition;

        #region Initialization

        protected override void OnInitialize()
        {
            // Set initial target position to current position
            targetPosition = CameraContext.CameraTarget.position;
            hasTargetPosition = true;
            
            // Maintain Y position if required
            if (maintainYPosition)
            {
                targetPosition.y = fixedYPosition;
            }
        }

        #endregion

        #region Update Logic

        protected override void OnUpdateComponent()
        {
            ApplyConstraints();
            ApplySmoothing();
        }

        private void ApplyConstraints()
        {
            Vector3 currentPosition = CameraContext.CameraTarget.position;
            Vector3 constrainedPosition = currentPosition;

            // Apply Y position constraint
            if (maintainYPosition)
            {
                constrainedPosition.y = fixedYPosition;
            }

            // Apply bounds constraints
            if (CameraContext.Settings.EnforceBounds)
            {
                var bounds = CameraContext.Settings.MapBounds;

                if (enforceXBounds)
                {
                    constrainedPosition.x = Mathf.Clamp(constrainedPosition.x, -bounds.x, bounds.x);
                }

                if (enforceZBounds)
                {
                    constrainedPosition.z = Mathf.Clamp(constrainedPosition.z, -bounds.y, bounds.y);
                }
            }

            // Update target position for smoothing
            targetPosition = constrainedPosition;
        }

        private void ApplySmoothing()
        {
            if (!enablePositionSmoothing || !hasTargetPosition) return;

            Vector3 currentPosition = CameraContext.CameraTarget.position;
            
            // Calculate smoothing based on settings
            float smoothTime = CameraContext.Settings.MovementSmoothTime;
            
            // Apply curve-based smoothing if available
            if (smoothingCurve.length > 0)
            {
                float distance = Vector3.Distance(currentPosition, targetPosition);
                if (distance > 0.01f)
                {
                    float curveValue = smoothingCurve.Evaluate(Mathf.Clamp01(distance / 10f));
                    smoothTime *= curveValue;
                }
            }

            // Apply smooth movement
            Vector3 smoothedPosition = Vector3.SmoothDamp(
                currentPosition,
                targetPosition,
                ref smoothVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );

            CameraContext.CameraTarget.position = smoothedPosition;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set fixed Y position for camera
        /// </summary>
        public void SetFixedYPosition(float yPosition)
        {
            fixedYPosition = yPosition;
            if (maintainYPosition)
            {
                targetPosition.y = yPosition;
            }
        }

        /// <summary>
        /// Enable or disable Y position constraint
        /// </summary>
        public void SetMaintainYPosition(bool maintain)
        {
            maintainYPosition = maintain;
            if (maintain)
            {
                targetPosition.y = fixedYPosition;
            }
        }

        /// <summary>
        /// Set bounds enforcement for specific axes
        /// </summary>
        public void SetBoundsEnforcement(bool enforceX, bool enforceZ)
        {
            enforceXBounds = enforceX;
            enforceZBounds = enforceZ;
        }

        /// <summary>
        /// Set custom smoothing curve
        /// </summary>
        public void SetSmoothingCurve(AnimationCurve curve)
        {
            smoothingCurve = curve ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        /// <summary>
        /// Get current smoothing velocity for debugging
        /// </summary>
        public Vector3 GetSmoothVelocity() => smoothVelocity;

        /// <summary>
        /// Check if position is currently being smoothed
        /// </summary>
        public bool IsSmoothing() => smoothVelocity.magnitude > 0.01f;

        /// <summary>
        /// Snap to target position immediately (disable smoothing for this frame)
        /// </summary>
        public void SnapToTarget()
        {
            if (hasTargetPosition)
            {
                CameraContext.CameraTarget.position = targetPosition;
                smoothVelocity = Vector3.zero;
            }
        }

        #endregion

        #region Debug

        protected override void OnDrawDebugGizmos()
        {
            if (CameraContext?.CameraTarget == null) return;

            Vector3 currentPos = CameraContext.CameraTarget.position;

            // Draw constraint bounds
            if (CameraContext.Settings.EnforceBounds)
            {
                var bounds = CameraContext.Settings.MapBounds;
                
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange
                Vector3 boundsSize = new Vector3(
                    enforceXBounds ? bounds.x * 2 : 1000f,
                    1f,
                    enforceZBounds ? bounds.y * 2 : 1000f
                );
                Gizmos.DrawWireCube(Vector3.zero, boundsSize);
            }

            // Draw target position
            if (hasTargetPosition && Vector3.Distance(currentPos, targetPosition) > 0.1f)
            {
                Gizmos.color = Color.orange;
                Gizmos.DrawLine(currentPos, targetPosition);
                Gizmos.DrawWireSphere(targetPosition, 0.5f);
            }

            // Draw Y position constraint
            if (maintainYPosition)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(
                    new Vector3(currentPos.x - 2f, fixedYPosition, currentPos.z),
                    new Vector3(currentPos.x + 2f, fixedYPosition, currentPos.z)
                );
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!debugMode || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 140, 300, 100));
            GUILayout.BeginVertical("box");
            GUILayout.Label("Camera Constraints Debug", GUI.skin.label);
            GUILayout.Label($"Smoothing: {IsSmoothing()}");
            GUILayout.Label($"Smooth Velocity: {smoothVelocity.magnitude:F3}");
            GUILayout.Label($"Y Constraint: {(maintainYPosition ? fixedYPosition.ToString("F1") : "Disabled")}");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
#endif

        #endregion
    }
}

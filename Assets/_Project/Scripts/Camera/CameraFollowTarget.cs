using UnityEngine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Handles automatic following of target objects (like squad center).
    /// Respects dead zones and provides smooth following behavior.
    /// </summary>
    public class CameraFollowTarget : MonoBehaviour, ICameraComponent
    {
        // Component state
        public bool IsEnabled { get; set; } = true;
        public int UpdatePriority => 70; // Lower priority than direct input

        // Dependencies
        private ICameraContext cameraContext;
        private Vector3 followVelocity;

        #region ICameraComponent Implementation

        public void Initialize(ICameraContext context)
        {
            cameraContext = context;
        }

        public void UpdateComponent()
        {
            if (!IsEnabled || cameraContext == null || cameraContext.FollowTarget == null) 
                return;

            HandleFollowTarget();
        }

        #endregion

        #region Follow Logic

        private void HandleFollowTarget()
        {
            // Don't follow if user input is active (user control takes priority)
            if (cameraContext.IsInputActive)
            {
                return;
            }

            Transform target = cameraContext.FollowTarget;
            Vector3 currentPosition = cameraContext.CameraTarget.position;
            Vector3 targetPosition = target.position;

            // Calculate distance to target
            float distance = Vector3.Distance(
                new Vector3(currentPosition.x, 0f, currentPosition.z),
                new Vector3(targetPosition.x, 0f, targetPosition.z)
            );

            // Only follow if outside dead zone
            if (distance > cameraContext.Settings.FollowDeadZone)
            {
                // Calculate follow direction (only X and Z, maintain Y)
                Vector3 followDirection = (targetPosition - currentPosition);
                followDirection.y = 0f; // Keep camera height constant
                followDirection = followDirection.normalized;

                // Calculate how far outside the dead zone we are
                float excessDistance = distance - cameraContext.Settings.FollowDeadZone;
                
                // Follow with speed proportional to excess distance
                float followSpeed = cameraContext.Settings.FollowSpeed * excessDistance;
                Vector3 targetFollowPosition = currentPosition + followDirection * followSpeed * Time.unscaledDeltaTime;

                // Smooth follow movement
                Vector3 smoothedPosition = Vector3.SmoothDamp(
                    currentPosition,
                    targetFollowPosition,
                    ref followVelocity,
                    cameraContext.Settings.FollowSmoothTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime
                );

                // Apply bounds clamping
                Vector3 clampedPosition = cameraContext.ClampToBounds(smoothedPosition);
                cameraContext.CameraTarget.position = clampedPosition;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set follow target with optional smooth transition
        /// </summary>
        public void SetFollowTarget(Transform target, bool smoothTransition = true)
        {
            cameraContext.FollowTarget = target;
            
            if (!smoothTransition && target != null)
            {
                // Snap to target immediately (outside dead zone)
                Vector3 targetPosition = target.position;
                Vector3 snapPosition = cameraContext.ClampToBounds(targetPosition);
                cameraContext.CameraTarget.position = snapPosition;
            }
        }

        /// <summary>
        /// Check if currently following a target
        /// </summary>
        public bool IsFollowing => cameraContext?.FollowTarget != null && !cameraContext.IsInputActive;

        /// <summary>
        /// Check if target is within dead zone
        /// </summary>
        public bool IsTargetInDeadZone()
        {
            if (cameraContext?.FollowTarget == null) return false;

            Vector3 currentPos = cameraContext.CameraTarget.position;
            Vector3 targetPos = cameraContext.FollowTarget.position;
            
            float distance = Vector3.Distance(
                new Vector3(currentPos.x, 0f, currentPos.z),
                new Vector3(targetPos.x, 0f, targetPos.z)
            );

            return distance <= cameraContext.Settings.FollowDeadZone;
        }

        /// <summary>
        /// Get current follow velocity for debugging
        /// </summary>
        public Vector3 GetFollowVelocity() => followVelocity;

        /// <summary>
        /// Stop following current target
        /// </summary>
        public void StopFollowing()
        {
            cameraContext.FollowTarget = null;
            followVelocity = Vector3.zero;
        }

        /// <summary>
        /// Force follow target immediately (ignoring dead zone)
        /// </summary>
        public void SnapToTarget()
        {
            if (cameraContext?.FollowTarget != null)
            {
                Vector3 targetPosition = cameraContext.FollowTarget.position;
                Vector3 snapPosition = cameraContext.ClampToBounds(targetPosition);
                cameraContext.CameraTarget.position = snapPosition;
                followVelocity = Vector3.zero;
            }
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (cameraContext == null) return;

            // Draw follow target connection
            if (cameraContext.FollowTarget != null)
            {
                Vector3 currentPos = cameraContext.CameraTarget.position;
                Vector3 targetPos = cameraContext.FollowTarget.position;

                // Draw line to follow target
                Gizmos.color = IsTargetInDeadZone() ? Color.green : Color.yellow;
                Gizmos.DrawLine(currentPos, targetPos);

                // Draw dead zone
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(targetPos, cameraContext.Settings.FollowDeadZone);
            }

            // Draw follow velocity
            if (followVelocity.magnitude > 0.01f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(cameraContext.CameraTarget.position, followVelocity.normalized * 4f);
            }
        }
#endif

        #endregion
    }
}

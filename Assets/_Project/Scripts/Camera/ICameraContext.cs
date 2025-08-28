using UnityEngine;
using Unity.Cinemachine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Context interface providing shared data and services to camera components.
    /// Follows the composition pattern for clean separation of concerns.
    /// </summary>
    public interface ICameraContext
    {
        /// <summary>
        /// The main camera transform being controlled
        /// </summary>
        Transform CameraTarget { get; }
        
        /// <summary>
        /// Cinemachine camera for advanced camera features
        /// </summary>
        CinemachineCamera VirtualCamera { get; }
        
        /// <summary>
        /// Camera configuration settings
        /// </summary>
        CameraSettings Settings { get; }
        
        /// <summary>
        /// Current camera movement velocity (for smooth interpolation)
        /// </summary>
        Vector3 CurrentVelocity { get; set; }
        
        /// <summary>
        /// Whether the camera is currently being controlled by user input
        /// </summary>
        bool IsInputActive { get; set; }
        
        /// <summary>
        /// Target to follow (squad center, selected unit, etc.)
        /// </summary>
        Transform FollowTarget { get; set; }
        
        /// <summary>
        /// Current zoom level (0.0 = max zoom out, 1.0 = max zoom in)
        /// </summary>
        float ZoomLevel { get; set; }
        
        /// <summary>
        /// Check if a world position is within camera bounds
        /// </summary>
        bool IsWithinBounds(Vector3 worldPosition);
        
        /// <summary>
        /// Clamp position to camera bounds
        /// </summary>
        Vector3 ClampToBounds(Vector3 position);
    }
}

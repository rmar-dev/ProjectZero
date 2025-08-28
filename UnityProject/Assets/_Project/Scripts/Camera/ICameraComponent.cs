using UnityEngine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Interface for modular camera components following single responsibility principle.
    /// Each camera component handles one specific aspect of camera behavior.
    /// </summary>
    public interface ICameraComponent
    {
        /// <summary>
        /// Initialize the component with required dependencies
        /// </summary>
        /// <param name="cameraTransform">Transform of the camera target</param>
        void Initialize(Transform cameraTransform);
        
        /// <summary>
        /// Update the component's functionality each frame
        /// </summary>
        void UpdateComponent();
        
        /// <summary>
        /// Clean up component resources
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// Whether this component is currently active and processing
        /// </summary>
        bool IsActive { get; set; }
    }
}

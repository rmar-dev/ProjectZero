using UnityEngine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Base interface for all camera components in the compositional camera system.
    /// Enables modular, testable, and maintainable camera behaviors.
    /// </summary>
    public interface ICameraComponent
    {
        /// <summary>
        /// Initialize the component with required dependencies
        /// </summary>
        /// <param name="context">Camera context containing shared data</param>
        void Initialize(ICameraContext context);
        
        /// <summary>
        /// Update component logic - called from main camera controller
        /// </summary>
        void UpdateComponent();
        
        /// <summary>
        /// Enable or disable this component
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Priority for update order (higher values update first)
        /// </summary>
        int UpdatePriority { get; }
    }
}

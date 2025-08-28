namespace ProjectZero.Camera
{
    /// <summary>
    /// Camera movement modes for different gameplay situations
    /// </summary>
    public enum CameraMode
    {
        /// <summary>
        /// Free movement controlled by player input
        /// </summary>
        Free,
        
        /// <summary>
        /// Following a specific target (squad, unit, etc.)
        /// </summary>
        Follow,
        
        /// <summary>
        /// Locked position, no movement allowed
        /// </summary>
        Locked,
        
        /// <summary>
        /// Cinematic mode for cutscenes
        /// </summary>
        Cinematic
    }

    /// <summary>
    /// Camera zoom behavior types
    /// </summary>
    public enum ZoomMode
    {
        /// <summary>
        /// Manual zoom control via input
        /// </summary>
        Manual,
        
        /// <summary>
        /// Automatic zoom based on squad spread
        /// </summary>
        AutoSquad,
        
        /// <summary>
        /// Fixed zoom level, no changes
        /// </summary>
        Fixed,
        
        /// <summary>
        /// Zoom to fit specific targets
        /// </summary>
        FitTargets
    }

    /// <summary>
    /// Edge scrolling activation modes
    /// </summary>
    public enum EdgeScrollMode
    {
        /// <summary>
        /// Edge scrolling always enabled
        /// </summary>
        Always,
        
        /// <summary>
        /// Only when no keyboard input is active
        /// </summary>
        WhenIdle,
        
        /// <summary>
        /// Edge scrolling disabled
        /// </summary>
        Disabled
    }

    /// <summary>
    /// Camera transition types for smooth movement
    /// </summary>
    public enum TransitionType
    {
        /// <summary>
        /// Instant movement, no transition
        /// </summary>
        Instant,
        
        /// <summary>
        /// Linear interpolation
        /// </summary>
        Linear,
        
        /// <summary>
        /// Smooth step interpolation
        /// </summary>
        SmoothStep,
        
        /// <summary>
        /// Ease in-out curve
        /// </summary>
        EaseInOut,
        
        /// <summary>
        /// Custom animation curve
        /// </summary>
        Custom
    }

    /// <summary>
    /// Camera bounds constraint types
    /// </summary>
    public enum BoundsType
    {
        /// <summary>
        /// Rectangular bounds (most common)
        /// </summary>
        Rectangle,
        
        /// <summary>
        /// Circular bounds around center
        /// </summary>
        Circle,
        
        /// <summary>
        /// No bounds constraints
        /// </summary>
        None,
        
        /// <summary>
        /// Custom shape defined by colliders
        /// </summary>
        Custom
    }
}

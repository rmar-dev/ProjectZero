using UnityEngine;

namespace ProjectZero.Core.Time
{
    /// <summary>
    /// Defines the different tactical time states for the game
    /// These states control Time.timeScale and overall game pacing
    /// </summary>
    public enum TacticalTimeState
    {
        /// <summary>
        /// Normal speed gameplay - Time.timeScale = 1.0f
        /// Basic combat, movement, and all systems run at normal speed
        /// </summary>
        RealTime,

        /// <summary>
        /// Complete pause for command queuing - Time.timeScale = 0.0f
        /// Used for tactical planning and command queue preparation
        /// </summary>
        TacticalPause,

        /// <summary>
        /// Slowed time for tactical decision making - Time.timeScale = 0.3f (configurable)
        /// Allows precise timing of abilities and tactical coordination
        /// </summary>
        SlowMotion,

        /// <summary>
        /// Command planning phase - Time.timeScale = 0.1f (configurable)
        /// Ultra-slow mode for complex ability coordination and ATB planning
        /// </summary>
        CommandPlanning,

        /// <summary>
        /// Special ability execution mode - Time.timeScale varies based on ability
        /// Used during cinematic ability executions or special sequences
        /// </summary>
        AbilityExecution
    }

    /// <summary>
    /// Defines how time state transitions should be handled
    /// </summary>
    public enum TimeTransitionType
    {
        /// <summary>
        /// Instant transition to new time scale
        /// </summary>
        Instant,

        /// <summary>
        /// Smooth interpolation to new time scale over time
        /// </summary>
        Smooth,

        /// <summary>
        /// Stepped transition with brief pause
        /// </summary>
        Stepped
    }

    /// <summary>
    /// Defines priority levels for time state changes
    /// Higher priority states can override lower priority ones
    /// </summary>
    public enum TimeStatePriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3,
        Emergency = 4
    }

    /// <summary>
    /// Event data structure for time state changes
    /// </summary>
    [System.Serializable]
    public struct TimeStateChangeEvent
    {
        public TacticalTimeState fromState;
        public TacticalTimeState toState;
        public float fromTimeScale;
        public float toTimeScale;
        public float transitionDuration;
        public TimeTransitionType transitionType;
        public string reason;
        public System.DateTime timestamp;

        public TimeStateChangeEvent(TacticalTimeState from, TacticalTimeState to, float fromScale, float toScale, 
                                  float duration, TimeTransitionType type, string changeReason)
        {
            fromState = from;
            toState = to;
            fromTimeScale = fromScale;
            toTimeScale = toScale;
            transitionDuration = duration;
            transitionType = type;
            reason = changeReason;
            timestamp = System.DateTime.Now;
        }
    }
}

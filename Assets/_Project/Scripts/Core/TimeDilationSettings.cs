using UnityEngine;
using ProjectZero.Core.Time;

namespace ProjectZero.Core.Time
{
    /// <summary>
    /// ScriptableObject that defines all time dilation settings for the tactical time system
    /// This allows designers to adjust time scales without touching code
    /// </summary>
    [CreateAssetMenu(fileName = "TimeDilationSettings", menuName = "ProjectZero/Core/Time Dilation Settings")]
    public class TimeDilationSettings : ScriptableObject
    {
        [Header("Time Scale Values")]
        [Tooltip("Normal gameplay speed")]
        [Range(0.1f, 2.0f)]
        public float realTimeScale = 1.0f;

        [Tooltip("Complete pause for tactical planning")]
        [Range(0.0f, 0.1f)]
        public float tacticalPauseScale = 0.0f;

        [Tooltip("Slow motion for tactical decision making")]
        [Range(0.1f, 0.8f)]
        public float slowMotionScale = 0.3f;

        [Tooltip("Ultra-slow for complex ability coordination")]
        [Range(0.05f, 0.3f)]
        public float commandPlanningScale = 0.1f;

        [Tooltip("Variable scale for ability executions")]
        [Range(0.1f, 1.0f)]
        public float abilityExecutionScale = 0.6f;

        [Header("Transition Settings")]
        [Tooltip("Default transition type for state changes")]
        public TimeTransitionType defaultTransitionType = TimeTransitionType.Smooth;

        [Tooltip("Duration for smooth transitions")]
        [Range(0.1f, 2.0f)]
        public float smoothTransitionDuration = 0.5f;

        [Tooltip("Duration for stepped transitions")]
        [Range(0.05f, 0.5f)]
        public float steppedTransitionDuration = 0.2f;

        [Header("Auto-Exit Settings")]
        [Tooltip("Should tactical modes auto-exit after timeout?")]
        public bool enableAutoExit = true;

        [Tooltip("Timeout for slow motion mode (seconds in real time)")]
        [Range(3.0f, 30.0f)]
        public float slowMotionTimeout = 8.0f;

        [Tooltip("Timeout for command planning mode (seconds in real time)")]
        [Range(5.0f, 60.0f)]
        public float commandPlanningTimeout = 15.0f;

        [Header("ATB Integration")]
        [Tooltip("Time scale multiplier for ATB gauge filling in tactical modes")]
        [Range(0.1f, 2.0f)]
        public float atbFillMultiplierInTacticalMode = 0.5f;

        [Tooltip("Minimum number of ready ATB units to suggest tactical mode")]
        [Range(1, 6)]
        public int minReadyUnitsForTacticalSuggestion = 2;

        [Header("Performance")]
        [Tooltip("Update frequency for time scale transitions (Hz)")]
        [Range(10f, 60f)]
        public float transitionUpdateFrequency = 30f;

        [Tooltip("Threshold for considering transition complete")]
        [Range(0.001f, 0.1f)]
        public float transitionCompleteThreshold = 0.01f;

        [Header("Debug Settings")]
        [Tooltip("Log time state changes to console")]
        public bool enableDebugLogging = false;

        [Tooltip("Show time scale in UI during gameplay")]
        public bool showTimeScaleInUI = false;

        /// <summary>
        /// Gets the time scale value for a specific tactical time state
        /// </summary>
        public float GetTimeScale(TacticalTimeState state)
        {
            return state switch
            {
                TacticalTimeState.RealTime => realTimeScale,
                TacticalTimeState.TacticalPause => tacticalPauseScale,
                TacticalTimeState.SlowMotion => slowMotionScale,
                TacticalTimeState.CommandPlanning => commandPlanningScale,
                TacticalTimeState.AbilityExecution => abilityExecutionScale,
                _ => realTimeScale
            };
        }

        /// <summary>
        /// Gets the auto-exit timeout for a specific tactical time state
        /// </summary>
        public float GetAutoExitTimeout(TacticalTimeState state)
        {
            return state switch
            {
                TacticalTimeState.SlowMotion => slowMotionTimeout,
                TacticalTimeState.CommandPlanning => commandPlanningTimeout,
                _ => 0f
            };
        }

        /// <summary>
        /// Checks if the given state should auto-exit after timeout
        /// </summary>
        public bool ShouldAutoExit(TacticalTimeState state)
        {
            if (!enableAutoExit) return false;

            return state switch
            {
                TacticalTimeState.SlowMotion => true,
                TacticalTimeState.CommandPlanning => true,
                _ => false
            };
        }

        /// <summary>
        /// Gets the transition duration based on transition type
        /// </summary>
        public float GetTransitionDuration(TimeTransitionType transitionType)
        {
            return transitionType switch
            {
                TimeTransitionType.Instant => 0f,
                TimeTransitionType.Smooth => smoothTransitionDuration,
                TimeTransitionType.Stepped => steppedTransitionDuration,
                _ => smoothTransitionDuration
            };
        }

        /// <summary>
        /// Validates settings on value change (called in editor)
        /// </summary>
        private void OnValidate()
        {
            // Ensure logical constraints
            if (commandPlanningScale > slowMotionScale)
            {
                commandPlanningScale = slowMotionScale * 0.5f;
            }

            if (tacticalPauseScale > 0.05f)
            {
                tacticalPauseScale = 0.0f;
            }

            // Ensure reasonable timeouts
            if (commandPlanningTimeout < slowMotionTimeout)
            {
                commandPlanningTimeout = slowMotionTimeout * 1.5f;
            }
        }
    }
}

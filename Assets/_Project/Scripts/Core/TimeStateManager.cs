using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using ProjectZero.Core.Time;

namespace ProjectZero.Core.Time
{
    /// <summary>
    /// Integration layer between TimeDilationController and other game systems
    /// Provides convenience methods and handles system-specific time state logic
    /// </summary>
    public class TimeStateManager : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Integration Settings")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool handleInputEvents = true;

        [Header("ATB Integration")]
        [SerializeField] private bool enableATBSuggestions = true;
        [SerializeField] private float atbSuggestionCooldown = 2.0f;

        [Header("Combat Integration")]
        [SerializeField] private bool pauseOnCombatStart = false;
        [SerializeField] private bool autoSlowMotionOnMultipleTargets = true;
        [SerializeField] private int multipleTargetThreshold = 3;

        [Header("Input Settings")]
        [SerializeField] private InputActionReference tacticalToggleAction;
        [SerializeField] private InputActionReference pauseAction;
        [SerializeField] private InputActionReference slowMotionAction;
        [SerializeField] private InputActionReference commandPlanningAction;
        #endregion

        #region Private Fields
        private TimeDilationController timeDilationController;
        private float lastATBSuggestionTime;
        private bool isInitialized = false;
        private Dictionary<string, System.DateTime> cooldowns = new Dictionary<string, System.DateTime>();
        #endregion

        #region Events
        [Header("Manager Events")]
        public UnityEvent<bool> OnCombatStateChanged = new UnityEvent<bool>();
        public UnityEvent<int> OnMultipleTargetsDetected = new UnityEvent<int>();
        public UnityEvent OnATBSuggestionTriggered = new UnityEvent();
        public UnityEvent<string> OnTimeStateChangeRequested = new UnityEvent<string>();
        #endregion

        #region Properties
        public bool IsInitialized => isInitialized;
        public TimeDilationController Controller => timeDilationController;
        public bool IsInCombat { get; private set; } = false;
        public int CurrentTargetCount { get; private set; } = 0;
        public bool CanSuggestATBMode => enableATBSuggestions && UnityEngine.Time.unscaledTime - lastATBSuggestionTime > atbSuggestionCooldown;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (autoInitialize)
            {
                Initialize();
            }
            
            // Enable input actions
            EnableInputActions();
        }

        private void Update()
        {
            if (!isInitialized) return;

            if (handleInputEvents)
            {
                HandleInputEvents();
            }
        }

        private void OnEnable()
        {
            if (isInitialized)
            {
                SubscribeToEvents();
            }
            EnableInputActions();
        }

        private void OnDisable()
        {
            if (isInitialized)
            {
                UnsubscribeFromEvents();
            }
            DisableInputActions();
        }
        #endregion

        #region Initialization
        public void Initialize()
        {
            if (isInitialized) return;

            // Get or create the TimeDilationController
            timeDilationController = TimeDilationController.Instance;

            if (timeDilationController == null)
            {
                Debug.LogError("TimeStateManager: Could not find or create TimeDilationController!");
                return;
            }

            SubscribeToEvents();
            isInitialized = true;

            Debug.Log("TimeStateManager initialized successfully.");
        }

        private void SubscribeToEvents()
        {
            if (timeDilationController != null)
            {
                timeDilationController.OnStateChanged.AddListener(OnTimeStateChanged);
                timeDilationController.OnTacticalModeEntered.AddListener(OnTacticalModeEntered);
                timeDilationController.OnTacticalModeExited.AddListener(OnTacticalModeExited);
                timeDilationController.OnAutoExitTriggered.AddListener(OnAutoExitTriggered);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (timeDilationController != null)
            {
                timeDilationController.OnStateChanged.RemoveListener(OnTimeStateChanged);
                timeDilationController.OnTacticalModeEntered.RemoveListener(OnTacticalModeEntered);
                timeDilationController.OnTacticalModeExited.RemoveListener(OnTacticalModeExited);
                timeDilationController.OnAutoExitTriggered.RemoveListener(OnAutoExitTriggered);
            }
        }
        #endregion

        #region Input Handling
        private void EnableInputActions()
        {
            if (tacticalToggleAction != null) tacticalToggleAction.action.Enable();
            if (pauseAction != null) pauseAction.action.Enable();
            if (slowMotionAction != null) slowMotionAction.action.Enable();
            if (commandPlanningAction != null) commandPlanningAction.action.Enable();
        }
        
        private void DisableInputActions()
        {
            if (tacticalToggleAction != null) tacticalToggleAction.action.Disable();
            if (pauseAction != null) pauseAction.action.Disable();
            if (slowMotionAction != null) slowMotionAction.action.Disable();
            if (commandPlanningAction != null) commandPlanningAction.action.Disable();
        }
        
        private void HandleInputEvents()
        {
            // Toggle tactical mode
            if (tacticalToggleAction?.action.WasPressedThisFrame() == true)
            {
                RequestToggleTacticalMode("Input Toggle");
            }

            // Pause key
            if (pauseAction?.action.WasPressedThisFrame() == true)
            {
                RequestPauseToggle("Input Pause");
            }

            // Slow motion key
            if (slowMotionAction?.action.IsPressed() == true && !timeDilationController.IsInTacticalMode)
            {
                RequestSlowMotion("Input Slow Motion");
            }
            else if (slowMotionAction?.action.WasReleasedThisFrame() == true && timeDilationController.CurrentState == TacticalTimeState.SlowMotion)
            {
                RequestRealTime("Release Slow Motion");
            }

            // Command planning key
            if (commandPlanningAction?.action.IsPressed() == true && timeDilationController.IsInTacticalMode)
            {
                RequestCommandPlanning("Input Command Planning");
            }
            else if (commandPlanningAction?.action.WasReleasedThisFrame() == true && timeDilationController.CurrentState == TacticalTimeState.CommandPlanning)
            {
                RequestSlowMotion("Release Command Planning");
            }
        }
        #endregion

        #region Public API - State Requests
        /// <summary>
        /// Requests a tactical mode toggle with reason tracking
        /// </summary>
        public void RequestToggleTacticalMode(string reason = "Manual Toggle")
        {
            if (!isInitialized) return;

            OnTimeStateChangeRequested?.Invoke($"Toggle Tactical: {reason}");
            timeDilationController.ToggleTacticalMode();
        }

        /// <summary>
        /// Requests slow motion mode
        /// </summary>
        public void RequestSlowMotion(string reason = "Manual Slow Motion")
        {
            if (!isInitialized) return;

            OnTimeStateChangeRequested?.Invoke($"Slow Motion: {reason}");
            timeDilationController.SetTimeState(TacticalTimeState.SlowMotion, TimeStatePriority.Normal, reason: reason);
        }

        /// <summary>
        /// Requests real time mode
        /// </summary>
        public void RequestRealTime(string reason = "Manual Real Time")
        {
            if (!isInitialized) return;

            OnTimeStateChangeRequested?.Invoke($"Real Time: {reason}");
            timeDilationController.SetTimeState(TacticalTimeState.RealTime, TimeStatePriority.Normal, reason: reason);
        }

        /// <summary>
        /// Requests command planning mode
        /// </summary>
        public void RequestCommandPlanning(string reason = "Manual Command Planning")
        {
            if (!isInitialized) return;

            OnTimeStateChangeRequested?.Invoke($"Command Planning: {reason}");
            timeDilationController.EnterCommandPlanning(reason);
        }

        /// <summary>
        /// Requests tactical pause toggle
        /// </summary>
        public void RequestPauseToggle(string reason = "Manual Pause Toggle")
        {
            if (!isInitialized) return;

            OnTimeStateChangeRequested?.Invoke($"Pause Toggle: {reason}");

            if (timeDilationController.IsPaused)
            {
                timeDilationController.ExitTacticalPause(reason);
            }
            else
            {
                timeDilationController.EnterTacticalPause(reason);
            }
        }
        #endregion

        #region Combat Integration
        /// <summary>
        /// Notifies the manager that combat has started
        /// </summary>
        public void OnCombatStarted(int enemyCount = 1)
        {
            IsInCombat = true;
            CurrentTargetCount = enemyCount;
            OnCombatStateChanged?.Invoke(true);

            if (pauseOnCombatStart)
            {
                RequestPauseToggle("Combat Started");
            }
            else if (autoSlowMotionOnMultipleTargets && enemyCount >= multipleTargetThreshold)
            {
                OnMultipleTargetsDetected?.Invoke(enemyCount);
                RequestSlowMotion($"Multiple Targets ({enemyCount})");
            }
        }

        /// <summary>
        /// Notifies the manager that combat has ended
        /// </summary>
        public void OnCombatEnded()
        {
            IsInCombat = false;
            CurrentTargetCount = 0;
            OnCombatStateChanged?.Invoke(false);

            // Return to real time if we were in tactical mode due to combat
            if (timeDilationController.IsInTacticalMode)
            {
                RequestRealTime("Combat Ended");
            }
        }

        /// <summary>
        /// Updates the current target count during combat
        /// </summary>
        public void UpdateTargetCount(int newCount)
        {
            int previousCount = CurrentTargetCount;
            CurrentTargetCount = newCount;

            if (autoSlowMotionOnMultipleTargets)
            {
                if (previousCount < multipleTargetThreshold && newCount >= multipleTargetThreshold)
                {
                    OnMultipleTargetsDetected?.Invoke(newCount);
                    RequestSlowMotion($"Target Count Increased ({newCount})");
                }
                else if (previousCount >= multipleTargetThreshold && newCount < multipleTargetThreshold)
                {
                    RequestRealTime($"Target Count Decreased ({newCount})");
                }
            }
        }
        #endregion

        #region ATB Integration
        /// <summary>
        /// Called by ATB system when multiple units have abilities ready
        /// </summary>
        public void OnATBUnitsReady(int readyCount)
        {
            if (!CanSuggestATBMode) return;

            var settings = timeDilationController.Settings;
            if (readyCount >= settings.minReadyUnitsForTacticalSuggestion)
            {
                SuggestTacticalMode($"ATB Units Ready ({readyCount})");
            }
        }

        /// <summary>
        /// Suggests tactical mode entry (doesn't force it)
        /// </summary>
        public void SuggestTacticalMode(string reason = "System Suggestion")
        {
            if (!CanSuggestATBMode) return;

            lastATBSuggestionTime = UnityEngine.Time.unscaledTime;
            OnATBSuggestionTriggered?.Invoke();

            // Only auto-enter if we're in real time and not in combat
            if (timeDilationController.IsInRealTime && !IsInCombat)
            {
                RequestSlowMotion($"Suggested: {reason}");
            }

            Debug.Log($"Tactical mode suggested: {reason}");
        }

        /// <summary>
        /// Gets the current ATB time multiplier
        /// </summary>
        public float GetATBTimeMultiplier()
        {
            return timeDilationController?.GetATBTimeMultiplier() ?? 1.0f;
        }
        #endregion

        #region Cooldown Management
        /// <summary>
        /// Checks if an action is on cooldown
        /// </summary>
        public bool IsOnCooldown(string actionName, float cooldownSeconds)
        {
            if (!cooldowns.ContainsKey(actionName))
                return false;

            return (System.DateTime.Now - cooldowns[actionName]).TotalSeconds < cooldownSeconds;
        }

        /// <summary>
        /// Sets a cooldown for an action
        /// </summary>
        public void SetCooldown(string actionName)
        {
            cooldowns[actionName] = System.DateTime.Now;
        }

        /// <summary>
        /// Clears a specific cooldown
        /// </summary>
        public void ClearCooldown(string actionName)
        {
            if (cooldowns.ContainsKey(actionName))
            {
                cooldowns.Remove(actionName);
            }
        }
        #endregion

        #region Event Handlers
        private void OnTimeStateChanged(TacticalTimeState newState)
        {
            // Handle system-specific logic when time state changes
            Debug.Log($"TimeStateManager: Time state changed to {newState}");
        }

        private void OnTacticalModeEntered(TacticalTimeState state)
        {
            Debug.Log($"TimeStateManager: Entered tactical mode ({state})");
        }

        private void OnTacticalModeExited(TacticalTimeState previousState)
        {
            Debug.Log($"TimeStateManager: Exited tactical mode (was {previousState})");
        }

        private void OnAutoExitTriggered()
        {
            Debug.Log("TimeStateManager: Auto-exit triggered");
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Gets debug information about the manager state
        /// </summary>
        public string GetDebugInfo()
        {
            if (!isInitialized)
                return "TimeStateManager: Not initialized";

            return $"Combat: {IsInCombat} | Targets: {CurrentTargetCount} | " +
                   $"Can ATB Suggest: {CanSuggestATBMode} | Controller: {timeDilationController?.GetDebugInfo()}";
        }

        /// <summary>
        /// Resets the manager to default state
        /// </summary>
        [ContextMenu("Reset Manager")]
        public void ResetManager()
        {
            IsInCombat = false;
            CurrentTargetCount = 0;
            cooldowns.Clear();
            lastATBSuggestionTime = 0f;

            if (timeDilationController != null)
            {
                timeDilationController.ForceRealTime("Manager Reset");
            }
        }
        #endregion

        #region Editor Support
#if UNITY_EDITOR
        [ContextMenu("Test Tactical Toggle")]
        private void TestTacticalToggle()
        {
            RequestToggleTacticalMode("Editor Test");
        }

        [ContextMenu("Test Combat Start")]
        private void TestCombatStart()
        {
            OnCombatStarted(5);
        }

        [ContextMenu("Test ATB Suggestion")]
        private void TestATBSuggestion()
        {
            OnATBUnitsReady(3);
        }
#endif
        #endregion
    }
}

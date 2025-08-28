using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using ProjectZero.Core.Time;

namespace ProjectZero.Core.Time
{
    /// <summary>
    /// Single Source of Truth for Time Dilation Control
    /// Manages all time scale transitions between real-time and tactical time states
    /// Uses singleton pattern to ensure consistent time management across the game
    /// </summary>
    public class TimeDilationController : MonoBehaviour
    {
        #region Singleton Implementation
        private static TimeDilationController _instance;
        public static TimeDilationController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TimeDilationController>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TimeDilationController");
                        _instance = go.AddComponent<TimeDilationController>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Inspector Fields
        [Header("Time Dilation Configuration")]
        [SerializeField] private TimeDilationSettings settings;

        [Header("Current State")]
        [SerializeField, ReadOnly] private TacticalTimeState currentState = TacticalTimeState.RealTime;
        [SerializeField, ReadOnly] private float currentTimeScale = 1.0f;
        [SerializeField, ReadOnly] private bool isTransitioning = false;

        [Header("State History (Debug)")]
        [SerializeField] private bool trackStateHistory = true;
        [SerializeField] private int maxHistorySize = 10;
        #endregion

        #region Private Fields
        private TacticalTimeState previousState;
        private float targetTimeScale;
        private Coroutine transitionCoroutine;
        private Coroutine autoExitCoroutine;
        private float stateEntryTime;
        private List<TimeStateChangeEvent> stateHistory = new List<TimeStateChangeEvent>();
        private Dictionary<TacticalTimeState, TimeStatePriority> statePriorities = new Dictionary<TacticalTimeState, TimeStatePriority>();
        private TimeStatePriority currentStatePriority = TimeStatePriority.Normal;
        #endregion

        #region Unity Events
        [Header("Events")]
        public UnityEvent<TacticalTimeState> OnStateChanged = new UnityEvent<TacticalTimeState>();
        public UnityEvent<float> OnTimeScaleChanged = new UnityEvent<float>();
        public UnityEvent<TimeStateChangeEvent> OnStateChangeEvent = new UnityEvent<TimeStateChangeEvent>();
        public UnityEvent OnTransitionStarted = new UnityEvent();
        public UnityEvent OnTransitionCompleted = new UnityEvent();
        public UnityEvent<TacticalTimeState> OnTacticalModeEntered = new UnityEvent<TacticalTimeState>();
        public UnityEvent<TacticalTimeState> OnTacticalModeExited = new UnityEvent<TacticalTimeState>();
        public UnityEvent OnAutoExitTriggered = new UnityEvent();
        #endregion

        #region Properties
        public TacticalTimeState CurrentState => currentState;
        public float CurrentTimeScale => currentTimeScale;
        public bool IsTransitioning => isTransitioning;
        public bool IsInTacticalMode => currentState != TacticalTimeState.RealTime;
        public bool IsInRealTime => currentState == TacticalTimeState.RealTime;
        public bool IsPaused => currentState == TacticalTimeState.TacticalPause;
        public TimeDilationSettings Settings => settings;
        public List<TimeStateChangeEvent> StateHistory => new List<TimeStateChangeEvent>(stateHistory);
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeController();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Ensure we start in real time
            SetTimeState(TacticalTimeState.RealTime, TimeStatePriority.Normal, TimeTransitionType.Instant, "Game Start");
        }

        private void Update()
        {
            // Input handling is now managed by TimeStateManager
            // This keeps the controller focused on time state management only
        }
        #endregion

        #region Initialization
        private void InitializeController()
        {
            if (settings == null)
            {
                Debug.LogWarning("TimeDilationController: No settings assigned! Creating default settings.");
                CreateDefaultSettings();
            }

            InitializeStatePriorities();
            currentTimeScale = UnityEngine.Time.timeScale;
            stateEntryTime = UnityEngine.Time.unscaledTime;

            if (settings.enableDebugLogging)
            {
                Debug.Log("TimeDilationController initialized successfully.");
            }
        }

        private void CreateDefaultSettings()
        {
            settings = ScriptableObject.CreateInstance<TimeDilationSettings>();
            // Default values are already set in the ScriptableObject
        }

        private void InitializeStatePriorities()
        {
            statePriorities[TacticalTimeState.RealTime] = TimeStatePriority.Normal;
            statePriorities[TacticalTimeState.SlowMotion] = TimeStatePriority.Normal;
            statePriorities[TacticalTimeState.CommandPlanning] = TimeStatePriority.High;
            statePriorities[TacticalTimeState.TacticalPause] = TimeStatePriority.High;
            statePriorities[TacticalTimeState.AbilityExecution] = TimeStatePriority.Critical;
        }
        #endregion

        #region Public API Methods
        /// <summary>
        /// Sets the tactical time state with specified priority and transition type
        /// </summary>
        public bool SetTimeState(TacticalTimeState newState, TimeStatePriority priority = TimeStatePriority.Normal, 
                               TimeTransitionType? transitionType = null, string reason = "")
        {
            // Check if we can change to this state based on priority
            if (!CanChangeToState(newState, priority))
            {
                if (settings.enableDebugLogging)
                {
                    Debug.LogWarning($"Cannot change to {newState} - insufficient priority ({priority} vs {currentStatePriority})");
                }
                return false;
            }

            // Store previous state
            previousState = currentState;
            float previousTimeScale = currentTimeScale;

            // Update state
            currentState = newState;
            currentStatePriority = priority;
            targetTimeScale = settings.GetTimeScale(newState);

            // Determine transition type
            TimeTransitionType actualTransitionType = transitionType ?? settings.defaultTransitionType;

            // Create state change event
            TimeStateChangeEvent stateEvent = new TimeStateChangeEvent(
                previousState, newState, previousTimeScale, targetTimeScale,
                settings.GetTransitionDuration(actualTransitionType), actualTransitionType, reason
            );

            // Add to history
            AddToStateHistory(stateEvent);

            // Handle transition
            HandleStateTransition(actualTransitionType, stateEvent);

            // Handle auto-exit
            HandleAutoExit(newState);

            // Fire events
            OnStateChanged?.Invoke(newState);
            OnStateChangeEvent?.Invoke(stateEvent);

            if (previousState == TacticalTimeState.RealTime && newState != TacticalTimeState.RealTime)
            {
                OnTacticalModeEntered?.Invoke(newState);
            }
            else if (previousState != TacticalTimeState.RealTime && newState == TacticalTimeState.RealTime)
            {
                OnTacticalModeExited?.Invoke(previousState);
            }

            if (settings.enableDebugLogging)
            {
                Debug.Log($"Time State Changed: {previousState} -> {newState} (Priority: {priority}) | Reason: {reason}");
            }

            return true;
        }

        /// <summary>
        /// Toggles between real time and slow motion
        /// </summary>
        public void ToggleTacticalMode()
        {
            if (currentState == TacticalTimeState.RealTime)
            {
                SetTimeState(TacticalTimeState.SlowMotion, TimeStatePriority.Normal, reason: "Toggle Tactical Mode");
            }
            else
            {
                SetTimeState(TacticalTimeState.RealTime, TimeStatePriority.Normal, reason: "Toggle to Real Time");
            }
        }

        /// <summary>
        /// Forces immediate return to real time (emergency)
        /// </summary>
        public void ForceRealTime(string reason = "Force Real Time")
        {
            SetTimeState(TacticalTimeState.RealTime, TimeStatePriority.Emergency, TimeTransitionType.Instant, reason);
        }

        /// <summary>
        /// Enters tactical pause mode
        /// </summary>
        public void EnterTacticalPause(string reason = "Tactical Pause")
        {
            SetTimeState(TacticalTimeState.TacticalPause, TimeStatePriority.High, TimeTransitionType.Instant, reason);
        }

        /// <summary>
        /// Exits tactical pause mode
        /// </summary>
        public void ExitTacticalPause(string reason = "Exit Tactical Pause")
        {
            SetTimeState(previousState, TimeStatePriority.Normal, reason: reason);
        }

        /// <summary>
        /// Enters command planning mode
        /// </summary>
        public void EnterCommandPlanning(string reason = "Command Planning")
        {
            SetTimeState(TacticalTimeState.CommandPlanning, TimeStatePriority.High, reason: reason);
        }

        /// <summary>
        /// Enters ability execution mode with custom time scale
        /// </summary>
        public void EnterAbilityExecution(float customTimeScale = -1f, string reason = "Ability Execution")
        {
            if (customTimeScale >= 0f)
            {
                // Temporarily override the settings for this execution
                float originalScale = settings.abilityExecutionScale;
                settings.abilityExecutionScale = customTimeScale;
                SetTimeState(TacticalTimeState.AbilityExecution, TimeStatePriority.Critical, reason: reason);
                settings.abilityExecutionScale = originalScale; // Restore original
            }
            else
            {
                SetTimeState(TacticalTimeState.AbilityExecution, TimeStatePriority.Critical, reason: reason);
            }
        }

        /// <summary>
        /// Gets the current time multiplier for ATB systems
        /// </summary>
        public float GetATBTimeMultiplier()
        {
            if (IsInTacticalMode)
            {
                return settings.atbFillMultiplierInTacticalMode;
            }
            return 1.0f;
        }
        #endregion

        #region Private Methods
        private bool CanChangeToState(TacticalTimeState newState, TimeStatePriority priority)
        {
            // Emergency priority always overrides
            if (priority == TimeStatePriority.Emergency)
                return true;

            // Check if new priority is sufficient
            return priority >= currentStatePriority;
        }

        private void HandleStateTransition(TimeTransitionType transitionType, TimeStateChangeEvent stateEvent)
        {
            // Stop any current transition
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            // Start new transition
            switch (transitionType)
            {
                case TimeTransitionType.Instant:
                    PerformInstantTransition();
                    break;

                case TimeTransitionType.Smooth:
                    transitionCoroutine = StartCoroutine(PerformSmoothTransition(stateEvent.transitionDuration));
                    break;

                case TimeTransitionType.Stepped:
                    transitionCoroutine = StartCoroutine(PerformSteppedTransition(stateEvent.transitionDuration));
                    break;
            }
        }

        private void PerformInstantTransition()
        {
            currentTimeScale = targetTimeScale;
            UnityEngine.Time.timeScale = targetTimeScale;
            OnTimeScaleChanged?.Invoke(currentTimeScale);
        }

        private IEnumerator PerformSmoothTransition(float duration)
        {
            isTransitioning = true;
            OnTransitionStarted?.Invoke();

            float startTimeScale = currentTimeScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                float t = elapsed / duration;
                
                // Use smoothstep for better feeling transitions
                t = t * t * (3f - 2f * t);
                
                currentTimeScale = Mathf.Lerp(startTimeScale, targetTimeScale, t);
                UnityEngine.Time.timeScale = currentTimeScale;
                OnTimeScaleChanged?.Invoke(currentTimeScale);

                yield return null;
            }

            // Ensure we end at the exact target
            currentTimeScale = targetTimeScale;
            UnityEngine.Time.timeScale = currentTimeScale;
            OnTimeScaleChanged?.Invoke(currentTimeScale);

            isTransitioning = false;
            OnTransitionCompleted?.Invoke();
            transitionCoroutine = null;
        }

        private IEnumerator PerformSteppedTransition(float duration)
        {
            isTransitioning = true;
            OnTransitionStarted?.Invoke();

            // Brief pause first
            float pauseTimeScale = currentTimeScale;
            currentTimeScale = 0f;
            UnityEngine.Time.timeScale = 0f;
            OnTimeScaleChanged?.Invoke(currentTimeScale);

            yield return new WaitForSecondsRealtime(duration * 0.3f);

            // Jump to target
            currentTimeScale = targetTimeScale;
            UnityEngine.Time.timeScale = currentTimeScale;
            OnTimeScaleChanged?.Invoke(currentTimeScale);

            yield return new WaitForSecondsRealtime(duration * 0.7f);

            isTransitioning = false;
            OnTransitionCompleted?.Invoke();
            transitionCoroutine = null;
        }

        private void HandleAutoExit(TacticalTimeState state)
        {
            // Stop any existing auto-exit
            if (autoExitCoroutine != null)
            {
                StopCoroutine(autoExitCoroutine);
                autoExitCoroutine = null;
            }

            // Start auto-exit if needed
            if (settings.ShouldAutoExit(state))
            {
                float timeout = settings.GetAutoExitTimeout(state);
                autoExitCoroutine = StartCoroutine(AutoExitCoroutine(timeout));
            }
        }

        private IEnumerator AutoExitCoroutine(float timeout)
        {
            yield return new WaitForSecondsRealtime(timeout);

            // Only exit if we're still in the same state and not transitioning
            if (!isTransitioning && currentStatePriority <= TimeStatePriority.Normal)
            {
                OnAutoExitTriggered?.Invoke();
                SetTimeState(TacticalTimeState.RealTime, TimeStatePriority.Normal, reason: "Auto Exit Timeout");
            }

            autoExitCoroutine = null;
        }

        private void AddToStateHistory(TimeStateChangeEvent stateEvent)
        {
            if (!trackStateHistory) return;

            stateHistory.Add(stateEvent);

            // Limit history size
            while (stateHistory.Count > maxHistorySize)
            {
                stateHistory.RemoveAt(0);
            }
        }
        #endregion

        #region Debug and Utility Methods
        /// <summary>
        /// Gets debug information about the current state
        /// </summary>
        public string GetDebugInfo()
        {
            return $"State: {currentState} | TimeScale: {currentTimeScale:F2} | Priority: {currentStatePriority} | " +
                   $"Transitioning: {isTransitioning} | AutoExit: {autoExitCoroutine != null}";
        }

        /// <summary>
        /// Resets the time dilation controller to default state
        /// </summary>
        [ContextMenu("Reset to Real Time")]
        public void ResetToRealTime()
        {
            ForceRealTime("Manual Reset");
        }

        /// <summary>
        /// Clears the state history
        /// </summary>
        [ContextMenu("Clear State History")]
        public void ClearStateHistory()
        {
            stateHistory.Clear();
        }
        #endregion

        #region Unity Editor Support
#if UNITY_EDITOR
        [System.Serializable]
        public class ReadOnlyAttribute : PropertyAttribute { }

        private void OnValidate()
        {
            if (settings == null)
            {
                // Try to find default settings
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TimeDilationSettings");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    settings = UnityEditor.AssetDatabase.LoadAssetAtPath<TimeDilationSettings>(path);
                }
            }
        }
#endif
        #endregion
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom property drawer for ReadOnly attribute
    /// </summary>
    [UnityEditor.CustomPropertyDrawer(typeof(TimeDilationController.ReadOnlyAttribute))]
    public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            UnityEditor.EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
#endif
}

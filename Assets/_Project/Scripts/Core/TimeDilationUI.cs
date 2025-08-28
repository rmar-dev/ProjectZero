using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using TMPro;
using ProjectZero.Core.Time;

namespace ProjectZero.Core.Time
{
    /// <summary>
    /// UI Integration Component for Time Dilation System
    /// Provides visual feedback about current time state and transition hints
    /// </summary>
    public class TimeDilationUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI References")]
        [SerializeField] private Canvas timeStateCanvas;
        [SerializeField] private TextMeshProUGUI timeStateLabel;
        [SerializeField] private TextMeshProUGUI timeScaleLabel;
        [SerializeField] private Slider timeScaleSlider;

        [Header("State Indicators")]
        [SerializeField] private Image stateIndicator;
        [SerializeField] private Color realTimeColor = Color.green;
        [SerializeField] private Color slowMotionColor = Color.yellow;
        [SerializeField] private Color pausedColor = Color.red;
        [SerializeField] private Color commandPlanningColor = Color.blue;
        [SerializeField] private Color abilityExecutionColor = Color.magenta;

        [Header("Transition Effects")]
        [SerializeField] private GameObject transitionEffectPrefab;
        [SerializeField] private AnimationCurve transitionPulse = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float transitionEffectDuration = 0.5f;

        [Header("Tactical Suggestions")]
        [SerializeField] private GameObject tacticalSuggestionPanel;
        [SerializeField] private TextMeshProUGUI suggestionText;
        [SerializeField] private Button acceptSuggestionButton;
        [SerializeField] private Button dismissSuggestionButton;
        [SerializeField] private float suggestionDisplayTime = 3.0f;

        [Header("Auto-Exit Warning")]
        [SerializeField] private GameObject autoExitWarningPanel;
        [SerializeField] private TextMeshProUGUI autoExitCountdownText;
        [SerializeField] private Slider autoExitProgressSlider;

        [Header("Input Hints")]
        [SerializeField] private GameObject inputHintsPanel;
        [SerializeField] private TextMeshProUGUI inputHintsText;
        [SerializeField] private bool showInputHints = true;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        #endregion

        #region Private Fields
        private TimeDilationController timeDilationController;
        private TimeStateManager timeStateManager;
        private bool isInitialized = false;
        private Coroutine suggestionCoroutine;
        private Coroutine autoExitCoroutine;
        private Coroutine transitionEffectCoroutine;
        private CanvasGroup canvasGroup;
        private string currentSuggestionReason;
        #endregion

        #region Properties
        public bool IsVisible { get; private set; } = true;
        public bool IsShowingSuggestion => suggestionCoroutine != null;
        public bool IsShowingAutoExitWarning => autoExitCoroutine != null;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (isInitialized)
            {
                SubscribeToEvents();
            }
        }

        private void OnDisable()
        {
            if (isInitialized)
            {
                UnsubscribeFromEvents();
            }
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            // Get or create canvas group for fading
            canvasGroup = timeStateCanvas?.GetComponent<CanvasGroup>();
            if (canvasGroup == null && timeStateCanvas != null)
            {
                canvasGroup = timeStateCanvas.gameObject.AddComponent<CanvasGroup>();
            }

            // Setup button events
            if (acceptSuggestionButton != null)
            {
                acceptSuggestionButton.onClick.AddListener(AcceptTacticalSuggestion);
            }

            if (dismissSuggestionButton != null)
            {
                dismissSuggestionButton.onClick.AddListener(DismissTacticalSuggestion);
            }

            // Initialize panels as hidden
            if (tacticalSuggestionPanel != null)
                tacticalSuggestionPanel.SetActive(false);

            if (autoExitWarningPanel != null)
                autoExitWarningPanel.SetActive(false);

            UpdateInputHints();
        }

        private void Initialize()
        {
            // Find the time dilation systems
            timeDilationController = TimeDilationController.Instance;
            timeStateManager = FindFirstObjectByType<TimeStateManager>();

            if (timeDilationController == null)
            {
                Debug.LogError("TimeDilationUI: Could not find TimeDilationController!");
                return;
            }

            SubscribeToEvents();
            UpdateUI();
            isInitialized = true;

            Debug.Log("TimeDilationUI initialized successfully.");
        }

        private void SubscribeToEvents()
        {
            if (timeDilationController != null)
            {
                timeDilationController.OnStateChanged.AddListener(OnTimeStateChanged);
                timeDilationController.OnTimeScaleChanged.AddListener(OnTimeScaleChanged);
                timeDilationController.OnTransitionStarted.AddListener(OnTransitionStarted);
                timeDilationController.OnTransitionCompleted.AddListener(OnTransitionCompleted);
                timeDilationController.OnAutoExitTriggered.AddListener(OnAutoExitTriggered);
            }

            if (timeStateManager != null)
            {
                timeStateManager.OnATBSuggestionTriggered.AddListener(OnATBSuggestionTriggered);
                timeStateManager.OnTimeStateChangeRequested.AddListener(OnTimeStateChangeRequested);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (timeDilationController != null)
            {
                timeDilationController.OnStateChanged.RemoveListener(OnTimeStateChanged);
                timeDilationController.OnTimeScaleChanged.RemoveListener(OnTimeScaleChanged);
                timeDilationController.OnTransitionStarted.RemoveListener(OnTransitionStarted);
                timeDilationController.OnTransitionCompleted.RemoveListener(OnTransitionCompleted);
                timeDilationController.OnAutoExitTriggered.RemoveListener(OnAutoExitTriggered);
            }

            if (timeStateManager != null)
            {
                timeStateManager.OnATBSuggestionTriggered.RemoveListener(OnATBSuggestionTriggered);
                timeStateManager.OnTimeStateChangeRequested.RemoveListener(OnTimeStateChangeRequested);
            }
        }
        #endregion

        #region UI Updates
        private void UpdateUI()
        {
            if (!isInitialized) return;

            UpdateStateLabel();
            UpdateTimeScaleDisplay();
            UpdateStateIndicator();
            UpdateInputHints();
        }

        private void UpdateStateLabel()
        {
            if (timeStateLabel == null) return;

            TacticalTimeState currentState = timeDilationController.CurrentState;
            string stateText = currentState switch
            {
                TacticalTimeState.RealTime => "REAL TIME",
                TacticalTimeState.SlowMotion => "TACTICAL MODE",
                TacticalTimeState.CommandPlanning => "COMMAND PLANNING",
                TacticalTimeState.TacticalPause => "PAUSED",
                TacticalTimeState.AbilityExecution => "ABILITY EXECUTION",
                _ => "UNKNOWN"
            };

            timeStateLabel.text = stateText;
        }

        private void UpdateTimeScaleDisplay()
        {
            float currentTimeScale = timeDilationController.CurrentTimeScale;

            if (timeScaleLabel != null)
            {
                timeScaleLabel.text = $"Time Scale: {currentTimeScale:F1}x";
            }

            if (timeScaleSlider != null)
            {
                timeScaleSlider.value = currentTimeScale;
            }
        }

        private void UpdateStateIndicator()
        {
            if (stateIndicator == null) return;

            Color indicatorColor = timeDilationController.CurrentState switch
            {
                TacticalTimeState.RealTime => realTimeColor,
                TacticalTimeState.SlowMotion => slowMotionColor,
                TacticalTimeState.CommandPlanning => commandPlanningColor,
                TacticalTimeState.TacticalPause => pausedColor,
                TacticalTimeState.AbilityExecution => abilityExecutionColor,
                _ => Color.white
            };

            stateIndicator.color = indicatorColor;
        }

        private void UpdateInputHints()
        {
            if (!showInputHints || inputHintsText == null) return;

            var manager = timeStateManager;
            if (manager == null) return;

            string hintsText = "Controls:\n";
            hintsText += "TAB - Toggle Tactical Mode\n";
            hintsText += "P - Pause/Resume\n";
            hintsText += "Shift - Hold for Slow Motion\n";
            hintsText += "Ctrl - Hold for Command Planning";

            inputHintsText.text = hintsText;

            if (inputHintsPanel != null)
            {
                inputHintsPanel.SetActive(showInputHints);
            }
        }
        #endregion

        #region Event Handlers
        private void OnTimeStateChanged(TacticalTimeState newState)
        {
            UpdateUI();

            // Show/hide UI based on settings
            var settings = timeDilationController.Settings;
            if (settings != null && settings.showTimeScaleInUI)
            {
                ShowUI();
            }
        }

        private void OnTimeScaleChanged(float newTimeScale)
        {
            UpdateTimeScaleDisplay();
        }

        private void OnTransitionStarted()
        {
            if (transitionEffectCoroutine != null)
            {
                StopCoroutine(transitionEffectCoroutine);
            }
            transitionEffectCoroutine = StartCoroutine(PlayTransitionEffect());
        }

        private void OnTransitionCompleted()
        {
            // Additional UI updates when transition completes
            UpdateUI();
        }

        private void OnAutoExitTriggered()
        {
            ShowAutoExitWarning();
        }

        private void OnATBSuggestionTriggered()
        {
            ShowTacticalSuggestion("Multiple ATB abilities are ready!");
        }

        private void OnTimeStateChangeRequested(string reason)
        {
            // Could show brief feedback about the requested change
            Debug.Log($"TimeDilationUI: Time state change requested - {reason}");
        }
        #endregion

        #region Tactical Suggestions
        private void ShowTacticalSuggestion(string reason)
        {
            if (tacticalSuggestionPanel == null) return;

            currentSuggestionReason = reason;

            if (suggestionText != null)
            {
                suggestionText.text = reason;
            }

            if (suggestionCoroutine != null)
            {
                StopCoroutine(suggestionCoroutine);
            }

            suggestionCoroutine = StartCoroutine(ShowSuggestionCoroutine());
        }

        private IEnumerator ShowSuggestionCoroutine()
        {
            tacticalSuggestionPanel.SetActive(true);

            // Fade in
            yield return StartCoroutine(FadeCanvasGroup(tacticalSuggestionPanel.GetComponent<CanvasGroup>(), 0f, 1f, fadeInDuration));

            // Wait for user interaction or timeout
            yield return new WaitForSecondsRealtime(suggestionDisplayTime);

            // Auto-dismiss if not interacted with
            DismissTacticalSuggestion();
        }

        private void AcceptTacticalSuggestion()
        {
            if (timeStateManager != null)
            {
                timeStateManager.RequestSlowMotion($"Accepted Suggestion: {currentSuggestionReason}");
            }

            DismissTacticalSuggestion();
        }

        private void DismissTacticalSuggestion()
        {
            if (suggestionCoroutine != null)
            {
                StopCoroutine(suggestionCoroutine);
                suggestionCoroutine = null;
            }

            if (tacticalSuggestionPanel != null)
            {
                StartCoroutine(HideSuggestionCoroutine());
            }
        }

        private IEnumerator HideSuggestionCoroutine()
        {
            var canvasGroup = tacticalSuggestionPanel.GetComponent<CanvasGroup>();
            
            // Fade out
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration));

            tacticalSuggestionPanel.SetActive(false);
        }
        #endregion

        #region Auto-Exit Warning
        private void ShowAutoExitWarning()
        {
            if (autoExitWarningPanel == null) return;

            var settings = timeDilationController.Settings;
            float timeout = settings.GetAutoExitTimeout(timeDilationController.CurrentState);

            if (timeout <= 0) return;

            if (autoExitCoroutine != null)
            {
                StopCoroutine(autoExitCoroutine);
            }

            autoExitCoroutine = StartCoroutine(ShowAutoExitWarningCoroutine(timeout));
        }

        private IEnumerator ShowAutoExitWarningCoroutine(float totalTime)
        {
            autoExitWarningPanel.SetActive(true);

            float elapsed = 0f;
            while (elapsed < totalTime)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                float remaining = totalTime - elapsed;

                if (autoExitCountdownText != null)
                {
                    autoExitCountdownText.text = $"Auto-exit in {remaining:F1}s";
                }

                if (autoExitProgressSlider != null)
                {
                    autoExitProgressSlider.value = 1f - (elapsed / totalTime);
                }

                yield return null;
            }

            autoExitWarningPanel.SetActive(false);
            autoExitCoroutine = null;
        }
        #endregion

        #region Visual Effects
        private IEnumerator PlayTransitionEffect()
        {
            if (transitionEffectPrefab != null && stateIndicator != null)
            {
                GameObject effect = Instantiate(transitionEffectPrefab, stateIndicator.transform);
                yield return new WaitForSeconds(transitionEffectDuration);
                Destroy(effect);
            }

            // Pulse effect on state indicator
            if (stateIndicator != null)
            {
                yield return StartCoroutine(PulseStateIndicator());
            }

            transitionEffectCoroutine = null;
        }

        private IEnumerator PulseStateIndicator()
        {
            Color originalColor = stateIndicator.color;
            float elapsed = 0f;

            while (elapsed < transitionEffectDuration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                float t = elapsed / transitionEffectDuration;
                float pulseValue = transitionPulse.Evaluate(t);

                Color pulseColor = Color.Lerp(originalColor, Color.white, pulseValue * 0.5f);
                stateIndicator.color = pulseColor;

                yield return null;
            }

            stateIndicator.color = originalColor;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float fromAlpha, float toAlpha, float duration)
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                float t = elapsed / duration;
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = toAlpha;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Shows the time dilation UI
        /// </summary>
        public void ShowUI()
        {
            if (canvasGroup != null)
            {
                StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeInDuration));
            }
            else if (timeStateCanvas != null)
            {
                timeStateCanvas.gameObject.SetActive(true);
            }

            IsVisible = true;
        }

        /// <summary>
        /// Hides the time dilation UI
        /// </summary>
        public void HideUI()
        {
            if (canvasGroup != null)
            {
                StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f, fadeOutDuration));
            }
            else if (timeStateCanvas != null)
            {
                timeStateCanvas.gameObject.SetActive(false);
            }

            IsVisible = false;
        }

        /// <summary>
        /// Toggles the UI visibility
        /// </summary>
        public void ToggleUI()
        {
            if (IsVisible)
                HideUI();
            else
                ShowUI();
        }

        /// <summary>
        /// Forces an immediate UI update
        /// </summary>
        public void ForceUpdateUI()
        {
            UpdateUI();
        }

        /// <summary>
        /// Shows a custom tactical suggestion
        /// </summary>
        public void ShowCustomSuggestion(string message)
        {
            ShowTacticalSuggestion(message);
        }
        #endregion

        #region Editor Support
#if UNITY_EDITOR
        [ContextMenu("Test Tactical Suggestion")]
        private void TestTacticalSuggestion()
        {
            ShowTacticalSuggestion("Test: Multiple abilities ready!");
        }

        [ContextMenu("Test Auto-Exit Warning")]
        private void TestAutoExitWarning()
        {
            ShowAutoExitWarning();
        }

        [ContextMenu("Toggle UI Visibility")]
        private void TestToggleUI()
        {
            ToggleUI();
        }

        private void OnValidate()
        {
            // Ensure animation curves are properly set
            if (transitionPulse.keys.Length == 0)
            {
                transitionPulse = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }

            if (fadeInCurve.keys.Length == 0)
            {
                fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }

            if (fadeOutCurve.keys.Length == 0)
            {
                fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
            }
        }
#endif
        #endregion
    }
}

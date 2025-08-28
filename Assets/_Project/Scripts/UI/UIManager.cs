using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace ProjectZero.UI
{
    /// <summary>
    /// Programmatic UI Manager for creating Canvas, Labels, and other UI elements at runtime
    /// Provides a clean API for dynamic UI generation without requiring scene setup
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UIManager");
                        _instance = go.AddComponent<UIManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Inspector Fields
        [Header("UI Configuration")]
        [SerializeField] private UISettings uiSettings;
        [SerializeField] private bool createDefaultCanvas = true;
        [SerializeField] private bool enableDebugMode = false;

        [Header("Canvas References")]
        [SerializeField] private Canvas worldSpaceCanvas;
        [SerializeField] private Canvas screenSpaceCanvas;
        [SerializeField] private Canvas overlayCanvas;
        #endregion

        #region Private Fields
        private Dictionary<string, Canvas> canvasRegistry = new Dictionary<string, Canvas>();
        private Dictionary<string, GameObject> uiElementRegistry = new Dictionary<string, GameObject>();
        private UnityEngine.Camera mainCamera;
        private bool isInitialized = false;
        #endregion

        #region Properties
        public Canvas DefaultCanvas => screenSpaceCanvas;
        public Canvas WorldCanvas => worldSpaceCanvas;
        public Canvas OverlayCanvas => overlayCanvas;
        public bool IsInitialized => isInitialized;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Canvases are now created during Initialize() in Awake()
            // This method is kept for any additional startup logic if needed
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            LoadUISettings();
            FindMainCamera();
            
            // Create default canvases immediately during initialization
            if (createDefaultCanvas)
            {
                CreateDefaultCanvases();
            }
            
            isInitialized = true;

            if (enableDebugMode)
            {
                Debug.Log("[UIManager] Initialized successfully");
            }
        }

        private void LoadUISettings()
        {
            if (uiSettings == null)
            {
                // Try to find default UI settings
                uiSettings = Resources.Load<UISettings>("DefaultUISettings");
                
                if (uiSettings == null)
                {
                    Debug.LogWarning("[UIManager] No UI settings found, creating default settings");
                    CreateDefaultUISettings();
                }
            }
        }

        private void CreateDefaultUISettings()
        {
            uiSettings = ScriptableObject.CreateInstance<UISettings>();
            // Default values will be set by the ScriptableObject
        }

        private void FindMainCamera()
        {
            mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<UnityEngine.Camera>();
            }
        }

        private void CreateDefaultCanvases()
        {
            // Create Screen Space Canvas
            screenSpaceCanvas = CreateCanvas("ScreenSpaceCanvas", RenderMode.ScreenSpaceOverlay);
            screenSpaceCanvas.sortingOrder = 0;

            // Create World Space Canvas
            worldSpaceCanvas = CreateCanvas("WorldSpaceCanvas", RenderMode.WorldSpace);
            worldSpaceCanvas.sortingOrder = -1;

            // Create Overlay Canvas (for HUD elements)
            overlayCanvas = CreateCanvas("OverlayCanvas", RenderMode.ScreenSpaceOverlay);
            overlayCanvas.sortingOrder = 100;

            if (enableDebugMode)
            {
                Debug.Log("[UIManager] Default canvases created");
            }
        }
        #endregion

        #region Canvas Creation
        /// <summary>
        /// Creates a new Canvas with the specified configuration
        /// </summary>
        public Canvas CreateCanvas(string canvasName, RenderMode renderMode, int sortingOrder = 0)
        {
            GameObject canvasGO = new GameObject(canvasName);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();

            // Configure Canvas
            canvas.renderMode = renderMode;
            canvas.sortingOrder = sortingOrder;

            if (renderMode == RenderMode.WorldSpace)
            {
                canvas.worldCamera = mainCamera;
            }
            else if (renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.worldCamera = mainCamera;
                canvas.planeDistance = 10f;
            }

            // Configure Canvas Scaler
            ConfigureCanvasScaler(scaler, renderMode);

            // Register canvas
            canvasRegistry[canvasName] = canvas;

            // Parent to UIManager for organization
            canvasGO.transform.SetParent(transform);

            if (enableDebugMode)
            {
                Debug.Log($"[UIManager] Created canvas: {canvasName} ({renderMode})");
            }

            return canvas;
        }

        private void ConfigureCanvasScaler(CanvasScaler scaler, RenderMode renderMode)
        {
            if (renderMode == RenderMode.WorldSpace)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor = uiSettings.worldSpaceScaleFactor;
            }
            else
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = uiSettings.referenceResolution;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f; // Balance between width and height
            }
        }

        /// <summary>
        /// Gets or creates a canvas by name
        /// </summary>
        public Canvas GetOrCreateCanvas(string canvasName, RenderMode renderMode = RenderMode.ScreenSpaceOverlay)
        {
            if (canvasRegistry.ContainsKey(canvasName))
            {
                return canvasRegistry[canvasName];
            }

            return CreateCanvas(canvasName, renderMode);
        }
        #endregion

        #region Label Creation
        /// <summary>
        /// Creates a TextMeshPro label on the specified canvas
        /// </summary>
        public TextMeshProUGUI CreateLabel(string labelId, string text, Canvas parentCanvas = null, 
                                          Vector2? position = null, Vector2? size = null)
        {
            if (parentCanvas == null)
                parentCanvas = screenSpaceCanvas ?? GetOrCreateCanvas("DefaultCanvas");

            GameObject labelGO = new GameObject($"Label_{labelId}");
            labelGO.transform.SetParent(parentCanvas.transform, false);

            // Add TextMeshPro component
            TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
            
            // Configure label
            ConfigureLabel(label, text, position, size);

            // Register the label
            uiElementRegistry[labelId] = labelGO;

            if (enableDebugMode)
            {
                Debug.Log($"[UIManager] Created label: {labelId} - '{text}'");
            }

            return label;
        }

        /// <summary>
        /// Creates a world space label that follows a transform
        /// </summary>
        public TextMeshProUGUI CreateWorldLabel(string labelId, string text, Transform followTarget, 
                                               Vector3 offset = default)
        {
            Canvas worldCanvas = worldSpaceCanvas ?? GetOrCreateCanvas("WorldCanvas", RenderMode.WorldSpace);
            
            GameObject labelGO = new GameObject($"WorldLabel_{labelId}");
            labelGO.transform.SetParent(worldCanvas.transform, false);

            TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
            ConfigureWorldLabel(label, text);

            // Add world label tracker
            WorldLabelTracker tracker = labelGO.AddComponent<WorldLabelTracker>();
            tracker.Initialize(followTarget, offset, mainCamera);

            uiElementRegistry[labelId] = labelGO;

            if (enableDebugMode)
            {
                Debug.Log($"[UIManager] Created world label: {labelId} - '{text}'");
            }

            return label;
        }

        private void ConfigureLabel(TextMeshProUGUI label, string text, Vector2? position = null, Vector2? size = null)
        {
            label.text = text;
            label.fontSize = uiSettings.defaultFontSize;
            label.color = uiSettings.defaultTextColor;
            label.font = uiSettings.defaultFont;
            label.alignment = TextAlignmentOptions.Center;

            // Configure RectTransform
            RectTransform rectTransform = label.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size ?? uiSettings.defaultLabelSize;

            if (position.HasValue)
            {
                rectTransform.anchoredPosition = position.Value;
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.zero;
            }
            else
            {
                // Center the label by default
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }
        
        private void ConfigureWorldLabel(TextMeshProUGUI label, string text)
        {
            label.text = text;
            label.fontSize = uiSettings.worldLabelFontSize;
            label.color = uiSettings.defaultTextColor;
            label.font = uiSettings.defaultFont;
            label.alignment = TextAlignmentOptions.Center;

            // Configure RectTransform for world space
            RectTransform rectTransform = label.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 25); // Smaller size for world labels
            
            // Center the world label
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            
            // Apply world label scaling
            rectTransform.localScale = Vector3.one * uiSettings.worldLabelScale;
        }
        #endregion

        #region Button Creation
        /// <summary>
        /// Creates a button with text
        /// </summary>
        public Button CreateButton(string buttonId, string text, Canvas parentCanvas = null,
                                  Vector2? position = null, Vector2? size = null)
        {
            if (parentCanvas == null)
                parentCanvas = screenSpaceCanvas ?? GetOrCreateCanvas("DefaultCanvas");

            GameObject buttonGO = new GameObject($"Button_{buttonId}");
            buttonGO.transform.SetParent(parentCanvas.transform, false);

            // Add Image component for button background
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.sprite = uiSettings.defaultButtonSprite;
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = uiSettings.defaultButtonColor;

            // Add Button component
            Button button = buttonGO.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Configure button colors
            ColorBlock colors = button.colors;
            colors.normalColor = uiSettings.defaultButtonColor;
            colors.highlightedColor = uiSettings.buttonHighlightColor;
            colors.pressedColor = uiSettings.buttonPressedColor;
            colors.disabledColor = uiSettings.buttonDisabledColor;
            button.colors = colors;

            // Add text child
            if (!string.IsNullOrEmpty(text))
            {
                GameObject textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform, false);
                
                TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
                ConfigureLabel(buttonText, text);
                
                // Make text fill the button
                RectTransform textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }

            // Configure button RectTransform
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.sizeDelta = size ?? uiSettings.defaultButtonSize;

            if (position.HasValue)
            {
                buttonRect.anchoredPosition = position.Value;
                buttonRect.anchorMin = Vector2.zero;
                buttonRect.anchorMax = Vector2.zero;
            }
            else
            {
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.anchoredPosition = Vector2.zero;
            }

            uiElementRegistry[buttonId] = buttonGO;

            if (enableDebugMode)
            {
                Debug.Log($"[UIManager] Created button: {buttonId} - '{text}'");
            }

            return button;
        }
        #endregion

        #region Panel Creation
        /// <summary>
        /// Creates a panel (container) for organizing UI elements
        /// </summary>
        public RectTransform CreatePanel(string panelId, Canvas parentCanvas = null,
                                       Vector2? position = null, Vector2? size = null,
                                       Color? backgroundColor = null)
        {
            if (parentCanvas == null)
                parentCanvas = screenSpaceCanvas ?? GetOrCreateCanvas("DefaultCanvas");

            GameObject panelGO = new GameObject($"Panel_{panelId}");
            panelGO.transform.SetParent(parentCanvas.transform, false);

            // Add Image component for background
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.sprite = uiSettings.defaultPanelSprite;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = backgroundColor ?? uiSettings.defaultPanelColor;

            // Configure RectTransform
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.sizeDelta = size ?? uiSettings.defaultPanelSize;

            if (position.HasValue)
            {
                panelRect.anchoredPosition = position.Value;
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.zero;
            }
            else
            {
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
            }

            uiElementRegistry[panelId] = panelGO;

            if (enableDebugMode)
            {
                Debug.Log($"[UIManager] Created panel: {panelId}");
            }

            return panelRect;
        }
        #endregion

        #region UI Element Management
        /// <summary>
        /// Updates an existing label's text
        /// </summary>
        public void UpdateLabelText(string labelId, string newText)
        {
            if (uiElementRegistry.TryGetValue(labelId, out GameObject labelGO))
            {
                TextMeshProUGUI label = labelGO.GetComponent<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = newText;
                }
            }
            else
            {
                Debug.LogWarning($"[UIManager] Label '{labelId}' not found");
            }
        }

        /// <summary>
        /// Sets the visibility of a UI element
        /// </summary>
        public void SetElementVisibility(string elementId, bool visible)
        {
            if (uiElementRegistry.TryGetValue(elementId, out GameObject element))
            {
                element.SetActive(visible);
            }
            else
            {
                Debug.LogWarning($"[UIManager] UI element '{elementId}' not found");
            }
        }

        /// <summary>
        /// Destroys a UI element
        /// </summary>
        public void DestroyElement(string elementId)
        {
            if (uiElementRegistry.TryGetValue(elementId, out GameObject element))
            {
                uiElementRegistry.Remove(elementId);
                Destroy(element);
                
                if (enableDebugMode)
                {
                    Debug.Log($"[UIManager] Destroyed UI element: {elementId}");
                }
            }
        }

        /// <summary>
        /// Gets a UI element by ID
        /// </summary>
        public GameObject GetUIElement(string elementId)
        {
            uiElementRegistry.TryGetValue(elementId, out GameObject element);
            return element;
        }

        /// <summary>
        /// Gets a UI element component by ID
        /// </summary>
        public T GetUIElement<T>(string elementId) where T : Component
        {
            if (uiElementRegistry.TryGetValue(elementId, out GameObject element))
            {
                return element.GetComponent<T>();
            }
            return null;
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Clears all dynamically created UI elements
        /// </summary>
        public void ClearAllElements()
        {
            foreach (var kvp in uiElementRegistry)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
            uiElementRegistry.Clear();

            if (enableDebugMode)
            {
                Debug.Log("[UIManager] Cleared all UI elements");
            }
        }

        /// <summary>
        /// Shows a temporary message that fades out
        /// </summary>
        public void ShowTemporaryMessage(string message, float duration = 3f, Canvas targetCanvas = null)
        {
            StartCoroutine(ShowTemporaryMessageCoroutine(message, duration, targetCanvas));
        }

        private IEnumerator ShowTemporaryMessageCoroutine(string message, float duration, Canvas targetCanvas)
        {
            string tempId = $"temp_msg_{System.Guid.NewGuid()}";
            TextMeshProUGUI label = CreateLabel(tempId, message, targetCanvas ?? overlayCanvas);
            
            CanvasGroup canvasGroup = label.gameObject.AddComponent<CanvasGroup>();
            
            // Fade in
            float fadeInTime = 0.5f;
            float elapsed = 0f;
            while (elapsed < fadeInTime)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = elapsed / fadeInTime;
                yield return null;
            }

            // Wait
            yield return new WaitForSecondsRealtime(duration - 1f); // Subtract fade times

            // Fade out
            float fadeOutTime = 0.5f;
            elapsed = 0f;
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - (elapsed / fadeOutTime);
                yield return null;
            }

            DestroyElement(tempId);
        }
        #endregion

        #region Debug
        /// <summary>
        /// Creates a debug HUD for testing
        /// </summary>
        [ContextMenu("Create Debug HUD")]
        public void CreateDebugHUD()
        {
            Canvas debugCanvas = GetOrCreateCanvas("DebugCanvas", RenderMode.ScreenSpaceOverlay);
            debugCanvas.sortingOrder = 999;

            // Create debug panel
            RectTransform debugPanel = CreatePanel("debug_panel", debugCanvas, 
                new Vector2(-400, 300), new Vector2(300, 400), new Color(0, 0, 0, 0.7f));

            // Create some debug labels
            CreateLabel("debug_fps", "FPS: 60", debugCanvas, new Vector2(-400, 350), new Vector2(200, 30));
            CreateLabel("debug_time", "Time: 0.0", debugCanvas, new Vector2(-400, 320), new Vector2(200, 30));
            CreateLabel("debug_status", "Status: Running", debugCanvas, new Vector2(-400, 290), new Vector2(200, 30));

            // Create test button
            Button testButton = CreateButton("debug_button", "Test Button", debugCanvas, 
                new Vector2(-400, 200), new Vector2(150, 40));
            testButton.onClick.AddListener(() => ShowTemporaryMessage("Button clicked!", 2f));

            Debug.Log("[UIManager] Debug HUD created");
        }
        #endregion
    }
}

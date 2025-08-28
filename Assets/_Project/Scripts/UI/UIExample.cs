using UnityEngine;
using UnityEngine.InputSystem;
using ProjectZero.UI;
using TMPro;
using UnityEngine.UI;

namespace ProjectZero.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the UIManager for programmatic UI creation
    /// This shows various use cases for dynamic UI generation
    /// </summary>
    public class UIExample : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private bool createExampleUI = true;
        [SerializeField] private bool createGameHUD = true;
        [SerializeField] private bool createWorldLabels = true;
        
        [Header("Test Targets")]
        [SerializeField] private Transform[] worldTargets;
        
        private UIManager uiManager;
        private float gameTime = 0f;
        private int score = 0;

        #region Unity Lifecycle
        private void Start()
        {
            // Get the UI Manager instance
            uiManager = UIManager.Instance;
            
            // Wait a frame for UIManager to initialize
            Invoke(nameof(SetupExampleUI), 0.1f);
        }
        
        private void Update()
        {
            // Update game time and score for demo
            gameTime += Time.deltaTime;
            score = Mathf.FloorToInt(gameTime * 10);
            
            // Update HUD elements
            UpdateGameHUD();
            
            // Handle input for demonstrations
            HandleExampleInput();
        }
        #endregion

        #region Example Setup
        private void SetupExampleUI()
        {
            if (createExampleUI)
            {
                CreateExampleUI();
            }
            
            if (createGameHUD)
            {
                CreateGameHUD();
            }
            
            if (createWorldLabels)
            {
                CreateWorldLabels();
            }
        }

        /// <summary>
        /// Creates basic UI elements for demonstration
        /// </summary>
        private void CreateExampleUI()
        {
            // Create a main menu panel
            RectTransform menuPanel = uiManager.CreatePanel("main_menu", null, 
                new Vector2(0, 0), new Vector2(300, 400));

            // Create title label
            TextMeshProUGUI titleLabel = uiManager.CreateLabel("menu_title", "ProjectZero UI Demo", 
                menuPanel.GetComponent<Canvas>(), new Vector2(0, 150), new Vector2(280, 50));
            titleLabel.fontSize = 24;
            titleLabel.fontStyle = FontStyles.Bold;

            // Create menu buttons
            Button startButton = uiManager.CreateButton("start_button", "Start Game", 
                menuPanel.GetComponent<Canvas>(), new Vector2(0, 50), new Vector2(200, 40));
            startButton.onClick.AddListener(() => {
                uiManager.ShowTemporaryMessage("Game Started!", 2f);
                uiManager.SetElementVisibility("main_menu", false);
            });

            Button optionsButton = uiManager.CreateButton("options_button", "Options", 
                menuPanel.GetComponent<Canvas>(), new Vector2(0, 0), new Vector2(200, 40));
            optionsButton.onClick.AddListener(() => {
                uiManager.ShowTemporaryMessage("Options not implemented yet", 1.5f);
            });

            Button exitButton = uiManager.CreateButton("exit_button", "Exit", 
                menuPanel.GetComponent<Canvas>(), new Vector2(0, -50), new Vector2(200, 40));
            exitButton.onClick.AddListener(() => {
                uiManager.ShowTemporaryMessage("Exiting...", 1f);
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });

            Debug.Log("[UIExample] Example UI created");
        }

        /// <summary>
        /// Creates a game HUD with real-time information
        /// </summary>
        private void CreateGameHUD()
        {
            // Create HUD canvas with high sorting order
            Canvas hudCanvas = uiManager.GetOrCreateCanvas("HUD_Canvas", RenderMode.ScreenSpaceOverlay);
            hudCanvas.sortingOrder = 10;

            // Create HUD panel in top-left
            RectTransform hudPanel = uiManager.CreatePanel("hud_panel", hudCanvas,
                new Vector2(-650, 350), new Vector2(300, 150), new Color(0, 0, 0, 0.3f));

            // Create HUD labels
            uiManager.CreateLabel("hud_time", "Time: 0.00", hudCanvas, 
                new Vector2(-650, 380), new Vector2(280, 30));
            uiManager.CreateLabel("hud_score", "Score: 0", hudCanvas, 
                new Vector2(-650, 350), new Vector2(280, 30));
            uiManager.CreateLabel("hud_fps", "FPS: 60", hudCanvas, 
                new Vector2(-650, 320), new Vector2(280, 30));

            // Create action buttons in bottom-right
            uiManager.CreateButton("action_pause", "Pause", hudCanvas, 
                new Vector2(600, -350), new Vector2(100, 40));
            uiManager.CreateButton("action_reset", "Reset", hudCanvas, 
                new Vector2(600, -400), new Vector2(100, 40));

            Debug.Log("[UIExample] Game HUD created");
        }

        /// <summary>
        /// Creates world-space labels that follow 3D objects
        /// </summary>
        private void CreateWorldLabels()
        {
            if (worldTargets == null || worldTargets.Length == 0)
            {
                // Create some demo targets if none exist
                CreateDemoTargets();
            }

            for (int i = 0; i < worldTargets.Length; i++)
            {
                if (worldTargets[i] != null)
                {
                    string labelId = $"world_label_{i}";
                    string labelText = $"Target {i + 1}";
                    Vector3 offset = new Vector3(0, 2, 0); // Above the target
                    
                    TextMeshProUGUI worldLabel = uiManager.CreateWorldLabel(labelId, labelText, 
                        worldTargets[i], offset);
                    worldLabel.fontSize = 14;
                    worldLabel.color = Color.yellow;
                }
            }

            Debug.Log($"[UIExample] Created {worldTargets.Length} world labels");
        }

        private void CreateDemoTargets()
        {
            worldTargets = new Transform[3];
            
            for (int i = 0; i < 3; i++)
            {
                GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
                target.name = $"Demo Target {i + 1}";
                target.transform.position = new Vector3(i * 3f - 3f, 0, 5f);
                target.GetComponent<Renderer>().material.color = Random.ColorHSV();
                worldTargets[i] = target.transform;
            }
        }
        #endregion

        #region HUD Updates
        private void UpdateGameHUD()
        {
            // Update time
            uiManager.UpdateLabelText("hud_time", $"Time: {gameTime:F2}");
            
            // Update score
            uiManager.UpdateLabelText("hud_score", $"Score: {score}");
            
            // Update FPS
            float fps = 1.0f / Time.deltaTime;
            uiManager.UpdateLabelText("hud_fps", $"FPS: {fps:F0}");
        }
        #endregion

        #region Input Handling
        private void HandleExampleInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Press H to toggle HUD visibility
            if (keyboard.hKey.wasPressedThisFrame)
            {
                Canvas hudCanvas = uiManager.GetOrCreateCanvas("HUD_Canvas");
                hudCanvas.gameObject.SetActive(!hudCanvas.gameObject.activeSelf);
            }
            
            // Press M to toggle main menu
            if (keyboard.mKey.wasPressedThisFrame)
            {
                GameObject menuPanel = uiManager.GetUIElement("main_menu");
                if (menuPanel != null)
                {
                    menuPanel.SetActive(!menuPanel.activeSelf);
                }
            }
            
            // Press T for temporary message
            if (keyboard.tKey.wasPressedThisFrame)
            {
                uiManager.ShowTemporaryMessage($"Random message #{Random.Range(1, 100)}", 2f);
            }
            
            // Press C to clear all UI elements
            if (keyboard.cKey.wasPressedThisFrame)
            {
                uiManager.ClearAllElements();
                uiManager.ShowTemporaryMessage("All UI elements cleared!", 2f);
            }
        }
        #endregion

        #region Demo Methods
        /// <summary>
        /// Creates a notification popup
        /// </summary>
        public void ShowNotification(string message, float duration = 3f)
        {
            Canvas notificationCanvas = uiManager.GetOrCreateCanvas("NotificationCanvas");
            notificationCanvas.sortingOrder = 999;
            
            string notificationId = $"notification_{System.DateTime.Now.Ticks}";
            
            // Create notification panel
            RectTransform notificationPanel = uiManager.CreatePanel(notificationId + "_panel", 
                notificationCanvas, new Vector2(0, 200), new Vector2(400, 80), 
                new Color(0.2f, 0.2f, 0.2f, 0.9f));
            
            // Create notification text
            TextMeshProUGUI notificationText = uiManager.CreateLabel(notificationId + "_text", 
                message, notificationCanvas, new Vector2(0, 200), new Vector2(380, 60));
            notificationText.fontSize = 16;
            notificationText.alignment = TMPro.TextAlignmentOptions.Center;
            
            // Auto-destroy after duration
            StartCoroutine(DestroyNotificationAfterDelay(notificationId, duration));
        }
        
        private System.Collections.IEnumerator DestroyNotificationAfterDelay(string notificationId, float delay)
        {
            yield return new WaitForSeconds(delay);
            uiManager.DestroyElement(notificationId + "_panel");
            uiManager.DestroyElement(notificationId + "_text");
        }

        /// <summary>
        /// Creates a health bar above a target
        /// </summary>
        public void CreateHealthBar(Transform target, float currentHealth, float maxHealth)
        {
            string healthBarId = $"health_bar_{target.GetInstanceID()}";
            
            Canvas worldCanvas = uiManager.WorldCanvas;
            if (worldCanvas == null)
            {
                worldCanvas = uiManager.GetOrCreateCanvas("WorldCanvas", RenderMode.WorldSpace);
            }
            
            // Create health bar background
            RectTransform healthBarBG = uiManager.CreatePanel(healthBarId + "_bg", worldCanvas,
                Vector2.zero, new Vector2(100, 10), Color.red);
            
            // Create health bar fill
            RectTransform healthBarFill = uiManager.CreatePanel(healthBarId + "_fill", worldCanvas,
                Vector2.zero, new Vector2(100 * (currentHealth / maxHealth), 10), Color.green);
            
            // Add world tracker to both
            WorldLabelTracker bgTracker = healthBarBG.gameObject.AddComponent<WorldLabelTracker>();
            bgTracker.Initialize(target, new Vector3(0, 2.5f, 0), UnityEngine.Camera.main);
            
            WorldLabelTracker fillTracker = healthBarFill.gameObject.AddComponent<WorldLabelTracker>();
            fillTracker.Initialize(target, new Vector3(0, 2.5f, 0), UnityEngine.Camera.main);
        }
        #endregion

        #region Context Menu Actions
        [ContextMenu("Create Debug HUD")]
        private void CreateDebugHUD()
        {
            uiManager.CreateDebugHUD();
        }
        
        [ContextMenu("Show Test Notification")]
        private void ShowTestNotification()
        {
            ShowNotification("This is a test notification!", 3f);
        }
        
        [ContextMenu("Clear All UI")]
        private void ClearAllUI()
        {
            uiManager.ClearAllElements();
        }
        #endregion
    }
}

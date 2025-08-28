using UnityEngine;
using UnityEngine.InputSystem;
using ProjectZero.UI;
using TMPro;
using UnityEngine.UI;

namespace ProjectZero.Examples
{
    /// <summary>
    /// Quick start example - Shows the simplest way to use the UI system
    /// Just attach this to any GameObject and press Play!
    /// </summary>
    public class QuickStartUI : MonoBehaviour
    {
        [Header("Quick Test Settings")]
        [SerializeField] private bool createTestUI = true;
        
        private UIManager uiManager;

        void Start()
        {
            if (createTestUI)
            {
                // Wait a frame for UIManager to initialize
                Invoke(nameof(CreateTestUI), 0.1f);
            }
        }

        void CreateTestUI()
        {
            // Get the UI Manager (auto-creates if needed)
            uiManager = UIManager.Instance;

            // 1. CREATE A SIMPLE LABEL
            TextMeshProUGUI myLabel = uiManager.CreateLabel("test_label", "Hello ProjectZero!");
            myLabel.fontSize = 24;
            myLabel.color = Color.yellow;

            // 2. CREATE A BUTTON
            Button myButton = uiManager.CreateButton("test_button", "Click Me!", null, 
                new Vector2(0, -50)); // Position it below the label
            
            myButton.onClick.AddListener(() => {
                uiManager.ShowTemporaryMessage("Button Clicked! 🎉", 2f);
                Debug.Log("UI Button was clicked!");
            });

            // 3. CREATE A PANEL (CONTAINER)
            RectTransform panel = uiManager.CreatePanel("test_panel", null,
                new Vector2(300, 200), new Vector2(250, 150), new Color(0, 0, 0, 0.5f));

            // 4. CREATE LABELS INSIDE THE PANEL
            Canvas panelCanvas = panel.GetComponent<Canvas>();
            uiManager.CreateLabel("panel_title", "Info Panel", panelCanvas, 
                new Vector2(300, 250), new Vector2(200, 30));
            uiManager.CreateLabel("panel_content", "This is content inside the panel", panelCanvas,
                new Vector2(300, 200), new Vector2(200, 60));

            Debug.Log("✅ Test UI Created! Check the screen!");
        }

        // Update labels in real-time (optional)
        void Update()
        {
            if (uiManager != null)
            {
                // Update a label with real-time data
                float time = Time.time;
                uiManager.UpdateLabelText("test_label", $"Time: {time:F1}s");
            }

            // Press SPACE for temporary message
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                uiManager?.ShowTemporaryMessage("Space key pressed!", 1.5f);
            }
        }
    }
}

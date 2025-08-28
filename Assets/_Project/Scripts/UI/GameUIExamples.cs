using UnityEngine;
using UnityEngine.InputSystem;
using ProjectZero.UI;
using TMPro;
using UnityEngine.UI;

namespace ProjectZero.Examples
{
    /// <summary>
    /// Practical examples for using the UI system in your RTS extraction shooter
    /// Shows common game UI patterns and how to implement them
    /// </summary>
    public class GameUIExamples : MonoBehaviour
    {
        [Header("Example Controls")]
        [SerializeField] private bool createGameHUD = true;
        [SerializeField] private bool createUnitLabels = true;
        [SerializeField] private bool createMissionUI = true;
        
        [Header("Test Objects (optional)")]
        [SerializeField] private Transform[] squadMembers;
        [SerializeField] private Transform extractionPoint;
        
        private UIManager uiManager;
        private float gameTimer = 0f;
        private int score = 0;
        private int health = 100;
        private int ammo = 30;

        void Start()
        {
            uiManager = UIManager.Instance;
            Invoke(nameof(SetupGameUI), 0.1f);
        }

        void SetupGameUI()
        {
            if (createGameHUD) CreateGameHUD();
            if (createUnitLabels) CreateUnitLabels();
            if (createMissionUI) CreateMissionUI();
            
            Debug.Log("[GAME] Game UI Examples Created!");
        }

        void Update()
        {
            UpdateGameStats();
            HandleTestInputs();
        }

        #region GAME HUD EXAMPLE
        void CreateGameHUD()
        {
            // Create main HUD canvas
            Canvas hudCanvas = uiManager.GetOrCreateCanvas("GameHUD", RenderMode.ScreenSpaceOverlay);
            hudCanvas.sortingOrder = 10;

            // TOP-LEFT: Player Stats Panel
            RectTransform statsPanel = uiManager.CreatePanel("stats_panel", hudCanvas,
                new Vector2(-750, 400), new Vector2(200, 120), new Color(0, 0, 0, 0.6f));

            uiManager.CreateLabel("health_label", "Health: 100", hudCanvas,
                new Vector2(-750, 430), new Vector2(180, 25));
            uiManager.CreateLabel("ammo_label", "Ammo: 30/120", hudCanvas,
                new Vector2(-750, 405), new Vector2(180, 25));
            uiManager.CreateLabel("score_label", "Score: 0", hudCanvas,
                new Vector2(-750, 380), new Vector2(180, 25));

            // TOP-RIGHT: Mission Timer
            uiManager.CreateLabel("timer_label", "Time: 00:00", hudCanvas,
                new Vector2(750, 430), new Vector2(150, 30));

            // BOTTOM-RIGHT: Action Buttons
            Button tacticalButton = uiManager.CreateButton("tactical_btn", "Tactical Mode", hudCanvas,
                new Vector2(650, -400), new Vector2(120, 35));
            tacticalButton.onClick.AddListener(ToggleTacticalMode);

            Button extractButton = uiManager.CreateButton("extract_btn", "Extract", hudCanvas,
                new Vector2(780, -400), new Vector2(80, 35));
            extractButton.onClick.AddListener(StartExtraction);

            Debug.Log("✅ Game HUD created!");
        }

        void UpdateGameStats()
        {
            // Update game timer
            gameTimer += Time.deltaTime;
            int minutes = Mathf.FloorToInt(gameTimer / 60);
            int seconds = Mathf.FloorToInt(gameTimer % 60);
            uiManager.UpdateLabelText("timer_label", $"Time: {minutes:00}:{seconds:00}");

            // Update stats (simulate changing values)
            uiManager.UpdateLabelText("health_label", $"Health: {health}");
            uiManager.UpdateLabelText("ammo_label", $"Ammo: {ammo}/120");
            uiManager.UpdateLabelText("score_label", $"Score: {score}");
        }
        #endregion

        #region UNIT LABELS EXAMPLE
        void CreateUnitLabels()
        {
            // Create demo squad members if none assigned
            if (squadMembers == null || squadMembers.Length == 0)
            {
                CreateDemoSquadMembers();
            }

            // Create labels for each squad member
            for (int i = 0; i < squadMembers.Length; i++)
            {
                if (squadMembers[i] != null)
                {
                    string memberId = $"squad_{i}";
                    string memberName = $"Alpha-{i + 1}";
                    Vector3 labelOffset = new Vector3(0, 2.5f, 0); // Above the unit

                    TextMeshProUGUI unitLabel = uiManager.CreateWorldLabel(memberId, memberName, 
                        squadMembers[i], labelOffset);
                    
                    // Style the unit label
                    unitLabel.fontSize = 12;
                    unitLabel.color = Color.cyan;
                    unitLabel.alignment = TextAlignmentOptions.Center;
                    
                    // Optional: Add health bar
                    CreateSimpleHealthBar(squadMembers[i], i);
                }
            }

            Debug.Log($"✅ Created labels for {squadMembers.Length} squad members!");
        }

        void CreateDemoSquadMembers()
        {
            squadMembers = new Transform[3];
            
            for (int i = 0; i < 3; i++)
            {
                GameObject member = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                member.name = $"Squad Member {i + 1}";
                member.transform.position = new Vector3(i * 2f - 2f, 0, 2f);
                
                // Random color for visibility
                member.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f);
                
                squadMembers[i] = member.transform;
            }
        }

        void CreateSimpleHealthBar(Transform target, int index)
        {
            string healthBarId = $"health_bar_{index}";
            Vector3 barOffset = new Vector3(0, 2.8f, 0); // Slightly above the name

            Canvas worldCanvas = uiManager.WorldCanvas;
            
            // Health bar background (red) - much smaller size
            RectTransform healthBG = uiManager.CreatePanel($"{healthBarId}_bg", worldCanvas,
                Vector2.zero, new Vector2(20, 3), Color.red);
            
            // Health bar fill (green) - much smaller size  
            RectTransform healthFill = uiManager.CreatePanel($"{healthBarId}_fill", worldCanvas,
                Vector2.zero, new Vector2(20, 3), Color.green);
            
            // Apply world space scaling to health bars
            healthBG.localScale = Vector3.one * 0.02f; // Same as world label scale
            healthFill.localScale = Vector3.one * 0.02f;
            
            // Add trackers to follow the target
            WorldLabelTracker bgTracker = healthBG.gameObject.AddComponent<WorldLabelTracker>();
            bgTracker.Initialize(target, barOffset, UnityEngine.Camera.main);
            
            WorldLabelTracker fillTracker = healthFill.gameObject.AddComponent<WorldLabelTracker>();
            fillTracker.Initialize(target, barOffset, UnityEngine.Camera.main);
        }
        #endregion

        #region MISSION UI EXAMPLE
        void CreateMissionUI()
        {
            Canvas missionCanvas = uiManager.GetOrCreateCanvas("MissionUI", RenderMode.ScreenSpaceOverlay);
            missionCanvas.sortingOrder = 5;

            // Mission Objectives Panel (LEFT SIDE)
            RectTransform objectivesPanel = uiManager.CreatePanel("objectives_panel", missionCanvas,
                new Vector2(-750, 0), new Vector2(250, 200), new Color(0.1f, 0.1f, 0.1f, 0.8f));

            uiManager.CreateLabel("objectives_title", "OBJECTIVES", missionCanvas,
                new Vector2(-750, 80), new Vector2(230, 30));

            // Individual objectives
            uiManager.CreateLabel("obj1", "• Secure the area", missionCanvas,
                new Vector2(-750, 50), new Vector2(220, 20));
            uiManager.CreateLabel("obj2", "• Eliminate hostiles", missionCanvas,
                new Vector2(-750, 30), new Vector2(220, 20));
            uiManager.CreateLabel("obj3", "• Reach extraction point", missionCanvas,
                new Vector2(-750, 10), new Vector2(220, 20));

            // Create extraction point marker if we have one
            if (extractionPoint != null)
            {
                CreateExtractionMarker();
            }

            Debug.Log("✅ Mission UI created!");
        }

        void CreateExtractionMarker()
        {
            TextMeshProUGUI extractionLabel = uiManager.CreateWorldLabel("extraction_marker", 
                "[LZ] EXTRACTION", extractionPoint, Vector3.up * 3f);
            
            extractionLabel.fontSize = 16;
            extractionLabel.color = Color.green;
            extractionLabel.fontStyle = FontStyles.Bold;
        }
        #endregion

        #region INPUT HANDLERS & GAME ACTIONS
        void HandleTestInputs()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Press H to toggle HUD
            if (keyboard.hKey.wasPressedThisFrame)
            {
                Canvas hud = uiManager.GetOrCreateCanvas("GameHUD");
                hud.gameObject.SetActive(!hud.gameObject.activeSelf);
                uiManager.ShowTemporaryMessage("HUD Toggled", 1f);
            }

            // Press T for tactical message
            if (keyboard.tKey.wasPressedThisFrame)
            {
                uiManager.ShowTemporaryMessage("Entering Tactical Mode...", 2f);
            }

            // Press E near extraction (simulate)
            if (keyboard.eKey.wasPressedThisFrame)
            {
                ShowExtractionDialog();
            }

            // Simulate taking damage
            if (keyboard.xKey.wasPressedThisFrame)
            {
                TakeDamage();
            }

            // Add score
            if (keyboard.zKey.wasPressedThisFrame)
            {
                AddScore(100);
            }
        }

        void ToggleTacticalMode()
        {
            uiManager.ShowTemporaryMessage("Tactical Mode Activated", 2f);
            // Here you would integrate with your time dilation system
            Debug.Log("[TACTICAL] Tactical Mode Toggle");
        }

        void StartExtraction()
        {
            uiManager.ShowTemporaryMessage("Calling for extraction...", 3f);
            
            // Create extraction countdown
            StartCoroutine(ExtractionCountdown());
            Debug.Log("[EXTRACTION] Extraction Started");
        }

        System.Collections.IEnumerator ExtractionCountdown()
        {
            for (int i = 10; i > 0; i--)
            {
                uiManager.ShowTemporaryMessage($"Extraction in {i}...", 1f);
                yield return new WaitForSeconds(1f);
            }
            
            uiManager.ShowTemporaryMessage("[SUCCESS] MISSION COMPLETE!", 3f);
        }

        void TakeDamage()
        {
            health -= 10;
            if (health < 0) health = 0;
            
            uiManager.ShowTemporaryMessage($"[HIT] Damage Taken! Health: {health}", 1.5f);
        }

        void AddScore(int points)
        {
            score += points;
            uiManager.ShowTemporaryMessage($"[SCORE] +{points} points!", 1.5f);
        }

        void ShowExtractionDialog()
        {
            // Create a temporary dialog
            Canvas dialogCanvas = uiManager.GetOrCreateCanvas("DialogCanvas");
            dialogCanvas.sortingOrder = 999;

            RectTransform dialog = uiManager.CreatePanel("extraction_dialog", dialogCanvas,
                Vector2.zero, new Vector2(400, 200), new Color(0.2f, 0.2f, 0.2f, 0.95f));

            uiManager.CreateLabel("dialog_title", "EXTRACTION AVAILABLE", dialogCanvas,
                new Vector2(0, 50), new Vector2(350, 30));
            uiManager.CreateLabel("dialog_text", "Do you want to extract now?", dialogCanvas,
                new Vector2(0, 0), new Vector2(350, 30));

            Button yesBtn = uiManager.CreateButton("dialog_yes", "Yes", dialogCanvas,
                new Vector2(-80, -50), new Vector2(70, 30));
            Button noBtn = uiManager.CreateButton("dialog_no", "No", dialogCanvas,
                new Vector2(80, -50), new Vector2(70, 30));

            yesBtn.onClick.AddListener(() => {
                StartExtraction();
                CloseDialog();
            });

            noBtn.onClick.AddListener(CloseDialog);

            // Auto-close after 5 seconds
            Invoke(nameof(CloseDialog), 5f);
        }

        void CloseDialog()
        {
            uiManager.DestroyElement("extraction_dialog");
            uiManager.DestroyElement("dialog_title");
            uiManager.DestroyElement("dialog_text");
            uiManager.DestroyElement("dialog_yes");
            uiManager.DestroyElement("dialog_no");
        }
        #endregion

        #region Context Menu for Testing
        [ContextMenu("Create Demo Extraction Point")]
        void CreateDemoExtractionPoint()
        {
            if (extractionPoint == null)
            {
                GameObject extraction = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                extraction.name = "Extraction Point";
                extraction.transform.position = new Vector3(0, 0, 10);
                extraction.GetComponent<Renderer>().material.color = Color.green;
                extraction.transform.localScale = new Vector3(3, 0.1f, 3);
                
                extractionPoint = extraction.transform;
                CreateExtractionMarker();
            }
        }

        [ContextMenu("Test All UI Features")]
        void TestAllFeatures()
        {
            uiManager.ShowTemporaryMessage("Testing all UI features!", 2f);
            TakeDamage();
            AddScore(250);
        }
        #endregion
    }
}

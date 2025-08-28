using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectZero.Camera
{
    /// <summary>
    /// Quick setup script to configure camera system with working WASD controls.
    /// Attach this to your camera system and click "Setup Camera" button in Inspector.
    /// </summary>
    public class CameraQuickSetup : MonoBehaviour
    {
        [Header("Setup Configuration")]
        [SerializeField] private bool createCameraSettings = true;
        [SerializeField] private bool setupInputActions = true;
        [SerializeField] private bool createCameraTarget = true;
        [SerializeField] private bool setupCinemachine = true;

        [Header("Manual References (Optional)")]
        [SerializeField] private CameraSettings existingSettings;
        [SerializeField] private InputActionAsset existingInputActions;

        [Header("Setup Result")]
        [SerializeField] private TacticalCameraController configuredController;

        /// <summary>
        /// Main setup method - call this to configure everything
        /// </summary>
        [ContextMenu("Setup Camera System")]
        public void SetupCameraSystem()
        {
            Debug.Log("[CameraQuickSetup] Starting camera system setup...");

            // Step 1: Create or find camera settings
            CameraSettings settings = GetOrCreateCameraSettings();
            
            // Step 2: Create camera target
            Transform cameraTarget = GetOrCreateCameraTarget();
            
            // Step 3: Create Cinemachine camera
            CinemachineCamera cmCamera = GetOrCreateCinemachineCamera(cameraTarget);
            
            // Step 4: Setup TacticalCameraController
            TacticalCameraController controller = SetupTacticalController(settings, cameraTarget, cmCamera);
            
            // Step 5: Configure input actions
            SetupInputActions(controller);
            
            // Store reference
            configuredController = controller;
            
            Debug.Log("[CameraQuickSetup] ✅ Camera system setup complete!");
            Debug.Log("💡 Test with WASD keys, mouse wheel for zoom, and screen edges for scrolling");
        }

        private CameraSettings GetOrCreateCameraSettings()
        {
            if (existingSettings != null)
            {
                Debug.Log("[CameraQuickSetup] Using existing camera settings");
                return existingSettings;
            }

            if (!createCameraSettings)
            {
                Debug.LogWarning("[CameraQuickSetup] No camera settings provided and creation disabled");
                return null;
            }

            // Create new settings asset
            var settings = ScriptableObject.CreateInstance<CameraSettings>();
            
#if UNITY_EDITOR
            // Save as asset in editor
            string path = "Assets/_Project/Settings/DefaultCameraSettings.asset";
            UnityEditor.AssetDatabase.CreateAsset(settings, path);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[CameraQuickSetup] Created CameraSettings at {path}");
#endif
            
            return settings;
        }

        private Transform GetOrCreateCameraTarget()
        {
            // Look for existing camera target
            var existing = GameObject.Find("CameraTarget");
            if (existing != null)
            {
                Debug.Log("[CameraQuickSetup] Using existing CameraTarget");
                return existing.transform;
            }

            if (!createCameraTarget)
            {
                Debug.LogWarning("[CameraQuickSetup] No camera target found and creation disabled");
                return null;
            }

            // Create new camera target
            var cameraTarget = new GameObject("CameraTarget");
            cameraTarget.transform.position = Vector3.zero;
            Debug.Log("[CameraQuickSetup] Created CameraTarget at origin");
            
            return cameraTarget.transform;
        }

        private CinemachineCamera GetOrCreateCinemachineCamera(Transform cameraTarget)
        {
            // Look for existing Cinemachine camera
            var existing = FindFirstObjectByType<CinemachineCamera>();
            if (existing != null)
            {
                Debug.Log("[CameraQuickSetup] Using existing CinemachineCamera");
                return existing;
            }

            if (!setupCinemachine)
            {
                Debug.LogWarning("[CameraQuickSetup] No Cinemachine camera found and creation disabled");
                return null;
            }

            // Create new Cinemachine camera
            var cmCameraGO = new GameObject("CM TacticalCamera");
            var cmCamera = cmCameraGO.AddComponent<CinemachineCamera>();
            
            // Configure for tactical view
            cmCamera.Target.TrackingTarget = cameraTarget;
            cmCamera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            
            // Add position composer for zoom control
            var positionComposer = cmCameraGO.AddComponent<CinemachinePositionComposer>();
            positionComposer.CameraDistance = 25f;
            positionComposer.TargetOffset = Vector3.zero;
            
            Debug.Log("[CameraQuickSetup] Created Cinemachine Camera with tactical angle");
            
            return cmCamera;
        }

        private TacticalCameraController SetupTacticalController(CameraSettings settings, Transform cameraTarget, CinemachineCamera cmCamera)
        {
            // Get or add TacticalCameraController
            var controller = GetComponent<TacticalCameraController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<TacticalCameraController>();
                Debug.Log("[CameraQuickSetup] Added TacticalCameraController");
            }

            // Configure references using reflection (since fields are private)
            var settingsField = typeof(TacticalCameraController).GetField("settings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var targetField = typeof(TacticalCameraController).GetField("cameraTarget", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cameraField = typeof(TacticalCameraController).GetField("virtualCamera", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            settingsField?.SetValue(controller, settings);
            targetField?.SetValue(controller, cameraTarget);
            cameraField?.SetValue(controller, cmCamera);

            // Add all camera components
            EnsureCameraComponent<CameraKeyboardMovement>();
            EnsureCameraComponent<CameraZoomControl>();
            EnsureCameraComponent<CameraEdgeScrolling>();
            EnsureCameraComponent<CameraFollowTarget>();
            EnsureCameraComponent<CameraConstraints>();
            EnsureCameraComponent<CameraInputHandler>();

            Debug.Log("[CameraQuickSetup] Configured TacticalCameraController with all components");
            
            return controller;
        }

        private T EnsureCameraComponent<T>() where T : Component
        {
            var component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
                Debug.Log($"[CameraQuickSetup] Added {typeof(T).Name}");
            }
            return component;
        }

        private void SetupInputActions(TacticalCameraController controller)
        {
            if (!setupInputActions)
            {
                Debug.Log("[CameraQuickSetup] Input Actions setup skipped");
                return;
            }

            // Load the input actions asset
            InputActionAsset inputAsset = existingInputActions;
            
#if UNITY_EDITOR
            if (inputAsset == null)
            {
                inputAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                    "Assets/_Project/Input/CameraControls.inputactions");
            }
#endif

            if (inputAsset == null)
            {
                Debug.LogWarning("[CameraQuickSetup] Could not find CameraControls.inputactions - WASD won't work until configured");
                return;
            }

            Debug.Log("[CameraQuickSetup] ⚠️ IMPORTANT: You need to manually assign Input Action References in Inspector:");
            Debug.Log("1. Select the CameraInputHandler component");
            Debug.Log("2. Assign Movement Action: CameraControls/CameraMap/Movement");
            Debug.Log("3. Assign Zoom Action: CameraControls/CameraMap/Zoom");
            Debug.Log("4. Assign Reset and Focus actions as well");
        }

#if UNITY_EDITOR
        /// <summary>
        /// Custom inspector button for easy setup
        /// </summary>
        [UnityEditor.CustomEditor(typeof(CameraQuickSetup))]
        public class CameraQuickSetupEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("🎬 Setup Complete Camera System", GUILayout.Height(30)))
                {
                    var setup = (CameraQuickSetup)target;
                    setup.SetupCameraSystem();
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "This will create:\n" +
                    "• TacticalCameraController with all components\n" +
                    "• CameraTarget GameObject\n" +
                    "• Cinemachine Camera configured for tactical view\n" +
                    "• CameraSettings asset (if enabled)\n\n" +
                    "After setup, manually assign Input Action References for WASD to work!",
                    MessageType.Info);
            }
        }
#endif
    }
}

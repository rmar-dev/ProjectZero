using UnityEngine;
using ProjectZero.Core.Time;

namespace ProjectZero.Core.Time
{
    /// <summary>
    /// Easy setup component for Time Dilation System
    /// Drop this on a GameObject to automatically configure the entire time dilation system
    /// </summary>
    public class TimeDilationSystemSetup : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private TimeDilationSettings timeDilationSettings;
        
        [Header("Auto Setup Options")]
        [SerializeField] private bool createTimeStateManager = true;
        [SerializeField] private bool createTimeDilationUI = false; // Optional UI
        [SerializeField] private bool setupOnAwake = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        private void Awake()
        {
            if (setupOnAwake)
            {
                SetupTimeDilationSystem();
            }
        }
        
        [ContextMenu("Setup Time Dilation System")]
        public void SetupTimeDilationSystem()
        {
            if (enableDebugLogs)
                Debug.Log("Setting up Time Dilation System...");
            
            // 1. Get or create the TimeDilationController (Singleton)
            var controller = TimeDilationController.Instance;
            
            // 2. Assign settings if provided
            if (timeDilationSettings != null && controller.Settings != timeDilationSettings)
            {
                // We need to set the settings via reflection or make Settings settable
                // For now, this will be handled by the TimeDilationController's OnValidate in editor
                if (enableDebugLogs)
                    Debug.Log($"Time Dilation Settings will be assigned: {timeDilationSettings.name}");
            }
            else if (timeDilationSettings == null)
            {
                Debug.LogWarning("No TimeDilationSettings assigned! Please assign settings in the inspector.");
            }
            
            // 3. Create TimeStateManager if requested
            if (createTimeStateManager && FindFirstObjectByType<TimeStateManager>() == null)
            {
                GameObject timeStateManagerGO = new GameObject("TimeStateManager");
                timeStateManagerGO.transform.SetParent(transform);
                var timeStateManager = timeStateManagerGO.AddComponent<TimeStateManager>();
                
                if (enableDebugLogs)
                    Debug.Log("Created TimeStateManager");
            }
            
            // 4. Create TimeDilationUI if requested
            if (createTimeDilationUI && FindFirstObjectByType<TimeDilationUI>() == null)
            {
                GameObject uiGO = new GameObject("TimeDilationUI");
                uiGO.transform.SetParent(transform);
                var ui = uiGO.AddComponent<TimeDilationUI>();
                
                if (enableDebugLogs)
                    Debug.Log("Created TimeDilationUI");
            }
            
            // 5. Ensure this GameObject persists across scenes (since it contains the singleton)
            if (controller.transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            if (enableDebugLogs)
                Debug.Log("✅ Time Dilation System setup complete!");
        }
        
        /// <summary>
        /// Quick access to the controller for other systems
        /// </summary>
        public TimeDilationController GetController()
        {
            return TimeDilationController.Instance;
        }
        
        /// <summary>
        /// Quick access to time state manager
        /// </summary>
        public TimeStateManager GetTimeStateManager()
        {
            return FindFirstObjectByType<TimeStateManager>();
        }
        
        /// <summary>
        /// Test method to verify the system is working
        /// </summary>
        [ContextMenu("Test Time Dilation")]
        public void TestTimeDilation()
        {
            var controller = TimeDilationController.Instance;
            
            Debug.Log($"Current State: {controller.CurrentState}");
            Debug.Log($"Current Time Scale: {controller.CurrentTimeScale}");
            
            // Toggle to tactical mode
            controller.ToggleTacticalMode();
            
            Debug.Log($"After Toggle - State: {controller.CurrentState}, Time Scale: {controller.CurrentTimeScale}");
        }
        
        private void OnValidate()
        {
            // Auto-find settings if not assigned
            if (timeDilationSettings == null)
            {
                #if UNITY_EDITOR
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TimeDilationSettings");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    timeDilationSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<TimeDilationSettings>(path);
                    
                    if (enableDebugLogs)
                        Debug.Log($"Auto-found TimeDilationSettings: {timeDilationSettings.name}");
                }
                #endif
            }
        }
    }
}

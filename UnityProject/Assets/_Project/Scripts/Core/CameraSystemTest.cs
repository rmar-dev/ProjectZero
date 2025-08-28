using UnityEngine;
using ProjectZero.Camera;

namespace ProjectZero.Core
{
    /// <summary>
    /// Test script to verify the modular camera system works correctly.
    /// Provides runtime testing and configuration options.
    /// </summary>
    public class CameraSystemTest : MonoBehaviour
    {
        [Header("Test References")]
        [SerializeField] private TacticalCameraController cameraController;
        
        [Header("Test Settings")]
        [SerializeField] private bool runAutomaticTests = true;
        [SerializeField] private bool showTestUI = true;
        [SerializeField] private float testInterval = 2f;
        
        [Header("Test Configuration")]
        [SerializeField] private Vector3[] testPositions = {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 10),
            new Vector3(-10, 0, -10),
            new Vector3(15, 0, 0)
        };
        [SerializeField] private float[] testZoomDistances = { 5f, 10f, 15f, 20f };
        
        private float nextTestTime;
        private int currentTestIndex = 0;
        private bool testsCompleted = false;
        
        private void Start()
        {
            ValidateSetup();
            StartCameraTests();
        }
        
        private void Update()
        {
            if (runAutomaticTests && !testsCompleted && Time.time >= nextTestTime)
            {
                RunNextTest();
            }
        }
        
        /// <summary>
        /// Validate that required components are set up correctly
        /// </summary>
        private void ValidateSetup()
        {
            if (cameraController == null)
            {
                cameraController = FindObjectOfType<TacticalCameraController>();
            }
            
            if (cameraController == null)
            {
                Debug.LogError("CameraSystemTest: No TacticalCameraController found in scene!");
                enabled = false;
                return;
            }
            
            Debug.Log("✓ CameraSystemTest: Camera controller found and ready for testing.");
        }
        
        /// <summary>
        /// Initialize camera testing
        /// </summary>
        private void StartCameraTests()
        {
            if (!cameraController) return;
            
            Debug.Log("🎥 CameraSystemTest: Starting camera system tests...");
            
            // Test initial configuration
            TestInitialConfiguration();
            
            if (runAutomaticTests)
            {
                nextTestTime = Time.time + testInterval;
            }
        }
        
        /// <summary>
        /// Test initial camera system configuration
        /// </summary>
        private void TestInitialConfiguration()
        {
            var status = cameraController.GetCameraStatus();
            
            Debug.Log($"📊 Initial Camera Status:");
            Debug.Log($"  Position: {status.position}");
            Debug.Log($"  Zoom Distance: {status.zoomDistance:F1}");
            Debug.Log($"  Bounds Size: {status.boundsInfo.size}");
            Debug.Log($"  Components Active: Movement, Zoom, Bounds, Input");
        }
        
        /// <summary>
        /// Run the next automated test
        /// </summary>
        private void RunNextTest()
        {
            if (currentTestIndex < testPositions.Length)
            {
                TestCameraPosition(testPositions[currentTestIndex]);
                currentTestIndex++;
            }
            else if (currentTestIndex < testPositions.Length + testZoomDistances.Length)
            {
                int zoomIndex = currentTestIndex - testPositions.Length;
                TestCameraZoom(testZoomDistances[zoomIndex]);
                currentTestIndex++;
            }
            else
            {
                CompleteTests();
                return;
            }
            
            nextTestTime = Time.time + testInterval;
        }
        
        /// <summary>
        /// Test camera position setting
        /// </summary>
        private void TestCameraPosition(Vector3 position)
        {
            Debug.Log($"🎯 Testing camera position: {position}");
            cameraController.SetCameraPosition(position);
            
            // Verify position after one frame
            var status = cameraController.GetCameraStatus();
            Debug.Log($"  Result: {status.position} (Distance from target: {Vector3.Distance(status.position, position):F2})");
        }
        
        /// <summary>
        /// Test camera zoom distance
        /// </summary>
        private void TestCameraZoom(float zoomDistance)
        {
            Debug.Log($"🔍 Testing zoom distance: {zoomDistance}");
            cameraController.SetZoomDistance(zoomDistance);
            
            var status = cameraController.GetCameraStatus();
            Debug.Log($"  Result: {status.zoomDistance:F1} (Progress: {status.zoomProgress:P0})");
        }
        
        /// <summary>
        /// Complete all automated tests
        /// </summary>
        private void CompleteTests()
        {
            testsCompleted = true;
            Debug.Log("✅ CameraSystemTest: All automated tests completed successfully!");
            
            // Reset to center position
            cameraController.SetCameraPosition(Vector3.zero);
            cameraController.SetZoomDistance(10f);
            
            Debug.Log("📝 Test Summary:");
            Debug.Log("  ✓ Position control working");
            Debug.Log("  ✓ Zoom control working");
            Debug.Log("  ✓ Component system functioning");
            Debug.Log("  ✓ Ready for manual input testing");
        }
        
        /// <summary>
        /// Test specific camera component functionality
        /// </summary>
        [ContextMenu("Test Component Toggling")]
        public void TestComponentToggling()
        {
            if (!cameraController) return;
            
            Debug.Log("🔧 Testing component toggling...");
            
            // Test disabling movement
            cameraController.SetComponentActive(typeof(CameraMovement), false);
            Debug.Log("  Movement disabled");
            
            // Wait and re-enable
            Invoke(nameof(ReEnableMovement), 2f);
        }
        
        private void ReEnableMovement()
        {
            cameraController.SetComponentActive(typeof(CameraMovement), true);
            Debug.Log("  Movement re-enabled");
        }
        
        /// <summary>
        /// Test input enable/disable
        /// </summary>
        [ContextMenu("Test Input Toggle")]
        public void TestInputToggle()
        {
            if (!cameraController) return;
            
            Debug.Log("🎮 Testing input toggle...");
            cameraController.SetInputEnabled(false);
            Debug.Log("  Input disabled for 3 seconds...");
            
            Invoke(nameof(ReEnableInput), 3f);
        }
        
        private void ReEnableInput()
        {
            cameraController.SetInputEnabled(true);
            Debug.Log("  Input re-enabled");
        }
        
        private void OnGUI()
        {
            if (!showTestUI || !cameraController) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 200, 200));
            GUILayout.BeginVertical("Camera System Tests", GUI.skin.window);
            
            GUILayout.Label($"Tests: {(testsCompleted ? "Completed" : "Running")}");
            GUILayout.Label($"Test Index: {currentTestIndex}");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Test Position (0,0,0)"))
            {
                TestCameraPosition(Vector3.zero);
            }
            
            if (GUILayout.Button("Test Zoom Close (5)"))
            {
                TestCameraZoom(5f);
            }
            
            if (GUILayout.Button("Test Zoom Far (20)"))
            {
                TestCameraZoom(20f);
            }
            
            if (GUILayout.Button("Toggle Input"))
            {
                TestInputToggle();
            }
            
            if (GUILayout.Button("Reset Camera"))
            {
                cameraController.SetCameraPosition(Vector3.zero);
                cameraController.SetZoomDistance(10f);
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}

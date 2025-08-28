using UnityEngine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Test script to verify camera settings work correctly with zoom rotation settings
    /// </summary>
    public class CameraSettingsTest : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private bool runTestOnStart = true;
        
        [Header("Settings Reference")]
        [SerializeField] private CameraSettings testSettings;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                TestCameraSettings();
            }
        }
        
        [ContextMenu("Test Camera Settings")]
        public void TestCameraSettings()
        {
            Debug.Log("[CameraSettingsTest] Testing camera settings...");
            
            if (testSettings == null)
            {
                Debug.LogError("[CameraSettingsTest] No CameraSettings assigned! Please assign the DefaultCameraSettings asset in the inspector.");
                return;
            }
            
            Debug.Log($"[CameraSettingsTest] Testing settings: {testSettings.name}");
            Debug.Log($"[CameraSettingsTest] EnableZoomRotation: {testSettings.EnableZoomRotation}");
            Debug.Log($"[CameraSettingsTest] MinPitchAngle: {testSettings.MinPitchAngle}°");
            Debug.Log($"[CameraSettingsTest] MaxPitchAngle: {testSettings.MaxPitchAngle}°");
            Debug.Log($"[CameraSettingsTest] PitchSmoothTime: {testSettings.PitchSmoothTime}");
            Debug.Log($"[CameraSettingsTest] EnableYawRotation: {testSettings.EnableYawRotation}");
            
            // Test the angle calculation methods
            Debug.Log($"[CameraSettingsTest] Testing angle calculations:");
            for (float zoom = 0f; zoom <= 1f; zoom += 0.25f)
            {
                float pitchAngle = testSettings.GetPitchAngle(zoom);
                float yawAngle = testSettings.GetYawAngle(zoom);
                Debug.Log($"[CameraSettingsTest] Zoom {zoom:F2} -> Pitch: {pitchAngle:F1}°, Yaw: {yawAngle:F1}°");
            }
            
            Debug.Log("[CameraSettingsTest] Test completed successfully!");
        }
    }
}

using UnityEngine;
using Unity.Cinemachine;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Helper script to analyze and fix Cinemachine configuration issues with manual rotation
    /// </summary>
    public class CinemachineRotationAnalyzer : MonoBehaviour
    {
        [Header("Analysis Target")]
        [SerializeField] private CinemachineCamera virtualCamera;
        
        [Header("Debug Controls")]
        [SerializeField] private bool runAnalysisOnStart = true;
        [SerializeField] private bool fixIssuesAutomatically = false;
        
        private void Start()
        {
            if (runAnalysisOnStart)
            {
                AnalyzeCinemachineConfiguration();
            }
        }
        
        [ContextMenu("Analyze Cinemachine Configuration")]
        public void AnalyzeCinemachineConfiguration()
        {
            if (virtualCamera == null)
            {
                Debug.LogError("[CinemachineAnalyzer] No virtual camera assigned!");
                return;
            }
            
            Debug.Log($"[CinemachineAnalyzer] Analyzing {virtualCamera.name}...");
            
            // Check Body components
            AnalyzeBodyComponents();
            
            // Check Aim components  
            AnalyzeAimComponents();
            
            // Check other components that might affect rotation
            AnalyzeOtherComponents();
            
            // Provide recommendations
            ProvideRecommendations();
        }
        
        private void AnalyzeBodyComponents()
        {
            Debug.Log("[CinemachineAnalyzer] === BODY COMPONENTS ===");
            
            var positionComposer = virtualCamera.GetComponent<CinemachinePositionComposer>();
            if (positionComposer != null)
            {
                Debug.Log($"[CinemachineAnalyzer] ✓ CinemachinePositionComposer found");
                Debug.Log($"  - Camera Distance: {positionComposer.CameraDistance}");
                Debug.Log($"  - Target Offset: {positionComposer.TargetOffset}");
                Debug.Log($"  - Enabled: {positionComposer.enabled}");
            }
            
            var thirdPersonFollow = virtualCamera.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow != null)
            {
                Debug.LogWarning($"[CinemachineAnalyzer] ⚠️ CinemachineThirdPersonFollow found - this might control rotation!");
                Debug.Log($"  - Enabled: {thirdPersonFollow.enabled}");
            }
            
            var orbitalFollow = virtualCamera.GetComponent<CinemachineOrbitalFollow>();
            if (orbitalFollow != null)
            {
                Debug.LogWarning($"[CinemachineAnalyzer] ⚠️ CinemachineOrbitalFollow found - this controls rotation!");
                Debug.Log($"  - Enabled: {orbitalFollow.enabled}");
                Debug.Log($"  - Horizontal Axis: {orbitalFollow.HorizontalAxis.Value}");
                Debug.Log($"  - Vertical Axis: {orbitalFollow.VerticalAxis.Value}");
            }
            
            var hardLockToTarget = virtualCamera.GetComponent<CinemachineHardLockToTarget>();
            if (hardLockToTarget != null)
            {
                Debug.LogWarning($"[CinemachineAnalyzer] ⚠️ CinemachineHardLockToTarget found - this might control rotation!");
                Debug.Log($"  - Enabled: {hardLockToTarget.enabled}");
            }
        }
        
        private void AnalyzeAimComponents()
        {
            Debug.Log("[CinemachineAnalyzer] === AIM COMPONENTS ===");
            
            // Check for newer rotation composer (current API)
            var rotationComposer = virtualCamera.GetComponent<CinemachineRotationComposer>();
            if (rotationComposer != null)
            {
                Debug.LogWarning($"[CinemachineAnalyzer] ⚠️ CinemachineRotationComposer found - this controls rotation!");
                Debug.Log($"  - Enabled: {rotationComposer.enabled}");
            }
            
            // Check for group framing (newer API)
            var groupFraming = virtualCamera.GetComponent<CinemachineGroupFraming>();
            if (groupFraming != null)
            {
                Debug.LogWarning($"[CinemachineAnalyzer] ⚠️ CinemachineGroupFraming found - this might control rotation!");
                Debug.Log($"  - Enabled: {groupFraming.enabled}");
            }
            
            // Check for deprecated components (with warnings)
#pragma warning disable CS0618
            var composer = virtualCamera.GetComponent<CinemachineComposer>();
            if (composer != null)
            {
                Debug.LogWarning($"[CinemachineAnalyzer] ⚠️ CinemachineComposer found (DEPRECATED) - this controls rotation!");
                Debug.Log($"  - Enabled: {composer.enabled}");
                Debug.Log($"  - Consider upgrading to CinemachineRotationComposer");
            }
            
            var groupComposer = virtualCamera.GetComponent<CinemachineGroupComposer>();
            if (groupComposer != null)
            {
                Debug.LogWarning($"[CinemachineAnalyzer] ⚠️ CinemachineGroupComposer found (DEPRECATED) - this controls rotation!");
                Debug.Log($"  - Enabled: {groupComposer.enabled}");
                Debug.Log($"  - Consider upgrading to CinemachineGroupFraming");
            }
#pragma warning restore CS0618
            
            var panTilt = virtualCamera.GetComponent<CinemachinePanTilt>();
            if (panTilt != null)
            {
                Debug.Log($"[CinemachineAnalyzer] ✓ CinemachinePanTilt found - this allows manual rotation");
                Debug.Log($"  - Enabled: {panTilt.enabled}");
                Debug.Log($"  - Pan Axis: {panTilt.PanAxis.Value}");
                Debug.Log($"  - Tilt Axis: {panTilt.TiltAxis.Value}");
            }
        }
        
        private void AnalyzeOtherComponents()
        {
            Debug.Log("[CinemachineAnalyzer] === OTHER COMPONENTS ===");
            
            var allComponents = virtualCamera.GetComponents<MonoBehaviour>();
            foreach (var comp in allComponents)
            {
                if (comp.GetType().Name.Contains("Cinemachine") && 
                    !comp.GetType().Name.Contains("PositionComposer") &&
                    !comp.GetType().Name.Contains("PanTilt") &&
                    comp.GetType() != typeof(CinemachineCamera))
                {
                    Debug.Log($"[CinemachineAnalyzer] - {comp.GetType().Name}: {(comp.enabled ? "ENABLED" : "disabled")}");
                }
            }
        }
        
        private void ProvideRecommendations()
        {
            Debug.Log("[CinemachineAnalyzer] === RECOMMENDATIONS ===");
            
            var problemComponents = new System.Collections.Generic.List<string>();
            
            // Check for problematic aim components (with pragma to suppress warnings)
#pragma warning disable CS0618
            if (virtualCamera.GetComponent<CinemachineComposer>()?.enabled == true)
                problemComponents.Add("CinemachineComposer (deprecated)");
            if (virtualCamera.GetComponent<CinemachineGroupComposer>()?.enabled == true)
                problemComponents.Add("CinemachineGroupComposer (deprecated)");
#pragma warning restore CS0618
            
            if (virtualCamera.GetComponent<CinemachineRotationComposer>()?.enabled == true)
                problemComponents.Add("CinemachineRotationComposer");
            if (virtualCamera.GetComponent<CinemachineGroupFraming>()?.enabled == true)
                problemComponents.Add("CinemachineGroupFraming");
                
            // Check for problematic body components
            if (virtualCamera.GetComponent<CinemachineOrbitalFollow>()?.enabled == true)
                problemComponents.Add("CinemachineOrbitalFollow");
            if (virtualCamera.GetComponent<CinemachineThirdPersonFollow>()?.enabled == true)
                problemComponents.Add("CinemachineThirdPersonFollow");
                
            if (problemComponents.Count > 0)
            {
                Debug.LogWarning($"[CinemachineAnalyzer] 🚨 FOUND {problemComponents.Count} COMPONENTS THAT OVERRIDE ROTATION:");
                foreach (var comp in problemComponents)
                {
                    Debug.LogWarning($"  - {comp} should be disabled for manual rotation to work");
                }
                
                Debug.Log($"[CinemachineAnalyzer] 💡 RECOMMENDED SETUP FOR MANUAL ROTATION:");
                Debug.Log($"  - Body: CinemachinePositionComposer (for distance control)");
                Debug.Log($"  - Aim: CinemachinePanTilt or None (to allow manual rotation)");
                Debug.Log($"  - Disable all other rotation-controlling components");
                
                if (fixIssuesAutomatically)
                {
                    FixRotationIssues();
                }
            }
            else
            {
                Debug.Log($"[CinemachineAnalyzer] ✅ No problematic components found! Manual rotation should work.");
            }
        }
        
        [ContextMenu("Fix Rotation Issues Automatically")]
        public void FixRotationIssues()
        {
            if (virtualCamera == null) return;
            
            Debug.Log("[CinemachineAnalyzer] 🔧 Automatically fixing rotation issues...");
            
            // Disable problematic aim components (with pragma for deprecated ones)
#pragma warning disable CS0618
            var composer = virtualCamera.GetComponent<CinemachineComposer>();
            if (composer != null) { composer.enabled = false; Debug.Log("  - Disabled CinemachineComposer (deprecated)"); }
            
            var groupComposer = virtualCamera.GetComponent<CinemachineGroupComposer>();
            if (groupComposer != null) { groupComposer.enabled = false; Debug.Log("  - Disabled CinemachineGroupComposer (deprecated)"); }
#pragma warning restore CS0618
            
            var rotationComposer = virtualCamera.GetComponent<CinemachineRotationComposer>();
            if (rotationComposer != null) { rotationComposer.enabled = false; Debug.Log("  - Disabled CinemachineRotationComposer"); }
            
            var groupFraming = virtualCamera.GetComponent<CinemachineGroupFraming>();
            if (groupFraming != null) { groupFraming.enabled = false; Debug.Log("  - Disabled CinemachineGroupFraming"); }
            
            // Disable problematic body components
            var orbitalFollow = virtualCamera.GetComponent<CinemachineOrbitalFollow>();
            if (orbitalFollow != null) { orbitalFollow.enabled = false; Debug.Log("  - Disabled CinemachineOrbitalFollow"); }
            
            var thirdPersonFollow = virtualCamera.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow != null) { thirdPersonFollow.enabled = false; Debug.Log("  - Disabled CinemachineThirdPersonFollow"); }
            
            // Ensure we have the correct components
            var positionComposer = virtualCamera.GetComponent<CinemachinePositionComposer>();
            if (positionComposer == null)
            {
                positionComposer = virtualCamera.gameObject.AddComponent<CinemachinePositionComposer>();
                Debug.Log("  - Added CinemachinePositionComposer");
            }
            positionComposer.enabled = true;
            
            // Add PanTilt if it doesn't exist (optional, for manual rotation support)
            var panTilt = virtualCamera.GetComponent<CinemachinePanTilt>();
            if (panTilt == null)
            {
                panTilt = virtualCamera.gameObject.AddComponent<CinemachinePanTilt>();
                Debug.Log("  - Added CinemachinePanTilt for manual rotation support");
            }
            panTilt.enabled = true;
            
            Debug.Log("[CinemachineAnalyzer] ✅ Fixed! Manual rotation should now work.");
        }
    }
}

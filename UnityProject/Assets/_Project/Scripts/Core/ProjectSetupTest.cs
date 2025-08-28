using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Unity.AI.Navigation;

/// <summary>
/// Simple test script to verify that all required packages are properly installed
/// </summary>
public class ProjectSetupTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("ProjectZero Setup Test Starting...");
        
        // Test Input System
        Debug.Log("✓ Input System package is available");
        
        // Test Cinemachine
        Debug.Log("✓ Cinemachine package is available");
        
        // Test AI Navigation
        Debug.Log("✓ AI Navigation package is available");
        
        Debug.Log("🎉 All packages successfully loaded! ProjectZero Unity setup is complete.");
    }
}

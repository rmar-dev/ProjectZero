# Unity Testing & Troubleshooting Guide - Cover System

## 🎯 Overview
Comprehensive guide for testing the Unity cover system and solving common Unity-specific issues.

## 🧪 Unity Testing Framework

### Phase 1: Component Verification (5 minutes)

#### ✅ Test 1: Script Compilation
**Goal**: Verify CoverPoint.cs script compiles without errors

**Steps**:
1. Create `CoverPoint.cs` in `Assets/_Project/Scripts/Cover/`
2. Check Unity Console for compilation errors
3. Verify script appears in Component menu

**Expected Result**: No red errors in Console, script compiles successfully
**If Failed**: [Jump to Compilation Issues](#compilation-issues)

#### ✅ Test 2: Component Assignment
**Goal**: Ensure CoverPoint component can be added to GameObjects

**Steps**:
1. Create empty GameObject in scene
2. Click "Add Component" in Inspector
3. Search for "Cover Point"
4. Add the component

**Expected Result**: Component appears in Inspector with configurable fields
**If Failed**: [Jump to Component Issues](#component-assignment-issues)

#### ✅ Test 3: Inspector Values
**Goal**: Verify all properties are accessible in Inspector

**Steps**:
1. Add CoverPoint component to GameObject
2. Check Inspector shows these sections:
   - Cover Configuration
   - Status

**Expected Result**: All fields visible and editable in Inspector
**If Failed**: [Jump to Inspector Issues](#inspector-issues)

### Phase 2: Unity Runtime Testing (10 minutes)

#### ✅ Test 4: Initialization Testing
**Goal**: Component initializes correctly at runtime

**Unity Test Setup**:
1. Create test scene with CoverPoint GameObject
2. Press Play
3. Check Unity Console

**Expected Console Output**:
```
Cover Point initialized: Heavy cover with 100 health
```

**If Failed**: [Jump to Runtime Issues](#runtime-initialization-issues)

#### ✅ Test 5: Health System Testing
**Goal**: Damage and health system works correctly

**Unity Test Method**:
1. **During Play Mode**:
2. **Select CoverPoint in Hierarchy**
3. **In Inspector**, find CoverPoint script
4. **Call methods using Inspector**:
   - Call `TakeDamage(25)`
   - Verify health changes in Inspector

**Expected Console Output**:
```
Cover damaged: 75/100 health remaining
```

**If Failed**: [Jump to Health System Issues](#health-system-issues)

#### ✅ Test 6: Occupancy System Testing  
**Goal**: Unit occupation and reservation works

**Unity Test Setup**:
```csharp
// Add this test method to CoverPoint.cs for testing
[ContextMenu("Test Occupancy")]
public void TestOccupancy()
{
    Debug.Log("Testing occupancy system...");
    
    bool result1 = TryOccupy();
    Debug.Log($"First occupy attempt: {result1}");
    
    bool result2 = TryOccupy();  
    Debug.Log($"Second occupy attempt: {result2}");
    
    bool result3 = TryOccupy();
    Debug.Log($"Third occupy attempt (should fail): {result3}");
    
    Release();
    Debug.Log("Released one occupant");
    
    bool result4 = TryOccupy();
    Debug.Log($"Occupy after release: {result4}");
}
```

**Expected Result**: 
- First two occupancy attempts succeed
- Third fails (max occupants reached)  
- Fourth succeeds after release

**If Failed**: [Jump to Occupancy Issues](#occupancy-system-issues)

### Phase 3: Unity Integration Testing (15 minutes)

#### ✅ Test 7: Gizmo Visualization
**Goal**: Unity Gizmos display cover information correctly

**Steps**:
1. Add `OnDrawGizmos()` method to CoverPoint.cs
2. Select GameObject in Scene view
3. Verify Gizmos appear

**Expected Result**: 
- Green/red wireframe cube showing occupancy status
- Text labels showing occupant count and health

**If Failed**: [Jump to Gizmos Issues](#gizmos-issues)

#### ✅ Test 8: Prefab System
**Goal**: CoverPoint works properly as Unity prefab

**Steps**:
1. Create CoverPoint prefab
2. Instantiate multiple copies in scene
3. Verify each has independent state
4. Test prefab modifications update all instances

**Expected Result**: Each instance maintains separate state
**If Failed**: [Jump to Prefab Issues](#prefab-system-issues)

---

## 🚨 Unity Troubleshooting Guide

### Compilation Issues

**Symptoms**: 
- Red errors in Unity Console
- Script won't attach to GameObject
- "Script compilation failed" messages

**Solutions**:
1. **Check Syntax**:
   ```csharp
   // Common Unity syntax issues:
   // - Missing using UnityEngine;
   // - Class name doesn't match file name
   // - Missing semicolons or brackets
   ```

2. **Verify Dependencies**:
   ```csharp
   // Make sure these are included:
   using UnityEngine;
   using System.Collections;
   using System.Collections.Generic;
   ```

3. **Clear Script Cache**:
   ```
   // In Unity menu:
   Assets → Reimport All
   // Or delete Library folder and reopen project
   ```

### Component Assignment Issues

**Symptoms**:
- Component doesn't appear in Add Component menu
- "Missing Script" warnings in Inspector

**Solutions**:
1. **Wait for Compilation**:
   - Check bottom-right of Unity for compilation progress
   - Don't try to add components while compiling

2. **Check Script Location**:
   ```
   // Script must be in Assets/ folder structure
   Assets/_Project/Scripts/Cover/CoverPoint.cs
   ```

3. **Verify Class Declaration**:
   ```csharp
   public class CoverPoint : MonoBehaviour  // Must match filename
   {
       // Class implementation
   }
   ```

### Inspector Issues

**Symptoms**:
- Properties don't appear in Inspector
- Values reset to default on Play

**Solutions**:
1. **Use Proper Attributes**:
   ```csharp
   [Header("Cover Configuration")]
   [SerializeField] private float maxHealth = 100f;  // Private with SerializeField
   
   public float MaxHealth => maxHealth;  // Public property for access
   ```

2. **Check Property Types**:
   ```csharp
   // Unity supports these in Inspector:
   public float, int, bool, string
   public Vector3, Color, GameObject
   public enum types
   [SerializeField] private fields
   ```

### Runtime Initialization Issues

**Symptoms**:
- No console output during Play mode
- NullReferenceException errors
- Components seem inactive

**Solutions**:
1. **Check GameObject State**:
   ```csharp
   private void Start()
   {
       if (!gameObject.activeInHierarchy)
       {
           Debug.LogError("CoverPoint GameObject is inactive!");
           return;
       }
       
       Debug.Log("CoverPoint initializing...");
   }
   ```

2. **Verify Component References**:
   ```csharp
   private void Awake()
   {
       // Cache component references
       coverRenderer = GetComponent<Renderer>();
       if (coverRenderer == null)
       {
           Debug.LogWarning("No Renderer found on CoverPoint");
       }
   }
   ```

### Health System Issues

**Symptoms**:
- Damage doesn't apply
- Health values don't update  
- No damage feedback

**Solutions**:
1. **Debug Health Changes**:
   ```csharp
   public void TakeDamage(float damage)
   {
       float oldHealth = currentHealth;
       currentHealth = Mathf.Max(0, currentHealth - damage);
       
       Debug.Log($"Damage applied: {damage}, Health: {oldHealth} → {currentHealth}");
       
       if (currentHealth <= 0 && canBeDestroyed)
       {
           Debug.Log("Cover destroyed!");
           OnCoverDestroyed();
       }
   }
   ```

2. **Validate Input Parameters**:
   ```csharp
   public void TakeDamage(float damage)
   {
       if (damage < 0)
       {
           Debug.LogWarning($"Negative damage value: {damage}");
           return;
       }
       // Continue with damage logic...
   }
   ```

### Occupancy System Issues

**Symptoms**:
- Units can't occupy cover
- Occupancy count incorrect
- Reservation failures

**Solutions**:
1. **Debug Occupancy Logic**:
   ```csharp
   public bool TryOccupy(GameObject unit = null)
   {
       Debug.Log($"Occupation attempt. Current: {currentOccupants}/{maxOccupants}");
       
       if (currentOccupants >= maxOccupants)
       {
           Debug.Log("Cover at capacity!");
           return false;
       }
       
       currentOccupants++;
       Debug.Log($"Cover occupied successfully. New count: {currentOccupants}");
       return true;
   }
   ```

2. **Track Occupants**:
   ```csharp
   private List<GameObject> occupyingUnits = new List<GameObject>();
   
   public bool TryOccupy(GameObject unit)
   {
       if (occupyingUnits.Contains(unit))
       {
           Debug.LogWarning($"Unit {unit.name} already occupies this cover!");
           return false;
       }
       
       if (occupyingUnits.Count >= maxOccupants)
       {
           Debug.Log("Cover at maximum capacity");
           return false;
       }
       
       occupyingUnits.Add(unit);
       currentOccupants = occupyingUnits.Count;
       return true;
   }
   ```

### Gizmos Issues

**Symptoms**:
- No visual feedback in Scene view
- Gizmos don't update with state changes

**Solutions**:
1. **Enable Gizmos in Scene View**:
   - Check Scene view → Gizmos button is enabled
   - Verify specific script gizmos are enabled in Gizmos menu

2. **Debug Gizmo Drawing**:
   ```csharp
   private void OnDrawGizmos()
   {
       // Always draw something to test if gizmos work
       Gizmos.color = Color.yellow;
       Gizmos.DrawSphere(transform.position, 0.5f);
       
       // Then add conditional drawing
       if (Application.isPlaying)
       {
           Gizmos.color = currentOccupants > 0 ? Color.red : Color.green;
           Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
       }
   }
   ```

### Prefab System Issues

**Symptoms**:
- Prefab instances don't maintain independent state
- Changes to prefab affect all instances incorrectly

**Solutions**:
1. **Ensure Instance Independence**:
   ```csharp
   private void Awake()
   {
       // Initialize instance-specific data
       currentHealth = maxHealth;
       currentOccupants = 0;
       occupyingUnits = new List<GameObject>();
   }
   ```

2. **Use Prefab Variants for Customization**:
   - Create base CoverPoint prefab
   - Create variants for Light/Heavy cover types
   - Override values in variants, not base prefab

---

## 🔍 Debug Console Commands (Unity)

### Basic Information
```csharp
// In Unity Console during Play mode:
// Select CoverPoint GameObject and use Inspector to call methods

// Or add these debug methods to CoverPoint.cs:
[ContextMenu("Debug Info")]
public void DebugInfo()
{
    Debug.Log($"Cover Point Status:");
    Debug.Log($"  Type: {coverType}");
    Debug.Log($"  Health: {currentHealth}/{maxHealth}");  
    Debug.Log($"  Occupants: {currentOccupants}/{maxOccupants}");
    Debug.Log($"  Destructible: {canBeDestroyed}");
}

[ContextMenu("Test Damage")]  
public void TestDamage()
{
    TakeDamage(25f);
}

[ContextMenu("Test Occupy")]
public void TestOccupy()
{
    bool result = TryOccupy();
    Debug.Log($"Occupy test result: {result}");
}
```

### Performance Debugging
```csharp
// Add to CoverPoint for performance monitoring
private void Update()
{
    if (Input.GetKeyDown(KeyCode.P)) // Press P for performance info
    {
        Debug.Log($"Cover Point {name}: Update called");
        // Monitor if Update is being called too frequently
    }
}
```

## 📊 Unity Performance Testing

### Frame Rate Monitoring
1. **Enable Stats**: Window → Analysis → Profiler
2. **Monitor during Play**: Watch CPU and Memory usage
3. **Test with Multiple Cover Points**: 10, 50, 100+ points
4. **Target**: Maintain 60+ FPS with 100 cover points

### Memory Usage Testing
```csharp
// Add to test scene for memory monitoring
public class CoverSystemTester : MonoBehaviour
{
    public GameObject coverPointPrefab;
    public int testCount = 100;
    
    [ContextMenu("Spawn Test Cover Points")]
    public void SpawnTestCoverPoints()
    {
        for (int i = 0; i < testCount; i++)
        {
            Vector3 randomPos = Random.insideUnitSphere * 50f;
            randomPos.y = 0; // Keep on ground
            Instantiate(coverPointPrefab, randomPos, Quaternion.identity);
        }
        
        Debug.Log($"Spawned {testCount} cover points for testing");
    }
}
```

## 🎯 Unity Testing Best Practices

### Use Unity Test Framework
```csharp
// Create test script in Assets/Tests/
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CoverPointTests
{
    [Test]
    public void CoverPoint_TakeDamage_ReducesHealth()
    {
        // Create test GameObject
        GameObject testObj = new GameObject();
        CoverPoint cover = testObj.AddComponent<CoverPoint>();
        
        // Initialize
        cover.maxHealth = 100f;
        cover.Start(); // Manually call Start for testing
        
        // Test damage
        cover.TakeDamage(25f);
        
        // Assert
        Assert.AreEqual(75f, cover.currentHealth);
        
        // Cleanup
        Object.DestroyImmediate(testObj);
    }
    
    [Test]
    public void CoverPoint_Occupancy_WorksCorrectly()
    {
        GameObject testObj = new GameObject();
        CoverPoint cover = testObj.AddComponent<CoverPoint>();
        cover.maxOccupants = 2;
        
        Assert.IsTrue(cover.TryOccupy());
        Assert.IsTrue(cover.TryOccupy()); 
        Assert.IsFalse(cover.TryOccupy()); // Should fail - at capacity
        
        Object.DestroyImmediate(testObj);
    }
}
```

## ✅ Success Criteria

Your Unity cover system is working correctly when:
- [ ] Scripts compile without errors
- [ ] Components appear in Inspector correctly
- [ ] Console shows proper initialization messages
- [ ] Damage system reduces health appropriately
- [ ] Occupancy system prevents overcrowding
- [ ] Gizmos display in Scene view
- [ ] Prefabs maintain independent state
- [ ] Performance remains stable with multiple cover points

---

**Time Investment**: 30 minutes total testing
**Target Performance**: 60+ FPS with 100 cover points
**Unity Version**: Tested on Unity 2022.3 LTS

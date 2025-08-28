# Unity Quick Start Guide - 5 Minutes to Working Cover

## 🎯 Goal
Get a working cover point in your Unity scene in 5 minutes or less.

## ⚡ Prerequisites
- Unity 2022.3 LTS is open
- ProjectZero project is loaded
- You have NavMesh Components package installed

## 🚀 Step 1: Create Cover Point Script (2 minutes)

### Create Basic CoverPoint MonoBehaviour
1. **Create Script**: `Assets/_Project/Scripts/Cover/CoverPoint.cs`
2. **Add this basic implementation:**

```csharp
using UnityEngine;

public class CoverPoint : MonoBehaviour
{
    [Header("Cover Configuration")]
    public CoverType coverType = CoverType.Heavy;
    public float maxHealth = 100f;
    public int maxOccupants = 2;
    public bool canBeDestroyed = true;
    
    [Header("Status")]
    public float currentHealth;
    public int currentOccupants = 0;
    
    private void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"Cover Point initialized: {coverType} cover with {maxHealth} health");
    }
    
    public bool TryOccupy()
    {
        if (currentOccupants < maxOccupants)
        {
            currentOccupants++;
            Debug.Log($"Cover occupied. Current occupants: {currentOccupants}/{maxOccupants}");
            return true;
        }
        return false;
    }
    
    public void Release()
    {
        if (currentOccupants > 0)
        {
            currentOccupants--;
            Debug.Log($"Cover released. Current occupants: {currentOccupants}/{maxOccupants}");
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (!canBeDestroyed) return;
        
        currentHealth -= damage;
        Debug.Log($"Cover damaged: {currentHealth}/{maxHealth} health remaining");
        
        if (currentHealth <= 0)
        {
            Debug.Log("Cover destroyed!");
            // Add destruction logic here
        }
    }
}

public enum CoverType
{
    Light,
    Heavy,
    Destructible
}
```

## 🏗️ Step 2: Create Cover Point Prefab (2 minutes)

### Build the Prefab
1. **Create Empty GameObject**: "CoverPoint_Basic"
2. **Add Components**:
   - Add the `CoverPoint.cs` script
   - Add **Cube** child object (visual representation)
   - Scale cube to `(2, 2, 0.5)` (wall-like shape)
3. **Configure CoverPoint**:
   - Cover Type: `Heavy`
   - Max Health: `100`
   - Max Occupants: `2`
   - Can Be Destroyed: `✓ Checked`

### Make it a Prefab
1. **Drag to Project**: Drag GameObject to `Assets/_Project/Prefabs/Cover/`
2. **Name it**: `CoverPoint_Basic.prefab`
3. **Delete from scene** (we'll place it in Step 3)

## 🎮 Step 3: Test in Scene (1 minute)

### Place and Test
1. **Drag prefab** into your scene
2. **Press Play**
3. **Open Console** (Window → General → Console)
4. **Look for messages** like:
   - "Cover Point initialized: Heavy cover with 100 health"

### Quick Console Test
1. **Select CoverPoint in Hierarchy** during Play mode
2. **In Inspector**, click the gear next to CoverPoint script
3. **Call method**: `TryOccupy()` - should see occupancy message
4. **Call method**: `TakeDamage(25)` - should see damage message

✅ **Success**: You see cover-related log messages  
❌ **Problem**: No messages → Check script compilation errors

## 🔧 Step 4: Add Visual Feedback (Optional - 2 minutes)

### Add Gizmos for Scene View
Add this to your `CoverPoint.cs`:

```csharp
private void OnDrawGizmos()
{
    // Draw cover area
    Gizmos.color = currentOccupants > 0 ? Color.red : Color.green;
    Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    
    // Draw occupancy info
    Gizmos.color = Color.yellow;
    Vector3 textPos = transform.position + Vector3.up * 3f;
    
#if UNITY_EDITOR
    UnityEditor.Handles.Label(textPos, $"{currentOccupants}/{maxOccupants}\nHP: {currentHealth:F0}");
#endif
}
```

## ✅ Success Criteria

After 5 minutes, you should have:
- ✅ CoverPoint script created and working
- ✅ CoverPoint prefab in Project window
- ✅ Cover point placed in scene
- ✅ Script initializes without errors in Console
- ✅ Basic occupancy and damage testing works
- ✅ Gizmos show cover status in Scene view

## 🚨 Common Unity Issues

### Script Won't Compile
```csharp
// Check Console for errors:
// - Missing using statements
// - Typos in script name
// - File name doesn't match class name
```

### No Console Output
```csharp
// In Unity Console window:
// 1. Check "Collapse" is unchecked
// 2. Verify Play mode is active
// 3. Make sure script is attached to GameObject
```

### Prefab Issues
- Ensure script is saved before creating prefab
- Check that all components are properly assigned
- Verify prefab is in correct folder structure

## 🎉 You Did It!

**Congratulations!** You now have a working Unity cover point system!

### Next Steps:
- 📖 **Learn More**: Read [Unity Detailed Guide](Unity_DetailedGuide.md)
- 🎨 **Create Better Visuals**: Add materials and effects
- 🔧 **Add NavMesh Integration**: Connect to unit movement system
- 🐛 **Having Issues**: See Unity-specific troubleshooting

### Quick Validation Checklist:
- [ ] Script compiles without errors
- [ ] Prefab created successfully  
- [ ] Scene loads without errors
- [ ] Press Play works
- [ ] Console shows initialization messages
- [ ] Gizmos display cover state

**Time Spent: ⏱️ ~5 minutes**  
**Status: 🎯 Ready for Unity integration!**

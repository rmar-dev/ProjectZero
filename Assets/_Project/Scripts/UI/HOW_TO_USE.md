# 🚀 HOW TO USE THE UI SYSTEM - 2 MINUTE GUIDE

## ✅ **Step 1: Quick Test (30 seconds)**

1. **Create empty GameObject** (Right-click Hierarchy → Create Empty)
2. **Name it "UITest"**
3. **Add script** → Attach `QuickStartUI` component
4. **Press Play** 🎮

**You'll see:**
- Yellow label showing time
- Clickable button
- Dark panel with text
- Press SPACE for messages

## 🎯 **Step 2: For Your RTS Game (2 minutes)**

### **Create Your Game UI Script:**

```csharp
using UnityEngine;
using ProjectZero.UI;
using TMPro;
using UnityEngine.UI;

public class MyGameUI : MonoBehaviour 
{
    private UIManager ui;
    
    void Start() 
    {
        ui = UIManager.Instance;
        CreateMyUI();
    }
    
    void CreateMyUI()
    {
        // HEALTH BAR (top-left)
        ui.CreateLabel("health", "Health: 100", null, 
            new Vector2(-800, 400), new Vector2(200, 30));
        
        // AMMO COUNT (top-left) 
        ui.CreateLabel("ammo", "Ammo: 30", null,
            new Vector2(-800, 370), new Vector2(200, 30));
        
        // TACTICAL BUTTON (bottom-right)
        Button tacticalBtn = ui.CreateButton("tactical", "Tactical Mode", null,
            new Vector2(700, -400), new Vector2(120, 40));
        tacticalBtn.onClick.AddListener(() => {
            ui.ShowTemporaryMessage("Tactical Mode!", 2f);
        });
        
        // EXTRACTION BUTTON (bottom-right)
        Button extractBtn = ui.CreateButton("extract", "Extract", null, 
            new Vector2(580, -400), new Vector2(100, 40));
        extractBtn.onClick.AddListener(() => {
            ui.ShowTemporaryMessage("Calling extraction...", 3f);
        });
    }
    
    void Update()
    {
        // UPDATE HUD WITH REAL GAME DATA
        ui.UpdateLabelText("health", $"Health: {playerHealth}");
        ui.UpdateLabelText("ammo", $"Ammo: {playerAmmo}");
        
        // TEST: Press H to toggle UI (New Input System)
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.hKey.wasPressedThisFrame)
        {
            ui.ShowTemporaryMessage("UI System Working!", 2f);
        }
    }
    
    // EXAMPLE: Add unit labels above 3D objects
    void AddUnitLabel(Transform unit, string name)
    {
        string labelId = $"unit_{unit.GetInstanceID()}";
        Vector3 offset = new Vector3(0, 2, 0); // Above the unit
        
        TextMeshProUGUI label = ui.CreateWorldLabel(labelId, name, unit, offset);
        label.color = Color.cyan;
        label.fontSize = 14;
    }
}
```

## 🎮 **What Each Method Does:**

### **Labels (Text)**
```csharp
// Screen label at specific position
ui.CreateLabel("my_label", "Hello World!", null, new Vector2(100, 200));

// World label that follows 3D object
ui.CreateWorldLabel("unit_name", "Alpha-1", unitTransform, Vector3.up * 2);

// Update text
ui.UpdateLabelText("my_label", "New Text");
```

### **Buttons (Clickable)**
```csharp
Button btn = ui.CreateButton("my_button", "Click Me", null, new Vector2(0, -300));
btn.onClick.AddListener(() => Debug.Log("Clicked!"));
```

### **Panels (Containers)**
```csharp
// Background panel for grouping UI elements
ui.CreatePanel("info_panel", null, new Vector2(300, 0), new Vector2(250, 200));
```

### **Temporary Messages**
```csharp
ui.ShowTemporaryMessage("Level Complete!", 3f); // Auto-fades
```

## 🔥 **Common Use Cases:**

### **HUD Elements:**
```csharp
ui.CreateLabel("health", "Health: 100", null, new Vector2(-800, 400));
ui.CreateLabel("score", "Score: 0", null, new Vector2(-800, 370));
ui.CreateLabel("timer", "Time: 00:00", null, new Vector2(700, 400));
```

### **Unit Labels (above 3D soldiers):**
```csharp
foreach (GameObject soldier in soldiers)
{
    ui.CreateWorldLabel($"soldier_{soldier.GetInstanceID()}", 
        "Alpha Squad", soldier.transform, new Vector3(0, 2.5f, 0));
}
```

### **Action Buttons:**
```csharp
Button pauseBtn = ui.CreateButton("pause", "Pause", null, new Vector2(700, -400));
pauseBtn.onClick.AddListener(() => Time.timeScale = 0);
```

### **Notifications:**
```csharp
ui.ShowTemporaryMessage("Enemy Spotted!", 2f);
ui.ShowTemporaryMessage("Mission Complete!", 4f);
ui.ShowTemporaryMessage("Taking Damage!", 1.5f);
```

## 🎯 **Position Guide:**

```csharp
// Screen positions (in pixels from center)
new Vector2(-800, 400)   // Top-left corner
new Vector2(800, 400)    // Top-right corner  
new Vector2(-800, -400)  // Bottom-left corner
new Vector2(800, -400)   // Bottom-right corner
new Vector2(0, 0)        // Center screen
```

## ⚡ **Advanced Features:**

### **Show/Hide Elements:**
```csharp
ui.SetElementVisibility("my_label", false); // Hide
ui.SetElementVisibility("my_label", true);  // Show
```

### **Get UI Elements:**
```csharp
TextMeshProUGUI label = ui.GetUIElement<TextMeshProUGUI>("my_label");
Button button = ui.GetUIElement<Button>("my_button");
```

### **Clean Up:**
```csharp
ui.DestroyElement("my_label");  // Remove single element
ui.ClearAllElements();          // Remove all UI
```

## 🐛 **Troubleshooting:**

**"Nothing appears":**
- Make sure you attached the script to a GameObject
- Press Play
- Check Console for any error messages

**"Text looks weird":**
- Import TextMeshPro essentials: Window → TextMeshPro → Import TMP Essential Resources

**"Can't click buttons":**
- Make sure there's an EventSystem in scene (Unity creates automatically)

## 🎉 **You're Ready!**

1. **Copy the `MyGameUI` script above**
2. **Attach it to any GameObject** 
3. **Press Play**
4. **See your UI working instantly!**

The system handles everything else automatically - canvas creation, scaling, positioning, etc. Just focus on your game! 🚀

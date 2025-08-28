# ProjectZero - Programmatic UI System

## 🎯 Overview

A complete programmatic UI system for Unity that allows you to create Canvas, Labels, Buttons, and other UI elements entirely through code, without requiring any scene setup. Perfect for dynamic UI generation, runtime menus, HUDs, and world-space labels.

## 🏗️ Architecture

The system consists of several key components:

- **UIManager** - Main singleton for creating and managing UI elements
- **UISettings** - ScriptableObject for configuration
- **WorldLabelTracker** - Component for world-space labels that follow 3D objects
- **UIExample** - Complete example implementation

## ✨ Features

### Canvas Management
- **Screen Space Overlay** - Standard UI overlays
- **Screen Space Camera** - Camera-rendered UI
- **World Space** - 3D world-integrated UI
- **Automatic Canvas Creation** - Creates default canvases on startup
- **Canvas Registry** - Named canvas system for organization

### UI Elements
- **Labels** - TextMeshPro labels with full configuration
- **Buttons** - Interactive buttons with callbacks
- **Panels** - Container panels for grouping
- **World Labels** - 3D world-space labels that follow objects
- **Temporary Messages** - Auto-fading notification system

### Advanced Features
- **Distance-based Scaling** - World labels scale with camera distance
- **Screen Bounds Clamping** - Keep world labels on-screen
- **Visibility Culling** - Hide labels behind camera or too far away
- **Element Registry** - Track and update elements by ID
- **Animation System** - Fade in/out with customizable curves

## 🚀 Quick Start

### 1. Basic Setup

```csharp
// Get the UI Manager instance (auto-creates if needed)
UIManager uiManager = UIManager.Instance;

// Create a simple label
TextMeshProUGUI label = uiManager.CreateLabel("my_label", "Hello World!");

// Create a button with callback
Button button = uiManager.CreateButton("my_button", "Click Me!");
button.onClick.AddListener(() => Debug.Log("Button clicked!"));
```

### 2. HUD Creation

```csharp
// Create HUD canvas
Canvas hudCanvas = uiManager.GetOrCreateCanvas("HUD", RenderMode.ScreenSpaceOverlay);

// Create HUD elements
uiManager.CreateLabel("health", "Health: 100", hudCanvas, 
    new Vector2(-800, 400), new Vector2(200, 30));
uiManager.CreateLabel("score", "Score: 0", hudCanvas, 
    new Vector2(-800, 370), new Vector2(200, 30));

// Update HUD in your Update loop
uiManager.UpdateLabelText("health", $"Health: {currentHealth}");
uiManager.UpdateLabelText("score", $"Score: {currentScore}");
```

### 3. World Labels

```csharp
// Create a label that follows a 3D object
Transform enemyTransform = enemy.transform;
Vector3 offset = new Vector3(0, 2, 0); // Above the enemy

TextMeshProUGUI nameLabel = uiManager.CreateWorldLabel(
    "enemy_name", "Enemy", enemyTransform, offset);
nameLabel.color = Color.red;
```

### 4. Temporary Notifications

```csharp
// Show a message that automatically fades out
uiManager.ShowTemporaryMessage("Level Complete!", 3f);

// Show on specific canvas
uiManager.ShowTemporaryMessage("Achievement Unlocked!", 2f, overlayCanvas);
```

## 🎮 Complete Example Usage

```csharp
using UnityEngine;
using ProjectZero.UI;

public class GameUI : MonoBehaviour
{
    private UIManager uiManager;
    
    void Start()
    {
        uiManager = UIManager.Instance;
        CreateGameUI();
    }
    
    void CreateGameUI()
    {
        // Create main menu
        CreateMainMenu();
        
        // Create HUD
        CreateHUD();
        
        // Create world labels for all enemies
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            CreateEnemyLabel(enemy);
        }
    }
    
    void CreateMainMenu()
    {
        RectTransform menuPanel = uiManager.CreatePanel("main_menu", null,
            Vector2.zero, new Vector2(300, 400));
        
        uiManager.CreateLabel("title", "My Game", null, 
            new Vector2(0, 150), new Vector2(280, 50));
        
        Button startBtn = uiManager.CreateButton("start_btn", "Start Game", null,
            new Vector2(0, 50), new Vector2(200, 40));
        startBtn.onClick.AddListener(StartGame);
    }
    
    void CreateHUD()
    {
        Canvas hud = uiManager.GetOrCreateCanvas("HUD", RenderMode.ScreenSpaceOverlay);
        
        uiManager.CreateLabel("health", "Health: 100", hud,
            new Vector2(-800, 400), new Vector2(200, 30));
        uiManager.CreateLabel("ammo", "Ammo: 30", hud,
            new Vector2(-800, 370), new Vector2(200, 30));
    }
    
    void CreateEnemyLabel(Enemy enemy)
    {
        string labelId = $"enemy_{enemy.GetInstanceID()}";
        Vector3 offset = new Vector3(0, 2, 0);
        
        uiManager.CreateWorldLabel(labelId, enemy.name, enemy.transform, offset);
    }
    
    void StartGame()
    {
        uiManager.SetElementVisibility("main_menu", false);
        uiManager.ShowTemporaryMessage("Game Started!", 2f);
    }
}
```

## 🔧 Configuration

### UISettings ScriptableObject

Create custom UI settings assets:

```csharp
// Right-click in Project: Create > ProjectZero > UI > UI Settings
```

Configure:
- **Reference Resolution** - Target screen resolution (1920x1080)
- **Font Settings** - Default font, size, color
- **Element Sizes** - Default sizes for labels, buttons, panels
- **Button Colors** - Normal, highlight, pressed, disabled states
- **Animation Settings** - Fade durations and curves
- **World Label Settings** - Distance scaling, max visibility range

## 📋 API Reference

### Canvas Management

```csharp
// Create canvas
Canvas CreateCanvas(string name, RenderMode renderMode, int sortingOrder = 0)
Canvas GetOrCreateCanvas(string name, RenderMode renderMode = RenderMode.ScreenSpaceOverlay)
```

### Element Creation

```csharp
// Labels
TextMeshProUGUI CreateLabel(string id, string text, Canvas parent = null, 
                           Vector2? position = null, Vector2? size = null)
TextMeshProUGUI CreateWorldLabel(string id, string text, Transform followTarget, 
                                Vector3 offset = default)

// Buttons
Button CreateButton(string id, string text, Canvas parent = null,
                   Vector2? position = null, Vector2? size = null)

// Panels
RectTransform CreatePanel(string id, Canvas parent = null,
                         Vector2? position = null, Vector2? size = null,
                         Color? backgroundColor = null)
```

### Element Management

```csharp
// Update content
void UpdateLabelText(string id, string newText)

// Visibility
void SetElementVisibility(string id, bool visible)

// Access elements
GameObject GetUIElement(string id)
T GetUIElement<T>(string id) where T : Component

// Cleanup
void DestroyElement(string id)
void ClearAllElements()
```

### Utility Functions

```csharp
// Temporary messages
void ShowTemporaryMessage(string message, float duration = 3f, Canvas target = null)

// Canvas access
Canvas DefaultCanvas { get; }
Canvas WorldCanvas { get; }
Canvas OverlayCanvas { get; }
```

## 🎯 Use Cases

### RTS Game HUD
```csharp
void CreateRTSHUD()
{
    Canvas hud = uiManager.OverlayCanvas;
    
    // Resource display
    uiManager.CreatePanel("resources", hud, new Vector2(0, 450), new Vector2(600, 80));
    uiManager.CreateLabel("gold", "Gold: 1000", hud, new Vector2(-200, 450));
    uiManager.CreateLabel("wood", "Wood: 500", hud, new Vector2(0, 450));
    uiManager.CreateLabel("food", "Food: 50/100", hud, new Vector2(200, 450));
    
    // Selected unit info
    uiManager.CreatePanel("unit_info", hud, new Vector2(0, -400), new Vector2(400, 150));
}
```

### Health Bars
```csharp
void CreateHealthBar(Transform target, float health, float maxHealth)
{
    string id = $"health_{target.GetInstanceID()}";
    
    // Background
    uiManager.CreatePanel($"{id}_bg", worldCanvas, Vector2.zero, 
        new Vector2(100, 8), Color.red);
    
    // Fill
    float fillWidth = (health / maxHealth) * 100f;
    uiManager.CreatePanel($"{id}_fill", worldCanvas, Vector2.zero, 
        new Vector2(fillWidth, 8), Color.green);
}
```

### Notification System
```csharp
public class NotificationManager : MonoBehaviour
{
    private UIManager uiManager;
    
    void Start()
    {
        uiManager = UIManager.Instance;
    }
    
    public void ShowAchievement(string title, string description)
    {
        Canvas notifications = uiManager.GetOrCreateCanvas("Notifications");
        
        string id = $"achievement_{System.DateTime.Now.Ticks}";
        
        RectTransform panel = uiManager.CreatePanel($"{id}_panel", notifications,
            new Vector2(400, 300), new Vector2(350, 100), new Color(0.2f, 0.2f, 0.2f, 0.9f));
        
        uiManager.CreateLabel($"{id}_title", title, notifications,
            new Vector2(400, 320), new Vector2(330, 30));
        uiManager.CreateLabel($"{id}_desc", description, notifications,
            new Vector2(400, 290), new Vector2(330, 20));
            
        // Auto-remove after 4 seconds
        StartCoroutine(RemoveAfterDelay(id, 4f));
    }
}
```

## 🐛 Troubleshooting

### Common Issues

**Labels not appearing:**
- Check if the Canvas is active
- Verify the position is within screen bounds
- Ensure the Canvas has a CanvasScaler

**World labels behind objects:**
- Adjust the Canvas sorting order
- Check the WorldLabelTracker settings
- Verify the camera reference is correct

**Performance issues:**
- Limit the number of world labels
- Use object pooling for frequently created/destroyed elements
- Disable unused canvases

**TextMeshPro not working:**
- Import TextMeshPro via Window > TextMeshPro > Import TMP Essential Resources
- Assign a TMP font asset in UISettings

## 🔮 Advanced Features

### Custom Element Types

```csharp
public class CustomProgressBar
{
    private UIManager uiManager;
    private string barId;
    
    public CustomProgressBar(string id, Canvas parent, Vector2 position, Vector2 size)
    {
        uiManager = UIManager.Instance;
        barId = id;
        
        // Create background
        uiManager.CreatePanel($"{id}_bg", parent, position, size, Color.gray);
        
        // Create fill
        uiManager.CreatePanel($"{id}_fill", parent, position, 
            new Vector2(0, size.y), Color.blue);
    }
    
    public void SetProgress(float progress)
    {
        var fillPanel = uiManager.GetUIElement<RectTransform>($"{barId}_fill");
        var bgPanel = uiManager.GetUIElement<RectTransform>($"{barId}_bg");
        
        float width = bgPanel.sizeDelta.x * Mathf.Clamp01(progress);
        fillPanel.sizeDelta = new Vector2(width, fillPanel.sizeDelta.y);
    }
}
```

### Integration with Game Systems

```csharp
// Listen to game events and update UI
public class UIController : MonoBehaviour
{
    void Start()
    {
        GameManager.OnHealthChanged += UpdateHealthDisplay;
        GameManager.OnScoreChanged += UpdateScoreDisplay;
        GameManager.OnLevelComplete += ShowLevelCompleteUI;
    }
    
    void UpdateHealthDisplay(float health)
    {
        UIManager.Instance.UpdateLabelText("health", $"Health: {health:F0}");
    }
}
```

This programmatic UI system provides complete flexibility for creating dynamic, responsive user interfaces without requiring any pre-built UI scenes or prefabs. Perfect for games that need runtime UI generation, such as RTS games, RPGs with dynamic inventories, or any application requiring flexible UI systems.

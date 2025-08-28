# Unity Hybrid Special Ability ATB Combat System
## Combat Mechanics Architecture - ProjectZero

**Version:** 1.0 (Unity Port)  
**Date:** August 28, 2025  
**System Type:** Real-Time Combat with ATB Special Abilities  
**Integration Status:** Phase 3 Development Target  
**Engine:** Unity 2022.3 LTS

---

## 🎯 **Core Vision**

A hybrid combat system where basic combat flows freely in real-time, while special abilities are gated by Active Time Battle (ATB) gauges. This creates natural tactical moments when powerful abilities become available, encouraging strategic timing and resource management.

### **Design Goals**
- **Fluid Core Combat**: Movement and basic attacks flow freely without restrictions
- **Strategic Ability Usage**: Special abilities require ATB resource management
- **Tactical Moments**: ATB-ready abilities create natural decision points
- **Squad Coordination**: Multiple ATB gauges enable complex tactical combinations

---

## 🏗️ **Unity System Architecture Integration**

### **Integration with Unity GameStateManager**

```csharp
// Extended tactical states for ATB system
public enum TacticalState
{
    RealTime,           // Normal speed combat (1.0x)
    TacticalPause,      // Full pause for command queuing  
    SlowMotion,         // Tactical mode for ability coordination (0.3x)
    CommandPlanning,    // ATB ability planning phase
    AbilityExecution    // Special ability execution mode
}

// Unity GameStateManager with ATB integration
public class GameStateManager : MonoBehaviour
{
    [Header("ATB Integration")]
    public UnityEvent OnATBTacticalSuggestion;
    public UnityEvent OnATBModeEnter;
    public UnityEvent OnATBModeExit;
    
    public void EnterATBTacticalMode()
    {
        SetTacticalState(TacticalState.SlowMotion);
        OnATBModeEnter?.Invoke();
    }
    
    public void ExitATBTacticalMode()
    {
        SetTacticalState(TacticalState.RealTime);
        OnATBModeExit?.Invoke();
    }
    
    public bool IsInATBMode()
    {
        return currentTacticalState == TacticalState.SlowMotion || 
               currentTacticalState == TacticalState.CommandPlanning;
    }
}
```

### **Unity Squad Manager ATB Extensions**

```csharp
// Enhanced SquadManager for Unity ATB abilities
public class SquadManager : MonoBehaviour
{
    [Header("ATB Integration")]
    public ATBManager atbManager;
    
    [Header("ATB Events")]
    public UnityEvent<ATBAbilityType, Vector3> OnATBAbilityExecuted;
    
    public void ProcessATBAbility(ATBAbilityType abilityType, Vector3 targetLocation, GameObject targetObject = null)
    {
        // Find unit with ready ATB for this ability type
        SelectableUnit readyUnit = FindUnitWithReadyATB(abilityType);
        
        if (readyUnit != null)
        {
            readyUnit.ExecuteATBAbility(abilityType, targetLocation, targetObject);
            OnATBAbilityExecuted?.Invoke(abilityType, targetLocation);
        }
    }
    
    public void QueueATBAbility(int unitIndex, ATBAbilityType abilityType, float executionDelay)
    {
        if (GameStateManager.Instance.currentTacticalState == TacticalState.Paused)
        {
            StartCoroutine(ExecuteDelayedATBAbility(unitIndex, abilityType, executionDelay));
        }
    }
    
    public bool IsATBReadyForUnit(int unitIndex)
    {
        if (unitIndex < squadMembers.Count)
        {
            return squadMembers[unitIndex].GetATBProgress() >= 100f;
        }
        return false;
    }
    
    public int GetUnitsWithReadyATB()
    {
        int readyCount = 0;
        foreach (var unit in squadMembers)
        {
            if (unit.GetATBProgress() >= 100f)
                readyCount++;
        }
        return readyCount;
    }
}
```

---

## ⚙️ **Unity Combat System Rules**

### **Two-Mode System**

#### **Real-Time Mode (Default)**
- **Basic Actions**: Movement and basic attacks execute immediately
- **ATB Abilities**: Special abilities available when gauge is full
- **Time Scale**: Time.timeScale = 1.0f (normal speed)
- **Flow**: Continuous action with periodic ability opportunities

#### **Tactical Mode (Player Controlled)**
- **Basic Actions**: Still execute immediately but at 0.3x speed using Time.timeScale
- **ATB Abilities**: Can be queued and planned for precise timing
- **Time Scale**: Time.timeScale = 0.3f for better decision-making
- **Flow**: Slowed pace allows careful ability coordination

### **Unity Mode Transition Rules**
1. **Manual Entry**: Player activates tactical mode at any time
2. **Ability-Triggered Entry**: System suggests tactical mode when multiple abilities ready
3. **Auto-Exit**: Coroutine returns to real-time after 5-8 seconds (configurable)
4. **Instant Exit**: Player can exit tactical mode immediately

---

## ⏱️ **Unity ATB System Architecture**

### **Core Unity ATB Components**

#### **ATBManager (Unity MonoBehaviour)**
**Location:** `Assets/_Project/Scripts/Combat/ATBManager.cs`

```csharp
public class ATBManager : MonoBehaviour
{
    [Header("ATB Configuration")]
    public ATBSettings settings;
    
    [Header("ATB Events")]
    public UnityEvent<SelectableUnit> OnATBReady;
    public UnityEvent OnTacticalSuggested;
    
    private Dictionary<SelectableUnit, ATBUnitData> unitATBData = new Dictionary<SelectableUnit, ATBUnitData>();
    
    public void InitializeATBSystem(List<SelectableUnit> squadUnits)
    {
        foreach (var unit in squadUnits)
        {
            ATBUnitData data = new ATBUnitData
            {
                currentATB = 0f,
                fillRate = settings.baseFillRate,
                abilities = unit.GetComponent<ATBAbilities>()
            };
            unitATBData[unit] = data;
        }
        
        StartCoroutine(ATBUpdateLoop());
    }
    
    private IEnumerator ATBUpdateLoop()
    {
        while (true)
        {
            float deltaTime = Time.deltaTime;
            
            foreach (var kvp in unitATBData)
            {
                UpdateUnitATB(kvp.Key, kvp.Value, deltaTime);
            }
            
            CheckTacticalOpportunities();
            
            yield return null; // Wait one frame
        }
    }
    
    private void UpdateUnitATB(SelectableUnit unit, ATBUnitData data, float deltaTime)
    {
        if (data.currentATB < 100f)
        {
            data.currentATB += data.fillRate * deltaTime;
            
            if (data.currentATB >= 100f)
            {
                data.currentATB = 100f;
                OnATBReady?.Invoke(unit);
            }
        }
    }
    
    public float GetATBProgress(SelectableUnit unit)
    {
        return unitATBData.ContainsKey(unit) ? unitATBData[unit].currentATB : 0f;
    }
    
    public bool IsATBReady(SelectableUnit unit)
    {
        return GetATBProgress(unit) >= 100f;
    }
    
    public void ConsumeATB(SelectableUnit unit, float amount = 100f)
    {
        if (unitATBData.ContainsKey(unit))
        {
            unitATBData[unit].currentATB = Mathf.Max(0f, unitATBData[unit].currentATB - amount);
        }
    }
    
    private bool ShouldSuggestTacticalMode()
    {
        int readyUnits = 0;
        foreach (var data in unitATBData.Values)
        {
            if (data.currentATB >= 100f)
                readyUnits++;
        }
        
        // Suggest tactical mode when 2+ units have abilities ready
        return readyUnits >= 2;
    }
}
```

#### **ATB Component (Unity Component)**
**Location:** `Assets/_Project/Scripts/Combat/ATBComponent.cs`

```csharp
public class ATBComponent : MonoBehaviour
{
    [Header("ATB Configuration")]
    [SerializeField] private float currentATB = 0f;
    [SerializeField] private float fillRate = 10f;
    
    [Header("Abilities")]
    public ATBAbilitySet availableAbilities;
    
    [Header("Events")]
    public UnityEvent<float> OnATBChanged;
    public UnityEvent OnATBReady;
    
    public float ATBProgress => currentATB;
    public bool IsReady => currentATB >= 100f;
    
    public void AddATBProgress(float amount)
    {
        float oldATB = currentATB;
        currentATB = Mathf.Min(100f, currentATB + amount);
        
        if (oldATB < 100f && currentATB >= 100f)
        {
            OnATBReady?.Invoke();
        }
        
        OnATBChanged?.Invoke(currentATB);
    }
    
    public void ConsumeATB(float amount = 100f)
    {
        currentATB = Mathf.Max(0f, currentATB - amount);
        OnATBChanged?.Invoke(currentATB);
    }
    
    public bool CanUseAbility(ATBAbilityType abilityType)
    {
        return IsReady && availableAbilities.HasAbility(abilityType);
    }
    
    public void UseAbility(ATBAbilityType abilityType, Vector3 targetLocation, GameObject target = null)
    {
        if (!CanUseAbility(abilityType))
        {
            Debug.LogWarning($"Cannot use ability {abilityType} - ATB not ready or ability not available");
            return;
        }
        
        ExecuteAbility(abilityType, targetLocation, target);
        ConsumeATB(availableAbilities.GetAbilityCost(abilityType));
    }
    
    private void ExecuteAbility(ATBAbilityType abilityType, Vector3 targetLocation, GameObject target)
    {
        // Implement ability execution logic
        Debug.Log($"Executing ATB ability: {abilityType} at {targetLocation}");
        
        // Start ability coroutine
        StartCoroutine(AbilityExecutionCoroutine(abilityType, targetLocation, target));
    }
    
    private IEnumerator AbilityExecutionCoroutine(ATBAbilityType abilityType, Vector3 targetLocation, GameObject target)
    {
        // Implement ability-specific behavior
        switch (abilityType)
        {
            case ATBAbilityType.OverwatchShot:
                yield return ExecuteOverwatchShot(targetLocation);
                break;
            case ATBAbilityType.SuppressiveFire:
                yield return ExecuteSuppressiveFire(targetLocation);
                break;
            // Add more abilities...
        }
    }
}
```

### **Unity ATB Data Structures**

```csharp
[System.Serializable]
public class ATBSettings
{
    [Header("Fill Rates")]
    public float baseFillRate = 10f;
    public float combatMultiplier = 1.5f;
    public float tacticalModeMultiplier = 0.5f;
    
    [Header("Tactical Mode")]
    public float autoExitTimer = 6f;
    public int minimumReadyUnitsForSuggestion = 2;
    
    [Header("Performance")]
    public float updateFrequency = 0.1f; // 10Hz updates
}

[System.Serializable]  
public class ATBUnitData
{
    public float currentATB;
    public float fillRate;
    public ATBAbilities abilities;
    public bool isInCombat;
    public float lastDamageTime;
}

public enum ATBAbilityType
{
    OverwatchShot,
    SuppressiveFire,
    TacticalReload,
    FirstAid,
    Grenade,
    FlashBang,
    SmokeGrenade
}
```

---

## ⚔️ **Unity Combat Flow Design**

### **Real-Time Combat Loop**
```csharp
public class CombatManager : MonoBehaviour
{
    private void Update()
    {
        if (GameStateManager.Instance.currentTacticalState == TacticalState.RealTime)
        {
            // Process real-time combat
            ProcessBasicCombat();
            UpdateATBGauges(Time.deltaTime);
            CheckForTacticalOpportunities();
        }
        else if (GameStateManager.Instance.currentTacticalState == TacticalState.SlowMotion)
        {
            // Process slowed combat for tactical decision making
            ProcessBasicCombat();
            UpdateATBGauges(Time.deltaTime);
            ProcessQueuedATBAbilities();
        }
    }
    
    private void ProcessBasicCombat()
    {
        // Basic attacks and movement happen regardless of ATB state
        foreach (var unit in activeUnits)
        {
            unit.ProcessBasicActions(Time.deltaTime);
        }
    }
    
    private void CheckForTacticalOpportunities()
    {
        if (atbManager.ShouldSuggestTacticalMode())
        {
            // Suggest but don't force tactical mode
            uiManager.ShowTacticalModeHint();
        }
    }
}
```

### **ATB Ability Execution Flow**
```csharp
public class SelectableUnit : MonoBehaviour
{
    [Header("Combat Components")]
    public Health health;
    public ATBComponent atbComponent;
    public WeaponSystem weaponSystem;
    
    public void ExecuteATBAbility(ATBAbilityType abilityType, Vector3 targetLocation, GameObject target = null)
    {
        if (!atbComponent.CanUseAbility(abilityType))
        {
            Debug.LogWarning($"Unit {name} cannot use ability {abilityType}");
            return;
        }
        
        // Enter ability execution state
        isExecutingAbility = true;
        
        // Use the ability
        atbComponent.UseAbility(abilityType, targetLocation, target);
        
        // Start execution coroutine
        StartCoroutine(AbilityExecutionSequence(abilityType, targetLocation, target));
    }
    
    private IEnumerator AbilityExecutionSequence(ATBAbilityType abilityType, Vector3 targetLocation, GameObject target)
    {
        // Pause basic actions during ability
        canPerformBasicActions = false;
        
        // Execute ability-specific logic
        yield return ExecuteSpecificAbility(abilityType, targetLocation, target);
        
        // Resume basic actions
        canPerformBasicActions = true;
        isExecutingAbility = false;
    }
}
```

---

## 🎮 **Unity ATB UI Integration**

### **ATB Gauge UI Component**
```csharp
public class ATBGaugeUI : MonoBehaviour
{
    [Header("UI References")]
    public UnityEngine.UI.Slider atbSlider;
    public UnityEngine.UI.Image fillImage;
    public UnityEngine.UI.Text abilityText;
    
    [Header("Visual Settings")]
    public Color readyColor = Color.yellow;
    public Color chargingColor = Color.blue;
    
    private ATBComponent trackedUnit;
    
    public void Initialize(ATBComponent atbComponent)
    {
        trackedUnit = atbComponent;
        
        // Subscribe to ATB events
        trackedUnit.OnATBChanged.AddListener(UpdateGauge);
        trackedUnit.OnATBReady.AddListener(OnATBReady);
    }
    
    private void UpdateGauge(float atbValue)
    {
        atbSlider.value = atbValue / 100f;
        fillImage.color = Color.Lerp(chargingColor, readyColor, atbValue / 100f);
    }
    
    private void OnATBReady()
    {
        // Add visual effect when ATB is ready
        StartCoroutine(PulseEffect());
    }
    
    private IEnumerator PulseEffect()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float alpha = Mathf.PingPong(elapsed * 4f, 1f);
            fillImage.color = Color.Lerp(readyColor, Color.white, alpha);
            elapsed += Time.unscaledDeltaTime; // Ignore time scale
            yield return null;
        }
        
        fillImage.color = readyColor;
    }
}
```

---

## 🔧 **Unity Performance Considerations**

### **Update Frequency Optimization**
```csharp
public class ATBManager : MonoBehaviour
{
    [Header("Performance Settings")]
    public float updateInterval = 0.1f; // 10Hz instead of 60Hz
    
    private void Start()
    {
        // Use coroutine for controlled update frequency
        StartCoroutine(OptimizedATBUpdate());
    }
    
    private IEnumerator OptimizedATBUpdate()
    {
        WaitForSeconds waitInterval = new WaitForSeconds(updateInterval);
        
        while (true)
        {
            // Only update ATB when in combat
            if (IsAnyCombatActive())
            {
                UpdateAllATBGauges(updateInterval);
            }
            
            yield return waitInterval;
        }
    }
}
```

### **Unity Memory Management**
```csharp
public class ATBManager : MonoBehaviour
{
    // Use object pooling for ability effects
    public class ATBEffectPool : MonoBehaviour
    {
        private Queue<GameObject> effectPool = new Queue<GameObject>();
        
        public GameObject GetEffect(ATBAbilityType abilityType)
        {
            if (effectPool.Count > 0)
            {
                return effectPool.Dequeue();
            }
            
            return Instantiate(GetEffectPrefab(abilityType));
        }
        
        public void ReturnEffect(GameObject effect)
        {
            effect.SetActive(false);
            effectPool.Enqueue(effect);
        }
    }
}
```

---

## 🎯 **Unity Integration Examples**

### **Basic Unity ATB Setup**
```csharp
// Example setup in Unity scene
public class CombatSceneSetup : MonoBehaviour
{
    [Header("Combat Systems")]
    public GameStateManager gameStateManager;
    public SquadManager squadManager;
    public ATBManager atbManager;
    
    [Header("UI")]
    public GameObject atbGaugePrefab;
    public Transform uiParent;
    
    private void Start()
    {
        SetupCombatSystems();
    }
    
    private void SetupCombatSystems()
    {
        // Initialize ATB system with current squad
        atbManager.InitializeATBSystem(squadManager.squadMembers);
        
        // Create UI gauges for each unit
        foreach (var unit in squadManager.squadMembers)
        {
            GameObject gaugeUI = Instantiate(atbGaugePrefab, uiParent);
            ATBGaugeUI gauge = gaugeUI.GetComponent<ATBGaugeUI>();
            gauge.Initialize(unit.GetComponent<ATBComponent>());
        }
        
        // Connect systems
        atbManager.OnTacticalSuggested.AddListener(() => {
            gameStateManager.EnterATBTacticalMode();
        });
    }
}
```

---

## 📊 **Unity Testing & Validation**

### **Unity ATB Testing Script**
```csharp
public class ATBSystemTester : MonoBehaviour
{
    public ATBManager atbManager;
    public SelectableUnit testUnit;
    
    [ContextMenu("Test ATB Fill")]
    public void TestATBFill()
    {
        Debug.Log("Testing ATB fill rate...");
        StartCoroutine(MonitorATBFill());
    }
    
    private IEnumerator MonitorATBFill()
    {
        float startTime = Time.time;
        
        while (atbManager.GetATBProgress(testUnit) < 100f)
        {
            float progress = atbManager.GetATBProgress(testUnit);
            Debug.Log($"ATB Progress: {progress:F1}%");
            yield return new WaitForSeconds(1f);
        }
        
        float fillTime = Time.time - startTime;
        Debug.Log($"ATB reached 100% in {fillTime:F1} seconds");
    }
    
    [ContextMenu("Test Tactical Mode")]
    public void TestTacticalMode()
    {
        GameStateManager.Instance.EnterATBTacticalMode();
        Debug.Log($"Entered tactical mode. Time scale: {Time.timeScale}");
    }
}
```

---

This Unity combat architecture maintains the hybrid ATB design while leveraging Unity's strengths: coroutines for smooth timing, Unity Events for loose coupling, and MonoBehaviour components for modular design.

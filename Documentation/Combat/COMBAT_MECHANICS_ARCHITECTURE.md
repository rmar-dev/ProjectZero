# Hybrid Special Ability ATB Combat System
## Combat Mechanics Architecture - ProjectHive 5.5

**Version:** 1.0  
**Date:** August 25, 2025  
**System Type:** Real-Time Combat with ATB Special Abilities  
**Integration Status:** Phase 3 Development Target

---

## 🎯 **Core Vision**

A hybrid combat system where basic combat flows freely in real-time, while special abilities are gated by Active Time Battle (ATB) gauges. This creates natural tactical moments when powerful abilities become available, encouraging strategic timing and resource management.

### **Design Goals**
- **Fluid Core Combat**: Movement and basic attacks flow freely without restrictions
- **Strategic Ability Usage**: Special abilities require ATB resource management
- **Tactical Moments**: ATB-ready abilities create natural decision points
- **Squad Coordination**: Multiple ATB gauges enable complex tactical combinations

---

## 🏗️ **System Architecture Integration**

### **Integration with Existing Systems**

#### **GameStateManager Integration**
```cpp
// Extended tactical states for ATB system
enum class ETacticalState : uint8
{
    RealTime,           // Normal speed combat (1.0x)
    TacticalPause,      // Full pause for command queuing  
    SlowMotion,         // Tactical mode for ability coordination (0.3x)
    CommandPlanning,    // ATB ability planning phase
    AbilityExecution    // Special ability execution mode
};

// New ATB-specific state management
class UGameStateManager 
{
    // ATB system integration
    UFUNCTION(BlueprintCallable)
    void EnterATBTacticalMode();
    
    UFUNCTION(BlueprintCallable) 
    void ExitATBTacticalMode();
    
    // ATB state queries
    UFUNCTION(BlueprintPure)
    bool IsInATBMode() const;
    
    UFUNCTION(BlueprintPure)
    bool ShouldSuggestTacticalMode() const;
};
```

#### **Squad Manager ATB Extensions**
```cpp
// Enhanced SquadManager for ATB abilities
class USquadManager 
{
    // ATB Command Processing
    UFUNCTION(BlueprintCallable)
    void ProcessATBAbility(EATBAbilityType AbilityType, const FVector& TargetLocation, AActor* TargetActor);
    
    UFUNCTION(BlueprintCallable)
    void QueueATBAbility(int32 UnitIndex, EATBAbilityType AbilityType, float ExecutionDelay);
    
    // ATB State Management
    UFUNCTION(BlueprintPure)
    bool IsATBReadyForUnit(int32 UnitIndex) const;
    
    UFUNCTION(BlueprintPure)
    int32 GetUnitsWithReadyATB() const;
};
```

---

## ⚙️ **Combat System Rules**

### **Two-Mode System**

#### **Real-Time Mode (Default)**
- **Basic Actions**: Movement and basic attacks execute immediately
- **ATB Abilities**: Special abilities available when gauge is full
- **Time Scale**: 1.0x normal speed
- **Flow**: Continuous action with periodic ability opportunities

#### **Tactical Mode (Player Controlled)**
- **Basic Actions**: Still execute immediately but at 0.3x speed
- **ATB Abilities**: Can be queued and planned for precise timing
- **Time Scale**: 0.3x speed for better decision-making
- **Flow**: Slowed pace allows careful ability coordination

### **Mode Transition Rules**
1. **Manual Entry**: Player activates tactical mode at any time
2. **Ability-Triggered Entry**: System suggests tactical mode when multiple abilities ready
3. **Auto-Exit**: Returns to real-time after 5-8 seconds (configurable)
4. **Instant Exit**: Player can exit tactical mode immediately

---

## ⏱️ **ATB System Architecture**

### **Core ATB Components**

#### **ATB Manager (New System Component)**
**Location:** `Source/Combat/Public/ATBManager.h`

```cpp
UCLASS(BlueprintType, Blueprintable)
class COMBAT_API UATBManager : public UObject
{
    GENERATED_BODY()

public:
    // ATB Lifecycle
    UFUNCTION(BlueprintCallable)
    void InitializeATBSystem(const TArray<ASelectableUnit*>& SquadUnits);
    
    UFUNCTION(BlueprintCallable)
    void TickATB(float DeltaTime);
    
    // ATB State Management
    UFUNCTION(BlueprintPure)
    float GetATBProgress(ASelectableUnit* Unit) const;
    
    UFUNCTION(BlueprintPure)
    bool IsATBReady(ASelectableUnit* Unit) const;
    
    UFUNCTION(BlueprintCallable)
    void ConsumeATB(ASelectableUnit* Unit, float Amount = 100.0f);
    
    // Tactical Opportunity Detection
    UFUNCTION(BlueprintPure)
    bool ShouldSuggestTacticalMode() const;
    
    UFUNCTION(BlueprintPure)
    TArray<ASelectableUnit*> GetUnitsWithReadyATB() const;

private:
    UPROPERTY()
    TMap<ASelectableUnit*, FATBUnitData> UnitATBData;
    
    UPROPERTY(EditAnywhere)
    FATBSettings ATBSettings;
};
```

#### **ATB Unit Component**
**Location:** `Source/Combat/Public/ATBComponent.h`

```cpp
UCLASS(ClassGroup=(Combat), meta=(BlueprintSpawnableComponent))
class COMBAT_API UATBComponent : public UActorComponent
{
    GENERATED_BODY()

public:
    // ATB Progress
    UFUNCTION(BlueprintPure)
    float GetATBProgress() const { return CurrentATB; }
    
    UFUNCTION(BlueprintPure)
    bool IsATBReady() const { return CurrentATB >= 100.0f; }
    
    // ATB Filling
    UFUNCTION(BlueprintCallable)
    void AddATBProgress(float Amount);
    
    UFUNCTION(BlueprintCallable)
    void ConsumeATB(float Amount = 100.0f);
    
    // Ability Management
    UFUNCTION(BlueprintCallable)
    bool CanUseAbility(EATBAbilityType AbilityType) const;
    
    UFUNCTION(BlueprintCallable)
    void UseAbility(EATBAbilityType AbilityType, const FVector& TargetLocation, AActor* Target = nullptr);

private:
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, meta = (AllowPrivateAccess = "true"))
    float CurrentATB = 0.0f;
    
    UPROPERTY(EditAnywhere)
    FATBAbilitySet AvailableAbilities;
    
    UPROPERTY(EditAnywhere)
    float ATBFillRate = 10.0f;
};
```

### **ATB Data Structures**

```cpp
USTRUCT(BlueprintType)
struct FATBSettings
{
    GENERATED_BODY()

    // Fill Rates
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    float BaseFillRate = 10.0f;
    
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    float CombatFillMultiplier = 2.0f;
    
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    float TacticalModePenalty = 0.5f;
    
    // Tactical Triggers
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    int32 MinUnitsForTacticalSuggestion = 2;
    
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    float TacticalModeAutoExitTime = 6.0f;
};

USTRUCT(BlueprintType)
struct FATBUnitData
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite)
    float CurrentATB = 0.0f;
    
    UPROPERTY(BlueprintReadWrite)
    float LastATBUsage = 0.0f;
    
    UPROPERTY(BlueprintReadWrite)
    bool bInCombat = false;
    
    UPROPERTY(BlueprintReadWrite)
    EATBAbilityType LastUsedAbility = EATBAbilityType::None;
};

UENUM(BlueprintType)
enum class EATBAbilityType : uint8
{
    None                UMETA(DisplayName = "None"),
    
    // Precision Abilities
    PrecisionShot       UMETA(DisplayName = "Precision Shot"),
    MedicalStim         UMETA(DisplayName = "Medical Stim"),
    HackTerminal        UMETA(DisplayName = "Hack Terminal"),
    
    // Area Abilities  
    FragGrenade         UMETA(DisplayName = "Frag Grenade"),
    SmokeGrenade        UMETA(DisplayName = "Smoke Grenade"),
    BreachCharge        UMETA(DisplayName = "Breach Charge"),
    
    // Support Abilities
    SquadRally          UMETA(DisplayName = "Squad Rally"),
    TacticalScan        UMETA(DisplayName = "Tactical Scan"),
    SuppressiveFire     UMETA(DisplayName = "Suppressive Fire"),
    
    // Reactive Abilities
    Overwatch           UMETA(DisplayName = "Overwatch"),
    EmergencyStim       UMETA(DisplayName = "Emergency Stim"),
    DefensiveSmoke      UMETA(DisplayName = "Defensive Smoke")
};
```

---

## 🎮 **Enhanced Input Integration**

### **Extended Input Actions**
Building on existing Enhanced Input system:

```cpp
// New ATB-specific input actions (add to existing SquadPlayerController)
UPROPERTY(EditAnywhere, Category = "Input Actions|ATB")
UInputAction* TacticalModeToggleAction;

UPROPERTY(EditAnywhere, Category = "Input Actions|ATB") 
UInputAction* PrecisionAbilityAction;

UPROPERTY(EditAnywhere, Category = "Input Actions|ATB")
UInputAction* AggressiveAbilityAction;

UPROPERTY(EditAnywhere, Category = "Input Actions|ATB")
UInputAction* DefensiveAbilityAction;

UPROPERTY(EditAnywhere, Category = "Input Actions|ATB")
UInputAction* UtilityAbilityAction;
```

### **Input Processing Flow**
```cpp
// Enhanced contextual command processing for ATB abilities
void ASquadPlayerController::ProcessRightClickCommand(const FVector& WorldLocation, AActor* TargetActor)
{
    // Check if any squad members have ATB ready
    TArray<ASelectableUnit*> ReadyUnits = ATBManager->GetUnitsWithReadyATB();
    
    if (ReadyUnits.Num() > 0)
    {
        // Show extended context menu with ATB abilities
        ShowATBContextMenu(WorldLocation, TargetActor, ReadyUnits);
    }
    else
    {
        // Process normal contextual command
        SquadManager->ProcessContextualCommand(WorldLocation, TargetActor);
    }
}
```

---

## ⚡ **Combat Flow Design**

### **ATB Fill Rate System**

#### **Activity-Based Fill Rates**
```cpp
class UATBComponent
{
private:
    void UpdateATBProgress(float DeltaTime)
    {
        float FillRate = BaseFillRate;
        
        // Combat activity bonus
        if (bInCombat && IsEngagingEnemy())
        {
            FillRate *= CombatFillMultiplier;
        }
        
        // Tactical mode penalty
        if (GameStateManager->GetTacticalState() == ETacticalState::SlowMotion)
        {
            FillRate *= TacticalModePenalty;
        }
        
        // Apply time-based progression
        CurrentATB = FMath::Clamp(CurrentATB + (FillRate * DeltaTime), 0.0f, 100.0f);
        
        // Trigger ready notification
        if (CurrentATB >= 100.0f && !bWasATBReady)
        {
            OnATBReady.Broadcast(GetOwner());
        }
    }
};
```

### **Tactical Opportunity Detection**
```cpp
bool UATBManager::ShouldSuggestTacticalMode() const
{
    // Check for multiple ready abilities
    int32 ReadyCount = GetUnitsWithReadyATB().Num();
    if (ReadyCount >= ATBSettings.MinUnitsForTacticalSuggestion)
    {
        return true;
    }
    
    // Check for high-value targets
    if (DetectHighValueTargets() && ReadyCount > 0)
    {
        return true;
    }
    
    // Check for critical situations
    if (IsSquadInDanger() && HasMedicalAbilityReady())
    {
        return true;
    }
    
    return false;
}
```

---

## 🧩 **Ability System Architecture**

### **Ability Definition Framework**
```cpp
USTRUCT(BlueprintType)
struct FATBAbilityDefinition
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    EATBAbilityType AbilityType;
    
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    FString DisplayName;
    
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    float ATBCost = 100.0f;
    
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    float Cooldown = 0.0f;
    
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    float Range = 1000.0f;
    
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    EAbilityTargetType TargetType;
    
    UPROPERTY(EditAnywhere, BlueprintReadWrite)
    TSubclassOf<class UATBAbilityEffect> EffectClass;
};
```

### **Ability Effect System**
```cpp
UCLASS(Abstract, BlueprintType, Blueprintable)
class COMBAT_API UATBAbilityEffect : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintImplementableEvent)
    void ExecuteAbility(ASelectableUnit* User, const FVector& TargetLocation, AActor* TargetActor);
    
    UFUNCTION(BlueprintImplementableEvent)
    bool CanExecuteAbility(ASelectableUnit* User, const FVector& TargetLocation, AActor* TargetActor) const;
    
    UFUNCTION(BlueprintImplementableEvent)
    void GetAbilityPreview(ASelectableUnit* User, const FVector& TargetLocation, TArray<FVector>& PreviewPoints) const;

protected:
    UPROPERTY(EditAnywhere, BlueprintReadOnly)
    FATBAbilityDefinition AbilityData;
};
```

---

## 🎨 **UI Integration Architecture**

### **ATB UI Components**
**Location:** `Source/CoreUI/Public/ATBUserInterface.h`

```cpp
UCLASS()
class COREUI_API UATBUserInterface : public UUserWidget
{
    GENERATED_BODY()

public:
    // ATB Display
    UFUNCTION(BlueprintImplementableEvent)
    void UpdateATBGauge(ASelectableUnit* Unit, float ATBProgress);
    
    UFUNCTION(BlueprintImplementableEvent)
    void ShowAbilityReady(ASelectableUnit* Unit, EATBAbilityType AbilityType);
    
    UFUNCTION(BlueprintImplementableEvent)
    void ShowTacticalSuggestion(const TArray<ASelectableUnit*>& ReadyUnits);
    
    // Context Menu
    UFUNCTION(BlueprintImplementableEvent)
    void ShowATBContextMenu(const FVector& Location, const TArray<FATBAbilityOption>& Options);
    
    UFUNCTION(BlueprintImplementableEvent)
    void HideATBContextMenu();

protected:
    // Widget Bindings
    UPROPERTY(meta = (BindWidget))
    class UCanvasPanel* ATBGaugeContainer;
    
    UPROPERTY(meta = (BindWidget))
    class UWidgetComponent* TacticalSuggestionWidget;
};
```

### **Visual Feedback System**
- **ATB Gauges**: Circular progress indicators above each squad member
- **Ability Icons**: Lit icons when abilities become available
- **Tactical Suggestions**: Subtle screen effects when tactical mode recommended
- **Action Previews**: Show ability effects before execution in tactical mode
- **Combination Indicators**: Visual connections between synergistic abilities

---

## 📊 **Balance Framework**

### **ATB Balance Settings**
```cpp
USTRUCT(BlueprintType)
struct FATBBalanceSettings
{
    GENERATED_BODY()

    // Difficulty Scaling
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Difficulty")
    TMap<EDifficultyLevel, float> FillRateMultipliers = {
        {EDifficultyLevel::Easy, 1.5f},
        {EDifficultyLevel::Normal, 1.0f}, 
        {EDifficultyLevel::Hard, 0.75f},
        {EDifficultyLevel::Expert, 0.5f}
    };
    
    // Ability Cost Scaling
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Balance")
    TMap<EATBAbilityType, float> AbilityCosts;
    
    // Cooldown System
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Balance")
    TMap<EATBAbilityType, float> AbilityCooldowns;
};
```

---

## 🔄 **Module Integration**

### **Updated Module Dependencies**
```
ProjectHive 5.5
├── ProjectHive (Main)           # Core gameplay logic
├── CoreMechanics               # Game state, squad management
├── Combat (NEW)                # ATB system, ability management
├── Camera                      # Tactical camera control
├── Characters                  # Units, AI, combat
├── Terrain                     # Cover system, environment
├── Animations                  # Character animation systems
├── Inputs                      # Enhanced Input integration
└── CoreUi                      # User interface systems
```

### **Combat Module Structure**
```
Source/Combat/
├── Public/
│   ├── ATBManager.h
│   ├── ATBComponent.h
│   ├── ATBAbilityEffect.h
│   ├── ATBTypes.h
│   └── CombatConstants.h
├── Private/
│   ├── Combat.cpp
│   ├── ATBManager.cpp
│   ├── ATBComponent.cpp
│   └── ATBAbilityEffect.cpp
└── Combat.Build.cs
```

---

## 🚀 **Implementation Phases**

### **Phase 3.1: ATB Foundation** (Oct 11 - Oct 18, 2025)
- [ ] Create Combat module structure
- [ ] Implement ATBManager system
- [ ] Add ATBComponent to SelectableUnit
- [ ] Basic ATB gauge progression
- [ ] Integration with GameStateManager

### **Phase 3.2: Ability System** (Oct 18 - Oct 25, 2025)  
- [ ] Implement ability definition framework
- [ ] Create basic ability effects
- [ ] Add ability targeting system
- [ ] Implement ability execution pipeline
- [ ] Create ability preview system

### **Phase 3.3: UI Integration** (Oct 25 - Nov 1, 2025)
- [ ] ATB gauge UI components
- [ ] Ability ready notifications
- [ ] Tactical mode suggestions
- [ ] Extended context menus
- [ ] Visual effect integration

### **Phase 3.4: Balance & Polish** (Nov 1 - Nov 8, 2025)
- [ ] ATB fill rate tuning
- [ ] Ability cost balancing
- [ ] Difficulty scaling implementation
- [ ] Performance optimization
- [ ] Audio integration

---

## 📈 **Success Metrics**

### **Technical Metrics**
- ATB system maintains 60+ FPS with 6 units
- Ability execution latency < 100ms
- Tactical mode transitions smooth (< 0.3s)
- Memory usage increase < 50MB for ATB system

### **Gameplay Metrics** 
- Players use 60%+ of available ATB abilities
- 70%+ of players engage with tactical mode
- Average engagement duration: 90-180 seconds
- Ability combination discovery rate > 80%

---

## 💫 **Integration Benefits**

This ATB system perfectly complements your existing architecture:

1. **Preserves Squad Control Flow**: Basic squad movement and formation changes remain immediate
2. **Enhances Tactical Pause**: Existing tactical pause now has additional purpose for ability coordination
3. **Leverages Enhanced Input**: ATB abilities integrate seamlessly with existing input system
4. **Maintains Performance**: ATB only affects special abilities, not core combat loop
5. **Creates Strategic Depth**: Multiple ATB gauges enable complex squad coordination

The system transforms your tactical squad shooter into a hybrid experience that rewards both real-time skill and strategic planning, creating the perfect balance between action and tactics.

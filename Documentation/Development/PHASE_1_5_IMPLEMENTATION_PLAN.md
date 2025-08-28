# Phase 1.5 Implementation Plan
## Squad Movement & Cover Integration

**Date:** August 26, 2025  
**Phase:** 1.5 - Squad Movement & Cover Integration  
**Duration:** August 26 - September 15, 2025 (3 weeks)  
**Status:** Ready to Begin Implementation

---

## 🎯 **Phase 1.5 Overview**

### **Current Situation**
- ✅ **Phase 1 Complete** - All foundation systems working
- ✅ **Cover System Infrastructure** - BP_CoverPoint system implemented
- ✅ **Squad Control System** - Basic movement and formation working
- ❌ **Missing Link** - Units don't actually use the cover system

### **What We Need to Build**
This phase bridges the gap between the existing cover system infrastructure and the squad control system by implementing:

1. **Unit AI Cover Behavior** - Units that actively seek and use cover
2. **Squad Cover Commands** - Player controls for cover actions
3. **Combat Integration** - Cover providing actual protection

---

## 🏗️ **Technical Implementation Plan**

### **Week 1: Unit Cover AI (Aug 26 - Sep 1)**

#### **1. Extend SelectableUnit with Cover Behavior**
```cpp
// Add to SelectableUnit.h
class ACoverPoint* FindBestCoverPoint(const FVector& ThreatDirection) const;
void SeekCover(const FVector& ThreatDirection);
bool ShouldSeekCover() const;

// Cover evaluation properties
UPROPERTY(EditDefaultsOnly, Category = "Cover AI")
float CoverSearchRadius = 500.0f;

UPROPERTY(EditDefaultsOnly, Category = "Cover AI") 
float HealthThresholdForCover = 75.0f;

UPROPERTY()
ACoverPoint* CurrentCoverPoint = nullptr;
```

#### **2. Implement Cover Evaluation Algorithm**
- Distance weighting (closer = better)
- Cover type scoring (Heavy > Light > Concealment)
- Threat direction protection (cover blocks incoming fire)
- Availability checking (not already occupied)

#### **3. Add Damage-Triggered Cover Seeking**
- Override TakeDamage() to trigger cover evaluation
- Implement health threshold checking
- Add cooldown to prevent spam seeking

### **Week 2: Squad Cover Integration (Sep 1 - Sep 8)**

#### **1. Extend SquadManager with Cover Commands**
```cpp
// Add to SquadManager.h
UFUNCTION(BlueprintCallable, Category = "Squad Commands")
void TakeCover(const FVector& ThreatDirection = FVector::ZeroVector);

UFUNCTION(BlueprintCallable, Category = "Squad Commands")
void MoveToCoverPosition(ACoverPoint* TargetCover);

// Cover coordination
TArray<ACoverPoint*> FindSquadCoverPositions(const FVector& CenterLocation, const FVector& ThreatDirection) const;
void DistributeSquadToCover(const TArray<ACoverPoint*>& AvailableCover);
```

#### **2. Implement V-Key Cover Command**
- Add input action to Enhanced Input system
- Bind to SquadManager::TakeCover()
- Find threat direction from recent damage or cursor

#### **3. Add Contextual Cover Commands**
- Detect ACoverPoint objects in right-click targeting
- Show cover-specific cursor when hovering over cover
- Process cover commands in SquadPlayerController

### **Week 3: Combat Integration & Polish (Sep 8 - Sep 15)**

#### **1. Implement Cover Protection System**
```cpp
// Add to SelectableUnit TakeDamage override
float CoverDamageReduction = 0.0f;
if (CurrentCoverPoint && CurrentCoverPoint->IsAvailable())
{
    // Calculate directional protection
    FVector DamageDirection = GetDirectionFromDamageSource(DamageCauser);
    float ProtectionValue = CurrentCoverPoint->GetProtectionValue(DamageDirection);
    CoverDamageReduction = ProtectionValue;
}

// Apply damage reduction
float ActualDamage = DamageAmount * (1.0f - CoverDamageReduction);
```

#### **2. Visual Feedback Integration**
- Add cover status indicators to unit HUD
- Show cover position previews during movement commands
- Display cover effectiveness values in debug mode

#### **3. Performance Testing & Optimization**
- Test with 4-6 units simultaneously seeking cover
- Profile cover calculation performance
- Optimize cover search algorithms if needed

---

## 🎮 **Expected Gameplay Flow After Implementation**

### **Scenario 1: Automatic Cover Seeking**
1. Squad encounters enemy and takes damage
2. Units with health below 75% automatically evaluate nearby cover
3. Units path to best available cover positions
4. Units gain damage reduction while in cover

### **Scenario 2: Player-Directed Cover**
1. Player presses V key during combat
2. Squad evaluates cover relative to current threats
3. Squad members coordinate to occupy multiple cover positions
4. Formation is maintained across cover positions when possible

### **Scenario 3: Tactical Positioning**
1. Player right-clicks on specific cover position
2. Squad moves to occupy that cover area
3. Formation adapts to available cover points
4. Units gain tactical advantage from positioned cover

---

## 🎯 **Success Metrics**

### **Functional Requirements**
- [ ] Units automatically seek cover when health drops below 75%
- [ ] V key triggers coordinated squad cover behavior
- [ ] Right-click cover targeting works intuitively
- [ ] Units receive appropriate damage reduction in cover
- [ ] Multiple units can coordinate cover without conflicts

### **Performance Requirements**
- [ ] Cover calculations maintain 60+ FPS with 6 units
- [ ] Cover seeking completes within 1 second of trigger
- [ ] Memory usage increase < 10MB for cover system

### **User Experience Requirements**
- [ ] Cover behavior feels natural and responsive
- [ ] Visual feedback clearly shows cover status
- [ ] Cover commands integrate smoothly with existing controls

---

## 🚨 **Risk Mitigation**

### **Technical Risks**
1. **Performance Impact** - Cover calculations for multiple units
   - **Mitigation:** Optimize algorithms, cache results, limit update frequency

2. **AI Coordination Conflicts** - Multiple units seeking same cover
   - **Mitigation:** Implement reservation system, priority scoring

3. **Integration Complexity** - Merging with existing squad/formation systems
   - **Mitigation:** Incremental testing, modular implementation

### **Design Risks**
1. **Cover Behavior Too Aggressive** - Units constantly seeking cover
   - **Mitigation:** Proper threshold tuning, cooldown periods

2. **Player Control Conflicts** - AI overriding player commands
   - **Mitigation:** Clear priority system, player command always wins

---

## 🚀 **Implementation Priority Order**

### **Day 1-2: Foundation**
1. Add cover behavior methods to SelectableUnit
2. Implement basic cover evaluation algorithm
3. Test with single unit seeking cover

### **Day 3-5: AI Behavior**  
1. Add damage-triggered cover seeking
2. Implement cover reservation system
3. Test with multiple units

### **Day 6-10: Squad Integration**
1. Add squad-level cover commands
2. Implement V-key functionality
3. Add contextual cover targeting

### **Day 11-15: Combat & Polish**
1. Implement damage reduction system
2. Add visual feedback and UI
3. Performance testing and optimization

---

## 📖 **Documentation Requirements**

### **Code Documentation**
- API documentation for all new cover methods
- Algorithm descriptions for cover evaluation
- Integration notes for existing systems

### **Design Documentation**  
- Update Squad Movement & Interaction System document
- Create Unit Cover Behavior guide
- Document cover command usage patterns

### **Testing Documentation**
- Test cases for cover AI behavior
- Performance benchmarks and targets
- Bug tracking and resolution log

---

This implementation plan provides a clear path from the current state (Phase 1 complete, cover infrastructure ready) to full tactical squad gameplay with integrated cover mechanics. The 3-week timeline is aggressive but achievable given the solid foundation already in place.

**Next Step: Begin Week 1 implementation with Unit Cover AI behavior.**

# Squad Movement & Interaction System
## Design Document - ProjectHive 5.5

**Version:** 1.0  
**Date:** August 26, 2025  
**System Type:** Unified Squad Control with Smart Interactions

---

## 🎯 **Core Vision**

A unified squad control system where players command a 4-5 marine squad as a single cohesive unit. The system emphasizes intelligent automation, contextual interactions, and streamlined controls that make tactical squad management intuitive and responsive.

### **Design Goals**
- **Unified Control**: Squad moves and acts as one coordinated unit
- **Contextual Intelligence**: System understands player intent through cursor position
- **Smart Automation**: Units automatically handle formation maintenance and basic tactics
- **Streamlined Interaction**: Minimal input complexity for maximum tactical control

---

## 🎮 **Movement Control System**

### **Primary Movement Commands**

#### **Basic Squad Movement**
- **Input**: Right-click on empty ground
- **Behavior**: Entire squad moves to target location
- **Formation**: Maintains current formation during movement
- **Pathfinding**: Squad navigates as group, avoiding obstacles collectively
- **Speed**: All units move at speed of slowest member

#### **Formation Types**
| Formation | Description | Use Case |
|-----------|-------------|----------|
| **Tight** | Close together, minimal spacing | Narrow corridors, stealth movement |
| **Spread** | Wide spacing, tactical separation | Open combat, avoiding area attacks |
| **Wedge** | V-formation with point marine leading | Advancing through unknown territory |
| **Line** | Side-by-side formation | Coordinated fire lines |

#### **Formation Control**
- **Formation Cycle**: Press formation key to cycle through available formations
- **Auto-Formation**: System suggests optimal formation based on environment
- **Formation Lock**: Hold formation during movement vs adapt to terrain
- **Emergency Scatter**: Instant formation break for area attack avoidance

### **Movement Behaviors**

#### **Standard Movement**
- **Speed**: Normal walking/running pace
- **Formation**: Maintains current formation structure
- **Caution Level**: Standard awareness and reaction
- **Engagement**: Will engage enemies encountered during movement

#### **Tactical Movement Modifiers**
| Modifier | Input | Behavior |
|----------|-------|----------|
| **Cautious** | Shift + Right-click | Slow, cover-to-cover movement |
| **Sprint** | Ctrl + Right-click | Fast movement, reduced accuracy afterward |
| **Stealth** | Alt + Right-click | Quiet movement, avoids detection |
| **Aggressive** | Right-click + drag | Attack-move, engaging all enemies along path |

---

## 🖱️ **Contextual Cursor System**

### **Visual Cursor Feedback**
The cursor provides immediate visual feedback about available interactions:

#### **Cursor States**
| Target Type | Cursor Visual | Available Actions |
|-------------|---------------|-------------------|
| **Empty Ground** | Standard move cursor | Move, formation move, tactical advance |
| **Cover Object** | Cover icon overlay | Take cover, use cover, overwatch from cover |
| **Enemy Unit** | Attack reticle | Attack, suppress, flank |
| **Interactive Object** | Interaction symbol | Hack, plant explosive, examine |
| **Ally/Injured** | Medical cross | Heal, assist, protect |
| **Environmental** | Contextual icon | Destroy, climb, breach |

#### **Smart Cursor Detection**
- **Proximity Range**: Cursor changes within interaction range of objects
- **Priority System**: Most relevant interaction highlighted when multiple options available
- **Context Awareness**: Available actions based on squad capabilities and current state

### **Interaction Feedback**
- **Hover Highlighting**: Objects highlight when cursor hovers over them
- **Action Preview**: Brief text description of primary action appears
- **Range Indicators**: Visual feedback showing interaction range
- **Availability Status**: Grayed out cursors for unavailable actions

---

## 🛡️ **Cover System Design**

### **Automatic Cover Detection**
The system intelligently identifies and utilizes cover in the environment:

#### **Cover Types**
| Cover Type | Protection Level | Characteristics |
|------------|------------------|-----------------|
| **Full Cover** | 80% damage reduction | Walls, large objects, vehicles |
| **Partial Cover** | 50% damage reduction | Low walls, debris, small objects |
| **Concealment** | 0% damage reduction | Bushes, smoke, visual obstruction only |
| **Destructible Cover** | Variable protection | Can be destroyed by concentrated fire |

#### **Cover Positioning Rules**
- **Optimal Spacing**: Units automatically space themselves along cover
- **Fire Lanes**: System ensures units can fire from cover positions
- **Mutual Support**: Cover positions allow units to support each other
- **Escape Routes**: Cover selection considers retreat paths

### **Cover Command System**

#### **Manual Cover Commands**
- **Right-click Cover**: Squad moves to available cover positions around target object
- **Formation + Cover**: Combines current formation with available cover
- **Directional Cover**: Squad positions based on threat direction

#### **Automatic Cover Behavior**
- **Cover Key Command**: Press cover key (V) for immediate cover seeking
- **Search Range**: Units look for cover within configurable radius
- **Fallback Behavior**: If no cover available, units crouch in place
- **Priority System**: Better cover types preferred over lesser protection

### **Cover Seeking Algorithm**
```
Cover Command Execution:
1. Scan for available cover within range (X units from current position)
2. Evaluate cover quality (protection level, firing angles, escape routes)
3. Assign squad members to optimal cover positions
4. Execute coordinated movement to cover positions
5. If no cover found: Units crouch in current positions for minimal profile
```

---

## 🎯 **Interaction System**

### **Environmental Interactions**

#### **Interactive Object Types**
| Object Type | Interaction | Requirements | Result |
|-------------|-------------|--------------|---------|
| **Terminals** | Hack/Access | Tech specialist or ability | Information, door control, system access |
| **Doors** | Open/Breach | Standard or explosive | Access to new areas |
| **Vehicles** | Enter/Destroy | Proximity | Transport or area denial |
| **Weapons Cache** | Search/Loot | Proximity | Equipment upgrades |
| **Explosives** | Plant/Defuse | Demolitions ability | Area destruction or threat removal |
| **Medical Station** | Treat | Injured unit present | Health restoration |

#### **Interaction Range System**
- **Proximity Detection**: Objects become interactive when squad is within range
- **Visual Feedback**: Objects highlight when in interaction range
- **Auto-Approach**: Right-clicking distant interactive moves squad into range first
- **Multi-Unit Interactions**: Some objects require multiple units or specific specialists

### **Combat Interactions**

#### **Enemy Targeting**
- **Primary Target**: Right-click enemy for focused fire
- **Target Priority**: Squad automatically engages highest priority threats
- **Suppression Fire**: Special targeting mode for area denial
- **Coordinated Fire**: Multiple units focus on single target

#### **Tactical Actions**
- **Flank Orders**: Contextual flanking maneuvers based on enemy position
- **Suppression**: Pin enemies to reduce their effectiveness
- **Overwatch**: Defensive positioning with reaction fire
- **Breach and Clear**: Coordinated room entry tactics

---

## 🤖 **Squad Intelligence System**

### **Formation Intelligence**
Squad members automatically handle formation maintenance and adaptation:

#### **Adaptive Formation**
- **Terrain Response**: Formation adjusts to corridor width, obstacles
- **Threat Response**: Formation shifts based on enemy positions
- **Movement Efficiency**: Optimal spacing for current movement speed
- **Recovery**: Formation auto-corrects after combat or obstacles

#### **Individual Unit Behavior**
- **Position Maintenance**: Units work to maintain formation position
- **Obstacle Navigation**: Individual pathfinding within formation structure
- **Combat Spacing**: Units adjust spacing during firefights
- **Support Positioning**: Units position to support injured members

### **Contextual Decision Making**

#### **Movement Intelligence**
- **Path Optimization**: Squad chooses efficient routes automatically
- **Threat Avoidance**: Automatic routing around known dangers
- **Cover Awareness**: Movement considers available cover along route
- **Formation Preservation**: Maintains formation integrity during movement

#### **Combat Intelligence**
- **Target Selection**: Units engage appropriate targets automatically
- **Ammo Management**: Units reload during natural combat lulls
- **Mutual Support**: Units position to cover each other
- **Casualty Response**: Automatic protection of injured squad members

---

## 📋 **Control Scheme Integration**

### **Primary Controls**
| Input | Action | Context |
|-------|--------|---------|
| **Right Click** | Context Action | Movement, attack, interaction |
| **Formation Key** | Cycle Formation | Always available |
| **Cover Key (V)** | Seek Cover | Emergency defensive positioning |
| **Hold Position** | Stop All Movement | Immediate halt command |

### **Advanced Controls**
| Input | Action | Context |
|-------|--------|---------|
| **Shift + Right Click** | Cautious Movement | Tactical advance |
| **Ctrl + Right Click** | Sprint Movement | Fast repositioning |
| **Alt + Right Click** | Stealth Movement | Avoid detection |
| **Double Right Click** | Priority Command | Override current actions |

### **Context-Sensitive Commands**
Based on cursor target and squad state, right-click provides:
- **Movement commands** for empty terrain
- **Attack commands** for enemies
- **Interaction commands** for objects
- **Cover commands** for protective terrain
- **Support commands** for allies

---

## 🎨 **Visual Feedback System**

### **Movement Feedback**
- **Movement Preview**: Ghosted unit positions show intended movement
- **Path Indicators**: Lines or markers showing planned route
- **Formation Preview**: Visual representation of formation at destination
- **Obstacle Warnings**: Visual feedback for impassable terrain

### **Interaction Feedback**
- **Highlight System**: Interactive objects glow or outline when available
- **Range Indicators**: Circles or areas showing interaction zones
- **Action Icons**: Cursor overlays indicating available actions
- **Status Indicators**: Visual feedback for interaction success/failure

### **Cover Feedback**
- **Cover Visualization**: Highlighted cover positions during cover commands
- **Protection Levels**: Different visual styles for different cover types
- **Firing Angles**: Visual indicators showing effective fire zones from cover
- **Threat Direction**: Arrows or indicators showing optimal cover orientation

---

## 🔧 **System Configuration**

### **Behavior Settings**
- **Formation Strictness**: How rigidly units maintain formation
- **Cover Search Range**: Maximum distance units will travel for cover
- **Auto-Cover Threshold**: Damage level that triggers automatic cover seeking
- **Movement Speed**: Base movement speed for different movement types

### **Visual Settings**
- **Cursor Sensitivity**: How quickly cursor changes between states
- **Highlight Intensity**: Visibility level of interactive object highlighting
- **Feedback Duration**: How long visual indicators remain visible
- **Preview Opacity**: Transparency level of movement previews

### **Intelligence Settings**
- **Aggression Level**: How readily squad engages enemies
- **Formation Adaptability**: How much formations adapt to terrain
- **Cover Priority**: Preference for cover quality vs proximity
- **Support Behavior**: How units prioritize supporting each other

---

## 🎯 **Design Philosophy**

### **Core Principles**
1. **Player Intent Recognition**: System understands what player wants to do
2. **Intelligent Automation**: Handle routine tasks automatically
3. **Contextual Relevance**: Present options based on current situation
4. **Unified Control**: Squad acts as single entity with smart individual behavior
5. **Minimal Complexity**: Simple inputs achieve complex tactical maneuvers

### **User Experience Goals**
- **Intuitive Control**: Players naturally understand how to command squad
- **Tactical Depth**: Simple controls enable complex tactical possibilities
- **Responsive Feedback**: Immediate visual confirmation of all commands
- **Smart Defaults**: System makes good decisions when player doesn't specify

This movement and interaction system creates a seamless squad control experience where players can focus on tactical decisions rather than micromanagement, while still maintaining full control over their squad's behavior and positioning.

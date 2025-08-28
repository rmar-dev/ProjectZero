# ProjectZero - Game Design Document

## Version: 3.0 (Unity Port)
## Last Updated: August 28, 2025

---

## Table of Contents
1. [Game Overview](#game-overview)
2. [Core Mechanics](#core-mechanics)
3. [Combat System](#combat-system)
4. [Squad Management System](#squad-management-system)
5. [Map & Mission Structure](#map--mission-structure)
6. [Faction System](#faction-system)
7. [Vehicle & Progression](#vehicle--progression)
8. [Art Direction](#art-direction)
9. [Technical Architecture](#technical-architecture)
10. [Prototype & Testing Plan](#prototype--testing-plan)
11. [Future Considerations](#future-considerations)

---

## Game Overview

### Genre
Tactical Squad-Based Extraction Shooter / Top-down Action-Strategy

### Core Vision
ProjectZero is a tactical extraction shooter where players command squads of 3-6 characters in post-apocalyptic missions. Combining real-time movement with ATB-based combat, players must complete objectives and extract alive while managing faction relationships and permanent squad member loss.

### Key Pillars
- **Tactical Squad Command**: Context-sensitive and role-based unit control
- **Meaningful Stakes**: Permanent character loss creates genuine tension
- **Adaptive Strategy**: Procedural map elements prevent route memorization
- **Faction Consequences**: Political choices have lasting gameplay impact
- **Balanced Pacing**: Real-time action with slow-motion tactical pauses

### Setting & Atmosphere
- **Timeline**: Far future post-apocalypse (beyond The Last of Us timeframe)
- **Environment**: Multiple faction territories in a devastated world
- **Enemies**: Strange bug-type infected creatures with alien behaviors
- **Tone**: Tense survival with factional politics and territorial conflicts

---

## Core Mechanics

### Combat System
- **Real-time Movement**: Characters move freely in real-time
- **ATB (Active Time Battle) Bars**: Both player characters and enemies have action bars
- **Slow-Motion Tactical Mode**: Pause frenetic action to plan positioning and abilities
- **Top-down Perspective**: Diablo-style view for strategic squad oversight
- **Hybrid Pacing**: Balance between real-time action and tactical planning

### Extraction Mechanics
- **Mission Structure**: Get in, complete objectives, extract alive
- **Multiple Extraction Points**: Various escape routes per mission
- **Time Pressure**: Extended missions increase danger (swarms, rival factions)
- **Permanent Consequences**: Failed extractions result in permanent squad member loss
- **Risk/Reward Balance**: Longer missions offer better rewards but higher stakes

---

## Squad Management System

### Control Schemes (To Be Prototyped)
Four different approaches planned for testing:

1. **Pure Squad Commands**
    - Unified squad movement and attack orders
    - Focus on speed and simplicity
    - Commands: Move, Attack, Hold, Retreat

2. **Individual Unit Control**
    - Direct character selection and control
    - Maximum tactical precision
    - High skill ceiling, steep learning curve

3. **Context-Sensitive Hybrid**
    - Squad commands for movement/positioning
    - Individual control for special abilities
    - Toggle between modes with hotkey

4. **Role-Based Commands**
    - Commands directed at roles: "Sniper, overwatch that position"
    - Smart AI interpretation of tactical context
    - Balanced accessibility and depth

### Squad Composition
- **Variable Squad Size**: 3-6 characters depending on mission vehicle
- **Role Specialization**: Tank, DPS, Support, Specialist archetypes
- **Character Persistence**: Individual squad members have permanent identities
- **Permadeath Stakes**: Lost characters cannot be recovered

---

## Map & Mission Structure

### Procedural Generation (Zero Sievert Style)
- **Hybrid Approach**: Fixed landmarks with procedural variation
- **Fixed Elements**: Major faction strongholds, key extraction points (marked clearly)
- **Procedural Elements**: Smaller outposts, resource nodes, infected nests, terrain details
- **Strategic Consistency**: Players can plan around major features while adapting to changes

### Mission Types
- **Objective Variety**: Retrieval, elimination, reconnaissance, faction missions
- **Dynamic Threats**: Infected swarms build over time, faction patrols move
- **Environmental Factors**: Weather, radiation zones, faction territory shifts
- **Rogue-like Elements**: Each mission feels unique despite familiar landmarks

---

## Faction System

### Faction Relationships
- **Multiple Competing Factions**: Various groups controlling territory
- **Affinity System**: Player choices affect standing with each faction
- **Consequences**: Helping one faction may lock out areas controlled by enemies
- **Territory Control**: Faction boundaries shift based on conflicts and player actions

### Future Faction Features
- **Faction Battles**: Large-scale conflicts between territories
- **Reputation Rewards**: Better equipment and missions from allied factions
- **Political Complexity**: Multi-faction scenarios with shifting alliances

---

## Vehicle & Progression

### Transport System
- **Vehicle Determines Squad Size**: Natural progression milestones
- **Strategic Deployment**: Vehicle choice affects mission approach
    - Smaller vehicles: 3-4 squad members, faster, stealthier, lower resource cost
    - Larger vehicles: 5-6 squad members, more firepower, higher visibility, more resources required
- **Progression Rewards**: Unlock larger transports through successful missions
- **Risk Management**: Bigger squads mean more capability but higher stakes if lost

### Equipment Progression
- **Character Development**: Individual skill progression for surviving squad members
- **Gear Acquisition**: Mission rewards and faction trading
- **Customization**: Weapon modifications and tactical equipment

---

## Art Direction

### Visual Style: Stylized Low-Poly with Strong Color Coding
- **Character Design**: Clean, readable silhouettes with chunky proportions
- **Faction Color Schemes**: Strong, distinct colors for instant identification
- **Environmental Design**: Muted backgrounds that make characters pop
- **Bug Creature Design**: Geometric insectoid forms with alien color palettes

### Character Design Principles
- **Instant Recognition**: Characters readable at tactical camera distance
- **Personality Through Silhouette**: Distinctive shapes for different roles/classes
- **Weathered but Heroic**: Battle-worn equipment with confident postures
- **Functional Details**: Visual storytelling through gear and modifications

### Technical Art Requirements
- **Low Polygon Count**: 200-500 triangles per character for performance
- **Clear Visual Hierarchy**: Important elements stand out during fast action
- **Animation Friendly**: Simple geometry suitable for Mixamo integration
- **Scalable Detail**: Looks good from tactical distance to close-up

---

## Technical Architecture

### Prototype Development Plan
- **Engine**: Unity 2022.3 LTS
- **Character Creation**: Blender for low-poly modeling → Mixamo for animation → Unity import
- **Control Testing**: Multiple prototype iterations for squad management
- **ATB Implementation**: Custom timer system with Time.timeScale integration

### Core Systems
- **Squad Controller**: MonoBehaviour managing multiple character selection and commands
- **ATB Manager**: Unity component handling action timing for all units (player and AI)
- **Faction Manager**: ScriptableObject-based system tracking relationships and territory control
- **Mission Manager**: Procedural objective and extraction point generation using Unity's job system

### Performance Considerations
- **Low-Poly Assets**: Optimized for multiple characters on screen
- **Efficient AI**: Simple but effective behavior trees for squad members and enemies
- **Modular Systems**: Component-based architecture for easy iteration

---

## Prototype & Testing Plan

### Development Phases

**Phase 1: Basic Character and Movement**
- Create low-poly character in Blender (T-pose)
- Import to Mixamo for basic animations (idle, walk, run, attack)
- Implement in Unity with top-down camera using Cinemachine
- Test basic movement and selection with Unity Input System

**Phase 2: Control Scheme Testing**
- Implement all four control approaches
- Create simple test scenarios
- Gather feedback on feel and usability
- Identify best approach for full development

**Phase 3: ATB and Combat Integration**
- Add ATB bar system
- Implement slow-motion tactical mode
- Test combat flow with chosen control scheme
- Balance timing and responsiveness

**Phase 4: Mission Structure**
- Create basic procedural mission generation
- Add extraction mechanics
- Test tension and pacing
- Refine risk/reward balance

### Success Metrics
- **Control Responsiveness**: Players can execute intended tactics
- **Visual Clarity**: Characters and threats clearly identifiable
- **Tension Balance**: Meaningful stakes without frustration
- **Replayability**: Procedural elements create variety

---

## Future Considerations

### Expansion Features
- **Base Building**: Home base customization and defense
- **Character Specialization**: Deeper skill trees and role differentiation
- **Vehicle Customization**: Armor, stealth, and capacity modifications
- **Narrative Integration**: Story missions integrated with extraction format

### Technical Scaling
- **Multiplayer Considerations**: Potential for cooperative extraction missions
- **Mod Support**: Community content creation tools
- **Platform Adaptation**: Console and mobile optimization
- **Analytics Integration**: Player behavior tracking for balancing

### Content Expansion
- **Additional Factions**: New groups with unique mechanics and aesthetics
- **Environmental Hazards**: Weather systems, radiation zones, temporal anomalies
- **Creature Variety**: Different bug-infected types with unique tactical challenges
- **Mission Complexity**: Multi-stage objectives and dynamic events

---

## Decision Log

### Major Design Decisions
1. **Control System**: Multiple prototypes planned to determine optimal approach
2. **Art Style**: Stylized low-poly with strong faction color coding
3. **Map Generation**: Hybrid procedural system balancing familiarity and variety
4. **Stakes System**: Permanent character loss for meaningful tension
5. **Combat Pacing**: ATB system with slow-motion planning phases

### Decisions Pending
- **Final Control Scheme**: Awaiting prototype testing results
- **Faction Complexity**: Number and depth of faction interactions
- **Progression Pacing**: Character and vehicle unlock rates
- **Mission Variety**: Types and complexity of objectives
- **Narrative Integration**: Story delivery method and depth

### Testing Priorities
1. **Control Feel**: Which squad management approach feels most intuitive
2. **Visual Clarity**: Character readability during intense combat
3. **Tension Balance**: Stakes vs. frustration in permadeath system
4. **Procedural Balance**: Familiarity vs. variety in map generation

---

## Notes
- This document reflects the Unity port vision as of August 28, 2025
- Originally designed for Unreal Engine 5.5, now being developed in Unity 2022.3 LTS
- Control scheme selection is critical and will drive many other design decisions
- Art style testing with actual low-poly characters is essential for validation
- Faction system complexity should scale with development resources
- Regular playtesting will be crucial for balancing extraction tension
- Unity's cross-platform capabilities will enable broader distribution

---

*This document should be updated as prototype testing reveals optimal approaches.*
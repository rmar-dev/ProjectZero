# Simple Grid System

## Quick Setup

1. **Create a Ground Plane:**
   - Create a Plane GameObject in your scene (Right-click > 3D Object > Plane)
   - Position it at (0, 0, 0)
   - Scale it to (10, 1, 10) or larger
   - Set its layer to "Ground" (create this layer if needed)

2. **Create Grid Manager:**
   - Create an empty GameObject and name it "GridManager"
   - Add the `GridManager` component from `ProjectZero.SimpleGrid`
   - Configure the settings:
     - Grid Width: 10
     - Grid Height: 10
     - Cell Size: 1.0
     - Grid Origin: (0, 0, 0)
     - Ground Layer Mask: Set to include your ground plane layer

3. **Set Camera:**
   - Assign your scene camera to the "Player Camera" field in GridManager
   - Position camera to look down at the grid (e.g., position: (5, 10, 5), rotation: (45, 0, 0))

## Controls

- **Mouse Hover:** Select cells automatically on hover
- **Left Shift + Mouse Hover:** Multi-select (add to existing selection)
- **Left Mouse Button + Drag:** Select rectangular areas
- **Left Shift + Mouse Drag:** Multi-select areas (add to existing selection)

## Features

✅ Visual grid lines with customizable colors and width  
✅ Cell selection with visual feedback  
✅ Area selection by click and drag  
✅ Multi-selection support  
✅ Cell highlighting on hover  
✅ Configurable grid size and cell dimensions  
✅ Event system for selection changes  

## Customization

All visual settings can be adjusted in the GridVisualizer component:
- Grid line color and width
- Highlight and selection colors
- Cell visual height
- Custom materials for different states

The GridManager provides events you can subscribe to:
- `OnSelectionChanged`: Called when cell selection changes
- `OnCellHovered`: Called when mouse hovers over cells

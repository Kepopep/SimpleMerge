# Custom Drag-and-Drop System for Merge Game

## Overview

This system implements a custom drag-and-drop solution for the Merge Game that works with the existing 3x3 grid architecture. The system allows players to drag MergeItem objects between GridCell positions using mouse input.

## Architecture

### Components

1. **DragController** - Core drag-and-drop manager
2. **DragControllerExample** - Integration example with game logic
3. **DragSetup** - Scene setup helper

### Key Features

- Mouse-driven drag-and-drop functionality
- Compatibility with existing GridManager, GridCell, and MergeItem architecture
- Proper separation of concerns
- Event-based communication system

## DragController Class

The `DragController` class handles all aspects of the drag operation:

### Properties
- `IsDragging` - Whether an item is currently being dragged
- `DraggedItem` - The currently dragged MergeItem
- `OriginalCell` - The cell where the item originated

### Events
- `OnDropOnEmptyCell(MergeItem, GridCell)` - Triggered when item is dropped on an empty cell
- `OnDropOnOccupiedCell(MergeItem, GridCell)` - Triggered when item is dropped on an occupied cell
- `OnMergeAttempt(MergeItem, MergeItem, GridCell)` - Triggered when two items with the same value are dropped on the same cell

### Configuration
- `_itemLayerMask` - Layer mask for detecting MergeItem objects
- `_cellLayerMask` - Layer mask for detecting GridCell objects
- `_camera` - Camera used for raycasting (defaults to Main Camera)
- `_dragHeightOffset` - Vertical offset for dragged items

## How It Works

### Drag Start Phase
1. Player clicks on a MergeItem (detected via raycast)
2. System stores reference to the item and its original cell
3. Item is temporarily removed from the grid
4. Drag offset is calculated to maintain relative position

### Drag Move Phase
1. Item follows mouse cursor in world space
2. Maintains consistent height above the grid
3. No grid logic is executed during this phase

### Drag End Phase
1. System detects target GridCell under cursor
2. If cell is empty: Item is placed in the cell
3. If cell is occupied:
   - If items have the same value: Merge is triggered and OnMergeAttempt event fires
   - If items have different values: OnDropOnOccupiedCell event fires and item returns to original position
4. If no valid drop position: Item returns to original cell

## Integration Guide

### Basic Setup
```csharp
public class GameLogic : MonoBehaviour
{
    [SerializeField] private DragController dragController;
    
    private void Start()
    {
        dragController.OnDropOnEmptyCell.AddListener(HandleDropOnEmptyCell);
        dragController.OnDropOnOccupiedCell.AddListener(HandleDropOnOccupiedCell);
        dragController.OnMergeAttempt.AddListener(HandleMergeAttempt);
    }
    
    private void HandleDropOnEmptyCell(MergeItem item, GridCell cell)
    {
        // Handle placement in empty cell
        // Example: Check for adjacent merges
    }
    
    private void HandleDropOnOccupiedCell(MergeItem item, GridCell cell)
    {
        // Handle placement on occupied cell
        // Example: Attempt merge if values match
    }
    
    private void HandleMergeAttempt(MergeItem item1, MergeItem item2, GridCell targetCell)
    {
        // Handle merge between two items
        // The merge has already been processed by the DragController
        // item2 now has the upgraded value and item1 has been destroyed
        Debug.Log($"Items merged successfully! Resulting value: {item2.Value}");
    }
}
```

### Layer Setup
Ensure your MergeItem and GridCell objects are on appropriate layers:
- MergeItem objects should be on the layer specified in `_itemLayerMask`
- GridCell objects should be on the layer specified in `_cellLayerMask`

## Technical Constraints Compliance

✅ Does NOT use UnityEngine.InputSystem  
✅ Does NOT use Unity UI drag-and-drop interfaces (IDragHandler, IDropHandler, etc.)  
✅ Uses classic Unity input (Input.GetMouseButtonDown, Input.GetMouseButton, Input.GetMouseButtonUp)  
✅ Implements raycasts for object detection  
✅ Separates drag logic from merge logic  
✅ No input handling inside MergeItem, GridCell, or GridManager  

## Extensibility

The system is designed to be easily extendable for touch input (though not implemented):
- Input handling is centralized in DragController
- Raycasting approach works for both mouse and touch
- Event system allows for flexible response handling

## Error Handling

- Null checks prevent invalid transitions
- Proper cleanup when objects are destroyed
- Validation of grid coordinates
- Fallback behavior when target positions are invalid

## Performance Considerations

- Camera reference is cached to avoid repeated lookups
- Efficient raycasting with appropriate layer masks
- Minimal calculations during drag movement
- Proper cleanup of event listeners

## Usage Examples

See `DragControllerExample.cs` for implementation examples showing:
- Event subscription and handling
- Merge logic integration
- Adjacent item checking
- Value comparison for merging

See `DragSetup.cs` for scene integration examples showing:
- Runtime configuration of the drag controller
- Automatic event handler setup
- Item spawning utilities
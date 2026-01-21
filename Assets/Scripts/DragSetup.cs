using UnityEngine;
using SimpleMerge.Grid;
using SimpleMerge.Scripts;
using SimpleMerge.DragDrop;

namespace SimpleMerge.Setup
{
    /// <summary>
    /// Setup script to demonstrate proper integration of DragController with the existing architecture.
    /// This script handles the initialization and connection of all components.
    /// </summary>
    public class DragSetup : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private GridManager _gridManager;
        
        [Header("Drag Controller Configuration")]
        [SerializeField] private DragController _dragController;
        
        [Header("Layer Masks")]
        [SerializeField] private LayerMask _mergeItemLayer = 1 << 8; // UI layer as example
        [SerializeField] private LayerMask _gridCellLayer = 1 << 9;  // UI layer as example
        
        [Header("Drag Settings")]
        [SerializeField] private float _dragHeight = 1.0f;
        
        private void Start()
        {
            InitializeDragController();
            SetupEventHandlers();
        }
        
        /// <summary>
        /// Initializes the DragController with proper settings
        /// </summary>
        private void InitializeDragController()
        {
            if (_dragController != null)
            {
                // Configure layer masks
                _dragController.GetType().GetField("_itemLayerMask", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(_dragController, _mergeItemLayer);
                    
                _dragController.GetType().GetField("_cellLayerMask", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(_dragController, _gridCellLayer);
                
                // Configure drag height
                _dragController.GetType().GetField("_dragHeightOffset", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(_dragController, _dragHeight);
            }
            else
            {
                Debug.LogError("DragController reference is not set in the inspector!");
            }
        }
        
        /// <summary>
        /// Sets up event handlers for drag controller events
        /// </summary>
        private void SetupEventHandlers()
        {
            if (_dragController != null)
            {
                // Add default handlers if needed
                _dragController.OnDropOnEmptyCell.AddListener(OnDropOnEmptyCell);
                _dragController.OnDropOnOccupiedCell.AddListener(OnDropOnOccupiedCell);
            }
        }
        
        /// <summary>
        /// Default handler for drops on empty cells
        /// </summary>
        private void OnDropOnEmptyCell(MergeItem item, GridCell cell)
        {
            Debug.Log($"Default: Item {item.Value} placed in empty cell at ({cell.RowIndex}, {cell.ColumnIndex})");
        }
        
        /// <summary>
        /// Default handler for drops on occupied cells
        /// </summary>
        private void OnDropOnOccupiedCell(MergeItem item, GridCell cell)
        {
            Debug.Log($"Default: Item {item.Value} dropped on occupied cell at ({cell.RowIndex}, {cell.ColumnIndex})");
        }
        
        /// <summary>
        /// Creates a MergeItem and places it in a random free cell
        /// </summary>
        public void SpawnAndPlaceItem(GameObject mergeItemPrefab, int value = 1)
        {
            if (_gridManager != null && mergeItemPrefab != null)
            {
                GridCell freeCell = _gridManager.GetRandomFreeCell();
                
                if (freeCell != null)
                {
                    GameObject newItemObj = Instantiate(mergeItemPrefab, freeCell.WorldPosition, Quaternion.identity);
                    MergeItem newItem = newItemObj.GetComponent<MergeItem>();
                    
                    if (newItem != null)
                    {
                        // Set the value if needed
                        var fieldInfo = typeof(MergeItem).GetField("_value", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (fieldInfo != null)
                        {
                            fieldInfo.SetValue(newItem, value);
                        }
                        
                        // Assign to the selected cell
                        newItem.AssignToCell(freeCell);
                    }
                }
                else
                {
                    Debug.LogWarning("No free cells available to place the item!");
                }
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event listeners
            if (_dragController != null)
            {
                _dragController.OnDropOnEmptyCell.RemoveListener(OnDropOnEmptyCell);
                _dragController.OnDropOnOccupiedCell.RemoveListener(OnDropOnOccupiedCell);
            }
        }
    }
}
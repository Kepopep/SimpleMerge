using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SimpleMerge.Grid;
using SimpleMerge.Scripts;

namespace SimpleMerge.Spawning
{
    /// <summary>
    /// Advanced spawner for MergeItem objects that creates items with values based on the maximum value in the grid
    /// </summary>
    public class MergeItemSpawner : MonoBehaviour
    {
        [Header("Grid Setup")]
        [SerializeField] private GridManager _gridManager;
        
        [Header("Merge Item Prefab")]
        [SerializeField] private GameObject _mergeItemPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private int _spawnAttemptsBeforeFallback = 3;
        [SerializeField] private float _minSpawnDelay = 0.1f;
        [SerializeField] private float _maxSpawnDelay = 0.5f;
        
        private System.Random _random;
        
        private void Start()
        {
            _random = new System.Random();
            
            // Find grid manager if not assigned in inspector
            if (_gridManager == null)
            {
                _gridManager = FindObjectOfType<GridManager>();
                if (_gridManager == null)
                {
                    Debug.LogError("GridManager not found in the scene! Please assign it in the inspector.", this);
                }
            }
            
            // Validate prefab
            if (_mergeItemPrefab == null)
            {
                Debug.LogError("MergeItem prefab not assigned! Please assign it in the inspector.", this);
            }
        }
        
        /// <summary>
        /// Spawns a new MergeItem with a random value in a random free cell
        /// The value is randomly selected between 1 and the maximum value in the grid + 1
        /// </summary>
        public bool SpawnItemInRandomFreeCell()
        {
            if (_gridManager == null)
            {
                Debug.LogError("GridManager is not assigned!", this);
                return false;
            }
            
            if (_mergeItemPrefab == null)
            {
                Debug.LogError("MergeItem prefab is not assigned!", this);
                return false;
            }
            
            int maxValueForSpawn = Mathf.Max(1, GetMaxValueInGrid() + 1);
            
            // Generate a random value in the appropriate range
            int spawnValue = _random.Next(1, maxValueForSpawn + 1);
            
            return SpawnItemWithValueInRandomFreeCell(spawnValue);
        }
        
        /// <summary>
        /// Spawns a new MergeItem with a specific value in a random free cell
        /// </summary>
        public bool SpawnItemWithValueInRandomFreeCell(int value)
        {
            if (_gridManager == null)
            {
                Debug.LogError("GridManager is not assigned!", this);
                return false;
            }
            
            if (_mergeItemPrefab == null)
            {
                Debug.LogError("MergeItem prefab is not assigned!", this);
                return false;
            }
            
            // Get a random free cell
            GridCell freeCell = _gridManager.GetRandomFreeCell();
            
            if (freeCell == null)
            {
                Debug.LogWarning("No free cells available to spawn item with value " + value, this);
                return false;
            }
            
            // Spawn the item in the free cell
            return SpawnItemWithValueInSpecificCell(value, freeCell);
        }
        
        /// <summary>
        /// Spawns a new MergeItem with a specific value in a specific cell
        /// </summary>
        public bool SpawnItemWithValueInSpecificCell(int value, GridCell targetCell)
        {
            if (targetCell == null)
            {
                Debug.LogError("Target cell is null!", this);
                return false;
            }
            
            if (_mergeItemPrefab == null)
            {
                Debug.LogError("MergeItem prefab is not assigned!", this);
                return false;
            }
            
            // Check if the target cell is free
            if (targetCell.IsOccupied)
            {
                Debug.LogWarning($"Target cell ({targetCell.RowIndex}, {targetCell.ColumnIndex}) is already occupied!", this);
                return false;
            }
            
            // Create the new item
            GameObject newItemGO = Instantiate(_mergeItemPrefab, targetCell.WorldPosition, Quaternion.identity);
            MergeItem newItem = newItemGO.GetComponent<MergeItem>();
            
            if (newItem == null)
            {
                newItem = newItemGO.AddComponent<MergeItem>();
            }
            
            // Set the value using reflection since the field is private
            var valueField = typeof(MergeItem).GetField("_value", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (valueField != null)
            {
                valueField.SetValue(newItem, value);
            }
            
            // Assign to the target cell
            bool assigned = newItem.AssignToCell(targetCell);
            
            if (assigned)
            {
                Debug.Log($"Spawned merge item with value {value} at cell ({targetCell.RowIndex}, {targetCell.ColumnIndex})", this);
                return true;
            }
            else
            {
                Debug.LogError($"Failed to assign merge item with value {value} to cell ({targetCell.RowIndex}, {targetCell.ColumnIndex})", this);
                Destroy(newItem.gameObject);
                return false;
            }
        }
        
        /// <summary>
        /// Gets the maximum value among all items currently in the grid
        /// Returns 0 if the grid is empty
        /// </summary>
        public int GetMaxValueInGrid()
        {
            if (_gridManager == null)
            {
                Debug.LogWarning("GridManager is not assigned, returning 0 as max value", this);
                return 0;
            }
            
            List<GridCell> occupiedCells = _gridManager.GetOccupiedCells();
            
            if (occupiedCells.Count == 0)
            {
                return 0;
            }
            
            int maxValue = 0;
            
            foreach (GridCell cell in occupiedCells)
            {
                if (cell.OccupyingItem != null)
                {
                    MergeItem item = cell.OccupyingItem.GetComponent<MergeItem>();
                    if (item != null)
                    {
                        maxValue = Mathf.Max(maxValue, item.Value);
                    }
                }
            }
            
            return maxValue;
        }
        
        /// <summary>
        /// Gets all values present in the grid
        /// </summary>
        public List<int> GetAllValuesInGrid()
        {
            List<int> values = new List<int>();
            
            if (_gridManager == null)
            {
                Debug.LogWarning("GridManager is not assigned", this);
                return values;
            }
            
            List<GridCell> occupiedCells = _gridManager.GetOccupiedCells();
            
            foreach (GridCell cell in occupiedCells)
            {
                if (cell.OccupyingItem != null)
                {
                    MergeItem item = cell.OccupyingItem.GetComponent<MergeItem>();
                    if (item != null)
                    {
                        values.Add(item.Value);
                    }
                }
            }
            
            return values;
        }
        
        /// <summary>
        /// Spawns multiple items with random values in random free cells
        /// </summary>
        public int SpawnMultipleItems(int count)
        {
            int spawnedCount = 0;
            
            for (int i = 0; i < count; i++)
            {
                if (SpawnItemInRandomFreeCell())
                {
                    spawnedCount++;
                    
                    // Small delay between spawns to simulate real-time spawning
                    float delay = Random.Range(_minSpawnDelay, _maxSpawnDelay);
                    System.Threading.Thread.Sleep((int)(delay * 1000));
                }
                else
                {
                    Debug.LogWarning($"Failed to spawn item #{i + 1}, stopping spawn process.", this);
                    break; // Stop if we can't spawn more items
                }
            }
            
            return spawnedCount;
        }
        
        /// <summary>
        /// Gets the number of free cells in the grid
        /// </summary>
        public int GetFreeCellCount()
        {
            return _gridManager != null ? _gridManager.FreeCellCount : 0;
        }
        
        /// <summary>
        /// Gets the total number of cells in the grid
        /// </summary>
        public int GetTotalCellCount()
        {
            return _gridManager != null ? _gridManager.TotalCellCount : 0;
        }
        
        /// <summary>
        /// Checks if the grid has any free cells available
        /// </summary>
        public bool HasFreeCells()
        {
            return _gridManager != null ? _gridManager.FreeCellCount > 0 : false;
        }
    }
}
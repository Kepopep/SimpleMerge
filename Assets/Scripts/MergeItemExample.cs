using UnityEngine;
using SimpleMerge.Grid;
using SimpleMerge.Scripts;

namespace SimpleMerge.Scripts
{
    /// <summary>
    /// Пример, демонстрирующий использование класса MergeItem с существующей архитектурой сетки
    /// </summary>
    public class MergeItemExample : MonoBehaviour
    {
        [Header("Настройка сетки")]
        [SerializeField] private GridManager gridManager;
        
        [Header("Префаб элемента слияния")]
        [SerializeField] private GameObject mergeItemPrefab;
        
        private void Start()
        {
            DemonstrateMergeItemUsage();
        }
        
        private void DemonstrateMergeItemUsage()
        {
            if (gridManager == null)
            {
                Debug.LogError("GridManager reference is not set in MergeItemExample");
                return;
            }
            
            if (mergeItemPrefab != null)
            {
                GameObject newItemGO = Instantiate(mergeItemPrefab);
                MergeItem newItem = newItemGO.GetComponent<MergeItem>();
                
                if (newItem != null)
                {
#if UNITY_EDITOR
                    Debug.Log($"Created merge item with initial value: {newItem.Value}");
#endif
                    
                    GridCell targetCell = gridManager.GetCellAt(1, 1); // Центральная ячейка
                    if (targetCell != null)
                    {
                        bool assigned = newItem.AssignToCell(targetCell);
                        if (assigned)
                        {
#if UNITY_EDITOR
                            Debug.Log($"Successfully assigned merge item to cell (1,1). Value: {newItem.Value}");
#endif
                            
                            newItem.Upgrade();
#if UNITY_EDITOR
                            Debug.Log($"Upgraded merge item. New value: {newItem.Value}");
#endif
                            
                            newItem.UpdatePositionToCell();
                            
                            newItem.GetCurrentCellCoordinates(out int row, out int col);
#if UNITY_EDITOR
                            Debug.Log($"Current cell coordinates: ({row}, {col})");
#endif
                        }
                        else
                        {
                            Debug.LogWarning("Failed to assign merge item to cell (1,1)");
                        }
                    }
                    
                    GridCell newTargetCell = gridManager.GetCellAt(0, 0);
                    if (newTargetCell != null && newTargetCell != targetCell)
                    {
                        bool moved = newItem.AssignToCell(newTargetCell);
                        if (moved)
                        {
#if UNITY_EDITOR
                            Debug.Log($"Successfully moved merge item to cell (0,0). Value: {newItem.Value}");
#endif
                        }
                    }
                    
                    if (newItem.IsAssigned)
                    {
                        bool removed = newItem.RemoveFromCurrentCell();
                        if (removed)
                        {
#if UNITY_EDITOR
                            Debug.Log("Successfully removed merge item from its cell");
#endif
                        }
                    }
                }
                else
                {
                    Debug.LogError("MergeItem prefab does not have MergeItem component attached");
                }
            }
            
            if (mergeItemPrefab != null && gridManager.FreeCellCount > 0)
            {
                GameObject randomItemGO = Instantiate(mergeItemPrefab);
                MergeItem randomItem = randomItemGO.GetComponent<MergeItem>();
                
                if (randomItem != null)
                {
                    GridCell randomCell = gridManager.GetRandomFreeCell();
                    if (randomCell != null)
                    {
                        bool assigned = randomItem.AssignToCell(randomCell);
                        if (assigned)
                        {
#if UNITY_EDITOR
                            Debug.Log($"Assigned random merge item to cell ({randomCell.RowIndex}, {randomCell.ColumnIndex})");
#endif
                        }
                    }
                }
            }
        }
    }
}
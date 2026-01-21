using UnityEngine;
using SimpleMerge.Grid;

public class GridInitializationExample : MonoBehaviour
{
    [Header("Grid Setup")]
    [SerializeField] private GridManager gridManager;
    
    [Header("Sample Prefabs")]
    [SerializeField] private GameObject sampleItemPrefab;
    
    private void Start()
    {
        DemonstrateGridUsage();
    }
    
    private void DemonstrateGridUsage()
    {
        if (gridManager == null) return;
        
#if UNITY_EDITOR
        Debug.Log($"Grid initialized: {gridManager.GridWidth}x{gridManager.GridHeight}");
        Debug.Log($"Total cells: {gridManager.TotalCellCount}");
        Debug.Log($"Free cells: {gridManager.FreeCellCount}");
        Debug.Log($"Occupied cells: {gridManager.OccupiedCellCount}");
#endif
        
        GridCell cell = gridManager.GetCellAt(1, 1); // Center cell
        if (cell != null)
        {
#if UNITY_EDITOR
            Debug.Log($"Center cell at (1,1) is {(cell.IsOccupied ? "occupied" : "free")}");
#endif
        }
        
        var freeCells = gridManager.GetFreeCells();
#if UNITY_EDITOR
        Debug.Log($"Found {freeCells.Count} free cells");
#endif
        
        if (sampleItemPrefab != null)
        {
            GameObject newItem = Instantiate(sampleItemPrefab);
            bool assigned = gridManager.AssignItemToCell(0, 0, newItem.transform);
            if (assigned)
            {
#if UNITY_EDITOR
                Debug.Log("Successfully assigned item to cell (0,0)");
#endif
            }
        }
        
        if (sampleItemPrefab != null && gridManager.FreeCellCount > 1)
        {
            GameObject newItem2 = Instantiate(sampleItemPrefab);
            bool assigned = gridManager.AssignItemToRandomFreeCell(newItem2.transform);
            if (assigned)
            {
#if UNITY_EDITOR
                Debug.Log("Successfully assigned item to a random free cell");
#endif
            }
        }
        
        GridCell[] neighbors = gridManager.GetNeighbors(1, 1);
#if UNITY_EDITOR
        Debug.Log($"Center cell (1,1) has {neighbors.Length} neighbors");
#endif
        
        Transform clearedItem = gridManager.ClearCell(0, 0);
        if (clearedItem != null)
        {
#if UNITY_EDITOR
            Debug.Log("Successfully cleared cell (0,0)");
#endif
            Destroy(clearedItem.gameObject);
        }
    }
}
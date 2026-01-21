using UnityEngine;
using System.Collections.Generic;
using SimpleMerge.Grid;
using SimpleMerge.Scripts;

namespace SimpleMerge.Test
{
    /// <summary>
    /// Тестовый скрипт для демонстрации спауна нескольких элементов слияния на сетке
    /// </summary>
    public class TestElementSpawner : MonoBehaviour
    {
        [Header("Grid Setup")]
        [SerializeField] private GridManager gridManager;
        
        [Header("Merge Item Prefab")]
        [SerializeField] private GameObject mergeItemPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private int numberOfItemsToSpawn = 5;
        [SerializeField] private float spawnInterval = 1.0f;
        
        private List<MergeItem> spawnedItems = new List<MergeItem>();
        private int itemsSpawned = 0;
        
        private void Start()
        {
            // Найти менеджер сетки, если он не назначен
            if (gridManager == null)
            {
                gridManager = FindObjectOfType<GridManager>();
            }
            
            // Начать спаун элементов
            StartCoroutine(SpawnItemsOverTime());
        }
        
        private System.Collections.IEnumerator SpawnItemsOverTime()
        {
            while (itemsSpawned < numberOfItemsToSpawn && gridManager.FreeCellCount > 0)
            {
                SpawnSingleItem();
                
                itemsSpawned++;
                yield return new WaitForSeconds(spawnInterval);
            }
            
            if (itemsSpawned >= numberOfItemsToSpawn)
            {
#if UNITY_EDITOR
                Debug.Log($"Completed spawning {numberOfItemsToSpawn} test elements");
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"Stopped spawning after {itemsSpawned} items - no more free cells available");
#endif
            }
        }
        
        private void SpawnSingleItem()
        {
            if (gridManager == null)
            {
                Debug.LogError("GridManager not found - cannot spawn items");
                return;
            }
            
            if (mergeItemPrefab == null)
            {
                Debug.LogError("MergeItem prefab not assigned - cannot spawn items");
                return;
            }
            
            // Получить случайную свободную ячейку
            GridCell freeCell = gridManager.GetRandomFreeCell();
            if (freeCell == null)
            {
                Debug.LogWarning("No free cells available to spawn item");
                return;
            }
            
            // Создать новый элемент слияния
            GameObject newItemGO = Instantiate(mergeItemPrefab, transform);
            MergeItem newItem = newItemGO.GetComponent<MergeItem>();
            
            if (newItem == null)
            {
                // Если префаб не имеет компонента, добавить его
                newItem = newItemGO.AddComponent<MergeItem>();
            }
            
            // Установить случайное значение для разнообразия
            // Поскольку мы не можем напрямую установить значение (оно приватное), будем использовать значение по умолчанию
            // Или мы могли бы использовать рефлексию для установки значения, но в демонстрационных целях просто выведем это в лог
            
            // Назначить ячейке сетки
            bool assigned = newItem.AssignToCell(freeCell);
            
            if (assigned)
            {
                spawnedItems.Add(newItem);
#if UNITY_EDITOR
                Debug.Log($"Spawned merge item with value {newItem.Value} at cell ({freeCell.RowIndex}, {freeCell.ColumnIndex})");
                
                // Вывести в лог текущий статус сетки
                Debug.Log($"Grid status: {gridManager.OccupiedCellCount}/{gridManager.TotalCellCount} cells occupied");
#endif
            }
            else
            {
                Debug.LogError($"Failed to assign merge item to cell ({freeCell.RowIndex}, {freeCell.ColumnIndex})");
                Destroy(newItem.gameObject);
            }
        }
        
        /// <summary>
        /// Ручной спаун одного элемента немедленно
        /// </summary>
        public void SpawnItemNow()
        {
            SpawnSingleItem();
        }
        
        /// <summary>
        /// Очистить все созданные элементы из сетки
        /// </summary>
        public void ClearAllItems()
        {
            foreach (var item in spawnedItems)
            {
                if (item != null && item.IsAssigned)
                {
                    item.RemoveFromCurrentCell();
                    Destroy(item.gameObject);
                }
            }
            
            spawnedItems.Clear();
            itemsSpawned = 0;
            
#if UNITY_EDITOR
            Debug.Log("Cleared all test elements from the grid");
#endif
        }
        
        /// <summary>
        /// Сбросить спаунер и очистить все элементы
        /// </summary>
        public void ResetSpawner()
        {
            StopAllCoroutines();
            ClearAllItems();
            itemsSpawned = 0;
        }
        
        /// <summary>
        /// Переключить паузу/возобновление автоматического спауна
        /// </summary>
        public void TogglePause()
        {
            if (GetComponent<UnityEngine.Coroutine>() != null)
            {
                StopAllCoroutines();
            }
            else
            {
                StartCoroutine(SpawnItemsOverTime());
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            numberOfItemsToSpawn = Mathf.Max(1, numberOfItemsToSpawn);
            spawnInterval = Mathf.Max(0.1f, spawnInterval);
        }
#endif
    }
}
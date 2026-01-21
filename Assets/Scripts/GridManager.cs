using System.Collections.Generic;
using UnityEngine;

namespace SimpleMerge.Grid
{
    /// <summary>
    /// Управляет системой игровой сетки 3x3.
    /// Обрабатывает инициализацию сетки, хранение ячеек и предоставляет API для взаимодействия с сеткой.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Конфигурация сетки")]
        [Tooltip("Ширина сетки (количество столбцов)")]
        [SerializeField] private int _gridWidth = 3;
        
         [Tooltip("Высота сетки (количество строк)")]
         [SerializeField] private int _gridHeight = 3;
         
         [Tooltip("Расстояние между ячейками сетки")]
         [SerializeField] private float _cellSpacing = 1.5f;
         
         [Header("Префабы")]
         [Tooltip("Префаб для объектов ячеек сетки")]
         [SerializeField] private GameObject _cellPrefab;
        
        // Двумерный массив для хранения ячеек сетки
        private GridCell[,] _gridCells;
        
        // Кэшированный список свободных ячеек для эффективного поиска
        private List<GridCell> _freeCellsCache;
        
        // Кэшированный список занятых ячеек для эффективного поиска
        private List<GridCell> _occupiedCellsCache;
        
        /// <summary>
        /// Получает ширину сетки (количество столбцов)
        /// </summary>
        public int GridWidth => _gridWidth;
        
        /// <summary>
        /// Получает высоту сетки (количество строк)
        /// </summary>
        public int GridHeight => _gridHeight;
        
        /// <summary>
        /// Получает общее количество ячеек в сетке
        /// </summary>
        public int TotalCellCount => _gridWidth * _gridHeight;
        
        /// <summary>
        /// Получает расстояние между ячейками сетки
        /// </summary>
        public float CellSpacing => _cellSpacing;
        
        /// <summary>
        /// Получает общее количество занятых ячеек в сетке
        /// </summary>
        public int OccupiedCellCount => GetOccupiedCells().Count;
        
        /// <summary>
        /// Получает общее количество свободных (пустых) ячеек в сетке
        /// </summary>
        public int FreeCellCount => GetFreeCells().Count;
        
        /// <summary>
        /// Получает, полностью ли заполнена сетка (нет свободных ячеек)
        /// </summary>
        public bool IsFull => FreeCellCount == 0;
        
        /// <summary>
        /// Получает, полностью ли пуста сетка (нет занятых ячеек)
        /// </summary>
        public bool IsEmpty => OccupiedCellCount == 0;
        
        /// <summary>
        /// Инициализирует сетку при запуске объекта
        /// </summary>
        private void Awake()
        {
            InitializeGrid();
        }
        
        /// <summary>
        /// Инициализирует сетку с указанными размерами
        /// Создает и позиционирует все ячейки сетки
        /// </summary>
        public void InitializeGrid()
        {
            if (_gridWidth <= 0 || _gridHeight <= 0)
            {
#if UNITY_EDITOR
                Debug.LogError($"Invalid grid dimensions: Width={_gridWidth}, Height={_gridHeight}");
#endif
                return;
            }
            
            if (_gridCells != null)
            {
                for (int row = 0; row < _gridHeight; row++)
                {
                    for (int col = 0; col < _gridWidth; col++)
                    {
                        if (_gridCells[row, col] != null)
                        {
                            DestroyImmediate(_gridCells[row, col].gameObject);
                        }
                    }
                }
            }
            
            _gridCells = new GridCell[_gridHeight, _gridWidth];
            _freeCellsCache = new List<GridCell>();
            _occupiedCellsCache = new List<GridCell>();
            
            Vector3 centerOffset = new Vector3(-(_gridWidth - 1) * _cellSpacing / 2f,
                                              -(_gridHeight - 1) * _cellSpacing / 2f,
                                               0f);
            
            for (int row = 0; row < _gridHeight; row++)
            {
                for (int col = 0; col < _gridWidth; col++)
                {
                    Vector3 cellPosition = new Vector3(col * _cellSpacing, row * _cellSpacing, 0f) + centerOffset;
                    
                    GameObject cellObject;
                    if (_cellPrefab != null)
                    {
                        cellObject = Instantiate(_cellPrefab, transform);
                    }
                    else
                    {
                        cellObject = new GameObject($"GridCell_{row}_{col}");
                        cellObject.transform.SetParent(transform);
                    }
                    
                    cellObject.name = $"GridCell_{row}_{col}";
                    cellObject.transform.localPosition = cellPosition;
                    
                    GridCell cellComponent = cellObject.GetComponent<GridCell>();
                    if (cellComponent == null)
                    {
                        cellComponent = cellObject.AddComponent<GridCell>();
                    }
                    
                    cellComponent.Initialize(row, col, cellPosition);
                    _gridCells[row, col] = cellComponent;
                    _freeCellsCache.Add(cellComponent);
                }
            }
        }
        
        /// <summary>
        /// Получает ячейку по заданным индексам строки и столбца
        /// </summary>
        /// <param name="row">Индекс строки (начиная с 0)</param>
        /// <param name="col">Индекс столбца (начиная с 0)</param>
        /// <returns>Ячейка сетки в указанной позиции или null, если индексы недействительны</returns>
        public GridCell GetCellAt(int row, int col)
        {
            if (IsValidCoordinate(row, col))
            {
                return _gridCells[row, col];
            }
            
#if UNITY_EDITOR
            Debug.LogWarning($"Invalid grid coordinates: ({row}, {col}). Grid size is {_gridHeight}x{_gridWidth}.");
#endif
            return null;
        }
        
        /// <summary>
        /// Получает ячейку по указанному одномерному индексу (в порядке строк)
        /// </summary>
        /// <param name="index">Одномерный индекс (начиная с 0, в порядке строк)</param>
        /// <returns>Ячейка сетки с указанным индексом или null, если индекс недействителен</returns>
        public GridCell GetCellAt(int index)
        {
            if (index < 0 || index >= TotalCellCount)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Invalid grid index: {index}. Valid range is 0 to {TotalCellCount - 1}.");
#endif
                return null;
            }
            
            int row = index / _gridWidth;
            int col = index % _gridWidth;
            
            return GetCellAt(row, col);
        }
        
        /// <summary>
        /// Получает все ячейки в сетке
        /// </summary>
        /// <returns>Массив всех объектов GridCell в сетке</returns>
        public GridCell[] GetAllCells()
        {
            GridCell[] allCells = new GridCell[TotalCellCount];
            
            for (int i = 0; i < TotalCellCount; i++)
            {
                int row = i / _gridWidth;
                int col = i % _gridWidth;
                allCells[i] = _gridCells[row, col];
            }
            
            return allCells;
        }
        
        /// <summary>
        /// Получает все свободные (пустые) ячейки в сетке
        /// </summary>
        /// <returns>Список всех незанятых объектов GridCell</returns>
        public List<GridCell> GetFreeCells()
        {
            _freeCellsCache.Clear();
            
            for (int row = 0; row < _gridHeight; row++)
            {
                for (int col = 0; col < _gridWidth; col++)
                {
                    if (_gridCells[row, col] != null && !_gridCells[row, col].IsOccupied)
                    {
                        _freeCellsCache.Add(_gridCells[row, col]);
                    }
                }
            }
            
            return _freeCellsCache;
        }
        
        /// <summary>
        /// Получает все занятые ячейки в сетке
        /// </summary>
        /// <returns>Список всех занятых объектов GridCell</returns>
        public List<GridCell> GetOccupiedCells()
        {
            _occupiedCellsCache.Clear();
            
            for (int row = 0; row < _gridHeight; row++)
            {
                for (int col = 0; col < _gridWidth; col++)
                {
                    if (_gridCells[row, col] != null && _gridCells[row, col].IsOccupied)
                    {
                        _occupiedCellsCache.Add(_gridCells[row, col]);
                    }
                }
            }
            
            return _occupiedCellsCache;
        }
        
        /// <summary>
        /// Получает случайную свободную (пустую) ячейку в сетке
        /// </summary>
        /// <returns>Случайная незанятая ячейка GridCell или null, если нет свободных ячеек</returns>
        public GridCell GetRandomFreeCell()
        {
            List<GridCell> freeCells = GetFreeCells();
            
            if (freeCells.Count == 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning("No free cells available in the grid.");
#endif
                return null;
            }
            
            int randomIndex = Random.Range(0, freeCells.Count);
            return freeCells[randomIndex];
        }
        
        /// <summary>
        /// Пытается назначить элемент в указанную ячейку
        /// </summary>
        /// <param name="row">Индекс строки целевой ячейки</param>
        /// <param name="col">Индекс столбца целевой ячейки</param>
        /// <param name="item">Преобразование элемента для назначения</param>
        /// <returns>True, если назначение прошло успешно, иначе false</returns>
        public bool AssignItemToCell(int row, int col, Transform item)
        {
            GridCell cell = GetCellAt(row, col);
            if (cell == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"Cannot assign item to invalid cell: ({row}, {col})");
#endif
                return false;
            }
            
            bool success = cell.AssignItem(item);
            
            if (success && item != null)
            {
                _freeCellsCache.Remove(cell);
                _occupiedCellsCache.Add(cell);
            }
            
            return success;
        }
        
        /// <summary>
        /// Пытается назначить элемент в случайную свободную ячейку
        /// </summary>
        /// <param name="item">Преобразование элемента для назначения</param>
        /// <returns>True, если назначение прошло успешно, иначе false</returns>
        public bool AssignItemToRandomFreeCell(Transform item)
        {
            GridCell freeCell = GetRandomFreeCell();
            if (freeCell == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Cannot assign item to random free cell: no free cells available.");
#endif
                return false;
            }
            
            return AssignItemToCell(freeCell.RowIndex, freeCell.ColumnIndex, item);
        }
        
        /// <summary>
        /// Очищает указанную ячейку, удаляя любой назначенный элемент
        /// </summary>
        /// <param name="row">Индекс строки целевой ячейки</param>
        /// <param name="col">Индекс столбца целевой ячейки</param>
        /// <returns>Преобразование удаленного элемента или null, если ячейка была уже пустой</returns>
        public Transform ClearCell(int row, int col)
        {
            GridCell cell = GetCellAt(row, col);
            if (cell == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"Cannot clear invalid cell: ({row}, {col})");
#endif
                return null;
            }
            
            Transform removedItem = cell.Clear();
            
            if (removedItem != null)
            {
                _occupiedCellsCache.Remove(cell);
                _freeCellsCache.Add(cell);
            }
            
            return removedItem;
        }
        
        /// <summary>
        /// Проверяет, действительны ли заданные координаты в пределах границ сетки
        /// </summary>
        /// <param name="row">Индекс строки для проверки</param>
        /// <param name="col">Индекс столбца для проверки</param>
        /// <returns>True, если координаты действительны, иначе false</returns>
        public bool IsValidCoordinate(int row, int col)
        {
            return row >= 0 && row < _gridHeight && col >= 0 && col < _gridWidth;
        }
        
        /// <summary>
        /// Очищает все ячейки в сетке
        /// </summary>
        public void ClearAllCells()
        {
            for (int row = 0; row < _gridHeight; row++)
            {
                for (int col = 0; col < _gridWidth; col++)
                {
                    if (_gridCells[row, col] != null)
                    {
                        _gridCells[row, col].Clear();
                    }
                }
            }
            
            _freeCellsCache.Clear();
            _occupiedCellsCache.Clear();
            
            for (int row = 0; row < _gridHeight; row++)
            {
                for (int col = 0; col < _gridWidth; col++)
                {
                    _freeCellsCache.Add(_gridCells[row, col]);
                }
            }
        }
        
        /// <summary>
        /// Получает одномерный индекс для заданных двумерных координат (в порядке строк)
        /// </summary>
        /// <param name="row">Индекс строки</param>
        /// <param name="col">Индекс столбца</param>
        /// <returns>Одномерный индекс или -1, если координаты недействительны</returns>
        public int GetIndexFromCoordinates(int row, int col)
        {
            if (!IsValidCoordinate(row, col))
            {
                return -1;
            }
            
            return row * _gridWidth + col;
        }
        
        /// <summary>
        /// Получает двумерные координаты для заданного одномерного индекса (в порядке строк)
        /// </summary>
        /// <param name="index">Одномерный индекс</param>
        /// <returns>Двумерные координаты в виде Vector2Int или (-1, -1), если индекс недействителен</returns>
        public Vector2Int GetCoordinatesFromIndex(int index)
        {
            if (index < 0 || index >= TotalCellCount)
            {
                return new Vector2Int(-1, -1);
            }
            
            int row = index / _gridWidth;
            int col = index % _gridWidth;
            
            return new Vector2Int(col, row);
        }
        
        /// <summary>
        /// Получает соседей определенной ячейки
        /// </summary>
        /// <param name="row">Индекс строки центральной ячейки</param>
        /// <param name="col">Индекс столбца центральной ячейки</param>
        /// <returns>Массив соседних объектов GridCell</returns>
        public GridCell[] GetNeighbors(int row, int col)
        {
            List<GridCell> neighbors = new List<GridCell>();
            
            int[] dRow = {-1, 0, 1, 0};
            int[] dCol = {0, 1, 0, -1};
            
            for (int i = 0; i < 4; i++)
            {
                int newRow = row + dRow[i];
                int newCol = col + dCol[i];
                
                if (IsValidCoordinate(newRow, newCol))
                {
                    neighbors.Add(_gridCells[newRow, newCol]);
                }
            }
            
            return neighbors.ToArray();
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            _gridWidth = Mathf.Max(1, _gridWidth);
            _gridHeight = Mathf.Max(1, _gridHeight);
            _cellSpacing = Mathf.Max(0.1f, _cellSpacing);
        }
        #endif
    }
}
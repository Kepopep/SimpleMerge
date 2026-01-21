using UnityEngine;

namespace SimpleMerge.Grid
{
    /// <summary>
    /// Представляет собой отдельную ячейку в системе сетки.
    /// Отвечает за управление состоянием занятости и мировой позицией.
    /// </summary>
    public class GridCell : MonoBehaviour
    {
        [Header("Конфигурация сетки")]
        [Tooltip("Индекс строки этой ячейки в сетке")]
        [SerializeField] private int _rowIndex;
        
        [Tooltip("Индекс столбца этой ячейки в сетке")]
        [SerializeField] private int _columnIndex;
        
        [Header("Данные позиции")]
        [Tooltip("Локальное смещение от начала координат сетки")]
        [SerializeField] private Vector3 _localOffset = Vector3.zero;
        
        // Ссылка на объект, который в данный момент занимает эту ячейку
        private Transform _itemTransform;
        
        /// <summary>
        /// Получает индекс строки этой ячейки в сетке
        /// </summary>
        public int RowIndex => _rowIndex;
        
        /// <summary>
        /// Получает индекс столбца этой ячейки в сетке
        /// </summary>
        public int ColumnIndex => _columnIndex;
        
        /// <summary>
        /// Получает мировую позицию этой ячейки
        /// </summary>
        public Vector3 WorldPosition => transform.position;
        
        /// <summary>
        /// Получает локальное смещение этой ячейки от начала координат сетки
        /// </summary>
        public Vector3 LocalOffset => _localOffset;
        
        /// <summary>
        /// Получает, занята ли в данный момент эта ячейка каким-либо элементом
        /// </summary>
        public bool IsOccupied => _itemTransform != null;
        
        /// <summary>
        /// Получает трансформацию элемента, который в данный момент занимает эту ячейку
        /// Возвращает null, если ячейка пуста
        /// </summary>
        public Transform OccupyingItem => _itemTransform;
        
        /// <summary>
        /// Инициализирует ячейку с её позицией в сетке
        /// </summary>
        /// <param name="row">Индекс строки в сетке</param>
        /// <param name="col">Индекс столбца в сетке</param>
        /// <param name="offset">Локальное смещение от начала координат сетки</param>
        public void Initialize(int row, int col, Vector3 offset)
        {
            _rowIndex = row;
            _columnIndex = col;
            _localOffset = offset;
            
            transform.localPosition = _localOffset;
        }
        
        /// <summary>
        /// Назначает элемент в эту ячейку
        /// </summary>
        /// <param name="item">Трансформация элемента для назначения</param>
        /// <returns>True, если назначение прошло успешно, false, если ячейка уже занята</returns>
        public bool AssignItem(Transform item)
        {
            if (IsOccupied)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Cell ({_rowIndex}, {_columnIndex}) is already occupied!");
#endif
                return false;
            }
            
            _itemTransform = item;
            
            if (item != null)
            {
                item.SetParent(transform);
                item.localPosition = Vector3.zero;
                item.localRotation = Quaternion.identity;
            }
            
            return true;
        }
        
        /// <summary>
        /// Очищает ячейку, удаляя любой назначенный элемент
        /// </summary>
        /// <returns>Трансформация удаленного элемента или null, если ячейка была пустой</returns>
        public Transform Clear()
        {
            Transform removedItem = _itemTransform;
            _itemTransform = null;
            return removedItem;
        }
        
        /// <summary>
        /// Проверяет, является ли эта ячейка соседней для другой ячейки
        /// </summary>
        /// <param name="other">Другая ячейка, с которой проверяется смежность</param>
        /// <returns>True, если ячейки являются соседними (по горизонтали или вертикали), иначе false</returns>
        public bool IsAdjacentTo(GridCell other)
        {
            if (other == null) return false;
            
            int rowDiff = Mathf.Abs(_rowIndex - other._rowIndex);
            int colDiff = Mathf.Abs(_columnIndex - other._columnIndex);
            
            return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
        }
        
        /// <summary>
        /// Устанавливает локальное смещение ячейки от начала координат сетки
        /// </summary>
        /// <param name="offset">Новая позиция локального смещения</param>
        public void SetLocalOffset(Vector3 offset)
        {
            _localOffset = offset;
            transform.localPosition = _localOffset;
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                transform.localPosition = _localOffset;
            }
        }
        #endif
    }
}

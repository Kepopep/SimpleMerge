using UnityEngine;
using SimpleMerge.Grid;

namespace SimpleMerge.Scripts
{
    /// <summary>
    /// Представляет собой один объединяемый игровой объект, который можно поместить внутрь GridCell.
    /// Обрабатывает назначение в ячейки сетки, управление значением и синхронизацию позиции.
    /// </summary>
    public class MergeItem : MonoBehaviour
    {
        [Header("Конфигурация элемента слияния")]
        [Tooltip("Уровень слияния или стоимость этого элемента")]
        [SerializeField] private int _value = 1;
        
        // Reference to the current grid cell this item occupies
        private GridCell _currentCell;
        
        /// <summary>
        /// Получает текущее значение этого элемента слияния (уровень слияния или стоимость)
        /// Это только для чтения из внешних источников
        /// </summary>
        public int Value => _value;
        
        /// <summary>
        /// Получает текущую ячейку сетки, которую занимает этот элемент
        /// Возвращает null, если не назначен ни одной ячейке
        /// </summary>
        public GridCell CurrentCell => _currentCell;
        
        /// <summary>
        /// Получает, назначен ли в данный момент этот элемент ячейке сетки
        /// </summary>
        public bool IsAssigned => _currentCell != null;
        
        /// <summary>
        /// Назначает этот элемент слияния в конкретную ячейку сетки
        /// </summary>
        /// <param name="cell">Целевая ячейка сетки для назначения</param>
        /// <returns>True, если назначение прошло успешно, иначе false</returns>
        public bool AssignToCell(GridCell cell)
        {
            if (cell == null)
            {
                Debug.LogWarning("Cannot assign MergeItem to null cell");
                return false;
            }
            
            // Сначала очистить из любой предыдущей ячейки
            if (_currentCell != null)
            {
                RemoveFromCurrentCell();
            }
            
            // Попытаться назначить в новую ячейку
            bool success = cell.AssignItem(this.transform);
            if (success)
            {
                _currentCell = cell;
                
                // Позиционировать этот объект в месте расположения ячейки
                UpdatePositionToCell();
            }
            
            return success;
        }
        
        /// <summary>
        /// Удаляет этот элемент слияния из своей текущей ячейки
        /// </summary>
        /// <returns>True, если успешно удалено, false, если не назначен ни одной ячейке</returns>
        public bool RemoveFromCurrentCell()
        {
            if (_currentCell == null)
            {
                Debug.LogWarning("Cannot remove MergeItem from null cell");
                return false;
            }
            
            // Очистить элемент из ячейки
            Transform removedItem = _currentCell.Clear();
            if (removedItem == this.transform)
            {
                _currentCell = null;
                return true;
            }
            
            // Если мы не смогли успешно удалить нашу трансформацию, что-то пошло не так
            Debug.LogError("Failed to properly remove MergeItem from current cell");
            _currentCell = null;
            return false;
        }
        
        /// <summary>
        /// Обновляет мировую позицию этого элемента слияния, чтобы она соответствовала его текущей ячейке
        /// </summary>
        public void UpdatePositionToCell()
        {
            if (_currentCell != null)
            {
                // Установить нашу позицию в соответствии с позицией ячейки
                transform.position = _currentCell.WorldPosition;
            }
        }
        
        /// <summary>
        /// Улучшает этот элемент слияния, увеличивая его значение на 1
        /// Не обрабатывает логику слияния внутри - этим управляют извне
        /// </summary>
        public void Upgrade()
        {
            _value++;
        }
        
        /// <summary>
        /// Получает координаты сетки текущей ячейки, которую занимает этот элемент
        /// </summary>
        /// <param name="row">Выходной индекс строки или -1, если не назначен ячейке</param>
        /// <param name="col">Выходной индекс столбца или -1, если не назначен ячейке</param>
        public void GetCurrentCellCoordinates(out int row, out int col)
        {
            if (_currentCell != null)
            {
                row = _currentCell.RowIndex;
                col = _currentCell.ColumnIndex;
            }
            else
            {
                row = -1;
                col = -1;
            }
        }
        
        /// <summary>
        /// Вызывается при уничтожении объекта для обеспечения правильной очистки
        /// </summary>
        private void OnDestroy()
        {
            // Убедиться, что мы удалены из ячейки, если все еще назначены
            if (_currentCell != null)
            {
                _currentCell.Clear();
                _currentCell = null;
            }
        }
    }
}
using UnityEngine;
using UnityEngine.Events;
using SimpleMerge.Grid;
using SimpleMerge.Scripts;

namespace SimpleMerge.DragDrop
{
    /// <summary>
    /// Handles drag and drop functionality for MergeItem objects within a 3x3 grid.
    /// Manages mouse input, raycasting, and temporary movement of items during drag operations.
    /// </summary>
    public class DragController : MonoBehaviour
    {
        [Header("Drag Configuration")]
        [Tooltip("Layer mask for detecting MergeItem objects during raycast")]
        [SerializeField] private LayerMask _itemLayerMask = 1 << 0; 
        
        [Tooltip("Layer mask for detecting GridCell objects during raycast")]
        [SerializeField] private LayerMask _cellLayerMask = 1 << 0; 
        
        [Tooltip("Camera used for raycasting (will use main camera if null)")]
        [SerializeField] private Camera _camera;
        
        [Header("Drag Settings")]
        [Tooltip("Offset distance above the ground plane for dragging items")]
        [SerializeField] private float _dragHeightOffset = 1.0f;
        
        // Private fields to track drag state
        private MergeItem _draggedItem;
        private GridCell _originalCell;
        private bool _isDragging = false;
        private Vector3 _dragOffset;
        private Vector3 _raycastHitPoint;
        
        // Events for communication with higher-level gameplay logic
        public UnityEvent<MergeItem, GridCell> OnDropOnEmptyCell = new UnityEvent<MergeItem, GridCell>();
        public UnityEvent<MergeItem, GridCell> OnDropOnOccupiedCell = new UnityEvent<MergeItem, GridCell>();
        public UnityEvent<MergeItem, MergeItem, GridCell> OnMergeAttempt = new UnityEvent<MergeItem, MergeItem, GridCell>();
        
        /// <summary>
        /// Gets whether an item is currently being dragged
        /// </summary>
        public bool IsDragging => _isDragging;
        
        /// <summary>
        /// Gets the currently dragged item (null if none)
        /// </summary>
        public MergeItem DraggedItem => _draggedItem;
        
        /// <summary>
        /// Gets the original cell of the currently dragged item (null if none)
        /// </summary>
        public GridCell OriginalCell => _originalCell;
        
        private void Start()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleDragStart();
            }
            
            if (_isDragging && Input.GetMouseButton(0))
            {
                HandleDragMove();
            }
            
            if (_isDragging && Input.GetMouseButtonUp(0))
            {
                HandleDragEnd();
            }
        }
        
        /// <summary>
        /// Initiates the drag operation by detecting a MergeItem under the mouse cursor
        /// </summary>
        private void HandleDragStart()
        {
            if (_isDragging) return;
            
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _itemLayerMask))
            {
                MergeItem item = hit.collider.GetComponent<MergeItem>();
                
                if (item != null)
                {
                    _draggedItem = item;
                    _originalCell = item.CurrentCell;
                    
                    _raycastHitPoint = hit.point;
                    _dragOffset = item.transform.position - _raycastHitPoint;
                    _dragOffset.y = 0; // Only consider horizontal offset
                    
                    _isDragging = true;
                    
                    if (_originalCell != null)
                    {
                        _draggedItem.RemoveFromCurrentCell();
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates the position of the dragged item to follow the mouse cursor
        /// </summary>
        private void HandleDragMove()
        {
            if (!_isDragging || _draggedItem == null) return;
            
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _cellLayerMask | _itemLayerMask))
            {
                Vector3 targetPosition = hit.point + _dragOffset;
                targetPosition.z = _dragHeightOffset; // Maintain consistent height during drag
                
                _draggedItem.transform.position = targetPosition;
            }
            else
            {
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                float distance;
                
                if (groundPlane.Raycast(ray, out distance))
                {
                    Vector3 targetPosition = ray.GetPoint(distance) + _dragOffset;
                    targetPosition.y = _dragHeightOffset;
                    
                    _draggedItem.transform.position = targetPosition;
                }
            }
        }
        
        /// <summary>
        /// Completes the drag operation by placing the item in the target cell
        /// </summary>
        private void HandleDragEnd()
        {
            if (!_isDragging || _draggedItem == null) return;
            
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            GridCell targetCell = null;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, _cellLayerMask))
            {
                targetCell = hit.collider.GetComponent<GridCell>();
            }
            
            if (targetCell == null)
            {
                ReturnItemToOriginalCell();
            }
            else
            {
                if (targetCell.IsOccupied)
                {
                    MergeItem occupantItem = targetCell.OccupyingItem?.GetComponent<MergeItem>();
                    
                    if (occupantItem != null && occupantItem.Value == _draggedItem.Value)
                    {
                        OnMergeAttempt.Invoke(_draggedItem, occupantItem, targetCell);
                        
                        occupantItem.Upgrade();
                        GameObject.Destroy(_draggedItem.gameObject);
                    }
                    else
                    {
                        OnDropOnOccupiedCell.Invoke(_draggedItem, targetCell);
                        
                        ReturnItemToOriginalCell();
                    }
                }
                else
                {
                    if (_draggedItem.AssignToCell(targetCell))
                    {
                        OnDropOnEmptyCell.Invoke(_draggedItem, targetCell);
                    }
                    else
                    {
                        ReturnItemToOriginalCell();
                    }
                }
            }
            
            _isDragging = false;
            _draggedItem = null;
            _originalCell = null;
        }
        
        /// <summary>
        /// Returns the dragged item to its original cell if still available
        /// </summary>
        private void ReturnItemToOriginalCell()
        {
            if (_draggedItem != null && _originalCell != null)
            {
                if (!_originalCell.IsOccupied)
                {
                    _draggedItem.AssignToCell(_originalCell);
                }
                else
                {
                    Debug.LogWarning("Original cell is occupied, unable to return item.");
                }
            }
        }
        
        /// <summary>
        /// Cancels the current drag operation and returns the item to its original position
        /// </summary>
        public void CancelDrag()
        {
            if (_isDragging)
            {
                ReturnItemToOriginalCell();
                
                _isDragging = false;
                _draggedItem = null;
                _originalCell = null;
            }
        }
        
        /// <summary>
        /// Validates if a drag operation is possible (useful for UI feedback)
        /// </summary>
        public bool CanStartDrag()
        {
            return !_isDragging;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            _dragHeightOffset = Mathf.Max(0.1f, _dragHeightOffset);
        }
#endif
    }
}
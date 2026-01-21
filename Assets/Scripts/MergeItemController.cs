using UnityEngine;
using SimpleMerge.Grid;
using SimpleMerge.Scripts;

namespace SimpleMerge.Scripts
{
    /// <summary>
    /// Controller script for a merge item that can be used as a prefab
    /// </summary>
    public class MergeItemController : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _baseColor = Color.white;
        
        private MergeItem mergeItem;
        
        private void Awake()
        {
            mergeItem = GetComponent<MergeItem>() ?? gameObject.AddComponent<MergeItem>();
            
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                }
            }
            
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _baseColor;
            }
        }
        
        private void Update()
        {
            if (mergeItem != null && mergeItem.IsAssigned)
            {
                mergeItem.UpdatePositionToCell();
            }
            
            // Update visual representation based on value
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (_spriteRenderer != null && mergeItem != null)
            {
                float hue = Mathf.Clamp01(mergeItem.Value / 20.0f); // Scale value to 0-1 range for hue
                _spriteRenderer.color = Color.HSVToRGB(hue, 0.8f, 0.9f);
            }
        }
        
        /// <summary>
        /// Public method to trigger an upgrade of this merge item
        /// </summary>
        public void TriggerUpgrade()
        {
            if (mergeItem != null)
            {
                mergeItem.Upgrade();
                UpdateVisuals(); 
            }
        }
        
        /// <summary>
        /// Gets the current value of this merge item
        /// </summary>
        public int GetValue()
        {
            return mergeItem != null ? mergeItem.Value : 0;
        }
        
        /// <summary>
        /// Gets whether this merge item is currently assigned to a grid cell
        /// </summary>
        public bool IsAssigned()
        {
            return mergeItem != null && mergeItem.IsAssigned;
        }
    }
}
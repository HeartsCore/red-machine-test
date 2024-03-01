using System;
using UnityEngine;

namespace Connection
{
    public class ColorNodeTarget : MonoBehaviour
    {
        [SerializeField] private Color targetColor;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ColorNode colorNode;
        
        public event Action<ColorNodeTarget, bool> TargetCompletionChangeEvent;
        
        public bool IsCompleted => targetColor == colorNode.Color;
        
        private void Awake()
        {
            colorNode.ColorChangedEvent += OnColorChanged;
        }

        private void OnDestroy()
        {
            colorNode.ColorChangedEvent -= OnColorChanged;
        }

        private void OnColorChanged(Color currentColor)
        {
            TargetCompletionChangeEvent?.Invoke(this, targetColor == currentColor);
        }

        private void OnValidate()
        {
            spriteRenderer.color = targetColor;
        }
    }
}

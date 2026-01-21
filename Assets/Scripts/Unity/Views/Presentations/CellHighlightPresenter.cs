using Project.Gameplay.Components;
using Project.Gameplay.Presentations;
using UnityEngine;

namespace Project.Unity.Presentations
{
    public class CellHighlightPresenter : MonoBehaviour, IPresenter
    {
        [Header("Highlight")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _highlightColor = new(1f, 1f, 0.5f, 1f);
        
        private EntityLink _link;
        private Color _originalColor;
        private Material _material;

        public void Init(EntityLink link)
        {
            _link = link; 
            _material = _renderer.material;
            _originalColor = _material.color;
        }

        private void Update()
        {
            if(_link == null)
                return;
            
            var entity = _link.GetEntity();
            SetHighlight(entity.Exists<HighlightComponentTag>());
        }
        
        private void SetHighlight(bool enabled)
        {
            if (_material != null)
                _material.color = enabled ? _highlightColor : _originalColor;
        }
    }
}
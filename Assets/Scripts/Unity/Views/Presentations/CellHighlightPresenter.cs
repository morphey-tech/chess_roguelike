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
        [SerializeField] private Color _attackTargetColor = new(1f, 0.5f, 0.5f, 1f);
        
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
            if(entity.Exists<HighlightTag>())
                SetHighlight();
            else if(entity.Exists<AttackHighlightTag>())
                SetAttackHighlight();
            else
                SetDefault();
        }
        
        private void SetDefault()
        {
            if (_material != null)
                _material.color = _originalColor;
        }
        
        private void SetHighlight()
        {
            if (_material != null)
                _material.color = _highlightColor;
        }
        
        private void SetAttackHighlight()
        {
            if (_material != null)
                _material.color = _attackTargetColor;
        }
    }
}
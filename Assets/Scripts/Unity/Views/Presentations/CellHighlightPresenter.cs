using Project.Gameplay.Components;
using Project.Gameplay.Presentations;
using Shapes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Unity.Presentations
{
    public class CellHighlightPresenter : MonoBehaviour, IPresenter
    {
        [FormerlySerializedAs("_renderer")]
        [Header("Highlight")]
        [SerializeField] private GameObject _highlightRenderer;
        [SerializeField] private GameObject _attackRenderer;

        private EntityLink _link;
        
        public void Init(EntityLink link)
        {
            _link = link; 
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
            _highlightRenderer.gameObject.SetActive(false);
            _attackRenderer.gameObject.SetActive(false);
        }
        
        private void SetHighlight()
        {
            _highlightRenderer.gameObject.SetActive(true);
            _attackRenderer.gameObject.SetActive(false);
        }
        
        private void SetAttackHighlight()
        {
            _highlightRenderer.gameObject.SetActive(false);
            _attackRenderer.gameObject.SetActive(true);
        }
    }
}
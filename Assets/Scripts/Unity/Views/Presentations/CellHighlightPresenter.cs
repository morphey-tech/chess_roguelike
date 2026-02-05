using Project.Gameplay;
using Project.Gameplay.Components;
using Project.Gameplay.Presentations;
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
            Debug.Log($"[CellHighlightPresenter] Init called on {gameObject.name}, link: {link?.EntityId}");
        }

        private void Update()
        {
            if (_link == null)
            {
                // Only log once to avoid spam
                if (!_loggedOnce)
                {
                    Debug.LogWarning($"[CellHighlightPresenter] _link is null on {gameObject.name}");
                    _loggedOnce = true;
                }
                return;
            }
            
            Entity entity = _link.GetEntity();
            if (entity == null)
                return;
            
            if (entity.Exists<HighlightTag>())
                SetHighlight();
            else if (entity.Exists<AttackHighlightTag>())
                SetAttackHighlight();
            else
                SetDefault();
        }
        
        private bool _loggedOnce;
        
        private void SetDefault()
        {
            if (_highlightRenderer != null)
                _highlightRenderer.SetActive(false);
            if (_attackRenderer != null)
                _attackRenderer.SetActive(false);
        }
        
        private void SetHighlight()
        {
            if (_highlightRenderer != null)
                _highlightRenderer.SetActive(true);
            if (_attackRenderer != null)
                _attackRenderer.SetActive(false);
        }
        
        private void SetAttackHighlight()
        {
            if (_highlightRenderer != null)
                _highlightRenderer.SetActive(false);
            if (_attackRenderer != null)
                _attackRenderer.SetActive(true);
        }
    }
}
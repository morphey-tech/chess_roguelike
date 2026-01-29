using Project.Gameplay.Components;
using Project.Gameplay.Presentations;
using UnityEngine;

namespace Project.Unity.Presentations
{
    public class FigureSelectPresenter : MonoBehaviour, IPresenter
    {
        [Header("Highlight")] 
        [SerializeField] private GameObject _highlightObject;
        
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
            _highlightObject.SetActive(entity.Exists<SelectTag>());
        }
    }
}
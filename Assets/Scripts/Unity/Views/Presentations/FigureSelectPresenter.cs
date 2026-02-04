using Project.Gameplay.Components;
using Project.Gameplay.Presentations;
using UnityEngine;



namespace Project.Unity.Presentations
{
    public class FigureSelectPresenter : MonoBehaviour, IPresenter
    {
        [SerializeField] private Color _color;
        [SerializeField] private float _width;
        
        private EntityLink _link;
        private Outline _outline;
        
        public void Init(EntityLink link)
        {
            _link = link; 
        }

        private void Update()
        {
            if(_link == null || _outline == null)
                return;

            var entity = _link.GetEntity();
            _outline.enabled = entity.Exists<SelectTag>();
        }

        // билят, вьюшку бы спавнить в тот же момент когда презентер спавнится, шоб иниты нормально вызывать
        public void InitSelecting()
        {
            _outline = GetComponentInChildren<Renderer>().gameObject.AddComponent<Outline>();
            _outline.OutlineColor = _color;
            _outline.OutlineWidth = _width;
        }
    }
}
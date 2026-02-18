using Project.Gameplay.Components;
using Project.Gameplay.Presentations;
using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    public class FigureSelectPresenter : MonoBehaviour, IPresenter
    {
        [SerializeField] private Color _color;
        [SerializeField] private float _width;
        
        private EntityLink _link;
        private Outline[] _outlines = System.Array.Empty<Outline>();
        
        public void Init(EntityLink link)
        {
            _link = link; 
        }

        private void Update()
        {
            if (_link == null || _outlines.Length == 0)
                return;

            var entity = _link.GetEntity();
            bool enabled = entity.Exists<SelectTag>();
            foreach (Outline outline in _outlines)
            {
                if (outline != null)
                    outline.enabled = enabled;
            }
        }

        // билят, вьюшку бы спавнить в тот же момент когда презентер спавнится, шоб иниты нормально вызывать
        public void InitSelecting()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            var list = new System.Collections.Generic.List<Outline>(renderers.Length);
            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;

                Outline outline = r.GetComponent<Outline>();
                if (outline == null)
                    outline = r.gameObject.AddComponent<Outline>();
                outline.OutlineColor = _color;
                outline.OutlineWidth = _width;
                outline.enabled = false;
                list.Add(outline);
            }
            _outlines = list.ToArray();
        }
    }
}
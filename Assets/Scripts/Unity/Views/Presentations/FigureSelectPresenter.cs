using Project.Gameplay.Components;
using Project.Gameplay.Presentations;
using UnityEngine;

namespace Project.Unity.Unity.Views.Presentations
{
    public class FigureSelectPresenter : MonoBehaviour, IPresenter
    {
        [SerializeField] private Color _color;
        [SerializeField] private float _width;
        
        private FigureView Figure;
        private Outline[] _outlines = System.Array.Empty<Outline>();

        private void Start()
        {
            Figure = GetComponent<FigureView>();
            InitSelecting();
        }

        private void Update()
        {
            foreach (Outline outline in _outlines)
            {
                if (outline != null)
                    outline.enabled = Figure.Selected;
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

        public void Init(EntityLink link)
        {
            throw new System.NotImplementedException();
        }
    }
}
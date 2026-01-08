using UnityEngine;

namespace LiteUI.Element.PooledScroll
{
    public abstract class PooledScrollItemPanel<T> : PooledScrollItemPanel
            where T : IPooledScrollItemViewModel
    {
        protected T ItemModel { get; private set; } = default!;

        public sealed override void Configure(object itemModel)
        {
            ItemModel = (T) itemModel;
        }
    }

    public abstract class PooledScrollItemPanel : MonoBehaviour
    {
        public abstract void Configure(object itemModel);
        public abstract void Reinitialize();
        public abstract void Refresh();

        public Vector2 PanelSize
        {
          get
          {
            if (_rectTransform == null) {
              _rectTransform = GetComponent<RectTransform>();
            }
            return _rectTransform.sizeDelta;
          }
        }

        public Vector2 Position
        {
          set
          {
            if (_rectTransform == null) {
              _rectTransform = GetComponent<RectTransform>();
            }
            _rectTransform.anchoredPosition = value;
          }
        }

        private RectTransform? _rectTransform;
    }
}

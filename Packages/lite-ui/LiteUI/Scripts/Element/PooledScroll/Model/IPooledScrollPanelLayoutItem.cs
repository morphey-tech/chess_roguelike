using UnityEngine;

namespace LiteUI.Element.PooledScroll.Model
{
    public interface IPooledScrollPanelLayoutItem
    {
        public IPooledScrollItemViewModel ItemViewModel { get; }

        public bool NeedChangePanelType { get; }
        public bool NeedReinitialize { get; }
        public bool NeedRefresh { get; }
        
        public Vector2 Position { get; }
    }
}

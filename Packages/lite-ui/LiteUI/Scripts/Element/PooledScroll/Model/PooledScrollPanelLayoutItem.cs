using UnityEngine;

namespace LiteUI.Element.PooledScroll.Model
{
    public class PooledScrollPanelLayoutItem : IPooledScrollPanelLayoutItem
    {
        private static readonly Vector2 OFFSCREEN_POSITION = new(-1000, -1000); 

        public IPooledScrollItemViewModel ItemViewModel { get; private set; }

        public bool NeedChangePanelType { get; private set; }
        public bool NeedReinitialize { get; private set; }
        public bool NeedRefresh { get; private set; }
        
        public Vector2 Size { get; set; }
        public Vector2 Position { get; set; } = OFFSCREEN_POSITION;

        public PooledScrollPanelLayoutItem(IPooledScrollItemViewModel itemViewModel, Vector2 initialSize)
        {
            ItemViewModel = itemViewModel;
            NeedChangePanelType = true;
            NeedReinitialize = true;
            NeedRefresh = true;
            Size = initialSize;
        }

        public void UpdateItemViewModel<TVm>(TVm item, Vector2 initialSize)
                where TVm : IPooledScrollItemViewModel
        {
            if (ItemViewModel.GetType() != item.GetType()) {
                NeedChangePanelType = true;
                NeedReinitialize = true;
            } else if (ItemViewModel.Id != item.Id) {
                NeedReinitialize = true;
            }
            ItemViewModel = item;
            NeedRefresh = true;
            Size = initialSize;
        }

        public void Refresh()
        {
            NeedRefresh = true;
        }

        public void ClearDirty()
        {
            NeedChangePanelType = false;
            NeedReinitialize = false;
            NeedRefresh = false;
        }
    }
}

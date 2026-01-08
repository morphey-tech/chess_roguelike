using System.Collections.Generic;
using System.Linq;
using LiteUI.Common.Logger;
using LiteUI.Common.Extensions;
using LiteUI.Common.Utils;
using UnityEngine;

namespace LiteUI.Element.PooledScroll.Model
{
  public class PooledScrollPanelLayout
  {
    private static readonly IUILogger _logger = LoggerFactory.GetLogger<PooledScrollPanelLayout>();

    private const float MIN_PANEL_SIZE = 10f;

    private readonly PooledScrollViewModel _pooledScrollViewModel;
    private readonly PooledScrollCacheManager _pooledScrollCacheManager;

    private readonly List<PooledScrollPanelLayoutItem> _items = new();

    public bool Dirty { get; private set; }

    public PooledScrollPanelLayout(PooledScrollViewModel pooledScrollViewModel,
                                   PooledScrollCacheManager pooledScrollCacheManager)
    {
      _pooledScrollViewModel = pooledScrollViewModel;
      _pooledScrollCacheManager = pooledScrollCacheManager;
    }

    public void UpdateSizesToInitial()
    {
      foreach (PooledScrollPanelLayoutItem item in _items) {
        Vector2 modelSize = GetPanelInitialSize();
        Vector2 objectSize = _pooledScrollCacheManager.GetInitialSize(item.ItemViewModel.GetType());
        bool dynamic = _pooledScrollViewModel.DynamicItemSize;
        bool vertical = _pooledScrollViewModel.Vertical;

        if (!MathUtils.IsFloatEquals(objectSize.x, modelSize.x)) {
          if (!dynamic || vertical) {
            _logger.Warn(
              $"Panel widths not equal, sizeInPrefab={objectSize.x}, sizeInPooledScrollVM={modelSize.x}");
          }
        }
        if (!MathUtils.IsFloatEquals(objectSize.y, modelSize.y)) {
          if (!dynamic || !vertical) {
            _logger.Warn(
              $"Panel heights not equal, sizeInPrefab={objectSize.y}, sizeInPooledScrollVM={modelSize.y}");
          }
        }
        item.Size = modelSize;
      }
    }

    public void UpdateItems<TVm>(List<TVm> items)
        where TVm : IPooledScrollItemViewModel
    {
      List<PooledScrollPanelLayoutItem> newItems = new(items.Count);
      foreach (TVm item in items) {
        PooledScrollPanelLayoutItem? oldLayoutItem =
            _items.FirstOrDefault(it => it.ItemViewModel.Id == item.Id);
        if (oldLayoutItem == null) {
          newItems.Add(new PooledScrollPanelLayoutItem(item, GetPanelInitialSize()));
          continue;
        }
        oldLayoutItem.UpdateItemViewModel(item, GetPanelInitialSize());
        newItems.Add(oldLayoutItem);
      }
      _items.Clear();
      _items.AddRange(newItems);
      Dirty = true;
    }

    public void RefreshItem<TVm>(TVm item)
        where TVm : IPooledScrollItemViewModel
    {
      PooledScrollPanelLayoutItem? itemModel = FindItemByViewModel(item);
      if (itemModel == null) {
        _logger.Warn($"No item with model found to refresh. itemId={item.Id}");
        return;
      }
      itemModel.Refresh();
      Dirty = true;
    }

    public void RefreshAllItems()
    {
      _items.ForEach(it => it.Refresh());
      Dirty = true;
    }

    public void AddItem<TVm>(TVm item)
        where TVm : IPooledScrollItemViewModel
    {
      Vector2 initialSize = GetPanelInitialSize();
      _items.Add(new PooledScrollPanelLayoutItem(item, initialSize));
      Dirty = true;
    }

    public void RemoveItem<TVm>(TVm item)
        where TVm : IPooledScrollItemViewModel
    {
      PooledScrollPanelLayoutItem? itemModel = FindItemByViewModel(item);
      if (itemModel == null) {
        _logger.Warn($"No item with model found to remove. itemId={item.Id}");
        return;
      }
      _items.Remove(itemModel);
      Dirty = true;
    }

    public void ReplaceItem<TVm>(int index, TVm item)
        where TVm : IPooledScrollItemViewModel
    {
      if (index >= _items.Count) {
        _logger.Warn(
          $"Try to replace item out of range. index={index} size={_items.Count} itemId={item.Id}");
        return;
      }
      Vector2 initialSize = GetPanelInitialSize();
      _items[index].UpdateItemViewModel(item, initialSize);
      Dirty = true;
    }

    public void UpdateSize(IPooledScrollPanelLayoutItem panelLayoutItem, Vector2 panelSize)
    {
      PooledScrollPanelLayoutItem panelModel = (PooledScrollPanelLayoutItem)panelLayoutItem;
      if (panelModel.Size.FuzzyEquals(panelSize)) {
        return;
      }
      panelModel.Size = panelSize;
      Dirty = true;
    }

    public void RecalculateLayout()
    {
      if (!Dirty) {
        return;
      }

      if (_pooledScrollViewModel.Vertical) {
        float x = _pooledScrollViewModel.LeftPanelPadding;
        float y = _pooledScrollViewModel.TopPanelPadding;

        int columnCount = _pooledScrollViewModel.ColumnCount;
        for (int i = 0; i < _items.Count; i++) {
          PooledScrollPanelLayoutItem item = _items[i];
          item.Position = new Vector2(x, y);

          int currentColumn = i % columnCount;
          if (columnCount == currentColumn + 1) {
            x = _pooledScrollViewModel.LeftPanelPadding;
            y += item.Size.y + _pooledScrollViewModel.VerticalItemPadding;
            continue;
          }
          x += item.Size.x + _pooledScrollViewModel.HorizontalItemPadding;
        }
      }
      else {
	      float x = _pooledScrollViewModel.LeftPanelPadding;
	      float y = _pooledScrollViewModel.TopPanelPadding;

	      int rowCount = _pooledScrollViewModel.RowCount;
	      for (int i = 0; i < _items.Count; i++) {
		      PooledScrollPanelLayoutItem item = _items[i];
		      item.Position = new Vector2(x, y);

		      int currentRow = i % rowCount;
		      if (rowCount == currentRow + 1) {
			      y = _pooledScrollViewModel.TopPanelPadding;
			      x += item.Size.x + _pooledScrollViewModel.HorizontalItemPadding;
			      continue;
		      }
		      y += item.Size.y + _pooledScrollViewModel.VerticalItemPadding;
	      }
      }
    }

    public List<IPooledScrollPanelLayoutItem> ReceiveVisibleItems(
      Vector2 fromPosition, Vector2 viewPortSize)
    {
      List<IPooledScrollPanelLayoutItem> result = new();

      foreach (PooledScrollPanelLayoutItem item in _items) {
        if (_pooledScrollViewModel.Vertical) {
          if (item.Position.y + item.Size.y < fromPosition.y) {
            continue;
          }
          if (item.Position.y > fromPosition.y + viewPortSize.y) {
            continue;
          }
          result.Add(item);
        }
        else {
          if (item.Position.x + item.Size.x + fromPosition.x < 0f) {
            continue;
          }
          if (item.Position.x > viewPortSize.x - fromPosition.x) {
            continue;
          }
	        result.Add(item);
        }
      }
      return result;
    }

    public void ClearDirty()
    {
      Dirty = false;
      _items.ForEach(it => it.ClearDirty());
    }

    public IPooledScrollPanelLayoutItem? FindItem<TVm>(TVm itemViewModel)
        where TVm : IPooledScrollItemViewModel
    {
      return _items.FirstOrDefault(it => ReferenceEquals(it.ItemViewModel, itemViewModel));
    }

    public IPooledScrollPanelLayoutItem? FirstItem()
    {
      return _items.FirstOrDefault();
    }

    public IPooledScrollPanelLayoutItem? LastItem()
    {
      return _items.LastOrDefault();
    }

    private PooledScrollPanelLayoutItem? FindItemByViewModel(IPooledScrollItemViewModel viewModel)
    {
      foreach (PooledScrollPanelLayoutItem item in _items) {
        if (!ReferenceEquals(item.ItemViewModel, viewModel)) {
          continue;
        }
        return item;
      }
      return null;
    }

    public float GetScrollNormalizedPositionForItem(IPooledScrollPanelLayoutItem panelLayoutItem,
                                                    Vector2 currentViewportSize)
    {
      PooledScrollPanelLayoutItem panelModel = (PooledScrollPanelLayoutItem)panelLayoutItem;
      if (_pooledScrollViewModel.Vertical) {
        float offset = panelModel.Position.y - _pooledScrollViewModel.TopPanelPadding;
        float fullHeight = GetContentSize().y - currentViewportSize.y;
        float position = (1f - offset / fullHeight);
        return Mathf.Clamp01(position);
      }
      else
      {
	      float offset = panelModel.Position.x - _pooledScrollViewModel.LeftPanelPadding;
	      float fullWidth = GetContentSize().x - currentViewportSize.x;
	      float position = (1f - offset / fullWidth);
	      return Mathf.Clamp01(position);
      }
    }

    public Vector2 GetContentSize()
    {
      if (_items.Count == 0) {
        return Vector2.zero;
      }
      PooledScrollPanelLayoutItem lastItem = _items.Last();
      float x = lastItem.Position.x + lastItem.Size.x + _pooledScrollViewModel.RightPanelPadding;
      float y = lastItem.Position.y + lastItem.Size.y + _pooledScrollViewModel.BottomPanelPadding;
      return new Vector2(x, y);
    }

    private Vector2 GetPanelInitialSize()
    {
      Vector2 initialSize = new(_pooledScrollViewModel.InitialWidth,
                                _pooledScrollViewModel.InitialHeight);
      if (initialSize.x < MIN_PANEL_SIZE) {
        _logger.Warn(
          $"Panel width lower than {nameof(MIN_PANEL_SIZE)} in PooledScrollViewModel. initialWidth={initialSize.x}");
        initialSize.x = MIN_PANEL_SIZE;
      }
      if (initialSize.y < MIN_PANEL_SIZE) {
        _logger.Warn(
          $"Panel height lower than {nameof(MIN_PANEL_SIZE)} in PooledScrollViewModel. initialHeight={initialSize.y}");
        initialSize.y = MIN_PANEL_SIZE;
      }
      return initialSize;
    }
  }
}

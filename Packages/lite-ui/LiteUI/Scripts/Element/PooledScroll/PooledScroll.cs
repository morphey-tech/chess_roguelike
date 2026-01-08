using System.Collections.Generic;
using System.Linq;
using LiteUI.Common.Logger;
using LiteUI.Binding.Attributes;
using LiteUI.Common.Extensions;
using LiteUI.Element.PooledScroll.Model;
using LiteUI.UI.Service;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace LiteUI.Element.PooledScroll
{
  [UIController(nameof(PooledScroll))]
  public class PooledScroll : MonoBehaviour
  {
    private static readonly IUILogger _logger = LoggerFactory.GetLogger<PooledScroll>();

    [UIComponentBinding]
    private PooledScrollViewModel _pooledScrollViewModel = null!;
    [UIComponentBinding]
    private ScrollRect _scrollRect = null!;

    [Inject]
    private UIService _uiService = null!;

    private PooledScrollPanel _pooledScrollPanel = null!;
    private PooledScrollCacheManager _pooledScrollCacheManager = null!;

    private PooledScrollPanelLayout _pooledScrollLayout = null!;

    [UICreated]
    private void OnInit()
    {
      _pooledScrollCacheManager =
          _uiService.AttachController<PooledScrollCacheManager>(_scrollRect.content.gameObject);
      _pooledScrollLayout =
          new PooledScrollPanelLayout(_pooledScrollViewModel, _pooledScrollCacheManager);
      _pooledScrollPanel = _uiService.AttachController<PooledScrollPanel>(
        _scrollRect.content.gameObject, _pooledScrollLayout, _pooledScrollCacheManager,
        _pooledScrollViewModel, _scrollRect);

      _pooledScrollCacheManager.OnLoadComplete += () => {
        if (this.IsDestroyed()) {
          return;
        }

        _pooledScrollLayout.UpdateSizesToInitial();
      };
    }

    public void Register<TVm, TP>(params object?[]? initParams)
        where TVm : IPooledScrollItemViewModel
        where TP : PooledScrollItemPanel
    {
      Register<TVm, TP>(initParams?.ToList());
    }

    public void Register<TVm, TP>(List<object?>? initParams = null)
        where TVm : IPooledScrollItemViewModel
        where TP : PooledScrollItemPanel
    {
      _pooledScrollCacheManager.Register(typeof(TVm), typeof(TP), initParams);
    }

    public void UpdateItems<TVm>(List<TVm> items)
        where TVm : IPooledScrollItemViewModel
    {
      _pooledScrollLayout.UpdateItems(items);
    }

    public void RefreshItem<TVm>(TVm item)
        where TVm : IPooledScrollItemViewModel
    {
      _pooledScrollLayout.RefreshItem(item);
    }

    public void RefreshAllItems()
    {
      _pooledScrollLayout.RefreshAllItems();
    }

    public void AddItem<TVm>(TVm item)
        where TVm : IPooledScrollItemViewModel
    {
      _pooledScrollLayout.AddItem(item);
    }

    public void RemoveItem<TVm>(TVm item)
        where TVm : IPooledScrollItemViewModel
    {
      _pooledScrollLayout.RemoveItem(item);
    }

    public void ReplaceItem<TVm>(int index, TVm item)
        where TVm : IPooledScrollItemViewModel
    {
      _pooledScrollLayout.ReplaceItem(index, item);
    }

    public void ScrollTop()
    {
      IPooledScrollPanelLayoutItem? item = _pooledScrollLayout.FirstItem();
      if (item == null) {
        return;
      }
      _pooledScrollPanel.ScrollTo(item);
    }

    public void ScrollBottom()
    {
      IPooledScrollPanelLayoutItem? item = _pooledScrollLayout.LastItem();
      if (item == null) {
        return;
      }
      _pooledScrollPanel.ScrollTo(item);
    }

    public void ScrollToItem<TVm>(TVm item)
        where TVm : IPooledScrollItemViewModel
    {
      IPooledScrollPanelLayoutItem? layoutItem = _pooledScrollLayout.FindItem(item);
      if (layoutItem == null) {
        _logger.Warn($"No item with model found to scroll. ItemId={item.Id}");
        return;
      }
      _pooledScrollPanel.ScrollTo(layoutItem);
    }
  }
}

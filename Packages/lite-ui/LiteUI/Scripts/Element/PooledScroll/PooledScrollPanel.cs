using System.Collections.Generic;
using LiteUI.Binding.Attributes;
using LiteUI.Common.Extensions;
using LiteUI.Common.Utils;
using LiteUI.Element.PooledScroll.Model;
using UnityEngine;
using UnityEngine.UI;

namespace LiteUI.Element.PooledScroll
{
  [UIController(nameof(PooledScrollPanel))]
  public class PooledScrollPanel : MonoBehaviour
  {
    private PooledScrollCacheManager _pooledScrollCacheManager = null!;
    private PooledScrollViewModel _pooledScrollViewModel = null!;
    private PooledScrollPanelLayout _layout = null!;
    private ScrollRect _scrollRect = null!;

    private RectTransform _panelTransform = null!;
    private RectTransform? _viewPort;

    private readonly Dictionary<IPooledScrollPanelLayoutItem, PooledScrollItemPanel>
        _panels = new();

    private Vector2 _prevPositions = Vector2.zero;
    private Vector2 _prevViewportSize = Vector2.zero;
    private float _prevScrollPosition;
    private IPooledScrollPanelLayoutItem? _scrollTarget;

    [UICreated]
    private void OnInit(PooledScrollPanelLayout layout,
                        PooledScrollCacheManager pooledScrollCacheManager,
                        PooledScrollViewModel pooledScrollViewModel, ScrollRect scrollRect)
    {
      _layout = layout;
      _pooledScrollCacheManager = pooledScrollCacheManager;
      _pooledScrollViewModel = pooledScrollViewModel;
      _scrollRect = scrollRect;

      _panelTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
      if (_pooledScrollCacheManager.Loading) {
        return;
      }

      float scrollPosition = ScrollPosition;
      if (!MathUtils.IsFloatEquals(scrollPosition, _prevScrollPosition)) {
        _scrollTarget = null;
      }

      UpdateLayoutPanelSizes();

      Vector2 currentPosition = _panelTransform.anchoredPosition;
      Vector2 currentViewportSize = ViewPortSize;

      if (currentPosition.FuzzyEquals(_prevPositions) &&
          _prevViewportSize.FuzzyEquals(currentViewportSize) && !_layout.Dirty) {
        return;
      }

      List<IPooledScrollPanelLayoutItem> visibleItems =
          _layout.ReceiveVisibleItems(currentPosition, currentViewportSize);

      FreeOldPanels(visibleItems);
      UpdateCurrentPanels();
      AddNewPanels(visibleItems);

      if (_scrollTarget != null) {
        scrollPosition = _layout.GetScrollNormalizedPositionForItem(_scrollTarget, ViewPortSize);
        ScrollPosition = scrollPosition;
      }

      _prevPositions = currentPosition;
      _prevViewportSize = currentViewportSize;
      _prevScrollPosition = scrollPosition;
      _layout.ClearDirty();
    }

    public void ScrollTo(IPooledScrollPanelLayoutItem scrollTarget)
    {
      UpdateLayoutPanelSizes();

      _scrollTarget = scrollTarget;
      float normalizedPosition =
          _layout.GetScrollNormalizedPositionForItem(scrollTarget, ViewPortSize);
      _prevScrollPosition = normalizedPosition;
      ScrollPosition = normalizedPosition;
    }

    private void UpdateLayoutPanelSizes()
    {
      if (_pooledScrollViewModel.DynamicItemSize) {
        foreach (KeyValuePair<IPooledScrollPanelLayoutItem, PooledScrollItemPanel> pair in
                 _panels) {
          _layout.UpdateSize(pair.Key, pair.Value.PanelSize);
        }
      }

      _layout.RecalculateLayout();
      ContentSize = _layout.GetContentSize();
    }

    private void FreeOldPanels(ICollection<IPooledScrollPanelLayoutItem> visibleItems)
    {
      List<IPooledScrollPanelLayoutItem>? panelsToRemove = null;
      foreach (KeyValuePair<IPooledScrollPanelLayoutItem, PooledScrollItemPanel> pair in _panels) {
        if (visibleItems.Contains(pair.Key) && !pair.Key.NeedChangePanelType) {
          continue;
        }
        panelsToRemove ??= new List<IPooledScrollPanelLayoutItem>();
        panelsToRemove.Add(pair.Key);
        _pooledScrollCacheManager.FreeItem(pair.Value);
      }
      panelsToRemove?.ForEach(p => _panels.Remove(p));
    }

    private void UpdateCurrentPanels()
    {
      // все не нужные айтемы уже должны быть удалены, в текущих панелях только актуальные 
      foreach (KeyValuePair<IPooledScrollPanelLayoutItem, PooledScrollItemPanel> pair in _panels) {
        PooledScrollItemPanel panel = pair.Value;
        IPooledScrollPanelLayoutItem panelModel = pair.Key;

        panel.Configure(panelModel.ItemViewModel);
        if (panelModel.NeedReinitialize) {
          panel.Reinitialize();
        }
        if (panelModel.NeedRefresh) {
          panel.Refresh();
        }
        panel.Position = new Vector2(panelModel.Position.x, -panelModel.Position.y);
      }
    }

    private void AddNewPanels(List<IPooledScrollPanelLayoutItem> visibleItems)
    {
      foreach (IPooledScrollPanelLayoutItem panelModel in visibleItems) {
        if (_panels.ContainsKey(panelModel)) {
          continue;
        }
        PooledScrollItemPanel? panel =
            _pooledScrollCacheManager.GetOrCreateItem(panelModel.ItemViewModel.GetType());
        _layout.RefreshItem(panelModel.ItemViewModel);
        if (panel == null) {
          continue;
        }
        _panels[panelModel] = panel;
        panel.Configure(panelModel.ItemViewModel);
        panel.Reinitialize();
        panel.Refresh();
        panel.Position = new Vector2(panelModel.Position.x, -panelModel.Position.y);
      }
    }

    private Vector2 ViewPortSize
    {
      get
      {
        if (_viewPort == null) {
          _viewPort = transform.parent.GetComponent<RectTransform>();
        }
        return _viewPort.rect.size;
      }
    }

    private Vector2 ContentSize
    {
      set
      {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = _pooledScrollViewModel.Vertical 
          ? new Vector2(rectTransform.sizeDelta.x, value.y) 
          : new Vector2(value.x, rectTransform.sizeDelta.y);
      }
    }

    private float ScrollPosition
    {
      get => _pooledScrollViewModel.Vertical ? _scrollRect.verticalNormalizedPosition
                 : _scrollRect.horizontalNormalizedPosition;
      set
      {
        if (_pooledScrollViewModel.Vertical) {
          _scrollRect.verticalNormalizedPosition = value;
        }
        else {
          _scrollRect.horizontalNormalizedPosition = value;
        }
      }
    }
  }
}

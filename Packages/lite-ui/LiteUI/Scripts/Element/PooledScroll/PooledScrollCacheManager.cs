using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteUI.Common.Logger;
using Cysharp.Threading.Tasks;
using LiteUI.Addressable.Service;
using LiteUI.Binding.Attributes;
using LiteUI.UI.Service;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace LiteUI.Element.PooledScroll
{
  [UIController(nameof(PooledScrollCacheManager))]
  public class PooledScrollCacheManager : MonoBehaviour
  {
    private static readonly IUILogger _logger =
        LoggerFactory.GetLogger<PooledScrollCacheManager>();

    private UIService _uiService = null!;
    private AddressableManager _addressableManager = null!;
    private IObjectResolver _resolver = null!;

    public event Action? OnLoadComplete;

    private readonly Dictionary<Type, Type> _panelTypes = new();
    private readonly Dictionary<Type, GameObject?> _panelPrefabs = new();
    private readonly Dictionary<Type, List<object?>?> _initParams = new();
    private readonly Dictionary<Type, List<PooledScrollItemPanel>> _cachedItems = new();

    public bool Loading { get; private set; }

    [Inject]
    private void Construct(UIService uiService, AddressableManager addressableManager,
                           IObjectResolver resolver)
    {
      _uiService = uiService;
      _addressableManager = addressableManager;
      _resolver = resolver;
    }

    private void OnDestroy()
    {
      foreach (GameObject? prefab in _panelPrefabs.Values) {
        if (prefab != null) {
          _addressableManager.Release(prefab);
        }
      }
    }

    public void Register(Type viewModelType, Type panelType, List<object?>? initParams)
    {
      if (_panelTypes.ContainsKey(viewModelType)) {
        _logger.Warn($"Pooled viewModelType already registered, class={viewModelType.Name}");
        return;
      }
      if (_panelTypes.ContainsValue(panelType)) {
        _logger.Warn($"Pooled panelType already registered, class={viewModelType.Name}");
        return;
      }
      _panelTypes[viewModelType] = panelType;
      _initParams[panelType] = initParams;
      _cachedItems[panelType] = new List<PooledScrollItemPanel>();

      LoadPrefabAsync(panelType).Forget();
    }

    // null - скорее исключение
    public PooledScrollItemPanel? GetOrCreateItem(Type viewModelType)
    {
      if (Loading) {
        _logger.Warn("Try to create items while panel loaded");
      }
      if (!_panelTypes.ContainsKey(viewModelType)) {
        _logger.Warn($"Unsupported viewModelType. Type={viewModelType}");
        return null;
      }
      Type panelType = _panelTypes[viewModelType];
      if (!_panelPrefabs.ContainsKey(panelType) || _panelPrefabs[panelType] == null) {
        _logger.Warn($"No panel prefab for viewModelType. Type={viewModelType}");
        return null;
      }

      List<PooledScrollItemPanel> cachedTypeItems = _cachedItems[panelType];
      if (cachedTypeItems.Count != 0) {
        PooledScrollItemPanel cachedPanel = cachedTypeItems[^1];
        cachedTypeItems.RemoveAt(cachedTypeItems.Count - 1);
        cachedPanel.gameObject.SetActive(true);
        return cachedPanel;
      }

      GameObject panelPrefab = _panelPrefabs[panelType]!;
      List<object?>? initParams = _initParams[panelType];
      GameObject panelObject = Instantiate(panelPrefab, transform);
      panelObject.name = panelObject.name.Replace("(Clone)", "");
      _resolver.InjectGameObject(panelObject);

      if (initParams == null) {
        return (PooledScrollItemPanel)_uiService.AttachController(panelType, panelObject);
      }
      return (PooledScrollItemPanel)_uiService.AttachController(
        panelType, panelObject, initParams.ToArray());
    }

    public void FreeItem(PooledScrollItemPanel panel)
    {
      panel.gameObject.SetActive(false);
      Type panelType = panel.GetType();
      _cachedItems[panelType].Add(panel);
    }

    public Vector2 GetInitialSize(Type viewModelType)
    {
      if (!_panelTypes.ContainsKey(viewModelType)) {
        _logger.Warn($"Unsupported viewModelType. Type={viewModelType}");
        return Vector2.zero;
      }
      Type panelType = _panelTypes[viewModelType];
      if (!_panelPrefabs.ContainsKey(panelType) || _panelPrefabs[panelType] == null) {
        _logger.Warn($"No panel prefab for viewModelType. Type={viewModelType}");
        return Vector2.zero;
      }
      GameObject panelPrefab = _panelPrefabs[panelType]!;
      RectTransform rectTransform = panelPrefab.GetComponent<RectTransform>();
      return rectTransform.sizeDelta;
    }

    private async UniTaskVoid LoadPrefabAsync(Type panelType)
    {
      if (_panelPrefabs.ContainsKey(panelType)) {
        return;
      }

      UIControllerAttribute? uiControllerAttribute =
          (UIControllerAttribute?)panelType.GetCustomAttribute(typeof(UIControllerAttribute));
      if (uiControllerAttribute == null) {
        _logger.Warn($"Incorrect panel type. MustBe UIController, class={panelType.Name}");
        return;
      }
      string panelPrefabName = uiControllerAttribute.Id;

      Loading = true;
      _panelPrefabs[panelType] = null;
      try {
        _panelPrefabs[panelType] =
            await _addressableManager.LoadAssetAsync<GameObject>(panelPrefabName);
      }
      catch (Exception) {
        _panelPrefabs.Remove(panelType);
        throw;
      } finally {
        Loading = HasLoadingPanels();
      }
      if (!Loading) {
        OnLoadComplete?.Invoke();
      }
    }

    private bool HasLoadingPanels()
    {
      return _panelPrefabs.Any(p => p.Value == null);
    }
  }
}

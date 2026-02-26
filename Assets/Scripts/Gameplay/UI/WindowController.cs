
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Logging;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Project.Core.Window
{
  public class WindowsController : MonoBehaviour, IWindowsController
  {
    public event Action<Window> OnCreateWindow;

    [SerializeField] private GameObject _windowContainer = null;

    [Space] [Header("Canvas:")] [SerializeField]
    private Canvas _canvas = null;

    [SerializeField] private CanvasGroup _canvasGroup = null;
    [SerializeField] private float _planeDistance = 1f;
    [SerializeField] private int _sortingOrder = 10;
    
    public IReadOnlyList<Window> GetVisibleStack() => _visibleWindows;
    public IReadOnlyCollection<Window> GetAllWindows() => _windows.Values;

    private List<Window> _visibleWindows = new List<Window>();
    private readonly Dictionary<Type, Window> _windows = new Dictionary<Type, Window>();
    private readonly List<Window> _hidingWindows = new List<Window>();
    private readonly LinkedList<Action> _windowQueue = new LinkedList<Action>();

    public Canvas Canvas => _canvas;

    public CanvasGroup CanvasGroup => _canvasGroup;

    private Core.Logging.ILogger<WindowsController> _log;
    private IAssetService _assetService;
    private ILogService _logService;

    public async UniTask InitAsync(IAssetService assetService, ILogService logService)
    {
      _assetService = assetService;
      _logService = logService;
      _log = _logService.CreateLogger<WindowsController>();
      SetCanvasSettings();
    }

    private void SetCanvasSettings()
    {
      _canvas.planeDistance = _planeDistance;
      _canvas.sortingOrder = _sortingOrder;
    }

    private void TryCloseActiveWindow()
    {
      var window = GetTopmostWindow();

      if (window == null)
        return;

      if (window.CanBeClosedByBackground && window.NeedShowBackground)
        window.Close();
    }

    public T ShowWindow<T>(bool instant = false) where T : ParameterlessWindow
    {
      var wnd = GetOrCreateWindow<T>();
      wnd.Show(instant);
      return wnd;
    }

    public async UniTask<T> ShowWindowAsync<T>() where T : ParameterlessWindow
    {
      var wnd = (T)(await GetOrCreateWindowAsync(typeof(T)));
      wnd.Show();
      return wnd;
    }

    public async UniTask<T> ShowWindowAsync<T, A1>(A1 a1) where T : ParameterWindow<A1>
    {
      var wnd = (T)(await GetOrCreateWindowAsync(typeof(T)));
      wnd.Show(a1);
      return wnd;
    }

    public T ShowWindow<T, A1>(A1 a1, bool immediate = false) where T : ParameterWindow<A1>
    {
      var wnd = GetOrCreateWindow<T>();
      wnd.Show(a1, immediate);

      return wnd;
    }

    public T ShowWindow<T, A1, A2>(A1 a1, A2 a2, bool immediate = false) where T : ParameterWindow<A1, A2>
    {
      var wnd = GetOrCreateWindow<T>();
      wnd.Show(a1, a2, immediate);
      return wnd;
    }

    public T ShowWindow<T, A1, A2, A3>(A1 a1, A2 a2, A3 a3, bool immediate = false)
      where T : ParameterWindow<A1, A2, A3>
    {
      var wnd = GetOrCreateWindow<T>();
      wnd.Show(a1, a2, a3, immediate);
      return wnd;
    }

    public bool IsWindowVisible<T>() where T : Window
    {
      return _visibleWindows.Find(x => x.GetType() == typeof(T)) != null;
    }

    public bool IsWindowVisible(Type type)
    {
      return _visibleWindows.Find(x => x.GetType() == type) != null;
    }

    public bool HasAnyVisible()
    {
      return _visibleWindows.Count > 0;
    }

    private void SortWindows()
    {
      if (IsUnsorted(_visibleWindows))
      {
        _visibleWindows = _visibleWindows.OrderByDescending(w => w.ZOrder).ToList();
      }

      for (var i = 0; i < _visibleWindows.Count; i++)
      {
        _visibleWindows[i].transform.SetSiblingIndex(i + 1);
      }
    }

    private static bool IsUnsorted(IList<Window> windows_list)
    {
      var prev_z = int.MinValue;
      for (var index = 0; index < windows_list.Count; index++)
      {
        var current_z = windows_list[index].ZOrder;
        if (current_z > prev_z)
          return true;
        prev_z = current_z;
      }

      return false;
    }

    private void OnWindowShow(Window wnd)
    {
      if (!_visibleWindows.Contains(wnd))
        _visibleWindows.Add(wnd);

      // SortWindows();
      _log.Debug($"window show ID={wnd}, sibling_index={wnd.transform.GetSiblingIndex()}");
      UpdateBackground(FindTopmostWindowWithBackground());
    }

    private void OnWindowHide(Window wnd)
    {
      _visibleWindows.Remove(wnd);
      _hidingWindows.Add(wnd);

      wnd.OnShow += window => _hidingWindows.Remove(wnd); //для тех случаев, когда это же окно будет вновь открыто
      wnd.OnAfterClose += window => _hidingWindows.Remove(wnd);

      _log.Debug($"window hide ID={wnd}, window_stack= {ReportWindowStack()}");

      UpdateBackground(FindTopmostWindowWithBackground());
    }

    private void OnWindowClose(Window wnd)
    {
      _windows.Remove(wnd.GetType());
      _visibleWindows.Remove(wnd);
      UpdateBackground(FindTopmostWindowWithBackground());
    }

    private string ReportWindowStack()
    {
      var stack_report = "";
      foreach (var wnd in _visibleWindows)
      {
        if (stack_report != "")
          stack_report += ", ";
        stack_report += wnd;
      }

      return stack_report == "" ? "EMPTY" : stack_report;
    }

    private Window GetTopmostWindow()
    {
      if (_visibleWindows.Count == 0)
        return null;
      return Enumerable.Last(_visibleWindows);
    }

    private Window FindTopmostWindowWithBackground()
    {
      for (var i = _visibleWindows.Count - 1; i >= 0; i--)
      {
        Window window = _visibleWindows[i];

        if (window.CanvasInvisible)
          continue;

        if (window.NeedShowBackground)
          return window;
      }

      return null;
    }

    public string GetTopmostWindowName()
    {
      return GetTopmostWindow() != null ? GetTopmostWindow().name : null;
    }

    private void UpdateBackground(Window wnd)
    {
      if (wnd == null && _windowQueue.Count > 0)
        return;

      if (wnd == null || !wnd.NeedShowBackground) 
        return;
    }



    public T GetOrCreateWindow<T>() where T : Window
    {
      return (T)GetOrCreateWindow(typeof(T));
    }

    public Window GetWindow(Type type)
    {
      return _windows.GetValueOrDefault(type);
    }

    public Window GetOrCreateWindow(Type windowType)
    {
      if (_windows.TryGetValue(windowType, out Window? target))
      {
        return target;
      }

      // Sync создание без preload не поддерживается
      _log.Error($"Sync GetOrCreateWindow not supported for {windowType.Name}. Use async version.");
      return null;
    }

    public async UniTask<T> GetOrCreateWindowAsync<T>() where T : Window
    {
      return (T)(await GetOrCreateWindowAsync(typeof(T)));
    }

    public async UniTask<Window> GetOrCreateWindowAsync(Type windowType)
    {
      Window window;

      if (_windows.TryGetValue(windowType, out Window? existed))
      {
        window = existed;
      }
      else
      {
        window = await CreateWindowAsync(windowType);
        CreateWindowPostprocess(window);
      }

      if (window == null)
      {
        throw new Exception($"Failed to create window of type {windowType}");
      }

      return window;
    }

    private void CreateWindowPostprocess(Window window)
    {
      window.OnShow += OnWindowShow;
      window.OnHide += OnWindowHide;
      window.OnClose += OnWindowClose;
      window.OnAfterClose += OnAfterWindowClose;
      window.gameObject.SetActive(false);

      AssignClickFeedback<Button>(window.gameObject);
    }

    private static void AssignClickFeedback<T>(GameObject go) where T : Component
    {
      /*using var buttons = go.GetComponentsRecursive<T>();
      foreach(var button in buttons)
      {
        if(button.GetComponent<UIClickHaptics>())
          continue;

        var uich = button.gameObject.AddComponent<UIClickHaptics>();
        uich.ClickFeedback = HapticPatterns.PresetType.Hit;
      }*/
    }

    private void OnAfterWindowClose(Window wnd)
    {
      if (_windowQueue.Count == 0 || GetTopmostWindow() != null)
        return;

      var show_next_window = _windowQueue.First.Value;
      _windowQueue.RemoveFirst();
      show_next_window();
    }

    private Window CreateWindow(Type windowType)
    {
      _log.Error($"Sync CreateWindow is not supported without preload. Use GetOrCreateWindowAsync for {windowType.Name}.");
      throw new Exception($"Sync window creation not supported for {windowType.Name}");
    }

    private async UniTask<Window> CreateWindowAsync(Type windowType)
    {
      try
      {
        var window =
          (await _assetService.InstantiateAsync(windowType.FullName, Vector3.zero, Quaternion.identity,
            _windowContainer.transform)).GetComponent<Window>();
        var id = windowType;
        _windows.Add(id, window);
        
        //хз почему оно просто не спавнится так как надо
        window.GetComponent<RectTransform>().anchoredPosition = window.transform.parent.GetComponent<RectTransform>().anchoredPosition;
        window.GetComponent<RectTransform>().sizeDelta = window.transform.parent.GetComponent<RectTransform>().sizeDelta;
        window.name = id.Name;
        window.Init(_assetService, _logService);

        OnCreateWindow?.Invoke(window);

        return window;
      }
      catch (Exception e)
      {
        _log.Error($"Error happens:\n{e}");
        throw;
      }
    }

    public void CloseVisibleStack()
    {
      Window[] to_close = new Window[_visibleWindows.Count];
      _visibleWindows.CopyTo(to_close);

      foreach (var wnd in to_close)
      {
        wnd.OnShow -= OnWindowShow;
        wnd.OnHide -= OnWindowHide;
        wnd.OnClose -= OnWindowClose;
        wnd.OnAfterClose -= OnAfterWindowClose;

        wnd.Close();
      }
    }

    public void HideVisibleStack()
    {
      Window[] to_hide = new Window[_visibleWindows.Count];
      _visibleWindows.CopyTo(to_hide);

      foreach (var wnd in to_hide)
      {
        wnd.Hide();
      }
    }

    public T GetWindow<T>() where T : Window
    {
      var window = GetOr(_windows, typeof(T));
      return window as T;
    }

    private static V GetOr<K, V>(IDictionary<K, V> self, K key, V default_value = default(V))
    {
      V val;
      return self.TryGetValue(key, out val) ? val : default_value;
    }

    public IEnumerator ShowWithDelay<T>(float delay = .2f) where T : ParameterlessWindow
    {
      var window = GetOrCreateWindow<T>();
      yield return new WaitForSeconds(delay);
      window.Show();
    }

    public void CloseWindow<T>() where T : Window
    {
      var window = GetWindow<T>();
      if (window == null)
        return;

      window.Close();
    }

    public void HideWindow<T>() where T : Window
    {
      var window = GetWindow<T>();
      if (window == null)
        return;

      window.Hide();
    }

    public IEnumerator WaitForWindowClose<T>() where T : Window
    {
      var window = GetWindow<T>();
      if (window == null)
        yield break;

      var is_closed = false;
      window.OnAfterClose += w => is_closed = true;
      yield return new WaitUntil(() => is_closed);
    }

    public IEnumerator CloseWindowAndWait<T>() where T : Window
    {
      var window = GetWindow<T>();
      if (window == null)
        yield break;

      window.Close();
      yield return WaitForWindowClose<T>();
    }

    public IEnumerator WaitForWindow<T>() where T : Window
    {
      while (GetWindow<T>() == null)
        yield return null;
    }

    public IEnumerator WaitForWindowThenWaitForClose<T>() where T : Window
    {
      yield return WaitForWindow<T>();

      yield return WaitForWindowClose<T>();
    }

    public IEnumerator WaitForWindowForTimeThenWaitForClose<T>(float time_seconds = 2) where T : Window
    {
      yield return WaitForWindowForTime<T>(time_seconds);

      yield return WaitForWindowClose<T>();
    }

    public IEnumerator WaitForWindowForTime<T>(float time_seconds = 2) where T : Window
    {
      var time = 0f;
      while (time < time_seconds && GetWindow<T>() == null)
      {
        time += Time.deltaTime;
        yield return null;
      }
    }

    public void ShowAfter<T>() where T : ParameterlessWindow
    {
      if (HasAnyVisible())
      {
        _windowQueue.AddLast(() => ShowWindow<T>());
        return;
      }

      ShowWindow<T>();
    }

    public void ShowAfter<T>(Action<T> action) where T : ParameterlessWindow
    {
      _windowQueue.AddLast(() =>
      {
        Debug.LogError("ShowAfter invoke");
        var window = GetOrCreateWindow<T>();
        action(window);
        window.Show();
      });
    }

    public void ShowAfter<T, A1>(A1 a1) where T : ParameterWindow<A1>
    {
      if (HasAnyVisible())
      {
        _windowQueue.AddLast(() => ShowWindow<T, A1>(a1));
        return;
      }

      ShowWindow<T, A1>(a1);
    }

    public void ShowAfter<T, A1, A2>(A1 a1, A2 a2) where T : ParameterWindow<A1, A2>
    {
      if (HasAnyVisible())
      {
        _windowQueue.AddLast(() => ShowWindow<T, A1, A2>(a1, a2));
        return;
      }

      ShowWindow<T, A1, A2>(a1, a2);
    }

    public void ShowAfter<T, A1, A2>(A1 a1, A2 a2, Action<Window> on_close) where T : ParameterWindow<A1, A2>
    {
      Action show_window = () =>
      {
        var window = ShowWindow<T, A1, A2>(a1, a2);
        window.OnAfterClose += on_close;
      };

      if (HasAnyVisible())
      {
        _windowQueue.AddLast(show_window);
        return;
      }

      show_window();
    }

    public bool ThereIsNoHidingWindows()
    {
      return _hidingWindows.Count == 0;
    }

    public bool HasAnyOf(List<Type> check_types)
    {
      foreach (var window in _visibleWindows)
      {
        var index = check_types.IndexOf(window.GetType());
        if (index > -1)
          return true;
      }

      return false;
    }

    // Query visible windows by interface type
    public bool HasVisibleByInterface<TInterface>() where TInterface : class
    {
      for (int i = 0; i < _visibleWindows.Count; i++)
      {
        if (_visibleWindows[i] is TInterface)
          return true;
      }

      return false;
    }

    public TInterface GetTopVisibleByInterface<TInterface>() where TInterface : class
    {
      TInterface top = null;
      int topZ = int.MinValue;
      for (int i = 0; i < _visibleWindows.Count; i++)
      {
        var w = _visibleWindows[i];
        if (w is TInterface asIface && w.ZOrder >= topZ)
        {
          topZ = w.ZOrder;
          top = asIface;
        }
      }

      return top;
    }

    public IReadOnlyList<TInterface> GetVisibleByInterface<TInterface>() where TInterface : class
    {
      var list = new List<TInterface>();
      foreach (var window in _visibleWindows)
      {
        if (window is TInterface asIface)
          list.Add(asIface);
      }

      return list;
    }
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Core.Window;
using UnityEngine;

namespace Project.Gameplay.Gameplay.UI
{
  public interface IWindowsController
  {
    Canvas Canvas { get; }
    CanvasGroup CanvasGroup { get; }
    UniTask InitAsync(IUIAssetService uiAssetService, ILogService logService);
    T ShowWindow<T>(bool instant = false) where T : ParameterlessWindow;
    T ShowWindow<T, A1>(A1 a1, bool immediate = false) where T : ParameterWindow<A1>;
    T ShowWindow<T, A1, A2>(A1 a1, A2 a2, bool immediate = false) where T : ParameterWindow<A1, A2>;
    T ShowWindow<T, A1, A2, A3>(A1 a1, A2 a2, A3 a3, bool immediate = false) where T : ParameterWindow<A1, A2, A3>;
    void CloseWindow<T>() where T : Window;
    void HideWindow<T>() where T : Window;
    IReadOnlyList<Window> GetVisibleStack();
    IReadOnlyCollection<Window> GetAllWindows();
    void HideVisibleStack();
    Coroutine StartCoroutine(IEnumerator coroutine);
    T GetOrCreateWindow<T>() where T : Window;
    Window GetOrCreateWindow(Type windowType);
    T GetWindow<T>() where T : Window;
    Window GetWindow(Type type);
    bool IsWindowVisible<T>() where T : Window;
    bool IsWindowVisible(Type type);
    // Query visible windows by interface type
    bool HasVisibleByInterface<TInterface>() where TInterface : class;
    TInterface GetTopVisibleByInterface<TInterface>() where TInterface : class;
    IReadOnlyList<TInterface> GetVisibleByInterface<TInterface>() where TInterface : class;
    /*
    UniTask PreloadWindowsAsync(EnumWindowPreloadGroup group);
    void ReleasePreloadedWindows(EnumWindowPreloadGroup group);
    */

    UniTask<T> ShowWindowAsync<T>() where T : ParameterlessWindow;
    UniTask<T> ShowWindowAsync<T, A1>(A1 a1) where T : ParameterWindow<A1>;
    UniTask<T> GetOrCreateWindowAsync<T>() where T : Window;
    UniTask<Window> GetOrCreateWindowAsync(Type windowType);
  }
}

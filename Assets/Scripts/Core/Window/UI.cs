#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.Core.Window
{

public class UI
{
  public static Canvas Canvas => GetController().Canvas;

  public static CanvasGroup CanvasGroup => GetController().CanvasGroup;

  private static IWindowsController? _controller;
  public static bool IsValid => IsControllerValid();

  private IAssetService _assetService;
  
  public static async UniTask InitAsync(IAssetService assetService, ILogService logService, IWindowsController? controller)
  {
    await LoadControllerAsync(assetService, logService, controller);
  }

  public static async UniTask InitAsync(IAssetService assetService)
  {
    await InitAsync(assetService);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IWindowsController GetController() => IsValid ? _controller! : throw new Exception("UI is not valid");
  private static async UniTask LoadControllerAsync(IAssetService assetService, ILogService logService, IWindowsController? controller)
  {
    if (controller != null)
      _controller = controller;

    if (!IsControllerValid())
    {
      var instance = Object.Instantiate(await assetService.LoadAssetAsync<WindowsController>("Main/Windows"));
      Object.DontDestroyOnLoad(instance);
      _controller = instance;
    }

    await _controller!.InitAsync(assetService, logService);
  }

  private static bool IsControllerValid() => (_controller as Component) != null;

  public static IReadOnlyList<Window> GetVisibleStack() => GetController().GetVisibleStack();
  public static IReadOnlyCollection<Window> GetAllWindows() => GetController().GetAllWindows();
  public static void HideVisibleStack() => GetController().HideVisibleStack();
  public static int VisibleCount() => GetController().GetVisibleStack().Count;

  public static Coroutine? StartCoroutine(IEnumerator coroutine)
  {
    return !IsValid ? null : _controller.StartCoroutine(coroutine);

  }

  public static T GetOrCreate<T>() where T : Window
  {
    return GetController().GetOrCreateWindow<T>();
  }

  public static Window GetOrCreate(Type type)
  {
    return GetController().GetOrCreateWindow(type);
  }

  public static Window GetOrCreate(string typeName)
  {
    Assembly asm = typeof (Window).Assembly;
    Type type = asm.GetType(typeName);
    return GetController().GetOrCreateWindow(type);
  }

  public static T Show<T>(bool immediate = false) where T : ParameterlessWindow
  {
    return GetController().ShowWindow<T>(immediate);
  }

  public static T Show<T, A1>(A1 a1, bool immediate = false) where T : ParameterWindow<A1>
  {
    return GetController().ShowWindow<T, A1>(a1, immediate);
  }

  public static T Show<T, A1, A2>(A1 a1, A2 a2, bool immediate = false) where T : ParameterWindow<A1, A2>
  {
    return GetController().ShowWindow<T, A1, A2>(a1, a2, immediate);
  }

  public static T Show<T, A1, A2, A3>(A1 a1, A2 a2, A3 a3, bool immediate = false) where T : ParameterWindow<A1, A2, A3>
  {
    return GetController().ShowWindow<T, A1, A2, A3>(a1, a2, a3, immediate);
  }

  public static void Close<T>() where T : Window
  {
    if(!IsValid)
      return;

    GetController().CloseWindow<T>();
  }

  public static void Hide<T>() where T : Window
  {
    GetController().HideWindow<T>();
  }

  public static T FindWindow<T>() where T : Window
  {
    return GetController().GetWindow<T>();
  }

  public static bool TryFindWindow<T>(out T window) where T : Window
  {
    window = GetController().GetWindow<T>();
    return window != null;
  }

  public static Window FindWindow(Type type)
  {
    return GetController().GetWindow(type);
  }

  public static bool IsVisible<T>() where T : Window
  {
    return GetController().IsWindowVisible<T>();
  }

  public static bool IsVisible(Type type)
  {
    return GetController().IsWindowVisible(type);
  }

  // Interface-based queries
  public static bool HasVisibleByInterface<TInterface>() where TInterface : class
  {
    return GetController().HasVisibleByInterface<TInterface>();
  }

  public static TInterface GetTopVisibleByInterface<TInterface>() where TInterface : class
  {
    return GetController().GetTopVisibleByInterface<TInterface>();
  }

  public static IReadOnlyList<TInterface> GetVisibleByInterface<TInterface>() where TInterface : class
  {
    return GetController().GetVisibleByInterface<TInterface>();
  }
  
  /*public static async UniTask PreloadWindowsAsync(EnumWindowPreloadGroup group)
  {
    await GetController().PreloadWindowsAsync(group);
  }

  public static void ReleasePreloadedWindows(EnumWindowPreloadGroup group)
  {
    GetController().ReleasePreloadedWindows(group);
  }*/

  public static async UniTask<T> ShowAsync<T>() where T : ParameterlessWindow
  {
    return await GetController().ShowWindowAsync<T>();
  }

  public static async UniTask<T> ShowAsync<T, A1>(A1 a1) where T : ParameterWindow<A1>
  {
    return await GetController().ShowWindowAsync<T, A1>(a1);
  }

  public static async UniTask<T> GetOrCreateAsync<T>() where T : Window
  {
    return await GetController().GetOrCreateWindowAsync<T>();
  }

  public static async UniTask<Window> GetOrCreateAsync(Type windowType)
  {
    return await GetController().GetOrCreateWindowAsync(windowType);
  }
}
}

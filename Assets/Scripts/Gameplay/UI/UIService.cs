#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Logging;
using Project.Core.Window;
using Project.Gameplay.Gameplay.UI;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Project.Gameplay.Gameplay.UI
{
    public interface IUIService
    {
        Canvas Canvas { get; }
        CanvasGroup CanvasGroup { get; }
        UniTask Initialized { get; }

        UniTask InitializeAsync();

        IReadOnlyList<Window> GetVisibleStack();
        IReadOnlyCollection<Window> GetAllWindows();
        void HideVisibleStack();
        int VisibleCount();

        T GetOrCreate<T>() where T : Window;
        Window GetOrCreate(Type type);
        Window GetOrCreate(string typeName);

        T Show<T>(bool immediate = false) where T : ParameterlessWindow;
        T Show<T, A1>(A1 a1, bool immediate = false) where T : ParameterWindow<A1>;
        T Show<T, A1, A2>(A1 a1, A2 a2, bool immediate = false) where T : ParameterWindow<A1, A2>;
        T Show<T, A1, A2, A3>(A1 a1, A2 a2, A3 a3, bool immediate = false) where T : ParameterWindow<A1, A2, A3>;

        void Close<T>() where T : Window;
        void Hide<T>() where T : Window;

        T FindWindow<T>() where T : Window;
        Window FindWindow(Type type);
        bool IsVisible<T>() where T : Window;
        bool IsVisible(Type type);

        bool HasVisibleByInterface<TInterface>() where TInterface : class;
        TInterface GetTopVisibleByInterface<TInterface>() where TInterface : class;
        IReadOnlyList<TInterface> GetVisibleByInterface<TInterface>() where TInterface : class;

        UniTask<T> ShowAsync<T>() where T : ParameterlessWindow;
        UniTask<T> ShowAsync<T, A1>(A1 a1) where T : ParameterWindow<A1>;
        UniTask<T> GetOrCreateAsync<T>() where T : Window;
        UniTask<Window> GetOrCreateAsync(Type windowType);
    }

    public sealed class UIService : IUIService
    {
        private readonly WindowsControllerInitializer _controllerInitializer;
        private readonly ILogger<UIService> _logger;
        private readonly UniTaskCompletionSource _initCompletionSource;

        public Canvas Canvas => _controllerInitializer.Controller.Canvas;

        public CanvasGroup CanvasGroup => _controllerInitializer.Controller.CanvasGroup;

        public UniTask Initialized => _initCompletionSource.Task;

        [Inject]
        public UIService(
            WindowsControllerInitializer controllerInitializer,
            ILogService logService)
        {
            _controllerInitializer = controllerInitializer;
            _logger = logService.CreateLogger<UIService>();
            _initCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask InitializeAsync()
        {
            if (_initCompletionSource.Task.Status == UniTaskStatus.Succeeded)
            {
                _logger.Trace("[UIService] Already initialized");
                return;
            }

            _logger.Debug("[UIService] InitializeAsync started");

            try
            {
                await _controllerInitializer.InitializeAsync();
                _initCompletionSource.TrySetResult();
                _logger.Debug("[UIService] InitializeAsync completed");
            }
            catch (Exception ex)
            {
                _logger.Error($"[UIService] InitializeAsync failed: {ex.Message}", ex);
                _initCompletionSource.TrySetException(ex);
                throw;
            }
        }

        public IReadOnlyList<Window> GetVisibleStack() => _controllerInitializer.Controller.GetVisibleStack();

        public IReadOnlyCollection<Window> GetAllWindows() => _controllerInitializer.Controller.GetAllWindows();

        public void HideVisibleStack() => _controllerInitializer.Controller.HideVisibleStack();

        public int VisibleCount() => _controllerInitializer.Controller.GetVisibleStack().Count;

        public T GetOrCreate<T>() where T : Window
        {
            return _controllerInitializer.Controller.GetOrCreateWindow<T>();
        }

        public Window GetOrCreate(Type type)
        {
            return _controllerInitializer.Controller.GetOrCreateWindow(type);
        }

        public Window GetOrCreate(string typeName)
        {
            var asm = typeof(Window).Assembly;
            var type = asm.GetType(typeName);
            return _controllerInitializer.Controller.GetOrCreateWindow(type);
        }

        public T Show<T>(bool immediate = false) where T : ParameterlessWindow
        {
            return _controllerInitializer.Controller.ShowWindow<T>(immediate);
        }

        public T Show<T, A1>(A1 a1, bool immediate = false) where T : ParameterWindow<A1>
        {
            return _controllerInitializer.Controller.ShowWindow<T, A1>(a1, immediate);
        }

        public T Show<T, A1, A2>(A1 a1, A2 a2, bool immediate = false) where T : ParameterWindow<A1, A2>
        {
            return _controllerInitializer.Controller.ShowWindow<T, A1, A2>(a1, a2, immediate);
        }

        public T Show<T, A1, A2, A3>(A1 a1, A2 a2, A3 a3, bool immediate = false)
            where T : ParameterWindow<A1, A2, A3>
        {
            return _controllerInitializer.Controller.ShowWindow<T, A1, A2, A3>(a1, a2, a3, immediate);
        }

        public void Close<T>() where T : Window
        {
            _controllerInitializer.Controller.CloseWindow<T>();
        }

        public void Hide<T>() where T : Window
        {
            _controllerInitializer.Controller.HideWindow<T>();
        }

        public T FindWindow<T>() where T : Window
        {
            return _controllerInitializer.Controller.GetWindow<T>();
        }

        public Window FindWindow(Type type)
        {
            return _controllerInitializer.Controller.GetWindow(type);
        }

        public bool IsVisible<T>() where T : Window
        {
            return _controllerInitializer.Controller.IsWindowVisible<T>();
        }

        public bool IsVisible(Type type)
        {
            return _controllerInitializer.Controller.IsWindowVisible(type);
        }

        public bool HasVisibleByInterface<TInterface>() where TInterface : class
        {
            return _controllerInitializer.Controller.HasVisibleByInterface<TInterface>();
        }

        public TInterface GetTopVisibleByInterface<TInterface>() where TInterface : class
        {
            return _controllerInitializer.Controller.GetTopVisibleByInterface<TInterface>();
        }

        public IReadOnlyList<TInterface> GetVisibleByInterface<TInterface>() where TInterface : class
        {
            return _controllerInitializer.Controller.GetVisibleByInterface<TInterface>();
        }

        public async UniTask<T> ShowAsync<T>() where T : ParameterlessWindow
        {
            return await _controllerInitializer.Controller.ShowWindowAsync<T>();
        }

        public async UniTask<T> ShowAsync<T, A1>(A1 a1) where T : ParameterWindow<A1>
        {
            return await _controllerInitializer.Controller.ShowWindowAsync<T, A1>(a1);
        }

        public async UniTask<T> GetOrCreateAsync<T>() where T : Window
        {
            return (T)await GetOrCreateAsync(typeof(T));
        }

        public async UniTask<Window> GetOrCreateAsync(Type windowType)
        {
            return await _controllerInitializer.Controller.GetOrCreateWindowAsync(windowType);
        }
    }

    public static class UIServiceExtensions
    {
        public static async UniTask<T> ShowAsync<T, A1, A2>(this IUIService uiService, A1 a1, A2 a2)
            where T : ParameterWindow<A1, A2>
        {
            return await uiService.GetOrCreateAsync<T>();
        }

        public static async UniTask<T> ShowAsync<T, A1, A2, A3>(this IUIService uiService, A1 a1, A2 a2, A3 a3)
            where T : ParameterWindow<A1, A2, A3>
        {
            return await uiService.GetOrCreateAsync<T>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Core.Core.Logging;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ILogger = Project.Core.Core.Logging.ILogger;

namespace Project.Gameplay.Gameplay.UI
{
    /// <summary>
    /// Сервис для создания и управления UI-элементами с автоматическим инжектом зависимостей.
    /// Вдохновлён LiteUI.UIService.
    /// </summary>
    public class UIAssetService : IUIAssetService, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly IAssetService _assetService;
        private readonly ILogger _logger;

        private readonly Dictionary<string, GameObject> _cachedPrefabs = new();
        private readonly Dictionary<string, GameObject> _cachedPrefabsByName = new();
        private readonly List<CreateRequest> _inProgressRequests = new();
        private readonly List<CreateResponse> _deferredResponses = new();

        private bool _disposed;

        public bool Caching { get; set; } = true;

        [Inject]
        public UIAssetService(IObjectResolver resolver, IAssetService assetService, ILogService logService)
        {
            _resolver = resolver;
            _assetService = assetService;
            _logger = logService.CreateLogger<UIAssetService>();
        }

        public async UniTask<GameObject> LoadPrefabAsync(string address, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            _logger.Debug($"[UIAssetService] LoadPrefabAsync: {address}");
            return await _assetService.LoadAssetAsync<GameObject>(address, cancellationToken);
        }

        public GameObject? InstantiatePrefabDirectly(GameObject prefab, Vector3 position, Quaternion rotation, Transform? parent = null)
        {
            ThrowIfDisposed();
            
            if (prefab == null)
            {
                _logger.Error("InstantiatePrefabDirectly: prefab is null");
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            _resolver.InjectGameObject(instance);
            return instance;
        }

        public T Instantiate<T>(T prefab, Transform parent) where T : Component
        {
            ThrowIfDisposed();
            T instance = UnityEngine.Object.Instantiate(prefab, parent);
            _resolver.InjectGameObject(instance.gameObject);
            return instance;
        }

        public T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            ThrowIfDisposed();
            T instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            _resolver.InjectGameObject(instance.gameObject);
            return instance;
        }

        public GameObject Instantiate(GameObject prefab, Transform parent)
        {
            ThrowIfDisposed();
            GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
            _resolver.InjectGameObject(instance);
            return instance;
        }

        public UniTask<T> CreateAsync<T>(string address, Transform? parent = null, CancellationToken cancellationToken = default) where T : MonoBehaviour
        {
            return CreateAsync<T>(address, null, parent, cancellationToken);
        }

        public async UniTask<T> CreateAsync<T>(string address, object[]? initParams, Transform? parent = null, CancellationToken cancellationToken = default) where T : MonoBehaviour
        {
            ThrowIfDisposed();
            _logger.Debug($"[UIAssetService] CreateAsync<{typeof(T).Name}> started, address='{address}'");

            try
            {
                CreateRequest request = new(typeof(T), address, initParams);
                GameObject gameObject = await CreateAsyncInternal(request, cancellationToken);
                _logger.Debug($"[UIAssetService] CreateAsyncInternal completed, gameObject={gameObject?.name}");

                if (gameObject == null)
                {
                    _logger.Error($"[UIAssetService] gameObject is null after CreateAsyncInternal");
                    throw new NullReferenceException($"Failed to create {typeof(T).Name} from '{address}'");
                }

                if (parent != null)
                {
                    gameObject.transform.SetParent(parent, false);
                    _logger.Debug($"[UIAssetService] Parent set to {parent.name}");
                }

                T controller = gameObject.GetComponent<T>();
                if (controller == null)
                {
                    _logger.Error($"[UIAssetService] Component {typeof(T).Name} not found on created UI object: {address}");
                    Release(gameObject);
                    throw new MissingComponentException($"Component {typeof(T).Name} not found");
                }

                _logger.Debug($"[UIAssetService] CreateAsync<{typeof(T).Name}> completed successfully");
                return controller;
            }
            catch (Exception ex)
            {
                _logger.Error($"[UIAssetService] CreateAsync<{typeof(T).Name}> failed: {ex.Message}", ex);
                throw;
            }
        }

        public async UniTask<List<T>> CreateCollectionAsync<T>(string address, List<object[]>? itemsParams, Transform? parent = null, CancellationToken cancellationToken = default) where T : MonoBehaviour
        {
            ThrowIfDisposed();

            if (itemsParams == null || itemsParams.Count == 0)
            {
                return new List<T>();
            }

            List<UniTask<T>> loadTasks = new(itemsParams.Count);
            List<T> result = new(itemsParams.Count);

            for (int i = 0; i < itemsParams.Count; i++)
            {
                object[] itemParams = itemsParams[i];
                UniTask<T> task = CreateAsync<T>(address, itemParams, parent, cancellationToken);
                loadTasks.Add(task);
            }

            T[] results = await UniTask.WhenAll(loadTasks);
            return results.ToList();
        }

        public void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            _logger.Debug($"Releasing UI instance: {instance.name}");
            _assetService.ReleaseInstance(instance);
        }

        public void FlushCache()
        {
            _logger.Info("Flushing UI prefabs cache");

            foreach (GameObject prefab in _cachedPrefabs.Values)
            {
                _assetService.Release(prefab);
            }
            _cachedPrefabs.Clear();

            foreach (GameObject prefab in _cachedPrefabsByName.Values)
            {
                _assetService.Release(prefab);
            }
            _cachedPrefabsByName.Clear();
        }

        public T AttachController<T>(GameObject uiObject, params object?[]? initParams) where T : MonoBehaviour
        {
            ThrowIfDisposed();

            T controller = uiObject.GetComponent<T>();
            if (controller == null)
            {
                _logger.Error($"Component {typeof(T).Name} not found on UI object: {uiObject.name}");
                throw new MissingComponentException($"Component {typeof(T).Name} not found");
            }

            _resolver.InjectGameObject(uiObject);

            if (initParams != null && initParams.Length > 0)
            {
                InvokeInitIfExists(controller, initParams);
            }

            return controller;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _logger.Info("Disposing UIAssetService");
            FlushCache();
        }

        private async UniTask<GameObject> CreateAsyncInternal(CreateRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug($"[UIAssetService] CreateAsyncInternal started, address='{request.Address}'");
            
            if (HasSimilarInProgressRequest(request))
            {
                _logger.Debug($"[UIAssetService] Similar request in progress, deferring...");
                CreateResponse deferredResponse = new(request, cancellationToken);
                _deferredResponses.Add(deferredResponse);
                GameObject gameObject = await deferredResponse.CreateCompletionSource.Task;
                cancellationToken.ThrowIfCancellationRequested();
                _logger.Debug($"[UIAssetService] Deferred request completed");
                return gameObject;
            }

            GameObject? cachedObject = GetFromCache(request);
            if (cachedObject != null)
            {
                _logger.Debug($"[UIAssetService] Found cached prefab: {cachedObject.name}");
                return BuildUIObject(request, cachedObject);
            }

            _logger.Debug($"[UIAssetService] No cache, loading from Addressables...");
            _inProgressRequests.Add(request);

            try
            {
                GameObject prefab = await _assetService.LoadAssetAsync<GameObject>(request.Address, cancellationToken);
                _logger.Debug($"[UIAssetService] Prefab loaded: {prefab?.name}");
                
                if (prefab == null)
                {
                    _logger.Error($"[UIAssetService] Failed to load prefab: {request.Address}");
                    throw new NullReferenceException($"Failed to load prefab: {request.Address}");
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Debug($"[UIAssetService] Cancelled during load");
                    if (HasSimilarInProgressRequest(request))
                    {
                        SaveToCache(request, prefab);
                        ApplySameDeferred(request, prefab);
                    }
                    else
                    {
                        _assetService.Release(prefab);
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                }

                SaveToCache(request, prefab);
                GameObject result = BuildUIObject(request, prefab);
                ApplySameDeferred(request, prefab);
                _logger.Debug($"[UIAssetService] CreateAsyncInternal completed successfully");
                return result;
            }
            catch (Exception e)
            {
                _logger.Error($"[UIAssetService] Exception during load: {e}");
                AbortSameDeferred(request, e);
                throw;
            }
            finally
            {
                _inProgressRequests.Remove(request);
            }
        }

        private GameObject? GetFromCache(CreateRequest request)
        {
            if (!Caching)
            {
                return null;
            }

            if (request.Address != null && _cachedPrefabs.ContainsKey(request.Address))
            {
                return _cachedPrefabs[request.Address];
            }

            return null;
        }

        private void SaveToCache(CreateRequest request, GameObject prefab)
        {
            if (!Caching || request.Address == null)
            {
                return;
            }

            _cachedPrefabs[request.Address] = prefab;
        }

        private List<CreateResponse> GetSimilarResponses(CreateRequest request)
        {
            List<CreateResponse> similarRequests = new();
            foreach (CreateResponse createResponse in _deferredResponses)
            {
                if (request.IsSimilarRequest(createResponse.CreateRequest))
                {
                    similarRequests.Add(createResponse);
                }
            }
            return similarRequests;
        }

        private bool HasSimilarInProgressRequest(CreateRequest request)
        {
            return _inProgressRequests.Any(r => r.IsSimilarRequest(request));
        }

        private void AbortSameDeferred(CreateRequest request, Exception e)
        {
            List<CreateResponse> similarResponses = GetSimilarResponses(request);
            foreach (CreateResponse responses in similarResponses)
            {
                responses.CreateCompletionSource.TrySetException(
                    new NullReferenceException($"Error at load {request.Address}", e));
                _deferredResponses.Remove(responses);
            }
        }

        private void ApplySameDeferred(CreateRequest request, GameObject prefab)
        {
            List<CreateResponse> similarResponses = GetSimilarResponses(request);
            foreach (CreateResponse response in similarResponses)
            {
                try
                {
                    if (response.CancellationToken.IsCancellationRequested)
                    {
                        response.CreateCompletionSource.TrySetCanceled();
                        continue;
                    }

                    GameObject uiObject = BuildUIObject(response.CreateRequest, prefab);
                    response.CreateCompletionSource.TrySetResult(uiObject);
                }
                catch (Exception e)
                {
                    response.CreateCompletionSource.TrySetException(e);
                }
                finally
                {
                    _deferredResponses.Remove(response);
                }
            }
        }

        private GameObject BuildUIObject(CreateRequest request, GameObject prefab)
        {
            GameObject instantiatedObject = UnityEngine.Object.Instantiate(prefab);
            instantiatedObject.name = instantiatedObject.name.Replace("(Clone)", "");

            _resolver.InjectGameObject(instantiatedObject);

            try
            {
                InitUi(request, instantiatedObject);
            }
            catch (Exception)
            {
                if (instantiatedObject != null)
                {
                    _assetService.ReleaseInstance(instantiatedObject);
                }
                throw;
            }

            return instantiatedObject;
        }

        private void InitUi(CreateRequest request, GameObject uiObject)
        {
            if (request.ControllerType != null && request.InitParams != null && request.InitParams.Length > 0)
            {
                MonoBehaviour controller = uiObject.GetComponent(request.ControllerType) as MonoBehaviour;
                if (controller != null)
                {
                    InvokeInitIfExists(controller, request.InitParams);
                }
            }
        }

        private void InvokeInitIfExists(MonoBehaviour controller, object?[]? initParams)
        {
            if (initParams == null || initParams.Length == 0)
            {
                return;
            }

            Type controllerType = controller.GetType();
            MethodInfo? initMethod = controllerType.GetMethod("Init", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (initMethod != null)
            {
                try
                {
                    initMethod.Invoke(controller, initParams);
                    _logger.Trace($"Init method called on {controllerType.Name}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error calling Init method on {controllerType.Name}", ex);
                    throw;
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UIAssetService));
            }
        }

        private sealed class CreateRequest
        {
            public Type? ControllerType { get; }
            public string? Address { get; }
            public object?[]? InitParams { get; }

            public CreateRequest(Type controllerType, string address, object?[]? initParams)
            {
                ControllerType = controllerType;
                Address = address;
                InitParams = initParams;
            }

            public bool IsSimilarRequest(CreateRequest another)
            {
                return (Address != null && Address == another.Address) ||
                       (ControllerType != null && ControllerType == another.ControllerType);
            }
        }

        private sealed class CreateResponse
        {
            public CreateRequest CreateRequest { get; }
            public UniTaskCompletionSource<GameObject> CreateCompletionSource { get; }
            public CancellationToken CancellationToken { get; }

            public CreateResponse(CreateRequest createRequest, CancellationToken cancellationToken)
            {
                CreateRequest = createRequest;
                CreateCompletionSource = new UniTaskCompletionSource<GameObject>();
                CancellationToken = cancellationToken;
            }
        }
    }

    /// <summary>
    /// Fluent API модель для создания UI-элементов.
    /// </summary>
    public class UIModel
    {
        internal UiData Data { get; private set; }

        private UIModel()
        {
            Data = new UiData();
        }

        public static UIModel Create<TC>(params object?[]? initParameters)
        {
            return Create(typeof(TC), initParameters);
        }

        public static UIModel Create(Type controller, params object?[]? initParameters)
        {
            UIModel result = new()
            {
                Data =
                {
                    Controller = controller,
                    InitParameters = initParameters
                }
            };
            return result;
        }

        public UIModel Container(Transform? container)
        {
            Data.Container = container;
            return this;
        }

        public UIModel Container(GameObject container)
        {
            return Container(container.transform);
        }

        public UIModel Container(MonoBehaviour container)
        {
            return Container(container.transform);
        }

        public UIModel Name(string name)
        {
            Data.Name = name;
            return this;
        }

        internal class UiData
        {
            internal Type Controller { get; set; } = null!;
            internal Transform? Container { get; set; }
            internal object?[]? InitParameters { get; set; }
            internal string? Name { get; set; }
        }
    }

    /// <summary>
    /// Fluent API модель для создания UI-элементов с типом.
    /// </summary>
    public class UIModel<TC> where TC : MonoBehaviour
    {
        internal UIModel.UiData Data { get; private set; }

        private UIModel()
        {
            Data = new UIModel.UiData();
        }

        public static UIModel<TC> Create(params object?[]? initParameters)
        {
            UIModel<TC> result = new()
            {
                Data =
                {
                    Controller = typeof(TC),
                    InitParameters = initParameters
                }
            };
            return result;
        }

        public UIModel<TC> Container(Transform? container)
        {
            Data.Container = container;
            return this;
        }

        public UIModel<TC> Container(GameObject container)
        {
            return Container(container.transform);
        }

        public UIModel<TC> Container(MonoBehaviour container)
        {
            return Container(container.transform);
        }

        public UIModel<TC> Name(string name)
        {
            Data.Name = name;
            return this;
        }
    }

    /// <summary>
    /// Fluent API модель для создания коллекции UI-элементов.
    /// </summary>
    public class UICollectionModel
    {
        internal UiData Data { get; private set; }

        private UICollectionModel()
        {
            Data = new UiData();
        }

        public static UICollectionModel Create<TC>(List<object[]> initParams)
        {
            return Create(typeof(TC), initParams);
        }

        public static UICollectionModel Create(Type controllerType, List<object[]> initParams)
        {
            UICollectionModel result = new()
            {
                Data =
                {
                    Controller = controllerType,
                    Collection = initParams
                }
            };
            return result;
        }

        public UICollectionModel Container(Transform container)
        {
            Data.Container = container;
            return this;
        }

        public UICollectionModel Container(GameObject container)
        {
            return Container(container.transform);
        }

        public UICollectionModel Container(MonoBehaviour container)
        {
            return Container(container.transform);
        }

        public UICollectionModel LoadItemCallback<TC, TP>(Action<TC, int, TP> loadItemCallback) where TC : MonoBehaviour
        {
            Data.LoadItemCallback = (m, i, d) => loadItemCallback?.Invoke((TC)m, i, (TP)d);
            return this;
        }

        public UICollectionModel Name<T>(Func<T, string> nameCallback)
        {
            Data.NameCallback = p => nameCallback.Invoke((T)p);
            return this;
        }

        internal class UiData
        {
            internal Type Controller { get; set; } = null!;
            internal List<object[]>? Collection { get; set; }
            internal Transform? Container { get; set; }
            internal Action<MonoBehaviour, int, object>? LoadItemCallback { get; set; }
            internal Func<object, string>? NameCallback { get; set; }
        }
    }
}

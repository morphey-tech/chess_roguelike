using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LiteUI.Common.Extensions;
using Project.Core.Core.Assets;
using Project.Core.Core.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;
using ILogger = Project.Core.Core.Logging.ILogger;

namespace Project.Gameplay.Gameplay.Assets
{
    public class AssetService : IAssetService, IDisposable, IInitializable
    {
        private readonly IObjectResolver _resolver;
        private readonly ILogger _logger;

        private readonly Dictionary<string, AsyncOperationHandle> _loadedAssets = new();
        private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instantiatedObjects = new();
        private readonly Dictionary<int, object> _objectKeys = new();
        private readonly HashSet<string> _preloadedAddresses = new();
        private readonly Dictionary<Sprite, Texture2D> _spriteTextures = new();
        private bool _disposed;
        private bool _catalogInitialized;

        private const int LOAD_CATALOG_TRY_COUNT = 3;
        private static readonly TimeSpan LOAD_CATALOG_TRY_DELAY = TimeSpan.FromSeconds(1f);

        [Inject]
        private AssetService(IObjectResolver resolver, ILogService logService)
        {
            _resolver = resolver;
            _logger = logService.CreateLogger<AssetService>();
        }

        void IInitializable.Initialize()
        {
            _logger.Debug("[AssetService] IInitializable.Initialize called");
            _ = InitializeAsync(CancellationToken.None);
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_catalogInitialized)
            {
                _logger.Trace("[AssetService] Catalog already initialized");
                return;
            }

            _logger.Debug("[AssetService] InitializeAsync started");

            for (int i = 0; i < LOAD_CATALOG_TRY_COUNT; i++)
            {
                bool lastTry = i == LOAD_CATALOG_TRY_COUNT - 1;
                _logger.Debug($"[AssetService] InitCatalog attempt {i + 1}/{LOAD_CATALOG_TRY_COUNT}");

                bool initSuccess = await InitCatalogAsync(lastTry, cancellationToken);
                if (initSuccess)
                {
                    _catalogInitialized = true;
                    _logger.Info("[AssetService] Addressables catalog initialized successfully");
                    return;
                }

                if (!lastTry)
                {
                    _logger.Warning($"[AssetService] Addressables init attempt {i + 1} failed, retrying...");
                    await UniTask.Delay(LOAD_CATALOG_TRY_DELAY, cancellationToken: cancellationToken);
                }
            }

            _logger.Error("[AssetService] Addressables initialization failed after all attempts");
            throw new Exception("Addressables initialization failed. See logs for details.");
        }

        public async UniTask<T?> LoadAssetAsync<T>(string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(address))
            {
                _logger.Error("Address is null or empty");
                return null;
            }

            if (_loadedAssets.TryGetValue(address, out AsyncOperationHandle existingHandle))
            {
                if (existingHandle is { Status: AsyncOperationStatus.Succeeded })
                {
                    var result = existingHandle.Result as T;
                    if (result != null)
                    {
                        return result;
                    }
                    // Result is null (e.g., Texture2D when requesting Sprite) — remove from cache and reload
                    _loadedAssets.Remove(address);
                }
                else
                {
                    _loadedAssets.Remove(address);
                }
            }

            try
            {
                _logger.Debug($"[AssetService] Loading asset: {address}");

                if (typeof(T) == typeof(Sprite))
                {
                    UnityEngine.Object? loadedObject = await LoadAssetAsync<UnityEngine.Object>(address, cancellationToken);
                    
                    switch (loadedObject)
                    {
                        case Sprite loadedSprite:
                            return (T)(UnityEngine.Object)loadedSprite;
                        case Texture2D loadedTexture:
                        {
                            Sprite sprite = Sprite.Create(loadedTexture,
                                new Rect(0, 0, loadedTexture.width, loadedTexture.height),
                                new Vector2(0.5f, 0.5f));
                            _spriteTextures[sprite] = loadedTexture;
                            return (T)(UnityEngine.Object)sprite;
                        }
                        default:
                            return loadedObject as T;
                    }
                }

                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
                T? obj = await handle.ToUniTaskWithResult(cancellationToken);

                if (handle.Status != AsyncOperationStatus.Succeeded || obj == null)
                {
                    _logger.Error($"[AssetService] Failed to load asset: {address}");
                    Addressables.Release(handle);
                    return null;
                }

                int instanceId = obj.GetInstanceID();
                _loadedAssets[address] = handle;
                _objectKeys[instanceId] = address;

                _logger.Debug($"[AssetService] Asset loaded: {address}, instanceId={instanceId}");
                return obj;
            }
            catch (OperationCanceledException)
            {
                _logger.Trace($"[AssetService] Load cancelled: {address}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"[AssetService] Exception loading asset {address}: {ex.Message}");
                return null;
            }
        }

        public UniTask<T?> LoadAssetAsync<T>(AssetKey key, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return LoadAssetAsync<T>(key.Address, cancellationToken);
        }

        public async UniTask<GameObject?> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform? parent = null, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            
            if (string.IsNullOrEmpty(address))
            {
                _logger.Error("Address is null or empty");
                return null;
            }
            
            try
            {
                _logger.Debug($"Instantiating: {address}");
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, position, rotation, parent);
                await handle.ToUniTask();
                GameObject instance = handle.Result;
                
                if (handle.Status != AsyncOperationStatus.Succeeded || instance == null)
                {
                    _logger.Error($"Failed to instantiate: {address}");
                    return null;
                }
                
                _instantiatedObjects[instance] = handle;
                _resolver.InjectGameObject(instance);
                _logger.Debug($"Instantiated: {address}");
                return instance;
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception instantiating {address}", ex);
                return null;
            }
        }

        public UniTask<GameObject?> InstantiateAsync(AssetKey key, Vector3 position, Quaternion rotation, Transform? parent = null, CancellationToken cancellationToken = default)
        {
            return InstantiateAsync(key.Address, position, rotation, parent, cancellationToken);
        }

        public GameObject? InstantiateFromPrefab(GameObject prefab, Vector3 position, Quaternion rotation,
            Transform? parent = null)
        {
            if (prefab == null)
            {
                _logger.Error("InstantiateFromPrefab: prefab is null");
                return null;
            }
            GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            _resolver.InjectGameObject(instance);
            return instance;
        }

        public GameObject? InstantiatePrefabDirectly(GameObject prefab, Vector3 position, Quaternion rotation,
            Transform? parent = null)
        {
            if (prefab == null)
            {
                _logger.Error("InstantiatePrefabDirectly: prefab is null");
                return null;
            }
            GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            _resolver.InjectGameObject(instance);
            return instance;
        }

        public void Release<T>(T asset) where T : UnityEngine.Object
        {
            if (asset == null)
            {
                return;
            }

            string? addressToRemove = null;
            foreach (KeyValuePair<string, AsyncOperationHandle> kvp in _loadedAssets)
            {
                if (kvp.Value.Result as T == asset)
                {
                    addressToRemove = kvp.Key;
                    break;
                }
            }
            
            if (addressToRemove != null)
            {
                if (_loadedAssets.TryGetValue(addressToRemove, out AsyncOperationHandle handle))
                {
                    _logger.Debug($"Releasing asset: {addressToRemove}");
                    Addressables.Release(handle);
                    _loadedAssets.Remove(addressToRemove);
                    _preloadedAddresses.Remove(addressToRemove);
                }
            }
        }
        
        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (_instantiatedObjects.TryGetValue(instance, out AsyncOperationHandle<GameObject> handle))
            {
                _logger.Debug($"Releasing instance: {instance.name}");
                Addressables.ReleaseInstance(handle);
                _instantiatedObjects.Remove(instance);
            }
            else
            {
                _logger.Warning($"Instance not tracked, destroying: {instance.name}");
                UnityEngine.Object.Destroy(instance);
            }
        }
        
        public void ReleaseAll()
        {
            _logger.Info("Releasing all assets");
            
            foreach (AsyncOperationHandle<GameObject> handle in _instantiatedObjects.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.ReleaseInstance(handle);
                }
            }
            _instantiatedObjects.Clear();
            
            foreach (AsyncOperationHandle handle in _loadedAssets.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _loadedAssets.Clear();
            _preloadedAddresses.Clear();
        }
        
        public async UniTask PreloadAsync(string address, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (_preloadedAddresses.Contains(address))
            {
                return;
            }

            _logger.Debug($"Preloading: {address}");
            AsyncOperationHandle handle = Addressables.LoadAssetAsync<UnityEngine.Object>(address);
            await handle.ToUniTask(cancellationToken: cancellationToken);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedAssets[address] = handle;
                _preloadedAddresses.Add(address);
                _logger.Debug($"Preloaded: {address}");
            }
        }

        public async UniTask PreloadAsync(string[] addresses, CancellationToken cancellationToken = default)
        {
            List<UniTask> tasks = new(addresses.Length);

            foreach (string address in addresses)
            {
                tasks.Add(PreloadAsync(address, cancellationToken));
            }

            await UniTask.WhenAll(tasks);
        }
        
        public bool IsLoaded(string address)
        {
            if (!_loadedAssets.TryGetValue(address, out AsyncOperationHandle handle))
            {
                return false;
            }
            
            return handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded;
        }

        public async UniTask<long> GetDownloadSizeAsync(string address)
        {
            try
            {
                AsyncOperationHandle<long> handle = Addressables.GetDownloadSizeAsync(address);
                await handle.ToUniTask();
                long size = handle.Result;
                Addressables.Release(handle);
                return size;
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception getting download size for {address}", ex);
                return 0;
            }
        }
        
        public async UniTask DownloadDependenciesAsync(string address, Action<float>? onProgress = null)
        {
            try
            {
                _logger.Info($"Downloading dependencies: {address}");
                AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(address);

                while (!handle.IsDone)
                {
                    onProgress?.Invoke(handle.PercentComplete);
                    await UniTask.Yield();
                }

                onProgress?.Invoke(1f);
                Addressables.Release(handle);
                _logger.Info($"Dependencies downloaded: {address}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception downloading dependencies for {address}", ex);
            }
        }

        public async UniTask<bool> IsRemote(string address, CancellationToken cancellationToken = default)
        {
            try
            {
                AsyncOperationHandle<long> handle = Addressables.GetDownloadSizeAsync(address);
                long size = await handle.ToUniTaskWithResult(cancellationToken);
                Addressables.Release(handle);
                return size > 0;
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception checking remote status for {address}", ex);
                return false;
            }
        }

        private async UniTask<bool> InitCatalogAsync(bool logExceptionAsError, CancellationToken cancellationToken = default)
        {
            AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator>? initHandle = null;
            try
            {
                _logger.Debug("[AssetService] Addressables.InitializeAsync called");
                initHandle = Addressables.InitializeAsync();
                await initHandle.Value.ToUniTaskWithResult(cancellationToken);
                _logger.Debug($"[AssetService] Addressables.InitializeAsync completed, locations={initHandle.Value.Result.AllLocations}");
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.Trace("[AssetService] InitCatalogAsync cancelled");
                throw;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                if (initHandle.HasValue)
                {
                    Addressables.Release(initHandle.Value);
                }

                if (logExceptionAsError)
                {
                    _logger.Error("[AssetService] Error while init addressables. ArgumentOutOfRangeException", ex);
                }
                else
                {
                    _logger.Debug("[AssetService] Error while init addressables. ArgumentOutOfRangeException");
                }

                return false;
            }
            catch (Exception ex)
            {
                if (logExceptionAsError)
                {
                    _logger.Error("[AssetService] Error while init addressables", ex);
                }
                else
                {
                    _logger.Debug("[AssetService] Error while init addressables");
                }

                return false;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AssetService));
            }
        }
        
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _logger.Info("Disposing AssetService");
            ReleaseAll();
        }
    }
}



using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    public class AssetService : IAssetService, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly ILogger _logger;
        
        private readonly Dictionary<string, AsyncOperationHandle> _loadedAssets = new();
        private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instantiatedObjects = new();
        private readonly HashSet<string> _preloadedAddresses = new();
        private bool _disposed;
        
        [Inject]
        private AssetService(IObjectResolver resolver, ILogService logService)
        {
            _resolver = resolver;
            _logger = logService.CreateLogger<AssetService>();
        }
        
        public async UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            ThrowIfDisposed();
            
            if (string.IsNullOrEmpty(address))
            {
                _logger.Error("Address is null or empty");
                return null;
            }
            
            if (_loadedAssets.TryGetValue(address, out AsyncOperationHandle existingHandle))
            {
                if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    _logger.Trace($"Asset already loaded: {address}");
                    return existingHandle.Result as T;
                }
            }
            
            try
            {
                _logger.Debug($"Loading asset: {address}");
                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
                await handle.ToUniTask();
                
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    _logger.Error($"Failed to load asset: {address}");
                    return null;
                }
                
                _loadedAssets[address] = handle;
                _logger.Debug($"Asset loaded: {address}");
                return handle.Result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception loading asset {address}", ex);
                return null;
            }
        }
        
        public UniTask<T> LoadAssetAsync<T>(AssetKey key) where T : UnityEngine.Object
        {
            return LoadAssetAsync<T>(key.Address);
        }
        
        public async UniTask<GameObject> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null)
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
        
        public UniTask<GameObject> InstantiateAsync(AssetKey key, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return InstantiateAsync(key.Address, position, rotation, parent);
        }

        public GameObject InstantiateFromPrefab(GameObject prefab, Vector3 position, Quaternion rotation,
            Transform parent = null)
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

        public void Release<T>(T asset) where T : UnityEngine.Object
        {
            if (asset == null) return;
            
            string addressToRemove = null;
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
            if (instance == null) return;
            
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
        
        public async UniTask PreloadAsync(string address)
        {
            ThrowIfDisposed();
            
            if (_preloadedAddresses.Contains(address)) return;
            
            _logger.Debug($"Preloading: {address}");
            AsyncOperationHandle handle = Addressables.LoadAssetAsync<UnityEngine.Object>(address);
            await handle.ToUniTask();
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedAssets[address] = handle;
                _preloadedAddresses.Add(address);
                _logger.Debug($"Preloaded: {address}");
            }
        }
        
        public async UniTask PreloadAsync(string[] addresses)
        {
            List<UniTask> tasks = new List<UniTask>(addresses.Length);
            
            foreach (string address in addresses)
            {
                tasks.Add(PreloadAsync(address));
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
        
        public async UniTask DownloadDependenciesAsync(string address, Action<float> onProgress = null)
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

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AssetService));
            }
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _logger.Info("Disposing AssetService");
            ReleaseAll();
        }
    }
}



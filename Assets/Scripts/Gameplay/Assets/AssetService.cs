using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Project.Gameplay.Gameplay.Assets
{
    public class AssetService : IAssetService, IDisposable
    {
        private readonly IObjectResolver _resolver;
        
        private readonly Dictionary<string, ICachedAsset> _assets = new();
        private readonly Dictionary<Object, ICachedAsset> _objectAssets = new();

        [Inject]
        private AssetService(IObjectResolver resolver)
        {
            _resolver = resolver;
        }
        
        public async UniTask<T> LoadAsync<T>(
            string address,
            CancellationToken ct = default) 
            where T : Object
        {
            if (_assets.TryGetValue(address, out ICachedAsset cachedObj))
            {
                if (cachedObj is CachedAsset<T> cached)
                {
                    cached.RefCount++;
                    return cached.Handle.Result;
                }
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            try
            {
                await handle.ToUniTask(cancellationToken: ct);
            }
            catch
            {
                Addressables.Release(handle);
                throw;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);
                throw new Exception($"Failed to load asset: {address}");
            }

            CachedAsset<T> cachedAsset = new(handle, address);
            _assets[address] = cachedAsset;
            _objectAssets[handle.Result] = cachedAsset;
            _resolver.Inject(handle.Result);
            return handle.Result;
        }

        public async UniTask<GameObject> InstantiateAsync(
            string address,
            Vector3 position,
            Quaternion rotation,
            Transform? parent = null,
            CancellationToken ct = default)
        {
            GameObject prefab = await LoadAsync<GameObject>(address, ct);
            GameObject instance = Object.Instantiate(prefab, position, rotation, parent);
            _resolver.InjectGameObject(instance);
            return instance;
        }

        public async UniTask<T> InstantiateAsync<T>(
            string address,
            Vector3 position,
            Quaternion rotation,
            Transform? parent = null,
            CancellationToken ct = default)
            where T : Component
        {
            GameObject prefab = await LoadAsync<GameObject>(address, ct);
            GameObject instance = Object.Instantiate(prefab, position, rotation, parent);
            T component = instance.GetComponent<T>();
            _resolver.InjectGameObject(instance);
            return component == null 
                ? throw new Exception($"Prefab {address} does not contain component of type {typeof(T).Name}") 
                : component;
        }

        public T Instantiate<T>(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform? parent = null)
            where T : Component
        {
            GameObject instance = Object.Instantiate(prefab, position, rotation, parent);
            T component = instance.GetComponent<T>();
            _resolver.InjectGameObject(instance);
            return component == null 
                ? throw new Exception($"Prefab does not contain component of type {typeof(T).Name}") 
                : component;
        }

        public GameObject Instantiate(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform? parent = null)
        {
            GameObject instance = Object.Instantiate(prefab, position, rotation, parent);
            _resolver.InjectGameObject(instance);
            return instance;
        }

        public void Release(string address)
        {
            if (!_assets.TryGetValue(address, out ICachedAsset cached))
            {
                return;
            }

            cached.RefCount--;
            if (cached.RefCount <= 0)
            {
                cached.ReleaseHandle();
                _assets.Remove(address);

                if (cached.Asset != null)
                {
                    _objectAssets.Remove(cached.Asset);
                }
            }
        }

        public void Release(Object obj)
        {
            if (!_objectAssets.TryGetValue(obj, out ICachedAsset cached))
            {
                return;
            }

            cached.RefCount--;
            if (cached.RefCount <= 0)
            {
                cached.ReleaseHandle();
                _objectAssets.Remove(obj);

                if (!string.IsNullOrEmpty(cached.Address))
                {
                    _assets.Remove(cached.Address);
                }
            }
        }

        public async UniTask PreloadAsync(
            string address,
            CancellationToken ct = default)
        {
            if (_assets.ContainsKey(address))
            {
                return;
            }
            await LoadAsync<Object>(address, ct);
        }

        public void Dispose()
        {
            foreach (ICachedAsset cached in _assets.Values)
            {
                cached.ReleaseHandle();
            }
            _assets.Clear();
            _objectAssets.Clear();
        }

        private interface ICachedAsset
        {
            int RefCount { get; set; }
            string Address { get; }
            Object Asset { get; }
            void ReleaseHandle();
        }

        private class CachedAsset<T> : ICachedAsset where T : Object
        {
            public AsyncOperationHandle<T> Handle { get; private set; }
            public int RefCount { get; set; }
            public string Address { get; private set; }
            public Object Asset => Handle.Result;

            public CachedAsset(AsyncOperationHandle<T> handle, string address)
            {
                Handle = handle;
                Address = address;
                RefCount = 1;
            }

            public void ReleaseHandle()
            {
                if (Handle.IsValid())
                    Addressables.Release(Handle);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using Project.Gameplay.Gameplay.UI.Project.Gameplay.Gameplay.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.Gameplay.UI
{
    public class UIAssetService : IUIAssetService, IDisposable
    {
        private readonly IAssetService _assetService;
        private readonly IObjectResolver _resolver;

        private readonly Dictionary<string, GameObject> _prefabs = new();

        public UIAssetService(
            IAssetService assetService,
            IObjectResolver resolver)
        {
            _assetService = assetService;
            _resolver = resolver;
        }

        public async UniTask<T> CreateAsync<T>(
            string address,
            Transform parent = null,
            CancellationToken ct = default)
            where T : MonoBehaviour
        {
            GameObject prefab = await LoadPrefab(address, ct);
            GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.name = prefab.name;

            T controller = instance.GetComponent<T>();
            if (controller == null)
            {
                throw new Exception(
                    $"Component {typeof(T).Name} not found on prefab {address}");
            }
            _resolver.InjectGameObject(instance);
            return controller;
        }

        public T Instantiate<T>(T prefab, Transform parent) where T : Component
        {
            T instance = UnityEngine.Object.Instantiate(prefab, parent);
            _resolver.InjectGameObject(instance.gameObject);
            return instance;
        }
        
        public T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            T instance = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            _resolver.InjectGameObject(instance.gameObject);
            return instance;
        }

        private async UniTask<GameObject> LoadPrefab(
            string address,
            CancellationToken ct)
        {
            if (_prefabs.TryGetValue(address, out GameObject? prefab))
            {
                return prefab;
            }
            prefab = await _assetService.LoadAsync<GameObject>(address, ct);
            _prefabs[address] = prefab;
            return prefab;
        }

        public void ClearCache()
        {
            foreach (KeyValuePair<string, GameObject> kv in _prefabs)
            {
                _assetService.Release(kv.Key);
            }
            _prefabs.Clear();
        }

        public void Dispose()
        {
            ClearCache();
        }
    }
}
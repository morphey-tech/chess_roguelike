using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Core.Core.Assets
{
    public interface IAssetService
    {
        /// <summary>
        /// Инициализирует каталог Addressables
        /// </summary>
        UniTask InitializeAsync(CancellationToken cancellationToken = default);

        UniTask<T?> LoadAssetAsync<T>(string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object;
        UniTask<T?> LoadAssetAsync<T>(AssetKey key, CancellationToken cancellationToken = default) where T : UnityEngine.Object;
        UniTask<GameObject?> InstantiateAsync(string address, Vector3 position,
            Quaternion rotation, Transform? parent = null, CancellationToken cancellationToken = default);
        UniTask<GameObject?> InstantiateAsync(AssetKey key, Vector3 position,
            Quaternion rotation, Transform? parent = null, CancellationToken cancellationToken = default);
        GameObject? InstantiateFromPrefab(GameObject prefab, Vector3 position,
            Quaternion rotation, Transform? parent = null);
        GameObject? InstantiatePrefabDirectly(GameObject prefab, Vector3 position,
            Quaternion rotation, Transform? parent = null);
        void Release<T>(T asset) where T : UnityEngine.Object;
        void ReleaseInstance(GameObject instance);
        void ReleaseAll();
        UniTask PreloadAsync(string address, CancellationToken cancellationToken = default);
        UniTask PreloadAsync(string[] addresses, CancellationToken cancellationToken = default);
        bool IsLoaded(string address);
        UniTask<bool> IsRemote(string address, CancellationToken cancellationToken = default);
    }
}



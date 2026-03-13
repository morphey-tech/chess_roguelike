using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.Core.Core.Assets
{
    public interface IAssetService
    {
        UniTask<T> LoadAsync<T>(string address, CancellationToken ct = default)
            where T : Object;

        UniTask<T> InstantiateAsync<T>(
            string address,
            Vector3 position,
            Quaternion rotation,
            Transform? parent = null,
            CancellationToken ct = default)
            where T : Component;

        UniTask<GameObject> InstantiateAsync(
            string address,
            Vector3 position,
            Quaternion rotation,
            Transform? parent = null,
            CancellationToken ct = default);

        T Instantiate<T>(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform? parent = null)
            where T : Component;

        GameObject Instantiate(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform? parent = null);

        void Release(string address);
        void Release(Object obj);

        UniTask PreloadAsync(string address, CancellationToken ct = default);

        void Dispose();
    }
}
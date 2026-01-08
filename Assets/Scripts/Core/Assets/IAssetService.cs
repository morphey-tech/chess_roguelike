using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Core.Assets
{
    public interface IAssetService
    {
        UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object;
        UniTask<T> LoadAssetAsync<T>(AssetKey key) where T : UnityEngine.Object;
        UniTask<GameObject> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null);
        UniTask<GameObject> InstantiateAsync(AssetKey key, Vector3 position, Quaternion rotation, Transform parent = null);
        void Release<T>(T asset) where T : UnityEngine.Object;
        void ReleaseInstance(GameObject instance);
        void ReleaseAll();
        UniTask PreloadAsync(string address);
        UniTask PreloadAsync(string[] addresses);
        bool IsLoaded(string address);
        UniTask<long> GetDownloadSizeAsync(string address);
        UniTask DownloadDependenciesAsync(string address, Action<float> onProgress = null);
    }

    public readonly struct AssetKey
    {
        public string Address { get; }

        public AssetKey(string address)
        {
            Address = address;
        }

        public static implicit operator AssetKey(string address)
        {
            return new AssetKey(address);
        }

        public static implicit operator string(AssetKey key)
        {
            return key.Address;
        }
    }
}



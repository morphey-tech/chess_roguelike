using System.Threading;

namespace Project.Gameplay.Gameplay.UI
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    namespace Project.Gameplay.Gameplay.UI
    {
        public interface IUIAssetService
        {
            UniTask<T> CreateAsync<T>(
                string address,
                Transform parent = null,
                CancellationToken ct = default)
                where T : MonoBehaviour;

            public T Instantiate<T>(T prefab, Transform parent)
                where T : Component;
            
            public T Instantiate<T>(
                T prefab,
                Vector3 position,
                Quaternion rotation,
                Transform? parent = null)
                where T : Component;
            
            void ClearCache();
            void Dispose();
        }
    }
}
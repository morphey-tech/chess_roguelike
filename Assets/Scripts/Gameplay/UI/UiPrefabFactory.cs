using UnityEngine;
using VContainer;

namespace Project.Gameplay.Gameplay.UI
{
    public class UIAssetService : IUIAssetService
    {
        private readonly IObjectResolver _resolver;

        public UIAssetService(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public T Instantiate<T>(T prefab, Transform parent) where T : Component
        {
            T? instance = Object.Instantiate(prefab, parent);
            _resolver.Inject(instance);
            return instance;
        }

        public T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            T? instance = Object.Instantiate(prefab, position, rotation, parent);
            _resolver.Inject(instance);
            return instance;
        }

        public GameObject Instantiate(GameObject prefab, Transform parent)
        {
            GameObject? instance = Object.Instantiate(prefab, parent);
            _resolver.Inject(instance);
            return instance;
        }
    }
}

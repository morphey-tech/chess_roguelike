using Project.Core.Core.Assets;
using UnityEngine;
using VContainer;

namespace Project.Unity.Unity.Prepare
{
    /// <summary>
    /// Только создание инстансов. Без анимаций, без конфигов, без логики.
    /// </summary>
    public sealed class PrepareViewFactory
    {
        private readonly IAssetService _assetService;

        [Inject]
        public PrepareViewFactory(IAssetService assetService)
        {
            _assetService = assetService;
        }

        public GameObject? CreateSlot(GameObject prefab, Vector3 position, Transform parent)
        {
            if (prefab == null)
            {
                return null;
            }
            return _assetService.Instantiate(prefab, position, Quaternion.identity, parent);
        }

        public GameObject? CreateFigure(GameObject controllerPrefab, GameObject viewPrefab, Vector3 position, Transform parent)
        {
            if (controllerPrefab == null || viewPrefab == null)
            {
                return null;
            }
            GameObject root = _assetService.Instantiate(controllerPrefab, position, Quaternion.identity, parent);
            GameObject view = _assetService.Instantiate(viewPrefab, Vector3.zero, Quaternion.identity, root.transform);
            view.transform.localPosition = Vector3.zero;
            view.transform.localRotation = Quaternion.identity;
            return root;
        }
    }
}

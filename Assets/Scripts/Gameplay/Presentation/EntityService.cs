using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.Presentations
{
  public class EntityService
  {
    private readonly EntityInstances _instances;
    private readonly IAssetService _assetService;
    
    [Inject]
    private EntityService(IAssetService assetService,
                          EntityInstances instances)
    {
      _assetService = assetService;
      _instances = instances;
    }
    
    public async UniTask<EntityLink> SpawnView(Entity entity, AssetKey assetKey, Vector3 pos, Quaternion rot, Transform root)
    {
      GameObject? go = await _assetService.InstantiateAsync(assetKey, pos, rot, root);
      await _instances.InitEntity(entity, go);
      return go.GetComponent<EntityLink>();
    }

    /// <summary>
    /// Spawns view from an already loaded prefab (sync instantiate).
    /// </summary>
    public async UniTask<EntityLink?> SpawnViewFromPrefab(Entity entity, GameObject prefab, Vector3 pos, Quaternion rot, Transform root)
    {
      if (prefab == null)
      {
        return null;
      }
      GameObject? go = _assetService.InstantiateFromPrefab(prefab, pos, rot, root);
      if (go == null)
      {
        return null;
      }
      await _instances.InitEntity(entity, go);
      return go.GetComponent<EntityLink>();
    }

    /// <summary>
    /// Instantiates an asset as a child of the given parent transform.
    /// </summary>
    public async UniTask<GameObject?> InstantiateAsChild(AssetKey assetKey, Transform parent)
    {
      GameObject? go = await _assetService.InstantiateAsync(assetKey, Vector3.zero, Quaternion.identity, parent);
      if (go != null)
      {
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
      }
      return go;
    }

    public void Destroy(int id)
    {
      _instances.DestroyView(id);
    }
  }
}

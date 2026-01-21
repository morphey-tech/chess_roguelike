using Cysharp.Threading.Tasks;
using Project.Core.Core.Assets;
using UnityEngine;
using UnityEngine.Playables;

namespace Project.Gameplay.Presentations
{
  public class PresentationManager
  {
    private PresentationManagerInstances Instances { get; set; }
    
    private readonly IAssetService _assetService;
    
    public PresentationManager(IAssetService assetService)
    {
      _assetService = assetService;
      Instances = new PresentationManagerInstances();
    }
    
    public async UniTask<EntityLink> SpawnView(int id, AssetKey assetKey, Vector3 pos, Quaternion rot, Transform root)
    {
      var go = await _assetService.InstantiateAsync(assetKey, pos, rot, root);
      Instances.InitEntity(id, go);
      return go.GetComponent<EntityLink>();
    }

    public void Destroy(int id)
    {
      Instances.DestroyView(id);
    }
  }
}

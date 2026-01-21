#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay.Presentations
{
  public class PresentationManagerInstances : IPresentationsMap
  {
    private readonly Dictionary<int, EntityLink> _presentationsList = new();
    
    private readonly List<IPresenter> _presenterBuffer = new();

    public void InitEntity(int entId, GameObject instance)
    {
      // наверн заменю потом Id на структуру Entity шоб держать там ещё поле Gen для понимания в какой раз этот айдишник юзается
      /*if(!entity.Unpack(_world, out var entId))
        return;*/

      try
      {
        if(!instance.TryGetComponent(out EntityLink link))
          link = instance.AddComponent<EntityLink>();
          
        link.Init(entId, this);
        link.Map = this;
        
        _presenterBuffer.Clear();
        instance.GetComponentsInChildren(includeInactive:true, _presenterBuffer);
        {
          foreach (var presenter in _presenterBuffer)
          {
            try
            {
              ((MonoBehaviour)presenter).enabled = true;
              presenter.Init(link);
            }
            catch (Exception e)
            {
            //  _logger.Exception(e);
            }
          }
        }

        instance.SetActive(true);
        _presentationsList.Add(entId, link);
      }
      catch (Exception e)
      {
      //  _logger.Exception(new Exception($"Exception while spawning presentation: {asset}", e));
      }
    }

    public bool Has(int id) => _presentationsList.ContainsKey(id);
    
    public GameObject? Find(int id) => _presentationsList.TryGetValue(id, out EntityLink link)  ? link.gameObject : null;
    public GameObject Get(int id)
    {
      return Find(id) ?? throw new KeyNotFoundException();
    }

    public void DestroyView(int id)
    {
      var link = _presentationsList[id];
      _presentationsList.Remove(id);
      UnityEngine.Object.Destroy(link.gameObject);
    }
  }
}

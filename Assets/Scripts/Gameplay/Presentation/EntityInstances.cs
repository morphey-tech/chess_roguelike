#nullable enable

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Gameplay.Presentations
{
  public class EntityInstances : IPresentationsMap
  {
    private readonly Dictionary<int, EntityLink> _presentationsList = new();

    public async UniTask InitEntity(Entity entity, GameObject instance)
    {
      try
      {
        if (!instance.TryGetComponent(out EntityLink link))
        {
          link = instance.AddComponent<EntityLink>();
        }

        link.Init(entity, this);
        link.Map = this;
        
        List<IPresenter> presenters = new();
        instance.GetComponentsInChildren(includeInactive:true, presenters);
        
        Debug.Log($"[PresentationManagerInstances] InitEntity {instance.name}: found {presenters.Count} presenters");
        
        foreach (IPresenter presenter in presenters)
        {
          try
          {
            ((MonoBehaviour)presenter).enabled = true;
            await presenter.Init(link);
            Debug.Log($"[PresentationManagerInstances] Initialized presenter {presenter.GetType().Name} on {instance.name}");
          }
          catch (Exception e)
          {
            Debug.LogError($"[PresentationManagerInstances] Failed to init presenter {presenter.GetType().Name}: {e.Message}");
          }
        }

        instance.SetActive(true);
        _presentationsList.Add(entity.Id, link);
      }
      catch (Exception e)
      {
        Debug.LogError($"[PresentationManagerInstances] InitEntity failed: {e.Message}");
      }
    }

    bool IPresentationsMap.Has(int id) => _presentationsList.ContainsKey(id);

    GameObject? IPresentationsMap.Find(int id) => _presentationsList.TryGetValue(id, out EntityLink link)  ? link.gameObject : null;

    GameObject IPresentationsMap.Get(int id)
    {
      return ((IPresentationsMap)this).Find(id) ?? throw new KeyNotFoundException();
    }

    public void DestroyView(int id)
    {
      EntityLink? link = _presentationsList[id];
      _presentationsList.Remove(id);
      UnityEngine.Object.Destroy(link.gameObject);
    }
  }
}

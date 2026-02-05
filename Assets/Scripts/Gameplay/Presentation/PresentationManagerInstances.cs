#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay.Presentations
{
  public class PresentationManagerInstances : IPresentationsMap
  {
    private readonly Dictionary<int, EntityLink> _presentationsList = new();

    public void InitEntity(Entity entity, GameObject instance)
    {
      try
      {
        if(!instance.TryGetComponent(out EntityLink link))
          link = instance.AddComponent<EntityLink>();
          
        link.Init(entity, this);
        link.Map = this;
        
        // Create new list each time to avoid race conditions during parallel spawning
        List<IPresenter> presenters = new List<IPresenter>();
        instance.GetComponentsInChildren(includeInactive:true, presenters);
        
        Debug.Log($"[PresentationManagerInstances] InitEntity {instance.name}: found {presenters.Count} presenters");
        
        foreach (IPresenter presenter in presenters)
        {
          try
          {
            ((MonoBehaviour)presenter).enabled = true;
            presenter.Init(link);
            Debug.Log($"[PresentationManagerInstances] Initialized presenter {presenter.GetType().Name} on {instance.name}");
          }
          catch (Exception e)
          {
            Debug.LogError($"[PresentationManagerInstances] Failed to init presenter: {e.Message}");
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

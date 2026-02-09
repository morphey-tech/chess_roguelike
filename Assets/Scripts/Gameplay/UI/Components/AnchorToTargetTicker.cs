using System.Collections.Generic;
using Project.Core;
using UnityEngine;

namespace Project.Unity.UI.Components
{
  [DefaultExecutionOrder(300)]
  public class AnchorToTargetTicker : LazySingleton<AnchorToTargetTicker>
  {
    private List<AnchorToTarget> _objects = new();

    private void LateUpdate()
    {
      foreach (var target in _objects) 
        target.Tick();
    }

    public void Register(AnchorToTarget obj)
    {
      _objects.Add(obj);
    }

    public void Unregister(AnchorToTarget obj)
    {
      _objects.Remove(obj);
    }
  }
}
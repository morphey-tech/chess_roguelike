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
      for (int i = _objects.Count - 1; i >= 0; i--)
      {
        var target = _objects[i];
        if (target != null && target.isActiveAndEnabled)
          target.Tick();
      }
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
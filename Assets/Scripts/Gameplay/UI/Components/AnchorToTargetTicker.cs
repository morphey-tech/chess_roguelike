using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace Project.Unity.UI.Components
{
    [DefaultExecutionOrder(300)]
    public sealed class AnchorToTargetTicker : IAnchorToTargetTicker, ILateTickable
    {
        private readonly List<AnchorToTarget> _objects = new();

        void ILateTickable.LateTick()
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                AnchorToTarget? target = _objects[i];
                if (target != null && target.isActiveAndEnabled)
                {
                    target.Tick();
                }
            }
        }

        void IAnchorToTargetTicker.Register(AnchorToTarget obj)
        {
            _objects.Add(obj);
        }

        void IAnchorToTargetTicker.Unregister(AnchorToTarget obj)
        {
            _objects.Remove(obj);
        }

    }
}
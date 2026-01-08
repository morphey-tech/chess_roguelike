using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class BoundsExtension
    {
        public static bool IntersectsHard(this Bounds bounds, Bounds another)
        {
            return another.min.x < bounds.max.x && another.max.x > bounds.min.x && another.min.y < bounds.max.y && another.max.y > bounds.min.y
                   && another.min.z < bounds.max.z && another.max.z > bounds.min.z;
        }
    }
}

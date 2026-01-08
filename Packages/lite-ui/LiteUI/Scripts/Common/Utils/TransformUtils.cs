using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Common.Utils
{
    [PublicAPI]
    public static class TransformUtils
    {
        public static Quaternion LookRotation(GameObject source, GameObject target)
        {
            return LookRotation(target.transform, source.transform);
        }

        public static Quaternion LookRotation(Transform source, Transform target)
        {
            return LookRotation(target.position, source.position);
        }

        public static Quaternion LookRotation(Vector3 source, Vector3 target)
        {
            return Quaternion.LookRotation(target - source);
        }
    }
}

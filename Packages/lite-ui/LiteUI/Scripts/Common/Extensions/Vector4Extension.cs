
using UnityEngine;

namespace LiteUI.Common.Extensions
{
    public static class Vector4Extension
    {
        public static Vector4 Modify(this Vector4 point, float? x = null, float? y = null, float? z = null, float? w = null)
        {
            return new Vector4(x ?? point.x, y ?? point.y, z ?? point.z, w ?? point.w);
        }
    }
}

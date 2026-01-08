using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class Vector3Extension
    {
        public static Vector3 Abs(this Vector3 vector3)
        {
            return new Vector3(Math.Abs(vector3.x), Math.Abs(vector3.y), Math.Abs(vector3.z));
        }

        public static Vector3 Ceil(this Vector3 vector3)
        {
            return new Vector3(Mathf.Ceil(vector3.x), Mathf.Ceil(vector3.y), Mathf.Ceil(vector3.z));
        }

        public static Vector3 Round(this Vector3 vector3)
        {
            return new Vector3(Mathf.Round(vector3.x), Mathf.Round(vector3.y), Mathf.Round(vector3.z));
        }

        public static Vector3 Floor(this Vector3 vector3)
        {
            return new Vector3(Mathf.Floor(vector3.x), Mathf.Floor(vector3.y), Mathf.Floor(vector3.z));
        }

        public static Vector3 RoundByBasis(this Vector3 vector3, float basis)
        {
            return new Vector3(Mathf.Round(vector3.x / basis) * basis, Mathf.Round(vector3.y / basis) * basis,
                               Mathf.Round(vector3.z / basis) * basis);
        }

        public static Vector3 Modify(this Vector3 point, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? point.x, y ?? point.y, z ?? point.z);
        }
    }
}

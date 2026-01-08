using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class Vector2Extension
    {
        private const float EPSILON = 0.001f;

        public static Vector2 Abs(this Vector2 vector2)
        {
            return new Vector2(Math.Abs(vector2.x), Math.Abs(vector2.y));
        }

        public static Vector2 Round(this Vector2 vector2)
        {
            return new Vector2(Mathf.Round(vector2.x), Mathf.Round(vector2.y));
        }

        public static bool FuzzyEquals(this Vector2 source, Vector2 dest)
        {
            return Vector2.SqrMagnitude(dest - source) < EPSILON;
        }

        public static Vector2 RoundByBasis(this Vector2 vector2, float basis)
        {
            return new Vector2(Mathf.Round(vector2.x / basis) * basis, Mathf.Round(vector2.y / basis) * basis);
        }

        public static Vector2 RotateByAngle(this Vector2 vector2, float angle)
        {
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            Vector2 result = new() {
                    x = cos * vector2.x - sin * vector2.y,
                    y = sin * vector2.x + cos * vector2.y
            };
            return result;
        }
        
        public static Vector2 Modify(this Vector2 point, float? x = null, float? y = null)
        {
            return new Vector2(x ?? point.x, y ?? point.y);
        }
    }
}

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class QuaternionExtension
    {
#pragma warning disable S109
        public static Quaternion FromAngleY(float angle)
        {
            return new Quaternion(0.0f, 1.0f * (float) Math.Sin(angle / 2), 0.0f, (float) Math.Cos(angle / 2));
        }

        public static float ToAngleX(this Quaternion quaternion)
        {
            return (float) Math.Atan2(2.0f * quaternion.x * quaternion.w - 2.0f * quaternion.y * quaternion.z,
                                      1.0f - 2.0f * quaternion.x * quaternion.x - 2.0f * quaternion.z * quaternion.z);
        }

        public static float ToAngleY(this Quaternion quaternion)
        {
            return (float) Math.Atan2(2.0f * quaternion.y * quaternion.w - 2.0f * quaternion.x * quaternion.z,
                                      1.0f - 2.0f * quaternion.y * quaternion.y - 2.0f * quaternion.z * quaternion.z);
        }

        public static float ToAngleZ(this Quaternion quaternion)
        {
            return (float) Math.Asin(2.0f * quaternion.x * quaternion.y + 2.0f * quaternion.z * quaternion.w);
        }

        // Источник: https://answers.unity.com/questions/26783/how-to-get-the-signed-angle-between-two-quaternion.html
        public static float GetSignedAngleXZ(this Quaternion from, Quaternion to)
        {
            // get a "forward vector" for each rotation
            Vector3 forwardA = from * Vector3.forward;
            Vector3 forwardB = to * Vector3.forward;

            // get a numeric angle for each vector, on the X-Z plane (relative to world forward)
            float angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
            float angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;

            // get the signed difference in these angles
            float angleDiff = Mathf.DeltaAngle(angleA, angleB);
            return angleDiff;
        }
#pragma warning restore S109
    }
}

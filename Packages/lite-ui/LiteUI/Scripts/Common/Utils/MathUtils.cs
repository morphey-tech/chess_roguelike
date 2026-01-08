using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LiteUI.Common.Utils
{
    [PublicAPI]
    public static class MathUtils
    {
        public const float EPSILON = 0.001f;
        private const float PI = 3.1415926f;
        private const float PI2 = PI * 2.0f;
        private const float PI_IN_DEGREE = 180;
        private const float PI2_IN_DEGREE = PI_IN_DEGREE * 2;

        private const float FACTOR = 4f;
        
        public static Vector3 Direction(Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }

        public static Vector3 Direction2D(Vector3 from, Vector3 to)
        {
            Vector3 direction = Direction(from, to);
            direction.y = 0;
            return direction;
        }

        public static float ClampWithStep(float value, float min, float max, float step)
        {
            while (value < min && !IsFloatEquals(value, min)) {
                value += step;
            }

            while (value > max && !IsFloatEquals(value, max)) {
                value -= step;
            }

            return value;
        }

        public static Vector3 ClampWithStep(Vector3 point, Vector3 minPoint, Vector3 maxPoint, float step)
        {
            Vector3 result = point;
            result.x = ClampWithStep(point.x, minPoint.x, maxPoint.x, step);
            result.y = ClampWithStep(point.y, minPoint.y, maxPoint.y, step);
            result.z = ClampWithStep(point.z, minPoint.z, maxPoint.z, step);
            return result;
        }

        public static Vector3 RoundByBasis(Vector3 vector3, float basis)
        {
            return new Vector3(Mathf.Round(vector3.x / basis) * basis, Mathf.Round(vector3.y / basis) * basis,
                               Mathf.Round(vector3.z / basis) * basis);
        }
        
        public static int FloorToInt(float value, float epsilon = EPSILON)
        {
            int rounded = Mathf.RoundToInt(value);
            if (IsDoubleEquals(rounded, value, epsilon)) {
                return rounded;
            }
            return Mathf.FloorToInt(value);
        }
        
        public static long FloorToInt(double value, float epsilon = EPSILON)
        {
            long rounded = Convert.ToInt64(value);
            if (IsDoubleEquals(rounded, value, epsilon)) {
                return rounded;
            }
            return (long) Math.Floor(value);
        }

        public static bool IsFloatEquals(float value1, float value2, float epsilon = EPSILON)
        {
            return Math.Abs(value1 - value2) < epsilon;
        }

        public static bool IsDoubleEquals(double value1, double value2, float epsilon = EPSILON)
        {
            return Math.Abs(value1 - value2) < epsilon;
        }
        
        public static bool IsVector2Equals(Vector2 value1, Vector2 value2, float epsilon = EPSILON)
        {
            return Vector2.Distance(value1, value2) < epsilon;
        }

        public static bool IsVector3Equals(Vector3 value1, Vector3 value2, float epsilon = EPSILON)
        {
            return Mathf.Abs(Vector3.Distance(value1, value2)) < epsilon;
        }

        public static bool IsQuaternionEquals(Quaternion value1, Quaternion value2, float epsilon = EPSILON * 100)
        {
            float angle = AngleToInterval0To360(Quaternion.Angle(value1, value2));
            return angle < epsilon || angle > PI2_IN_DEGREE - epsilon;
        }


        public static float AngleToRadians(float angleInDegree, bool isInterval0To2Pi = false)
        {
            float angleInPi = angleInDegree / PI_IN_DEGREE * PI;
            if (isInterval0To2Pi) {
                angleInPi = AngleToInterval0To2Pi(angleInPi);
            }

            return angleInPi;
        }

        public static float RadiansToDegree(float angleInRadians, bool isInterval0To360 = false)
        {
            float angleInDegree = angleInRadians * PI_IN_DEGREE / PI;
            if (isInterval0To360) {
                angleInDegree = AngleToInterval0To360(angleInDegree);
            }

            return angleInDegree;
        }

        public static float AngleToInterval0To2Pi(float angleInRadians)
        {
            float normalizedAngle = angleInRadians;
            while (normalizedAngle > PI2) {
                normalizedAngle -= PI2;
            }

            while (normalizedAngle < 0) {
                normalizedAngle += PI2;
            }

            return normalizedAngle;
        }

        public static float AngleToInterval0To360(float angleInDegrees)
        {
            float normalizedAngle = angleInDegrees;
            while (normalizedAngle > PI2_IN_DEGREE) {
                normalizedAngle -= PI2_IN_DEGREE;
            }

            while (normalizedAngle < 0) {
                normalizedAngle += PI2_IN_DEGREE;
            }

            return normalizedAngle;
        }

        public static float AngleToInterval180To180(float angleInDegrees)
        {
            float normalizedAngle = angleInDegrees;
            while (normalizedAngle > PI_IN_DEGREE) {
                normalizedAngle -= PI2_IN_DEGREE;
            }

            while (normalizedAngle < -PI_IN_DEGREE) {
                normalizedAngle += PI2_IN_DEGREE;
            }

            return normalizedAngle;
        }

#pragma warning disable S109
        public static Vector3 CalcParabola(Vector3 start, Vector3 end, float height, float t)
        {
            float Func(float x) => -FACTOR * height * Mathf.Pow(x, 2) + FACTOR * height * x;

            Vector3 mid = Vector3.Lerp(start, end, t);
            return new Vector3(mid.x, Func(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
        }

        public static Vector2 CalcParabola(Vector2 start, Vector2 end, float height, float t)
        {
            float Func(float x) => -FACTOR * height * Mathf.Pow(x, 2) + FACTOR * height * x;

            Vector3 mid = Vector2.Lerp(start, end, t);
            return new Vector2(mid.x, Func(t) + Mathf.Lerp(start.y, end.y, t));
        }
#pragma warning restore S109
    }
}

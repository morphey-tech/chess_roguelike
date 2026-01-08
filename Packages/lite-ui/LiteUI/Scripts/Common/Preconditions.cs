using System;

namespace LiteUI.Common
{
    public static class Preconditions
    {
        public static T CheckNotNull<T>(T value, string paramName = null) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(paramName ?? "value");
            return value;
        }

        public static void CheckArgument(bool condition, string message = null)
        {
            if (!condition)
                throw new ArgumentException(message ?? "Argument check failed");
        }

        public static void CheckState(bool condition, string message = null)
        {
            if (!condition)
                throw new InvalidOperationException(message ?? "State check failed");
        }
    }
}


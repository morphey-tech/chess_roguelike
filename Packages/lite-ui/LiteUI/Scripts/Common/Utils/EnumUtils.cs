using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace LiteUI.Common.Utils
{
    [PublicAPI]
    public static class EnumUtils
    {
        public static IEnumerable<T> Values<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}

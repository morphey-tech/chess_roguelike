using System.Collections.Generic;

namespace LiteUI.Common.Utils
{
    public static class ListUtils
    {
        public static List<T> Create<T>(params T[] objs)
        {
            return new List<T>(objs);
        }
    }
}

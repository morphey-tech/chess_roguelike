using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteUI.Common.Extensions
{
    public static class CollectionExtension
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int currentIndex = list.Count;

            while (currentIndex > 1) {
                int swapIndex = UnityEngine.Random.Range(0, currentIndex--);
                (list[swapIndex], list[currentIndex]) = (list[currentIndex], list[swapIndex]);
            }
        }

        public static T RandomItem<T>(this IList<T> list)
        {
            return list.ElementAt(UnityEngine.Random.Range(0, list.Count));
        }
        
        public static bool IsEmpty<T>(this IList<T> collection)
        {
            return collection.Count == 0;
        }
        
        public static bool Contains<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
                where T : class
        {
            return collection.FirstOrDefault(predicate) != null;
        }
    }
}

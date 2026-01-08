using System.Collections.Generic;
using JetBrains.Annotations;

namespace LiteUI.Common.Extensions
{
    [PublicAPI]
    public static class DictionaryExtension
    {
        public static TV? GetOrDefault<TK, TV>(this Dictionary<TK, TV> target, TK key, TV? defaultValue = null)
                where TV : class
        {
            return target.ContainsKey(key) ? target[key] : defaultValue;
        }

        public static TV? FindValueForMaxKeyNotGreater<TV>(this IDictionary<float, TV> target, float value)
        {
            float maxKey = float.MinValue;
            foreach (float key in target.Keys) {
                if (key > value) {
                    continue;
                }
                if (maxKey > key) {
                    continue;
                }
                maxKey = key;
            }
            return target[maxKey];
        }
    }
}

#nullable enable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace LiteUI.Common.Collections
{
    [PublicAPI]
    public class CachedDictionary<TKey, TValue>
            where TKey : class, IKeyCacheId
            where TValue : class
    {
        private readonly Dictionary<int, TValue> _items = new();
        private readonly Dictionary<TKey, int> _keys = new();
        private int _keyCacheId;

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key)) {
                throw new ArgumentException($"Duplicate key {key}");
            }
            this[key] = value;
        }
        
        public TValue? Get(TKey key)
        {
            int keyCacheId = key.KeyCacheId;
            if (keyCacheId <= 0) {
                keyCacheId = CacheKey(key);
            }
            return _items.ContainsKey(keyCacheId) ? _items[keyCacheId] : null;
        }
        
        public TValue Require(TKey key)
        {
            TValue? value = Get(key);
            if (value == null) {
                throw new NullReferenceException($"Key not found in dictionary {key}");
            }
            return value;
        }

        public bool ContainsKey(TKey key)
        {
            return Get(key) != null;
        }
        
        public TValue this[TKey key]
        {
            get => Require(key);
            set
            {
                int keyCacheId = key.KeyCacheId;
                if (keyCacheId <= 0) {
                    keyCacheId = CacheKey(key);
                }
                _items[keyCacheId] = value;
            }
        }

        public void Clear()
        {
            _items.Clear();
        }

        private int CacheKey(TKey key)
        {
            if (_keys.ContainsKey(key)) {
                int keyCacheId = _keys[key];
                key.KeyCacheId = keyCacheId;
                return keyCacheId;
            }
            ++_keyCacheId;
            key.KeyCacheId = _keyCacheId;
            _keys[key] = _keyCacheId;
            return _keyCacheId;
        }
    }
}

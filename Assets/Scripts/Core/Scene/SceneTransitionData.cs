using System;
using System.Collections.Generic;

namespace Project.Core.Scene
{
    /// <summary>
    /// Данные для передачи между сценами.
    /// Создаётся перед переходом, доступны в новой сцене после загрузки.
    /// </summary>
    public sealed class SceneTransitionData
    {
        private readonly Dictionary<Type, object> _data = new();

        public string SpawnPointId { get; set; } = string.Empty;

        public void Set<T>(T value) where T : notnull
        {
            _data[typeof(T)] = value;
        }

        public bool TryGet<T>(out T value)
        {
            if (_data.TryGetValue(typeof(T), out object obj) && obj is T typed)
            {
                value = typed;
                return true;
            }

            value = default!;
            return false;
        }

        public T GetOrDefault<T>(T defaultValue)
        {
            return TryGet(out T value) ? value : defaultValue;
        }

        public bool Has<T>()
        {
            return _data.ContainsKey(typeof(T));
        }

        public void Clear()
        {
            _data.Clear();
            SpawnPointId = string.Empty;
        }
    }

}


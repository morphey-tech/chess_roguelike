using System;

namespace Project.Gameplay.Gameplay.Figures
{
    public class FlatModifier<T> : StatModifier<T>
    {
        private readonly T _value;
        private readonly Func<T, T, T> _addFunc;

        public FlatModifier(string id, T value, int priority = 0, int duration = -1, bool stackable = true)
            : base(id, priority, duration, stackable)
        {
            _value = value;

            if (typeof(T) == typeof(float))
            {
                _addFunc = (a, b) => (T)(object)((float)(object)a + (float)(object)b);
            }
            else if (typeof(T) == typeof(double))
            {
                _addFunc = (a, b) => (T)(object)((double)(object)a + (double)(object)b);
            }
            else if (typeof(T) == typeof(int))
            {
                _addFunc = (a, b) => (T)(object)((int)(object)a + (int)(object)b);
            }
            else
            {
                throw new NotSupportedException($"FlatModifier not supported for type {typeof(T)}");
            }
        }

        public override T Apply(T value) => _addFunc(value, _value);
    }
}

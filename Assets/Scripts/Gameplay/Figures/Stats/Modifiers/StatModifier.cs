using System;

namespace Project.Gameplay.Gameplay.Figures
{
    public abstract class StatModifier<T> : IStatModifier<T>
    {
        public string Id { get; }
        public int Priority { get; }
        public int Duration { get; private set; }
        public bool Stackable { get; }
        public bool IsExpired => Duration == 0;
        public ModifierSourceContext SourceContext { get; }

        protected StatModifier(string id, int priority, int duration, bool stackable, ModifierSourceContext sourceContext = ModifierSourceContext.Passive)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Priority = priority;
            Duration = duration;
            Stackable = stackable;
            SourceContext = sourceContext;
        }

        public abstract T Apply(T value);

        public virtual void Tick()
        {
            if (Duration > 0)
                Duration--;
        }
    }
}

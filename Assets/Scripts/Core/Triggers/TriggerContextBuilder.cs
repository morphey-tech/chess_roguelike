namespace Project.Core.Core.Triggers
{
    /// <summary>
    /// Fluent builder for creating TriggerContext instances.
    /// </summary>
    public sealed class TriggerContextBuilder
    {
        private readonly TriggerContext _context = new();

        public TriggerContextBuilder WithType(TriggerType type)
        {
            _context.Type = type;
            return this;
        }

        public TriggerContextBuilder WithPhase(TriggerPhase phase)
        {
            _context.Phase = phase;
            return this;
        }

        public TriggerContextBuilder WithSource(TriggerSource sourceType, object? sourceObject = null)
        {
            _context.SourceType = sourceType;
            _context.SourceObject = sourceObject;
            return this;
        }

        public TriggerContextBuilder WithActor(ITriggerEntity actor)
        {
            _context.Actor = actor;
            return this;
        }

        public TriggerContextBuilder WithTarget(ITriggerEntity target)
        {
            _context.Target = target;
            return this;
        }

        public TriggerContextBuilder WithValue(int value)
        {
            _context.BaseValue = value;
            _context.CurrentValue = value;
            return this;
        }

        public TriggerContextBuilder WithStackCount(int count)
        {
            _context.StackCount = count;
            return this;
        }

        public TriggerContextBuilder WithData(object? data)
        {
            _context.Data = data;
            return this;
        }

        /// <summary>
        /// Set type-safe custom data.
        /// Key is derived from type T, no string keys.
        /// </summary>
        public TriggerContextBuilder WithCustomData<T>(T value) where T : class
        {
            _context.SetCustomData(value);
            return this;
        }

        public TriggerContext Build()
        {
            return _context;
        }

        public TriggerResult Execute(TriggerService triggerService)
        {
            return triggerService.Execute(_context.Type, _context.Phase, _context);
        }

        public static TriggerContext Create()
        {
            return new TriggerContextBuilder().Build();
        }

        public static TriggerContextBuilder For(TriggerType type)
        {
            return new TriggerContextBuilder().WithType(type);
        }

        public static TriggerContextBuilder For(TriggerType type, TriggerPhase phase)
        {
            return new TriggerContextBuilder().WithType(type).WithPhase(phase);
        }

        public static TriggerContextBuilder For(TriggerType type, TriggerPhase phase, TriggerSource source)
        {
            return new TriggerContextBuilder().WithType(type).WithPhase(phase).WithSource(source);
        }
    }
}

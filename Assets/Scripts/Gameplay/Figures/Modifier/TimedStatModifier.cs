namespace Project.Gameplay.Gameplay.Modifier
{
    /// <summary>Wraps a stat modifier and expires after a number of ticks (e.g. turns).</summary>
    public sealed class TimedStatModifier : IStatModifier<float>, ITimedModifier
    {
        private readonly IStatModifier<float> _inner;
        private int _remainingTurns;

        public int Priority => _inner.Priority;
        public bool IsExpired => _remainingTurns <= 0;

        public TimedStatModifier(IStatModifier<float> inner, int durationTurns)
        {
            _inner = inner;
            _remainingTurns = durationTurns;
        }

        public float Apply(float value) => _inner.Apply(value);

        public void Tick()
        {
            if (_remainingTurns > 0)
                _remainingTurns--;
        }
    }
}

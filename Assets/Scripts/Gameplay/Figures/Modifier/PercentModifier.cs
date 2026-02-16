namespace Project.Gameplay.Gameplay.Modifier
{
    /// <summary>Multiplies value by (1 + percent/100). E.g. percent=10 => +10%.</summary>
    public sealed class PercentModifier : IStatModifier<float>
    {
        public int Priority { get; }
        private readonly float _percent;

        public PercentModifier(float percent, int priority = 0)
        {
            _percent = percent;
            Priority = priority;
        }

        public float Apply(float value) => value * (1f + _percent / 100f);
    }
}

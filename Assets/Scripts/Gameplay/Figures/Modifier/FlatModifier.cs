namespace Project.Gameplay.Gameplay.Modifier
{
    public sealed class FlatModifier : IStatModifier<float>
    {
        public int Priority { get; }
        private readonly float _value;

        public FlatModifier(float value, int priority = 0)
        {
            _value = value;
            Priority = priority;
        }

        public float Apply(float value) => value + _value;
    }
}

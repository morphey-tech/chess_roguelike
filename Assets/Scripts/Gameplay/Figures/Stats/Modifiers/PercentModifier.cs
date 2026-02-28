namespace Project.Gameplay.Gameplay.Figures
{
    public class PercentModifier : StatModifier<float>
    {
        private readonly float _percentMultiplier;

        public PercentModifier(string id, float percent, int priority = 100, int duration = -1, bool stackable = true, ModifierSourceContext sourceContext = ModifierSourceContext.Passive)
            : base(id, priority, duration, stackable, sourceContext)
        {
            _percentMultiplier = 1f + (percent / 100f);
        }

        public override float Apply(float value) => value * _percentMultiplier;
    }
}

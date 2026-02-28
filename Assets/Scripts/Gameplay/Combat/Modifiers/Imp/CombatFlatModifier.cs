using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Imp
{
    public sealed class CombatFlatModifier : FlatModifier<float>
    {
        public CombatFlatModifier(string id, float value, int priority = 0, int duration = -1, bool stackable = true, ModifierSourceContext sourceContext = ModifierSourceContext.Passive)
            : base(id, value, priority, duration, stackable, sourceContext)
        {
        }
    }
}

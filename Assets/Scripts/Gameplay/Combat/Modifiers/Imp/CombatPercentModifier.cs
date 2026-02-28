using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Imp
{
    public sealed class CombatPercentModifier : PercentModifier
    {
        public CombatPercentModifier(string id, float percent, int priority = 100, int duration = -1, bool stackable = true, ModifierSourceContext sourceContext = ModifierSourceContext.Passive)
            : base(id, percent, priority, duration, stackable, sourceContext)
        {
        }
    }
}

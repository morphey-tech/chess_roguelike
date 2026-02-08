using System.Collections.Generic;

namespace Project.Gameplay.Gameplay.Combat.Visual
{
    public interface ICombatVisualPlanner
    {
        VisualCombatPlan Build(CombatResult result, IReadOnlyList<ICombatVisualEvent> visualEvents);
    }
}

using System.Collections.Generic;
using Project.Gameplay.Gameplay.Visual;
using Project.Gameplay.Gameplay.Visual.Commands;

namespace Project.Gameplay.Gameplay.Combat.Visual
{
    public sealed class VisualCombatPlan
    {
        public IReadOnlyList<IVisualCommand> Commands { get; }

        public VisualCombatPlan(IReadOnlyList<IVisualCommand> commands)
        {
            Commands = commands;
        }
    }
}

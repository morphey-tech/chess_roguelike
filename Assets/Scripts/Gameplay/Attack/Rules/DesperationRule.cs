using System.Linq;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Passives;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Desperation rule: if figure has no allies nearby, can attack in any adjacent direction (8 directions).
    /// This rule overrides pawn's diagonal-forward restriction when desperation condition is met.
    /// Runs BEFORE RangeRule (negative priority) to allow desperation attacks.
    /// </summary>
    public sealed class DesperationRule : IAttackRule
    {
        public int Priority => -1;

        public bool Validate(AttackRuleContext context)
        {
            Figure attacker = context.Attacker;

            if (!attacker.BasePassives.Any(p => p is DesperationPassive))
            {
                return true; // Let other rules handle
            }

            BoardGrid grid = context.Grid;
            int allies = grid.CountAlliesAround(attacker);

            if (allies == 0)
            {
                return context.From.IsAdjacentTo(context.To);
            }
            return true;
        }
    }
}

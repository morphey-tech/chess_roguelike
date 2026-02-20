using System;
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
            var attacker = context.Attacker;

            // Check if attacker has Desperation passive
            if (!attacker.BasePassives.Any(p => p is DesperationPassive))
                return true; // Let other rules handle

            var grid = context.Grid;
            int allies = grid.CountAlliesAround(attacker);

            // If no allies nearby, allow attack in any adjacent cell (8 directions)
            if (allies == 0)
            {
                // Check if target is adjacent (any direction including diagonals)
                return IsAdjacent(context.From, context.To);
            }

            // If allies exist, use normal attack rules (let other rules handle it)
            return true;
        }

        private bool IsAdjacent(GridPosition from, GridPosition to)
        {
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            // Adjacent means max(|dr|, |dc|) == 1 and not both zero (8 directions)
            return Math.Max(Math.Abs(dr), Math.Abs(dc)) == 1;
        }
    }
}

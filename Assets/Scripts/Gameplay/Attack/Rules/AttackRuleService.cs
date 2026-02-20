using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using VContainer;

namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Service that manages and executes attack rules.
    /// </summary>
    public sealed class AttackRuleService
    {
        private readonly IReadOnlyList<IAttackRule> _rules;

        [Inject]
        public AttackRuleService(IEnumerable<IAttackRule> rules)
        {
            _rules = rules.OrderBy(r => r.Priority).ToList();
        }

        /// <summary>
        /// Validates if attack is allowed based on all rules.
        /// </summary>
        public bool CanAttack(AttackRuleContext context)
        {
            foreach (var rule in _rules)
            {
                if (!rule.Validate(context))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets all valid targets based on rules.
        /// </summary>
        public IReadOnlyList<GridPosition> GetValidTargets(
            Figure attacker,
            GridPosition from,
            BoardGrid grid)
        {
            var enemyTeam = attacker.Team == Team.Player ? Team.Enemy : Team.Player;
            var result = new List<GridPosition>();

            foreach (var enemy in grid.GetFiguresByTeam(enemyTeam))
            {
                var cell = grid.FindFigure(enemy);
                if (cell == null)
                    continue;

                var context = new AttackRuleContext(
                    attacker,
                    enemy,
                    from,
                    cell.Position,
                    grid);

                if (CanAttack(context))
                    result.Add(cell.Position);
            }

            return result;
        }
    }
}

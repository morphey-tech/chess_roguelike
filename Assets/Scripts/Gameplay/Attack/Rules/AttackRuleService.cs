using Project.Core.Core.Combat;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using VContainer;

namespace Project.Gameplay.Gameplay.Attack.Rules
{
    public sealed class AttackRuleService
    {
        private readonly IReadOnlyList<IAttackRule> _rules;

        [Inject]
        private AttackRuleService(IEnumerable<IAttackRule> rules)
        {
            _rules = rules.OrderBy(r => r.Priority).ToList();
        }

        /// <summary>
        /// Validates if attack is allowed based on all rules.
        /// </summary>
        public bool CanAttack(AttackRuleContext context)
        {
            foreach (IAttackRule? rule in _rules)
            {
                if (!rule.Validate(context))
                    return false;
            }
            return true;
        }

        public IReadOnlyList<GridPosition> GetValidTargets(
            Figure attacker,
            GridPosition from,
            BoardGrid grid)
        {
            Team enemyTeam = attacker.Team == Team.Player ? Team.Enemy : Team.Player;
            List<GridPosition> result = new();

            foreach (Figure? enemy in grid.GetFiguresByTeam(enemyTeam))
            {
                BoardCell? cell = grid.FindFigure(enemy);
                if (cell == null)
                {
                    continue;
                }

                AttackRuleContext context = new(
                    attacker,
                    enemy,
                    from,
                    cell.Position,
                    grid);

                if (CanAttack(context))
                {
                    result.Add(cell.Position);
                }
            }
            return result;
        }
    }
}

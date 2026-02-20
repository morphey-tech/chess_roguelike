using System.Linq;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Passives;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Taunt rule - if any provokers are in range, only provokers can be attacked.
    /// </summary>
    public sealed class TauntRule : IAttackRule
    {
        public int Priority => 100;

        public bool Validate(AttackRuleContext context)
        {
            var attacker = context.Attacker;
            var grid = context.Grid;

            var enemyTeam = attacker.Team == Team.Player
                ? Team.Enemy
                : Team.Player;

            var enemiesInRange = grid.GetFiguresByTeam(enemyTeam)
                .Where(e =>
                {
                    var cell = grid.FindFigure(e);
                    if (cell == null)
                        return false;

                    return IsInRange(attacker, context.From, cell.Position, grid);
                })
                .ToList();

            var tauntTargets = enemiesInRange
                .Where(e => e.BasePassives.Any(p => p is ProvocationPassive))
                .ToList();

            if (tauntTargets.Count == 0)
                return true;

            return tauntTargets.Contains(context.Target);
        }

        private bool IsInRange(
            Figure attacker,
            GridPosition from,
            GridPosition to,
            BoardGrid grid)
        {
            var strategy = AttackStrategyFactory.Instance.Get(attacker.AttackId);
            return strategy.CanAttack(attacker, from, to, grid);
        }
    }
}

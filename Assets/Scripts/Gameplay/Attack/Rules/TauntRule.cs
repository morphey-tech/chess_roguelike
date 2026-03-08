using Project.Core.Core.Combat;
using System.Collections.Generic;
using System.Linq;
using Project.Gameplay.Gameplay.Combat.Passives;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using VContainer;

namespace Project.Gameplay.Gameplay.Attack.Rules
{
    /// <summary>
    /// Taunt rule - if any provokers are in range, only provokers can be attacked.
    /// </summary>
    public sealed class TauntRule : IAttackRule
    {
        private readonly AttackStrategyFactory _strategyFactory;

        public int Priority => 100;

        [Inject]
        private TauntRule(AttackStrategyFactory strategyFactory)
        {
            _strategyFactory = strategyFactory;
        }

        public bool Validate(AttackRuleContext context)
        {
            Figure attacker = context.Attacker;
            BoardGrid grid = context.Grid;

            Team enemyTeam = attacker.Team == Team.Player
                ? Team.Enemy
                : Team.Player;

            IAttackStrategy strategy = _strategyFactory.Get(attacker.AttackId);

            List<Figure> enemiesInRange = grid.GetFiguresByTeam(enemyTeam)
                .Where(e =>
                {
                    var cell = grid.FindFigure(e);
                    if (cell == null)
                        return false;

                    return strategy.CanAttack(attacker, context.From, cell.Position, grid);
                })
                .ToList();

            List<Figure> tauntTargets = enemiesInRange
                .Where(e => e.BasePassives.Any(p => p is ProvocationPassive))
                .ToList();

            return tauntTargets.Count == 0 || tauntTargets.Contains(context.Target);
        }
    }
}

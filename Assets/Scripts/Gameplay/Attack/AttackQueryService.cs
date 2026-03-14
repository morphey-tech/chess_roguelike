using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack.Strategies;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.Gameplay.Attack
{
    public sealed class AttackQueryService : IAttackQueryService
    {
        private readonly AttackStrategyFactory _strategyFactory;
        private readonly ILogger<AttackQueryService> _logger;

        [Inject]
        private AttackQueryService(AttackStrategyFactory strategyFactory, ILogService logService)
        {
            _strategyFactory = strategyFactory;
            _logger = logService.CreateLogger<AttackQueryService>();
        }

        public IReadOnlyCollection<GridPosition> GetTargets(Figure? actor, GridPosition from, BoardGrid? grid)
        {
            if (actor == null || grid == null)
            {
                return new List<GridPosition>();
            }

            List<GridPosition> targets = new List<GridPosition>();
            IAttackStrategy strategy = _strategyFactory.Get(actor.AttackId);
            foreach (Figure enemy in grid.GetFiguresByTeam(actor.Team == Team.Player ? Team.Enemy : Team.Player))
            {
                BoardCell? cell = grid.FindFigure(enemy);
                if (cell != null && strategy.CanAttack(actor, from, cell.Position, grid))
                {
                    targets.Add(cell.Position);
                }
            }

            int baseCount = targets.Count;
            AttackContext context = new AttackContext(actor, from, grid);
            foreach (IAttackFilter passive in actor.BasePassives.OfType<IAttackFilter>())
            {
                passive.FilterTargets(targets, context);
            }

            _logger.Debug($"GetTargets: {actor.Id} from=({from.Row},{from.Column}) base={baseCount} filters={targets.Count - baseCount} total={targets.Count}");
            return targets;
        }

        public bool CanAttackCell(Figure attacker, GridPosition from, GridPosition targetCell, BoardGrid grid)
        {
            if (from == targetCell)
            {
                return false;
            }

            if (!grid.IsInside(targetCell))
            {
                return false;
            }

            BoardCell? cell = grid.GetBoardCell(targetCell);
            if (cell.OccupiedBy != null && cell.OccupiedBy.Team == attacker.Team)
            {
                return false;
            }

            foreach (IAttackRangeModifier passive in attacker.BasePassives.OfType<IAttackRangeModifier>())
            {
                if (passive.CanAttackCell(attacker, from, targetCell, grid))
                {
                    _logger.Debug($"CanAttackCell: {attacker.Id} -> ({targetCell.Row},{targetCell.Column}) = true (via {passive.GetType().Name})");
                    return true;
                }
            }

            IAttackStrategy strategy = _strategyFactory.Get(attacker.AttackId);
            bool result = strategy.CanAttackPosition(attacker, from, targetCell, grid);
            _logger.Debug($"CanAttackCell: {attacker.Id} -> ({targetCell.Row},{targetCell.Column}) = {result} (via {strategy.Id})");
            return result;
        }

        public IAttackStrategy GetStrategy(string attackId)
        {
            return _strategyFactory.Get(attackId);
        }
    }
}

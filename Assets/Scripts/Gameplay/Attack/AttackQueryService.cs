using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using VContainer;

namespace Project.Gameplay.Gameplay.Attack
{
    public sealed class AttackQueryService : IAttackQueryService
    {
        private readonly AttackStrategyFactory _attackFactory;
        private readonly IAttackResolver _attackResolver;

        [Inject]
        public AttackQueryService(AttackStrategyFactory attackFactory, IAttackResolver attackResolver)
        {
            _attackFactory = attackFactory;
            _attackResolver = attackResolver;
        }

        public IReadOnlyCollection<GridPosition> GetTargets(Figure? actor, GridPosition from, BoardGrid? grid)
        {
            HashSet<GridPosition> result = new HashSet<GridPosition>();
            if (actor == null || grid == null)
                return result;

            Team enemyTeam = actor.Team == Team.Player ? Team.Enemy : Team.Player;
            foreach (Figure enemy in grid.GetFiguresByTeam(enemyTeam))
            {
                BoardCell? cell = grid.FindFigure(enemy);
                if (cell == null)
                    continue;

                if (CanAttack(actor, from, cell.Position, grid))
                    result.Add(cell.Position);
            }

            return result;
        }

        private bool CanAttack(Figure actor, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (actor.AttackId == "profiled")
                return _attackResolver.Resolve(actor, from, to, grid) != null;

            IAttackStrategy strategy = _attackFactory.Get(actor.AttackId);
            return strategy.CanAttack(actor, from, to, grid);
        }
    }
}

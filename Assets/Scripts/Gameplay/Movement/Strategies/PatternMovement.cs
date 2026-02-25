using System.Collections.Generic;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Movement.Extensions;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Universal pattern-based movement strategy.
    /// Configures movement via MovementPatternConfig.
    /// </summary>
    public sealed class PatternMovement : IMovementStrategy
    {
        public string Id { get; }

        private readonly MovementPatternConfig _pattern;

        public PatternMovement(MovementPatternConfig pattern)
        {
            Id = pattern.Id;
            _pattern = pattern;
        }

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            (int dr, int dc)[] directions = _pattern.Directions.GetDirections();

            foreach ((int dr, int dc) in directions)
            {
                int maxDist = _pattern.MaxDistance;

                if (_pattern.ExtraStraightDistance > 0 && (dr == 0 || dc == 0))
                {
                    maxDist = System.Math.Max(maxDist, _pattern.ExtraStraightDistance);
                }

                for (int i = _pattern.MinDistance; i <= maxDist; i++)
                {
                    GridPosition to = new(from.Row + dr * i, from.Column + dc * i);
                    if (!grid.IsInside(to))
                    {
                        break;
                    }

                    BoardCell cell = grid.GetBoardCell(to);
                    MovementStrategyResult result = new(figure, to, true, cell.OccupiedBy);

                    if (!result.CanOccupy())
                    {
                        if (_pattern.JumpOver)
                        {
                            continue;
                        }

                        break;
                    }

                    yield return result;
                }
            }
        }

        public MovementStrategyResult GetFor(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            if (!_pattern.Directions.IsDirection(dr, dc))
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            int distance = (dr, dc).GetMovementDistance();
            if (distance < _pattern.MinDistance || distance > _pattern.MaxDistance)
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            if (!_pattern.JumpOver && !grid.IsPathClear(from, to))
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            BoardCell cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }
    }
}

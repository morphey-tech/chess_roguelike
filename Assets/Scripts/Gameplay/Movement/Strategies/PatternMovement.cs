using System;
using System.Collections.Generic;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
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
            var directions = GetDirections(_pattern.Directions);

            foreach ((int dr, int dc) in directions)
            {
                int maxDist = _pattern.MaxDistance;
                
                // Extra straight distance for special movements
                if (_pattern.ExtraStraightDistance > 0 && (dr == 0 || dc == 0))
                {
                    maxDist = Math.Max(maxDist, _pattern.ExtraStraightDistance);
                }
                
                for (int i = _pattern.MinDistance; i <= maxDist; i++)
                {
                    GridPosition to = new(from.Row + dr * i, from.Column + dc * i);
                    if (!grid.IsInside(to))
                        break;

                    var cell = grid.GetBoardCell(to);
                    var result = new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
                    
                    if (!result.CanOccupy())
                    {
                        if (_pattern.JumpOver)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }

                    yield return result;
                }
            }
        }

        public MovementStrategyResult GetFor(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            // Check direction
            if (!IsValidDirection(dr, dc, _pattern.Directions))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            // Check distance
            int distance = GetDistance(dr, dc);
            if (distance < _pattern.MinDistance || distance > _pattern.MaxDistance)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            // Check path clear (if not jumping)
            if (!_pattern.JumpOver && !IsPathClear(from, to, grid, dr, dc))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            var cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }

        private (int, int)[] GetDirections(string directionsType)
        {
            return directionsType switch
            {
                "straight" => new[] { (-1, 0), (1, 0), (0, -1), (0, 1) },
                "diagonal" => new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) },
                "cross" => new[] { (-1, 0), (1, 0), (0, -1), (0, 1) },
                "all" => new[] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) },
                _ => new[] { (-1, 0), (1, 0), (0, -1), (0, 1) }
            };
        }

        private bool IsValidDirection(int dr, int dc, string directionsType)
        {
            var directions = GetDirections(directionsType);
            foreach ((int rdr, int rdc) in directions)
            {
                // Check if (dr, dc) is in the same direction as (rdr, rdc)
                if (rdr == 0 && rdc == 0) continue;
                
                if (rdr == 0 && dr == 0 && Math.Sign(dc) == Math.Sign(rdc))
                    return true;
                if (rdc == 0 && dc == 0 && Math.Sign(dr) == Math.Sign(rdr))
                    return true;
                if (rdr != 0 && rdc != 0 && Math.Abs(dr) == Math.Abs(dc) && 
                    Math.Sign(dr) == Math.Sign(rdr) && Math.Sign(dc) == Math.Sign(rdc))
                    return true;
            }
            return false;
        }

        private int GetDistance(int dr, int dc)
        {
            // For straight: max(|dr|, |dc|)
            // For diagonal: |dr| (which equals |dc|)
            return Math.Max(Math.Abs(dr), Math.Abs(dc));
        }

        private bool IsPathClear(GridPosition from, GridPosition to, BoardGrid grid, int dr, int dc)
        {
            int stepR = dr == 0 ? 0 : (dr > 0 ? 1 : -1);
            int stepC = dc == 0 ? 0 : (dc > 0 ? 1 : -1);

            GridPosition current = new(from.Row + stepR, from.Column + stepC);

            while (current.Row != to.Row || current.Column != to.Column)
            {
                if (!grid.IsInside(current))
                    return false;

                BoardCell cell = grid.GetBoardCell(current);
                if (cell.OccupiedBy != null)
                    return false;

                current = new(current.Row + stepR, current.Column + stepC);
            }

            return true;
        }
    }
}

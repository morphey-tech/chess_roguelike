using Project.Core.Core.Combat;
using System;
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Movement.Extensions;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Splasher movement: 1 cell in any direction OR 2 cells forward (based on team).
    /// </summary>
    public sealed class SplasherMovement : IMovementStrategy
    {
        public string Id => "splasher";

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            int forwardDr = figure.Team == Team.Player ? 1 : -1;

            foreach ((int dr, int dc) in MovementDirectionsExtensions.GetAdjacentDirections())
            {
                GridPosition to = new(from.Row + dr, from.Column + dc);
                if (!grid.IsInside(to))
                {
                    continue;
                }

                BoardCell cell = grid.GetBoardCell(to);
                MovementStrategyResult result = new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
                if (result.CanOccupy())
                {
                    yield return result;
                }
            }

            GridPosition forward2 = new(from.Row + forwardDr * 2, from.Column);
            if (grid.IsInside(forward2))
            {
                GridPosition intermediate = new(from.Row + forwardDr, from.Column);
                BoardCell intermediateCell = grid.GetBoardCell(intermediate);
                if (intermediateCell.OccupiedBy == null)
                {
                    BoardCell cell = grid.GetBoardCell(forward2);
                    MovementStrategyResult result = new(figure, forward2, true, cell.OccupiedBy);
                    if (result.CanOccupy())
                    {
                        yield return result;
                    }
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
            int forwardDr = figure.Team == Team.Player ? 1 : -1;

            if (!IsValidSplasherMove(dr, dc, forwardDr))
            {
                return MovementStrategyResult.MakeUnreachable(figure, to, null);
            }

            if (Math.Abs(dr) == 2 && dc == 0)
            {
                GridPosition intermediate = new(from.Row + forwardDr, from.Column);
                BoardCell intermediateCell = grid.GetBoardCell(intermediate);
                if (intermediateCell.OccupiedBy != null)
                {
                    return MovementStrategyResult.MakeUnreachable(figure, to, null);
                }
            }

            BoardCell cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }

        private static bool IsValidSplasherMove(int dr, int dc, int forwardDr)
        {
            if (MovementDirectionsExtensions.IsAdjacentMove(dr, dc))
            {
                return true;
            }
            return dr == forwardDr * 2 && dc == 0;
        }
    }
}

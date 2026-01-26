using System.Collections.Generic;
using System;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement.Strategies
{
    /// <summary>
    /// Queen: straight + diagonal lines, unlimited distance.
    /// </summary>
    public sealed class QueenMovement : IMovementStrategy
    {
        public string Id => "queen";

        private static readonly (int row, int col)[] Directions =
        {
            (-1, -1), (-1, 0), (-1, 1),
            ( 0, -1),          ( 0, 1),
            ( 1, -1), ( 1, 0), ( 1, 1)
        };

        public IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid)
        {
            foreach ((int dr, int dc) in Directions)
            {
                for (int i = 1; i < 20; i++)
                {
                    GridPosition to = new(from.Row + dr * i, from.Column + dc * i);
                    
                    if (!grid.IsInside(to))
                        break;
                    
                    var cell = grid.GetBoardCell(to);
                    var result = new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
                    if (!result.CanOccupy()) 
                        break;
                    
                    yield return result;
                    break;
                }
            }
        }

        public MovementStrategyResult GetFor(Figure figure, GridPosition from, GridPosition to, BoardGrid grid)
        {
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            if (dr == 0 && dc == 0)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            bool isStraight = dr == 0 || dc == 0;
            bool isDiagonal = Math.Abs(dr) == Math.Abs(dc);

            if (!isStraight && !isDiagonal)
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            int stepR = dr == 0 ? 0 : (dr > 0 ? 1 : -1);
            int stepC = dc == 0 ? 0 : (dc > 0 ? 1 : -1);

            GridPosition current = new(from.Row + stepR, from.Column + stepC);
            while (current.Row != to.Row || current.Column != to.Column)
            {
                if(!grid.IsInside(current))
                    return MovementStrategyResult.MakeUnreachable(figure, to, null);
                
                var tempCell = grid.GetBoardCell(current);
                if (!tempCell.IsFree)
                    return new MovementStrategyResult(figure, current, false, tempCell.OccupiedBy);
                
                current = new GridPosition(current.Row + stepR, current.Column + stepC);
            }

            if(!grid.IsInside(to))
                return MovementStrategyResult.MakeUnreachable(figure, to, null);

            var cell = grid.GetBoardCell(to);
            return new MovementStrategyResult(figure, to, true, cell.OccupiedBy);
        }
    }
}

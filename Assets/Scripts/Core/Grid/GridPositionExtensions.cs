using System;

namespace Project.Core.Core.Grid
{
    public static class GridPositionExtensions
    {
        /// <summary>
        /// Checks if two grid positions are adjacent (8 directions).
        /// Adjacent means max(|dr|, |dc|) == 1 and not both zero.
        /// </summary>
        public static bool IsAdjacentTo(this GridPosition from, GridPosition to)
        {
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;
            return Math.Max(Math.Abs(dr), Math.Abs(dc)) == 1;
        }

        /// <summary>
        /// Checks if target position is on a diagonal line from the source position.
        /// Diagonal means row and column differences are equal and non-zero.
        /// </summary>
        public static bool IsOnDiagonal(this GridPosition from, GridPosition to)
        {
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);
            return rowDiff == colDiff && rowDiff > 0;
        }

        /// <summary>
        /// Returns the direction vector (normalized to -1, 0, or 1) from this position to another.
        /// </summary>
        public static (int dr, int dc) GetDirectionTo(this GridPosition from, GridPosition to)
        {
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            return (
                dr == 0 ? 0 : (dr > 0 ? 1 : -1),
                dc == 0 ? 0 : (dc > 0 ? 1 : -1)
            );
        }
    }
}

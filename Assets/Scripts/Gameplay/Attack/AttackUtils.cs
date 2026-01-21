using System;
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Attack
{
    public static class AttackUtils
    {
        /// <summary>
        /// Calculate Chebyshev distance (max of row/col difference).
        /// Used for range checks - diagonal movement counts as 1.
        /// </summary>
        public static int GetDistance(GridPosition from, GridPosition to)
        {
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);
            return Math.Max(rowDiff, colDiff);
        }
        
        /// <summary>
        /// Check if target is within attack range.
        /// </summary>
        public static bool IsInRange(GridPosition from, GridPosition to, int attackRange)
        {
            return GetDistance(from, to) <= attackRange;
        }
    }
}

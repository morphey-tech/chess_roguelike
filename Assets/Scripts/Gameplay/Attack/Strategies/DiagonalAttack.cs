using System;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Strategies
{
    /// <summary>
    /// Diagonal attack strategy - can only attack targets on diagonal lines.
    /// Used for Bishop figures.
    /// </summary>
    public sealed class DiagonalAttack : IAttackStrategy
    {
        public string Id => STRATEGY_ID;
        public DeliveryType Delivery => DeliveryType.Projectile;
        
        private const string STRATEGY_ID = "diagonal";

        public bool CanAttack(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
            {
                return false;
            }
            if (!IsOnDiagonal(from, to))
            {
                return false;
            }

            if (!AttackUtils.IsInRange(from, to, attacker.Stats.AttackRange))
            {
                return false;
            }

            // Check that path is clear (no figures blocking)
            if (!IsPathClear(from, to, grid))
            {
                return false;
            }

            BoardCell targetCell = grid.GetBoardCell(to);
            return targetCell.OccupiedBy != null && targetCell.OccupiedBy.Team != attacker.Team;
        }

        public HitContext CreateHitContext(Figure attacker, Figure defender, GridPosition attackerPos, GridPosition defenderPos, BoardGrid grid)
        {
            return new HitContext
            {
                Attacker = attacker,
                Target = defender,
                AttackerPosition = attackerPos,
                TargetPosition = defenderPos,
                Grid = grid,
                HitType = HitType.Ranged,
                AttackerMovesOnKill = false
            };
        }

        /// <summary>
        /// Check if path is clear (no figures blocking except target).
        /// </summary>
        private bool IsPathClear(GridPosition from, GridPosition to, BoardGrid grid)
        {
            int dr = Math.Sign(to.Row - from.Row);
            int dc = Math.Sign(to.Column - from.Column);

            GridPosition current = new(from.Row + dr, from.Column + dc);

            while (current.Row != to.Row || current.Column != to.Column)
            {
                if (!grid.IsInside(current))
                    return false;

                BoardCell cell = grid.GetBoardCell(current);
                if (cell.OccupiedBy != null)
                    return false; // Blocked by a figure

                current = new(current.Row + dr, current.Column + dc);
            }

            return true;
        }

        /// <summary>
        /// Check if target is on a diagonal line from attacker.
        /// Diagonal means row and column differences are equal.
        /// </summary>
        private static bool IsOnDiagonal(GridPosition from, GridPosition to)
        {
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);
            return rowDiff == colDiff && rowDiff > 0;
        }
    }
}

using System;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Grid
{
    public static class CombatContextExtensions
    {
        /// <summary>
        /// Returns the attack direction from attacker to target as a normalized vector.
        /// </summary>
        public static (int dr, int dc) GetAttackDirection(this AfterHitContext context)
        {
            return context.AttackerPosition.GetDirectionTo(context.TargetPosition);
        }

        /// <summary>
        /// Checks if a position is blocked (outside grid or occupied by any figure).
        /// </summary>
        public static bool IsBlocked(this BoardGrid grid, GridPosition position)
        {
            if (!grid.IsInside(position))
            {
                return true;
            }

            return !grid.GetBoardCell(position).IsFree;
        }

        /// <summary>
        /// Checks if pushing a figure to a position would result in a collision.
        /// </summary>
        public static bool HasCollision(this BoardGrid grid, GridPosition pushTo)
        {
            return grid.IsBlocked(pushTo);
        }

        /// <summary>
        /// Finds the nearest figure matching the predicate using Chebyshev distance.
        /// Returns null if no matching figure is found.
        /// </summary>
        public static GridPosition? GetNearestFigure(
            this BoardGrid grid,
            GridPosition from,
            Func<Figure, bool> predicate)
        {
            GridPosition? nearest = null;
            int minDistance = int.MaxValue;

            foreach (BoardCell cell in grid.AllCells())
            {
                Figure other = cell.OccupiedBy;

                if (other == null || !predicate(other))
                {
                    continue;
                }

                int distance = Attack.AttackUtils.GetDistance(from, cell.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = cell.Position;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Finds the nearest enemy figure from the given actor's position.
        /// Returns null if no enemy is found.
        /// </summary>
        public static GridPosition? GetNearestEnemy(this BoardGrid grid, Figure actor)
        {
            GridPosition? actorPos = grid.FindFigure(actor)?.Position;
            if (!actorPos.HasValue)
            {
                return null;
            }

            return grid.GetNearestFigure(actorPos.Value, f => f.Team != actor.Team);
        }
    }
}

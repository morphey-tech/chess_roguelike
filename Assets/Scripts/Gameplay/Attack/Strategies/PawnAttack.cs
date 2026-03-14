using System;
using System.Collections.Generic;
using Project.Core.Core.Combat;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Strategies
{
    public sealed class PawnAttack : IAttackStrategy
    {
        public string Id => STRATEGY_ID;
        public DeliveryType Delivery => DeliveryType.Instant;

        private const string STRATEGY_ID = "pawn";

        public bool CanAttack(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
            {
                return false;
            }

            int forwardDr = attacker.Team == Team.Player ? 1 : -1;
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            bool isDiagonalForward = dr == forwardDr && Math.Abs(dc) == 1;

            if (!isDiagonalForward)
            {
                return false;
            }

            BoardCell targetCell = grid.GetBoardCell(to);
            return targetCell.OccupiedBy != null && targetCell.OccupiedBy.Team != attacker.Team;
        }

        public bool CanAttackPosition(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
            {
                return false;
            }

            int forwardDr = attacker.Team == Team.Player ? 1 : -1;
            int dr = to.Row - from.Row;
            int dc = to.Column - from.Column;

            return dr == forwardDr && Math.Abs(dc) == 1;
        }

        public IEnumerable<GridPosition> GetAttackPositions(Figure attacker, GridPosition from, BoardGrid grid)
        {
            int forwardDr = attacker.Team == Team.Player ? 1 : -1;
            GridPosition left = new GridPosition(from.Row + forwardDr, from.Column - 1);
            GridPosition right = new GridPosition(from.Row + forwardDr, from.Column + 1);

            if (grid.IsInside(left))
                yield return left;
            if (grid.IsInside(right))
                yield return right;
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
                HitType = HitType.Melee,
                AttackerMovesOnKill = true
            };
        }
    }
}

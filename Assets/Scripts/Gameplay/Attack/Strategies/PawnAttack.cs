using System;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Strategies
{
    /// <summary>
    /// Pawn attack: 1 cell diagonally forward (forward-left or forward-right).
    /// Forward direction is based on team (Player moves to higher row, Enemy to lower row).
    /// In Unity: Row 0 is near camera (bottom), Row 7 is far (top).
    /// Player starts at bottom and attacks toward higher row numbers.
    /// </summary>
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

            bool isDiagonalForward = (dr == forwardDr && Math.Abs(dc) == 1);

            if (!isDiagonalForward)
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
                HitType = HitType.Melee,
                AttackerMovesOnKill = true
            };
        }
    }
}

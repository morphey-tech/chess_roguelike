using System.Collections.Generic;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Strategies
{
    public sealed class SimpleAttack : IAttackStrategy
    {
        public string Id => STRATEGY_ID;
        public DeliveryType Delivery => DeliveryType.Instant;

        private const string STRATEGY_ID = "simple";

        public bool CanAttack(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
            {
                return false;
            }
            if (!AttackUtils.IsInRange(from, to, attacker.Stats.AttackRange))
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
            return AttackUtils.IsInRange(from, to, attacker.Stats.AttackRange);
        }

        public IEnumerable<GridPosition> GetAttackPositions(Figure attacker, GridPosition from, BoardGrid grid)
        {
            int range = attacker.Stats.AttackRange;
            for (int row = from.Row - range; row <= from.Row + range; row++)
            {
                for (int col = from.Column - range; col <= from.Column + range; col++)
                {
                    GridPosition pos = new GridPosition(row, col);
                    if (grid.IsInside(pos) && pos != from)
                    {
                        yield return pos;
                    }
                }
            }
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

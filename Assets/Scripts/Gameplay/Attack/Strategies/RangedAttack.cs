using System.Collections.Generic;
using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Strategies
{
    public sealed class RangedAttack : IAttackStrategy
    {
        public string Id => STRATEGY_ID;
        public DeliveryType Delivery => DeliveryType.Projectile;

        private const string STRATEGY_ID = "ranged";

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
            if (!grid.IsPathClear(from, to))
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
            if (!AttackUtils.IsInRange(from, to, attacker.Stats.AttackRange))
            {
                return false;
            }
            return grid.IsPathClear(from, to);
        }

        public IEnumerable<GridPosition> GetAttackPositions(Figure attacker, GridPosition from, BoardGrid grid)
        {
            int range = attacker.Stats.AttackRange;
            int[][] directions = new int[][] { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { 0, 1 } };

            foreach (int[] dir in directions)
            {
                for (int i = 1; i <= range; i++)
                {
                    GridPosition pos = new GridPosition(from.Row + dir[0] * i, from.Column + dir[1] * i);
                    if (!grid.IsInside(pos))
                        break;
                    yield return pos;
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
                HitType = HitType.Ranged,
                AttackerMovesOnKill = false
            };
        }
    }
}

using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Strategies
{
    /// <summary>
    /// Сплэш: бьёт основную цель и 2 соседние клетки.
    /// </summary>
    public sealed class SplashAttack : IAttackStrategy
    {
        public string Id => "splash";

        public bool CanAttack(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
                return false;
            if (!AttackUtils.IsInRange(from, to, attacker.Stats.AttackRange))
                return false;
            
            BoardCell targetCell = grid.GetBoardCell(to);
            return targetCell.OccupiedBy != null && targetCell.OccupiedBy.Team != attacker.Team;
        }

        public HitContext CreateHitContext(Figure attacker, Figure defender, GridPosition attackerPos, GridPosition defenderPos, BoardGrid grid)
        {
            List<Figure> additionalTargets = new();
            
            GridPosition[] adjacentOffsets = {
                new(-1, 0), new(1, 0), new(0, -1), new(0, 1),
                new(-1, -1), new(-1, 1), new(1, -1), new(1, 1)
            };

            int found = 0;
            foreach (var offset in adjacentOffsets)
            {
                if (found >= 2) break;
                
                GridPosition adjacent = new(defenderPos.Row + offset.Row, defenderPos.Column + offset.Column);
                if (!grid.IsInside(adjacent)) continue;
                
                BoardCell cell = grid.GetBoardCell(adjacent);
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != attacker.Team && cell.OccupiedBy != defender)
                {
                    additionalTargets.Add(cell.OccupiedBy);
                    found++;
                }
            }

            return new HitContext
            {
                Attacker = attacker,
                Target = defender,
                AttackerPosition = attackerPos,
                TargetPosition = defenderPos,
                Grid = grid,
                BaseDamage = attacker.Stats.Attack,
                HitType = HitType.Melee,
                AttackerMovesOnKill = true,
                HitCount = 1,
                AdditionalTargets = additionalTargets,
                AdditionalDamageMultiplier = 1f
            };
        }
    }
}

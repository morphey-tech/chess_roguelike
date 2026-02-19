using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
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
        public DeliveryType Delivery => DeliveryType.Instant;

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
            HitContext context = new()
            {
                Attacker = attacker,
                Target = defender,
                AttackerPosition = attackerPos,
                TargetPosition = defenderPos,
                Grid = grid,
                HitType = HitType.Melee,
                AttackerMovesOnKill = true
            };

            // Find up to 2 adjacent enemies and add splash effects
            int splashDamage = (int)attacker.Stats.Attack.Value;
            int found = 0;
            
            foreach (BoardCell cell in grid.GetAdjacentCells(defenderPos))
            {
                if (found >= 2) break;
                
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != attacker.Team && cell.OccupiedBy != defender)
                {
                    context.Effects.Add(new SplashDamageEffect(attacker, cell.OccupiedBy, splashDamage));
                    found++;
                }
            }

            return context;
        }
    }
}

using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Strategies
{
    /// <summary>
    /// Пробивной: бьёт первого на X урон, и N врагов позади на %X.
    /// </summary>
    public sealed class PierceAttack : IAttackStrategy
    {
        public string Id => "pierce";
        public DeliveryType Delivery => DeliveryType.Projectile;
        
        private readonly int _maxPierceCount;
        private readonly float _pierceDamagePercent;

        public PierceAttack(int maxPierceCount = 2, float pierceDamagePercent = 0.5f)
        {
            _maxPierceCount = maxPierceCount;
            _pierceDamagePercent = pierceDamagePercent;
        }

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

        public HitContext CreateHitContext(Figure attacker, Figure defender, GridPosition attackerPos, GridPosition defenderPos, BoardGrid grid)
        {
            var context = new HitContext
            {
                Attacker = attacker,
                Target = defender,
                AttackerPosition = attackerPos,
                TargetPosition = defenderPos,
                Grid = grid,
                HitType = HitType.Ranged,
                AttackerMovesOnKill = false
            };

            // Calculate pierce direction
            int dirRow = defenderPos.Row - attackerPos.Row;
            int dirCol = defenderPos.Column - attackerPos.Column;

            if (dirRow != 0)
            {
                dirRow = dirRow > 0 ? 1 : -1;
            }
            if (dirCol != 0)
            {
                dirCol = dirCol > 0 ? 1 : -1;
            }

            int pierceDamage = (int)(attacker.Stats.Attack.Value * _pierceDamagePercent);

            // Find targets behind the primary target and add pierce effects
            GridPosition current = defenderPos;
            for (int i = 0; i < _maxPierceCount; i++)
            {
                current = new GridPosition(current.Row + dirRow, current.Column + dirCol);

                if (!grid.IsInside(current))
                {
                    break;
                }

                BoardCell cell = grid.GetBoardCell(current);
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != attacker.Team)
                {
                    context.Effects.Add(new PierceDamageEffect(attacker, cell.OccupiedBy, pierceDamage));
                }
            }

            return context;
        }
    }
}

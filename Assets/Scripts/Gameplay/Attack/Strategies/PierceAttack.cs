using Project.Core.Core.Configs.Stats;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Attack.Strategies
{
    /// <summary>
    /// Pierce: melee attack, hits primary target for 100%, and up to 2 enemies behind for 75%/50%.
    /// </summary>
    public sealed class PierceAttack : IAttackStrategy
    {
        public string Id => "pierce";
        public DeliveryType Delivery => DeliveryType.Instant;

        public bool CanAttack(Figure attacker, GridPosition from, GridPosition to, BoardGrid grid)
        {
            if (!grid.IsInside(to))
                return false;

            // Must be adjacent (melee)
            if (!AttackUtils.IsInRange(from, to, 1))
                return false;

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
                HitType = HitType.Melee,
                AttackerMovesOnKill = true
            };

            // Calculate pierce direction (from attacker to defender)
            int dirRow = defenderPos.Row - attackerPos.Row;
            int dirCol = defenderPos.Column - attackerPos.Column;

            if (dirRow != 0)
                dirRow = dirRow > 0 ? 1 : -1;
            if (dirCol != 0)
                dirCol = dirCol > 0 ? 1 : -1;

            float baseDamage = attacker.Stats.Attack.Value;

            // Find up to 2 enemies behind the primary target
            // First behind: 75% damage, second behind: 50% damage
            float[] pierceDamagePercents = { 0.75f, 0.50f };
            
            GridPosition current = defenderPos;
            for (int i = 0; i < pierceDamagePercents.Length; i++)
            {
                current = new GridPosition(current.Row + dirRow, current.Column + dirCol);

                if (!grid.IsInside(current))
                    break;

                BoardCell cell = grid.GetBoardCell(current);
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != attacker.Team)
                {
                    int pierceDamage = (int)(baseDamage * pierceDamagePercents[i]);
                    context.Effects.Add(new PierceDamageEffect(attacker, cell.OccupiedBy, pierceDamage));
                }
            }

            return context;
        }
    }
}

using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Ranger Provocation: If enemy is adjacent, must use melee attack instead of ranged.
    /// Melee attacks deal reduced damage (multiplier).
    /// </summary>
    public class MeleeOverridePasssive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => 100;

        private readonly float _meleeDamageMultiplier;

        public MeleeOverridePasssive(string id, float meleeDamageMultiplier)
        {
            Id = id;
            _meleeDamageMultiplier = meleeDamageMultiplier;
        }

        void IOnBeforeHit.OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            // Check if any enemy is adjacent to the attacker
            var grid = context.Grid;
            var attackerCell = grid.FindFigure(context.Attacker);
            
            if (attackerCell == null)
                return;

            bool enemyAdjacent = false;
            foreach (var cell in grid.GetAdjacentCells(attackerCell.Position))
            {
                if (cell.OccupiedBy != null && cell.OccupiedBy.Team != context.Attacker.Team)
                {
                    enemyAdjacent = true;
                    break;
                }
            }

            // If enemy is adjacent, apply melee damage multiplier
            if (enemyAdjacent)
            {
                context.BonusDamage *= _meleeDamageMultiplier;
            }
        }
    }
}

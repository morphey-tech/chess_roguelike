using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Impact: Pushes target in direction of attack. If target hits obstacle (wall, enemy, ally),
    /// deals bonus damage.
    /// </summary>
    public class ImpactPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => 100;

        private readonly int _bonusDamage;

        public ImpactPassive(string id, int bonusDamage)
        {
            Id = id;
            _bonusDamage = bonusDamage;
        }

        void IOnAfterHit.OnAfterHit(Figure owner, AfterHitContext context)
        {
            // Only trigger if owner was the attacker
            if (owner != context.Attacker)
                return;

            var grid = context.Grid;
            
            // Calculate push direction (from attacker to target)
            int dirRow = context.TargetPosition.Row - context.AttackerPosition.Row;
            int dirCol = context.TargetPosition.Column - context.AttackerPosition.Column;

            if (dirRow != 0)
                dirRow = dirRow > 0 ? 1 : -1;
            if (dirCol != 0)
                dirCol = dirCol > 0 ? 1 : -1;

            // Calculate push target position
            GridPosition pushTo = new(context.TargetPosition.Row + dirRow, context.TargetPosition.Column + dirCol);
            
            // Check if push position is valid
            bool hasCollision = false;
            
            if (!grid.IsInside(pushTo))
            {
                // Hit wall
                hasCollision = true;
            }
            else
            {
                var pushCell = grid.GetBoardCell(pushTo);
                if (pushCell.OccupiedBy != null)
                {
                    // Hit figure (enemy or ally)
                    hasCollision = true;
                }
            }

            // Add push effect with bonus damage on collision
            var pushEffect = new PushEffect(
                context.Attacker,
                context.Target,
                context.TargetPosition,
                pushTo,
                hasCollision ? _bonusDamage : 0);
            
            context.AddEffect(pushEffect);
        }
    }
}

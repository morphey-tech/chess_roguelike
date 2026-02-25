using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

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
            {
                return;
            }

            BoardGrid grid = context.Grid;
            (int dirRow, int dirCol) = context.GetAttackDirection();

            GridPosition pushTo = new(context.TargetPosition.Row + dirRow, context.TargetPosition.Column + dirCol);
            bool hasCollision = grid.HasCollision(pushTo);

            PushEffect pushEffect = new(
                context.Attacker,
                context.Target,
                context.TargetPosition,
                pushTo,
                hasCollision ? _bonusDamage : 0);

            context.AddEffect(pushEffect);
        }
    }
}

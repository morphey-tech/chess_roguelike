using Project.Core.Core.Grid;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
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
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterHit;

        private readonly int _bonusDamage;

        public ImpactPassive(string id, int bonusDamage)
        {
            Id = id;
            _bonusDamage = bonusDamage;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnAfterHit)
            {
                return false;
            }
            if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
            {
                return false;
            }
            return context.Actor == afterHit.Attacker;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not IDamageContext damageContext)
            {
                return TriggerResult.Continue;
            }
            return HandleAfterHit(damageContext);
        }

        public TriggerResult HandleAfterHit(IDamageContext context)
        {
            if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
            {
                return TriggerResult.Continue;
            }

            BoardGrid grid = afterHit.Grid;
            (int dirRow, int dirCol) = afterHit.GetAttackDirection();

            GridPosition pushTo = new(afterHit.TargetPosition.Row + dirRow, afterHit.TargetPosition.Column + dirCol);
            bool hasCollision = grid.HasCollision(pushTo);

            PushEffect pushEffect = new(
                afterHit.Attacker,
                afterHit.Target,
                afterHit.TargetPosition,
                pushTo,
                hasCollision ? _bonusDamage : 0);

            afterHit.AddEffect(pushEffect);

            return TriggerResult.Continue;
        }
    }
}

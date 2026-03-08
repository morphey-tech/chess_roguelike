using System.Collections.Generic;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public class FirstShotPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Additive;
        public TriggerPhase Phase => TriggerPhase.BeforeCalculation;

        private readonly List<object> _cachedTargets = new();
        private readonly float _damage;


        public FirstShotPassive(string id, float damage)
        {
            Id = id;
            _damage = damage;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnBeforeHit)
            {
                return false;
            }
            if (context.Target == null)
            {
                return false;
            }
            return !_cachedTargets.Contains(context.Target);
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not IDamageContext damageContext)
            {
                return TriggerResult.Continue;
            }
            return HandleBeforeHit(damageContext);
        }

        public TriggerResult HandleBeforeHit(IDamageContext context)
        {
            if (context.Target == null)
            {
                return TriggerResult.Continue;
            }
            if (_cachedTargets.Contains(context.Target))
            {
                return TriggerResult.Continue;
            }
            _cachedTargets.Add(context.Target);
            context.BonusDamage = _damage;

            return TriggerResult.Continue;
        }
    }
}
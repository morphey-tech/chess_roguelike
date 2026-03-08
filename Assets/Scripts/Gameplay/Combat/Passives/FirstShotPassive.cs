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
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext beforeHit))
            {
                return false;
            }
            return !_cachedTargets.Contains(beforeHit.Target);
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext beforeHit))
            {
                return TriggerResult.Continue;
            }

            if (_cachedTargets.Contains(beforeHit.Target))
            {
                return TriggerResult.Continue;
            }
            _cachedTargets.Add(beforeHit.Target);
            beforeHit.BonusDamage = _damage;

            return TriggerResult.Continue;
        }
    }
}
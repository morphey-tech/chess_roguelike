using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Heal for a percentage of damage dealt. Only triggers when the owner attacks.
    /// </summary>
    public sealed class LifestealPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterHit;

        private readonly float _percent;

        public LifestealPassive(string id, float percent)
        {
            Id = id;
            _percent = percent;
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

            int heal = (int)(afterHit.DamageDealt * _percent);
            if (heal > 0)
            {
                afterHit.Attacker.Stats.Heal(heal);
                afterHit.Effects.Add(new HealEffect(afterHit.Attacker, heal));
            }

            return TriggerResult.Continue;
        }
    }
}

using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Deal bonus damage to low HP targets. Only triggers when the owner attacks.
    /// </summary>
    public sealed class ExecutePassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Multiplicative;
        public TriggerPhase Phase => TriggerPhase.BeforeHit;

        private readonly float _hpThreshold;
        private readonly float _damageMultiplier;

        public ExecutePassive(string id, float hpThreshold, float damageMultiplier)
        {
            Id = id;
            _hpThreshold = hpThreshold;
            _damageMultiplier = damageMultiplier;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnBeforeHit)
            {
                return false;
            }
            if (context.Target is not Figure target)
            {
                return false;
            }

            float hpPercent = target.Stats.CurrentHp.Value / target.Stats.MaxHp;
            return hpPercent <= _hpThreshold;
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
            if (context.Target is not Figure target)
            {
                return TriggerResult.Continue;
            }
            
            float hpPercent = target.Stats.CurrentHp.Value / target.Stats.MaxHp;
            if (hpPercent <= _hpThreshold)
            {
                context.DamageMultiplier *= _damageMultiplier;
            }

            return TriggerResult.Continue;
        }
    }
}

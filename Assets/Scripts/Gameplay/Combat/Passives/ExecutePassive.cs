using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

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
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext beforeHit))
            {
                return false;
            }
            if (context.Actor != beforeHit.Attacker)
            {
                return false;
            }

            float hpPercent = beforeHit.Target.Stats.CurrentHp.Value / beforeHit.Target.Stats.MaxHp;
            return hpPercent <= _hpThreshold;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext beforeHit))
            {
                return TriggerResult.Continue;
            }

            float hpPercent = beforeHit.Target.Stats.CurrentHp.Value / beforeHit.Target.Stats.MaxHp;
            if (hpPercent <= _hpThreshold)
            {
                beforeHit.DamageMultiplier *= _damageMultiplier;
            }

            return TriggerResult.Continue;
        }
    }
}

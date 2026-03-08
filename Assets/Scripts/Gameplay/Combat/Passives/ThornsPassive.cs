using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Reflect damage back to attacker. Only triggers when the owner is being attacked (is the target).
    /// </summary>
    public sealed class ThornsPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterHit;

        private readonly float _reflectPercent;

        public ThornsPassive(string id, float reflectPercent)
        {
            Id = id;
            _reflectPercent = reflectPercent;
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
            return context.Target == afterHit.Target;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<AfterHitContext>(out AfterHitContext afterHit))
            {
                return TriggerResult.Continue;
            }

            int reflect = (int)(afterHit.DamageDealt * _reflectPercent);
            if (reflect > 0)
            {
                afterHit.Effects.Add(new ThornsReflectEffect(afterHit.Target, afterHit.Attacker, reflect));
            }

            return TriggerResult.Continue;
        }
    }
}

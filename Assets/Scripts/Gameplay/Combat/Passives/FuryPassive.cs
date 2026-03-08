using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures.StatusEffects;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public class FuryPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.AfterHit;

        private readonly float _damagePreStack;
        private readonly int _maxStacks;

        public FuryPassive(string id, float damage, int maxStacks)
        {
            Id = id;
            _damagePreStack = damage;
            _maxStacks = maxStacks;
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

            FuryEffect status = new(_damagePreStack, 1, _maxStacks);
            afterHit.Attacker.Effects.AddOrStack(status);

            return TriggerResult.Continue;
        }
    }
}

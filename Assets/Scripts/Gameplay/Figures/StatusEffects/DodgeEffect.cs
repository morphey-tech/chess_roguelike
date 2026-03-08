using Project.Core.Core.Random;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public class DodgeEffect : StatusEffectBase, IOnBeforeHit
    {
        public override string Id => "dodge";
        public override EffectCategory Category => EffectCategory.Buff;

        private readonly float _chance;
        private readonly IRandomService _random;

        public DodgeEffect(float chance,
            IRandomService random,
            int turns = -1,
            int uses = -1) : base(turns, uses)
        {
            _chance = chance;
            _random = random;
        }

        public override bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnBeforeHit)
            {
                return false;
            }
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext ctx))
            {
                return false;
            }
            return context.Target == ctx.Target && RemainingUses > 0;
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not IDamageContext dc) return TriggerResult.Continue;
            return HandleBeforeHit(dc);
        }

        public TriggerResult HandleBeforeHit(IDamageContext context)
        {
            if (!TryConsumeUse())
            {
                return TriggerResult.Continue;
            }
            if (_random.Chance(_chance))
            {
                context.IsDodged = true;
                context.IsCancelled = true;
                return TriggerResult.Cancel;
            }
            return TriggerResult.Continue;
        }
    }
}
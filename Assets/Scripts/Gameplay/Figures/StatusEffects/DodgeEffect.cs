using Project.Core.Core.Random;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public class DodgeEffect : StatusEffectBase, IOnBeforeHit
    {
        public override string Id => "dodge";

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

        public override TriggerResult Execute(TriggerContext context)
        {
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext ctx))
            {
                return TriggerResult.Continue;
            }

            if (!TryConsumeUse())
            {
                return TriggerResult.Continue;
            }
            if (_random.Chance(_chance))
            {
                ctx.IsDodged = true;
                ctx.IsCancelled = true;
                return TriggerResult.Cancel; // Cancel the hit
            }
            return TriggerResult.Continue;
        }
    }
}
using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public class DodgeEffect : StatusEffectBase
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

        public override void OnBeforeHit(Figure owner, BeforeHitContext ctx)
        {
            if (ctx.Target != owner)
            {
                return;
            }
            if (!TryConsumeUse())
            {
                return;
            }
            if (_random.Chance(_chance))
            {
                ctx.IsDodged = true;
                ctx.IsCancelled = true;
            }
        }
    }
}
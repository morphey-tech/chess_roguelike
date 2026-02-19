using Project.Gameplay.Gameplay.Combat.Contexts;
using UnityEngine;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public class DodgeEffect : StatusEffectBase
    {
        public override string Id => "dodge";

        private readonly float _chance;
        
        public DodgeEffect(float chance, int turns = -1, int uses = -1) : base(turns, uses)
        {
            _chance = chance;
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
            if (Random.value < _chance)
            {
                ctx.IsDodged = true;
                ctx.IsCancelled = true;
            }
        }
    }
}
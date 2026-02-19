using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    public class FuryEffect : StackableStatusEffect
    {
        public override string Id => "fury";

        private readonly float _damagePerStack;
        
        public FuryEffect(float damage, int stacks, int maxStacks, int turns = -1, int uses = -1) 
            : base(stacks, maxStacks, turns, uses)
        {
            _damagePerStack = damage;
        }

        public override void OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            context.BaseDamage += _damagePerStack * Stacks;
        }
    }
}
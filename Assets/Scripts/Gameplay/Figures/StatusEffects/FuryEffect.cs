using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Figures.StatusEffects
{
    /// <summary>
    /// Fury: gain attack damage stacks when attacking. Stacks persist until death.
    /// </summary>
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
            // Add bonus damage directly to the hit context
            float totalBonus = _damagePerStack * Stacks;
            context.BonusDamage += totalBonus;
        }
    }
}
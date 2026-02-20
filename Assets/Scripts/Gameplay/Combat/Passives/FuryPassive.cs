using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Figures.StatusEffects;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public class FuryPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => 100;

        private readonly float _damagePreStack;
        private readonly int _maxStacks;

        public FuryPassive(string id, float damage, int maxStacks)
        {
            Id = id;
            _damagePreStack = damage;
            _maxStacks = maxStacks;
        }

        void IOnAfterHit.OnAfterHit(Figure owner, AfterHitContext context)
        {
            // Only add stacks when owner is the attacker
            if (owner != context.Attacker)
                return;
                
            FuryEffect status = new(_damagePreStack, 1, _maxStacks);
            owner.Effects.AddOrStack(status);
        }
    }
}
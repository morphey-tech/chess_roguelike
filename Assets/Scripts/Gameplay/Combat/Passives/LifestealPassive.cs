using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public sealed class LifestealPassive : IPassive, IOnAfterHit
    {
        public string Id { get; }
        public int Priority => 100;
        
        private readonly float _percent;

        public LifestealPassive(string id, float percent)
        {
            Id = id;
            _percent = percent;
        }

        public void OnAfterHit(AfterHitContext context)
        {
            if (context.Attacker != context.Target) // only for attacker
            {
                int heal = (int)(context.DamageDealt * _percent);
                if (heal > 0)
                {
                    context.Attacker.Stats.Heal(heal);
                    context.HealedAmount += heal;
                }
            }
        }
    }
}

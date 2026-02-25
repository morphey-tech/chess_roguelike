using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Effects.Impl;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Heal for a percentage of damage dealt. Only triggers when the owner attacks.
    /// </summary>
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

        public void OnAfterHit(Figure owner, AfterHitContext context)
        {
            if (owner != context.Attacker)
            {
                return;
            }

            int heal = (int)(context.DamageDealt * _percent);
            if (heal > 0)
            {
                context.Attacker.Stats.Heal(heal);
                context.Effects.Add(new HealEffect(context.Attacker, heal));
            }
        }
    }
}

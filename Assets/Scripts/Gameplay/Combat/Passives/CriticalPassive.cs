using Project.Core.Core.Random;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Chance to deal critical hit. Only triggers when the owner attacks.
    /// </summary>
    public sealed class CriticalPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => 5;

        private readonly float _critChance;
        private readonly float _critMultiplier;
        private readonly IRandomService _random;

        public CriticalPassive(string id, float critChance, float critMultiplier, IRandomService random)
        {
            Id = id;
            _critChance = critChance;
            _critMultiplier = critMultiplier;
            _random = random;
        }

        public void OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            // Only trigger when the owner is attacking
            if (owner != context.Attacker)
            {
                return;
            }

            if (_random.Chance(_critChance))
            {
                context.DamageMultiplier *= _critMultiplier;
                context.IsCritical = true;
            }
        }
    }
}

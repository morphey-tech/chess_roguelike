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

        public CriticalPassive(string id, float critChance, float critMultiplier)
        {
            Id = id;
            _critChance = critChance;
            _critMultiplier = critMultiplier;
        }

        public void OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            // Only trigger when the owner is attacking
            if (owner != context.Attacker)
                return;

            if (UnityEngine.Random.value <= _critChance)
            {
                context.DamageMultiplier *= _critMultiplier;
                context.IsCritical = true;
            }
        }
    }
}

using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
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

        public void OnBeforeHit(BeforeHitContext context)
        {
            if (UnityEngine.Random.value <= _critChance)
            {
                context.DamageMultiplier *= _critMultiplier;
                context.IsCritical = true;
            }
        }
    }
}

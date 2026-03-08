using Project.Core.Core.Random;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Chance to deal critical hit. Only triggers when the owner attacks.
    /// </summary>
    public sealed class CriticalPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Default;
        public TriggerPhase Phase => TriggerPhase.BeforeHit;

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

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnBeforeHit)
            {
                return false;
            }
            if (context.Actor == null)
            {
                return false;
            }
            return _random.Chance(_critChance);
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not IDamageContext damageContext)
            {
                return TriggerResult.Continue;
            }
            return HandleBeforeHit(damageContext);
        }
        
        public TriggerResult HandleBeforeHit(IDamageContext context)
        {
            context.DamageMultiplier *= _critMultiplier;
            context.IsCritical = true;

            return TriggerResult.Continue;
        }
    }
}

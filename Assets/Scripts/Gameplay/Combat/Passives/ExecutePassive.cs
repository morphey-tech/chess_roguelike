using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    public sealed class ExecutePassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => 10;
        
        private readonly float _hpThreshold;
        private readonly float _damageMultiplier;

        public ExecutePassive(string id, float hpThreshold, float damageMultiplier)
        {
            Id = id;
            _hpThreshold = hpThreshold;
            _damageMultiplier = damageMultiplier;
        }

        public void OnBeforeHit(BeforeHitContext context)
        {
            float hpPercent = (float)context.Target.Stats.CurrentHp / context.Target.Stats.MaxHp;
            if (hpPercent <= _hpThreshold)
                context.DamageMultiplier *= _damageMultiplier;
        }
    }
}

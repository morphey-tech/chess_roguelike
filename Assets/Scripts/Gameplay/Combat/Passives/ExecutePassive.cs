using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Deal bonus damage to low HP targets. Only triggers when the owner attacks.
    /// </summary>
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

        public void OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            // Only trigger when the owner is attacking
            if (owner != context.Attacker)
            {
                return;
            }

            float hpPercent = (float)context.Target.Stats.CurrentHp / context.Target.Stats.MaxHp;
            if (hpPercent <= _hpThreshold)
            {
                context.DamageMultiplier *= _damageMultiplier;
            }
        }
    }
}

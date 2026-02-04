using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Queues bonus damage visual from passives (e.g., push blocked damage).
    /// Note: actual damage is already applied during passive execution.
    /// </summary>
    public sealed class BonusDamageEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.SecondaryDamage;
        public int OrderInPhase => 20; // After splash/pierce

        private readonly Figure _target;
        private readonly int _bonusDamage;
        private readonly string _source;

        public BonusDamageEffect(Figure target, int bonusDamage, string source = "passives")
        {
            _target = target;
            _bonusDamage = bonusDamage;
            _source = source;
        }

        public void Apply(CombatEffectContext context)
        {
            if (_bonusDamage > 0)
            {
                var visualCtx = new DamageVisualContext(_target.Id, _bonusDamage, damageType: _source);
                context.Visuals.Enqueue(new DamageCommand(visualCtx));
                context.Logger.Info($"{_target} took {_bonusDamage} bonus damage from {_source}. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");
            }
        }
    }
}

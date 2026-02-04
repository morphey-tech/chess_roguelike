using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Queues thorns damage reflection visual.
    /// Note: actual damage is already applied during passive execution.
    /// </summary>
    public sealed class ThornsReflectEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.SecondaryDamage;
        public int OrderInPhase => 30; // After bonus damage

        private readonly Figure _attacker;
        private readonly int _reflectedDamage;

        public ThornsReflectEffect(Figure attacker, int reflectedDamage)
        {
            _attacker = attacker;
            _reflectedDamage = reflectedDamage;
        }

        public void Apply(CombatEffectContext context)
        {
            // Visual: Queue damage effect on attacker
            var visualCtx = new DamageVisualContext(_attacker.Id, _reflectedDamage, damageType: "thorns");
            context.Visuals.Enqueue(new DamageCommand(visualCtx));
            context.Logger.Info($"{_attacker} takes {_reflectedDamage} thorns damage. HP: {_attacker.Stats.CurrentHp}/{_attacker.Stats.MaxHp}");
        }
    }
}

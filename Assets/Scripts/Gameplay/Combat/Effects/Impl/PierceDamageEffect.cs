using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Visual.Commands.Contexts;
using Project.Gameplay.Gameplay.Visual.Commands.Impl;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Applies pierce damage to a target behind the primary target.
    /// If target dies, adds KillEffect to pending effects.
    /// </summary>
    public sealed class PierceDamageEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.SecondaryDamage;
        public int OrderInPhase => 10; // After splash

        private readonly Figure _attacker;
        private readonly Figure _target;
        private readonly int _damage;

        public PierceDamageEffect(Figure attacker, Figure target, int damage)
        {
            _attacker = attacker;
            _target = target;
            _damage = damage;
        }

        public void Apply(CombatEffectContext context)
        {
            if (_target == null || _target.Stats.CurrentHp <= 0)
                return;

            // Domain: Apply damage
            bool died = _target.Stats.TakeDamage(_damage);
            
            // Visual: Queue damage effect
            var visualCtx = new DamageVisualContext(_target.Id, _damage, damageType: "pierce");
            context.Visuals.Enqueue(new DamageCommand(visualCtx));
            context.Logger.Info($"Pierce hit {_target} for {_damage} damage. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");

            // Domain: If died, add KillEffect
            if (died)
            {
                context.Passives.TriggerKill(_attacker, _target);
                context.Passives.TriggerDeath(_target, _attacker);
                
                BoardCell cell = context.Grid.FindFigure(_target);
                context.PendingEffects.Add(new KillEffect(_target, cell, "pierce"));
            }
        }
    }
}

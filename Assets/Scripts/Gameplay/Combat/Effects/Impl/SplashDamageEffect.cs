using System;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Applies splash damage via DamageApplier. Смерть обрабатывает LifeService.
    /// </summary>
    public sealed class SplashDamageEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.SecondaryDamage;
        public int OrderInPhase => 0;

        private readonly Figure _attacker;
        private readonly Figure _target;
        private readonly int _damage;

        public SplashDamageEffect(Figure attacker, Figure target, int damage)
        {
            _attacker = attacker;
            _target = target;
            _damage = damage;
        }

        public void Apply(CombatEffectContext context)
        {
            if (_target == null || _target.Stats.CurrentHp <= 0)
                return;

            var dmgCtx = new DamageContext(_attacker, _target, _damage, false, "splash", Array.Empty<IDamageModifier>());
            (DamageResult result, _) = context.DamageApplier.Apply(context, dmgCtx);

            context.AddVisualEvent(new DamageVisualEvent(_target.Id, result.Final, false, "splash"));
            context.Logger.Info($"Splash hit {_target} for {result.Final} damage. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");
        }
    }
}

using System;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Applies pierce damage via DamageApplier. Смерть обрабатывает LifeService.
    /// </summary>
    public sealed class PierceDamageEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.SecondaryDamage;
        public int OrderInPhase => 10;

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

            DamageContext dmgCtx = new(_attacker, _target, _damage, false, false, false,
                "pierce", Array.Empty<IDamageModifier>());
            (DamageResult result, _) = context.DamageApplier.Apply(context, dmgCtx);

            context.AddVisualEvent(new DamageVisualEvent(_target.Id, result.Final, false, "pierce"));
            context.Logger.Info($"Pierce hit {_target} for {result.Final} damage. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");
        }
    }
}

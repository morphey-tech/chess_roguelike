using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Figures;
using System;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Applies passive bonus damage via DamageApplier and queues visual event.
    /// </summary>
    public sealed class BonusDamageEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.SecondaryDamage;
        public int OrderInPhase => 20; // After splash/pierce

        private readonly Figure _attacker;
        private readonly Figure? _target;
        private readonly int _bonusDamage;
        private readonly string _source;

        public BonusDamageEffect(Figure attacker, Figure target, int bonusDamage, string source = "passives")
        {
            _attacker = attacker;
            _target = target;
            _bonusDamage = bonusDamage;
            _source = source;
        }

        public void Apply(CombatEffectContext context)
        {
            if (_target == null || _target.Stats.CurrentHp.Value <= 0 || _bonusDamage <= 0)
            {
                return;
            }

            DamageContext dmgCtx = new(_attacker, _target, _bonusDamage, false, false,
                false, _source, Array.Empty<IDamageModifier>());
            (DamageResult result, _) = context.DamageApplier.Apply(context, dmgCtx);

            context.AddVisualEvent(new DamageVisualEvent(_target.Id, result.Final, damageType: _source));
            context.Logger.Info($"{_target} took {result.Final} bonus damage from {_source}. HP: {_target.Stats.CurrentHp.Value}/{_target.Stats.MaxHp}");
        }
    }
}

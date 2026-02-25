using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Figures;
using System;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Applies thorns reflected damage via DamageApplier and queues visual event.
    /// </summary>
    public sealed class ThornsReflectEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.SecondaryDamage;
        public int OrderInPhase => 30; // After bonus damage

        private readonly Figure _source;
        private readonly Figure? _target;
        private readonly int _reflectedDamage;

        public ThornsReflectEffect(Figure source, Figure target, int reflectedDamage)
        {
            _source = source;
            _target = target;
            _reflectedDamage = reflectedDamage;
        }

        public void Apply(CombatEffectContext context)
        {
            if (_target == null || _target.Stats.CurrentHp <= 0 || _reflectedDamage <= 0)
            {
                return;
            }

            DamageContext dmgCtx = new(_source, _target, _reflectedDamage, false, false,
                false, "thorns", Array.Empty<IDamageModifier>());
            (DamageResult result, _) = context.DamageApplier.Apply(context, dmgCtx);

            context.AddVisualEvent(new DamageVisualEvent(_target.Id, result.Final, damageType: "thorns"));
            context.Logger.Info($"{_target} takes {result.Final} thorns damage. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");
        }
    }
}

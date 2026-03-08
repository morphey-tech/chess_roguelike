using System;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Applies damage via DamageApplier; queues damage visual. Смерть обрабатывает LifeService.
    /// </summary>
    public sealed class DealDamageEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.Damage;
        public int OrderInPhase => 0;

        private readonly Figure _attacker;
        private readonly Figure _target;
        private readonly int _damage;
        private readonly bool _isCritical;
        private readonly bool _isDodged;
        private readonly bool _isCancelled;

        public DealDamageEffect(Figure attacker, Figure target, int damage,
            bool isCritical = false, bool isDodged = false, bool isCancelled = false)
        {
            _attacker = attacker;
            _target = target;
            _damage = damage;
            _isCritical = isCritical;
            _isDodged = isDodged;
            _isCancelled = isCancelled;
        }

        public void Apply(CombatEffectContext context)
        {
            DamageContext dmgCtx = new(_attacker, _target, _damage, _isCritical, _isDodged, _isCancelled,
                "primary", Array.Empty<IDamageModifier>());
            (DamageResult result, bool _) = context.DamageApplier.Apply(context, dmgCtx);

            context.AddVisualEvent(new DamageVisualEvent(_target.EntityId, result.Final, _isCritical,
                _isDodged,"primary"));
            context.ActionContext.LastDamageDealt = result.Final;

            string critText = _isCritical ? " (CRIT)" : "";
            context.Logger.Info($"{_target} takes {result.Final} damage{critText}. HP: {_target.Stats.CurrentHp.Value}/{_target.Stats.MaxHp}");
        }
    }
}

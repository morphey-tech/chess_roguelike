using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Combat.Damage;
using Project.Gameplay.Gameplay.Combat.Visual;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Combat.Effects.Impl
{
    /// <summary>
    /// Pushes target and deals bonus damage if collision occurs.
    /// </summary>
    public sealed class PushEffect : ICombatEffect
    {
        public CombatEffectPhase Phase => CombatEffectPhase.Movement;
        public int OrderInPhase => 0;

        private readonly Figure _attacker;
        private readonly Figure _target;
        private readonly GridPosition _fromPosition;
        private readonly GridPosition _toPosition;
        private readonly int _bonusDamage;

        public PushEffect(Figure attacker, Figure target, GridPosition fromPosition, GridPosition toPosition, int bonusDamage = 0)
        {
            _attacker = attacker;
            _target = target;
            _fromPosition = fromPosition;
            _toPosition = toPosition;
            _bonusDamage = bonusDamage;
        }

        public void Apply(CombatEffectContext context)
        {
            context.Logger.Info($"{_target} pushed from ({_fromPosition.Row}, {_fromPosition.Column}) to ({_toPosition.Row}, {_toPosition.Column})");
            context.AddVisualEvent(new PushVisualEvent(_target.Id, _fromPosition, _toPosition));

            // Apply bonus damage if collision
            if (_bonusDamage > 0)
            {
                DamageContext dmgCtx = new(_attacker, _target, _bonusDamage, false, false, false,
                    "impact", System.Array.Empty<IDamageModifier>());
                (DamageResult result, _) = context.DamageApplier.Apply(context, dmgCtx);
                
                context.Logger.Info($"Impact bonus: {_target} takes {result.Final} damage. HP: {_target.Stats.CurrentHp}/{_target.Stats.MaxHp}");
            }
        }
    }
}

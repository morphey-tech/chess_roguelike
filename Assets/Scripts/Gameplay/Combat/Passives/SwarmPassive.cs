using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Imp;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Swarm: +1 DMG per allied neighbour. Adds a flat damage modifier to Attack stat.
    /// </summary>
    public sealed class SwarmPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.Normal;
        public TriggerGroup Group => TriggerGroup.Additive;
        public TriggerPhase Phase => TriggerPhase.BeforeCalculation;

        private readonly int _damagePerAlly;
        private readonly int _duration;

        public SwarmPassive(string id, int damagePerAlly, int duration)
        {
            Id = id;
            _damagePerAlly = damagePerAlly;
            _duration = duration;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnBeforeHit)
            {
                return false;
            }
            return context.TryGetData(out BeforeHitContext? _);
        }

        public TriggerResult Execute(TriggerContext context)
        {
            if (context is not IDamageContext damageContext)
            {
                return TriggerResult.Continue;
            }
            return HandleBeforeHit(damageContext);
        }

        public TriggerResult HandleBeforeHit(IDamageContext context)
        {
            if (!context.TryGetData(out BeforeHitContext? beforeHit))
            {
                return TriggerResult.Continue;
            }

            int allies = beforeHit.Grid.CountAlliesAround(beforeHit.Attacker);
            int totalBonus = allies * _damagePerAlly;

            beforeHit.Attacker.Stats.Attack.RemoveModifiersById(Id);

            CombatFlatModifier modifier = new(Id, totalBonus, 0, _duration, false,
                ModifierSourceContext.PreviewCalculation);

            beforeHit.Attacker.Stats.Attack.AddModifier(modifier);

            return TriggerResult.Continue;
        }
    }
}
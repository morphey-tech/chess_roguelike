using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Imp;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Desperation: if no allies nearby, base damage = 1.
    /// Adds a flat modifier to set Attack to 1. Swarm bonus is added separately.
    /// </summary>
    public sealed class DesperationPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => TriggerPriorities.High; // Execute BEFORE Swarm
        public TriggerGroup Group => TriggerGroup.First;
        public TriggerPhase Phase => TriggerPhase.BeforeCalculation;

        public DesperationPassive(string id)
        {
            Id = id;
        }

        public bool Matches(TriggerContext context)
        {
            if (context.Type != TriggerType.OnBeforeHit)
            {
                return false;
            }
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext beforeHit))
            {
                return false;
            }

            int allies = beforeHit.Grid.CountAlliesAround(beforeHit.Attacker);
            return allies == 0;
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
            if (!context.TryGetData<BeforeHitContext>(out BeforeHitContext beforeHit))
            {
                return TriggerResult.Continue;
            }

            int allies = beforeHit.Grid.CountAlliesAround(beforeHit.Attacker);

            if (allies == 0)
            {
                Figure owner = beforeHit.Attacker;
                owner.Stats.Attack.RemoveModifiersById(Id);

                float currentAttack = owner.Stats.Attack.Value;
                float delta = 1f - currentAttack;

                CombatFlatModifier modifier = new(Id, delta, 0, 1, false,
                    ModifierSourceContext.PreviewCalculation);
                owner.Stats.Attack.AddModifier(modifier);
            }

            return TriggerResult.Continue;
        }
    }
}

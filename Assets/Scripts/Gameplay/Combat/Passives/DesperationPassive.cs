using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Imp;
using Project.Gameplay.Gameplay.Combat.Triggers;
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
        public int Priority => 50; // Execute BEFORE Swarm (Priority 100)

        public DesperationPassive(string id)
        {
            Id = id;
        }

        public void OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            int allies = context.Grid.CountAlliesAround(owner);

            if (allies == 0)
            {
                // No allies nearby: set Attack to 1 via modifier
                // Remove old modifier first to avoid stacking
                owner.Stats.Attack.RemoveModifiersById(Id);

                // Calculate the delta needed to bring Attack to 1
                float currentAttack = owner.Stats.Attack.Value;
                float delta = 1f - currentAttack;

                CombatFlatModifier modifier = new(Id, delta, 0, 1, false, ModifierSourceContext.PreviewCalculation);
                owner.Stats.Attack.AddModifier(modifier);
            }
        }
    }
}

using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Imp;
using Project.Gameplay.Gameplay.Combat.Triggers;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Combat.Passives
{
    /// <summary>
    /// Desperation: if no allies nearby, base damage = 1.
    /// Uses stat modifier to set attack to 1 (preserves bonus damage from other passives like Swarm).
    /// </summary>
    public sealed class DesperationPassive : IPassive, IOnBeforeHit
    {
        public string Id { get; }
        public int Priority => 50; // Execute BEFORE Swarm to set base first

        public DesperationPassive(string id)
        {
            Id = id;
        }

        public void OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            int allies = context.Grid.CountAlliesAround(owner);

            if (allies == 0)
            {
                // No allies nearby: set attack to 1 via modifier
                // This effectively sets base damage to 1 (before defence subtraction)
                // Swarm bonus will be added on top as a separate modifier
                var modifier = new CombatFlatModifier(Id, 1f - owner.Stats.Attack.Value, 0, 1, false);
                owner.Stats.Attack.AddModifier(modifier);
            }
        }
    }
}

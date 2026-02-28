using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Imp;
using Project.Gameplay.Gameplay.Combat.Triggers;
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
        public int Priority => 100;

        private readonly int _damagePerAlly;
        private readonly int _duration;

        public SwarmPassive(string id, int damagePerAlly, int duration)
        {
            Id = id;
            _damagePerAlly = damagePerAlly;
            _duration = duration;
        }

        public void OnBeforeHit(Figure owner, BeforeHitContext context)
        {
            int allies = context.Grid.CountAlliesAround(owner);
            int totalBonus = allies * _damagePerAlly;
            owner.Stats.Attack.RemoveModifiersById(Id);
            CombatFlatModifier modifier = new(Id, totalBonus, 0, _duration, false, ModifierSourceContext.PreviewCalculation);
            owner.Stats.Attack.AddModifier(modifier);
        }
    }
}
using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public readonly struct BeamVisualContext
    {
        public int AttackerId { get; }
        public int TargetId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }
        public string AttackType { get; }

        public BeamVisualContext(int attackerId, int targetId, GridPosition from, GridPosition to, string attackType = null)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            From = from;
            To = to;
            AttackType = attackType;
        }
    }
}

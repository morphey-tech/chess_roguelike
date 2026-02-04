using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public readonly struct AttackVisualContext
    {
        public int AttackerId { get; }
        public GridPosition TargetPosition { get; }
        public string AttackType { get; }

        public AttackVisualContext(int attackerId, GridPosition targetPosition, string attackType = null)
        {
            AttackerId = attackerId;
            TargetPosition = targetPosition;
            AttackType = attackType;
        }
    }
}

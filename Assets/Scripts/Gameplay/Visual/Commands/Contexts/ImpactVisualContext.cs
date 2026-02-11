using Project.Core.Core.Grid;

namespace Project.Gameplay.Gameplay.Visual.Commands.Contexts
{
    public readonly struct ImpactVisualContext
    {
        public GridPosition Position { get; }
        public string ImpactFxId { get; }

        public ImpactVisualContext(GridPosition position, string impactFxId = null)
        {
            Position = position;
            ImpactFxId = impactFxId;
        }
    }
}
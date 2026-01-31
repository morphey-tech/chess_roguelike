using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Movement
{
    public readonly struct MovementStrategyResult
    {
        public GridPosition Position { get; }
        private bool CanBeReached { get; }
        public bool IsFree => OccupiedBy == null;
        
        private Figure Figure { get; }
        private Figure? OccupiedBy { get; }

        public MovementStrategyResult(Figure figure, GridPosition position, bool canBeReached, Figure? occupiedBy)
        {
            Figure = figure;
            Position = position;
            CanBeReached = canBeReached;
            OccupiedBy = occupiedBy;
        }
        
        public bool CanOccupy()
        {
            return CanBeReached && (IsFree || OccupiedBy?.Team != Figure.Team);
        }

        
        public static MovementStrategyResult MakeEmpty() => new(null, default, false, null);
        public static MovementStrategyResult MakeUnreachable(Figure figure, GridPosition gridPosition, Figure? occupiedBy) =>
            new(figure, gridPosition, false, occupiedBy);
    }
}
using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Movement;

namespace Project.Gameplay.Gameplay.Movement
{
    /// <summary>
    /// Defines movement rules for a figure type.
    /// </summary>
    public interface IMovementStrategy
    {
        string Id { get; }
        
        /// <summary>
        /// Returns all positions this figure can move to.
        /// </summary>
        IEnumerable<MovementStrategyResult> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid);
        
        /// <summary>
        /// Checks if a specific move is valid.
        /// </summary>
        MovementStrategyResult GetFor(Figure figure, GridPosition from, GridPosition to, BoardGrid grid);
    }
}

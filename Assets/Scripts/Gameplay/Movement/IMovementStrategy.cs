using System.Collections.Generic;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

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
        IEnumerable<GridPosition> GetAvailableMoves(Figure figure, GridPosition from, BoardGrid grid);
        
        /// <summary>
        /// Checks if a specific move is valid.
        /// </summary>
        bool CanMove(Figure figure, GridPosition from, GridPosition to, BoardGrid grid);
    }
}

using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Turn.BonusMove
{
    /// <summary>
    /// Controls bonus move state - when a figure gets an extra move after action.
    /// Examples: retreat after attack, jump after kill, chain moves.
    /// </summary>
    public interface IBonusMoveController
    {
        bool IsActive { get; }
        Figure Actor { get; }
        GridPosition From { get; }
        
        /// <summary>
        /// Starts bonus move mode for the given actor.
        /// </summary>
        void Start(Figure actor, GridPosition from, int maxDistance);
        
        /// <summary>
        /// Attempts to handle a move as a bonus move.
        /// Returns true if the move was valid and executed.
        /// </summary>
        bool TryExecute(GridPosition to);
        
        /// <summary>
        /// Cancels the current bonus move (e.g., player pressed cancel).
        /// </summary>
        void Cancel();
        
        /// <summary>
        /// Gets available positions for the current bonus move.
        /// </summary>
        System.Collections.Generic.IEnumerable<GridPosition> GetAvailablePositions();
    }
}

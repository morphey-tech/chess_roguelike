using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Turn.Execution
{
    /// <summary>
    /// Executes turn actions for figures.
    /// Handles pattern resolution, step execution, and returns results.
    /// </summary>
    public interface ITurnExecutor
    {
        /// <summary>
        /// Attempts to execute a turn for the given figure.
        /// </summary>
        /// <param name="actor">The figure taking the turn</param>
        /// <param name="from">Starting position</param>
        /// <param name="to">Target position</param>
        /// <param name="grid">The game grid</param>
        /// <returns>Result of the turn execution</returns>
        UniTask<TurnExecutionResult> ExecuteAsync(Figure actor, GridPosition from, GridPosition to, BoardGrid grid);
    }
}

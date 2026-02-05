using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Turn.BonusMove
{
    /// <summary>
    /// Interactive session for bonus move phase.
    /// Handles the full cycle: input → validation → visual → completion.
    /// 
    /// Separates the "interactive waiting" logic from TurnController.
    /// TurnController just calls RunAsync and awaits completion.
    /// </summary>
    public interface IBonusMoveSession
    {
        /// <summary>
        /// Runs the bonus move interaction session.
        /// Waits for valid click, executes move, plays animation, returns.
        /// </summary>
        UniTask RunAsync(Figure actor, GridPosition from, int maxDistance, BoardGrid grid);
        
        /// <summary>
        /// Cancels the current session if active.
        /// </summary>
        void Cancel();
    }
}

using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Turn
{
    /// <summary>
    /// Controller for executing player turns.
    /// Handles move/attack execution, lock acquisition, and bonus move coordination.
    /// </summary>
    public interface ITurnController
    {
        /// <summary>
        /// Executes a move action for the given actor.
        /// Acquires interaction lock, executes turn, handles bonus moves, and ends turn.
        /// </summary>
        UniTask ExecuteMoveAsync(Figure actor, GridPosition from, GridPosition to);

        /// <summary>
        /// Executes an attack action for the given actor.
        /// Acquires interaction lock, executes turn, handles bonus moves, and ends turn.
        /// </summary>
        UniTask ExecuteAttackAsync(Figure actor, GridPosition from, GridPosition to);
    }
}

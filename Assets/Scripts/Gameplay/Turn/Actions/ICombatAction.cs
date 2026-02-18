using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Turn.Actions
{
    /// <summary>
    /// One logical thing a unit can do in a turn (move, attack, move+attack, etc.).
    /// Same instance used for: validation, UI targeting, AI, and execution — avoids desync.
    /// </summary>
    public interface ICombatAction
    {
        string Id { get; }

        /// <summary>
        /// Can this action be performed with the given context?
        /// Context.From = actor position, Context.To = chosen target (move dest or attack target).
        /// </summary>
        bool CanExecute(ActionContext context);

        /// <summary>
        /// All positions that are valid "To" for this action from the given actor position.
        /// Used by UI (highlight) and AI (candidate targets). Same logic as CanExecute.
        /// </summary>
        IReadOnlyCollection<ActionPreview> GetPreviews(Figure actor, GridPosition from, BoardGrid grid);

        /// <summary>
        /// Execute the action. Call only when CanExecute(context) is true.
        /// Sets context.ActionExecuted and updates context.From as needed.
        /// </summary>
        UniTask ExecuteAsync(ActionContext context);
    }
}

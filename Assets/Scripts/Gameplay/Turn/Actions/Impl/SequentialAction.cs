using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Turn.Actions.Impl
{
    /// <summary>
    /// Executes multiple actions sequentially.
    /// Used for patterns like "attack then move_to_killed".
    /// </summary>
    public sealed class SequentialAction : ICombatAction
    {
        public string Id { get; }

        private readonly IReadOnlyList<ICombatAction> _actions;

        public SequentialAction(string id, IEnumerable<ICombatAction> actions)
        {
            Id = id;
            _actions = actions.ToList();
        }

        public bool CanExecute(ActionContext context)
        {
            // Check if first action can execute (others will be checked during execution)
            if (_actions.Count == 0)
                return false;

            return _actions[0].CanExecute(context);
        }

        public IReadOnlyCollection<ActionPreview> GetPreviews(Figure actor, GridPosition from, BoardGrid grid)
        {
            return _actions.Count == 0 
                ? new HashSet<ActionPreview>() 
                : _actions[0].GetPreviews(actor, from, grid);
        }

        public async UniTask ExecuteAsync(ActionContext context)
        {
            bool anyExecuted = false;

            foreach (ICombatAction action in _actions)
            {
                // Check if action can execute (some actions like move_to_killed may not be valid)
                if (!action.CanExecute(context))
                    continue; // Skip if action can't execute (e.g. move_to_killed if no kill happened)

                // Save ActionExecuted state before action
                bool wasExecuted = context.ActionExecuted;
                context.ActionExecuted = false; // Reset for this action

                await action.ExecuteAsync(context);

                // If this action executed, mark overall execution
                if (context.ActionExecuted)
                {
                    anyExecuted = true;
                }
                else if (!wasExecuted && action == _actions[0])
                {
                    // First action failed, stop execution
                    break;
                }
            }

            // Mark as executed if at least one action executed
            context.ActionExecuted = anyExecuted;
        }
    }
}

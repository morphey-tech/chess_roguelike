using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Selection;
using VContainer;

namespace Project.Gameplay.Gameplay.Interaction
{
    /// <summary>
    /// Default implementation of click intent resolution.
    /// Contains all game rules for determining what action a click should perform.
    /// </summary>
    public sealed class ClickIntentResolver : IClickIntentResolver
    {
        private readonly ILogger<ClickIntentResolver> _logger;

        [Inject]
        public ClickIntentResolver(ILogService logService)
        {
            _logger = logService.CreateLogger<ClickIntentResolver>();
        }

        public ClickIntent Resolve(InteractionContext context)
        {
            if (context.Grid == null)
            {
                _logger.Warning("Cannot resolve intent: Grid is null");
                return ClickIntent.None;
            }

            if (!context.Grid.IsInside(context.ClickedPosition))
            {
                _logger.Debug($"Click outside grid bounds: ({context.ClickedPosition.Row},{context.ClickedPosition.Column})");
                return ClickIntent.None;
            }

            BoardCell clickedCell = context.Grid.GetBoardCell(context.ClickedPosition);

            if (!context.HasSelection)
            {
                return ResolveWithoutSelection(context, clickedCell);
            }

            return ResolveWithSelection(context, clickedCell);
        }

        private ClickIntent ResolveWithoutSelection(InteractionContext context, BoardCell clickedCell)
        {
            // No selection: can only select a friendly figure
           // if (IsFriendly(clickedCell, context.CurrentTeam))
            {
                return ClickIntent.Select(clickedCell.OccupiedBy, clickedCell.Position);
            }

            return ClickIntent.None;
        }

        private ClickIntent ResolveWithSelection(InteractionContext context, BoardCell clickedCell)
        {
            // Has selection: determine Move, Attack, or Re-select

            // Empty cell = Move
            if (clickedCell.IsFree)
            {
                return ClickIntent.Move(context.SelectedPosition.Value, context.ClickedPosition);
            }

            // Friendly figure = Re-select
            if (IsFriendly(clickedCell, context.CurrentTeam))
            {
                return ClickIntent.Select(clickedCell.OccupiedBy, clickedCell.Position);
            }

            // Enemy figure = Attack
            return ClickIntent.Attack(
                clickedCell.OccupiedBy,
                context.SelectedPosition.Value,
                context.ClickedPosition
            );
        }

        /// <summary>
        /// Checks if a cell contains a figure the current team can control.
        /// Abstracted to handle future cases: mind-control, neutral figures, etc.
        /// </summary>
        private bool IsFriendly(BoardCell cell, Team currentTeam)
        {
            return !cell.IsFree && cell.OccupiedBy?.Team == currentTeam;
        }
    }
}

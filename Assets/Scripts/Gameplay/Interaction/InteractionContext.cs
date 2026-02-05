using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Interaction
{
    /// <summary>
    /// Context data for resolving click intent.
    /// Immutable struct containing all information needed to determine what the player wants to do.
    /// </summary>
    public readonly struct InteractionContext
    {
        /// <summary>
        /// The game board grid.
        /// </summary>
        public readonly BoardGrid Grid;

        /// <summary>
        /// The position that was clicked.
        /// </summary>
        public readonly GridPosition ClickedPosition;

        /// <summary>
        /// The currently selected figure, or null if no selection.
        /// </summary>
        public readonly Figure SelectedFigure;

        /// <summary>
        /// The position of the selected figure, or null if no selection.
        /// </summary>
        public readonly GridPosition? SelectedPosition;

        /// <summary>
        /// The team that is currently active (whose turn it is).
        /// </summary>
        public readonly Team CurrentTeam;

        /// <summary>
        /// Whether there is currently a figure selected.
        /// </summary>
        public bool HasSelection => SelectedFigure != null;

        public InteractionContext(
            BoardGrid grid,
            GridPosition clickedPosition,
            Figure selectedFigure,
            GridPosition? selectedPosition,
            Team currentTeam)
        {
            Grid = grid;
            ClickedPosition = clickedPosition;
            SelectedFigure = selectedFigure;
            SelectedPosition = selectedPosition;
            CurrentTeam = currentTeam;
        }
    }
}

using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Selection;

namespace Project.Gameplay.Gameplay.Interaction
{
    /// <summary>
    /// Represents the resolved intent from a click action.
    /// Contains all information needed to execute the intended action.
    /// </summary>
    public readonly struct ClickIntent
    {
        /// <summary>
        /// The type of action the player intends to perform.
        /// </summary>
        public readonly CellClickIntent Type;

        /// <summary>
        /// The figure involved in the action (for SelectFigure or Attack).
        /// </summary>
        public readonly Figure? TargetFigure;

        /// <summary>
        /// The source position (for Move or Attack).
        /// </summary>
        public readonly GridPosition? From;

        /// <summary>
        /// The target position (for Move or Attack).
        /// </summary>
        public readonly GridPosition? To;

        private ClickIntent(CellClickIntent type, Figure? targetFigure, GridPosition? from, GridPosition? to)
        {
            Type = type;
            TargetFigure = targetFigure;
            From = from;
            To = to;
        }

        /// <summary>
        /// Creates an intent representing no valid action.
        /// </summary>
        public static ClickIntent None => new(CellClickIntent.None, null, null, null);

        /// <summary>
        /// Creates an intent to select a figure.
        /// </summary>
        public static ClickIntent Select(Figure figure, GridPosition position) =>
            new(CellClickIntent.SelectFigure, figure, null, position);

        /// <summary>
        /// Creates an intent to move from one position to another.
        /// </summary>
        public static ClickIntent Move(GridPosition from, GridPosition to) =>
            new(CellClickIntent.Move, null, from, to);

        /// <summary>
        /// Creates an intent to attack from one position to another.
        /// </summary>
        public static ClickIntent Attack(Figure target, GridPosition from, GridPosition to) =>
            new(CellClickIntent.Attack, target, from, to);
    }
}

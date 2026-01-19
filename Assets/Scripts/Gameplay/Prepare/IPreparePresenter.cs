using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Presenter for the prepare zone - spawns slots and figures for placement.
    /// </summary>
    public interface IPreparePresenter
    {
        /// <summary>
        /// Creates a slot at the given index and spawns a figure on it.
        /// </summary>
        /// <param name="index">Index of the slot (0-based)</param>
        /// <param name="totalCount">Total number of figures to place (for centering)</param>
        /// <param name="figureId">Unique figure instance ID</param>
        /// <param name="figureTypeId">Figure type (e.g. "pawn")</param>
        UniTask CreateSlotWithFigureAsync(int index, int totalCount, string figureId, string figureTypeId);

        /// <summary>
        /// Removes figure from slot (when placed on board).
        /// </summary>
        void RemoveFigure(string figureId);

        /// <summary>
        /// Highlights the selected figure.
        /// </summary>
        void SetSelected(string figureId, bool selected);

        /// <summary>
        /// Clears all slots and figures.
        /// </summary>
        void Clear();
    }
}

using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Presenter for the prepare zone - spawns slots and figures for placement.
    /// Handles all visual timing internally.
    /// </summary>
    public interface IPreparePresenter
    {
        /// <summary>
        /// Spawns the entire prepare zone with animated slots and figures.
        /// Presenter handles all timing/animation internally.
        /// </summary>
        UniTask SpawnPrepareZoneAsync(IReadOnlyList<PrepareZoneFigureData> figures);

        /// <summary>
        /// Removes figure from slot (when placed on board).
        /// </summary>
        void RemoveFigure(string figureId);

        /// <summary>
        /// Restores figure back to prepare zone (spawn failed).
        /// </summary>
        UniTask RestoreFigureAsync(string figureId);

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

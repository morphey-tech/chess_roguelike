using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Project.Gameplay.Gameplay.Prepare
{
    /// <summary>
    /// Data for spawning a figure in prepare zone.
    /// </summary>
    public readonly struct PrepareZoneFigureData
    {
        public readonly string FigureId;
        public readonly string FigureTypeId;

        public PrepareZoneFigureData(string figureId, string figureTypeId)
        {
            FigureId = figureId;
            FigureTypeId = figureTypeId;
        }
    }

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
        /// Highlights the selected figure.
        /// </summary>
        void SetSelected(string figureId, bool selected);

        /// <summary>
        /// Clears all slots and figures.
        /// </summary>
        void Clear();
    }
}

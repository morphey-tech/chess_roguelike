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
}
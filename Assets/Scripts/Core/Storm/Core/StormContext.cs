namespace Project.Core.Core.Storm.Core
{
    /// <summary>
    /// Контекст для расчёта зоны
    /// </summary>
    public struct StormContext
    {
        public int BoardSize;
        public int CurrentLayer;
        public int StepInLayer;
        public int ShrinkInterval;
        public int SafeZoneMinSize;

        public StormContext(
            int boardSize,
            int currentLayer,
            int stepInLayer,
            int shrinkInterval,
            int safeZoneMinSize)
        {
            BoardSize = boardSize;
            CurrentLayer = currentLayer;
            StepInLayer = stepInLayer;
            ShrinkInterval = shrinkInterval;
            SafeZoneMinSize = safeZoneMinSize;
        }
    }
}
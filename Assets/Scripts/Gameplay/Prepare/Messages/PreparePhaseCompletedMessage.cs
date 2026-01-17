namespace Project.Gameplay.Gameplay.Prepare.Messages
{
    /// <summary>
    /// Published when the prepare phase is completed and gameplay begins.
    /// </summary>
    public readonly struct PreparePhaseCompletedMessage
    {
        public int PlacedFiguresCount { get; }

        public PreparePhaseCompletedMessage(int placedFiguresCount)
        {
            PlacedFiguresCount = placedFiguresCount;
        }
    }
}

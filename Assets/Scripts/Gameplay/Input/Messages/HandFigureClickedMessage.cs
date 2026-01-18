namespace Project.Gameplay.Gameplay.Input.Messages
{
    /// <summary>
    /// Published when player clicks on a hand figure during prepare phase.
    /// </summary>
    public readonly struct HandFigureClickedMessage
    {
        public string FigureId { get; }

        public HandFigureClickedMessage(string figureId)
        {
            FigureId = figureId;
        }
    }
}

namespace Project.Gameplay.Gameplay.Input.Messages
{
    public readonly struct FigureHoverChangedMessage
    {
        public int? FigureId { get; }
        public bool HasFigure => FigureId.HasValue;

        public FigureHoverChangedMessage(int? figureId)
        {
            FigureId = figureId;
        }
    }
}

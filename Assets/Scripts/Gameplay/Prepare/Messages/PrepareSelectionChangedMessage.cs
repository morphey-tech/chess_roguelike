namespace Project.Gameplay.Gameplay.Prepare.Messages
{
    public readonly struct PrepareSelectionChangedMessage
    {
        public string FigureId { get; }
        public bool IsSelected { get; }

        public PrepareSelectionChangedMessage(string figureId, bool isSelected)
        {
            FigureId = figureId;
            IsSelected = isSelected;
        }
    }
}

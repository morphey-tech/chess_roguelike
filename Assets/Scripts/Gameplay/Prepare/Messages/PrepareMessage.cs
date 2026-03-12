namespace Project.Gameplay.Gameplay.Prepare.Messages
{
    /// <summary>
    /// Сообщения Prepare фазы (выбор фигуры, завершение, сброс визуала).
    /// </summary>
    public readonly struct PrepareMessage
    {
        public const string SELECTION_CHANGED = "prepareSelectionChanged";
        public const string PHASE_COMPLETED = "preparePhaseCompleted";
        public const string VISUAL_RESET = "prepareVisualReset";
        public const string COMPLETE_REQUESTED = "prepareCompleteRequested";

        public readonly string Type;
        public readonly string FigureId;
        public readonly bool IsSelected;
        public readonly int PlacedFiguresCount;

        private PrepareMessage(string type, string figureId, bool isSelected, int placedFiguresCount)
        {
            Type = type;
            FigureId = figureId;
            IsSelected = isSelected;
            PlacedFiguresCount = placedFiguresCount;
        }

        public static PrepareMessage SelectionChanged(string figureId, bool isSelected)
        {
            return new PrepareMessage(SELECTION_CHANGED, figureId, isSelected, 0);
        }

        public static PrepareMessage PhaseCompleted(int placedFiguresCount)
        {
            return new PrepareMessage(PHASE_COMPLETED, string.Empty, false, placedFiguresCount);
        }

        public static PrepareMessage VisualReset()
        {
            return new PrepareMessage(VISUAL_RESET, string.Empty, false, 0);
        }

        public static PrepareMessage CompleteRequested()
        {
            return new PrepareMessage(COMPLETE_REQUESTED, string.Empty, false, 0);
        }
    }
}

namespace Project.Gameplay.Gameplay.Stage
{
    public interface IStageHighlightRenderer
    {
        void Show(StageSelectionInfo info);
        void Clear();
    }
}

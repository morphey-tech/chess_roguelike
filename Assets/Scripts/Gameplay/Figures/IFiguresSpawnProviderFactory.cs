using Project.Gameplay.Gameplay.Stage;

namespace Project.Gameplay.Gameplay.Figures
{
    public interface IFiguresSpawnProviderFactory
    {
        IFiguresSpawnProvider Create(StageType stageType);
    }
}

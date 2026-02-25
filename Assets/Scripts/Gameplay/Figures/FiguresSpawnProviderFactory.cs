using Project.Gameplay.Gameplay.Stage;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class FiguresSpawnProviderFactory : IFiguresSpawnProviderFactory
    {
        private readonly DuelFiguresSpawnProvider _duelProvider;
        private readonly EmptyFiguresSpawnProvider _emptyProvider;

        [Inject]
        private FiguresSpawnProviderFactory(
            DuelFiguresSpawnProvider duelProvider,
            EmptyFiguresSpawnProvider emptyProvider)
        {
            _duelProvider = duelProvider;
            _emptyProvider = emptyProvider;
        }

        public IFiguresSpawnProvider Create(StageType stageType)
        {
            return stageType switch
            {
                StageType.Duel => _duelProvider,
                _ => _emptyProvider
            };
        }
    }
}

using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Suites;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Save.Service;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    //Пока врагов спавнит потом хз наверное переделаю
    public sealed class FigureSpawnPhase : IStagePhase
    {
        private readonly FigureSpawnService _figureSpawnService;
        private readonly ConfigProvider _configProvider;
        private readonly PlayerLoadoutService _loadoutService;
        private readonly ILogger<FigureSpawnPhase> _logger;

        public FigureSpawnPhase(
            FigureSpawnService figureSpawnService,
            ConfigProvider configProvider,
            PlayerLoadoutService loadoutService,
            ILogService logService)
        {
            _figureSpawnService = figureSpawnService;
            _configProvider = configProvider;
            _loadoutService = loadoutService;
            _logger = logService.CreateLogger<FigureSpawnPhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _logger.Info("Spawning initial figures");

            SuiteConfig suiteConfig = await LoadSuiteConfigAsync();
            BoardGrid grid = context.Grid;
            
            for (int index = 0; index < suiteConfig.Figures.Length; index++)
            {
                string figureId = suiteConfig.Figures[index];
                GridPosition spawnPosition = new(grid.Height - 1 - index, grid.Width / 2);

                _logger.Debug($"Spawning {figureId} at ({spawnPosition.Row},{spawnPosition.Column})");
                await _figureSpawnService.SpawnAsync(grid, spawnPosition, figureId, Team.Enemy);
            }

            _logger.Info($"Spawned {suiteConfig.Figures.Length} enemy figures");
            return PhaseResult.Continue;
        }

        private async UniTask<SuiteConfig> LoadSuiteConfigAsync()
        {
            string suiteId = _loadoutService.Current.SuiteId;
            SuiteConfigRepository repository = await _configProvider.Get<SuiteConfigRepository>("suites_conf");
            SuiteConfig config = Array.Find(repository.Suites, s => s.Id == suiteId);
            return config ?? throw new NullReferenceException($"Suite config '{suiteId}' not found");
        }
    }
}

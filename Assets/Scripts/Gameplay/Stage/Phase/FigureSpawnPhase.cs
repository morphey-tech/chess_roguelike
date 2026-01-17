using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Suites;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    public sealed class FigureSpawnPhase : IStagePhase
    {
        private readonly FigureSpawnService _figureSpawnService;
        private readonly SuiteConfig _suiteConfig;
        private readonly ILogger<FigureSpawnPhase> _logger;

        public FigureSpawnPhase(
            FigureSpawnService figureSpawnService,
            SuiteConfig suiteConfig,
            ILogService logService)
        {
            _figureSpawnService = figureSpawnService;
            _suiteConfig = suiteConfig;
            _logger = logService.CreateLogger<FigureSpawnPhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _logger.Info("Spawning initial figures");

            BoardGrid grid = context.Grid;
            
            for (int index = 0; index < _suiteConfig.Figures.Length; index++)
            {
                string figureId = _suiteConfig.Figures[index];
                GridPosition spawnPosition = new(1 + index, grid.Width / 2);

                _logger.Debug($"Spawning {figureId} at ({spawnPosition.Row},{spawnPosition.Column})");
                await _figureSpawnService.SpawnAsync(grid, spawnPosition, figureId, Team.Player);
            }

            _logger.Info($"Spawned {_suiteConfig.Figures.Length} figures");
            return PhaseResult.Continue;
        }
    }
}
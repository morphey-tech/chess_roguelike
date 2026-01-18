using System;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;

namespace Project.Gameplay.Gameplay.Stage.Phase
{
    /// <summary>
    /// Spawns enemies from DuelStageConfig.
    /// </summary>
    public sealed class EnemySpawnPhase : IStagePhase
    {
        private readonly FigureSpawnService _figureSpawnService;
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<EnemySpawnPhase> _logger;

        public EnemySpawnPhase(
            FigureSpawnService figureSpawnService,
            ConfigProvider configProvider,
            ILogService logService)
        {
            _figureSpawnService = figureSpawnService;
            _configProvider = configProvider;
            _logger = logService.CreateLogger<EnemySpawnPhase>();
        }

        public async UniTask<PhaseResult> ExecuteAsync(StageContext context)
        {
            _logger.Info($"Spawning enemies for stage: {context.StageConfig.Id}");

            DuelStageConfig duelConfig = await LoadDuelConfigAsync(context.StageConfig.TypeConfigId);
            
            if (duelConfig?.Enemies == null || duelConfig.Enemies.Length == 0)
            {
                _logger.Warning($"No enemies configured for stage: {context.StageConfig.TypeConfigId}");
                return PhaseResult.Continue;
            }

            foreach (EnemySpawnData enemy in duelConfig.Enemies)
            {
                GridPosition pos = new(enemy.Row, enemy.Column);
                
                if (!context.Grid.IsInside(pos))
                {
                    _logger.Warning($"Enemy position ({pos.Row}, {pos.Column}) is outside grid, skipping");
                    continue;
                }

                _logger.Debug($"Spawning {enemy.TypeId} at ({pos.Row}, {pos.Column})");
                await _figureSpawnService.SpawnAsync(context.Grid, pos, enemy.TypeId, Team.Enemy);
            }

            _logger.Info($"Spawned {duelConfig.Enemies.Length} enemies");
            return PhaseResult.Continue;
        }

        private async UniTask<DuelStageConfig> LoadDuelConfigAsync(string stageId)
        {
            DuelStageConfigRepository repository = 
                await _configProvider.Get<DuelStageConfigRepository>("duels_conf");
            return Array.Find(repository.Configs, c => c.Id == stageId);
        }
    }
}

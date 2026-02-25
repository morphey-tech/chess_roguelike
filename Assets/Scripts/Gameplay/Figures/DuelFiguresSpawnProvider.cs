using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Stage;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class DuelFiguresSpawnProvider : IFiguresSpawnProvider
    {
        private readonly ConfigProvider _configProvider;
        private readonly SpawnPatternParser _patternParser;

        [Inject]
        private DuelFiguresSpawnProvider(ConfigProvider configProvider, SpawnPatternParser patternParser)
        {
            _configProvider = configProvider;
            _patternParser = patternParser;
        }

        public async UniTask<IReadOnlyList<FigureSpawnEntry>> BuildAsync(StageContext context)
        {
            DuelStageConfigRepository repo = await _configProvider.Get<DuelStageConfigRepository>("duels_conf");
            DuelStageConfig? config = repo.Get(context.StageConfig.TypeConfigId);

            if (config == null)
                return Array.Empty<FigureSpawnEntry>();

            List<EnemySpawnData> enemies;
            
            if (!string.IsNullOrEmpty(config.SpawnPatternId))
            {
                enemies = await _patternParser.ParsePatternAsync(config.SpawnPatternId);
            }
            else if (config.Enemies != null)
            {
                enemies = new List<EnemySpawnData>(config.Enemies);
            }
            else
            {
                return Array.Empty<FigureSpawnEntry>();
            }

            List<FigureSpawnEntry> result = new();
            foreach (EnemySpawnData e in enemies)
            {
                result.Add(new FigureSpawnEntry
                {
                    Id = e.TypeId,
                    Team = Team.Enemy,
                    Position = new GridPosition(e.Row, e.Column)
                });
            }

            return result;
        }
    }
}

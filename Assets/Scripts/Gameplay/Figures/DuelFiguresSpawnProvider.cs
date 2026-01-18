using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Grid;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Stage;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class DuelFiguresSpawnProvider : IFiguresSpawnProvider
    {
        private readonly ConfigProvider _configProvider;

        public DuelFiguresSpawnProvider(ConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public async UniTask<IReadOnlyList<FigureSpawnEntry>> BuildAsync(StageContext context)
        {
            DuelStageConfigRepository repo =
                await _configProvider.Get<DuelStageConfigRepository>("duels_conf");

            DuelStageConfig config =
                Array.Find(repo.Configs, c => c.Id == context.StageConfig.TypeConfigId);

            if (config == null)
            {
                return Array.Empty<FigureSpawnEntry>();
            }

            List<FigureSpawnEntry> result = new();
            foreach (EnemySpawnData e in config.Enemies)
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
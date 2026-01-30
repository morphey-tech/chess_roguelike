using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Configs;
using VContainer;

namespace Project.Gameplay.Gameplay.Figures
{
    public sealed class SpawnPatternParser
    {
        private readonly ConfigProvider _configProvider;
        private readonly ILogger<SpawnPatternParser> _logger;
        
        private SpawnPatternConfigRepository _patternCache;
        private FigureConfigRepository _figureCache;
        private Dictionary<string, FigureConfig> _aliasToFigure;

        [Inject]
        public SpawnPatternParser(ConfigProvider configProvider, ILogService logService)
        {
            _configProvider = configProvider;
            _logger = logService.CreateLogger<SpawnPatternParser>();
        }

        public async UniTask<List<EnemySpawnData>> ParsePatternAsync(string patternId)
        {
            await EnsureLoadedAsync();

            SpawnPatternConfig pattern = Array.Find(_patternCache.Patterns, p => p.Id == patternId);
            if (pattern == null)
            {
                _logger.Error($"Spawn pattern not found: {patternId}");
                return new List<EnemySpawnData>();
            }

            return ParsePattern(pattern);
        }

        private List<EnemySpawnData> ParsePattern(SpawnPatternConfig pattern)
        {
            var result = new List<EnemySpawnData>();

            for (int row = 0; row < pattern.Rows.Length; row++)
            {
                string rowStr = pattern.Rows[row];
                for (int col = 0; col < rowStr.Length; col++)
                {
                    char c = rowStr[col];
                    if (c == '.')
                        continue;

                    string alias = c.ToString();
                    if (!_aliasToFigure.TryGetValue(alias, out FigureConfig figure))
                    {
                        _logger.Warning($"Unknown alias '{alias}' at ({row}, {col})");
                        continue;
                    }

                    result.Add(new EnemySpawnData
                    {
                        TypeId = figure.Id,
                        Row = row,
                        Column = col
                    });
                }
            }

            _logger.Info($"Parsed pattern '{pattern.Id}': {result.Count} enemies");
            return result;
        }

        private async UniTask EnsureLoadedAsync()
        {
            if (_aliasToFigure != null)
                return;

            _patternCache = await _configProvider.Get<SpawnPatternConfigRepository>("spawn_patterns_conf");
            _figureCache = await _configProvider.Get<FigureConfigRepository>("figures_conf");

            _aliasToFigure = _figureCache.Figures
                .Where(f => !string.IsNullOrEmpty(f.Alias))
                .ToDictionary(f => f.Alias, f => f);

            _logger.Info($"Loaded {_aliasToFigure.Count} figure aliases");
        }
    }
}

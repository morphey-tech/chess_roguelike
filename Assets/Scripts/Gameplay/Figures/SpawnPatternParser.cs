using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Configs.Stage;
using Project.Core.Core.Infrastructure;
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

            SpawnPatternConfig pattern = _patternCache.Get(patternId);
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
            int totalRows = pattern.Rows.Length;

            for (int row = 0; row < totalRows; row++)
            {
                List<string> aliases = BracketNotationParser.ParseRow(pattern.Rows[row]);
                
                // Invert row index so that visual top in config = top of the board in game
                int gameRow = (totalRows - 1) - row;
                
                for (int col = 0; col < aliases.Count; col++)
                {
                    string alias = aliases[col];
                    if (alias == ".")
                        continue;

                    if (!_aliasToFigure.TryGetValue(alias, out FigureConfig figure))
                    {
                        _logger.Warning($"Unknown alias '{alias}' at ({row}, {col})");
                        continue;
                    }

                    result.Add(new EnemySpawnData
                    {
                        TypeId = figure.Id,
                        Row = gameRow,
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

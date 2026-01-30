using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Turn.Steps;

namespace Project.Gameplay.Gameplay.Turn
{
    public sealed class TurnPatternResolver
    {
        private readonly ILogger<TurnPatternResolver> _logger;

        public TurnPatternResolver(ILogService logService)
        {
            _logger = logService.CreateLogger<TurnPatternResolver>();
        }

        public ITurnStep Resolve(Figure actor, TurnPatternSet patternSet, ActionContext context)
        {
            if (patternSet == null || patternSet.Patterns.Count == 0)
            {
                _logger.Warning($"No patterns for {actor}, returning null");
                return null;
            }

            List<TurnPattern> validPatterns = patternSet.Patterns
                .Where(p => p.Evaluate(context))
                .OrderByDescending(p => p.Priority)
                .ToList();

            if (validPatterns.Count == 0)
            {
                _logger.Debug($"No valid patterns for {actor}");
                return null;
            }

            TurnPattern chosen = validPatterns.First();
            _logger.Debug($"{actor} chose pattern '{chosen.Id}' (priority {chosen.Priority})");

            return chosen.Step;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Configs.Figure;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Movement.Strategies;
using VContainer;

namespace Project.Gameplay.Gameplay.Movement
{
    /// <summary>
    /// Factory for movement strategies. Maps movement ID to strategy.
    /// Supports PatternMovement via MovementPatternConfig.
    /// </summary>
    public sealed class MovementStrategyFactory
    {
        private readonly Dictionary<string, IMovementStrategy> _strategies;
        private readonly IMovementStrategy _fallback;
        private readonly ILogger<MovementStrategyFactory> _logger;

        [Inject]
        private MovementStrategyFactory(
            IEnumerable<IMovementStrategy> strategies,
            ILogService logService)
        {
            _strategies = strategies.ToDictionary(s => s.Id);
            _fallback = new PawnMovement();
            _logger = logService.CreateLogger<MovementStrategyFactory>();

            _logger.Info($"Registered strategies: {string.Join(", ", _strategies.Keys)}");
        }

        public IMovementStrategy Get(string movementId)
        {
            if (string.IsNullOrEmpty(movementId))
            {
                _logger.Warning($"Empty movementId, using fallback pawn");
                return _fallback;
            }

            if (_strategies.TryGetValue(movementId, out var strategy))
            {
                return strategy;
            }

            _logger.Warning($"Unknown movementId '{movementId}', using fallback pawn");
            return _fallback;
        }

        public void RegisterPattern(string id, IMovementStrategy strategy)
        {
            if (_strategies.TryAdd(id, strategy))
            {
                _logger.Info($"Registered pattern movement: {id}");
            }
            else
            {
                _logger.Debug($"Pattern movement '{id}' already registered");
            }
        }

        public static IMovementStrategy CreatePattern(MovementPatternConfig pattern)
        {
            return new PatternMovement(pattern);
        }
    }
}

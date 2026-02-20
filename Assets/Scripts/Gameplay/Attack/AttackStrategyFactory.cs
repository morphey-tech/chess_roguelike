using System.Collections.Generic;
using System.Linq;
using Project.Core.Core.Logging;
using Project.Gameplay.Gameplay.Attack.Strategies;

namespace Project.Gameplay.Gameplay.Attack
{
    /// <summary>
    /// Factory for attack strategies. Maps attack ID to strategy.
    /// </summary>
    public sealed class AttackStrategyFactory
    {
        private readonly Dictionary<string, IAttackStrategy> _strategies;
        private readonly IAttackStrategy _fallback;
        private readonly ILogger<AttackStrategyFactory> _logger;
        private static AttackStrategyFactory _instance;

        public AttackStrategyFactory(
            IEnumerable<IAttackStrategy> strategies,
            ILogService logService)
        {
            _strategies = strategies.ToDictionary(s => s.Id);
            _fallback = new SimpleAttack();
            _logger = logService.CreateLogger<AttackStrategyFactory>();
            _instance = this;
            
            _logger.Info($"Registered attack strategies: {string.Join(", ", _strategies.Keys)}");
        }

        public static AttackStrategyFactory Instance => _instance;

        public IAttackStrategy Get(string attackId)
        {
            if (string.IsNullOrEmpty(attackId))
            {
                return _fallback;
            }
            if (_strategies.TryGetValue(attackId, out IAttackStrategy? strategy))
            {
                return strategy;
            }
            _logger.Warning($"Unknown attackId '{attackId}', using fallback simple");
            return _fallback;
        }
    }
}

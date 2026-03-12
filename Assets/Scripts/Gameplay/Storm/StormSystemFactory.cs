using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Configs.Storm;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Core;
using Project.Core.Core.Storm.Messages;
using Project.Gameplay.Gameplay.Configs;
using VContainer;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Реализация фабрики через ConfigProvider
    /// </summary>
    public class StormSystemFactory : IStormSystemFactory
    {
        private readonly ConfigProvider _configProvider;
        private readonly IObjectResolver _resolver;
        private readonly ILogService _logService;
        private readonly ILogger<StormSystemFactory> _logger;

        [Inject]
        private StormSystemFactory(
            ConfigProvider configProvider,
            IObjectResolver resolver,
            ILogService logService)
        {
            _configProvider = configProvider;
            _resolver = resolver;
            _logService = logService;
            _logger = logService.CreateLogger<StormSystemFactory>();
        }

        public async UniTask<StormSystem?> Create(string zoneConfigId)
        {
            StormConfigRepository repo = await _configProvider.Get<StormConfigRepository>("shrinking_zone_conf");
            StormConfig config = repo.Require(zoneConfigId);
            _logger.Info($"Config loaded: min_turn={config.MinTurn}, max_turn={config.MaxTurn}, shrink_interval={config.ShrinkInterval}, damage={config.ZoneDamageFlat}+{config.ZoneDamagePercent*100}%");

            IStormStrategy strategy = _resolver.Resolve<IStormStrategy>();
            IPublisher<string, StormMessage> stormPublisher = _resolver.Resolve<IPublisher<string, StormMessage>>();
            ISubscriber<string, StormMessage> stormSubscriber = _resolver.Resolve<ISubscriber<string, StormMessage>>();
            _logger.Info($"Creating ZoneShrinkSystem with strategy={strategy.GetType().Name}");

            StormSystem system = new(
                config,
                strategy,
                stormPublisher,
                stormSubscriber,
                _logService);

            return system;
        }
    }
}

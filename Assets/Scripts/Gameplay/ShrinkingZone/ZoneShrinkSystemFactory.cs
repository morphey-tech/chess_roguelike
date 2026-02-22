using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.ShrinkingZone.Config;
using Project.Core.Core.ShrinkingZone.Core;
using Project.Core.Core.ShrinkingZone.Messages;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.ShrinkingZone.Messages;
using UnityEngine;
using VContainer;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Фабрика для создания ZoneShrinkSystem под конкретный конфиг
    /// </summary>
    public interface IZoneShrinkSystemFactory
    {
        UniTask<ZoneShrinkSystem?> Create(string zoneConfigId);
    }

    /// <summary>
    /// Реализация фабрики через ConfigProvider
    /// </summary>
    public class ZoneShrinkSystemFactory : IZoneShrinkSystemFactory
    {
        private readonly ConfigProvider _configProvider;
        private readonly IObjectResolver _resolver;

        [Inject]
        public ZoneShrinkSystemFactory(
            ConfigProvider configProvider,
            IObjectResolver resolver)
        {
            _configProvider = configProvider;
            _resolver = resolver;
        }

        public async UniTask<ZoneShrinkSystem?> Create(string zoneConfigId)
        {
            ZoneShrinkConfigRepository repo = await _configProvider.Get<ZoneShrinkConfigRepository>("shrinking_zone_conf");
            ZoneShrinkConfig config = repo.Require(zoneConfigId);
            Debug.Log($"[ZONE FACTORY] Config loaded: min_turn={config.MinTurn}, max_turn={config.MaxTurn}, shrink_interval={config.ShrinkInterval}, damage={config.ZoneDamageFlat}+{config.ZoneDamagePercent*100}%");

            var strategy = _resolver.Resolve<IZoneShrinkStrategy>();
            var stateChangedPublisher = _resolver.Resolve<IPublisher<ZoneStateChangedMessage>>();
            var cellsUpdatedPublisher = _resolver.Resolve<IPublisher<ZoneCellsUpdatedMessage>>();
            var damagePublisher = _resolver.Resolve<IPublisher<UnitTakeZoneDamageMessage>>();
            var battleStartedSubscriber = _resolver.Resolve<ISubscriber<ZoneBattleStartedMessage>>();
            var turnStartedSubscriber = _resolver.Resolve<ISubscriber<ZoneTurnStartedMessage>>();
            var damageDealtSubscriber = _resolver.Resolve<ISubscriber<ZoneDamageDealtMessage>>();
            var figureTurnEndedSubscriber = _resolver.Resolve<ISubscriber<ZoneFigureTurnEndedMessage>>();

            Debug.Log($"[ZONE FACTORY] Creating ZoneShrinkSystem with strategy={strategy?.GetType().Name}");

            return new ZoneShrinkSystem(
                config,
                strategy,
                stateChangedPublisher,
                cellsUpdatedPublisher,
                damagePublisher,
                battleStartedSubscriber,
                turnStartedSubscriber,
                damageDealtSubscriber,
                figureTurnEndedSubscriber);
        }
    }
}

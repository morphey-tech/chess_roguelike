using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Logging;
using Project.Core.Core.ShrinkingZone.Config;
using Project.Core.Core.ShrinkingZone.Core;
using Project.Gameplay.Gameplay.Configs;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.ShrinkingZone.Messages;
using VContainer;
using VContainer.Unity;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Сервис управления shrinking zone на уровне стейджа
    /// </summary>
    public class ZoneBattleService : IDisposable
    {
        private readonly IZoneShrinkSystemFactory _zoneFactory;
        private readonly IPublisher<ZoneBattleStartedMessage> _battleStartedPublisher;
        private readonly IPublisher<ZoneTurnStartedMessage> _turnStartedPublisher;
        private readonly IPublisher<ZoneDamageDealtMessage> _damageDealtPublisher;
        private readonly IPublisher<ZoneFigureTurnEndedMessage> _figureTurnEndedPublisher;
        private readonly ILogger<ZoneBattleService> _logger;
        private readonly IDisposable _subscriptions;

        private ZoneShrinkSystem? _zoneSystem;

        [Inject]
        private ZoneBattleService(
            IZoneShrinkSystemFactory zoneFactory,
            IPublisher<ZoneBattleStartedMessage> battleStartedPublisher,
            IPublisher<ZoneTurnStartedMessage> turnStartedPublisher,
            IPublisher<ZoneDamageDealtMessage> damageDealtPublisher,
            IPublisher<ZoneFigureTurnEndedMessage> figureTurnEndedPublisher,
            ISubscriber<TurnChangedMessage> turnSubscriber,
            ISubscriber<FigureDeathMessage> figureDeathSubscriber,
            ILogService logService)
        {
            _zoneFactory = zoneFactory;
            _battleStartedPublisher = battleStartedPublisher;
            _turnStartedPublisher = turnStartedPublisher;
            _damageDealtPublisher = damageDealtPublisher;
            _figureTurnEndedPublisher = figureTurnEndedPublisher;
            _logger = logService.CreateLogger<ZoneBattleService>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            turnSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            figureDeathSubscriber.Subscribe(OnFigureDeath).AddTo(bag);
            _subscriptions = bag.Build();
        }

        /// <summary>
        /// Инициализировать зону для текущего стейджа
        /// </summary>
        public async UniTask InitializeForStage(string? zoneConfigId)
        {
            _logger.Info($"[ZONE] InitializeForStage called with configId='{zoneConfigId}'");

            // Очищаем старую систему
            (_zoneSystem as IDisposable)?.Dispose();

            if (string.IsNullOrEmpty(zoneConfigId))
            {
                _zoneSystem = null;
                _logger.Warning("[ZONE] Zone disabled for this stage (configId is null/empty)");
                return;
            }

            _zoneSystem = await _zoneFactory.Create(zoneConfigId);

            if (_zoneSystem != null)
            {
                _logger.Info($"[ZONE] Zone initialized successfully with config '{zoneConfigId}'");
                _battleStartedPublisher.Publish(new Messages.ZoneBattleStartedMessage());
            }
            else
            {
                _logger.Error($"[ZONE] Failed to create ZoneShrinkSystem for config '{zoneConfigId}'");
            }
        }

        /// <summary>
        /// Вызывается в начале каждого хода
        /// </summary>
        public void OnTurnStarted(int turn)
        {
            _logger.Debug($"[ZONE] OnTurnStarted(turn={turn}), system={(_zoneSystem != null ? "active" : "null")}");
            if (_zoneSystem != null)
            {
                _turnStartedPublisher.Publish(new Messages.ZoneTurnStartedMessage(turn));
            }
        }

        /// <summary>
        /// Вызывается при нанесении урона (для активации зоны)
        /// </summary>
        public void OnDamageDealt(int turn)
        {
            _logger.Debug($"[ZONE] OnDamageDealt(turn={turn}), system={(_zoneSystem != null ? "active" : "null")}");
            if (_zoneSystem != null)
            {
                _damageDealtPublisher.Publish(new Messages.ZoneDamageDealtMessage(turn));
            }
        }

        /// <summary>
        /// Вызывается при завершении хода figure
        /// </summary>
        public void OnFigureTurnEnded(Figure figure, int row, int col)
        {
            if (_zoneSystem == null)
                return;

            var status = _zoneSystem.GetCellStatus(row, col);
            _logger.Debug($"[ZONE] OnFigureTurnEnded(figure={figure.Id}, pos=({row},{col}), status={status})");

            var target = new FigureZoneDamageTarget(figure);
            _figureTurnEndedPublisher.Publish(new Messages.ZoneFigureTurnEndedMessage(target, row, col));
        }

        /// <summary>
        /// Получить статус клетки
        /// </summary>
        public CellStatus GetCellStatus(int row, int col)
        {
            return _zoneSystem?.GetCellStatus(row, col) ?? CellStatus.Safe;
        }

        /// <summary>
        /// Получить состояние зоны
        /// </summary>
        public ZoneState GetZoneState()
        {
            return _zoneSystem?.CurrentState ?? ZoneState.Inactive;
        }

        private void OnTurnChanged(TurnChangedMessage msg)
        {
            OnTurnStarted(msg.TurnNumber);
        }

        private void OnFigureDeath(FigureDeathMessage msg)
        {
            // Фиксируем урон для активации зоны
            if (_zoneSystem is { CurrentState: ZoneState.Inactive })
            {
                OnDamageDealt(_zoneSystem.ActivationTurn);
            }
        }

        void IDisposable.Dispose()
        {
            (_zoneSystem as IDisposable)?.Dispose();
            _subscriptions?.Dispose();
        }
    }
}

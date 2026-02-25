using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Core;
using Project.Core.Core.Storm.Messages;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Run;
using Project.Gameplay.Gameplay.Turn;
using Project.Gameplay.ShrinkingZone.Messages;
using VContainer;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Сервис управления shrinking zone на уровне стейджа
    /// </summary>
    public class StormBattleService : IDisposable
    {
        private readonly IStormSystemFactory _zoneFactory;
        private readonly IPublisher<StormBattleStartedMessage> _battleStartedPublisher;
        private readonly IPublisher<StormTurnStartedMessage> _turnStartedPublisher;
        private readonly IPublisher<StormDamageDealtMessage> _damageDealtPublisher;
        private readonly IPublisher<FigureTakeStormDamageMessage> _figureDamagePublisher;
        private readonly ILogger<StormBattleService> _logger;
        private readonly IDisposable _subscriptions;
        private readonly RunHolder _runHolder;

        private StormSystem? _zoneSystem;

        [Inject]
        private StormBattleService(
            IStormSystemFactory zoneFactory,
            IPublisher<StormBattleStartedMessage> battleStartedPublisher,
            IPublisher<StormTurnStartedMessage> turnStartedPublisher,
            IPublisher<StormDamageDealtMessage> damageDealtPublisher,
            IPublisher<FigureTakeStormDamageMessage> figureDamagePublisher,
            ISubscriber<TurnChangedMessage> turnSubscriber,
            ISubscriber<FigureDeathMessage> figureDeathSubscriber,
            ILogService logService,
            RunHolder runHolder)
        {
            _zoneFactory = zoneFactory;
            _battleStartedPublisher = battleStartedPublisher;
            _turnStartedPublisher = turnStartedPublisher;
            _damageDealtPublisher = damageDealtPublisher;
            _figureDamagePublisher = figureDamagePublisher;
            _logger = logService.CreateLogger<StormBattleService>();
            _runHolder = runHolder;

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            turnSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            figureDeathSubscriber.Subscribe(OnFigureDeath).AddTo(bag);
            _subscriptions = bag.Build();
            
            _logger.Debug("ZoneBattleService initialized and subscribed to TurnChangedMessage");
        }

        /// <summary>
        /// Инициализировать зону для текущего стейджа
        /// </summary>
        public async UniTask InitializeForStage(string? zoneConfigId)
        {
            _logger.Info($"InitializeForStage called with configId='{zoneConfigId}'");
            (_zoneSystem as IDisposable)?.Dispose();

            if (string.IsNullOrEmpty(zoneConfigId))
            {
                _zoneSystem = null;
                _logger.Warning("Zone disabled for this stage (configId is null/empty)");
                return;
            }

            _logger.Info("Creating ZoneShrinkSystem...");
            _zoneSystem = await _zoneFactory.Create(zoneConfigId);

            if (_zoneSystem != null)
            {
                _logger.Info($"Zone initialized successfully with config '{zoneConfigId}', state={_zoneSystem.CurrentState}");
                _battleStartedPublisher.Publish(new StormBattleStartedMessage());
            }
            else
            {
                _logger.Error($"Failed to create ZoneShrinkSystem for config '{zoneConfigId}'");
            }
        }

        /// <summary>
        /// Вызывается в начале каждого хода
        /// </summary>
        public void OnTurnStarted(int turn)
        {
            _logger.Debug($"OnTurnStarted(turn={turn}), system={(_zoneSystem != null ? "active" : "null")}");
            if (_zoneSystem != null)
            {
                _logger.Debug($"Publishing ZoneTurnStartedMessage for turn {turn}");
                _turnStartedPublisher.Publish(new StormTurnStartedMessage(turn));
            }
            else
            {
                _logger.Warning("ZoneSystem is null, cannot publish ZoneTurnStartedMessage");
            }
        }

        public void OnDamageDealt(int turn)
        {
            _logger.Debug($"OnDamageDealt(turn={turn}), system={(_zoneSystem != null ? "active" : "null")}");
            if (_zoneSystem != null)
            {
                _damageDealtPublisher.Publish(new StormDamageDealtMessage(turn));
            }
        }

        private void OnTurnChanged(TurnChangedMessage msg)
        {
            _logger.Debug($"OnTurnChanged: team={msg.CurrentTeam}, turn={msg.TurnNumber}, zoneSystem={(_zoneSystem != null ? "active" : "null")}");
            OnTurnStarted(msg.TurnNumber);
            ApplyDamageToFiguresInZone(msg.CurrentTeam);
        }

        private void ApplyDamageToFiguresInZone(Team team)
        {
            _logger.Debug($"ApplyDamageToFiguresInZone called for team={team}");

            if (_zoneSystem == null)
            {
                _logger.Warning("ZoneSystem is null, skipping damage");
                return;
            }
            _logger.Debug($"ZoneSystem exists, state={_zoneSystem.CurrentState}, layer={_zoneSystem.CurrentLayer}, step={_zoneSystem.StepInLayer}, dangerCells={_zoneSystem.GetDangerCellsCount()}");

            if (_zoneSystem.CurrentState != StormState.Active && _zoneSystem.CurrentState != StormState.MinSizeReached)
            {
                _logger.Warning($"Zone state is {_zoneSystem.CurrentState}, skipping damage (need Active or MinSizeReached)");
                return;
            }

            BoardGrid? grid = _runHolder.Current?.CurrentStage?.Grid;
            if (grid == null)
            {
                _logger.Warning("Grid is null, cannot apply zone damage");
                return;
            }
            _logger.Debug($"Grid found: {grid.Width}x{grid.Height}");

            int figuresInDanger = 0;
            int figuresTotal = 0;
            int figuresOtherTeam = 0;
            foreach (BoardCell? cell in grid.AllCells())
            {
                Figure? figure = cell.OccupiedBy;
                if (figure == null)
                {
                    continue;
                }

                StormCellStatus status = _zoneSystem.GetCellStatus(cell.Position.Row, cell.Position.Column);
                _logger.Debug($"Figure {figure.Id} (team={figure.Team}) at ({cell.Position.Row},{cell.Position.Column}) has status={status}");

                if (figure.Team != team)
                {
                    figuresOtherTeam++;
                    continue;
                }

                figuresTotal++;

                if (IsDangerCell(cell.Position.Row, cell.Position.Column))
                {
                    figuresInDanger++;
                    int damage = CalculateDamage(figure.Stats.MaxHp);
                    _logger.Debug($"Applying {damage} damage to {figure.Id} at ({cell.Position.Row},{cell.Position.Column}) at start of turn");
                    _figureDamagePublisher.Publish(new FigureTakeStormDamageMessage(
                        new FigureStormDamageTarget(figure), damage, cell.Position));
                    _logger.Debug($"Published FigureTakeZoneDamageMessage for {figure.Id}");
                }
            }

            _logger.Debug($"Figures: total={figuresTotal}, otherTeam={figuresOtherTeam}, inDanger={figuresInDanger}");
        }

        private bool IsDangerCell(int row, int col)
        {
            return _zoneSystem?.GetCellStatus(row, col) == StormCellStatus.Danger;
        }

        /// <summary>
        /// Получить статус клетки
        /// </summary>
        public StormCellStatus GetCellStatus(int row, int col)
        {
            return _zoneSystem?.GetCellStatus(row, col) ?? StormCellStatus.Safe;
        }

        /// <summary>
        /// Получить состояние зоны
        /// </summary>
        public StormState GetZoneState()
        {
            return _zoneSystem?.CurrentState ?? StormState.Inactive;
        }

        /// <summary>
        /// Нанести урон фигуре за нахождение в зоне (при перемещении)
        /// </summary>
        public void ApplyZoneDamage(Figure figure, GridPosition position)
        {
            if (_zoneSystem == null)
            {
                return;
            }
            if (_zoneSystem.CurrentState != StormState.Active && _zoneSystem.CurrentState != StormState.MinSizeReached)
            {
                return;
            }
            if (!IsDangerCell(position.Row, position.Column))
            {
                return;
            }

            int damage = CalculateDamage(figure.Stats.MaxHp);
            _logger.Debug($"ApplyZoneDamage: {figure.Id} takes {damage} damage at ({position.Row},{position.Column})");
            _figureDamagePublisher.Publish(new FigureTakeStormDamageMessage(
                new FigureStormDamageTarget(figure), damage, position));
        }

        private int CalculateDamage(int unitMaxHP)
        {
            return _zoneSystem?.CalculateDamage(unitMaxHP) ?? 0;
        }

        private void OnFigureDeath(FigureDeathMessage msg)
        {
            if (_zoneSystem is { CurrentState: StormState.Inactive })
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

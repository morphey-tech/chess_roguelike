using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.ShrinkingZone.Core;
using Project.Core.Core.ShrinkingZone.Messages;
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
    public class ZoneBattleService : IDisposable
    {
        private readonly IZoneShrinkSystemFactory _zoneFactory;
        private readonly IPublisher<ZoneBattleStartedMessage> _battleStartedPublisher;
        private readonly IPublisher<ZoneTurnStartedMessage> _turnStartedPublisher;
        private readonly IPublisher<ZoneDamageDealtMessage> _damageDealtPublisher;
        private readonly IPublisher<FigureTakeZoneDamageMessage> _figureDamagePublisher;
        private readonly ILogger<ZoneBattleService> _logger;
        private readonly IDisposable _subscriptions;
        private readonly RunHolder _runHolder;

        private ZoneShrinkSystem? _zoneSystem;

        [Inject]
        private ZoneBattleService(
            IZoneShrinkSystemFactory zoneFactory,
            IPublisher<ZoneBattleStartedMessage> battleStartedPublisher,
            IPublisher<ZoneTurnStartedMessage> turnStartedPublisher,
            IPublisher<ZoneDamageDealtMessage> damageDealtPublisher,
            IPublisher<FigureTakeZoneDamageMessage> figureDamagePublisher,
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
            _logger = logService.CreateLogger<ZoneBattleService>();
            _runHolder = runHolder;

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            turnSubscriber.Subscribe(OnTurnChanged).AddTo(bag);
            figureDeathSubscriber.Subscribe(OnFigureDeath).AddTo(bag);
            _subscriptions = bag.Build();
            
            _logger.Debug("[ZONE] ZoneBattleService initialized and subscribed to TurnChangedMessage");
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

            _logger.Info("[ZONE] Creating ZoneShrinkSystem...");
            _zoneSystem = await _zoneFactory.Create(zoneConfigId);

            if (_zoneSystem != null)
            {
                _logger.Info($"[ZONE] Zone initialized successfully with config '{zoneConfigId}', state={_zoneSystem.CurrentState}");
                _battleStartedPublisher.Publish(new ZoneBattleStartedMessage());
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
                _logger.Debug($"[ZONE] Publishing ZoneTurnStartedMessage for turn {turn}");
                _turnStartedPublisher.Publish(new ZoneTurnStartedMessage(turn));
            }
            else
            {
                _logger.Warning($"[ZONE] ZoneSystem is null, cannot publish ZoneTurnStartedMessage");
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
                _damageDealtPublisher.Publish(new ZoneDamageDealtMessage(turn));
            }
        }

        private void OnTurnChanged(TurnChangedMessage msg)
        {
            _logger.Debug($"[ZONE] OnTurnChanged: team={msg.CurrentTeam}, turn={msg.TurnNumber}, zoneSystem={(_zoneSystem != null ? "active" : "null")}");
            OnTurnStarted(msg.TurnNumber);
            ApplyDamageToFiguresInZone(msg.CurrentTeam);
        }

        /// <summary>
        /// Наносит урон всем фигурам текущей команды, находящимся в опасной зоне
        /// </summary>
        private void ApplyDamageToFiguresInZone(Team team)
        {
            _logger.Debug($"[ZONE] ApplyDamageToFiguresInZone called for team={team}");
            
            if (_zoneSystem == null)
            {
                _logger.Warning("[ZONE] ZoneSystem is null, skipping damage");
                return;
            }
            _logger.Debug($"[ZONE] ZoneSystem exists, state={_zoneSystem.CurrentState}, layer={_zoneSystem.CurrentLayer}, step={_zoneSystem.StepInLayer}, dangerCells={_zoneSystem.GetDangerCellsCount()}");
            
            // Зона наносит урон в состояниях Active и MinSizeReached
            if (_zoneSystem.CurrentState != ZoneState.Active && _zoneSystem.CurrentState != ZoneState.MinSizeReached)
            {
                _logger.Warning($"[ZONE] Zone state is {_zoneSystem.CurrentState}, skipping damage (need Active or MinSizeReached)");
                return;
            }

            BoardGrid? grid = _runHolder.Current?.CurrentStage?.Grid;
            if (grid == null)
            {
                _logger.Warning("[ZONE] Grid is null, cannot apply zone damage");
                return;
            }
            _logger.Debug($"[ZONE] Grid found: {grid.Width}x{grid.Height}");

            int figuresInDanger = 0;
            int figuresTotal = 0;
            int figuresOtherTeam = 0;
            foreach (var cell in grid.AllCells())
            {
                var figure = cell.OccupiedBy;
                if (figure == null)
                    continue;

                // Логируем все фигуры на доске
                var status = _zoneSystem.GetCellStatus(cell.Position.Row, cell.Position.Column);
                _logger.Debug($"[ZONE] Figure {figure.Id} (team={figure.Team}) at ({cell.Position.Row},{cell.Position.Column}) has status={status}");
                    
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
                    _logger.Debug($"[ZONE] Applying {damage} damage to {figure.Id} at ({cell.Position.Row},{cell.Position.Column}) at start of turn");
                    // Публикуем сообщение для ZoneDamageService
                    _figureDamagePublisher.Publish(new FigureTakeZoneDamageMessage(
                        new FigureZoneDamageTarget(figure), damage, cell.Position));
                    _logger.Debug($"[ZONE] Published FigureTakeZoneDamageMessage for {figure.Id}");
                }
            }
            
            _logger.Debug($"[ZONE] Figures: total={figuresTotal}, otherTeam={figuresOtherTeam}, inDanger={figuresInDanger}");
        }

        private bool IsDangerCell(int row, int col)
        {
            return _zoneSystem?.GetCellStatus(row, col) == CellStatus.Danger;
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

        /// <summary>
        /// Нанести урон фигуре за нахождение в зоне (при перемещении)
        /// </summary>
        public void ApplyZoneDamage(Figure figure, GridPosition position)
        {
            if (_zoneSystem == null)
                return;

            // Проверяем, что зона активна и клетка опасная
            if (_zoneSystem.CurrentState != ZoneState.Active && _zoneSystem.CurrentState != ZoneState.MinSizeReached)
                return;

            if (!IsDangerCell(position.Row, position.Column))
                return;

            int damage = CalculateDamage(figure.Stats.MaxHp);
            _logger.Debug($"[ZONE] ApplyZoneDamage: {figure.Id} takes {damage} damage at ({position.Row},{position.Column})");
            _figureDamagePublisher.Publish(new FigureTakeZoneDamageMessage(
                new FigureZoneDamageTarget(figure), damage, position));
        }

        private int CalculateDamage(int unitMaxHP)
        {
            return _zoneSystem?.CalculateDamage(unitMaxHP) ?? 0;
        }

        private void OnFigureDeath(FigureDeathMessage msg)
        {
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

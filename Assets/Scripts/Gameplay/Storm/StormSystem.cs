using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using Project.Core.Core.Configs.Storm;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Storm.Core;
using Project.Core.Core.Storm.Messages;
using Project.Gameplay.ShrinkingZone.Messages;

namespace Project.Gameplay.ShrinkingZone
{
    /// <summary>
    /// Основная система shrinking zone
    /// </summary>
    public class StormSystem : IStormQueryService, IDisposable
    {
        public StormState CurrentState => _state;
        public int CurrentLayer => _currentLayer;
        public int StepInLayer => _stepInLayer;
        public int ActivationTurn => _activationTurn;
        public int? FirstDamageTurn => _firstDamageTurn;
        public bool FirstDamageDealt => _firstDamageDealt;

        private readonly StormConfig _config;
        private readonly IStormStrategy _strategy;
        private readonly IPublisher<StormStateChangedMessage> _stateChangedPublisher;
        private readonly IPublisher<StormCellsUpdatedMessage> _cellsUpdatedPublisher;
        private readonly IPublisher<FigureTakeStormDamageMessage> _damagePublisher;
        private readonly IDisposable _subscriptions;
        private readonly ILogger<StormSystem> _logger;

        private StormState _state = StormState.Inactive;
        private int _currentLayer;
        private int _stepInLayer;
        private int _activationTurn;
        private int? _firstDamageTurn;
        private bool _firstDamageDealt;

        private HashSet<GridPosition> _warningCells;
        private HashSet<GridPosition> _dangerCells;

        public StormSystem(
            StormConfig config,
            IStormStrategy strategy,
            IPublisher<StormStateChangedMessage> stateChangedPublisher,
            IPublisher<StormCellsUpdatedMessage> cellsUpdatedPublisher,
            IPublisher<FigureTakeStormDamageMessage> damagePublisher,
            ISubscriber<StormBattleStartedMessage> battleStartedSubscriber,
            ISubscriber<StormTurnStartedMessage> turnStartedSubscriber,
            ISubscriber<StormDamageDealtMessage> damageDealtSubscriber,
            ISubscriber<StormFigureTurnEndedMessage> figureTurnEndedSubscriber,
            ILogService logService)
        {
            _config = config;
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _stateChangedPublisher = stateChangedPublisher;
            _cellsUpdatedPublisher = cellsUpdatedPublisher;
            _damagePublisher = damagePublisher;
            _logger = logService.CreateLogger<StormSystem>();

            _warningCells = new HashSet<GridPosition>();
            _dangerCells = new HashSet<GridPosition>();

            DisposableBagBuilder bag = DisposableBag.CreateBuilder();
            battleStartedSubscriber.Subscribe(_ => OnBattleStarted()).AddTo(bag);
            turnStartedSubscriber.Subscribe(msg => OnTurnStarted(msg.Turn)).AddTo(bag);
            damageDealtSubscriber.Subscribe(msg => OnDamageDealt(msg.Turn)).AddTo(bag);
            figureTurnEndedSubscriber.Subscribe(OnFigureTurnEnded).AddTo(bag);
            _subscriptions = bag.Build();

            _logger.Info($"Created with config: min_turn={_config.MinTurn}, max_turn={_config.MaxTurn}");
        }

        private void OnBattleStarted()
        {
            _state = StormState.Inactive;
            _currentLayer = 0;
            _stepInLayer = 0;
            _activationTurn = CalculateActivationTurn();
            _firstDamageTurn = null;
            _firstDamageDealt = false;

            _warningCells.Clear();
            _dangerCells.Clear();

            _logger.Info($"Battle started, activation_turn={_activationTurn}");
            PublishStateChange(_state);
        }

        private void OnTurnStarted(int turn)
        {
            _logger.Debug($"Turn {turn} started, state={_state}, activation_turn={_activationTurn}, firstDamageTurn={_firstDamageTurn?.ToString() ?? "null"}, layer={_currentLayer}, step={_stepInLayer}");

            if (_state == StormState.Inactive && turn >= _activationTurn)
            {
                _state = StormState.Active;
                _firstDamageTurn = turn;
                _stepInLayer = 1;
                UpdateStormCells();
                _logger.Info($"Zone ACTIVATED at turn {turn}, layer={_currentLayer}, step={_stepInLayer}, dangerCells={_dangerCells.Count}, warningCells={_warningCells.Count}");
                PublishStateChange(_state);
            }

            if (_state == StormState.Active && _firstDamageTurn.HasValue)
            {
                int turnsSinceActivation = turn - _firstDamageTurn.Value;
                _logger.Debug($"Checking shrink: turnsSinceActivation={turnsSinceActivation}, shrinkInterval={_config.ShrinkInterval}, shouldShrink={turnsSinceActivation > 0 && turnsSinceActivation % _config.ShrinkInterval == 0}");
                if (turnsSinceActivation > 0 && turnsSinceActivation % _config.ShrinkInterval == 0)
                {
                    _logger.Debug($"Shrinking zone: BEFORE layer={_currentLayer}, step={_stepInLayer}");
                    AdvanceStorm();
                    _logger.Debug($"Shrinking zone: AFTER layer={_currentLayer}, step={_stepInLayer}");
                }
            }
        }

        private void OnDamageDealt(int turn)
        {
            _logger.Debug($"Damage dealt at turn {turn}, firstDamageDealt={_firstDamageDealt}, state={_state}");

            if (!_firstDamageDealt && _state == StormState.Inactive)
            {
                _firstDamageDealt = true;
                int oldActivationTurn = _activationTurn;
                _activationTurn = CalculateActivationTurn();
                _logger.Info($"First damage recorded, activation_turn: {oldActivationTurn} -> {_activationTurn}");
            }
        }

        private void OnFigureTurnEnded(StormFigureTurnEndedMessage msg)
        {
            _logger.Debug($"Figure turn ended at ({msg.Row},{msg.Col})");
        }

        public StormCellStatus GetCellStatus(int row, int col)
        {
            GridPosition pos = new(row, col);
            if (_dangerCells.Contains(pos))
            {
                return StormCellStatus.Danger;
            }
            return _warningCells.Contains(pos) ? StormCellStatus.Warning : StormCellStatus.Safe;
        }

        public int GetDangerCellsCount() => _dangerCells.Count;
        public int GetWarningCellsCount() => _warningCells.Count;

        public StormState GetCurrentState() => _state;

        public int GetActivationTurn() => _activationTurn;

        private int CalculateActivationTurn()
        {
            if (_firstDamageTurn.HasValue)
            {
                return UnityEngine.Mathf.Max(_config.MinTurn,
                    UnityEngine.Mathf.Min(_config.MaxTurn, _firstDamageTurn.Value + 1));
            }
            return _config.MinTurn;
        }

        public int CalculateDamage(int unitMaxHP)
        {
            int flatDamage = _config.ZoneDamageFlat;
            int percentDamage = UnityEngine.Mathf.FloorToInt(unitMaxHP * _config.ZoneDamagePercent);
            return flatDamage + percentDamage;
        }

        private void UpdateStormCells()
        {
            StormContext context = new(
                _config.BoardSize,
                _currentLayer,
                _stepInLayer,
                _config.ShrinkInterval,
                _config.SafeZoneMinSize
            );

            _warningCells = _strategy.GetWarningCells(context).ToHashSet();
            _dangerCells = _strategy.GetDangerCells(context).ToHashSet();

            _logger.Info($"Cells updated: layer={_currentLayer}, step={_stepInLayer}, {_warningCells.Count} warning, {_dangerCells.Count} danger");

            string dangerCellsStr = string.Join(", ", _dangerCells.Select(c => $"({c.Row},{c.Column})"));
            _logger.Debug($"Danger cells: {dangerCellsStr}");

            string warningCellsStr = string.Join(", ", _warningCells.Select(c => $"({c.Row},{c.Column})"));
            _logger.Debug($"Warning cells: {warningCellsStr}");

            _cellsUpdatedPublisher.Publish(new StormCellsUpdatedMessage(
                _warningCells.ToArray(),
                _dangerCells.ToArray()
            ));
        }

        private void AdvanceStorm()
        {
            StormContext context = new(
                _config.BoardSize,
                _currentLayer,
                _stepInLayer,
                _config.ShrinkInterval,
                _config.SafeZoneMinSize
            );

            if (_strategy.HasNextStep(context))
            {
                _strategy.AdvanceStep(ref context);
                _currentLayer = context.CurrentLayer;
                _stepInLayer = context.StepInLayer;
                _logger.Info($"Advanced to layer={_currentLayer}, step={_stepInLayer}");
                UpdateStormCells();
            }
            else
            {
                _state = StormState.MinSizeReached;
                _logger.Info("Zone reached minimum size");
                PublishStateChange(_state);
            }
        }

        private bool IsDangerCell(int row, int col)
        {
            return _dangerCells.Contains(new GridPosition(row, col));
        }

        private void PublishStateChange(StormState state)
        {
            _stateChangedPublisher.Publish(new StormStateChangedMessage(state));
        }

        public StormSaveContext SaveState()
        {
            return new StormSaveContext
            {
                State = _state,
                CurrentLayer = _currentLayer,
                StepInLayer = _stepInLayer,
                ActivationTurn = _activationTurn,
                FirstDamageTurn = _firstDamageTurn,
                FirstDamageDealt = _firstDamageDealt
            };
        }

        public void LoadState(StormSaveContext state)
        {
            _state = state.State;
            _currentLayer = state.CurrentLayer;
            _stepInLayer = state.StepInLayer;
            _activationTurn = state.ActivationTurn;
            _firstDamageTurn = state.FirstDamageTurn;
            _firstDamageDealt = state.FirstDamageDealt;

            UpdateStormCells();
            PublishStateChange(_state);
        }

        void IDisposable.Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
